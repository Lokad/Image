using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmBilinearCase { Down8, Down16, Up8, Up16 }

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
    private int nativeRgbMaximum, nativeAlphaMaximum;
    private int Depth => Case is WarmBilinearCase.Down16 or WarmBilinearCase.Up16 ? 16 : 8;
    private double nativeRgbRmse, nativeAlphaRmse;
    private int Width => Case is WarmBilinearCase.Up8 or WarmBilinearCase.Up16 ? 1027 : 256;
    private int Height => Case is WarmBilinearCase.Up8 or WarmBilinearCase.Up16 ? 519 : 129;

    public static WarmBilinearCase[] SelectedCases()
        => Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") switch
        {
            null or "Down8" => [WarmBilinearCase.Down8],
            "Down16" => [WarmBilinearCase.Down16],
            "Up8" => [WarmBilinearCase.Up8],
            "Up16" => [WarmBilinearCase.Up16],
            _ => throw new ArgumentException("Choose one bilinear case: Down8, Down16, Up8 or Up16.")
        };

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        if (Case is not (WarmBilinearCase.Down8 or WarmBilinearCase.Down16 or WarmBilinearCase.Up8 or WarmBilinearCase.Up16)) throw new ArgumentOutOfRangeException(nameof(Case));
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
            input = HorizontalFlipInput.Create(Depth);
            expected = ExactResizeReference.Resize(input, 513, 259, Depth, Width, Height, ResizeFilter.Bilinear);
            source = JpegBenchmarkCase.Buffer(input, 513, 259, Depth);
            File.WriteAllBytes(state.Input, input);
            ValidateManaged();
            native = new NativeWarmReference(state, Depth, cpu, "bilinear-resize", resizeWidth: Width, resizeHeight: Height);
            RequireNativeOutput();
            var scalarArguments = ResizeOracle.PremultipliedBilinearArguments(513, 259, Width, Height, Depth);
            if (!MatchesNativeCli(OracleProcess.Run(state.Ffmpeg, scalarArguments, input)))
                throw new InvalidOperationException("Scalar CLI bilinear pixels differ from the native worker.");
            var arguments = ResizeOracle.PremultipliedBilinearArguments(513, 259, Width, Height, Depth).ToList();
            arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
            if (!MatchesNativeCli(OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), input)))
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
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (Width, Height, Depth, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Bilinear geometry or RGBA differs from exact center coordinates.");
        if (Depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Bilinear outputs alias or source changed.");
    }
    private bool MatchesNativeCli(byte[] pixels)
    {
        if (Case == WarmBilinearCase.Up8)
            for (var i = 0; i + 3 < pixels.Length; i += 4)
                if (pixels[i + 3] == 0) pixels[i] = pixels[i + 1] = pixels[i + 2] = 0;
        return pixels.AsSpan().SequenceEqual(nativePixels);
    }
    private void RequireNativeOutput()
    {
        var pixels = File.ReadAllBytes(state.NativeOutput);
        if (pixels.Length != expected.Length || (nativePixels.Length != 0 && !pixels.AsSpan().SequenceEqual(nativePixels)))
            throw new InvalidOperationException("Native bilinear output length or deterministic pixels differ.");
        long squared = 0, alphaSquared = 0; var maximum = 0; nativeAlphaMaximum = 0;
        var sampleBytes = Depth / 8; var pixelBytes = 4 * sampleBytes;
        int Sample(byte[] bytes, int offset) => Depth == 8 ? bytes[offset] : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));
        for (var i = 0; i < pixels.Length; i += pixelBytes)
        {
            var alpha = Sample(pixels, i + 3 * sampleBytes);
            var alphaDifference = Math.Abs(alpha - Sample(expected, i + 3 * sampleBytes));
            alphaSquared += (long)alphaDifference * alphaDifference;
            nativeAlphaMaximum = Math.Max(nativeAlphaMaximum, alphaDifference);
            for (var c = 0; c < 3; c++)
            {
                var value = Sample(pixels, i + c * sampleBytes);
                if (alpha == 0 && value != 0) throw new InvalidOperationException("Native bilinear zero-alpha RGB differs.");
                var difference = Math.Abs(value - Sample(expected, i + c * sampleBytes));
                squared += (long)difference * difference; maximum = Math.Max(maximum, difference);
            }
        }
        var pixelCount = pixels.Length / pixelBytes;
        nativeRgbMaximum = maximum; nativeRgbRmse = Math.Sqrt((double)squared / (pixelCount * 3));
        nativeAlphaRmse = Math.Sqrt((double)alphaSquared / pixelCount);
        if (nativeAlphaMaximum > (Depth == 16 || Case == WarmBilinearCase.Up8 ? 1 : 0)
            || nativeAlphaRmse > (Depth == 16 || Case == WarmBilinearCase.Up8 ? 0.25 : 0))
            throw new InvalidOperationException("Native bilinear alpha exceeds the depth-specific comparison bound.");
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
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = Depth,
                outputWidth = Width, outputHeight = Height, cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount,
                serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription,
                platform = RuntimeInformation.RuntimeIdentifier, sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                managedRationalReferenceExact = true, nativeAlphaExact = nativeAlphaMaximum == 0, nativeAlphaMaximum, nativeAlphaRmse,
                nativeAlphaBounded16 = Depth == 16, nativeAlphaBoundedUp8 = Case == WarmBilinearCase.Up8, nativeZeroAlphaRgbZero = true,
                nativeRgbMaximum, nativeRgbRmse, nativePixelsDeterministic = true, nativeCliPixelsExact = true, nativeCliNormalScalarPixelsExact = true,
                nativeCliZeroAlphaNormalizationApplied = Case == WarmBilinearCase.Up8,
                nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmBilinearResize).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "One resident original 513x259 RGBA" + Depth + " raster to independently owned " + Width + "x" + Height + " bilinear output. Complete public Resize, admission, allocation/materialization/disposal. Native public FFmpeg filter graph includes packed/planar conversion, zscale widened triangle kernel with straight-alpha metadata and internal encoded-space premultiplication/unpremultiplication, final RGBA conversion and output pixel acquisition/release. Graph/source/frame wrappers and geometry contexts are reused; each call filters the full input. Native padded output rows exposed directly. Managed exact rational-reference pixels; native RGB maximum1/RMSE0.25 existing filtered comparison bound, Down8 alpha exact; approved RGBA16/Up8 native-only alpha maximum1/RMSE0.25; Up8 writable output acquisition and zero-alpha RGB normalization are timed; zero-alpha RGBzero; native CLI/deterministic output exact. No output cache or expression evaluator. Startup, IPC, I/O, generation, hashing and validation excluded; one CPU/filter thread, normal SIMD. Only this selected geometry and depth are measured."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
