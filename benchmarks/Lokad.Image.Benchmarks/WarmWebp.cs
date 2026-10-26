using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmWebpCase { Encode8, Encode16, EncodeOpaque8, EncodeOpaque16 }

internal sealed record WarmWebpPreparation(int SchemaVersion, string Project, string GeneratedUtc, string Recipe,
    string RasterSha256, string ReferenceSha256, int NativeQuality, string NativeFile, string ManagedFile,
    QualityMatch Match, QualitySample[] OwnedSettings, int[] OwnedEvaluationOrder, JsonElement SourceIdentity,
    string LibrarySha256, string BenchmarkAssemblySha256, string FfmpegSha256, JsonElement NativeBuild, JsonElement NativeReady, string QualityPolicy, int SourceDepth = 8, bool SourceOpaque = false);

[MemoryDiagnoser]
public class WarmWebp
{
    public static IEnumerable<WarmWebpCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmWebpCase Case { get; set; }
    private int Depth => Case is WarmWebpCase.Encode16 or WarmWebpCase.EncodeOpaque16 ? 16 : 8;
    private bool Opaque => Case is WarmWebpCase.EncodeOpaque8 or WarmWebpCase.EncodeOpaque16;
    private EncoderQualityProfile TargetProfile => ProfileFor(Depth, Opaque);
    private static EncoderQualityProfile ProfileFor(int depth, bool opaque = false) => new(513, depth, QualityEncoder.Vp8, QualityPattern.Asymmetric, opaque ? QualityAlpha.Opaque : QualityAlpha.Gradient);
    private const string Recipe8 = "HorizontalFlipInput.Create(8): original 513x259 asymmetric RGBA8, phase zero, alpha zero at both first-row ends, hidden RGB unchanged. VP8 target is 8-bit 4:2:0 with exact source alpha; effort 4 on both subjects. No extra source raster.";
    private string Recipe => RecipeFor(Depth, Opaque);
    private static string RecipeFor(int depth, bool opaque = false) => opaque
        ? depth == 16
            ? "Existing original 513x259 RGBA16 RGB words unchanged; alpha set to65535. Opaque VP8 4:2:0, effort4, whole-raster RGB source quality; full channel normalization (sample+128)/257 and repacking inside every operation. No alpha-plane encoding or extra original raster."
            : "Existing original 513x259 RGBA8 RGB bytes unchanged; alpha set to255. Opaque VP8 4:2:0, effort4, whole-raster RGB source quality; no alpha-plane encoding or extra original raster."
        : depth == 16
        ? Recipe8.Replace("Create(8)", "Create(16)").Replace("RGBA8", "RGBA16").Replace("exact source alpha", "exact normalized alpha")
            + " Full channel normalization (sample+128)/257 and repacking execute inside every operation." : Recipe8;
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
        var result = selection is null ? new[] { WarmWebpCase.Encode8 } : selection.Split(',')
            .Select(value => Enum.TryParse<WarmWebpCase>(value, out var parsed) && Enum.IsDefined(parsed)
                ? parsed : throw new ArgumentException("Choose Encode8, Encode16, EncodeOpaque8 or EncodeOpaque16.")).ToArray();
        if (result.Length is < 1 or > 3 || result.Distinct().Count() != result.Length)
            throw new ArgumentException("Choose one to three distinct VP8 cases.");
        return result;
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

    private static void ValidatePreparation(WarmWebpPreparation record, int depth = 8, bool opaque = false)
    {
        var anchor = record.Match.Native;
        // Archived managed quality-calibration fields do not select current
        // settings. Frozen native identity plus complete packet equality does.
        if (record.SchemaVersion != 1 || record.Project != "Lokad.Image"
            || record.Recipe != RecipeFor(depth, opaque) || record.SourceDepth != depth
            || record.SourceOpaque != opaque || depth is not (8 or 16)
            || record.NativeQuality != 75 || anchor.Setting != 75
            || anchor.EncodedBytes <= 0 || anchor.EncodedSha256.Length != 64
            || !anchor.EncodedSha256.All(char.IsAsciiHexDigit)
            || !anchor.AlphaExact || anchor.Codec != "VP8" || anchor.Sampling != "4:2:0")
            throw new InvalidOperationException("Frozen VP8 native settings or source metadata differ.");
    }

    public static void ValidateControls()
    {
        using var document = JsonDocument.Parse("{}"); var empty = document.RootElement.Clone();
        var anchor = new QualitySample(75, 100, new string('0', 64), "", "", "VP8", "4:2:0", new(0, 0, 3), new(0, 0, 3));
        var record = new WarmWebpPreparation(1, "Lokad.Image", "", Recipe8, "", "", 75, "", "",
            new(anchor, anchor, null, false, null), [], [], empty, "", "", "", empty, empty, "");
        foreach (var depth in new[] { 8, 16 })
        foreach (var opaque in new[] { false, true })
            ValidatePreparation(record with { Recipe = RecipeFor(depth, opaque), SourceDepth = depth, SourceOpaque = opaque }, depth, opaque);
        WarmWebpPreparation[] rejected = [
            record with { NativeQuality = 74 }, record with { SourceDepth = 16 },
            record with { SourceOpaque = true }, record with { Recipe = "Changed recipe" },
            record with { Match = record.Match with { Native = anchor with { Setting = 74 } } },
            record with { Match = record.Match with { Native = anchor with { EncodedSha256 = "invalid" } } },
            record with { Match = record.Match with { Native = anchor with { AlphaExact = false } } },
            record with { Match = record.Match with { Native = anchor with { Codec = "JPEG" } } },
            record with { Match = record.Match with { Native = anchor with { Sampling = "4:4:4" } } }
        ];
        foreach (var invalid in rejected)
        {
            try { ValidatePreparation(invalid); } catch (InvalidOperationException) { continue; }
            throw new InvalidOperationException("VP8 native-input gate accepted invalid metadata.");
        }
        Console.WriteLine($"VP8 native-input gate: four existing depth/opacity forms accepted, {rejected.Length} invalid records rejected; no encode or timing.");
    }

    // Preparation is explicit and untimed. Benchmark launches validate the
    // selected bitstreams again without repeating quality enumeration.
    public static void Prepare(int cpu, int nativeQuality = 75, int depth = 8, bool opaque = false)
    {
        if (!OperatingSystem.IsLinux() || cpu is < 0 or > 62 || nativeQuality != 75 || depth is not (8 or 16))
            throw new ArgumentException("The fixed VP8 pair requires Linux, one allowed CPU, native/managed quality75 and complete packet equality.");
        using var process = Process.GetCurrentProcess(); var oldAffinity = process.ProcessorAffinity;
        process.ProcessorAffinity = new IntPtr(1L << cpu);
        try
        {
            using var state = new JpegBenchmarkCase(".webp");
            var pixels = CreatePixels(depth, opaque);
            // The metric reference is this same original RGBA raster, with exact alpha.
            var reference = Normalize(pixels, depth);
            using var source = JpegBenchmarkCase.Buffer(pixels, 513, 259, depth);
            File.WriteAllBytes(state.Input, pixels);
            using var native = new NativeWarmReference(state, depth, cpu, opaque ? "webp-opaque-encode" : "webp-encode", expectedRaw: NormalizedInput(state.Root, pixels, reference, depth, opaque), nativeQuality: nativeQuality);
            var nativeBytes = File.ReadAllBytes(state.NativeOutput);
            var nativeSample = EncoderQualityCalibration.Validate(nativeBytes, reference, ProfileFor(depth, opaque), nativeQuality, state.Ffmpeg);
            using var output = new MemoryStream();
            ImageCodec.EncodeWebp(source, output, new WebpEncodeOptions { Lossless = false, Quality = 75, Effort = 4 });
            var ownedBytes = output.ToArray();
            QualitySample[] owned = [EncoderQualityCalibration.Validate(ownedBytes, reference, ProfileFor(depth, opaque), 75, state.Ffmpeg)];
            if (!ownedBytes.AsSpan().SequenceEqual(nativeBytes))
                throw new InvalidOperationException("Prepared managed/native VP8 packets differ.");
            var match = EncoderQualityCalibration.Match(nativeSample, owned);
            native.Check();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativeBytes)
                || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Quality preparation changed source/bitstream.");
            native.Stop();
            var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "webp-match-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var nativeFile = Path.Combine(directory, "native.webp"); var managedFile = Path.Combine(directory, "managed.webp");
            File.WriteAllBytes(nativeFile, nativeBytes); File.WriteAllBytes(managedFile, ownedBytes);
            var record = new WarmWebpPreparation(1, "Lokad.Image", DateTime.UtcNow.ToString("O"), RecipeFor(depth, opaque),
                JpegBenchmarkCase.Hash(pixels), JpegBenchmarkCase.Hash(reference), nativeQuality,
                Path.GetRelativePath(state.Root, nativeFile), Path.GetRelativePath(state.Root, managedFile), match, owned, [75], SourceIdentity(state.Root),
                JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), JpegBenchmarkCase.HashFile(typeof(WarmWebp).Assembly.Location),
                JpegBenchmarkCase.HashFile(state.Ffmpeg), native.BuildIdentity, native.Ready, "exact-packet-v1", depth, opaque);
            var path = Path.Combine(state.Root, "artifacts", "warm-reference", opaque ? depth == 16 ? "webp-opaque16-match.json" : "webp-opaque-match.json" : depth == 16 ? "webp16-match.json" : "webp-match.json");
            File.WriteAllText(path, JsonSerializer.Serialize(record, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine($"Prepared one warm VP8 pair: native quality {nativeQuality}, owned quality {match.ClosestOwned.Setting}, gap {match.PsnrGapDb:F4} dB, matched={match.Matched}; {owned.Length} owned settings, one source raster. {path}");
        }
        finally { process.ProcessorAffinity = oldAffinity; }
    }

    private static byte[] CreatePixels(int depth, bool opaque)
    {
        var result = HorizontalFlipInput.Create(depth);
        if (opaque)
            for (var i = depth == 16 ? 6 : 3; i < result.Length; i += depth == 16 ? 8 : 4)
            {
                result[i] = 255;
                if (depth == 16) result[i + 1] = 255;
            }
        return result;
    }

    private static byte[] Normalize(byte[] pixels, int depth)
    {
        if (depth == 8) return pixels;
        var result = new byte[pixels.Length / 2];
        for (var i = 0; i < result.Length; i++)
            result[i] = (byte)((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(pixels.AsSpan(i * 2)) + 128) / 257);
        return result;
    }

    private static string? NormalizedInput(string root, byte[] pixels, byte[] reference, int depth, bool opaque = false)
    {
        if (depth != 16) return null;
        var directory = Path.Combine(root, "artifacts", "warm-reference", opaque ? "webp-opaque16-input" : "webp16-input"); Directory.CreateDirectory(directory);
        Save("source.rgba", pixels); Save("normalized.rgba", reference);
        return Path.Combine(directory, "normalized.rgba");
        void Save(string name, byte[] bytes)
        {
            var path = Path.Combine(directory, name);
            if (File.Exists(path))
            {
                if (!File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes)) throw new InvalidOperationException("Frozen VP8 source/reference changed.");
            }
            else File.WriteAllBytes(path, bytes);
        }
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
            var path = Path.Combine(state.Root, "artifacts", "warm-reference", Opaque ? Depth == 16 ? "webp-opaque16-match.json" : "webp-opaque-match.json" : Depth == 16 ? "webp16-match.json" : "webp-match.json");
            prepared = JsonSerializer.Deserialize<WarmWebpPreparation>(File.ReadAllText(path), EncoderQualityCalibration.Json)
                ?? throw new InvalidOperationException("Missing warm VP8 preparation; run --prepare-warm-webp first.");
            ValidatePreparation(prepared, Depth, Opaque);
            sourceIdentity = SourceIdentity(state.Root);
            pixels = CreatePixels(Depth, Opaque); reference = Normalize(pixels, Depth);
            if (JpegBenchmarkCase.Hash(pixels) != prepared.RasterSha256 || JpegBenchmarkCase.Hash(reference) != prepared.ReferenceSha256
                || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256)
                throw new InvalidOperationException("Warm VP8 input/reference/oracle identity changed; prepare only this pair again.");
            options = new() { Lossless = false, Quality = prepared.NativeQuality, Effort = 4 };
            source = JpegBenchmarkCase.Buffer(pixels, 513, 259, Depth); File.WriteAllBytes(state.Input, pixels);
            native = new NativeWarmReference(state, Depth, cpu, Opaque ? "webp-opaque-encode" : "webp-encode", expectedRaw: NormalizedInput(state.Root, pixels, reference, Depth, Opaque), nativeQuality: prepared.NativeQuality);
            if (native.BuildIdentity.GetProperty("workerSha256").GetString() != prepared.NativeBuild.GetProperty("workerSha256").GetString())
                throw new InvalidOperationException("Native VP8 worker changed; prepare this pair again.");
            encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.NativeFile));
            if (JpegBenchmarkCase.Hash(encoded) != prepared.Match.Native.EncodedSha256
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
        var nativePacket = File.ReadAllBytes(state.NativeOutput);
        if (!encoded.AsSpan().SequenceEqual(nativePacket))
            throw new InvalidOperationException("Complete managed/native VP8 packets differ.");
        // Retain only a matched-packet reconstruction sanity check. Current
        // public decoder hashes replace no frozen input or native setting.
        managedQuality = EncoderQualityCalibration.Validate(encoded, reference, TargetProfile, options.Quality, state.Ffmpeg);
        nativeQuality = EncoderQualityCalibration.Validate(nativePacket, reference, TargetProfile, prepared.NativeQuality, state.Ffmpeg);
        if (!EncoderQualityCalibration.Match(nativeQuality, [managedQuality]).Matched)
            throw new InvalidOperationException("Byte-matched VP8 packets reconstruct different quality.");
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
                width = source.Width, height = source.Height, depth = Depth, encodedDepth = 8, cpu, managedAffinityCount = 1,
                processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(),
                managedIntrinsics = new { avx = System.Runtime.Intrinsics.X86.Avx.IsSupported, avx512F = System.Runtime.Intrinsics.X86.Avx512F.IsSupported },
                runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                jitPolicy = "Tiered compilation disabled; fully optimized JIT before measurement", sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(pixels), rasterSha256 = JpegBenchmarkCase.Hash(pixels), referenceSha256 = JpegBenchmarkCase.Hash(reference),
                managedOutputBytes = encoded.Length, nativeOutputBytes = nativeQuality.EncodedBytes,
                managedEncodedSha256 = JpegBenchmarkCase.Hash(encoded), nativeOutputSha256 = JpegBenchmarkCase.HashFile(state.NativeOutput),
                recipe = Recipe, opaque = Opaque,
                entropyPolicy = "Coefficient modeling selected by the identified producer and geometry, memory and cancellation eligibility", sourceQualityMetric = TargetProfile.SourceMetric, sampling = "4:2:0", effort = 4, lossless = false, qualityMatched = true, fullSourceNormalizationTimed = Depth == 16, normalizedReferenceExact = true, alphaExact = true, decoderAgreementWithinBounds = true,
                completePacketExact = true, psnrGapDb = EncoderQualityCalibration.Match(nativeQuality, [managedQuality]).PsnrGapDb, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb,
                ownedEnumeratedSettings = 1, ownedEvaluationOrder = new[] { options.Quality },
                ownedSettingsPolicy = "exact-packet-v1", qualityEnumerationPerformed = false,
                ownedSettings = new[] { managedQuality }, managedQuality, nativeQuality,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmWebp).Assembly.Location), nativeBuild = native.BuildIdentity,
                nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"Resident original RGBA{Depth} to complete independent VP8 WebP bytes with exact normalized alpha. Full source normalization and repacking included. Native codec/scratch/frame/packet wrappers reused; RGBA-to-BGRA repacking and complete encode execute every operation. Fresh managed MemoryStream creation, growth, encode and disposal included; no redundant ToArray. Both effort settings fixed at 4; managed coefficient modeling follows the identified producer's geometry, memory and cancellation eligibility; native adaptive coding declared. {(Opaque ? "Opaque source: whole-raster RGB source-error matching, no alpha-plane encoding" : "Alpha-weighted RGB source-error matching before composite rounding")}, Complete native packet equality at frozen quality75/method4 is required; matched-packet reconstruction is a sanity check, including hidden RGB. Fresh geometry/decoder/alpha/source/ownership/bitstream checks before/after. Startup, IPC, I/O, fixture generation, quality enumeration and checks excluded."
            }, new JsonSerializerOptions(EncoderQualityCalibration.Json) { PropertyNamingPolicy = null }) + "\n");
            Console.WriteLine("Validated warm VP8 pair: " + path);
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
