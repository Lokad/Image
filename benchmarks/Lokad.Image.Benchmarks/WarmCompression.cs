using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmCompressionCase { PhotographLossless8, DiagramLossless8, PhotographLossy8, DiagramLossy8 }

internal sealed record CompressionSizes(int ManagedBytes, int NativeBytes, string ManagedSha256, string NativeSha256);
internal sealed record CompressionLossy(QualitySample[] OwnedSettings, int[] OwnedEvaluationOrder, QualityMatch Match);
internal sealed record CompressionControl(string Name, string SourceSha256, CompressionSizes Lossless, CompressionLossy Lossy);
internal sealed record CompressionQuality(string LibrarySha256, CompressionControl[] Cases);

/// <summary>Four fixed complete encode operations on the required photographic/text controls.</summary>
[MemoryDiagnoser]
public class WarmCompression
{
    [ParamsSource(nameof(Cases))] public WarmCompressionCase Case { get; set; }
    public static IEnumerable<WarmCompressionCase> Cases => SelectedCases();
    private bool Lossless => Case is WarmCompressionCase.PhotographLossless8 or WarmCompressionCase.DiagramLossless8;
    private string InputName => Case is WarmCompressionCase.PhotographLossless8 or WarmCompressionCase.PhotographLossy8 ? "Photograph8" : "Diagram8";
    public static WarmCompressionCase[] SelectedCases()
    {
        var selected = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        if (selected == "All") return Enum.GetValues<WarmCompressionCase>();
        if (Enum.TryParse<WarmCompressionCase>(selected, out var value) && Enum.IsDefined(value)) return [value];
        throw new ArgumentException("Choose one named compression control or All for the four fixed cases.");
    }

    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], packet = [];
    private WebpEncodeOptions options = null!;
    private CompressionControl control = null!;
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, count;
    private static readonly EncoderQualityProfile Profile = new(513, 8, QualityEncoder.Vp8, QualityPattern.Asymmetric, QualityAlpha.Opaque);

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        count = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0" || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Compression measurements require one CPU, optimized JIT and nonconcurrent workstation GC.");
        state = new JpegBenchmarkCase(".webp");
        try
        {
            var catalog = JsonSerializer.Deserialize<CompressionQuality>(File.ReadAllBytes(Path.Combine(state.Root, "benchmarks/warm-webp-compression-quality-evidence.json")), EncoderQualityCalibration.Json)
                ?? throw new InvalidOperationException("Missing compression quality controls.");
            if (catalog.LibrarySha256 != JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location)) throw new InvalidOperationException("Refresh affected quality controls for changed production.");
            control = catalog.Cases.Single(v => v.Name == InputName);
            options = new() { Lossless = Lossless, Quality = Lossless ? 85 : control.Lossy.Match.ClosestOwned.Setting, Effort = 4 };
            var fixture = Path.Combine(state.Root, "benchmarks/Fixtures/Compression", InputName == "Photograph8" ? "photograph.png" : "diagram.png");
            source = ImageCodec.Decode(File.ReadAllBytes(fixture)); pixels = OriginalPixels.Bytes(source);
            if ((source.Width, source.Height, source.BitDepth) != (513, 259, 8) || JpegBenchmarkCase.Hash(pixels) != control.SourceSha256)
                throw new InvalidOperationException("Frozen compression pixels differ.");
            File.WriteAllBytes(state.Input, pixels);
            using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json")));
            sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            native = new NativeWarmReference(state, 8, cpu, Lossless ? "webp-lossless-encode" : "webp-opaque-encode", nativeQuality: 75);
            Validate(); before = native.Measure(smoke, sampleCount: count); ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void Validate()
    {
        using var a = new MemoryStream(); using var b = new MemoryStream();
        ImageCodec.EncodeWebp(source, a, options); ImageCodec.EncodeWebp(source, b, options);
        var encoded = a.ToArray();
        if (ReferenceEquals(a.GetBuffer(), b.GetBuffer()) || !encoded.AsSpan().SequenceEqual(b.ToArray()) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels)
            || packet.Length != 0 && !packet.AsSpan().SequenceEqual(encoded)) throw new InvalidOperationException("Source, deterministic bytes or output ownership differ.");
        var baseline = File.ReadAllBytes(state.NativeOutput);
        if (Lossless)
        {
            if (encoded.Length != control.Lossless.ManagedBytes || baseline.Length != control.Lossless.NativeBytes
                || JpegBenchmarkCase.Hash(encoded) != control.Lossless.ManagedSha256 || JpegBenchmarkCase.Hash(baseline) != control.Lossless.NativeSha256)
                throw new InvalidOperationException("Frozen lossless packets differ.");
            foreach (var bytes in new[] { encoded, baseline })
            {
                var info = ImageCodec.Identify(bytes);
                using var decoded = ImageCodec.Decode(bytes);
                if ((info.Format, info.Width, info.Height, info.BitDepth) != (ImageFormat.WebP, 513, 259, 8) || !OriginalPixels.Bytes(decoded).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Lossless syntax/pixels differ.");
                foreach (var scalar in new[] { false, true })
                {
                    var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-noautorotate" };
                    if (scalar) arguments.AddRange(["-cpuflags", "0"]);
                    arguments.AddRange(["-f", "image2pipe", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-thread_type:v", "0", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"]);
                    if (!OracleProcess.Run(state.Ffmpeg, arguments, bytes).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Normal/scalar native lossless pixels differ.");
                }
            }
        }
        else
        {
            var managedQuality = EncoderQualityCalibration.Validate(encoded, pixels, Profile, options.Quality, state.Ffmpeg);
            var nativeQuality = EncoderQualityCalibration.Validate(baseline, pixels, Profile, 75, state.Ffmpeg);
            if (managedQuality != control.Lossy.Match.ClosestOwned || nativeQuality != control.Lossy.Match.Native || !EncoderQualityCalibration.Match(nativeQuality, [managedQuality]).Matched)
                throw new InvalidOperationException("Frozen quality, alpha or decoder gates differ.");
        }
        packet = encoded;
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
            Validate(); var after = native.Measure(smoke, before[0].Operations, count); native.Check(); Validate(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", nameof(WarmCompression) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmCompression), caseId = Case.ToString(), inputName = InputName, smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), sourceIdentity,
                width = 513, height = 259, depth = 8, encodedDepth = 8, cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount,
                serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                lossless = Lossless, effort = 4, managedQuality = options.Quality, nativeQuality = 75,
                baselineBackend = Lossless ? "libwebp exact=1 (FFmpeg encoder backend)" : "FFmpeg libwebp",
                inputSha256 = JpegBenchmarkCase.Hash(pixels), rasterSha256 = JpegBenchmarkCase.Hash(pixels),
                managedOutputBytes = packet.Length, managedOutputSha256 = JpegBenchmarkCase.Hash(packet),
                nativeOutputBytes = Lossless ? control.Lossless.NativeBytes : control.Lossy.Match.Native.EncodedBytes,
                nativeOutputSha256 = Lossless ? control.Lossless.NativeSha256 : control.Lossy.Match.Native.EncodedSha256,
                qualityMatched = true, psnrGapDb = Lossless ? 0 : control.Lossy.Match.PsnrGapDb, maximumPsnrGapDb = EncoderQualityCalibration.MaximumPsnrGapDb,
                allRgbaSamplesExact = Lossless, alphaExact = true, hiddenRgbExact = Lossless, nativeCliNormalScalarPixelsExact = Lossless,
                decoderAgreementWithinBounds = true, bitstreamSyntaxVerified = true, sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmCompression).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "One frozen resident 513x259 opaque RGBA8 photograph or text/diagram to complete owned WebP. Public managed EncodeWebp/admission, source processing, fresh MemoryStream growth/disposal included, no redundant ToArray. Native normalization/repacking, pixel import/encoding and independent packet ownership included. Wrappers/private scratch reused; every pixel reprocessed, no packet cache. Font/image preparation, I/O, IPC, validation and startup excluded. One CPU; effort4. VP8L exact pixels; VP8 frozen matched source quality and decoder/alpha gates."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
