using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>Warm single-CPU operation; native elapsed samples are measured inside its worker.</summary>
[MemoryDiagnoser]
public class WarmHorizontalFlip
{
    [Params(8, 16)] public int Depth { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] input = [], expected = [];
    private NativeWarmSample[] before = [];
    private bool smoke, ready;
    private int cpu;
    private JsonElement sourceIdentity;

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm FFmpeg reference measurements require Linux.");
        ready = false;
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0")
            throw new InvalidOperationException("Managed worker requires one pinned CPU, processor count one and workstation GC.");
        var gc = GC.GetConfigurationVariables();
        if (!gc.TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Managed warm worker requires background GC disabled.");
        state = new JpegBenchmarkCase();
        try
        {
            var manifestPath = Path.Combine(state.Root, "tmp", "benchmark-source.json");
            if (!File.Exists(manifestPath)) throw new InvalidOperationException("Capture the owned source inventory before warm execution.");
            using (var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath)))
                sourceIdentity = JsonSerializer.SerializeToElement(new
                {
                    sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(),
                    sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString()
                });
            input = HorizontalFlipInput.Create(Depth); expected = HorizontalFlipInput.Expected(input, Depth);
            source = JpegBenchmarkCase.Buffer(input, HorizontalFlipInput.Width, HorizontalFlipInput.Height, Depth);
            File.WriteAllBytes(state.Input, input);
            ValidateManaged();
            native = new NativeWarmReference(state, Depth, cpu);
            RequireNativeOutput();
            before = native.Measure(smoke);
            ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateManaged()
    {
        HorizontalFlipInput.Validate(source, expected);
        if (!OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Managed flip changed source samples.");
    }

    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Native pixels differ from independent forward placement.");
    }

    [Benchmark]
    public int Managed()
    {
        using var result = ImageTransforms.FlipHorizontal(source);
        return result.Stride * result.Height;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged();
            var after = native.Measure(smoke, before[0].Operations);
            native.Check(); RequireNativeOutput(); native.Stop();
            var directory = Path.Combine(state.Root, "artifacts", "benchmark-validation");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, nameof(WarmHorizontalFlip) + "-" + Guid.NewGuid().ToString("N") + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmHorizontalFlip), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),
                width = source.Width, height = source.Height, depth = Depth, cpu, managedAffinityCount = 1,
                processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription,
                jitPolicy = "Tiered compilation disabled; fully optimized JIT before measurement",
                vector128 = System.Runtime.Intrinsics.Vector128.IsHardwareAccelerated,
                vector256 = System.Runtime.Intrinsics.Vector256.IsHardwareAccelerated,
                vector512 = System.Runtime.Intrinsics.Vector512.IsHardwareAccelerated,
                platform = RuntimeInformation.RuntimeIdentifier, sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact = true, sourceUnchanged = true, independentOwnedOutput = true,
                nativeExitCode = 0, librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmHorizontalFlip).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready,
                nativeBefore = before, nativeAfter = after,
                boundary = "Warm memory horizontal flip with output pixel allocation, complete materialization and release. Managed public ImageBuffer call, including admission/metadata; native public FFmpeg hflip graph with reused graph/source/frame wrappers and fresh non-aliasing pixel result. Native row alignment/padding is recorded; both expose packed RGBA rows, with no extra packing copy. Process launch, IPC, file transport and hashing are outside scored intervals. Native samples use worker monotonic elapsed time, not caller protocol duration. No codec or pixel/depth conversion. Native nativeBefore/nativeAfter bracket the managed BenchmarkDotNet iterations. Exact independent placement includes zero-alpha hidden RGB."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine("Validated warm memory pair; untimed evidence: " + path);
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
