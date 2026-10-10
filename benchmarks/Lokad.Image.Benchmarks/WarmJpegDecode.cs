using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmJpegDecodeCase { Decode8 }

[MemoryDiagnoser]
public class WarmJpegDecode
{
    public static IEnumerable<WarmJpegDecodeCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmJpegDecodeCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private byte[] encoded = [], managedPixels = [], nativePixels = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount, decoderMaximum;
    private double decoderRmse;
    private string rasterHash = "";

    public static WarmJpegDecodeCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        if (selection is not (null or "Decode8")) throw new ArgumentException("The warm JPEG decode family has one Decode8 case.");
        return [WarmJpegDecodeCase.Decode8];
    }

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm native JPEG decode requires Linux.");
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1
            || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("JPEG decode requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase();
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp", "benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new
                {
                    sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(),
                    sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString()
                });
            var prepared = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllText(Path.Combine(state.Root, "artifacts", "warm-reference", "jpeg-match.json")), EncoderQualityCalibration.Json)
                ?? throw new InvalidOperationException("Reuse the existing warm JPEG preparation.");
            rasterHash = JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(8));
            encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.ManagedFile));
            if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.RasterSha256 != rasterHash
                || JpegBenchmarkCase.Hash(encoded) != prepared.Match.ClosestOwned.EncodedSha256
                || !JpegBenchmarkCase.MatchesSampling(JpegQualitySyntax.Read(encoded), JpegSampling.YCbCr444))
                throw new InvalidOperationException("Prepared JPEG/input identity or sampling differs.");
            var info = ImageCodec.Identify(encoded);
            if ((info.Format, info.Width, info.Height, info.BitDepth) != (ImageFormat.Jpeg, 513, 259, 8))
                throw new InvalidOperationException("JPEG decode input geometry differs.");
            File.WriteAllBytes(state.Input, encoded);
            native = new NativeWarmReference(state, 8, cpu, "jpeg-decode");
            nativePixels = File.ReadAllBytes(state.NativeOutput);
            using (var output = ImageCodec.Decode(encoded)) { ValidateGeometry(output); managedPixels = OriginalPixels.Bytes(output); }
            var difference = JpegQualityCorpus.Compare(nativePixels, managedPixels);
            decoderMaximum = difference.Maximum; decoderRmse = difference.Rmse;
            if (decoderMaximum > 4 || decoderRmse > 1.5) throw new InvalidOperationException("JPEG decoder agreement exceeds maximum 4/RMSE 1.5.");
            // Independently verify the adapter against the public CLI conversion,
            // outside timing and with normal native CPU dispatch.
            var oracle = JpegOracle.DecodeArguments(513, 259, JpegSampling.YCbCr444).ToList();
            oracle.RemoveRange(oracle.IndexOf("-cpuflags"), 2);
            oracle.InsertRange(oracle.IndexOf("-i"), ["-threads:v", "1", "-thread_type:v", "0"]);
            oracle.InsertRange(oracle.IndexOf("-filter_complex"), ["-filter_complex_threads", "1"]);
            if (!OracleProcess.Run(state.Ffmpeg, oracle.ToArray(), encoded).AsSpan().SequenceEqual(nativePixels))
                throw new InvalidOperationException("Native warm JPEG decoder differs from the pinned public CLI.");
            ValidateManaged(); before = native.Measure(smoke, sampleCount: sampleCount); ready = true;
        }
        catch { native?.Dispose(); state.Dispose(); throw; }
    }

    private static void ValidateGeometry(ImageBuffer value)
    {
        if ((value.Width, value.Height, value.BitDepth, value.Orientation) != (513, 259, 8, ImageOrientation.Normal))
            throw new InvalidOperationException("JPEG decoded geometry/depth/orientation differs.");
    }
    private void ValidateManaged()
    {
        using var first = ImageCodec.Decode(encoded); using var second = ImageCodec.Decode(encoded);
        ValidateGeometry(first); ValidateGeometry(second);
        if (!OriginalPixels.Bytes(first).AsSpan().SequenceEqual(managedPixels) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(managedPixels))
            throw new InvalidOperationException("Repeated managed JPEG pixels differ.");
        first.GetRow8(0)[0] ^= 255;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(managedPixels) || !File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(encoded))
            throw new InvalidOperationException("Managed JPEG outputs alias or encoded source changed.");
    }

    [Benchmark]
    public int Managed()
    {
        using var result = ImageCodec.Decode(encoded);
        return result.Stride * result.Height ^ result.GetRow8(0)[513 * 2];
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged(); var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); native.Stop();
            if (!File.ReadAllBytes(state.NativeOutput).AsSpan().SequenceEqual(nativePixels)) throw new InvalidOperationException("Repeated native JPEG pixels differ.");
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmJpegDecode) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmJpegDecode), caseId = Case.ToString(), smoke,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"), width = 513, height = 259, depth = 8,
                cpu, managedAffinityCount = 1, processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC,
                gcConfiguration = GC.GetConfigurationVariables(), runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                sourceIdentity, inputSha256 = JpegBenchmarkCase.Hash(encoded), rasterSha256 = rasterHash,
                managedOutputBytes = managedPixels.Length, nativeOutputBytes = nativePixels.Length,
                managedOutputSha256 = JpegBenchmarkCase.Hash(managedPixels), nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                alphaExact = true, decoderAgreementWithinBounds = true, decoderMaximum, decoderRmse, nativeCliPixelsExact = true,
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpegDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "Identical resident prepared 8-bit 4:4:4 JPEG to independent owned RGBA8. Public managed Decode, allocation and disposal; native software mjpeg decoder, per-operation state reset, full-range BT.601 conversion, owned output allocation and release. Native context/conversion/input packet/frame wrappers reused; every call decodes and converts all pixels. Padded native rows are exposed without a gratuitous packing copy. Startup, I/O, IPC, generation, hashing, checks and tight-row file writes excluded. No encoder quality search or additional source raster. Decoder RGB maximum 4/RMSE 1.5, exact opaque alpha."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); state?.Dispose(); ready = false; }
    }
}
