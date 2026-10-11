using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmBilinearCase { Down8 }

/// <summary>One complete bilinear resize on the existing original raster.</summary>
[MemoryDiagnoser]
public class WarmBilinearResize
{
    public static IEnumerable<WarmBilinearCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmBilinearCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] input = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount;
    private byte[] nativePixels = [];
    private int nativeRgbMaximum;
    private double nativeRgbRmse;
    private const int Width = 256, Height = 129;

    public static WarmBilinearCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            null or "Down8" => [WarmBilinearCase.Down8],
            _ => throw new ArgumentException("Choose the single bilinear case Down8.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        if (Case != WarmBilinearCase.Down8) throw new ArgumentOutOfRangeException(nameof(Case));
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Bilinear requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            input = HorizontalFlipInput.Create(8);
            expected = ExactResizeReference.Resize(input, 513, 259, 8, Width, Height, ResizeFilter.Bilinear);
            source = JpegBenchmarkCase.Buffer(input, 513, 259, 8);
            File.WriteAllBytes(state.Input, input);
            ValidateManaged();
            native = new NativeWarmReference(state, 8, cpu, "bilinear-resize");
            RequireNativeOutput();
            var arguments = ResizeOracle.PremultipliedBilinearArguments(513, 259, Width, Height).ToList();
            arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
            if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input).AsSpan().SequenceEqual(nativePixels))
                throw new InvalidOperationException("Pinned CLI bilinear pixels differ from the native worker.");
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateManaged()
    {
        using var first = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Bilinear);
        using var second = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Bilinear);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (Width, Height, 8, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Bilinear geometry or RGBA differs from exact center coordinates.");
        first.GetRow8(0)[0] ^= 255;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Bilinear outputs alias or source changed.");
    }
    private void RequireNativeOutput()
    {
        var pixels = File.ReadAllBytes(state.NativeOutput);
        if (pixels.Length != expected.Length || (nativePixels.Length != 0 && !pixels.AsSpan().SequenceEqual(nativePixels)))
            throw new InvalidOperationException("Native bilinear output length or deterministic pixels differ.");
        long squared = 0; var maximum = 0;
        for (var i = 0; i < pixels.Length; i += 4)
        {
            if (pixels[i + 3] != expected[i + 3] || (pixels[i + 3] == 0 && (pixels[i] | pixels[i + 1] | pixels[i + 2]) != 0))
                throw new InvalidOperationException("Native bilinear alpha or zero-alpha RGB differs.");
            for (var c = 0; c < 3; c++)
            {
                var difference = Math.Abs(pixels[i + c] - expected[i + c]);
                squared += difference * difference; maximum = Math.Max(maximum, difference);
            }
        }
        nativeRgbMaximum = maximum; nativeRgbRmse = Math.Sqrt((double)squared / (pixels.Length / 4 * 3));
        if (maximum > 1 || nativeRgbRmse > 0.25)
            throw new InvalidOperationException("Native bilinear RGB exceeds the existing filtered comparison bound.");
        nativePixels = pixels;
    }
    [Benchmark]
    public int Managed()
    {
        using var result = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Bilinear);
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
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", nameof(WarmBilinearResize) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmBilinearResize), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = 8,
                outputWidth = Width, outputHeight = Height, cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount,
                serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription,
                platform = RuntimeInformation.RuntimeIdentifier, sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                managedRationalReferenceExact = true, nativeAlphaExact = true, nativeZeroAlphaRgbZero = true,
                nativeRgbMaximum, nativeRgbRmse, nativePixelsDeterministic = true, nativeCliPixelsExact = true,
                nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmBilinearResize).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "One resident original 513x259 RGBA8 raster to independently owned 256x129 bilinear output. Complete public Resize, admission, allocation/materialization/disposal. Native public FFmpeg filter graph includes packed/planar conversion, zscale widened triangle kernel with straight-alpha metadata and internal encoded-space premultiplication/unpremultiplication, final RGBA conversion and output pixel acquisition/release. Graph/source/frame wrappers and geometry contexts are reused; each call filters the full input. Native padded output rows exposed directly. Managed exact rational-reference pixels; native RGB maximum1/RMSE0.25 existing filtered comparison bound, exact alpha and zero-alpha RGBzero; native CLI/deterministic output exact. No output cache or expression evaluator. Startup, IPC, I/O, generation, hashing and validation excluded; one CPU/filter thread, normal SIMD. Only this downsample case is measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
