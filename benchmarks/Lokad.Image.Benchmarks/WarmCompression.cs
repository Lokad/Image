using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmCompressionCase { PhotographLossless8, DiagramLossless8, PhotographLossy8, DiagramLossy8, DiagramLossyTrellis8 }

/// <summary>Fixed photographic/text controls, plus the distinct effort6 trellis algorithm.</summary>
[MemoryDiagnoser]
public class WarmCompression
{
    [ParamsSource(nameof(Cases))] public WarmCompressionCase Case { get; set; }
    public static IEnumerable<WarmCompressionCase> Cases => SelectedCases();
    private bool Lossless => Case is WarmCompressionCase.PhotographLossless8 or WarmCompressionCase.DiagramLossless8;
    private int Effort => Case == WarmCompressionCase.DiagramLossyTrellis8 ? 6 : 4;
    private string InputName => Case is WarmCompressionCase.PhotographLossless8 or WarmCompressionCase.PhotographLossy8 ? "Photograph8" : "Diagram8";
    public static WarmCompressionCase[] SelectedCases()
    {
        var selected = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        if (selected == "All") return Enum.GetValues<WarmCompressionCase>();
        if (Enum.TryParse<WarmCompressionCase>(selected, out var value) && Enum.IsDefined(value)) return [value];
        throw new ArgumentException("Choose one named compression control or All for the five named controls.");
    }

    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], packet = [];
    private WebpEncodeOptions options = null!;
    private int packetBytes;
    private string packetSha256 = "";
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, count;

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
            // Fixture pixels and independently frozen native packets qualify the
            // operation directly. Historical quality-search catalogs are evidence
            // of their own producers, not current build-dependent inputs.
            var name = InputName == "Photograph8" ? "photograph" : "diagram";
            var expected = Lossless
                ? OriginalVp8lControls.Cases().Single(v => (string)v[0] == name && (int)v[1] == 4)
                : OriginalVp8Controls.Cases().Single(v => (string)v[0] == name && (int)v[1] == 8 && (int)v[2] == Effort && (int)v[3] == 75);
            packetBytes = (int)expected[Lossless ? 2 : 4]; packetSha256 = (string)expected[Lossless ? 3 : 5];
            options = new() { Lossless = Lossless, Quality = Lossless ? 85 : 75, Effort = Effort };
            using var fixtures = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "benchmarks/Fixtures/Compression/manifest.json")));
            var input = fixtures.RootElement.GetProperty("inputs").EnumerateArray().Single(v => v.GetProperty("name").GetString() == InputName);
            var fixture = Path.Combine(state.Root, "benchmarks/Fixtures/Compression", input.GetProperty("file").GetString()!);
            if (JpegBenchmarkCase.HashFile(fixture) != input.GetProperty("pngSha256").GetString()) throw new InvalidOperationException("Frozen compression PNG differs.");
            source = ImageCodec.Decode(File.ReadAllBytes(fixture)); pixels = OriginalPixels.Bytes(source);
            if ((source.Width, source.Height, source.BitDepth) != (513, 259, 8) || JpegBenchmarkCase.Hash(pixels) != input.GetProperty("rgbaSha256").GetString())
                throw new InvalidOperationException("Frozen compression pixels differ.");
            for (var i = 3; i < pixels.Length; i += 4) if (pixels[i] != 255) throw new InvalidOperationException("Expected an opaque compression control.");
            File.WriteAllBytes(state.Input, pixels);
            using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json")));
            sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            native = new NativeWarmReference(state, 8, cpu, Lossless ? "webp-lossless-encode" : "webp-opaque-encode", nativeQuality: 75, vp8Trellis: Effort == 6);
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
        if (encoded.Length != packetBytes || baseline.Length != packetBytes
            || JpegBenchmarkCase.Hash(encoded) != packetSha256 || JpegBenchmarkCase.Hash(baseline) != packetSha256)
            throw new InvalidOperationException("Frozen complete compression packet differs.");
        var nativePixels = Lossless ? pixels : OracleProcess.Run(state.Ffmpeg, DecodeArguments(false), baseline);
        foreach (var bytes in new[] { encoded, baseline })
        {
            var info = ImageCodec.Identify(bytes);
            using var decoded = ImageCodec.Decode(bytes);
            if ((info.Format, info.Width, info.Height, info.BitDepth) != (ImageFormat.WebP, 513, 259, 8)
                || !OriginalPixels.Bytes(decoded).AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Compression syntax/reconstructed pixels differ.");
            foreach (var scalar in new[] { false, true })
                if (!OracleProcess.Run(state.Ffmpeg, DecodeArguments(scalar), bytes).AsSpan().SequenceEqual(nativePixels))
                    throw new InvalidOperationException("Normal/scalar native compression pixels differ.");
        }
        packet = encoded;
    }

    private string[] DecodeArguments(bool scalar)
    {
        if (!Lossless)
        {
            var normalized = WebpOracle.DecodeArguments(513, 259, false).ToList();
            if (!scalar) normalized.RemoveRange(normalized.IndexOf("-cpuflags"), 2);
            return normalized.ToArray();
        }
        var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-noautorotate" };
        if (scalar) arguments.AddRange(["-cpuflags", "0"]);
        arguments.AddRange(["-f", "image2pipe", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-thread_type:v", "0", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"]);
        return arguments.ToArray();
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
                lossless = Lossless, effort = Effort, managedQuality = options.Quality, nativeQuality = 75,
                baselineBackend = Lossless ? "libwebp exact=1 (FFmpeg encoder backend)" : "FFmpeg libwebp",
                inputSha256 = JpegBenchmarkCase.Hash(pixels), rasterSha256 = JpegBenchmarkCase.Hash(pixels),
                managedOutputBytes = packet.Length, managedOutputSha256 = JpegBenchmarkCase.Hash(packet),
                nativeOutputBytes = packetBytes, nativeOutputSha256 = packetSha256, completePacketExact = true,
                qualityMatched = true, qualityGate = "Frozen complete packet equality, including reconstructed pixels; no equal-quality-number inference.",
                allRgbaSamplesExact = Lossless, allReconstructedSamplesExact = true, alphaExact = true, hiddenRgbExact = Lossless, nativeCliNormalScalarPixelsExact = true,
                decoderAgreementWithinBounds = true, bitstreamSyntaxVerified = true, sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmCompression).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"One frozen resident 513x259 opaque RGBA8 photograph or text/diagram to complete owned WebP. Public managed EncodeWebp/admission, source processing, fresh MemoryStream growth/disposal included, no redundant ToArray. Native normalization/repacking, pixel import/encoding and independent packet ownership included. Wrappers/private scratch reused; every pixel reprocessed, no packet cache. Font/image preparation, I/O, IPC, validation and startup excluded. One CPU; effort{Effort}, frozen reference quality75. Complete packets exact; VP8L source pixels exact, VP8 identical reconstructed pixels, exact alpha. Quality numbers are not the matching criterion."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
