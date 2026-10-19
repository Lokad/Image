using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmJpegYcckDecode
{
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private YcckJpegPreparation prepared = null!;
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
            var directory = WarmJpegYcckInput.DirectoryPath(state.Root);
            prepared = JsonSerializer.Deserialize<YcckJpegPreparation>(File.ReadAllText(Path.Combine(directory, "preparation.json")), EncoderQualityCalibration.Json) ?? throw new InvalidOperationException("Run --prepare-warm-jpeg-ycck420 first.");
            input = File.ReadAllBytes(Path.Combine(directory, "input.jpg")); expected = File.ReadAllBytes(Path.Combine(directory, "expected.rgba"));
            if (prepared.Project != "Lokad.Image" || prepared.SchemaVersion != 1 || prepared.RasterSha256 != JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(8))
                || prepared.EncodedSha256 != "211fcc243d40db6e6e3e8944a2e479825289201bff140e5cf2901e85ee97053d"
                || input.Length != prepared.EncodedBytes || JpegBenchmarkCase.Hash(input) != prepared.EncodedSha256 || JpegBenchmarkCase.Hash(expected) != prepared.RgbaSha256
                || JpegQualitySyntax.Read(input).QuantizationHash != prepared.QuantizationSha256 || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256
                || JpegBenchmarkCase.HashFile(Path.Combine(directory, "projection.ycck")) != prepared.ProjectionSha256
                || JpegBenchmarkCase.HashFile(Path.Combine(directory, "blocks.ycck")) != prepared.BlocksSha256)
                throw new InvalidOperationException("Original YCCK projection/packet/analytical samples differ.");
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            File.WriteAllBytes(state.Input, input); Validate();
            native = new NativeWarmReference(state, cpu, "jpeg-ycck-decode", expected.Length); nativePixels = File.ReadAllBytes(state.NativeOutput);
            (maximum, rmse) = Agreement(nativePixels, expected);
            using (var validation = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "native-validation.json"))))
            {
                var v = validation.RootElement;
                if (!v.GetProperty("passed").GetBoolean() || v.GetProperty("inputSha256").GetString() != prepared.EncodedSha256
                    || v.GetProperty("ffmpegSha256").GetString() != prepared.FfmpegSha256)
                    throw new InvalidOperationException("Frozen YCCK native CLI checks differ.");
                foreach (var output in v.GetProperty("outputs").EnumerateArray())
                {
                    var pixels = File.ReadAllBytes(Path.Combine(directory, output.GetProperty("file").GetString()!));
                    if (JpegBenchmarkCase.Hash(pixels) != output.GetProperty("sha256").GetString()) throw new InvalidOperationException("Frozen YCCK CLI bytes differ.");
                    if (output.GetProperty("scalar").GetBoolean()) (scalarMaximum, scalarRmse) = Agreement(pixels, expected);
                    else if (!pixels.AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("YCCK native full-range BT.601 delivery differs from public CLI.");
                }
            }
            before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private static (int Maximum, double Rmse) Agreement(byte[] actual, byte[] expected)
    {
        if (actual.Length != 513 * 259 * 4 || actual.Length != expected.Length) throw new InvalidOperationException("YCCK reference dimensions differ.");
        var maximum = 0; long squared = 0;
        for (var i = 0; i < actual.Length; i++)
        {
            if (i % 4 == 3) { if (actual[i] != 255 || expected[i] != 255) throw new InvalidOperationException("K was mistaken for alpha."); continue; }
            var error = Math.Abs(actual[i] - expected[i]); maximum = Math.Max(maximum, error); squared += error * error;
            // Owner-approved exception is limited to this authored YCCK420
            // fixture's chroma transitions. Every other pixel keeps maximum4.
            var pixel = i / 4; var x = pixel % 513; var y = pixel / 513;
            var transition = x % 16 is 0 or 15 || y % 16 is 0 or 15;
            if (error > (transition ? 16 : 4)) throw new InvalidOperationException("YCCK native transition/interior maximum16/4 exceeded.");
        }
        var rmse = Math.Sqrt((double)squared / (513 * 259 * 3));
        if (rmse > 1.5) throw new InvalidOperationException("YCCK native RGB RMSE1.5 exceeded.");
        return (maximum, rmse);
    }

    private void Validate()
    {
        using var first = ImageCodec.Decode(input); using var survivor = ImageCodec.Decode(input);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (513, 259, 8, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Analytical YCCK RGBA samples/geometry differ.");
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        if (!OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected) || JpegBenchmarkCase.Hash(input) != prepared.EncodedSha256
            || !File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(input)) throw new InvalidOperationException("YCCK outputs alias or source changed.");
    }

    public int Apply() { using var output = ImageCodec.Decode(input); return output.Stride * output.Height ^ output.GetRow8(0)[513 * 2]; }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); native.Stop();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Repeated YCCK reference samples differ.");
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", "WarmJpegDecode-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmJpegDecode", caseId = "Ycck420", publicBoundary = "JpegDecode/Ycck420Resident",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, width = 513, height = 259, depth = 8, sourceIdentity,
                preparation = prepared, inputSha256 = prepared.EncodedSha256, rasterSha256 = prepared.RasterSha256, inputBytes = input.Length,
                managedOutputBytes = expected.Length, nativeOutputBytes = nativePixels.Length, managedOutputSha256 = prepared.RgbaSha256, nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                decoderMaximum = maximum, decoderRmse = rmse, scalarDecoderMaximum = scalarMaximum, scalarDecoderRmse = scalarRmse,
                decoderAgreementWithinBounds = true, exactAnalyticalManagedPixels = true, nativeCliPixelsExact = true, scalarCliAgreementWithinBounds = true,
                approvedNativeOnlyGate = "RGB<=16 at authored chroma transitions (x/y mod16=0or15), <=4 elsewhere; RGB RMSE<=1.5; alpha exact255 and exact managed pixels unchanged.",
                alphaExact = true, blackIsNotAlpha = true, sourceUnchanged = true, independentOwnedOutput = true, iccConversion = false, exifReorientation = false,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpegYcckDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Identical resident authored Adobe-transform2 H2V2 full-K four-component baseline JPEG to fresh independently owned RGBA8. Public managed Decode includes complete decode, component materialization, inversion/device-YCCK conversion, zeroed output allocation and disposal. Native software mjpeg includes state reset, all four component decodes and K/YCbCr multiplication, opaque alpha, fresh zeroed RGBA allocation, explicit full-range BT.601 YUVA444P-to-RGBA conversion and decoded-plane/output release. Codec, conversion, padded resident packet and empty frame wrapper reused; no decoded content cache. ICC conversion and EXIF reorientation disabled. Startup, projection/packet preparation, I/O, IPC, hashing and checks excluded. One CPU, "
                    + "exact analytical managed pixels" + "; normal native CLI exact; approved native-only maximum16 at authored chroma transitions (x/y mod16=0or15), maximum4 elsewhere/RMSE1.5 with exact opaque alpha."
            }, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine("Validated YCCK JPEG decode: " + path);
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }
}
