using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmJpegCase { Encode8 }

internal sealed record WarmJpegPreparation(int SchemaVersion, string Project, string GeneratedUtc, string Recipe,
    string RasterSha256, string ReferenceSha256, int NativeQuality, uint BackgroundRgb, string NativeFile, string ManagedFile,
    QualityMatch Match, QualitySample[] OwnedSettings, int[] OwnedEvaluationOrder, JsonElement SourceIdentity,
    string LibrarySha256, string BenchmarkAssemblySha256, string FfmpegSha256, JsonElement NativeBuild, JsonElement NativeReady);

[MemoryDiagnoser]
public class WarmJpeg
{
    public static IEnumerable<WarmJpegCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmJpegCase Case { get; set; }
    private const uint Background = 0x204060;
    private static readonly EncoderQualityProfile TargetProfile = new(513, 8, QualityEncoder.Jpeg444, QualityPattern.Asymmetric);
    private const string Recipe = "HorizontalFlipInput.Create(8): original 513x259 asymmetric RGBA8, phase zero, alpha zero at both first-row ends, hidden RGB unchanged. JPEG source alpha is flattened once onto RGB 0x204060; target is opaque 8-bit 4:4:4. No extra source raster.";
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], reference = [], encoded = [];
    private WarmJpegPreparation prepared = null!;
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private QualitySample managedQuality = null!, nativeQuality = null!;
    private bool smoke, ready;
    private int cpu, sampleCount;
    private JpegEncodeOptions options = null!;

    public static WarmJpegCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        if (selection is not (null or "Encode8")) throw new ArgumentException("The warm JPEG family has one Encode8 case.");
        return [WarmJpegCase.Encode8];
    }

    private static JsonElement SourceIdentity(string root)
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tmp", "benchmark-source.json")));
        return JsonSerializer.SerializeToElement(new
        {
            sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(),
            sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString()
        });
    }

    // Preparation is explicit and untimed. Benchmark launches validate the
    // selected bitstreams again without repeating quality enumeration.
    public static void Prepare(int cpu, int nativeQscale = 5)
    {
        if (!OperatingSystem.IsLinux() || cpu is < 0 or > 62 || nativeQscale is < 1 or > 31)
            throw new ArgumentException("JPEG preparation requires Linux, one allowed CPU and qscale 1..31.");
        using var process = Process.GetCurrentProcess(); var oldAffinity = process.ProcessorAffinity;
        process.ProcessorAffinity = new IntPtr(1L << cpu);
        try
        {
            using var state = new JpegBenchmarkCase();
            var pixels = HorizontalFlipInput.Create(8);
            // Reference is the specification's target, not another source image.
            var reference = JpegQualityCorpus.Reference(pixels, 8, Background);
            using var source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8);
            File.WriteAllBytes(state.Input, pixels);
            using var native = new NativeWarmReference(state, 8, cpu, "jpeg-encode", nativeQuality: nativeQscale);
            var nativeBytes = File.ReadAllBytes(state.NativeOutput);
            var nativeSample = EncoderQualityCalibration.Validate(nativeBytes, reference, TargetProfile, nativeQscale, state.Ffmpeg);
            var files = new Dictionary<int, byte[]>(); var order = new List<int>();
            var owned = EncoderQualityCalibration.SampleOwned([nativeSample], quality =>
            {
                using var output = new MemoryStream();
                ImageCodec.EncodeJpeg(source, output, new JpegEncodeOptions { Quality = quality, Sampling = JpegSampling.YCbCr444, BackgroundRgb = Background });
                var bytes = output.ToArray(); files.Add(quality, bytes); order.Add(quality);
                return EncoderQualityCalibration.Validate(bytes, reference, TargetProfile, quality, state.Ffmpeg);
            });
            var match = EncoderQualityCalibration.Match(nativeSample, owned);
            native.Check();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativeBytes)
                || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Quality preparation changed source/bitstream.");
            native.Stop();
            var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "jpeg-match-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var nativeFile = Path.Combine(directory, "native.jpg"); var managedFile = Path.Combine(directory, "managed.jpg");
            File.WriteAllBytes(nativeFile, nativeBytes); File.WriteAllBytes(managedFile, files[match.ClosestOwned.Setting]);
            var record = new WarmJpegPreparation(1, "Lokad.Image", DateTime.UtcNow.ToString("O"), Recipe,
                JpegBenchmarkCase.Hash(pixels), JpegBenchmarkCase.Hash(reference), nativeQscale, Background,
                Path.GetRelativePath(state.Root, nativeFile), Path.GetRelativePath(state.Root, managedFile), match, owned, order.ToArray(), SourceIdentity(state.Root),
                JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), JpegBenchmarkCase.HashFile(typeof(WarmJpeg).Assembly.Location),
                JpegBenchmarkCase.HashFile(state.Ffmpeg), native.BuildIdentity, native.Ready);
            var path = Path.Combine(state.Root, "artifacts", "warm-reference", "jpeg-match.json");
            File.WriteAllText(path, JsonSerializer.Serialize(record, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine($"Prepared one warm JPEG pair: native qscale {nativeQscale}, owned quality {match.ClosestOwned.Setting}, gap {match.PsnrGapDb:F4} dB, matched={match.Matched}; {owned.Length} owned settings, one source raster. {path}");
        }
        finally { process.ProcessorAffinity = oldAffinity; }
    }

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm native JPEG requires Linux.");
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1
            || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("JPEG worker requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            var path = Path.Combine(state.Root, "artifacts", "warm-reference", "jpeg-match.json");
            prepared = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllText(path), EncoderQualityCalibration.Json)
                ?? throw new InvalidOperationException("Missing warm JPEG preparation; run --prepare-warm-jpeg first.");
            if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.Recipe != Recipe || prepared.BackgroundRgb != Background
                || !prepared.Match.Matched || prepared.Match.PsnrGapDb is not (>= 0 and <= EncoderQualityCalibration.MaximumPsnrGapDb)
                || prepared.OwnedSettings.Length is < 5 or > EncoderQualityCalibration.MaximumOwnedSettings
                || prepared.OwnedEvaluationOrder.Length != prepared.OwnedSettings.Length
                || prepared.OwnedEvaluationOrder.Distinct().Count() != prepared.OwnedSettings.Length)
                throw new InvalidOperationException("Warm JPEG quality preparation is unmatched or outside its bounded contract.");
            sourceIdentity = SourceIdentity(state.Root);
            pixels = HorizontalFlipInput.Create(8); reference = JpegQualityCorpus.Reference(pixels, 8, Background);
            if (JpegBenchmarkCase.Hash(pixels) != prepared.RasterSha256 || JpegBenchmarkCase.Hash(reference) != prepared.ReferenceSha256
                || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256)
                throw new InvalidOperationException("Warm JPEG input/reference/oracle identity changed; prepare only this pair again.");
            options = new() { Quality = prepared.Match.ClosestOwned.Setting, Sampling = JpegSampling.YCbCr444, BackgroundRgb = Background };
            source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8); File.WriteAllBytes(state.Input, pixels);
            native = new NativeWarmReference(state, 8, cpu, "jpeg-encode", nativeQuality: prepared.NativeQuality);
            if (native.BuildIdentity.GetProperty("workerSha256").GetString() != prepared.NativeBuild.GetProperty("workerSha256").GetString())
                throw new InvalidOperationException("Native JPEG worker changed; prepare this pair again.");
            encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.ManagedFile));
            if (JpegBenchmarkCase.Hash(encoded) != prepared.Match.ClosestOwned.EncodedSha256
                || JpegBenchmarkCase.HashFile(state.NativeOutput) != prepared.Match.Native.EncodedSha256
                || JpegBenchmarkCase.HashFile(Path.Combine(state.Root, prepared.NativeFile)) != prepared.Match.Native.EncodedSha256)
                throw new InvalidOperationException("Prepared JPEG bitstreams changed.");
            ValidateManaged(); ValidateQuality();
            before = native.Measure(smoke, sampleCount: sampleCount); ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateManaged()
    {
        using var first = new MemoryStream(); using var second = new MemoryStream();
        ImageCodec.EncodeJpeg(source, first, options); ImageCodec.EncodeJpeg(source, second, options);
        if (ReferenceEquals(first.GetBuffer(), second.GetBuffer()) || !first.ToArray().AsSpan().SequenceEqual(encoded)
            || !second.ToArray().AsSpan().SequenceEqual(encoded) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("Warm JPEG output is not independent/deterministic or source changed.");
    }

    private void ValidateQuality()
    {
        managedQuality = EncoderQualityCalibration.Validate(encoded, reference, TargetProfile, options.Quality, state.Ffmpeg);
        nativeQuality = EncoderQualityCalibration.Validate(File.ReadAllBytes(state.NativeOutput), reference, TargetProfile, prepared.NativeQuality, state.Ffmpeg);
        if (managedQuality != prepared.Match.ClosestOwned || nativeQuality != prepared.Match.Native
            || !EncoderQualityCalibration.Match(nativeQuality, [managedQuality]).Matched)
            throw new InvalidOperationException("Warm JPEG source/decoder quality changed; prepare this pair again.");
    }

    [Benchmark]
    public int Managed()
    {
        using var output = new MemoryStream(); ImageCodec.EncodeJpeg(source, output, options);
        return checked((int)output.Length) ^ output.GetBuffer()[checked((int)output.Length) / 2];
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged();
            var after = native.Measure(smoke, before[0].Operations, sampleCount); native.Check(); ValidateQuality(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmJpeg) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmJpeg), caseId = Case.ToString(), smoke, nativeSampleCount = smoke ? 1 : sampleCount,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),
                width = source.Width, height = source.Height, depth = 8, cpu, managedAffinityCount = 1,
                processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(),
                runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                jitPolicy = "Tiered compilation disabled; fully optimized JIT before measurement", sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(pixels), rasterSha256 = JpegBenchmarkCase.Hash(pixels), referenceSha256 = JpegBenchmarkCase.Hash(reference),
                managedOutputBytes = encoded.Length, nativeOutputBytes = nativeQuality.EncodedBytes,
                managedEncodedSha256 = JpegBenchmarkCase.Hash(encoded), nativeOutputSha256 = JpegBenchmarkCase.HashFile(state.NativeOutput),
                recipe = Recipe, sampling = "4:4:4", backgroundRgb = Background, qualityMatched = true, alphaExact = true, decoderAgreementWithinBounds = true,
                psnrGapDb = prepared.Match.PsnrGapDb, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb,
                ownedEnumeratedSettings = prepared.OwnedSettings.Length, ownedEvaluationOrder = prepared.OwnedEvaluationOrder,
                ownedSettings = prepared.OwnedSettings, managedQuality, nativeQuality,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpeg).Assembly.Location), nativeBuild = native.BuildIdentity,
                nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "Resident original RGBA8 to complete independent JPEG bytes: explicit single-round alpha flattening on 0x204060, full-range BT.601 conversion, 4:4:4 encode, output acquisition/release. Native codec/conversion/scratch/frame/packet wrappers reused, but pixels are flattened/converted and encoded every operation. Fresh managed MemoryStream creation, growth, encode and disposal included; no redundant ToArray. Native optimal Huffman and managed fixed Huffman recorded. Quality matched by frozen <=0.25 dB RGB source-error band; fresh geometry/decoder/alpha/source/ownership/bitstream checks before/after. Startup, IPC, I/O, fixture generation, quality enumeration and checks excluded."
            }, new JsonSerializerOptions(EncoderQualityCalibration.Json) { PropertyNamingPolicy = null }) + "\n");
            Console.WriteLine("Validated warm JPEG pair: " + path);
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
