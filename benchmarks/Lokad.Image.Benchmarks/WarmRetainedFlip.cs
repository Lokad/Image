using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>Warm single-CPU operation; native elapsed samples are measured inside its worker.</summary>
public enum RetainedFlipCase { Rgba8 }

[MemoryDiagnoser]
public class WarmRetainedFlip
{
    public static IEnumerable<RetainedFlipCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public RetainedFlipCase Case { get; set; }
    private const int Depth = 8;
    public static RetainedFlipCase[] SelectedCases() => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
    {
        null or "Rgba8" => [RetainedFlipCase.Rgba8],
        _ => throw new ArgumentException("Choose the single retained-two flip case: Rgba8.")
    };
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] input = [], expected = [];
    private NativeWarmSample[] before = [];
    private bool smoke, ready;
    private int cpu;
    private int sampleCount;
    private JsonElement sourceIdentity;

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm FFmpeg reference measurements require Linux.");
        ready = false;
        if (Case != RetainedFlipCase.Rgba8) throw new ArgumentOutOfRangeException(nameof(Case));
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
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
            native = new NativeWarmReference(state, Depth, cpu, "hflip-retained2");
            RequireNativeOutput();
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateManaged()
    {
        using var first = ImageTransforms.FlipHorizontal(source);
        using var second = ImageTransforms.FlipHorizontal(source);
        if (!OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("A retained output differs from independent pixel placement.");
        var saved = first.GetRow8(0)[0]; first.GetRow8(0)[0] ^= 255;
        if (second.GetRow8(0)[0] != expected[0] || source.GetRow8(0)[0] != input[0])
            throw new InvalidOperationException("Simultaneously live output/source storage aliases.");
        first.GetRow8(0)[0] = saved;
        if (!OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Managed flip changed source samples.");
    }

    private void RequireNativeOutput()
    {
        var bytes = File.ReadAllBytes(state.NativeOutput);
        if (bytes.Length != expected.Length * 2 || !bytes.AsSpan(0, expected.Length).SequenceEqual(expected)
            || !bytes.AsSpan(expected.Length).SequenceEqual(expected))
            throw new InvalidOperationException("Native pixels differ from independent forward placement.");
    }

    [Benchmark(OperationsPerInvoke = 2)]
    public int Managed()
    {
        using var first = ImageTransforms.FlipHorizontal(source);
        using var second = ImageTransforms.FlipHorizontal(source);
        return first.GetRow8(0)[0] + second.GetRow8(0)[0] + first.Stride * first.Height + second.Stride * second.Height;
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
            var directory = Path.Combine(state.Root, "artifacts", "benchmark-validation");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, nameof(WarmRetainedFlip) + "-" + Guid.NewGuid().ToString("N") + ".json");
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmRetainedFlip), caseId = Case.ToString(), smoke, nativeSampleCount = smoke ? 1 : sampleCount,
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
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmRetainedFlip).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready,
                outputsPerUnit = 2, releaseAfterBoth = true, timingAndAllocationUnit = "per image",
                nativeRawUnit = "two-image burst", rawNativeBefore = before, rawNativeAfter = after,
                nativeBefore = PerImage(before), nativeAfter = PerImage(after),
                boundary = "Two complete public horizontal flips from the same resident original RGBA8 source; both independent owned pixel outputs stay live until both calls finish, then both are released. BenchmarkDotNet OperationsPerInvoke=2 and native burst counters scaled by two report elapsed/allocation per image. Native raw pair counters are retained. Native public FFmpeg hflip graph performs the same two complete transforms with two non-aliasing owned frames held before either unref; prepared graph/source/frame wrappers reused. Allocation, full materialization and release included; no packing copy or pixel/depth conversion. Exact placement, hidden RGB, source preservation and live-owner independence checked outside timing. Startup, IPC, transport, file IO, hashes and validation excluded."

            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine("Validated warm memory pair; untimed evidence: " + path);
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
    private static NativeWarmSample[] PerImage(NativeWarmSample[] samples)
        => samples.Select(s => s with { Operations = checked(s.Operations * 2), CompletedOperations = checked(s.CompletedOperations * 2) }).ToArray();

}
