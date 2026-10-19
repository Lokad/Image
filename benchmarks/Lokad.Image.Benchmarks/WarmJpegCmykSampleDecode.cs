using System.Text.Json;
using Lokad.Image;

internal sealed class WarmJpegCmykSampleDecode
{
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private CmykSamplePreparation prepared = null!;
    private byte[] input = [], expected = [], nativePixels = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private int cpu, count;
    private bool ready, smoke;

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count; state = new JpegBenchmarkCase();
        try
        {
            var directory = WarmJpegCmykSampleInput.DirectoryPath(state.Root);
            prepared = JsonSerializer.Deserialize<CmykSamplePreparation>(File.ReadAllText(Path.Combine(directory, "benchmark-preparation.json")), EncoderQualityCalibration.Json) ?? throw new InvalidOperationException("Run --prepare-warm-jpeg-cmyk-caller first.");
            input = File.ReadAllBytes(Path.Combine(directory, "input.jpg")); expected = File.ReadAllBytes(Path.Combine(directory, "expected.cmyk"));
            if (prepared.Project != "Lokad.Image" || prepared.SchemaVersion != 1 || input.Length != 10895 || expected.Length != 531468
                || input.Length != prepared.EncodedBytes || JpegBenchmarkCase.Hash(input) != prepared.EncodedSha256 || JpegBenchmarkCase.Hash(expected) != prepared.CmykSha256
                || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256)
                throw new InvalidOperationException("Original canonical CMYK packet/sample identities differ.");
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            File.WriteAllBytes(state.Input, input); Validate();
            native = new NativeWarmReference(state, cpu, JpegSampleFormat.Cmyk, expected.Length); nativePixels = File.ReadAllBytes(state.NativeOutput);
            if (!nativePixels.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Native canonical ink/black samples differ.");
            using (var validation = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "native-validation.json"))))
            {
                var v = validation.RootElement;
                if (!v.GetProperty("passed").GetBoolean() || v.GetProperty("inputSha256").GetString() != prepared.EncodedSha256 || v.GetProperty("ffmpegSha256").GetString() != prepared.FfmpegSha256)
                    throw new InvalidOperationException("Raw CMYK native CLI checks differ.");
                foreach (var output in v.GetProperty("outputs").EnumerateArray())
                {
                    var planar = File.ReadAllBytes(Path.Combine(directory, output.GetProperty("file").GetString()!));
                    if (JpegBenchmarkCase.Hash(planar) != output.GetProperty("sha256").GetString() || !WarmJpegCmykSampleInput.Pack(planar).AsSpan().SequenceEqual(nativePixels))
                        throw new InvalidOperationException("Native CMYK component packing differs from normal/scalar public CLI.");
                }
            }
            before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    public int Apply()
    {
        var output = new byte[531468]; ImageCodec.DecodeJpeg(input, output, JpegSampleFormat.Cmyk);
        return output.Length ^ output[output.Length / 2];
    }

    private void Validate()
    {
        var first = new byte[expected.Length]; var second = new byte[expected.Length];
        var info = ImageCodec.DecodeJpeg(input, first, JpegSampleFormat.Cmyk); ImageCodec.DecodeJpeg(input, second, JpegSampleFormat.Cmyk);
        if ((info.Width, info.Height, info.BitDepth, info.Orientation) != (513, 259, 8, ImageOrientation.Normal)
            || !first.AsSpan().SequenceEqual(expected) || !second.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Analytical caller CMYK samples/geometry differ.");
        first[0] ^= 255; Array.Clear(first); if (!second.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Caller CMYK outputs alias.");
        var padded = Enumerable.Repeat((byte)165, 2060 * 259 + 17).ToArray();
        ImageCodec.DecodeJpeg(input, padded, JpegSampleFormat.Cmyk, stride: 2060);
        for (var y = 0; y < 259; y++)
            if (!padded.AsSpan(y * 2060, 2052).SequenceEqual(expected.AsSpan(y * 2052, 2052)) || padded.AsSpan(y * 2060 + 2052, 8).ContainsAnyExcept((byte)165))
                throw new InvalidOperationException("Caller CMYK row/padding differs.");
        if (padded.AsSpan(2060 * 259).ContainsAnyExcept((byte)165) || JpegBenchmarkCase.Hash(input) != prepared.EncodedSha256
            || !File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(input)) throw new InvalidOperationException("CMYK suffix/source changed.");
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); native.Stop();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Repeated native CMYK samples differ.");
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", "WarmJpegDecode-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmJpegDecode", caseId = "CmykCaller", publicBoundary = "JpegSamplesDecode/CmykResident",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, width = 513, height = 259, depth = 8, sourceIdentity,
                preparation = prepared, inputSha256 = prepared.EncodedSha256, rasterSha256 = prepared.RasterSha256, inputBytes = input.Length,
                managedOutputBytes = expected.Length, nativeOutputBytes = nativePixels.Length, managedOutputSha256 = prepared.CmykSha256, nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                decoderMaximum = 0, decoderRmse = 0d, scalarDecoderMaximum = 0, scalarDecoderRmse = 0d,
                decoderAgreementWithinBounds = true, exactAnalyticalManagedPixels = true, nativeCliPixelsExact = true, scalarCliAgreementWithinBounds = true,
                blackIsNotAlpha = true, sourceUnchanged = true, independentOwnedOutput = true, callerPaddingUnchanged = true, noRgbaIntermediate = true, iccConversion = false, exifReorientation = false,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpegCmykSampleDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Same original resident untagged four-component full-resolution DC JPEG to fresh zeroed packed caller-owned canonical CMYK8. Includes complete decoding/admission, raw component materialization, C/M/Y/K packing and output lifetime on both sides. Native software mjpeg resets state, decodes four raw YUVA444P component containers without color/black/alpha conversion, packs caller CMYK and releases decoded planes/output. Context, padded resident packet and empty wrappers reused, never decoded content. No RGBA intermediate/header patch/ICC/EXIF transformation. Preparation, hashing, validation, I/O, IPC and startup excluded. One CPU/thread. Independent analytical managed pixels and normal/scalar native CLI samples exact across all four ink channels; source/padding and surviving independent outputs validated outside timing."
            }, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine("Validated caller CMYK JPEG decode: " + path);
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }
}
