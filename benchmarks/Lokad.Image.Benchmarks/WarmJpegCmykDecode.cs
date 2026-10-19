using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmJpegCmykDecode
{
    private readonly bool nonzeroAc;
    private readonly bool general;
    private readonly bool twoDimensional;
    public WarmJpegCmykDecode(bool nonzeroAc = false, bool general = false, bool twoDimensional = false) { this.nonzeroAc = nonzeroAc || general || twoDimensional; this.general = general; this.twoDimensional = twoDimensional; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private CmykJpegPreparation prepared = null!;
    private byte[] input = [], expected = [], nativePixels = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private int cpu, count, maximum, scalarMaximum;
    private double rmse, scalarRmse;
    private bool ready, smoke;

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count; state = new JpegBenchmarkCase();
        try
        {
            var directory = WarmJpegCmykInput.DirectoryPath(state.Root, nonzeroAc, general, twoDimensional);
            prepared = JsonSerializer.Deserialize<CmykJpegPreparation>(File.ReadAllText(Path.Combine(directory, "preparation.json")), EncoderQualityCalibration.Json) ?? throw new InvalidOperationException("Run --prepare-warm-jpeg-cmyk first.");
            input = File.ReadAllBytes(Path.Combine(directory, "input.jpg")); expected = File.ReadAllBytes(Path.Combine(directory, "expected.rgba"));
            if (prepared.Project != "Lokad.Image" || prepared.SchemaVersion != 1 || prepared.RasterSha256 != JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(8))
                || input.Length != prepared.EncodedBytes || JpegBenchmarkCase.Hash(input) != prepared.EncodedSha256 || JpegBenchmarkCase.Hash(expected) != prepared.RgbaSha256
                || JpegQualitySyntax.Read(input).QuantizationHash != prepared.QuantizationSha256 || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256
                || JpegBenchmarkCase.HashFile(Path.Combine(directory, "projection.cmyk-inverted")) != prepared.ProjectionSha256
                || JpegBenchmarkCase.HashFile(Path.Combine(directory, "blocks.cmyk-inverted")) != prepared.BlockSamplesSha256
                || JpegBenchmarkCase.HashFile(Path.Combine(directory, "expected.cmyk")) != prepared.CanonicalCmykSha256)
                throw new InvalidOperationException("Original CMYK projection/packet/analytical samples differ.");
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            File.WriteAllBytes(state.Input, input); Validate();
            native = new NativeWarmReference(state, cpu, "jpeg-cmyk-decode", expected.Length); nativePixels = File.ReadAllBytes(state.NativeOutput);
            (maximum, rmse) = Agreement(nativePixels, expected);
            foreach (var scalar in new[] { false, true })
            {
                List<string> arguments = ["-hide_banner", "-loglevel", "error"];
                if (scalar) arguments.AddRange(["-cpuflags", "0"]);
                arguments.AddRange(["-threads:v", "1", "-thread_type:v", "0", "-noautorotate", "-f", "image2pipe", "-c:v", "mjpeg", "-i", "pipe:0", "-filter_threads", "1", "-vf", "format=rgba", "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"]);
                var oracle = OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input);
                if (scalar) (scalarMaximum, scalarRmse) = Agreement(oracle, expected);
                else if (!oracle.AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("CMYK native delivery differs from public CLI.");
            }
            before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private static (int Maximum, double Rmse) Agreement(byte[] actual, byte[] expected)
    {
        if (actual.Length != 513 * 259 * 4 || actual.Length != expected.Length) throw new InvalidOperationException("CMYK reference dimensions differ.");
        var maximum = 0; long squared = 0;
        for (var i = 0; i < actual.Length; i++)
        {
            if (i % 4 == 3) { if (actual[i] != 255 || expected[i] != 255) throw new InvalidOperationException("K was mistaken for alpha."); continue; }
            var error = Math.Abs(actual[i] - expected[i]); maximum = Math.Max(maximum, error); squared += error * error;
        }
        var rmse = Math.Sqrt((double)squared / (513 * 259 * 3));
        if (maximum > 4 || rmse > 1.5) throw new InvalidOperationException("CMYK JPEG decoder maximum4/RMSE1.5 exceeded.");
        return (maximum, rmse);
    }

    private void Validate()
    {
        using var first = ImageCodec.Decode(input); using var survivor = ImageCodec.Decode(input);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (513, 259, 8, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Analytical CMYK RGBA samples/geometry differ.");
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        if (!OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected) || JpegBenchmarkCase.Hash(input) != prepared.EncodedSha256
            || !File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(input)) throw new InvalidOperationException("CMYK outputs alias or source changed.");
    }

    public int Apply() { using var output = ImageCodec.Decode(input); return output.Stride * output.Height ^ output.GetRow8(0)[513 * 2]; }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); native.Stop();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Repeated CMYK reference samples differ.");
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", "WarmJpegDecode-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmJpegDecode", caseId = general ? "CmykAdobeGeneral" : twoDimensional ? "CmykAdobeSparse2D" : nonzeroAc ? "CmykAdobeAc" : "CmykAdobe", publicBoundary = general ? "JpegDecode/CmykAdobeGeneralResident" : twoDimensional ? "JpegDecode/CmykAdobeSparse2DResident" : nonzeroAc ? "JpegDecode/CmykAdobeAcResident" : "JpegDecode/CmykAdobeResident",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, width = 513, height = 259, depth = 8, sourceIdentity,
                preparation = prepared, inputSha256 = prepared.EncodedSha256, rasterSha256 = prepared.RasterSha256, inputBytes = input.Length,
                managedOutputBytes = expected.Length, nativeOutputBytes = nativePixels.Length, managedOutputSha256 = prepared.RgbaSha256, nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                decoderMaximum = maximum, decoderRmse = rmse, scalarDecoderMaximum = scalarMaximum, scalarDecoderRmse = scalarRmse,
                decoderAgreementWithinBounds = true, exactAnalyticalManagedPixels = !general, exactQualifiedGeneralManagedPixels = general, nativeCliPixelsExact = true, scalarCliAgreementWithinBounds = true,
                alphaExact = true, blackIsNotAlpha = true, sourceUnchanged = true, independentOwnedOutput = true, iccConversion = false, exifReorientation = false,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpegCmykDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Identical resident authored Adobe-transform0 full-resolution four-component baseline JPEG to fresh independently owned RGBA8. Public managed Decode includes complete decode, component materialization, inversion/device-CMYK conversion, zeroed output allocation and disposal. Native software mjpeg includes state reset, all four component decodes and K/device-color multiplication, opaque alpha, fresh zeroed RGBA allocation, GBRAP packing and decoded-plane/output release. Codec, conversion, padded resident packet and empty frame wrapper reused; no decoded content cache. ICC conversion and EXIF reorientation disabled. Startup, projection/packet preparation, I/O, IPC, hashing and checks excluded. One CPU, "
                    + (general ? "exact qualified general-reference managed pixels" : "exact analytical managed pixels") + "; normal native CLI exact and normal/scalar JPEG maximum4/RMSE1.5 with exact opaque alpha."
            }, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine("Validated CMYK JPEG decode: " + path);
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }
}
