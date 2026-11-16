using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmCopyRegionCase { DrawingCopy8, DrawingCopy16, DrawingCopy8OriginalClipped }

/// <summary>Exact rectangle mutation of an existing caller-owned canvas.</summary>
[MemoryDiagnoser]
public class WarmCopyRegion
{
    public static IEnumerable<WarmCopyRegionCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmCopyRegionCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!, canvas = null!;
    private byte[] input = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount, depth, sourceX, sourceY, destinationX, destinationY, width;
    private bool original;

    public static WarmCopyRegionCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            null => [WarmCopyRegionCase.DrawingCopy8, WarmCopyRegionCase.DrawingCopy16],
            "DrawingCopy8" => [WarmCopyRegionCase.DrawingCopy8],
            "DrawingCopy16" => [WarmCopyRegionCase.DrawingCopy16],
            "DrawingCopy8OriginalClipped" => [WarmCopyRegionCase.DrawingCopy8OriginalClipped],
            _ => throw new ArgumentException("Choose a named rectangle-copy case.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        depth = Case switch { WarmCopyRegionCase.DrawingCopy8 or WarmCopyRegionCase.DrawingCopy8OriginalClipped => 8, WarmCopyRegionCase.DrawingCopy16 => 16, _ => throw new ArgumentOutOfRangeException(nameof(Case)) };
        original = Case == WarmCopyRegionCase.DrawingCopy8OriginalClipped;
        sourceX = original ? 0 : 127; sourceY = original ? 0 : 65;
        destinationX = original ? -3 : 17; destinationY = original ? 5 : 19; width = original ? 257 : 256;
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
            expected = input.ToArray();
            var pixel = depth / 8 * 4; var stride = 513 * pixel;
            for (var y = 0; y < 129; y++)
                input.AsSpan((y + sourceY) * stride + (original ? 3 : 127) * pixel, (original ? 254 : 256) * pixel)
                    .CopyTo(expected.AsSpan((y + destinationY) * stride + (original ? 0 : 17) * pixel, (original ? 254 : 256) * pixel));
            source = JpegBenchmarkCase.Buffer(input, 513, 259, depth);
            canvas = JpegBenchmarkCase.Buffer(input, 513, 259, depth);
            File.WriteAllBytes(state.Input, input);
            ValidateManaged();
            native = new NativeWarmReference(state, cpu, Case);
            RequireNativeOutput();
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); canvas?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }
    private void ValidateManaged()
    {
        for (var i = 0; i < 2; i++)
            ImageDrawing.CopyRegion(source, sourceX, sourceY, width, 129, canvas, destinationX, destinationY);
        if (!OriginalPixels.Bytes(canvas).AsSpan().SequenceEqual(expected)
            || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Rectangle-copy full canvas/source differs.");
    }
    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Native rectangle-copy full canvas differs.");
    }
    [Benchmark]
    public int Managed()
    {
        ImageDrawing.CopyRegion(source, sourceX, sourceY, width, 129, canvas, destinationX, destinationY);
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
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", nameof(WarmCopyRegion) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmCopyRegion), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth,
                outputWidth = 513, outputHeight = 259, sourceX, sourceY, copyWidth = width, copyHeight = 129, destinationX, destinationY,
                originalRequiredControl = original ? "CopyRegion/Rgba8Independent" : null, clippingTimed = true,
                clippedSourceX = original ? 3 : 127, clippedSourceY = sourceY, clippedX = original ? 0 : 17, clippedY = destinationY, clippedWidth = original ? 254 : 256,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                sourceIdentity, nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, untouchedCanvasExact = true,
                sourceUnchanged = true, borrowedSource = true, borrowedCanvas = true, callerBuffersAlive = true,
                sourcePreparationTimed = false, canvasPreparationTimed = false, outputAllocationTimed = false, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmCopyRegion).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"One complete public CopyRegion from the resident original 513x259 RGBA{depth} raster: source x{sourceX},y{sourceY},w{width},h129 to x{destinationX},y{destinationY} in a separate caller-owned 513x259 canvas at the same depth. Admission, validation, clipping, cooperative cancellation checkpoints and operation-scope cleanup are included. Native public FFmpeg av_image_copy_plane mutates the equivalent borrowed canvas with peroperation clipping. Source/canvas preparation, reset, IPC, startup, I/O and verification excluded equally; identical overwrites are idempotent, with no reset between operations. No output allocation or full-frame copy on either subject. Exact full canvas, alpha, hidden RGB, untouched pixels and unchanged source. One CPU, normal SIMD; only this rectangle is measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); canvas?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
