using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmCropCase { Fixed8, Fixed16 }

/// <summary>One complete owned crop on the existing original raster.</summary>
[MemoryDiagnoser]
public class WarmCrop
{
    public static IEnumerable<WarmCropCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmCropCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] input = [], expected = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount, depth;
    private const int Width = 256, Height = 129;

    public static WarmCropCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            "Fixed8" => [WarmCropCase.Fixed8],
            null or "Fixed16" => [WarmCropCase.Fixed16],
            _ => throw new ArgumentException("Choose one crop case: Fixed8 or Fixed16.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        depth = Case switch { WarmCropCase.Fixed8 => 8, WarmCropCase.Fixed16 => 16, _ => throw new ArgumentOutOfRangeException(nameof(Case)) };
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC
            || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Crop requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            input = HorizontalFlipInput.Create(depth);
            expected = new byte[Width * Height * (depth / 8) * 4];
            for (var y = 0; y < Height; y++)
                input.AsSpan(((y + 65) * 513 + 127) * (depth / 8) * 4, Width * (depth / 8) * 4)
                    .CopyTo(expected.AsSpan(y * Width * (depth / 8) * 4));
            source = JpegBenchmarkCase.Buffer(input, 513, 259, depth);
            File.WriteAllBytes(state.Input, input);
            ValidateManaged();
            native = new NativeWarmReference(state, depth, cpu, "crop");
            RequireNativeOutput();
            var format = depth == 8 ? "rgba" : "rgba64le";
            var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-cpuflags", "0",
                "-f", "rawvideo", "-pix_fmt", format, "-s", "513x259", "-i", "pipe:0", "-vf", "crop=w=256:h=129:x=127:y=65:exact=1",
                "-frames:v", "1", "-f", "rawvideo", "-pix_fmt", format, "pipe:1" };
            if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input).AsSpan().SequenceEqual(expected))
                throw new InvalidOperationException("Scalar CLI crop pixels differ from the original integer reference.");
            arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
            if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input).AsSpan().SequenceEqual(expected))
                throw new InvalidOperationException("Pinned CLI crop pixels differ from the original integer reference.");
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidateManaged()
    {
        using var first = ImageTransforms.Crop(source, 127, 65, Width, Height);
        using var second = ImageTransforms.Crop(source, 127, 65, Width, Height);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (Width, Height, depth, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Crop geometry or RGBA differs from exact center coordinates.");
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Crop outputs alias or source changed.");
    }
    private void RequireNativeOutput()
    {
        if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Native crop RGBA differs from integer coordinates.");
    }
    [Benchmark]
    public int Managed()
    {
        using var result = ImageTransforms.Crop(source, 127, 65, Width, Height);
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
            var path = Path.Combine(state.Root, "artifacts/benchmark-validation", nameof(WarmCrop) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmCrop), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth,
                outputWidth = Width, outputHeight = Height, cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount,
                serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription,
                platform = RuntimeInformation.RuntimeIdentifier, sourceIdentity,
                nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                cropX = 127, cropY = 65, inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true, nativeCliPixelsExact = true, nativeCliNormalScalarPixelsExact = true,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmCrop).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = $"One resident original 513x259 RGBA{depth} raster cropped at x=127,y=65 to independent owned 256x129 RGBA at the same depth. Complete public Crop, admission, allocation, materialization and disposal. Native public FFmpeg crop graph produces a view, then public frame allocation/copy materializes independent pixels inside every operation before release. Graph/source/frame wrappers are reused. Native padded rows exposed directly. Exact placement, alpha and hidden RGB; no output cache. Startup, IPC, I/O, generation, hashing and validation excluded; one CPU/filter thread, normal SIMD. Only this rectangle is measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
