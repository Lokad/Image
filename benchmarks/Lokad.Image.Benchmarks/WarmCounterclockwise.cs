using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmCounterclockwiseCase { Whole8, Whole16 }

/// <summary>One complete owned counterclockwise quarter turn on the existing original raster.</summary>
[MemoryDiagnoser]
public class WarmCounterclockwise
{
    public static IEnumerable<WarmCounterclockwiseCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmCounterclockwiseCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] input = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount, depth;
    private const int Width = 259, Height = 513;

    public static WarmCounterclockwiseCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            "Whole8" => [WarmCounterclockwiseCase.Whole8],
            null or "Whole16" => [WarmCounterclockwiseCase.Whole16],
            _ => throw new ArgumentException("Choose one counterclockwise quarter turn case: Whole8 or Whole16.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        depth = Case switch { WarmCounterclockwiseCase.Whole8 => 8, WarmCounterclockwiseCase.Whole16 => 16, _ => throw new ArgumentOutOfRangeException(nameof(Case)) };
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Counterclockwise quarter turn requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            input = HorizontalFlipInput.Create(depth);
            expected = new byte[Width * Height * (depth / 8) * 4];
            var pixelBytes = depth / 8 * 4;
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                input.AsSpan((x * 513 + 513 - 1 - y) * pixelBytes, pixelBytes)
                    .CopyTo(expected.AsSpan((y * Width + x) * pixelBytes, pixelBytes));
            source = JpegBenchmarkCase.Buffer(input, 513, 259, depth);
            File.WriteAllBytes(state.Input, input);
            ValidateManaged();
            native = new NativeWarmReference(state, depth, cpu, "quarter-turn-ccw");
            RequireNativeOutput();
            var format = depth == 8 ? "rgba" : "rgba64le";
            var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-cpuflags", "0",
                "-f", "rawvideo", "-pix_fmt", format, "-s", "513x259", "-i", "pipe:0", "-vf", "transpose=dir=cclock:passthrough=none",
                "-frames:v", "1", "-f", "rawvideo", "-pix_fmt", format, "pipe:1" };
            if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input).AsSpan().SequenceEqual(expected))
                throw new InvalidOperationException("Scalar CLI counterclockwise quarter turn placement differs.");
            arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
            if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input).AsSpan().SequenceEqual(expected))
                throw new InvalidOperationException("Pinned CLI counterclockwise quarter turn pixels differ from the original integer reference.");
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateManaged()
    {
        using var first = ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90);
        using var second = ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (Width, Height, depth, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Counterclockwise quarter turn geometry or RGBA differs from counterclockwise whole-pixel placement.");
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Counterclockwise quarter turn outputs alias or source changed.");
    }
    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Native counterclockwise quarter turn RGBA differs from integer coordinates.");
    }
    [Benchmark]
    public int Managed()
    {
        using var result = ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90);
        return result.Stride * result.Height;
    }
    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged();
            var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); RequireNativeOutput(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", nameof(WarmCounterclockwise) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmCounterclockwise), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth,
                outputWidth = Width, outputHeight = Height, cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount,
                serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription,
                platform = RuntimeInformation.RuntimeIdentifier, sourceIdentity,
                nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, nativeCliPixelsExact = true, nativeCliNormalScalarPixelsExact = true,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmCounterclockwise).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"One resident original 513x259 RGBA{depth} raster rotated counterclockwise into an independent owned 259x513 raster at the same depth. Complete public Rotate(CounterClockwise90), admission, allocation, materialization and disposal. Native public FFmpeg transpose graph allocates/processes independent pixels and releases output inside every operation. Graph/source/frame wrappers are reused. Native padded rows exposed directly; no redundant packing/materialization copy. Exact counterclockwise placement, alpha and hidden RGB; no output cache. Startup, IPC, I/O, generation, hashing and validation excluded; one CPU/filter thread, normal SIMD. Only this original raster is measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
