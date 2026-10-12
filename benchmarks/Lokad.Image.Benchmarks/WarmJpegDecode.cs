using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmJpegDecodeCase { Decode8, NativeDecode8, NativeDecode420, NativeDecode422 }

internal sealed record WarmJpegSubsampledPreparation(int SchemaVersion, string Project, string Recipe,
    string RasterSha256, string NativeFile, int EncodedBytes, string EncodedSha256,
    string QuantizationSha256, string ManagedDecodedSha256, string FfmpegSha256);

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
    private string inputOrigin = "", sampling = "";
    private int scalarDecoderMaximum;
    private double scalarDecoderRmse;
    private JpegSampling Sampling => Case switch
    {
        WarmJpegDecodeCase.NativeDecode420 => JpegSampling.YCbCr420,
        WarmJpegDecodeCase.NativeDecode422 => JpegSampling.YCbCr422,
        _ => JpegSampling.YCbCr444
    };

    public static WarmJpegDecodeCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        return selection switch
        {
            null or "Decode8" => [WarmJpegDecodeCase.Decode8],
            "NativeDecode8" => [WarmJpegDecodeCase.NativeDecode8],
            "NativeDecode420" => [WarmJpegDecodeCase.NativeDecode420],
            "NativeDecode422" => [WarmJpegDecodeCase.NativeDecode422],
            _ => throw new ArgumentException("Choose one warm JPEG decode case: Decode8, NativeDecode8, NativeDecode420 or NativeDecode422.")
        };
    }

    // One explicit, untimed fixed-setting input preparation; no quality search.
    public static void Prepare420(int cpu) => PrepareSubsampled(cpu, JpegSampling.YCbCr420);
    public static void Prepare422(int cpu) => PrepareSubsampled(cpu, JpegSampling.YCbCr422);
    private static void PrepareSubsampled(int cpu, JpegSampling sampling)
    {
        if (!OperatingSystem.IsLinux() || cpu is < 0 or > 62) throw new ArgumentException("JPEG preparation requires Linux and one allowed CPU.");
        using var process = Process.GetCurrentProcess(); var oldAffinity = process.ProcessorAffinity;
        process.ProcessorAffinity = new IntPtr(1L << cpu);
        try
        {
            var label = sampling == JpegSampling.YCbCr420 ? "420" : "422";
            var display = sampling == JpegSampling.YCbCr420 ? "4:2:0" : "4:2:2";
            using var state = new JpegBenchmarkCase(); var pixels = HorizontalFlipInput.Create(8);
            File.WriteAllBytes(state.Input, pixels);
            var folder = Path.Combine(state.Root, "artifacts", "warm-reference", $"jpeg-decode{label}");
            Directory.CreateDirectory(folder); var input = Path.Combine(folder, "native.jpg");
            // The original asymmetric raster, alpha flattened onto RGB 0x204060,
            // area chroma reduction at the centered subsampled grid, fixed qscale 5.
            OracleProcess.Run(state.Ffmpeg, JpegBenchmarkCase.EncodeArguments(state.Input, input, 513, 259, 8, sampling, 5, 0x204060), []);
            var bytes = File.ReadAllBytes(input); var header = JpegQualitySyntax.Read(bytes);
            if ((header.Width, header.Height, header.Depth, header.Components) != (513, 259, 8, 3)
                || !JpegBenchmarkCase.MatchesSampling(header, sampling)) throw new InvalidOperationException($"Prepared JPEG {display} header differs.");
            using var decoded = ImageCodec.Decode(bytes); ValidateGeometry(decoded); var actual = OriginalPixels.Bytes(decoded);
            var arguments = JpegOracle.DecodeArguments(513, 259, sampling).ToList();
            arguments.InsertRange(arguments.IndexOf("-i"), ["-threads:v", "1", "-thread_type:v", "0"]);
            arguments.InsertRange(arguments.IndexOf("-filter_complex"), ["-filter_complex_threads", "1"]);
            ValidateAgreement(OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), bytes), actual);
            arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
            ValidateAgreement(OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), bytes), actual);
            if (!File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Preparation source changed.");
            var record = new WarmJpegSubsampledPreparation(1, "Lokad.Image", $"Original RGBA8 513x259 raster; native baseline {display}, alpha flatten RGB 0x204060, centered area chroma, optimal Huffman, fixed qscale 5; normal SIMD, one thread; no quality enumeration.",
                JpegBenchmarkCase.Hash(pixels), Path.GetRelativePath(state.Root, input), bytes.Length, JpegBenchmarkCase.Hash(bytes), header.QuantizationHash, JpegBenchmarkCase.Hash(actual), JpegBenchmarkCase.HashFile(state.Ffmpeg));
            File.WriteAllText(Path.Combine(state.Root, "artifacts", "warm-reference", $"jpeg-decode{label}.json"), JsonSerializer.Serialize(record, EncoderQualityCalibration.Json) + "\n");
            Console.WriteLine($"Prepared one fixed JPEG {display} decode input: {bytes.Length} bytes, SHA256 {record.EncodedSha256}.");
        }
        finally { process.ProcessorAffinity = oldAffinity; }
    }

    private static (int Maximum, double Rmse) ValidateAgreement(byte[] reference, byte[] actual)
    {
        var difference = JpegQualityCorpus.Compare(reference, actual);
        if (difference.Maximum > 4 || difference.Rmse > 1.5) throw new InvalidOperationException($"JPEG decoder agreement exceeds maximum 4/RMSE 1.5: {difference.Maximum}/{difference.Rmse}.");
        return (difference.Maximum, difference.Rmse);
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
            rasterHash = JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(8));
            string decodedHash;
            if (Case is WarmJpegDecodeCase.NativeDecode420 or WarmJpegDecodeCase.NativeDecode422)
            {
                var label = Sampling == JpegSampling.YCbCr420 ? "420" : "422";
                var prepared = JsonSerializer.Deserialize<WarmJpegSubsampledPreparation>(File.ReadAllText(Path.Combine(state.Root, "artifacts", "warm-reference", $"jpeg-decode{label}.json")), EncoderQualityCalibration.Json)
                    ?? throw new InvalidOperationException("Prepare one fixed subsampled JPEG input first.");
                encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.NativeFile)); decodedHash = prepared.ManagedDecodedSha256;
                if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.RasterSha256 != rasterHash
                    || encoded.Length != prepared.EncodedBytes || JpegBenchmarkCase.Hash(encoded) != prepared.EncodedSha256
                    || JpegQualitySyntax.Read(encoded).QuantizationHash != prepared.QuantizationSha256
                    || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256) throw new InvalidOperationException("Prepared subsampled JPEG identity differs.");
                inputOrigin = "prepared-native-fixed" + label;
            }
            else
            {
                var prepared = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllText(Path.Combine(state.Root, "artifacts", "warm-reference", "jpeg-match.json")), EncoderQualityCalibration.Json)
                    ?? throw new InvalidOperationException("Reuse the existing warm JPEG preparation.");
                var (file, selected) = Case == WarmJpegDecodeCase.Decode8 ? (prepared.ManagedFile, prepared.Match.ClosestOwned) : (prepared.NativeFile, prepared.Match.Native);
                encoded = File.ReadAllBytes(Path.Combine(state.Root, file)); decodedHash = selected.PublicDecodedSha256;
                if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.RasterSha256 != rasterHash
                    || encoded.Length != selected.EncodedBytes || JpegBenchmarkCase.Hash(encoded) != selected.EncodedSha256) throw new InvalidOperationException("Prepared JPEG identity differs.");
                inputOrigin = Case == WarmJpegDecodeCase.Decode8 ? "prepared-owned" : "prepared-native";
            }
            var header = JpegQualitySyntax.Read(encoded); sampling = header.Sampling;
            if (!JpegBenchmarkCase.MatchesSampling(header, Sampling))
                throw new InvalidOperationException("Prepared JPEG/input identity or sampling differs.");
            var info = ImageCodec.Identify(encoded);
            if ((info.Format, info.Width, info.Height, info.BitDepth) != (ImageFormat.Jpeg, 513, 259, 8))
                throw new InvalidOperationException("JPEG decode input geometry differs.");
            File.WriteAllBytes(state.Input, encoded);
            native = new NativeWarmReference(state, 8, cpu, Case == WarmJpegDecodeCase.NativeDecode420 ? "jpeg420-decode" : Case == WarmJpegDecodeCase.NativeDecode422 ? "jpeg422-decode" : "jpeg-decode");
            nativePixels = File.ReadAllBytes(state.NativeOutput);
            using (var output = ImageCodec.Decode(encoded)) { ValidateGeometry(output); managedPixels = OriginalPixels.Bytes(output); }
            if (JpegBenchmarkCase.Hash(managedPixels) != decodedHash)
                throw new InvalidOperationException("Prepared JPEG decoded pixels changed.");
            var difference = JpegQualityCorpus.Compare(nativePixels, managedPixels);
            decoderMaximum = difference.Maximum; decoderRmse = difference.Rmse;
            if (decoderMaximum > 4 || decoderRmse > 1.5) throw new InvalidOperationException("JPEG decoder agreement exceeds maximum 4/RMSE 1.5.");
            // Independently verify the adapter against the public CLI conversion,
            // outside timing and with normal native CPU dispatch.
            var oracle = JpegOracle.DecodeArguments(513, 259, Sampling).ToList();
            oracle.InsertRange(oracle.IndexOf("-i"), ["-threads:v", "1", "-thread_type:v", "0"]);
            oracle.InsertRange(oracle.IndexOf("-filter_complex"), ["-filter_complex_threads", "1"]);
            (scalarDecoderMaximum, scalarDecoderRmse) = ValidateAgreement(OracleProcess.Run(state.Ffmpeg, oracle.ToArray(), encoded), managedPixels);
            oracle.RemoveRange(oracle.IndexOf("-cpuflags"), 2);
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
                sourceIdentity, inputOrigin, sampling, inputBytes = encoded.Length,
                inputSha256 = JpegBenchmarkCase.Hash(encoded), rasterSha256 = rasterHash,
                managedOutputBytes = managedPixels.Length, nativeOutputBytes = nativePixels.Length,
                managedOutputSha256 = JpegBenchmarkCase.Hash(managedPixels), nativeOutputSha256 = JpegBenchmarkCase.Hash(nativePixels),
                alphaExact = true, decoderAgreementWithinBounds = true, decoderMaximum, decoderRmse, nativeCliPixelsExact = true,
                scalarCliAgreementWithinBounds = true, scalarDecoderMaximum, scalarDecoderRmse,
                nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmJpegDecode).Assembly.Location),
                nativeBuild = native.BuildIdentity, nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = "Identical resident prepared 8-bit JPEG to independent owned RGBA8. Public managed Decode, allocation and disposal; native software mjpeg decoder, per-operation state reset, full-range BT.601 conversion, owned output allocation and release. For subsampled inputs, expand both ceil-sized chroma planes by their sampling factors and crop to the original grid inside every operation, including intermediate acquisition/release. Native context/conversion/input packet/frame wrappers reused; every call decodes and converts all pixels. Padded native rows are exposed without a gratuitous packing copy. Startup, I/O, IPC, generation, hashing, checks and tight-row file writes excluded. No encoder quality search or additional source raster. Decoder RGB maximum 4/RMSE 1.5, exact opaque alpha."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); state?.Dispose(); ready = false; }
    }
}
