using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmWebpDecodeCase { Lossless8 }

[MemoryDiagnoser]
public class WarmWebpDecode
{
    private const string Recipe = "HorizontalFlipInput.Create(8): existing original 513x259 asymmetric RGBA8, phase zero, exact alpha and zero-alpha hidden RGB. One owned VP8L input, effort 4; no transforms, color cache or spatial entropy groups. No new source raster or quality enumeration.";
    public static IEnumerable<WarmWebpDecodeCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmWebpDecodeCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] encoded = [], pixels = [];
    private NativeWarmSample[] before = [];
    private WarmWebpDecodePreparation prepared = null!;
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount;

    public static WarmWebpDecodeCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        if (selection is not (null or "Lossless8")) throw new ArgumentException("Choose the one warm WebP decode case: Lossless8.");
        return [WarmWebpDecodeCase.Lossless8];
    }

    public static void Prepare(int cpu)
    {
        if (!OperatingSystem.IsLinux() || cpu is < 0 or > 62) throw new ArgumentException("VP8L preparation requires Linux and one allowed CPU.");
        using var process = Process.GetCurrentProcess(); var previousAffinity = process.ProcessorAffinity;
        process.ProcessorAffinity = new IntPtr(1L << cpu);
        try
        {
            using var state = new JpegBenchmarkCase();
            var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "webp-decode");
            var manifest = Path.Combine(directory, "preparation.json");
            if (File.Exists(manifest)) throw new InvalidOperationException("Reuse the frozen VP8L preparation; do not regenerate it for benchmark launches.");
            var pixels = HorizontalFlipInput.Create(8);
            using var source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8);
            using var output = new MemoryStream();
            ImageCodec.EncodeWebp(source, output, new WebpEncodeOptions { Lossless = true, Effort = 4 });
            var encoded = output.ToArray(); ValidateInput(encoded);
            using var decoded = ImageCodec.Decode(encoded); ValidatePixels(decoded, pixels);
            var oracle = OracleProcess.Run(state.Ffmpeg, DecodeArguments(), encoded);
            if (!oracle.AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Frozen VP8L input differs from original pixels through the FFmpeg CLI.");
            Directory.CreateDirectory(directory);
            var input = Path.Combine(directory, "input.webp"); var raster = Path.Combine(directory, "source.rgba");
            File.WriteAllBytes(input, encoded); File.WriteAllBytes(raster, pixels);
            var record = new WarmWebpDecodePreparation(1, "Lokad.Image", Recipe,
                Path.GetRelativePath(state.Root, input), Path.GetRelativePath(state.Root, raster), encoded.Length,
                JpegBenchmarkCase.Hash(encoded), JpegBenchmarkCase.Hash(pixels),
                JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), DateTime.UtcNow.ToString("O"));
            File.WriteAllText(manifest, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine($"Frozen one original VP8L decode input: {encoded.Length} bytes; exact RGBA through both decoders. No timing/quality enumeration.");
        }
        finally { process.ProcessorAffinity = previousAffinity; }
    }

    private static string[] DecodeArguments() => ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-thread_type:v", "0",
        "-filter_threads", "1", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"];

    private static void ValidateInput(byte[] value)
    {
        if (value.Length < 26 || !value.AsSpan(8, 8).SequenceEqual("WEBPVP8L"u8) || value[20] != 0x2f || (value[25] & 7) != 0)
            throw new InvalidOperationException("Expected the frozen simple VP8L profile without transforms/cache/spatial groups.");
        var info = ImageCodec.Identify(value);
        if ((info.Format, info.Width, info.Height, info.BitDepth, info.Orientation) != (ImageFormat.WebP, 513, 259, 8, ImageOrientation.Normal))
            throw new InvalidOperationException("VP8L input geometry/depth/orientation differs.");
    }
    private static void ValidatePixels(ImageBuffer value, byte[] pixels)
    {
        if ((value.Width, value.Height, value.BitDepth, value.Orientation) != (513, 259, 8, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(value).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("VP8L decoded RGBA differs, including alpha and zero-alpha hidden RGB.");
    }

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm native VP8L decode requires Linux.");
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1
            || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("VP8L worker requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp", "benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            prepared = JsonSerializer.Deserialize<WarmWebpDecodePreparation>(File.ReadAllText(Path.Combine(state.Root, "artifacts", "warm-reference", "webp-decode", "preparation.json")))
                ?? throw new InvalidOperationException("Prepare one VP8L input explicitly before the warm run.");
            pixels = HorizontalFlipInput.Create(8); encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.InputFile));
            if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.Recipe != Recipe
                || prepared.InputBytes != encoded.Length || prepared.InputSha256 != JpegBenchmarkCase.Hash(encoded)
                || prepared.RasterSha256 != JpegBenchmarkCase.Hash(pixels)
                || !File.ReadAllBytes(Path.Combine(state.Root, prepared.RasterFile)).AsSpan().SequenceEqual(pixels))
                throw new InvalidOperationException("Frozen VP8L preparation identity differs.");
            ValidateInput(encoded); File.WriteAllBytes(state.Input, encoded);
            native = new NativeWarmReference(state, 8, cpu, "webp-decode"); ValidateNative();
            if (!OracleProcess.Run(state.Ffmpeg, DecodeArguments(), encoded).AsSpan().SequenceEqual(pixels))
                throw new InvalidOperationException("Native VP8L output differs from the pinned public CLI.");
            ValidateManaged(); before = native.Measure(smoke, sampleCount: sampleCount); ready = true;
        }
        catch { native?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateNative()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Native VP8L RGBA differs from original pixels.");
    }
    private void ValidateManaged()
    {
        using var first = ImageCodec.Decode(encoded); using var second = ImageCodec.Decode(encoded);
        ValidatePixels(first, pixels); ValidatePixels(second, pixels); first.GetRow8(0)[0] ^= 255; ValidatePixels(second, pixels);
        if (!File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(encoded)) throw new InvalidOperationException("Resident VP8L source changed.");
    }

    [Benchmark]
    public int Managed()
    {
        using var result = ImageCodec.Decode(encoded);
        return result.Stride * result.Height ^ result.GetRow8(0)[513 * 2];
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged(); var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); ValidateNative(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmWebpDecode) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmWebpDecode), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = 8,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                sourceIdentity, preparation = prepared, inputOrigin = "prepared-owned-lossless", codec = "VP8L",
                inputBytes = encoded.Length, inputSha256 = JpegBenchmarkCase.Hash(encoded), rasterSha256 = JpegBenchmarkCase.Hash(pixels),
                managedOutputBytes = pixels.Length, nativeOutputBytes = pixels.Length, managedOutputSha256 = JpegBenchmarkCase.Hash(pixels), nativeOutputSha256 = JpegBenchmarkCase.HashFile(state.NativeOutput),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, nativeCliPixelsExact = true,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmWebpDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "Identical resident frozen VP8L input to independent owned RGBA8. Complete public managed Decode, output acquisition/disposal; native software webp decode, state reset, ARGB-to-RGBA conversion, owned output allocation/release. Native codec/conversion contexts and packet/frame wrappers reused; every call decodes and converts every pixel. Padded native rows exposed without an extra packing copy. Startup, I/O, IPC, generation/encoding, hashing and validation excluded. Exact original RGBA, alpha and hidden RGB. One existing raster; no quality enumeration. Only this literal/run VP8L profile is measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); state?.Dispose(); ready = false; }
    }
}

internal sealed record WarmWebpDecodePreparation(int SchemaVersion, string Project, string Recipe, string InputFile, string RasterFile,
    int InputBytes, string InputSha256, string RasterSha256, string LibrarySha256, string PreparedUtc);
