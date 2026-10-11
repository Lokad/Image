using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmNormalOrientationCase { Whole8, Whole16 }

/// <summary>One complete owned normal-orientation copy on the existing original raster.</summary>
[MemoryDiagnoser]
public class WarmNormalOrientation
{
    public static IEnumerable<WarmNormalOrientationCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmNormalOrientationCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] input = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount, depth;
    private const int Width = 513, Height = 259;

    public static WarmNormalOrientationCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            "Whole8" => [WarmNormalOrientationCase.Whole8],
            null or "Whole16" => [WarmNormalOrientationCase.Whole16],
            _ => throw new ArgumentException("Choose one normal-orientation copy case: Whole8 or Whole16.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        depth = Case switch { WarmNormalOrientationCase.Whole8 => 8, WarmNormalOrientationCase.Whole16 => 16, _ => throw new ArgumentOutOfRangeException(nameof(Case)) };
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Normal-orientation copy requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            input = HorizontalFlipInput.Create(depth);
            expected = input.ToArray();
            source = JpegBenchmarkCase.Buffer(input, 513, 259, depth);
            File.WriteAllBytes(state.Input, input);
            ValidateManaged();
            native = new NativeWarmReference(state, depth, cpu, "normal-orient");
            RequireNativeOutput();
            var format = depth == 8 ? "rgba" : "rgba64le";
            var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-cpuflags", "0",
                "-f", "rawvideo", "-pix_fmt", format, "-s", "513x259", "-i", "pipe:0", "-vf", "null",
                "-frames:v", "1", "-f", "rawvideo", "-pix_fmt", format, "pipe:1" };
            if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input).AsSpan().SequenceEqual(expected))
                throw new InvalidOperationException("Scalar CLI normal-orientation copy placement differs.");
            arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
            if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input).AsSpan().SequenceEqual(expected))
                throw new InvalidOperationException("Pinned CLI normal-orientation copy pixels differ from the original integer reference.");
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateManaged()
    {
        using var first = ImageTransforms.AutoOrient(source);
        using var second = ImageTransforms.AutoOrient(source);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (Width, Height, depth, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Normal-orientation copy geometry or RGBA differs from complete channel bytes.");
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Normal-orientation copy outputs alias or source changed.");
    }
    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Native normal-orientation copy RGBA differs from integer coordinates.");
    }
    [Benchmark]
    public int Managed()
    {
        using var result = ImageTransforms.AutoOrient(source);
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
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", nameof(WarmNormalOrientation) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmNormalOrientation), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth,
                outputWidth = Width, outputHeight = Height, cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount,
                serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription,
                platform = RuntimeInformation.RuntimeIdentifier, sourceIdentity,
                nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, nativeCliPixelsExact = true, nativeCliNormalScalarPixelsExact = true,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmNormalOrientation).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"One resident original 513x259 RGBA{depth} raster with Normal orientation copied into an independent owned 513x259 raster at the same depth. Complete public AutoOrient, admission, allocation, full materialization and disposal. Native public FFmpeg null graph emits a shared view; av_frame_get_buffer plus av_frame_copy allocates and materializes an independent raster inside every operation before release. Graph/source/frame wrappers are reused. Native padded rows exposed directly; one necessary owned copy, no redundant packing copy. Exact complete channel bytes, alpha and hidden RGB; no output cache. Startup, IPC, I/O, generation, hashing and validation excluded; one CPU/filter thread, normal SIMD. Only this original raster is measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
