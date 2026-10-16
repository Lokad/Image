using System.Runtime;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

// One material single-component case in the existing JPEG job. The reference
// exception belongs exclusively to benchmark tooling.
internal sealed class WarmGrayscaleJpeg
{
    internal const string Recipe = "original513x259-RGBA8; single-round alpha flatten on0x204060; BT.601 grayscale; quality69 owned mathematical quantizers; adaptive optimal Huffman; no restart; one component";
    private const string Label = "SIMD-enabled libjpeg-turbo 3.1.4.1, standalone single-component grayscale reference";
    private readonly JpegEncodeOptions options = new() { Quality = 69, Sampling = JpegSampling.Grayscale, BackgroundRgb = 0x204060 };
    private readonly EncoderQualityProfile profile = new(513, 8, QualityEncoder.Jpeg444, QualityPattern.Asymmetric);
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], reference = [], encoded = [];
    private WarmJpegPreparation prepared = null!;
    private QualitySample managedQuality = null!, nativeQuality = null!;
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private int cpu, sampleCount;
    private bool smoke, ready;

    public void Setup(int cpu, bool smoke, int sampleCount)
    {
        this.cpu = cpu; this.smoke = smoke; this.sampleCount = sampleCount;
        state = new JpegBenchmarkCase();
        try
        {
            var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "jpeg-gray");
            prepared = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllText(Path.Combine(directory, "preparation.json")), EncoderQualityCalibration.Json)
                ?? throw new InvalidOperationException("Missing prepared grayscale anchor.");
            if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.Recipe != Recipe
                || prepared.SourceDepth != 8 || prepared.Sampling != "gray" || prepared.NativeQuality != 69
                || prepared.BackgroundRgb != 0x204060 || prepared.OwnedSettings.Length != 1
                || !prepared.OwnedEvaluationOrder.AsSpan().SequenceEqual([69]) || !prepared.Match.Matched
                || prepared.Match.PsnrGapDb is not (>= 0 and <= EncoderQualityCalibration.MaximumPsnrGapDb))
                throw new InvalidOperationException("Grayscale geometry/quality policy differs.");
            pixels = HorizontalFlipInput.Create(8);
            reference = File.ReadAllBytes(Path.Combine(directory, "reference.rgba"));
            var gray = File.ReadAllBytes(Path.Combine(directory, "reference.gray"));
            if (JpegBenchmarkCase.Hash(pixels) != prepared.RasterSha256 || prepared.RasterSha256 != "70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80"
                || JpegBenchmarkCase.Hash(reference) != prepared.ReferenceSha256 || gray.Length != 513 * 259
                || reference.Length != pixels.Length || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256)
                throw new InvalidOperationException("Grayscale source/reference/oracle differs.");
            for (var i = 0; i < gray.Length; i++)
            {
                var a = pixels[4 * i + 3];
                var r = (pixels[4 * i] * a + 32 * (255 - a) + 127) / 255;
                var g = (pixels[4 * i + 1] * a + 64 * (255 - a) + 127) / 255;
                var b = (pixels[4 * i + 2] * a + 96 * (255 - a) + 127) / 255;
                var y = (byte)Math.Round((.299 * r + .587 * g) + .114 * b, MidpointRounding.AwayFromZero);
                if (gray[i] != y || reference[4 * i] != y || reference[4 * i + 1] != y || reference[4 * i + 2] != y || reference[4 * i + 3] != 255)
                    throw new InvalidOperationException("Grayscale normalization reference differs.");
            }
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp", "benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8); File.WriteAllBytes(state.Input, pixels);
            native = new NativeWarmReference(state, cpu, Path.Combine(directory, "reference.gray"));
            if (native.BuildIdentity.GetProperty("workerSha256").GetString() != prepared.NativeBuild.GetProperty("workerSha256").GetString())
                throw new InvalidOperationException("Prepared grayscale worker differs.");
            encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.ManagedFile));
            if (JpegBenchmarkCase.Hash(encoded) != "2442100fb89c3000f4646e8ae9c5e25b45980dd2d9e1be3d38f38e69f9a32cea"
                || JpegBenchmarkCase.HashFile(state.NativeOutput) != "444e39e6126dd3077d22c0b4f60e38d8a22611fa5721b226c5168a63b7e94423"
                || JpegBenchmarkCase.HashFile(Path.Combine(state.Root, prepared.NativeFile)) != prepared.Match.Native.EncodedSha256)
                throw new InvalidOperationException("Frozen grayscale packets differ.");
            ValidateManaged(); ValidateQuality(); before = native.Measure(smoke, sampleCount: sampleCount); ready = true;
        }
        catch { Dispose(); throw; }
    }

    public int Apply()
    {
        using var output = new MemoryStream(); ImageCodec.EncodeJpeg(source, output, options);
        return checked((int)output.Length) ^ output.GetBuffer()[checked((int)output.Length) / 2];
    }

    private void ValidateManaged()
    {
        using var first = new MemoryStream(); using var second = new MemoryStream();
        ImageCodec.EncodeJpeg(source, first, options); ImageCodec.EncodeJpeg(source, second, options);
        if (ReferenceEquals(first.GetBuffer(), second.GetBuffer()) || !first.ToArray().AsSpan().SequenceEqual(encoded)
            || !second.ToArray().AsSpan().SequenceEqual(encoded) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("Grayscale packet/source/independent ownership differs.");
    }

    private void ValidateQuality()
    {
        var packet = File.ReadAllBytes(state.NativeOutput);
        var managedHeader = JpegQualitySyntax.Read(encoded); var nativeHeader = JpegQualitySyntax.Read(packet);
        if (managedHeader.Components != 1 || nativeHeader.Components != 1 || managedHeader.QuantizationHash != nativeHeader.QuantizationHash)
            throw new InvalidOperationException("Grayscale components/quantization differ.");
        managedQuality = EncoderQualityCalibration.Validate(encoded, reference, profile, 69, state.Ffmpeg, JpegSampling.Grayscale);
        nativeQuality = EncoderQualityCalibration.Validate(packet, reference, profile, 69, state.Ffmpeg, JpegSampling.Grayscale);
        if (managedQuality != prepared.Match.ClosestOwned || nativeQuality != prepared.Match.Native
            || !EncoderQualityCalibration.Match(nativeQuality, [managedQuality]).Matched)
            throw new InvalidOperationException("Frozen grayscale decoder/source quality differs.");
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged(); var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); ValidateQuality(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmJpeg) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmJpeg), caseId = nameof(WarmJpegCase.EncodeGray8), smoke, nativeSampleCount = smoke ? 1 : sampleCount,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = 8, encodedDepth = 8, components = 1,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(), sourceIdentity,
                referenceLabel = Label, referenceExceptionScope = "Single-component grayscale JPEG benchmark tooling only; production remains managed and dependency-free",
                nativePreparedPreapprovalAnnotationRetained = true, inputSha256 = prepared.RasterSha256, rasterSha256 = prepared.RasterSha256, referenceSha256 = prepared.ReferenceSha256,
                sampling = "gray", backgroundRgb = 0x204060, recipe = Recipe, managedHuffman = "adaptive-v1", managedOutputBytes = encoded.Length, nativeOutputBytes = nativeQuality.EncodedBytes,
                managedEncodedSha256 = managedQuality.EncodedSha256, nativeOutputSha256 = nativeQuality.EncodedSha256, qualityMatched = true,
                psnrGapDb = EncoderQualityCalibration.Match(nativeQuality, [managedQuality]).PsnrGapDb, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb,
                alphaExact = true, decoderAgreementWithinBounds = true, normalizedReferenceExact = true, quantizationTablesExact = true,
                normalScalarNativePixelsExactFromUnchangedPreparedPackets = true, ownedEnumeratedSettings = 1, ownedEvaluationOrder = new[] { 69 }, managedQuality, nativeQuality,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpeg).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "Complete public original RGBA8 to one-component grayscale JPEG: single-round alpha flattening on0x204060, full-range BT.601 grayscale normalization, transform/quantization, adaptive histogram/Huffman/entropy coding and fresh owned output acquisition/release. Fresh managed MemoryStream creation/growth/disposal included. Standalone SIMD libjpeg-turbo reuses only codec/gray scratch; every source sample is normalized and encoded again, each packet newly acquired and freed. Startup/I/O/IPC/fixture creation/quality checks excluded; no output cache. Same owned mathematical quantizers, one frozen setting per encoder, existing0.25dB source-quality gate and max4/RMSE1.5 decoder agreement, exact alpha. One CPU, no concurrent codec/filter work. Production uses no native binding or dependency."
            }, new JsonSerializerOptions(EncoderQualityCalibration.Json) { PropertyNamingPolicy = null }) + "\n");
            Console.WriteLine("Validated warm grayscale JPEG pair: " + path);
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
}
