using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmJpegCase { Encode8, Encode420_8, Encode422_8, Encode16, Encode420_16, Encode420Stream8, Encode420Stream16, Encode422_16, Encode420Restart8 }

internal sealed record WarmJpegPreparation(int SchemaVersion, string Project, string GeneratedUtc, string Recipe,
    string RasterSha256, string ReferenceSha256, int NativeQuality, uint BackgroundRgb, string NativeFile, string ManagedFile,
    QualityMatch Match, QualitySample[] OwnedSettings, int[] OwnedEvaluationOrder, JsonElement SourceIdentity,
    string LibrarySha256, string BenchmarkAssemblySha256, string FfmpegSha256, JsonElement NativeBuild, JsonElement NativeReady,
    string? Sampling = null, string? QualityPolicy = null, int SourceDepth = 8);

[MemoryDiagnoser]
public class WarmJpeg
{
    public static IEnumerable<WarmJpegCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmJpegCase Case { get; set; }
    private const uint Background = 0x204060;
    private bool Source16 => Case is WarmJpegCase.Encode422_16 or WarmJpegCase.Encode16 or WarmJpegCase.Encode420_16 or WarmJpegCase.Encode420Stream16;
    private int Depth => Source16 ? 16 : 8;
    private bool FixedAnchor => Subsampled || Source16 || AdaptiveEntropy;
    private bool Horizontal => Case is WarmJpegCase.Encode422_8 or WarmJpegCase.Encode422_16;
    private bool Restart => Case == WarmJpegCase.Encode420Restart8;
    private bool ForwardOnly => Case is WarmJpegCase.Encode420Stream8 or WarmJpegCase.Encode420Stream16;
    private bool Subsampled => Case is WarmJpegCase.Encode420Restart8 or WarmJpegCase.Encode422_16 or WarmJpegCase.Encode420_8 or WarmJpegCase.Encode422_8 or WarmJpegCase.Encode420_16 or WarmJpegCase.Encode420Stream8 or WarmJpegCase.Encode420Stream16;
    private string NativeSampling => Horizontal ? "422" : Subsampled ? "420" : "444";
    private string DisplaySampling => Horizontal ? "4:2:2" : Subsampled ? "4:2:0" : "4:4:4";
    private string FixedPolicy => Restart ? FixedRestartPolicy : Source16 ? (Horizontal ? Fixed42216Policy : Subsampled ? Fixed42016Policy : Fixed16Policy)
        : Horizontal ? Fixed422Policy : Subsampled ? Fixed420Policy : Fixed444Policy;
    private int FixedQuality => Horizontal ? 65 : 69;
    private EncoderQualityProfile TargetProfile => ProfileFor(Subsampled, Horizontal, Depth);
    private static EncoderQualityProfile ProfileFor(bool subsampled, bool horizontal = false, int depth = 8) => new(513, depth,
        horizontal ? QualityEncoder.Jpeg422 : subsampled ? QualityEncoder.Jpeg420 : QualityEncoder.Jpeg444, QualityPattern.Asymmetric);
    private const string Recipe444 = "HorizontalFlipInput.Create(8): original 513x259 asymmetric RGBA8, phase zero, alpha zero at both first-row ends, hidden RGB unchanged. JPEG source alpha is flattened once onto RGB 0x204060; target is opaque 8-bit 4:4:4. No extra source raster.";
    private static string RecipeFor(bool subsampled, bool horizontal = false, int depth = 8, bool forwardOnly = false, bool restartRows = false)
    {
        var recipe = horizontal ? Recipe444.Replace("4:4:4", "4:2:2") : subsampled ? Recipe444.Replace("4:4:4", "4:2:0") : Recipe444;
        var result = depth == 16 ? recipe.Replace("Create(8)", "Create(16)").Replace("RGBA8", "RGBA16")
            + " Full 16-bit alpha flattening and normalization to 8-bit use one rounding after blending, inside every operation." : recipe;
        if (forwardOnly) result += " Fresh forward-only Stream wrapper over fresh owned MemoryStream storage; fixed geometric AC Huffman path.";
        if (restartRows) result += " Restart interval33 MCUs, one row per interval, sixteen sequenced restart markers.";
        return AdaptiveEntropy ? result + " Independently implemented adaptive Huffman coding, admitted private event workspace, same coefficients and quality anchor." : result;
    }
    private static bool AdaptiveEntropy => Environment.GetEnvironmentVariable("LOKAD_IMAGE_JPEG_ENTROPY") switch
    {
        null => false,
        "adaptive-v1" => true,
        "fixed-geometric-v1" => false,
        _ => throw new InvalidOperationException("Unknown JPEG entropy preparation policy.")
    };
    private static string MatchFile(bool subsampled, bool horizontal, int depth, bool forwardOnly = false, bool restartRows = false)
    {
        if (restartRows) return "jpeg420-restart33-adaptive-match.json";
        if (forwardOnly) return depth == 16 ? "jpeg42016-stream-geometric-match.json" : "jpeg420-stream-geometric-match.json";
        var name = depth == 16 ? (horizontal ? "jpeg42216-match.json" : subsampled ? "jpeg42016-match.json" : "jpeg16-match.json")
            : horizontal ? "jpeg422-match.json" : subsampled ? "jpeg420-match.json" : "jpeg-match.json";
        return AdaptiveEntropy ? name.Replace("-match.json", "-adaptive-match.json") : name;
    }
    private string Recipe => RecipeFor(Subsampled, Horizontal, Depth, ForwardOnly, Restart);
    private const string FixedRestartPolicy = "fixed-anchor-v1: native qscale5 / managed quality69 / 4:2:0 / restart interval33; serial native slices; one owned evaluation; no quality sweep";
    private const string Fixed444Policy = "fixed-anchor-v1: native qscale 5 / managed quality 69 / 4:4:4; previously matched anchor; one owned evaluation; no quality sweep";
    private const string Fixed420Policy = "fixed-anchor-v1: native qscale 5 / managed quality 69 / 4:2:0; one owned evaluation; no quality sweep";
    private const string Fixed422Policy = "fixed-anchor-v1: native qscale 5 / managed quality 65 / 4:2:2; one owned evaluation; no quality sweep";
    private const string Fixed16Policy = "fixed-anchor-v1: native qscale 5 / managed quality 69 / RGBA16 to 4:4:4; one owned evaluation; no quality sweep";
    private const string Fixed42216Policy = "fixed-anchor-v1: native qscale 5 / managed quality 65 / RGBA16 to 4:2:2; one owned evaluation; no quality sweep";
    private const string Fixed42016Policy = "fixed-anchor-v1: native qscale 5 / managed quality 69 / RGBA16 to 4:2:0; one owned evaluation; no quality sweep";
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
        return selection switch
        {
            null or "Encode8" => [WarmJpegCase.Encode8],
            "Encode420_8" => [WarmJpegCase.Encode420_8],
            "Encode422_8" => [WarmJpegCase.Encode422_8],
            "Encode16" => [WarmJpegCase.Encode16],
            "Encode420_16" => [WarmJpegCase.Encode420_16],
            "Encode422_16" => [WarmJpegCase.Encode422_16],
            "Encode420Restart8" when AdaptiveEntropy => [WarmJpegCase.Encode420Restart8],
            "Encode420Stream8" when !AdaptiveEntropy => [WarmJpegCase.Encode420Stream8],
            "Encode420Stream16" when !AdaptiveEntropy => [WarmJpegCase.Encode420Stream16],
            _ => throw new ArgumentException("Choose one JPEG case: Encode8, Encode16, Encode420_8, Encode422_8, Encode422_16, Encode420Restart8 (adaptive-v1), Encode420_16, Encode420Stream8 or Encode420Stream16 (fixed-geometric-v1 policy).")
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

    // Preparation is explicit and untimed. Benchmark launches validate the
    // selected bitstreams again without repeating quality enumeration.
    public static void Prepare(int cpu, int nativeQscale = 5, bool subsampled = false, bool horizontal = false, int depth = 8, bool forwardOnly = false, bool restartRows = false)
    {
        if (!OperatingSystem.IsLinux() || cpu is < 0 or > 62 || nativeQscale is < 1 or > 31)
            throw new ArgumentException("JPEG preparation requires Linux, one allowed CPU and qscale 1..31.");
        if (subsampled && horizontal) throw new ArgumentException("Choose one JPEG subsampling layout.");
        if (depth is not (8 or 16))
            throw new ArgumentException("JPEG source depth must be 8 or16.");
        if (forwardOnly && (!subsampled || horizontal || Environment.GetEnvironmentVariable("LOKAD_IMAGE_JPEG_ENTROPY") != "fixed-geometric-v1"))
            throw new ArgumentException("Forward-only preparation requires RGBA8/16 4:2:0 and fixed-geometric-v1.");
        if (restartRows && (depth != 8 || !subsampled || horizontal || forwardOnly || !AdaptiveEntropy))
            throw new ArgumentException("Row-restart preparation requires adaptive RGBA8 4:2:0.");
        var fixedAnchor = subsampled || horizontal || depth == 16 || AdaptiveEntropy;
        var sampling = horizontal ? "422" : subsampled ? "420" : "444";
        if (fixedAnchor && nativeQscale != 5) throw new ArgumentException("The fixed JPEG anchor requires native qscale 5.");
        using var process = Process.GetCurrentProcess(); var oldAffinity = process.ProcessorAffinity;
        process.ProcessorAffinity = new IntPtr(1L << cpu);
        try
        {
            using var state = new JpegBenchmarkCase();
            var pixels = HorizontalFlipInput.Create(depth);
            // Reference is the specification's target, not another source image.
            var reference = JpegQualityCorpus.Reference(pixels, depth, Background);
            using var source = JpegBenchmarkCase.Buffer(pixels, 513, 259, depth);
            File.WriteAllBytes(state.Input, pixels);
            var target = ProfileFor(subsampled, horizontal, depth);
            using var native = new NativeWarmReference(state, depth, cpu, restartRows ? "jpeg-restart-encode" : "jpeg-encode", expectedRaw: NormalizedInput(state.Root, pixels, reference, depth), nativeQuality: nativeQscale, jpegSampling: sampling);
            var nativeBytes = File.ReadAllBytes(state.NativeOutput);
            var nativeSample = EncoderQualityCalibration.Validate(nativeBytes, reference, target, nativeQscale, state.Ffmpeg);
            var files = new Dictionary<int, byte[]>(); var order = new List<int>();
            QualitySample Sample(int quality)
            {
                using var output = new MemoryStream();
                using var wrapper = forwardOnly ? new ForwardOnlyOutput(output) : null;
                ImageCodec.EncodeJpeg(source, (Stream?)wrapper ?? output, new JpegEncodeOptions { Quality = quality, Sampling = horizontal ? JpegSampling.YCbCr422 : subsampled ? JpegSampling.YCbCr420 : JpegSampling.YCbCr444, BackgroundRgb = Background, RestartInterval = restartRows ? 33 : 0 });
                var bytes = output.ToArray(); files.Add(quality, bytes); order.Add(quality);
                return EncoderQualityCalibration.Validate(bytes, reference, target, quality, state.Ffmpeg);
            }
            var owned = fixedAnchor ? new[] { Sample(horizontal ? 65 : 69) } : EncoderQualityCalibration.SampleOwned([nativeSample], Sample);
            var match = EncoderQualityCalibration.Match(nativeSample, owned);
            if (restartRows)
            {
                ValidateRestartPacket(nativeBytes); ValidateRestartPacket(files[match.ClosestOwned.Setting]);
                ValidateRestartNegativeControls(files[match.ClosestOwned.Setting]);
                ValidateRestartPixels(state.Root, match.ClosestOwned, nativeSample);
            }
            native.Check();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativeBytes)
                || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Quality preparation changed source/bitstream.");
            native.Stop();
            var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "jpeg-match-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var nativeFile = Path.Combine(directory, "native.jpg"); var managedFile = Path.Combine(directory, "managed.jpg");
            File.WriteAllBytes(nativeFile, nativeBytes); File.WriteAllBytes(managedFile, files[match.ClosestOwned.Setting]);
            var record = new WarmJpegPreparation(1, "Lokad.Image", DateTime.UtcNow.ToString("O"), RecipeFor(subsampled, horizontal, depth, forwardOnly, restartRows),
                JpegBenchmarkCase.Hash(pixels), JpegBenchmarkCase.Hash(reference), nativeQscale, Background,
                Path.GetRelativePath(state.Root, nativeFile), Path.GetRelativePath(state.Root, managedFile), match, owned, order.ToArray(), SourceIdentity(state.Root),
                JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), JpegBenchmarkCase.HashFile(typeof(WarmJpeg).Assembly.Location),
                JpegBenchmarkCase.HashFile(state.Ffmpeg), native.BuildIdentity, native.Ready,
                sampling, restartRows ? FixedRestartPolicy : depth == 16 ? (horizontal ? Fixed42216Policy : subsampled ? Fixed42016Policy : Fixed16Policy)
                    : horizontal ? Fixed422Policy : subsampled ? Fixed420Policy
                    : AdaptiveEntropy ? Fixed444Policy : EncoderQualityCalibration.OwnedQualityPolicy, depth);
            var path = Path.Combine(state.Root, "artifacts", "warm-reference", MatchFile(subsampled, horizontal, depth, forwardOnly, restartRows));
            if ((AdaptiveEntropy || forwardOnly) && File.Exists(path)) throw new InvalidOperationException("Preserve the existing preparation before replacing it.");
            File.WriteAllText(path, JsonSerializer.Serialize(record, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine($"Prepared one warm JPEG pair: native qscale {nativeQscale}, owned quality {match.ClosestOwned.Setting}, gap {match.PsnrGapDb:F4} dB, matched={match.Matched}; {owned.Length} owned settings, one source raster. {path}");
        }
        finally { process.ProcessorAffinity = oldAffinity; }
    }

    private static string? NormalizedInput(string root, byte[] pixels, byte[] reference, int depth)
    {
        if (depth != 16) return null;
        var directory = Path.Combine(root, "artifacts", "warm-reference", "jpeg16-input");
        Directory.CreateDirectory(directory);
        Save("source.rgba", pixels); Save("normalized.rgba", reference);
        return Path.Combine(directory, "normalized.rgba");
        void Save(string name, byte[] bytes)
        {
            var path = Path.Combine(directory, name);
            if (File.Exists(path))
            {
                if (!File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes))
                    throw new InvalidOperationException("Frozen JPEG16 source/reference changed.");
            }
            else File.WriteAllBytes(path, bytes);
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm native JPEG requires Linux.");
        if (ForwardOnly && Environment.GetEnvironmentVariable("LOKAD_IMAGE_JPEG_ENTROPY") != "fixed-geometric-v1") throw new InvalidOperationException("Forward-only output retains the fixed geometric AC Huffman path.");
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
            var path = Path.Combine(state.Root, "artifacts", "warm-reference", MatchFile(Subsampled, Horizontal, Depth, ForwardOnly, Restart));
            prepared = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllText(path), EncoderQualityCalibration.Json)
                ?? throw new InvalidOperationException("Missing warm JPEG preparation; run --prepare-warm-jpeg first.");
            if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.Recipe != Recipe || prepared.BackgroundRgb != Background || prepared.SourceDepth != Depth
                || !prepared.Match.Matched || prepared.Match.PsnrGapDb is not (>= 0 and <= EncoderQualityCalibration.MaximumPsnrGapDb)
                || (!FixedAnchor && prepared.OwnedSettings.Length is < 5 or > EncoderQualityCalibration.MaximumOwnedSettings)
                || (FixedAnchor && (prepared.Sampling != NativeSampling || prepared.QualityPolicy != FixedPolicy
                    || prepared.NativeQuality != 5 || prepared.OwnedSettings.Length != 1
                    || prepared.OwnedSettings[0].Setting != FixedQuality || prepared.Match.ClosestOwned.Setting != FixedQuality
                    || prepared.Match.Native.Setting != 5 || prepared.OwnedEvaluationOrder.Length != 1 || prepared.OwnedEvaluationOrder[0] != FixedQuality))
                || prepared.OwnedEvaluationOrder.Length != prepared.OwnedSettings.Length
                || prepared.OwnedEvaluationOrder.Distinct().Count() != prepared.OwnedSettings.Length)
                throw new InvalidOperationException("Warm JPEG quality preparation is unmatched or outside its bounded contract.");
            sourceIdentity = SourceIdentity(state.Root);
            pixels = HorizontalFlipInput.Create(Depth); reference = JpegQualityCorpus.Reference(pixels, Depth, Background);
            if (JpegBenchmarkCase.Hash(pixels) != prepared.RasterSha256 || JpegBenchmarkCase.Hash(reference) != prepared.ReferenceSha256
                || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256)
                throw new InvalidOperationException("Warm JPEG input/reference/oracle identity changed; prepare only this pair again.");
            options = new() { Quality = prepared.Match.ClosestOwned.Setting, Sampling = Horizontal ? JpegSampling.YCbCr422 : Subsampled ? JpegSampling.YCbCr420 : JpegSampling.YCbCr444, BackgroundRgb = Background, RestartInterval = Restart ? 33 : 0 };
            source = JpegBenchmarkCase.Buffer(pixels, 513, 259, Depth); File.WriteAllBytes(state.Input, pixels);
            native = new NativeWarmReference(state, Depth, cpu, Restart ? "jpeg-restart-encode" : "jpeg-encode", expectedRaw: NormalizedInput(state.Root, pixels, reference, Depth), nativeQuality: prepared.NativeQuality, jpegSampling: NativeSampling);
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
        EncodeManaged(first); EncodeManaged(second);
        if (ReferenceEquals(first.GetBuffer(), second.GetBuffer()) || !first.ToArray().AsSpan().SequenceEqual(encoded)
            || !second.ToArray().AsSpan().SequenceEqual(encoded) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("Warm JPEG output is not independent/deterministic or source changed.");
    }

    private void ValidateQuality()
    {
        if (Restart) { ValidateRestartPacket(encoded); ValidateRestartPacket(File.ReadAllBytes(state.NativeOutput)); }
        managedQuality = EncoderQualityCalibration.Validate(encoded, reference, TargetProfile, options.Quality, state.Ffmpeg);
        nativeQuality = EncoderQualityCalibration.Validate(File.ReadAllBytes(state.NativeOutput), reference, TargetProfile, prepared.NativeQuality, state.Ffmpeg);
        if (managedQuality != prepared.Match.ClosestOwned || nativeQuality != prepared.Match.Native
            || !EncoderQualityCalibration.Match(nativeQuality, [managedQuality]).Matched)
            throw new InvalidOperationException("Warm JPEG source/decoder quality changed; prepare this pair again.");
        if (Restart) ValidateRestartPixels(state.Root, managedQuality, nativeQuality);
    }

    private static (int Dri, int FirstRestart) ValidateRestartPacket(byte[] bytes)
    {
        // Original bounded T.81 B.2 framing check, outside timed encoding.
        if (bytes.Length < 4 || bytes[0] != 255 || bytes[1] != 216) throw new InvalidOperationException("Restart JPEG SOI missing.");
        var p = 2; var scan = false; var dri = -1; var first = -1; var count = 0;
        while (p < bytes.Length)
        {
            if (scan) { while (p < bytes.Length && bytes[p] != 255) p++; }
            if (p >= bytes.Length || bytes[p++] != 255) break;
            while (p < bytes.Length && bytes[p] == 255) p++;
            if (p >= bytes.Length) break;
            var marker = bytes[p++];
            if (scan && marker == 0) continue;
            if (scan && marker is >= 0xD0 and <= 0xD7)
            {
                if (marker != 0xD0 + count % 8) throw new InvalidOperationException("Restart sequence differs.");
                if (first < 0) first = p - 1;
                count++; continue;
            }
            if (marker == 0xD9 && scan && p == bytes.Length && dri >= 0 && count == 16) return (dri, first);
            if (scan || p + 2 > bytes.Length) break;
            var length = bytes[p] * 256 + bytes[p + 1];
            if (length < 2 || length > bytes.Length - p) break;
            if (marker == 0xDD)
            {
                if (dri >= 0 || length != 4 || bytes[p + 2] != 0 || bytes[p + 3] != 33) break;
                dri = p + 2;
            }
            if (marker == 0xDA) scan = true;
            p += length;
        }
        throw new InvalidOperationException("JPEG must contain DRI33 and sixteen sequenced row restart markers.");
    }

    private static void ValidateRestartNegativeControls(byte[] bytes)
    {
        var offsets = ValidateRestartPacket(bytes);
        foreach (var offset in new[] { offsets.Dri + 1, offsets.FirstRestart })
        {
            var broken = bytes.ToArray(); broken[offset] ^= 1;
            try { ValidateRestartPacket(broken); }
            catch (InvalidOperationException) { continue; }
            throw new InvalidOperationException("Corrupted restart framing was accepted.");
        }
    }

    private static void ValidateRestartPixels(string root, QualitySample managed, QualitySample native)
    {
        var anchor = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllText(Path.Combine(root,
            "artifacts", "warm-reference", "jpeg420-adaptive-match.json")), EncoderQualityCalibration.Json)
            ?? throw new InvalidOperationException("Original 420 anchor missing.");
        if (managed.PublicDecodedSha256 != anchor.Match.ClosestOwned.PublicDecodedSha256
            || managed.NativeDecodedSha256 != anchor.Match.ClosestOwned.NativeDecodedSha256
            || native.PublicDecodedSha256 != anchor.Match.Native.PublicDecodedSha256
            || native.NativeDecodedSha256 != anchor.Match.Native.NativeDecodedSha256)
            throw new InvalidOperationException("Restart coding changed per-encoder decoded pixels.");
    }

    [Benchmark]
    public int Managed()
    {
        using var output = new MemoryStream(); EncodeManaged(output);
        return checked((int)output.Length) ^ output.GetBuffer()[checked((int)output.Length) / 2];
    }

    private void EncodeManaged(MemoryStream output)
    {
        if (ForwardOnly)
        {
            using var stream = new ForwardOnlyOutput(output);
            ImageCodec.EncodeJpeg(source, stream, options);
        }
        else ImageCodec.EncodeJpeg(source, output, options);
    }

    // Own the wrapper per operation; the caller retains the backing storage.
    // Length/Position/Seek are unavailable, as with a forward-only destination.
    private sealed class ForwardOnlyOutput(MemoryStream output) : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => output.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Write(ReadOnlySpan<byte> buffer) => output.Write(buffer);
        public override void Write(byte[] buffer, int offset, int count) => output.Write(buffer, offset, count);
        public override void WriteByte(byte value) => output.WriteByte(value);
        public override void Flush() => output.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
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
                width = source.Width, height = source.Height, depth = Depth, encodedDepth = 8, cpu, managedAffinityCount = 1,
                processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(),
                managedIntrinsics = new { avx = System.Runtime.Intrinsics.X86.Avx.IsSupported, avx512F = System.Runtime.Intrinsics.X86.Avx512F.IsSupported },
                runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                jitPolicy = "Tiered compilation disabled; fully optimized JIT before measurement", sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(pixels), rasterSha256 = JpegBenchmarkCase.Hash(pixels), referenceSha256 = JpegBenchmarkCase.Hash(reference),
                managedOutputBytes = encoded.Length, nativeOutputBytes = nativeQuality.EncodedBytes,
                managedEncodedSha256 = JpegBenchmarkCase.Hash(encoded), nativeOutputSha256 = JpegBenchmarkCase.HashFile(state.NativeOutput),
                recipe = Recipe, sampling = DisplaySampling, restartInterval = Restart ? 33 : 0, restartMarkers = Restart ? 16 : 0,
                serialRestartFramingValidated = Restart, qualityPolicy = prepared.QualityPolicy,
                outputBoundary = ForwardOnly ? "Fresh forward-only Stream wrapper over fresh owned MemoryStream storage; wrapper creation/disposal and storage growth included; no seek/length/position access." : "Fresh MemoryStream",
                backgroundRgb = Background, managedHuffman = AdaptiveEntropy ? "adaptive-v1" : "fixed-geometric-v1", privateEntropyWorkspaceReused = AdaptiveEntropy, qualityMatched = true, fullSourceNormalizationTimed = Source16, normalizedReferenceExact = true, alphaExact = true, decoderAgreementWithinBounds = true,
                psnrGapDb = prepared.Match.PsnrGapDb, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb,
                ownedEnumeratedSettings = prepared.OwnedSettings.Length, ownedEvaluationOrder = prepared.OwnedEvaluationOrder,
                ownedSettings = prepared.OwnedSettings, managedQuality, nativeQuality,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpeg).Assembly.Location), nativeBuild = native.BuildIdentity,
                nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"Resident original RGBA{Depth} to complete independent JPEG bytes: explicit single-round alpha flattening and normalization on 0x204060, full-range BT.601 conversion, {DisplaySampling} encode, output acquisition/release. Native codec/conversion/scratch/frame/packet wrappers reused, but pixels are flattened/converted and encoded every operation. Fresh managed MemoryStream creation, growth, encode and disposal included{(ForwardOnly ? ", plus fresh forward-only Stream wrapper creation/disposal" : "")}; no redundant ToArray. Native optimal Huffman and managed {(AdaptiveEntropy ? "adaptive Huffman with histogram/cache work and private workspace reuse" : "fixed geometric AC Huffman")} recorded. Quality matched by frozen <=0.25 dB RGB source-error band; fresh geometry/decoder/alpha/source/ownership/bitstream checks before/after. Startup, IPC, I/O, fixture generation, quality enumeration and checks excluded."
            }, new JsonSerializerOptions(EncoderQualityCalibration.Json) { PropertyNamingPolicy = null }) + "\n");
            Console.WriteLine("Validated warm JPEG pair: " + path);
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
