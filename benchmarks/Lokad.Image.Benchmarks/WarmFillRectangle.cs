using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmFillRectangleCase { DrawingFill8, DrawingFill16, DrawingFill8OriginalClipped, DrawingFill16OriginalClipped }

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
    private bool smoke, ready, restored;
    private int cpu, sampleCount, depth, x, y, width, clippedWidth;
    private bool original;

    public static WarmFillRectangleCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            null => [WarmFillRectangleCase.DrawingFill8, WarmFillRectangleCase.DrawingFill16],
            "DrawingFill8" => [WarmFillRectangleCase.DrawingFill8],
            "DrawingFill16" => [WarmFillRectangleCase.DrawingFill16],
            "DrawingFill8OriginalClipped" => [WarmFillRectangleCase.DrawingFill8OriginalClipped],
            "DrawingFill16OriginalClipped" => [WarmFillRectangleCase.DrawingFill16OriginalClipped],
            "OriginalClipped" => [WarmFillRectangleCase.DrawingFill8OriginalClipped, WarmFillRectangleCase.DrawingFill16OriginalClipped],
            _ => throw new ArgumentException("Choose a named rectangle-fill case.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        depth = Case switch
        {
            WarmFillRectangleCase.DrawingFill8 or WarmFillRectangleCase.DrawingFill8OriginalClipped => 8,
            WarmFillRectangleCase.DrawingFill16 or WarmFillRectangleCase.DrawingFill16OriginalClipped => 16,
            _ => throw new ArgumentOutOfRangeException(nameof(Case))
        };
        original = Case is WarmFillRectangleCase.DrawingFill8OriginalClipped or WarmFillRectangleCase.DrawingFill16OriginalClipped;
        x = original ? -3 : -17; y = original ? 5 : 19; width = original ? 257 : 273;
        clippedWidth = width + x;
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
            for (var row = y; row < y + 129; row++)
            for (var column = 0; column < clippedWidth; column++)
            for (var c = 0; c < 4; c++)
            {
                var offset = row * stride + (column * 4 + c) * sampleBytes;
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
            ImageDrawing.FillRectangle(canvas, x, y, width, 129, color);
        if (!OriginalPixels.Bytes(canvas).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Rectangle-fill full canvas differs.");
    }
    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Native rectangle-fill full canvas differs.");
    }

    [IterationSetup]
    public void RestoreCanvas()
    {
        var stride = canvas.Stride;
        for (var row = 0; row < canvas.Height; row++)
        {
            var destination = depth == 8 ? canvas.GetRow8(row) : MemoryMarshal.AsBytes(canvas.GetRow16(row));
            input.AsSpan(row * stride, stride).CopyTo(destination);
        }
        restored = true;
    }

    [Benchmark]
    public int Managed()
    {
        if (!restored) throw new InvalidOperationException("Every measured fill requires a fresh canvas reset.");
        restored = false;
        ImageDrawing.FillRectangle(canvas, x, y, width, 129, color);
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
                outputWidth = 513, outputHeight = 259, x, y, fillWidth = width, fillHeight = 129, clippedX = 0, clippedY = y, clippedWidth, clippedHeight = 129,
                originalRequiredControl = original ? $"FillRectangle/Rgba{depth}Clipped" : null,
                color, colorNormalizationTimed = true, clippingTimed = true,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                sourceIdentity, nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, untouchedCanvasExact = true,
                sourceUnchanged = true, borrowedCanvas = true, callerBuffersAlive = true,
                sourcePreparationTimed = false, canvasPreparationTimed = false, outputAllocationTimed = false, nativeExitCode = 0,
                resetBeforeEveryOperation = true,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmFillRectangle).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"One complete public FillRectangle into the resident original 513x259 RGBA{depth} canvas: requested x={x},y={y},w={width},h=129, clipped to x=0,y={y},w={clippedWidth},h=129. Color is RGBA8(23,57,91,128) expanded exactly to RGBA16 or RGBA16(12345,23456,34567,45678). Complete borrowed-canvas mutation, admission, validation, clipping, required color normalization, cancellation and scope cleanup. Native public FFmpeg av_image_fill_color receives the same 16-bit source color and performs required normalization and clipping inside every operation. Both sides restore the entire original canvas outside the timer before every operation. Startup, IPC, I/O and verification are excluded. Exact full canvas, alpha, hidden RGB and untouched pixels. One CPU, normal SIMD."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); canvas?.Dispose(); state?.Dispose(); ready = false; }
    }
}
