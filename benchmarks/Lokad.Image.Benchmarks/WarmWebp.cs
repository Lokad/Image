using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmWebpCase { Encode8, Encode16, EncodeOpaque8 }

internal sealed record WarmWebpPreparation(int SchemaVersion, string Project, string GeneratedUtc, string Recipe,
    string RasterSha256, string ReferenceSha256, int NativeQuality, string NativeFile, string ManagedFile,
    QualityMatch Match, QualitySample[] OwnedSettings, int[] OwnedEvaluationOrder, JsonElement SourceIdentity,
    string LibrarySha256, string BenchmarkAssemblySha256, string FfmpegSha256, JsonElement NativeBuild, JsonElement NativeReady, string QualityPolicy, int SourceDepth = 8, bool SourceOpaque = false);

[MemoryDiagnoser]
public class WarmWebp
{
    public static IEnumerable<WarmWebpCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmWebpCase Case { get; set; }
    private int Depth => Case == WarmWebpCase.Encode16 ? 16 : 8;
    private bool Opaque => Case == WarmWebpCase.EncodeOpaque8;
    private EncoderQualityProfile TargetProfile => ProfileFor(Depth, Opaque);
    private static EncoderQualityProfile ProfileFor(int depth, bool opaque = false) => new(513, depth, QualityEncoder.Vp8, QualityPattern.Asymmetric, opaque ? QualityAlpha.Opaque : QualityAlpha.Gradient);
    private const string Recipe8 = "HorizontalFlipInput.Create(8): original 513x259 asymmetric RGBA8, phase zero, alpha zero at both first-row ends, hidden RGB unchanged. VP8 target is 8-bit 4:2:0 with exact source alpha; effort 4 on both subjects. No extra source raster.";
    private string Recipe => RecipeFor(Depth, Opaque);
    private static string RecipeFor(int depth, bool opaque = false) => opaque
        ? "Existing original 513x259 RGBA8 RGB bytes unchanged; alpha set to255. Opaque VP8 4:2:0, effort4, whole-raster RGB source quality; no alpha-plane encoding or extra original raster."
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
        return selection switch
        {
            null or "Encode8" => [WarmWebpCase.Encode8],
            "Encode16" => [WarmWebpCase.Encode16],
            "EncodeOpaque8" => [WarmWebpCase.EncodeOpaque8],
            _ => throw new ArgumentException("Choose one VP8 case: Encode8, Encode16 or EncodeOpaque8.")
        };
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
        var allowedPolicy = record.QualityPolicy switch
        {
            "fixed-anchor-v1" => record.NativeQuality == 75 && record.OwnedSettings.Length == 1
                && record.OwnedSettings[0].Setting == 100 && record.OwnedEvaluationOrder.Length == 1
                && record.OwnedEvaluationOrder[0] == 100,
            "bounded-v1" => record.OwnedSettings.Length is >= 5 and <= EncoderQualityCalibration.MaximumOwnedSettings,
            _ => false
        };
        if (record.SchemaVersion != 1 || record.Project != "Lokad.Image" || record.Recipe != RecipeFor(depth, opaque) || record.SourceDepth != depth || record.SourceOpaque != opaque || depth is not (8 or 16)
            || record.NativeQuality is < 1 or > 100 || record.NativeQuality != record.Match.Native.Setting
            || !record.Match.Matched || record.Match.PsnrGapDb is not (>= 0 and <= EncoderQualityCalibration.MaximumPsnrGapDb)
            || !allowedPolicy || (depth == 16 || opaque) && record.QualityPolicy != "fixed-anchor-v1" || opaque && depth != 8
            || record.OwnedEvaluationOrder.Length != record.OwnedSettings.Length
            || record.OwnedEvaluationOrder.Distinct().Count() != record.OwnedSettings.Length
            || !record.OwnedEvaluationOrder.Order().SequenceEqual(record.OwnedSettings.Select(sample => sample.Setting).Order())
            || !record.Match.Native.AlphaExact || !record.Match.ClosestOwned.AlphaExact
            || record.Match.Native.Codec != "VP8" || record.Match.ClosestOwned.Codec != "VP8"
            || record.Match.Native.Sampling != "4:2:0" || record.Match.ClosestOwned.Sampling != "4:2:0"
            || record.Match.Native.SourceError.Scale != (opaque ? 1 : 255)
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
        var record = new WarmWebpPreparation(1, "Lokad.Image", "", Recipe8, "", "", 75, "", "", matched,
            owned, seeds, empty, "", "", "", empty, empty, "bounded-v1");
        ValidatePreparation(record);
        var fixedOwned = Sample(100);
        var fixedRecord = record with { QualityPolicy = "fixed-anchor-v1", OwnedSettings = [fixedOwned],
            OwnedEvaluationOrder = [100], Match = EncoderQualityCalibration.Match(native, [fixedOwned]) };
        ValidatePreparation(fixedRecord);
        var fixed16 = fixedRecord with { SourceDepth = 16, Recipe = RecipeFor(16) };
        ValidatePreparation(fixed16, 16);
        var opaqueNative = native with { SourceError = native.SourceError with { Scale = 1 } };
        var opaqueOwned = fixedOwned with { SourceError = fixedOwned.SourceError with { Scale = 1 } };
        var fixedOpaque = fixedRecord with { SourceOpaque = true, Recipe = RecipeFor(8, true), Match = EncoderQualityCalibration.Match(opaqueNative, [opaqueOwned]), OwnedSettings = [opaqueOwned] };
        ValidatePreparation(fixedOpaque, 8, true);
        foreach (var invalid in new[] { fixedOpaque with { SourceOpaque = false }, fixedOpaque with { Recipe = Recipe8 }, fixedOpaque with { Match = fixedRecord.Match, OwnedSettings = fixedRecord.OwnedSettings } })
        {
            try { ValidatePreparation(invalid, 8, true); } catch (InvalidOperationException) { continue; }
            throw new InvalidOperationException("Opaque VP8 gate accepted invalid opacity/metric metadata.");
        }
        foreach (var invalid16 in new[] { fixed16 with { SourceDepth = 8 }, fixed16 with { Recipe = Recipe8 }, fixed16 with { QualityPolicy = "bounded-v1" } })
        {
            try { ValidatePreparation(invalid16, 16); } catch (InvalidOperationException) { continue; }
            throw new InvalidOperationException("VP8 depth16 gate accepted invalid metadata.");
        }
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
            record with { Match = matched with { ClosestOwned = matched.ClosestOwned with { Codec = "JPEG" } } },
            fixedRecord with { NativeQuality = 74 },
            fixedRecord with { OwnedSettings = [Sample(99)], OwnedEvaluationOrder = [99] },
            fixedRecord with { OwnedSettings = [fixedOwned, fixedOwned], OwnedEvaluationOrder = [100, 100] },
            fixedRecord with { QualityPolicy = "unknown" }
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
    public static void Prepare(int cpu, int nativeQuality = 75, int depth = 8, bool opaque = false)
    {
        if (!OperatingSystem.IsLinux() || cpu is < 0 or > 62 || nativeQuality != 75 || depth is not (8 or 16) || opaque && depth != 8)
            throw new ArgumentException("The fixed VP8 pair requires Linux, one allowed CPU, native quality 75 and owned quality 100.");
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
            using var native = new NativeWarmReference(state, depth, cpu, opaque ? "webp-opaque-encode" : "webp-encode", expectedRaw: NormalizedInput(state.Root, pixels, reference, depth), nativeQuality: nativeQuality);
            var nativeBytes = File.ReadAllBytes(state.NativeOutput);
            var nativeSample = EncoderQualityCalibration.Validate(nativeBytes, reference, ProfileFor(depth, opaque), nativeQuality, state.Ffmpeg);
            using var output = new MemoryStream();
            ImageCodec.EncodeWebp(source, output, new WebpEncodeOptions { Lossless = false, Quality = 100, Effort = 4 });
            var ownedBytes = output.ToArray();
            QualitySample[] owned = [EncoderQualityCalibration.Validate(ownedBytes, reference, ProfileFor(depth, opaque), 100, state.Ffmpeg)];
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
                Path.GetRelativePath(state.Root, nativeFile), Path.GetRelativePath(state.Root, managedFile), match, owned, [100], SourceIdentity(state.Root),
                JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), JpegBenchmarkCase.HashFile(typeof(WarmWebp).Assembly.Location),
                JpegBenchmarkCase.HashFile(state.Ffmpeg), native.BuildIdentity, native.Ready, "fixed-anchor-v1", depth, opaque);
            var path = Path.Combine(state.Root, "artifacts", "warm-reference", opaque ? "webp-opaque-match.json" : depth == 16 ? "webp16-match.json" : "webp-match.json");
            File.WriteAllText(path, JsonSerializer.Serialize(record, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine($"Prepared one warm VP8 pair: native quality {nativeQuality}, owned quality {match.ClosestOwned.Setting}, gap {match.PsnrGapDb:F4} dB, matched={match.Matched}; {owned.Length} owned settings, one source raster. {path}");
        }
        finally { process.ProcessorAffinity = oldAffinity; }
    }

    private static byte[] CreatePixels(int depth, bool opaque)
    {
        var result = HorizontalFlipInput.Create(depth);
        if (opaque) for (var i = 3; i < result.Length; i += 4) result[i] = 255;
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

    private static string? NormalizedInput(string root, byte[] pixels, byte[] reference, int depth)
    {
        if (depth != 16) return null;
        var directory = Path.Combine(root, "artifacts", "warm-reference", "webp16-input"); Directory.CreateDirectory(directory);
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
            var path = Path.Combine(state.Root, "artifacts", "warm-reference", Opaque ? "webp-opaque-match.json" : Depth == 16 ? "webp16-match.json" : "webp-match.json");
            prepared = JsonSerializer.Deserialize<WarmWebpPreparation>(File.ReadAllText(path), EncoderQualityCalibration.Json)
                ?? throw new InvalidOperationException("Missing warm VP8 preparation; run --prepare-warm-webp first.");
            ValidatePreparation(prepared, Depth, Opaque);
            sourceIdentity = SourceIdentity(state.Root);
            pixels = CreatePixels(Depth, Opaque); reference = Normalize(pixels, Depth);
            if (JpegBenchmarkCase.Hash(pixels) != prepared.RasterSha256 || JpegBenchmarkCase.Hash(reference) != prepared.ReferenceSha256
                || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256)
                throw new InvalidOperationException("Warm VP8 input/reference/oracle identity changed; prepare only this pair again.");
            options = new() { Lossless = false, Quality = prepared.Match.ClosestOwned.Setting, Effort = 4 };
            source = JpegBenchmarkCase.Buffer(pixels, 513, 259, Depth); File.WriteAllBytes(state.Input, pixels);
            native = new NativeWarmReference(state, Depth, cpu, Opaque ? "webp-opaque-encode" : "webp-encode", expectedRaw: NormalizedInput(state.Root, pixels, reference, Depth), nativeQuality: prepared.NativeQuality);
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
                width = source.Width, height = source.Height, depth = Depth, encodedDepth = 8, cpu, managedAffinityCount = 1,
                processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(),
                managedIntrinsics = new { avx = System.Runtime.Intrinsics.X86.Avx.IsSupported, avx512F = System.Runtime.Intrinsics.X86.Avx512F.IsSupported },
                runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                jitPolicy = "Tiered compilation disabled; fully optimized JIT before measurement", sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(pixels), rasterSha256 = JpegBenchmarkCase.Hash(pixels), referenceSha256 = JpegBenchmarkCase.Hash(reference),
                managedOutputBytes = encoded.Length, nativeOutputBytes = nativeQuality.EncodedBytes,
                managedEncodedSha256 = JpegBenchmarkCase.Hash(encoded), nativeOutputSha256 = JpegBenchmarkCase.HashFile(state.NativeOutput),
                recipe = Recipe, opaque = Opaque, sourceQualityMetric = TargetProfile.SourceMetric, sampling = "4:2:0", effort = 4, lossless = false, qualityMatched = true, fullSourceNormalizationTimed = Depth == 16, normalizedReferenceExact = true, alphaExact = true, decoderAgreementWithinBounds = true,
                psnrGapDb = prepared.Match.PsnrGapDb, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb,
                ownedEnumeratedSettings = prepared.OwnedSettings.Length, ownedEvaluationOrder = prepared.OwnedEvaluationOrder,
                ownedSettingsPolicy = prepared.QualityPolicy, qualityEnumerationPerformed = prepared.QualityPolicy != "fixed-anchor-v1",
                ownedSettings = prepared.OwnedSettings, managedQuality, nativeQuality,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmWebp).Assembly.Location), nativeBuild = native.BuildIdentity,
                nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"Resident original RGBA{Depth} to complete independent VP8 WebP bytes with exact normalized alpha. Full source normalization and repacking included. Native codec/scratch/frame/packet wrappers reused; RGBA-to-BGRA repacking and complete encode execute every operation. Fresh managed MemoryStream creation, growth, encode and disposal included; no redundant ToArray. Both effort settings fixed at 4; managed fixed entropy and native adaptive coding declared. {(Opaque ? "Opaque source: whole-raster RGB source-error matching, no alpha-plane encoding" : "Alpha-weighted RGB source-error matching before composite rounding")}, <=0.25 dB fixed band; unweighted decoder agreement includes hidden RGB. Fresh geometry/decoder/alpha/source/ownership/bitstream checks before/after. Startup, IPC, I/O, fixture generation, quality enumeration and checks excluded."
            }, new JsonSerializerOptions(EncoderQualityCalibration.Json) { PropertyNamingPolicy = null }) + "\n");
            Console.WriteLine("Validated warm VP8 pair: " + path);
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
