using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmLanczosCase { Reduce8, Reduce16, Enlarge8, Enlarge16 }

/// <summary>Complete Lanczos3 operations against the explicitly labeled FFmpeg scaler backend.</summary>
[MemoryDiagnoser]
public class WarmLanczosResize
{
    public static IEnumerable<WarmLanczosCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmLanczosCase Case { get; set; }
    public static WarmLanczosCase[] SelectedCases()
    {
        var names = (Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES") ?? "Reduce8").Split(',');
        if (names.Length is < 1 or > 4) throw new ArgumentException("Choose one to four distinct Lanczos3 cases.");
        var cases = names.Select(name => Enum.TryParse<WarmLanczosCase>(name, out var value) && Enum.IsDefined(value) && value.ToString() == name
            ? value : throw new ArgumentException("Choose a named Lanczos3 case.")).ToArray();
        if (cases.Distinct().Count() != cases.Length) throw new ArgumentException("Choose distinct Lanczos3 cases.");
        return cases;
    }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] input = [], expected = [];
    private string nativeHash = "";
    private JsonElement proof, sourceIdentity;
    private NativeWarmSample[] before = [];
    private int cpu, sampleCount;
    private bool smoke, ready;
    private int Depth => Case is WarmLanczosCase.Reduce16 or WarmLanczosCase.Enlarge16 ? 16 : 8;
    private int Width => Case is WarmLanczosCase.Enlarge8 or WarmLanczosCase.Enlarge16 ? 1027 : 256;
    private int Height => Width == 1027 ? 519 : 129;

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Use the dedicated Linux VM.");
        if (!Enum.IsDefined(Case)) throw new ArgumentOutOfRangeException(nameof(Case));
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        if (cpu is < 0 or > 62 || Process.GetCurrentProcess().ProcessorAffinity.ToInt64() != (1L << cpu)
            || Environment.ProcessorCount != 1 || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("Lanczos3 requires one CPU, optimized JIT and nonconcurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            var folder = Path.Combine(state.Root, "artifacts/warm-reference/lanczos3");
            using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(folder, "native-controls.json")));
            var controls = document.RootElement;
            if (!controls.GetProperty("allCasesPassed").GetBoolean() || !controls.GetProperty("noTimingClaim").GetBoolean()
                || controls.GetProperty("productionSha256").GetString() != JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location)
                || controls.GetProperty("workerSha256").GetString() != JpegBenchmarkCase.HashFile(Path.Combine(state.Root, "external/warm-reference/zimg-lanczos3-warm")))
                throw new InvalidOperationException("Prepare native quality controls for this immutable producer/worker first.");
            proof = controls.GetProperty("cases").EnumerateArray().Single(c => c.GetProperty("caseId").GetString() == Case.ToString()).Clone();
            if (!proof.GetProperty("independentDecimalBigIntegerExact").GetBoolean() || !proof.GetProperty("passQuality").GetBoolean()
                || proof.GetProperty("depth").GetInt32() != Depth || proof.GetProperty("outputWidth").GetInt32() != Width || proof.GetProperty("outputHeight").GetInt32() != Height)
                throw new InvalidOperationException("Lanczos3 prepared geometry/oracle differs.");
            var path = Path.Combine(state.Root, Depth == 8 ? "artifacts/warm-reference/webp-decode/source.rgba" : "artifacts/warm-reference/bilinear16-audit/input.rgba64le");
            input = File.ReadAllBytes(path); expected = File.ReadAllBytes(Path.Combine(folder, Case + ".raw"));
            if (JpegBenchmarkCase.Hash(input) != proof.GetProperty("inputSha256").GetString()
                || JpegBenchmarkCase.Hash(expected) != proof.GetProperty("outputSha256").GetString()) throw new InvalidOperationException("Lanczos3 fixture bytes differ.");
            File.WriteAllBytes(state.Input, input); source = ImageBuffer.Create(513, 259, Depth);
            for (var y = 0; y < 259; y++)
                if (Depth == 8) input.AsSpan(y * 513 * 4, 513 * 4).CopyTo(source.GetRow8(y));
                else for (var i = 0; i < 513 * 4; i++) source.GetRow16(y)[i] = BinaryPrimitives.ReadUInt16LittleEndian(input.AsSpan((y * 513 * 4 + i) * 2, 2));
            using (var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(state.Root, "tmp/benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new { sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(), sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString() });
            ValidateManaged();
            native = new NativeWarmReference(state, Depth, cpu, ResizeFilter.Lanczos3, Width, Height, Path.Combine(folder, Case + ".raw"));
            nativeHash = proof.GetProperty("modes").EnumerateArray().Single(m => m.GetProperty("mode").GetString() == "normal").GetProperty("outputSha256").GetString()!;
            native.Check(); RequireNativeOutput(); before = native.Measure(smoke, sampleCount: sampleCount); ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); state.Dispose(); throw; }
    }
    private void ValidateManaged()
    {
        using var first = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Lanczos3);
        using var second = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Lanczos3);
        if ((first.Width, first.Height, first.BitDepth, first.Orientation) != (Width, Height, Depth, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(first).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Lanczos3 managed pixels differ from the prepared independent oracle.");
        if (Depth == 8) first.GetRow8(0)[0] ^= 1; else first.GetRow16(0)[0] ^= 1;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(expected) || !OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Lanczos3 source/independent owners differ.");
    }
    private void RequireNativeOutput()
    {
        if (JpegBenchmarkCase.HashFile(state.NativeOutput) != nativeHash) throw new InvalidOperationException("Lanczos3 native pixels changed.");
    }
    [Benchmark]
    public int Managed()
    {
        using var result = ImageTransforms.Resize(source, Width, Height, ResizeFilter.Lanczos3);
        return result.Stride * result.Height;
    }
    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged(); var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); RequireNativeOutput(); native.Stop();
            var mode = proof.GetProperty("modes").EnumerateArray().Single(m => m.GetProperty("mode").GetString() == "normal");
            var folder = Path.Combine(state.Root, "artifacts/benchmark-validation"); Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, nameof(WarmLanczosResize) + "-" + Guid.NewGuid().ToString("N") + ".json"), JsonSerializer.Serialize(new {
                schemaVersion = 1, name = nameof(WarmLanczosResize), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = Depth, outputWidth = Width, outputHeight = Height,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC, sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected), nativeOutputSha256 = nativeHash,
                independentDecimalBigIntegerExact = true, nativeNormalScalarBounded = true, nativeZeroAlphaRgbZero = true, nativePixelsDeterministic = true,
                nativeRgbMaximum = mode.GetProperty("rgbMaximum").GetInt32(), nativeRgbRmse = mode.GetProperty("rgbRmse").GetDouble(),
                nativeAlphaMaximum = mode.GetProperty("alphaMaximum").GetInt32(), nativeAlphaRmse = mode.GetProperty("alphaRmse").GetDouble(),
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location), benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmLanczosResize).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "Complete public Lanczos3 Resize, local admission, independent output materialization and disposal on resident packed RGBA. "
                    + "FFmpeg zscale's zimg backend, radius3, encoded-space premultiplied filtering with unclipped float output. Timed packed input normalization and eight-pixel clamped halo, "
                    + "filtering, fresh planar/packed output ownership, unrounded-alpha division, quantization and release. Graph/input/scratch contexts reused; no converted input or output cache. "
                    + "Exact independent decimal/BigInteger managed pixels; native RGB/alpha max1/RMSE.25. One pinned CPU, normal SIMD, no concurrency. Startup/IPC/I/O/validation excluded."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); source?.Dispose(); state?.Dispose(); ready = false; }
    }
}
