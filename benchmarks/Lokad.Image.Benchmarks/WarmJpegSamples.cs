using System.Runtime;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

// Two new public input boundaries; reuse the existing job and fixed anchors.
internal sealed class WarmJpegSamples
{
    private readonly WarmJpegCase selected;
    private readonly bool gray;
    private readonly bool compact;
    private readonly JpegSampleFormat format;
    private readonly JpegEncodeOptions options;
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private WarmJpegPreparation prepared = null!;
    private byte[] input = [], expected = [], reference = [];
    private string inputHash = "";
    private JsonElement sourceIdentity;
    private QualitySample managedQuality = null!, nativeQuality = null!;
    private NativeWarmSample[] before = [];
    private bool ready, smoke;
    private int cpu, count;

    public WarmJpegSamples(WarmJpegCase selected)
    {
        if (selected is not (WarmJpegCase.EncodeGray8 or WarmJpegCase.Encode420_8)) throw new ArgumentException("Raw samples require EncodeGray8 or Encode420_8.");
        this.selected = selected; gray = selected == WarmJpegCase.EncodeGray8;
        compact = Environment.GetEnvironmentVariable("LOKAD_IMAGE_JPEG_QUANTIZATION") switch
        {
            null => false,
            "compact-color-v1" when !gray => true,
            _ => throw new InvalidOperationException("CompactColor measurement requires the caller-RGB420 control.")
        };
        format = gray ? JpegSampleFormat.Grayscale : JpegSampleFormat.Rgb;
        options = new() { Quality = 69, Sampling = gray ? JpegSampling.Grayscale : JpegSampling.YCbCr420,
            QuantizationPolicy = compact ? JpegQuantizationPolicy.CompactColor : JpegQuantizationPolicy.Original };
    }

    public void Setup(int cpu, bool smoke, int count)
    {
        this.cpu = cpu; this.smoke = smoke; this.count = count; state = new JpegBenchmarkCase();
        try
        {
            var path = Path.Combine(state.Root, "artifacts/warm-reference", compact ? "jpeg-compact-color-match.json" : gray ? "jpeg-gray/preparation.json" : "jpeg420-adaptive-match.json");
            prepared = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllText(path), EncoderQualityCalibration.Json) ?? throw new InvalidOperationException("Missing prepared anchor.");
            var pixels = HorizontalFlipInput.Create(8);
            reference = gray ? File.ReadAllBytes(Path.Combine(state.Root, "artifacts/warm-reference/jpeg-gray/reference.rgba")) : JpegQualityCorpus.Reference(pixels, 8, 0x204060);
            input = gray ? File.ReadAllBytes(Path.Combine(state.Root, "artifacts/warm-reference/jpeg-gray/reference.gray")) : new byte[513 * 259 * 3];
            if (!gray) for (var i = 0; i < 513 * 259; i++) reference.AsSpan(i * 4, 3).CopyTo(input.AsSpan(i * 3, 3));
            inputHash = JpegBenchmarkCase.Hash(input);
            if (prepared.Project != "Lokad.Image" || prepared.SourceDepth != 8 || !prepared.Match.Matched || prepared.Match.ClosestOwned.Setting != 69
                || compact && prepared.QualityPolicy != "compact-color-v1: original quality69 plus scaled16-basis precision floor; fixed FFmpegqscale5/RGB420; no quality search"
                || prepared.NativeQuality != (gray ? 69 : 5) || JpegBenchmarkCase.Hash(pixels) != prepared.RasterSha256
                || JpegBenchmarkCase.Hash(reference) != prepared.ReferenceSha256 || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256
                || inputHash != (gray ? "6b74f1d2c127f3bcccba2b377f4474b9fc7464890853d05bf058bbb38337df8d" : "257210317c8bb87ec196f89d511de45f014c66e73049fbcf5f44f399184d9668"))
                throw new InvalidOperationException("Raw JPEG input/quality/reference anchor differs.");
            expected = File.ReadAllBytes(Path.Combine(state.Root, prepared.ManagedFile));
            var frozenNative = Path.Combine(state.Root, prepared.NativeFile);
            if (JpegBenchmarkCase.Hash(expected) != prepared.Match.ClosestOwned.EncodedSha256 || JpegBenchmarkCase.HashFile(frozenNative) != prepared.Match.Native.EncodedSha256)
                throw new InvalidOperationException("Frozen JPEG packets differ.");
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            File.WriteAllBytes(state.Input, input);
            native = new NativeWarmReference(state, cpu, format, frozenNative);
            Validate(); ValidateQuality(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { Dispose(); throw; }
    }

    public int Apply()
    {
        using var output = new MemoryStream(); ImageCodec.EncodeJpeg(input, 513, 259, format, output, options: options);
        return checked((int)output.Length) ^ output.GetBuffer()[checked((int)output.Length) / 2];
    }

    private void Validate()
    {
        using var first = new MemoryStream(); using var second = new MemoryStream();
        ImageCodec.EncodeJpeg(input, 513, 259, format, first, options: options); ImageCodec.EncodeJpeg(input, 513, 259, format, second, options: options);
        if (ReferenceEquals(first.GetBuffer(), second.GetBuffer()) || !first.ToArray().AsSpan().SequenceEqual(expected)
            || !second.ToArray().AsSpan().SequenceEqual(expected) || JpegBenchmarkCase.Hash(input) != inputHash)
            throw new InvalidOperationException("Raw JPEG source/packets/independent owners differ.");
    }

    private void ValidateQuality()
    {
        var profile = new EncoderQualityProfile(513, 8, gray ? QualityEncoder.Jpeg444 : QualityEncoder.Jpeg420, QualityPattern.Asymmetric);
        managedQuality = EncoderQualityCalibration.Validate(expected, reference, profile, 69, state.Ffmpeg, options.Sampling);
        nativeQuality = EncoderQualityCalibration.Validate(File.ReadAllBytes(state.NativeOutput), reference, profile, prepared.NativeQuality, state.Ffmpeg, options.Sampling);
        if (managedQuality != prepared.Match.ClosestOwned || nativeQuality != prepared.Match.Native || !EncoderQualityCalibration.Match(nativeQuality, [managedQuality]).Matched)
            throw new InvalidOperationException("Raw JPEG source-quality/native packet differs from qualified anchor.");
    }

    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); ValidateQuality(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", "WarmJpeg-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = "WarmJpeg", caseId = selected.ToString(), publicBoundary = compact ? "JpegSamplesEncode/CompactColorRgb420Resident" : gray ? "JpegSamplesEncode/GrayscaleResident" : "JpegSamplesEncode/Rgb420Resident",
                quantizationPolicy = options.QuantizationPolicy.ToString(),
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), smoke, nativeSampleCount = smoke ? 1 : count, width = 513, height = 259, depth = 8, cpu,
                referenceLabel = gray ? "SIMD-enabled libjpeg-turbo 3.1.4.1; single-component grayscale benchmark exception" : "FFmpeg n8.1 MJPEG",
                managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(), sourceIdentity,
                inputSha256 = inputHash, referenceSha256 = prepared.ReferenceSha256, rawInputChannels = (int)format,
                managedOutputBytes = expected.Length, nativeOutputBytes = nativeQuality.EncodedBytes, managedQuality, nativeQuality,
                qualityMatched = true, alphaExact = true, decoderAgreementWithinBounds = true, sourceUnchanged = true, independentOwnedOutput = true,
                psnrGapDb = EncoderQualityCalibration.Match(nativeQuality, [managedQuality]).PsnrGapDb, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpegSamples).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after, nativeExitCode = 0,
                boundary = "Identical resident packed 8-bit caller gray/RGB samples to complete fresh independent JPEG bytes. Public EncodeJpeg admission, bounded block/row normalization, color conversion when RGB, transform, quantization, adaptive Huffman/entropy coding, MemoryStream creation/growth and owned output release are timed. Native borrows resident packed samples, reuses codec and private conversion scratch only; every operation processes/encodes all samples and acquires/releases a fresh owned packet. No RGBA staging or alpha flattening in either operation. Input projection, startup, disk I/O, IPC, hashing and quality checks excluded. One CPU; frozen matched-quality packets exact, no quality-number equivalence assumption."
            }, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine("Validated raw-sample JPEG pair: " + path);
        }
        finally { Dispose(); }
    }

    private void Dispose() { native?.Dispose(); state?.Dispose(); ready = false; }
}
