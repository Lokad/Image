using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmNearestCase { Down8, Down16, Up8, Up16 }

/// <summary>One complete nearest resize on the existing original raster.</summary>
[MemoryDiagnoser]
public class WarmNearestResize
{
    public static IEnumerable<WarmNearestCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmNearestCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] input = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount, depth;
    private bool Upscale => Case is WarmNearestCase.Up8 or WarmNearestCase.Up16;
    private int Width => Upscale ? 1027 : 256;
    private int Height => Upscale ? 519 : 129;

    public static WarmNearestCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            null or "Down8" => [WarmNearestCase.Down8],
            "Down16" => [WarmNearestCase.Down16],
            "Up8" => [WarmNearestCase.Up8],
            "Up16" => [WarmNearestCase.Up16],
            _ => throw new ArgumentException("Choose one nearest case: Down8, Down16, Up8 or Up16.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        depth = Case switch { WarmNearestCase.Down8 or WarmNearestCase.Up8 => 8, WarmNearestCase.Down16 or WarmNearestCase.Up16 => 16, _ => throw new ArgumentOutOfRangeException(nameof(Case)) };
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Nearest requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            input = HorizontalFlipInput.Create(depth);
            expected = ExactResizeReference.Resize(input, 513, 259, depth, Width, Height, ResizeFilter.Nearest);
            source = JpegBenchmarkCase.Buffer(input, 513, 259, depth);
            File.WriteAllBytes(state.Input, input);
            ValidateManaged();
            native = new NativeWarmReference(state, depth, cpu, "nearest-resize",
                resizeWidth: Upscale ? Width : null,
                resizeHeight: Upscale ? Height : null);
            RequireNativeOutput();
            var arguments = ResizeOracle.NearestArguments(513, 259, Width, Height, depth).ToList();
            arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
            if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input).AsSpan().SequenceEqual(expected))
                throw new InvalidOperationException("Pinned CLI nearest pixels differ from the original integer reference.");
            if (!OracleProcess.Run(state.Ffmpeg, ResizeOracle.NearestArguments(513, 259, Width, Height, depth), input).AsSpan().SequenceEqual(expected))
                throw new InvalidOperationException("Scalar CLI nearest pixels differ from the original integer reference.");
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateManaged()
    {
        using var first = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Nearest);
        using var second = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Nearest);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (Width, Height, depth, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Nearest geometry or RGBA differs from exact center coordinates.");
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Nearest outputs alias or source changed.");
    }
    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Native nearest RGBA differs from integer coordinates.");
    }
    [Benchmark]
    public int Managed()
    {
        using var result = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Nearest);
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
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", nameof(WarmNearestResize) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmNearestResize), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth,
                outputWidth = Width, outputHeight = Height, cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount,
                serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription,
                platform = RuntimeInformation.RuntimeIdentifier, sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, nativeCliPixelsExact = true, nativeScalarCliPixelsExact = true,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmNearestResize).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"One resident original 513x259 RGBA{depth} raster to independently owned {Width}x{Height} nearest output at the same sample depth. Complete public Resize, admission, allocation/materialization/disposal. Native public FFmpeg filter graph includes packing/plane extraction, separate RGB and replicated-alpha zscale point kernels, merging, output pixel allocation/release. Graph/source/frame wrappers and geometry contexts are reused; every call filters every selected sample. Native padded output rows exposed directly. Exact integer center mapping, alpha and hidden RGB. No codec, expression evaluator or output cache. Startup, IPC, I/O, generation, hashing and validation excluded; one CPU/filter thread, normal SIMD. Only this fixed resize case is measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
