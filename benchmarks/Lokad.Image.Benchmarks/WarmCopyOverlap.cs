using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmCopyOverlapCase { DrawingCopyOverlap8 }

/// <summary>Exact rectangle mutation of an existing caller-owned canvas.</summary>
[MemoryDiagnoser]
public class WarmCopyOverlap
{
    public static IEnumerable<WarmCopyOverlapCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmCopyOverlapCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer canvas = null!;
    private byte[] input = [], forward = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount, depth;

    public static WarmCopyOverlapCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            null or "DrawingCopyOverlap8" => [WarmCopyOverlapCase.DrawingCopyOverlap8],
            _ => throw new ArgumentException("Choose DrawingCopyOverlap8.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        depth = Case == WarmCopyOverlapCase.DrawingCopyOverlap8 ? 8 : throw new ArgumentOutOfRangeException(nameof(Case));
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Rectangle copy requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            input = HorizontalFlipInput.Create(depth);
            forward = Snapshot(input, 17, 19, 127, 65);
            expected = Snapshot(forward, 127, 65, 17, 19);
            canvas = JpegBenchmarkCase.Buffer(input, 513, 259, depth);
            File.WriteAllBytes(state.Input, input);
            ValidateManaged();
            native = new NativeWarmReference(state, cpu, Case);
            RequireNativeOutput();
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); canvas?.Dispose(); state.Dispose(); throw; }
    }
    private static byte[] Snapshot(byte[] source, int sx, int sy, int dx, int dy)
    {
        var result = source.ToArray();
        for (var row = 0; row < 129; row++)
            source.AsSpan(((sy + row) * 513 + sx) * 4, 256 * 4)
                .CopyTo(result.AsSpan(((dy + row) * 513 + dx) * 4));
        return result;
    }
    private void ValidateManaged()
    {
        for (var repeat = 0; repeat < 2; repeat++)
        {
            ImageDrawing.CopyRegion(canvas, 17, 19, 256, 129, canvas, 127, 65);
            if (!OriginalPixels.Bytes(canvas).AsSpan().SequenceEqual(forward))
                throw new InvalidOperationException("Forward snapshot pixels differ.");
            ImageDrawing.CopyRegion(canvas, 127, 65, 256, 129, canvas, 17, 19);
            if (!OriginalPixels.Bytes(canvas).AsSpan().SequenceEqual(expected))
                throw new InvalidOperationException("Reverse snapshot/restored canvas differs.");
        }
    }
    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Native rectangle-copy full canvas differs.");
    }
    [Benchmark]
    public int Managed()
    {
        ImageDrawing.CopyRegion(canvas, 17, 19, 256, 129, canvas, 127, 65);
        ImageDrawing.CopyRegion(canvas, 127, 65, 256, 129, canvas, 17, 19);
        return canvas.Stride * canvas.Height;
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
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", nameof(WarmCopyOverlap) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmCopyOverlap), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth,
                outputWidth = 513, outputHeight = 259, sourceX = 17, sourceY = 19, copyWidth = 256, copyHeight = 129, destinationX = 127, destinationY = 65,
                publicMovesPerPair = 2, nativeCallsPerPair = 6, sourceAliasesCanvas = true, snapshotSemanticsExact = true, restorationTimed = true, temporaryPixelBytes = 0,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                sourceIdentity, nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                inputSha256 = JpegBenchmarkCase.Hash(input), forwardSha256 = JpegBenchmarkCase.Hash(forward), outputSha256 = JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, untouchedCanvasExact = true,
                sourceUnchanged = false, borrowedSource = true, borrowedCanvas = true, callerBuffersAlive = true,
                sourcePreparationTimed = false, canvasPreparationTimed = false, outputAllocationTimed = false, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmCopyOverlap).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "One complete paired overlapping-copy operation on the original resident 513x259 RGBA8 canvas. Forward source17,19/256x129 to127,65, then reverse source127,65/256x129 to17,19. TWO public CopyRegion calls and their admission/cancellation/scope cleanup per managed operation; reverse restoration included in BOTH elapsed/allocation totals. Native public FFmpeg plane copies use three disjoint46-row-or-smaller bands in the correct direction per move, SIX calls per pair, no temporary pixels. One borrowed mutable canvas; snapshot semantics, full forward/final pixels, alpha/hiddenRGB and untouched pixels exact. Pair state stable after preparation; forward still changes overlap pixels. No reset/output allocation/full-frame copy on either subject. Startup/preparation/IPC/I/O/verification excluded. One CPU, normal SIMD. Costs are per pair, not per single move; only these fixed directions/rectangle/depth are measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); canvas?.Dispose(); state?.Dispose(); ready = false; }
    }
}
