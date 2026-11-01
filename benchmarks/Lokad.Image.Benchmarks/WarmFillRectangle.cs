using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmFillRectangleCase { DrawingFill8, DrawingFill16 }

/// <summary>Exact rectangle fill of an existing caller-owned canvas.</summary>
[MemoryDiagnoser]
public class WarmFillRectangle
{
    public static IEnumerable<WarmFillRectangleCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmFillRectangleCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer canvas = null!;
    private RgbaColor color;
    private byte[] input = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount, depth;

    public static WarmFillRectangleCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            null => [WarmFillRectangleCase.DrawingFill8, WarmFillRectangleCase.DrawingFill16],
            "DrawingFill8" => [WarmFillRectangleCase.DrawingFill8],
            "DrawingFill16" => [WarmFillRectangleCase.DrawingFill16],
            _ => throw new ArgumentException("Choose DrawingFill8 or DrawingFill16.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        depth = Case switch { WarmFillRectangleCase.DrawingFill8 => 8, WarmFillRectangleCase.DrawingFill16 => 16, _ => throw new ArgumentOutOfRangeException(nameof(Case)) };
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Rectangle fill requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            input = HorizontalFlipInput.Create(depth);
            expected = input.ToArray();
            color = depth == 8 ? RgbaColor.From8(23, 57, 91, 128) : new RgbaColor(12345, 23456, 34567, 45678);
            var values = new[] { color.R, color.G, color.B, color.A };
            var sampleBytes = depth / 8; var stride = 513 * sampleBytes * 4;
            for (var y = 19; y < 148; y++)
            for (var x = 0; x < 256; x++)
            for (var c = 0; c < 4; c++)
            {
                var offset = y * stride + (x * 4 + c) * sampleBytes;
                if (depth == 8) expected[offset] = (byte)((values[c] + 128) / 257);
                else System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(offset), values[c]);
            }
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
    private void ValidateManaged()
    {
        for (var i = 0; i < 2; i++)
            ImageDrawing.FillRectangle(canvas, -17, 19, 273, 129, color);
        if (!OriginalPixels.Bytes(canvas).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Rectangle-fill full canvas differs.");
    }
    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Native rectangle-fill full canvas differs.");
    }
    [Benchmark]
    public int Managed()
    {
        ImageDrawing.FillRectangle(canvas, -17, 19, 273, 129, color);
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
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", nameof(WarmFillRectangle) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmFillRectangle), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth,
                outputWidth = 513, outputHeight = 259, x = -17, y = 19, fillWidth = 273, fillHeight = 129, clippedX = 0, clippedY = 19, clippedWidth = 256, clippedHeight = 129,
                color, colorNormalizationTimed = true, clippingTimed = true,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                sourceIdentity, nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, untouchedCanvasExact = true,
                sourceUnchanged = true, borrowedCanvas = true, callerBuffersAlive = true,
                sourcePreparationTimed = false, canvasPreparationTimed = false, outputAllocationTimed = false, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmFillRectangle).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"One complete public FillRectangle into the resident original 513x259 RGBA{depth} canvas: requested x=-17,y=19,w=273,h=129, clipped to x=0,y=19,w=256,h=129. Color is RGBA8(23,57,91,128) expanded exactly to RGBA16 or RGBA16(12345,23456,34567,45678). Complete borrowed-canvas mutation, admission, validation, clipping, required color normalization, cancellation and scope cleanup. Native public FFmpeg av_image_fill_color receives the same 16-bit source color and performs required normalization and clipping inside every operation. Canvas preparation/reset, startup, IPC, I/O and verification excluded equally; overwrites are idempotent, no reset/output allocation/full-frame copy. Exact full canvas, alpha, hidden RGB and untouched pixels. One CPU, normal SIMD; only this rectangle/color per depth is measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); canvas?.Dispose(); state?.Dispose(); ready = false; }
    }
}
