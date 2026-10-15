using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmJpegDecodeCase { Decode8, NativeDecode8, NativeDecode420, NativeDecode422, OwnedDecodeGray, ProgressiveGray, Progressive420, Progressive444, Progressive422, NativeDecode420Restart, StreamDecode420Restart }

internal sealed record WarmJpegFixedPreparation(int SchemaVersion, string Project, string Recipe,
    string RasterSha256, string NativeFile, int EncodedBytes, string EncodedSha256,
    string QuantizationSha256, string ManagedDecodedSha256, string FfmpegSha256);

internal sealed record WarmJpegProgressivePreparation(WarmJpegFixedPreparation Input, string BaselineInputSha256,
    string CoefficientsSha256, JsonElement ProducerIdentity);

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
        WarmJpegDecodeCase.NativeDecode420 or WarmJpegDecodeCase.Progressive420 or WarmJpegDecodeCase.NativeDecode420Restart or WarmJpegDecodeCase.StreamDecode420Restart => JpegSampling.YCbCr420,
        WarmJpegDecodeCase.NativeDecode422 or WarmJpegDecodeCase.Progressive422 => JpegSampling.YCbCr422,
        WarmJpegDecodeCase.OwnedDecodeGray or WarmJpegDecodeCase.ProgressiveGray => JpegSampling.Grayscale,
        _ => JpegSampling.YCbCr444
    };
    private bool IsProgressive => Case is WarmJpegDecodeCase.ProgressiveGray or WarmJpegDecodeCase.Progressive420 or WarmJpegDecodeCase.Progressive444 or WarmJpegDecodeCase.Progressive422;
    private bool StreamInput => Case == WarmJpegDecodeCase.StreamDecode420Restart;
    private bool Restart => Case is WarmJpegDecodeCase.NativeDecode420Restart or WarmJpegDecodeCase.StreamDecode420Restart;

    public static WarmJpegDecodeCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        return selection switch
        {
            null or "Decode8" => [WarmJpegDecodeCase.Decode8],
            "NativeDecode8" => [WarmJpegDecodeCase.NativeDecode8],
            "NativeDecode420" => [WarmJpegDecodeCase.NativeDecode420],
            "NativeDecode422" => [WarmJpegDecodeCase.NativeDecode422],
            "OwnedDecodeGray" => [WarmJpegDecodeCase.OwnedDecodeGray],
            "ProgressiveGray" => [WarmJpegDecodeCase.ProgressiveGray],
            "Progressive420" => [WarmJpegDecodeCase.Progressive420],
            "Progressive444" => [WarmJpegDecodeCase.Progressive444],
            "Progressive422" => [WarmJpegDecodeCase.Progressive422],
            "NativeDecode420Restart" => [WarmJpegDecodeCase.NativeDecode420Restart],
            "StreamDecode420Restart" => [WarmJpegDecodeCase.StreamDecode420Restart],
            _ => throw new ArgumentException("Choose one warm JPEG decode case: Decode8, NativeDecode8, NativeDecode420, NativeDecode422, OwnedDecodeGray, ProgressiveGray, Progressive420, Progressive444, Progressive422, NativeDecode420Restart or StreamDecode420Restart.")
        };
    }

    // One explicit, untimed fixed-setting input preparation; no quality search.
    public static void PrepareProgressiveGray(int cpu) => PrepareProgressive(cpu, JpegSampling.Grayscale);
    public static void PrepareProgressive420(int cpu) => PrepareProgressive(cpu, JpegSampling.YCbCr420);
    public static void PrepareProgressive444(int cpu) => PrepareProgressive(cpu, JpegSampling.YCbCr444);
    public static void PrepareProgressive422(int cpu) => PrepareProgressive(cpu, JpegSampling.YCbCr422);
    private static void PrepareProgressive(int cpu, JpegSampling sampling)
    {
        if (!OperatingSystem.IsLinux() || cpu is < 0 or > 62) throw new ArgumentException("Progressive preparation requires Linux and one allowed CPU.");
        using var process = Process.GetCurrentProcess(); var oldAffinity = process.ProcessorAffinity;
        process.ProcessorAffinity = new IntPtr(1L << cpu);
        try
        {
            using var state = new JpegBenchmarkCase();
            var label = sampling == JpegSampling.Grayscale ? "gray" : sampling == JpegSampling.YCbCr444 ? "444" : sampling == JpegSampling.YCbCr422 ? "422" : "420";
            WarmJpegFixedPreparation baseline;
            if (sampling == JpegSampling.YCbCr444)
            {
                var match = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllText(Path.Combine(state.Root,"artifacts","warm-reference","jpeg-match.json")),EncoderQualityCalibration.Json)
                    ?? throw new InvalidOperationException("Reuse the existing native 4:4:4 preparation.");
                var baselineBytes = File.ReadAllBytes(Path.Combine(state.Root,match.NativeFile));
                baseline = new WarmJpegFixedPreparation(match.SchemaVersion,match.Project,"Existing fixed native 4:4:4 input",match.RasterSha256,
                    match.NativeFile,match.Match.Native.EncodedBytes,match.Match.Native.EncodedSha256,JpegQualitySyntax.Read(baselineBytes).QuantizationHash,
                    match.Match.Native.PublicDecodedSha256,match.FfmpegSha256);
            }
            else baseline = JsonSerializer.Deserialize<WarmJpegFixedPreparation>(File.ReadAllText(Path.Combine(state.Root,"artifacts","warm-reference",$"jpeg-decode{label}.json")),EncoderQualityCalibration.Json)
                ?? throw new InvalidOperationException("Prepare the existing fixed JPEG input first.");
            var original = Path.Combine(state.Root,baseline.NativeFile); var originalBytes = File.ReadAllBytes(original);
            if (baseline.SchemaVersion!=1 || baseline.Project!="Lokad.Image" || baseline.RasterSha256!=JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(8))
                || JpegBenchmarkCase.Hash(originalBytes)!=baseline.EncodedSha256) throw new InvalidOperationException("Original fixed input identity differs.");
            var tools = Path.Combine(state.Root,"external","warm-reference"); var jpegtran = Path.Combine(tools,"jpegtran");
            var visitor = Path.Combine(tools,"jpeg-coefficients");
            using var identityDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(tools,"jpegtran-identity.json")));
            var identity = identityDocument.RootElement.Clone();
            if (identity.GetProperty("revision").GetString()!="9217719d3a58633923b096af4c1d50d304768a64"
                || identity.GetProperty("jpegtranSha256").GetString()!=JpegBenchmarkCase.HashFile(jpegtran)
                || identity.GetProperty("coefficientVisitorSha256").GetString()!=JpegBenchmarkCase.HashFile(visitor)
                || identity.GetProperty("buildScriptSha256").GetString()!=JpegBenchmarkCase.HashFile(Path.Combine(state.Root,"tools","WarmReference","build-jpegtran-linux.sh"))
                || identity.GetProperty("coefficientVisitorSourceSha256").GetString()!=JpegBenchmarkCase.HashFile(Path.Combine(state.Root,"tools","WarmReference","jpeg-coefficients.c")))
                throw new InvalidOperationException("Progressive fixture producer identity differs.");
            var folder = Path.Combine(state.Root,"artifacts","warm-reference",$"jpeg-progressive-{label}"); Directory.CreateDirectory(folder);
            var input = Path.Combine(folder,"progressive.jpg");
            OracleProcess.Run(jpegtran,["-progressive","-copy","none","-outfile",input,original],[]);
            var before = Path.Combine(folder,"baseline.coefficients"); var after = Path.Combine(folder,"progressive.coefficients");
            OracleProcess.Run(visitor,[original,before],[]); OracleProcess.Run(visitor,[input,after],[]);
            if (!File.ReadAllBytes(before).AsSpan().SequenceEqual(File.ReadAllBytes(after))) throw new InvalidOperationException("Lossless progressive rewrite changed coefficient geometry, quantization or values.");
            var bytes = File.ReadAllBytes(input); var header = JpegQualitySyntax.Read(bytes);
            if ((header.Width,header.Height,header.Depth,header.Components)!=(513,259,8,sampling == JpegSampling.Grayscale ? 1 : 3)
                || header.FrameMarker!=0xC2 || JpegQualitySyntax.Read(originalBytes).FrameMarker!=0xC0
                || !JpegBenchmarkCase.MatchesSampling(header,sampling)
                || header.QuantizationHash!=baseline.QuantizationSha256) throw new InvalidOperationException("Progressive header/quantization differs.");
            using var decoded = ImageCodec.Decode(bytes); ValidateGeometry(decoded); var actual = OriginalPixels.Bytes(decoded);
            if (JpegBenchmarkCase.Hash(actual)!=baseline.ManagedDecodedSha256) throw new InvalidOperationException("Progressive owned pixels differ from the baseline.");
            ValidateAgreement(JpegOracle.Decode(bytes,513,259,sampling,state.Ffmpeg),actual);
            if (!File.ReadAllBytes(original).AsSpan().SequenceEqual(originalBytes)) throw new InvalidOperationException("Progressive preparation changed the original input.");
            var recipe = sampling == JpegSampling.Grayscale
                ? "Existing original-raster owned grayscale quality85/RGB204060 input; one default lossless jpegtran progressive rewrite; coefficients/quantization and owned decoded bytes exact; no scan or quality enumeration."
                : $"Existing original-raster native {label} qscale5/RGB204060 input; one default lossless jpegtran progressive rewrite; coefficient geometry/quantization and all valid coefficient values plus owned decoded bytes exact; no scan or quality enumeration.";
            var fixedInput = new WarmJpegFixedPreparation(1,"Lokad.Image",recipe,
                baseline.RasterSha256,Path.GetRelativePath(state.Root,input),bytes.Length,JpegBenchmarkCase.Hash(bytes),header.QuantizationHash,baseline.ManagedDecodedSha256,JpegBenchmarkCase.HashFile(state.Ffmpeg));
            var record = new WarmJpegProgressivePreparation(fixedInput,baseline.EncodedSha256,JpegBenchmarkCase.HashFile(before),identity);
            File.WriteAllText(Path.Combine(state.Root,"artifacts","warm-reference",$"jpeg-progressive-{label}.json"),JsonSerializer.Serialize(record,EncoderQualityCalibration.Json)+"\n");
            Console.WriteLine($"Prepared one progressive {label} decode input: {bytes.Length} bytes, SHA256 {fixedInput.EncodedSha256}; coefficient and owned pixel hashes preserved.");
        }
        finally { process.ProcessorAffinity = oldAffinity; }
    }

    public static void Prepare420(int cpu) => PrepareFixed(cpu, JpegSampling.YCbCr420);
    public static void Prepare422(int cpu) => PrepareFixed(cpu, JpegSampling.YCbCr422);
    public static void PrepareGray(int cpu) => PrepareFixed(cpu, JpegSampling.Grayscale);
    private static void PrepareFixed(int cpu, JpegSampling sampling)
    {
        if (!OperatingSystem.IsLinux() || cpu is < 0 or > 62) throw new ArgumentException("JPEG preparation requires Linux and one allowed CPU.");
        using var process = Process.GetCurrentProcess(); var oldAffinity = process.ProcessorAffinity;
        process.ProcessorAffinity = new IntPtr(1L << cpu);
        try
        {
            var label = sampling == JpegSampling.YCbCr420 ? "420" : sampling == JpegSampling.YCbCr422 ? "422" : "gray";
            var display = sampling == JpegSampling.YCbCr420 ? "4:2:0" : sampling == JpegSampling.YCbCr422 ? "4:2:2" : "grayscale";
            using var state = new JpegBenchmarkCase(); var pixels = HorizontalFlipInput.Create(8);
            File.WriteAllBytes(state.Input, pixels);
            var folder = Path.Combine(state.Root, "artifacts", "warm-reference", $"jpeg-decode{label}");
            Directory.CreateDirectory(folder); var input = Path.Combine(folder, sampling == JpegSampling.Grayscale ? "owned.jpg" : "native.jpg");
            // The original asymmetric raster, alpha flattened onto RGB 0x204060,
            // area chroma reduction at the centered subsampled grid, fixed qscale 5.
            if (sampling == JpegSampling.Grayscale)
            {
                // Native MJPEG cannot produce an equivalent one-component JPEG.
                // Use one fixed owned input; FFmpeg remains the decode oracle.
                using var source = JpegBenchmarkCase.Buffer(pixels, 513, 259, 8);
                using var output = new MemoryStream();
                ImageCodec.EncodeJpeg(source, output, new JpegEncodeOptions { Sampling = sampling, Quality = 85, BackgroundRgb = 0x204060 });
                File.WriteAllBytes(input, output.ToArray());
                if (!OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Owned preparation source changed.");
            }
            else OracleProcess.Run(state.Ffmpeg, JpegBenchmarkCase.EncodeArguments(state.Input, input, 513, 259, 8, sampling, 5, 0x204060), []);
            var bytes = File.ReadAllBytes(input); var header = JpegQualitySyntax.Read(bytes);
            if ((header.Width, header.Height, header.Depth, header.Components) != (513, 259, 8, sampling == JpegSampling.Grayscale ? 1 : 3)
                || !JpegBenchmarkCase.MatchesSampling(header, sampling)) throw new InvalidOperationException($"Prepared JPEG {display} header differs.");
            using var decoded = ImageCodec.Decode(bytes); ValidateGeometry(decoded); var actual = OriginalPixels.Bytes(decoded);
            var arguments = JpegOracle.DecodeArguments(513, 259, sampling).ToList();
            arguments.InsertRange(arguments.IndexOf("-i"), ["-threads:v", "1", "-thread_type:v", "0"]);
            arguments.InsertRange(arguments.IndexOf("-filter_complex"), ["-filter_complex_threads", "1"]);
            ValidateAgreement(OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), bytes), actual);
            arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
            ValidateAgreement(OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), bytes), actual);
            if (!File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Preparation source changed.");
            var recipe = sampling == JpegSampling.Grayscale
                ? "Original RGBA8 513x259 raster; owned baseline one-component grayscale JPEG, fixed quality 85, RGB 0x204060 alpha flatten; no quality enumeration. FFmpeg normal/scalar decode oracle."
                : $"Original RGBA8 513x259 raster; native baseline {display}, alpha flatten RGB 0x204060, centered area chroma, optimal Huffman, fixed qscale 5; normal SIMD, one thread; no quality enumeration.";
            var record = new WarmJpegFixedPreparation(1, "Lokad.Image", recipe,
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
            if (Restart)
            {
                var prepared = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllText(Path.Combine(state.Root,
                    "artifacts", "warm-reference", "jpeg420-restart33-adaptive-match.json")), EncoderQualityCalibration.Json)
                    ?? throw new InvalidOperationException("Reuse the validated row-restart encode preparation.");
                var anchor = JsonSerializer.Deserialize<WarmJpegPreparation>(File.ReadAllText(Path.Combine(state.Root,
                    "artifacts", "warm-reference", "jpeg420-adaptive-match.json")), EncoderQualityCalibration.Json)
                    ?? throw new InvalidOperationException("Original nonrestart anchor missing.");
                encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.NativeFile));
                decodedHash = prepared.Match.Native.PublicDecodedSha256;
                if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.RasterSha256 != rasterHash
                    || prepared.SourceDepth != 8 || prepared.NativeQuality != 5 || prepared.Sampling != "420"
                    || prepared.NativeReady.GetProperty("restartInterval").GetInt32() != 33
                    || prepared.NativeReady.GetProperty("restartMarkers").GetInt32() != 16
                    || encoded.Length != prepared.Match.Native.EncodedBytes || JpegBenchmarkCase.Hash(encoded) != prepared.Match.Native.EncodedSha256
                    || decodedHash != anchor.Match.Native.PublicDecodedSha256
                    || prepared.Match.Native.NativeDecodedSha256 != anchor.Match.Native.NativeDecodedSha256
                    || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256)
                    throw new InvalidOperationException("Restart decode packet/source/anchor identity differs.");
                WarmJpeg.ValidateRestartPacket(encoded); WarmJpeg.ValidateRestartNegativeControls(encoded);
                inputOrigin = "prepared-native-fixed420-restart33";
            }
            else if (Case is WarmJpegDecodeCase.NativeDecode420 or WarmJpegDecodeCase.NativeDecode422 or WarmJpegDecodeCase.OwnedDecodeGray or WarmJpegDecodeCase.ProgressiveGray or WarmJpegDecodeCase.Progressive420 or WarmJpegDecodeCase.Progressive444 or WarmJpegDecodeCase.Progressive422)
            {
                var label = Sampling == JpegSampling.YCbCr420 ? "420" : Sampling == JpegSampling.YCbCr422 ? "422" : Sampling == JpegSampling.YCbCr444 ? "444" : "gray";
                WarmJpegFixedPreparation prepared;
                if (IsProgressive)
                {
                    var progressive = JsonSerializer.Deserialize<WarmJpegProgressivePreparation>(File.ReadAllText(Path.Combine(state.Root,"artifacts","warm-reference",$"jpeg-progressive-{label}.json")),EncoderQualityCalibration.Json)
                        ?? throw new InvalidOperationException("Prepare the one lossless progressive input first.");
                    prepared = progressive.Input;
                    if (JpegBenchmarkCase.HashFile(Path.Combine(state.Root,"artifacts","warm-reference",$"jpeg-progressive-{label}","progressive.coefficients"))!=progressive.CoefficientsSha256)
                        throw new InvalidOperationException("Progressive coefficient proof identity differs.");
                }
                else prepared = JsonSerializer.Deserialize<WarmJpegFixedPreparation>(File.ReadAllText(Path.Combine(state.Root, "artifacts", "warm-reference", $"jpeg-decode{label}.json")), EncoderQualityCalibration.Json)
                    ?? throw new InvalidOperationException("Prepare one fixed JPEG input first.");
                encoded = File.ReadAllBytes(Path.Combine(state.Root, prepared.NativeFile)); decodedHash = prepared.ManagedDecodedSha256;
                if (prepared.SchemaVersion != 1 || prepared.Project != "Lokad.Image" || prepared.RasterSha256 != rasterHash
                    || encoded.Length != prepared.EncodedBytes || JpegBenchmarkCase.Hash(encoded) != prepared.EncodedSha256
                    || JpegQualitySyntax.Read(encoded).QuantizationHash != prepared.QuantizationSha256
                    || JpegBenchmarkCase.HashFile(state.Ffmpeg) != prepared.FfmpegSha256) throw new InvalidOperationException("Prepared fixed JPEG identity differs.");
                inputOrigin = IsProgressive ? (Sampling == JpegSampling.Grayscale ? "lossless-progressive-owned-gray" : "lossless-progressive-native-"+label) : Case == WarmJpegDecodeCase.OwnedDecodeGray ? "prepared-owned-gray" : "prepared-native-fixed" + label;
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
            if (!JpegBenchmarkCase.MatchesSampling(header, Sampling) || header.FrameMarker != (IsProgressive ? 0xC2 : 0xC0))
                throw new InvalidOperationException("Prepared JPEG/input identity or sampling differs.");
            var info = ImageCodec.Identify(encoded);
            if ((info.Format, info.Width, info.Height, info.BitDepth) != (ImageFormat.Jpeg, 513, 259, 8))
                throw new InvalidOperationException("JPEG decode input geometry differs.");
            File.WriteAllBytes(state.Input, encoded);
            native = new NativeWarmReference(state, 8, cpu, StreamInput ? "jpeg420-stream-decode" : Sampling == JpegSampling.YCbCr420 ? "jpeg420-decode" : Sampling == JpegSampling.YCbCr422 ? "jpeg422-decode" : Sampling == JpegSampling.Grayscale ? "jpeg-gray-decode" : "jpeg-decode");
            nativePixels = File.ReadAllBytes(state.NativeOutput);
            using (var output = DecodeManaged()) { ValidateGeometry(output); managedPixels = OriginalPixels.Bytes(output); }
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
        if (Restart) WarmJpeg.ValidateRestartPacket(encoded);
        using var first = DecodeManaged(); using var second = DecodeManaged();
        ValidateGeometry(first); ValidateGeometry(second);
        if (!OriginalPixels.Bytes(first).AsSpan().SequenceEqual(managedPixels) || !OriginalPixels.Bytes(second).AsSpan().SequenceEqual(managedPixels))
            throw new InvalidOperationException("Repeated managed JPEG pixels differ.");
        first.GetRow8(0)[0] ^= 255;
        if (!OriginalPixels.Bytes(second).AsSpan().SequenceEqual(managedPixels) || !File.ReadAllBytes(state.Input).AsSpan().SequenceEqual(encoded))
            throw new InvalidOperationException("Managed JPEG outputs alias or encoded source changed.");
        if (StreamInput)
        {
            using var caller = new MemoryStream(encoded, writable: false);
            if (caller.TryGetBuffer(out _)) throw new InvalidOperationException("Fresh JPEG input must not expose its buffer.");
            using var decoded = ImageCodec.Decode(caller);
            if (!caller.CanRead || caller.Position != caller.Length || caller.ReadByte() != -1
                || !OriginalPixels.Bytes(decoded).AsSpan().SequenceEqual(managedPixels))
                throw new InvalidOperationException("JPEG Stream pixels, complete reading or caller lifetime differ.");
        }
    }

    private ImageBuffer DecodeManaged()
    {
        if (!StreamInput) return ImageCodec.Decode(encoded);
        using var input = new MemoryStream(encoded, writable: false);
        return ImageCodec.Decode(input);
    }

    [Benchmark]
    public int Managed()
    {
        using var result = DecodeManaged();
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
                sourceIdentity, inputOrigin, sampling, progressive = IsProgressive, inputBytes = encoded.Length,
                restartInterval = Restart ? 33 : 0, restartMarkers = Restart ? 16 : 0,
                freshSeekableStream = StreamInput, inputView = false, inputCopiesTimed = StreamInput, callerInputLifetimeChecked = StreamInput,
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
                boundary = (StreamInput ? "Fresh non-exposed seekable JPEG Stream with complete input acquisition, all buffering/copies and EOF checks to independent owned RGBA8. Native fresh AVIO and owned packet acquisition/release execute every operation. " : "Identical resident prepared 8-bit JPEG to independent owned RGBA8. ") + "Public managed Decode, allocation and disposal; native software mjpeg decoder, per-operation state reset, full-range BT.601 conversion, owned output allocation and release. For subsampled inputs, expand both ceil-sized chroma planes by their sampling factors and crop to the original grid inside every operation, including intermediate acquisition/release. Grayscale includes expanding owned decoded GRAY8 samples into independent RGBA8 inside every operation. Native context/conversion/input packet/frame wrappers reused; every call decodes and converts all pixels. Padded native rows are exposed without a gratuitous packing copy. Startup, I/O, IPC, generation, hashing, checks and tight-row file writes excluded. No encoder quality search or additional source raster. Decoder RGB maximum 4/RMSE 1.5, exact opaque alpha."
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        finally { native?.Dispose(); state?.Dispose(); ready = false; }
    }
}
