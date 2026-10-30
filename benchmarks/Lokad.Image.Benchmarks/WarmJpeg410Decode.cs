using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal sealed class WarmJpeg410Decode
{
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] input = [], expected = [], nativePixels = [];
    private NativeWarmSample[] before = [];
    private JsonElement prepared, sourceIdentity, validation;
    private int cpu, count, maximum, scalarMaximum;
    private double rmse, scalarRmse;
    private bool ready, smoke;

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count; state = new JpegBenchmarkCase();
        try
        {
            var folder = Path.Combine(state.Root, "artifacts/warm-reference/jpeg-410");
            using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "preparation.json")))) prepared = document.RootElement.Clone();
            using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "native-validation.json")))) validation = document.RootElement.Clone();
            input = File.ReadAllBytes(Path.Combine(folder, "input.jpg")); expected = File.ReadAllBytes(Path.Combine(folder, "expected.rgba"));
            if (prepared.GetProperty("project").GetString() != "Lokad.Image" || prepared.GetProperty("schemaVersion").GetInt32() != 1
                || prepared.GetProperty("rasterSha256").GetString() != JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(8))
                || prepared.GetProperty("encodedSha256").GetString() != "07b96d752c1e776d3302680254402d063b7eb8c111fda77c014399791db76a3e"
                || JpegBenchmarkCase.Hash(input) != prepared.GetProperty("encodedSha256").GetString() || input.Length != prepared.GetProperty("encodedBytes").GetInt32()
                || JpegBenchmarkCase.Hash(expected) != prepared.GetProperty("rgbaSha256").GetString()
                || JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "artifacts/warm-reference/jpeg-411/projection.ycbcr")) != prepared.GetProperty("projectionSha256").GetString()
                || JpegBenchmarkCase.HashFile(Path.Combine(folder, "blocks.ycbcr")) != prepared.GetProperty("blocksSha256").GetString()
                || !validation.GetProperty("nativeHorizontalPreExpansionVerified").GetBoolean()
                || !validation.GetProperty("passed").GetBoolean() || !validation.GetProperty("nativePlanesExact").GetBoolean() || !validation.GetProperty("actualCodedGridNormalization").GetBoolean()
                || validation.GetProperty("inputSha256").GetString() != JpegBenchmarkCase.Hash(input) || validation.GetProperty("ffmpegSha256").GetString() != JpegBenchmarkCase.HashFile(state.Ffmpeg))
                throw new InvalidOperationException("Original410 packet, analytical reference or actual-grid native validation differs.");
            using (var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = document.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = document.RootElement.GetProperty("sourceRevision").GetString() });
            File.WriteAllBytes(state.Input, input); Validate();
            native = new NativeWarmReference(state, cpu, "jpeg-410-decode", expected.Length); nativePixels = File.ReadAllBytes(state.NativeOutput);
            (maximum, rmse) = Agreement(nativePixels);
            foreach (var output in validation.GetProperty("outputs").EnumerateArray())
            {
                var pixels = File.ReadAllBytes(Path.Combine(folder, output.GetProperty("file").GetString()!));
                if (JpegBenchmarkCase.Hash(pixels) != output.GetProperty("sha256").GetString()) throw new InvalidOperationException("Frozen410 CLI samples differ.");
                if (output.GetProperty("scalar").GetBoolean()) (scalarMaximum, scalarRmse) = Agreement(pixels);
                else if (!pixels.AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Native410 worker and actual-grid CLI differ.");
            }
            before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    private (int Maximum, double Rmse) Agreement(byte[] actual)
    {
        if (!actual.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("JPEG 410 native pixels differ from the independent fixed-point reference.");
        return (0, 0);
    }

    private void Validate()
    {
        using var first = ImageCodec.Decode(input); using var survivor = ImageCodec.Decode(input);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (513, 259, 8, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Exact analytical410 RGBA pixels differ.");
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        if (!OriginalPixels.Bytes(survivor).AsSpan().SequenceEqual(expected) || JpegBenchmarkCase.Hash(input) != prepared.GetProperty("encodedSha256").GetString()
            || !File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(input)) throw new InvalidOperationException("JPEG410 source changed or outputs alias.");
    }

    public int Apply() { using var output = ImageCodec.Decode(input); return output.Stride * output.Height ^ output.GetRow8(0)[513 * 2]; }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); native.Stop();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Repeated410 native samples differ.");
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", "WarmJpegDecode-" + Guid.NewGuid().ToString("N") + ".json"); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmJpegDecode", caseId = "Decode410", publicBoundary = "JpegDecode/410Resident",
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, cpu, width = 513, height = 259, depth = 8, sourceIdentity,
                preparation = prepared, nativeValidation = validation, inputSha256 = JpegBenchmarkCase.Hash(input), inputBytes = input.Length,
                managedOutputBytes = expected.Length, nativeOutputBytes = nativePixels.Length, managedOutputSha256 = JpegBenchmarkCase.Hash(expected), nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                decoderMaximum = maximum, decoderRmse = rmse, scalarDecoderMaximum = scalarMaximum, scalarDecoderRmse = scalarRmse,
                decoderAgreementWithinBounds = true, exactAnalyticalManagedPixels = true, nativeCliPixelsExact = true, scalarCliAgreementWithinBounds = true,
                nativePixelsExact = true, nativeExceptionUsed = false, alphaExact = true, sourceUnchanged = true, independentOwnedOutput = true, iccConversion = false, exifReorientation = false,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpeg410Decode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Same original resident authored DC/unit-quantization baseline H4V2 JPEG to fresh independently owned RGBA8. Complete component decode, actual coded-dimension chroma normalization, full-range BT.601 conversion, opaque alpha, output acquisition/disposal and decoded-plane release are timed on both sides. Native software MJPEG including its byte half-grid horizontal expansion feeds direct513x259 libswscale conversion with public bilinear/accurate/full-chroma/in_h_chr_pos0/in_v_chr_pos0 settings; no padded-grid remapping, recovered chroma raster or row compaction. Packet/input/source/settings unchanged. Codec/conversion/resident packet/empty wrapper reused, never decoded content. ICC/EXIF transforms disabled. Startup, preparation, I/O, IPC and checks excluded. One CPU/thread. Exact independent authored-block analytical pixels, exact normal/scalar native RGBA and independent output lifetimes."
            }, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine("Validated JPEG410 decode: " + path);
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }
}
