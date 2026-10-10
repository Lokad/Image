using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmWebpCase { Encode8 }

internal sealed record WarmWebpPreparation(int SchemaVersion, string Project, string GeneratedUtc, string Recipe,
    string RasterSha256, string ReferenceSha256, int NativeQuality, string NativeFile, string ManagedFile,
    QualityMatch Match, QualitySample[] OwnedSettings, int[] OwnedEvaluationOrder, JsonElement SourceIdentity,
    string LibrarySha256, string BenchmarkAssemblySha256, string FfmpegSha256, JsonElement NativeBuild, JsonElement NativeReady);

[MemoryDiagnoser]
public class WarmWebp
{
    public static IEnumerable<WarmWebpCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmWebpCase Case { get; set; }
    private static readonly EncoderQualityProfile TargetProfile = new(513, 8, QualityEncoder.Vp8, QualityPattern.Asymmetric, QualityAlpha.Gradient);
    private const string Recipe = "HorizontalFlipInput.Create(8): original 513x259 asymmetric RGBA8, phase zero, alpha zero at both first-row ends, hidden RGB unchanged. VP8 target is 8-bit 4:2:0 with exact source alpha; effort 4 on both subjects. No extra source raster.";
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], reference = [], encoded = [];
    private WarmWebpPreparation prepared = null!;
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private QualitySample managedQuality = null!, nativeQuality = null!;
    private bool smoke, ready;
    private int cpu, sampleCount;
    private WebpEncodeOptions options = null!;

    public static WarmWebpCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        if (selection is not (null or "Encode8")) throw new ArgumentException("The warm VP8 family has one Encode8 case.");
        return [WarmWebpCase.Encode8];
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

    private static void ValidatePreparation(WarmWebpPreparation record)
    {
        if (record.SchemaVersion != 1 || record.Project != "Lokad.Image" || record.Recipe != Recipe
            || record.NativeQuality is < 1 or > 100 || record.NativeQuality != record.Match.Native.Setting
            || !record.Match.Matched || record.Match.PsnrGapDb is not (>= 0 and <= EncoderQualityCalibration.MaximumPsnrGapDb)
            || record.OwnedSettings.Length is < 5 or > EncoderQualityCalibration.MaximumOwnedSettings
            || record.OwnedEvaluationOrder.Length != record.OwnedSettings.Length
            || record.OwnedEvaluationOrder.Distinct().Count() != record.OwnedSettings.Length
            || !record.OwnedEvaluationOrder.Order().SequenceEqual(record.OwnedSettings.Select(sample => sample.Setting).Order())
            || !record.Match.Native.AlphaExact || !record.Match.ClosestOwned.AlphaExact
            || record.Match.Native.Codec != "VP8" || record.Match.ClosestOwned.Codec != "VP8"
            || record.Match.Native.Sampling != "4:2:0" || record.Match.ClosestOwned.Sampling != "4:2:0"
            || EncoderQualityCalibration.Match(record.Match.Native, record.OwnedSettings) != record.Match)
            throw new InvalidOperationException("Warm VP8 quality preparation is unmatched or outside its bounded contract.");
    }

    public static void ValidateControls()
    {
        // Synthetic metadata only: no source raster, encode, native worker or
        // timing. Exercise the actual pre-native preparation gate, including
        // forged matched flags/gaps and duplicate/out-of-budget settings.
        using var document = JsonDocument.Parse("{}"); var empty = document.RootElement.Clone();
        QualitySample Sample(int quality) => new(quality, 100, "", "", "", "VP8", "4:2:0", new(100000, 1, 3, 255), new(0, 0, 3));
        int[] seeds = [1, 25, 50, 75, 100]; var owned = seeds.Select(Sample).ToArray(); var native = Sample(75);
        var matched = EncoderQualityCalibration.Match(native, owned);
        var record = new WarmWebpPreparation(1, "Lokad.Image", "", Recipe, "", "", 75, "", "", matched,
            owned, seeds, empty, "", "", "", empty, empty);
        ValidatePreparation(record);
        WarmWebpPreparation[] rejected = [
            record with { Match = matched with { Matched = false, PsnrGapDb = 0.8984, ExclusionReason = "Synthetic outside-band control" } },
            record with { Match = matched with { PsnrGapDb = 0.8984 } },
            record with { Match = matched with { PsnrGapDb = 0.1 } },
            record with { NativeQuality = 101 },
            record with { NativeQuality = 50 },
            record with { Recipe = "Changed input recipe" },
            record with { OwnedSettings = [.. owned, Sample(2), Sample(3), Sample(4), Sample(5)], OwnedEvaluationOrder = [.. seeds, 2, 3, 4, 5] },
            record with { OwnedEvaluationOrder = [1, 25, 50, 75, 75] },
            record with { OwnedEvaluationOrder = [1, 25, 50, 75, 99] },
            record with { Match = matched with { Native = native with { AlphaExact = false } } },
            record with { Match = matched with { ClosestOwned = matched.ClosestOwned with { Codec = "JPEG" } } }
        ];
        foreach (var invalid in rejected)
        {
            try { ValidatePreparation(invalid); }
            catch (InvalidOperationException) { continue; }
            throw new InvalidOperationException("Warm VP8 gate accepted an invalid synthetic preparation.");
        }
        Console.WriteLine($"Warm VP8 preparation gate: valid metadata accepted; {rejected.Length} invalid controls rejected before any native worker. No raster/encode/timing.");
    }

    // Preparation is explicit and untimed. Benchmark launches validate the
    // selected bitstreams again without repeating quality enumeration.
    public static void Prepare(int cpu, int nativeQuality = 75)
    {
        if (!OperatingSystem.IsLinux() || cpu is < 0 or > 62 || nativeQuality is < 1 or > 100)
            throw new ArgumentException("VP8 preparation requires Linux, one allowed CPU and quality 1..100.");
        using var process = Process.GetCurrentProcess(); var oldAffinity = process.ProcessorAffinity;
        process.ProcessorAffinity = new IntPtr(1L << cpu);
        try
        {
            using var state = new JpegBenchmarkCase(".webp");
            var pixels = HorizontalFlipInput.Create(8);
            // The metric reference is this same original RGBA raster, with exact alpha.
            var reference = pixels;
            using var source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8);
            File.WriteAllBytes(state.Input, pixels);
            using var native = new NativeWarmReference(state, 8, cpu, "webp-encode", nativeQuality: nativeQuality);
            var nativeBytes = File.ReadAllBytes(state.NativeOutput);
            var nativeSample = EncoderQualityCalibration.Validate(nativeBytes, reference, TargetProfile, nativeQuality, state.Ffmpeg);
            var files = new Dictionary<int, byte[]>(); var order = new List<int>();
            var owned = EncoderQualityCalibration.SampleOwned([nativeSample], quality =>
            {
                using var output = new MemoryStream();
                ImageCodec.EncodeWebp(source, output, new WebpEncodeOptions { Lossless = false, Quality = quality, Effort = 4 });
                var bytes = output.ToArray(); files.Add(quality, bytes); order.Add(quality);
                return EncoderQualityCalibration.Validate(bytes, reference, TargetProfile, quality, state.Ffmpeg);
            });
            var match = EncoderQualityCalibration.Match(nativeSample, owned);
            native.Check();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativeBytes)
                || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Quality preparation changed source/bitstream.");
            native.Stop();
            var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "webp-match-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var nativeFile = Path.Combine(directory, "native.webp"); var managedFile = Path.Combine(directory, "managed.webp");
            File.WriteAllBytes(nativeFile, nativeBytes); File.WriteAllBytes(managedFile, files[match.ClosestOwned.Setting]);
            var record = new WarmWebpPreparation(1, "Lokad.Image", DateTime.UtcNow.ToString("O"), Recipe,
                JpegBenchmarkCase.Hash(pixels), JpegBenchmarkCase.Hash(reference), nativeQuality,
                Path.GetRelativePath(state.Root, nativeFile), Path.GetRelativePath(state.Root, managedFile), match, owned, order.ToArray(), SourceIdentity(state.Root),
                JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), JpegBenchmarkCase.HashFile(typeof(WarmWebp).Assembly.Location),
                JpegBenchmarkCase.HashFile(state.Ffmpeg), native.BuildIdentity, native.Ready);
            var path = Path.Combine(state.Root, "artifacts", "warm-reference", "webp-match.json");
            File.WriteAllText(path, JsonSerializer.Serialize(record, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine($"Prepared one warm VP8 pair: native quality {nativeQuality}, owned quality {match.ClosestOwned.Setting}, gap {match.PsnrGapDb:F4} dB, matched={match.Matched}; {owned.Length} owned settings, one source raster. {path}");
        }
        finally { process.ProcessorAffinity = oldAffinity; }
    }

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm native VP8 requires Linux.");
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1
            || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("VP8 worker requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase(".webp");
        try
        {
            var path = Path.Combine(state.Root, "artifacts", "warm-reference", "webp-match.json");
            prepared = JsonSerializer.Deserialize<WarmWebpPreparation>(File.ReadAllText(path), EncoderQualityCalibration.Json)
                ?? throw new InvalidOperationException("Missing warm VP8 preparation; run --prepare-warm-webp first.");
            ValidatePreparation(prepared);
            sourceIdentity = SourceIdentity(state.Root);
            pixels = HorizontalFlipInput.Create(8); reference = pixels;
            if (JpegBenchmarkCase.Hash(pixels) != prepared.RasterSha256 || JpegBenchmarkCase.Hash(reference) != prepared.ReferenceSha256
                || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256)
                throw new InvalidOperationException("Warm VP8 input/reference/oracle identity changed; prepare only this pair again.");
            options = new() { Lossless = false, Quality = prepared.Match.ClosestOwned.Setting, Effort = 4 };
            source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8); File.WriteAllBytes(state.Input, pixels);
            native = new NativeWarmReference(state, 8, cpu, "webp-encode", nativeQuality: prepared.NativeQuality);
            if (native.BuildIdentity.GetProperty("workerSha256").GetString() != prepared.NativeBuild.GetProperty("workerSha256").GetString())
                throw new InvalidOperationException("Native VP8 worker changed; prepare this pair again.");
            encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.ManagedFile));
            if (JpegBenchmarkCase.Hash(encoded) != prepared.Match.ClosestOwned.EncodedSha256
                || JpegBenchmarkCase.HashFile(state.NativeOutput) != prepared.Match.Native.EncodedSha256
                || JpegBenchmarkCase.HashFile(Path.Combine(state.Root, prepared.NativeFile)) != prepared.Match.Native.EncodedSha256)
                throw new InvalidOperationException("Prepared VP8 bitstreams changed.");
            ValidateManaged(); ValidateQuality();
            before = native.Measure(smoke, sampleCount: sampleCount); ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateManaged()
    {
        using var first = new MemoryStream(); using var second = new MemoryStream();
        ImageCodec.EncodeWebp(source, first, options); ImageCodec.EncodeWebp(source, second, options);
        if (ReferenceEquals(first.GetBuffer(), second.GetBuffer()) || !first.ToArray().AsSpan().SequenceEqual(encoded)
            || !second.ToArray().AsSpan().SequenceEqual(encoded) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("Warm VP8 output is not independent/deterministic or source changed.");
    }

    private void ValidateQuality()
    {
        managedQuality = EncoderQualityCalibration.Validate(encoded, reference, TargetProfile, options.Quality, state.Ffmpeg);
        nativeQuality = EncoderQualityCalibration.Validate(File.ReadAllBytes(state.NativeOutput), reference, TargetProfile, prepared.NativeQuality, state.Ffmpeg);
        if (managedQuality != prepared.Match.ClosestOwned || nativeQuality != prepared.Match.Native
            || !EncoderQualityCalibration.Match(nativeQuality, [managedQuality]).Matched)
            throw new InvalidOperationException("Warm VP8 source/decoder quality changed; prepare this pair again.");
    }

    [Benchmark]
    public int Managed()
    {
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, options);
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
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmWebp) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmWebp), caseId = Case.ToString(), smoke, nativeSampleCount = smoke ? 1 : sampleCount,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),
                width = source.Width, height = source.Height, depth = 8, cpu, managedAffinityCount = 1,
                processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(),
                managedIntrinsics = new { avx = System.Runtime.Intrinsics.X86.Avx.IsSupported, avx512F = System.Runtime.Intrinsics.X86.Avx512F.IsSupported },
                runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                jitPolicy = "Tiered compilation disabled; fully optimized JIT before measurement", sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(pixels), rasterSha256 = JpegBenchmarkCase.Hash(pixels), referenceSha256 = JpegBenchmarkCase.Hash(reference),
                managedOutputBytes = encoded.Length, nativeOutputBytes = nativeQuality.EncodedBytes,
                managedEncodedSha256 = JpegBenchmarkCase.Hash(encoded), nativeOutputSha256 = JpegBenchmarkCase.HashFile(state.NativeOutput),
                recipe = Recipe, sampling = "4:2:0", effort = 4, lossless = false, qualityMatched = true, alphaExact = true, decoderAgreementWithinBounds = true,
                psnrGapDb = prepared.Match.PsnrGapDb, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb,
                ownedEnumeratedSettings = prepared.OwnedSettings.Length, ownedEvaluationOrder = prepared.OwnedEvaluationOrder,
                ownedSettings = prepared.OwnedSettings, managedQuality, nativeQuality,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmWebp).Assembly.Location), nativeBuild = native.BuildIdentity,
                nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "Resident original RGBA8 to complete independent VP8 WebP bytes with exact alpha. Native codec/scratch/frame/packet wrappers reused; RGBA-to-BGRA repacking and complete encode execute every operation. Fresh managed MemoryStream creation, growth, encode and disposal included; no redundant ToArray. Both effort settings fixed at 4; managed fixed entropy and native adaptive coding declared. Alpha-weighted RGB source-error matching before composite rounding, <=0.25 dB fixed band; unweighted decoder agreement includes hidden RGB. Fresh geometry/decoder/alpha/source/ownership/bitstream checks before/after. Startup, IPC, I/O, fixture generation, quality enumeration and checks excluded."
            }, new JsonSerializerOptions(EncoderQualityCalibration.Json) { PropertyNamingPolicy = null }) + "\n");
            Console.WriteLine("Validated warm VP8 pair: " + path);
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
