using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum WarmPngCase { Encode8, Decode8, Decode16, Encode16, DecodeAdam7_16, DecodePaeth16, DecodePaeth8, DecodeRgb16, DecodeGray8, DecodeGrayAlpha8, DecodeRgb8, DecodeGray16, DecodeGrayAlpha16, DecodePalette8, DecodeGray1, DecodeGray4, DecodePalette4, DecodePalette2, DecodePalette1, DecodeGray2, DecodeUp8, DecodeSub8, DecodeAverage8, EncodeRgb8, EncodeRgb16, EncodeGray8, EncodeGray16, EncodeGrayAlpha8, EncodeGrayAlpha16, EncodeNoCompression8, DecodeNoCompression8, DecodePalette8PartialAlpha, DecodeGray8Transparency, DecodeGray16Transparency, DecodeRgb8Transparency, DecodeRgb16Transparency, EncodeFast16, EncodeNoCompression16, DecodeNoCompression16, DecodeTight16, DecodeCancelable16, DecodeTightCancelable16, DecodeSub16, DecodeAverage16, EncodeFastCancelable16, EncodeFastForward16, DecodeUp16 }

[MemoryDiagnoser]
public class WarmPng
{
    public static IEnumerable<WarmPngCase> Cases => SelectedCases();
    [ParamsSource(nameof(Cases))] public WarmPngCase Case { get; set; }
    private JpegBenchmarkCase state = null!;
    private NativeWarmReference native = null!;
    private ImageBuffer source = null!;
    private byte[] pixels = [], encoded = [];
    private NativeWarmSample[] before = [];
    private JsonElement sourceIdentity;
    private bool smoke, ready;
    private int cpu, sampleCount, managedOutputBytes, nativeOutputBytes;
    private DecodeOptions? decodeOptions;
    private long tightWorkingBytes;
    private CancellationTokenSource? cancellationSource;
    private CancellationToken decodeCancellation;
    private bool Cancelable => Case is WarmPngCase.DecodeCancelable16 or WarmPngCase.DecodeTightCancelable16;
    private bool Tight => Case is WarmPngCase.DecodeTight16 or WarmPngCase.DecodeTightCancelable16;
    private PngEncodeOptions encodeOptions => new() { ColorType = GrayAlpha ? PngColorType.GrayscaleAlpha : Gray ? PngColorType.Grayscale : Rgb ? PngColorType.Rgb : PngColorType.Rgba, Compression = Fastest ? CompressionLevel.Fastest : NoCompression ? CompressionLevel.NoCompression : CompressionLevel.Optimal };
    public static WarmPngCase[] SelectedCases()
    {
        var selection = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CASES");
        var result = selection is null ? new[] { WarmPngCase.Decode8 } : selection.Split(',')
            .Select(value => Enum.TryParse<WarmPngCase>(value, out var parsed) && Enum.IsDefined(parsed)
                ? parsed : throw new ArgumentException("Choose DecodeUp16, EncodeFastForward16, EncodeFastCancelable16, DecodeAverage16, DecodeSub16, DecodeTightCancelable16, DecodeCancelable16, DecodeTight16, DecodeNoCompression16, EncodeNoCompression16, EncodeFast16, Encode8, Encode16, EncodeRgb8, EncodeRgb16, EncodeGray8, EncodeGray16, EncodeGrayAlpha8, EncodeGrayAlpha16, EncodeNoCompression8, DecodeNoCompression8, DecodePalette8PartialAlpha, DecodeGray8Transparency, DecodeGray16Transparency, DecodeRgb8Transparency, DecodeRgb16Transparency, Decode8, Decode16, DecodeAdam7_16, DecodePaeth16, DecodePaeth8, DecodeUp8, DecodeSub8, DecodeAverage8, DecodeRgb16, DecodeGray8, DecodeGrayAlpha8, DecodeRgb8, DecodeGray16, DecodeGrayAlpha16, DecodePalette8, DecodeGray1, DecodeGray2, DecodeGray4, DecodePalette1, DecodePalette2 or DecodePalette4.")).ToArray();
        if (result.Length is < 1 or > 3 || result.Distinct().Count() != result.Length)
            throw new ArgumentException("Choose one to three distinct PNG cases.");
        return result;
    }
    private int Depth => Case is WarmPngCase.DecodeUp16 or WarmPngCase.EncodeFastForward16 or WarmPngCase.EncodeFastCancelable16 or WarmPngCase.DecodeAverage16 or WarmPngCase.DecodeSub16 or WarmPngCase.DecodeTightCancelable16 or WarmPngCase.DecodeCancelable16 or WarmPngCase.DecodeTight16 or WarmPngCase.DecodeNoCompression16 or WarmPngCase.EncodeNoCompression16 or WarmPngCase.EncodeFast16 or WarmPngCase.DecodeRgb16Transparency or WarmPngCase.DecodeGray16Transparency or WarmPngCase.EncodeGrayAlpha16 or WarmPngCase.EncodeGray16 or WarmPngCase.Decode16 or WarmPngCase.Encode16 or WarmPngCase.EncodeRgb16 or WarmPngCase.DecodeAdam7_16 or WarmPngCase.DecodePaeth16 or WarmPngCase.DecodeRgb16 or WarmPngCase.DecodeGray16 or WarmPngCase.DecodeGrayAlpha16 ? 16 : 8;
    private bool RgbTransparency => Case is WarmPngCase.DecodeRgb8Transparency or WarmPngCase.DecodeRgb16Transparency;
    private bool Rgb => Case is WarmPngCase.DecodeRgb16Transparency or WarmPngCase.DecodeRgb8Transparency or WarmPngCase.DecodeRgb16 or WarmPngCase.DecodeRgb8 or WarmPngCase.EncodeRgb8 or WarmPngCase.EncodeRgb16;
    private bool Mono => Case is WarmPngCase.DecodeGray1 or WarmPngCase.DecodeGray2 or WarmPngCase.DecodeGray4;
    private int CodedDepth => Case is WarmPngCase.DecodeGray4 or WarmPngCase.DecodePalette4 ? 4 : Case is WarmPngCase.DecodePalette2 or WarmPngCase.DecodeGray2 ? 2 : Mono || Case == WarmPngCase.DecodePalette1 ? 1 : Depth;
    private bool GrayTransparency => Case is WarmPngCase.DecodeGray8Transparency or WarmPngCase.DecodeGray16Transparency;
    private ushort GrayTransparencyKey => WarmPngGrayTransparencyInput.KeyFor(Depth);
    private bool Gray => Case is WarmPngCase.DecodeGray16Transparency or WarmPngCase.DecodeGray8Transparency or WarmPngCase.DecodeGray8 or WarmPngCase.DecodeGray16 or WarmPngCase.DecodeGray1 or WarmPngCase.DecodeGray2 or WarmPngCase.DecodeGray4 or WarmPngCase.EncodeGray8 or WarmPngCase.EncodeGray16;
    private bool GrayAlpha => Case is WarmPngCase.DecodeGrayAlpha8 or WarmPngCase.DecodeGrayAlpha16 or WarmPngCase.EncodeGrayAlpha8 or WarmPngCase.EncodeGrayAlpha16;
    private bool PartialAlpha => Case == WarmPngCase.DecodePalette8PartialAlpha;
    private bool Palette => Case is WarmPngCase.DecodePalette8PartialAlpha or WarmPngCase.DecodePalette8 or WarmPngCase.DecodePalette4 or WarmPngCase.DecodePalette2 or WarmPngCase.DecodePalette1;
    private int ColorType => Palette ? 3 : GrayAlpha ? 4 : Gray ? 0 : Rgb ? 2 : 6;
    private bool NoCompression => Case is WarmPngCase.DecodeNoCompression16 or WarmPngCase.EncodeNoCompression16 or WarmPngCase.EncodeNoCompression8 or WarmPngCase.DecodeNoCompression8;
    private bool Fastest => Case is WarmPngCase.EncodeFastForward16 or WarmPngCase.EncodeFast16 or WarmPngCase.EncodeFastCancelable16;
    private bool ForwardOutput => Case == WarmPngCase.EncodeFastForward16;
    private bool EncodeCancelable => Case == WarmPngCase.EncodeFastCancelable16;
    private CancellationToken EncodeCancellation => EncodeCancelable ? decodeCancellation : default;
    private int EncoderLevel => Fastest ? 1 : NoCompression ? 0 : 6;
    private bool MatchedByteEncoding => Encode && Depth == 8 && ColorType is 0 or 2 or 4;
    private int ManagedEncoderLevel => Fastest ? 2 : MatchedByteEncoding ? 9 : EncoderLevel;
    private bool Encode => Case is WarmPngCase.EncodeFastForward16 or WarmPngCase.EncodeFastCancelable16 or WarmPngCase.EncodeNoCompression16 or WarmPngCase.EncodeFast16 or WarmPngCase.EncodeNoCompression8 or WarmPngCase.EncodeGrayAlpha16 or WarmPngCase.EncodeGrayAlpha8 or WarmPngCase.Encode8 or WarmPngCase.Encode16 or WarmPngCase.EncodeRgb8 or WarmPngCase.EncodeRgb16 or WarmPngCase.EncodeGray8 or WarmPngCase.EncodeGray16;
    private bool Interlaced => Case == WarmPngCase.DecodeAdam7_16;
    private bool Paeth => Case is WarmPngCase.DecodePaeth16 or WarmPngCase.DecodePaeth8;
    private bool Up => Case is WarmPngCase.DecodeUp8 or WarmPngCase.DecodeUp16;
    private bool Sub => Case is WarmPngCase.DecodeSub8 or WarmPngCase.DecodeSub16;
    private bool Average => Case is WarmPngCase.DecodeAverage8 or WarmPngCase.DecodeAverage16;
    private int Filter => Paeth ? 4 : Up ? 2 : Sub ? 1 : Average ? 3 : 0;
    private bool Prepared => Interlaced || Paeth;

    [GlobalSetup]
    public void Setup()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Warm native PNG runs require Linux.");
        cpu = int.Parse(Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_CPU") ?? throw new InvalidOperationException("Missing warm CPU."));
        smoke = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "smoke";
        sampleCount = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_MODE") == "confirm" ? 12 : 3;
        using var process = Process.GetCurrentProcess();
        if (cpu is < 0 or > 62 || process.ProcessorAffinity.ToInt64() != (1L << cpu) || Environment.ProcessorCount != 1
            || GCSettings.IsServerGC || Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") != "0"
            || !GC.GetConfigurationVariables().TryGetValue("ConcurrentGC", out var concurrent) || Convert.ToBoolean(concurrent))
            throw new InvalidOperationException("PNG worker requires one CPU, optimized JIT and non-concurrent workstation GC.");
        state = new JpegBenchmarkCase(Encode ? ".png" : "");
        try
        {
            using (var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(state.Root, "tmp", "benchmark-source.json"))))
                sourceIdentity = JsonSerializer.SerializeToElement(new
                {
                    sourceSha256 = manifest.RootElement.GetProperty("sourceSha256").GetString(),
                    sourceRevision = manifest.RootElement.GetProperty("sourceRevision").GetString()
                });
            pixels = HorizontalFlipInput.Create(Depth);
            if (Mono) WarmPngMonoInput.Project(pixels, CodedDepth);
            if (Palette) WarmPngPaletteInput.Project(pixels, CodedDepth, PartialAlpha);
            if (Rgb)
                for (var i = 3 * Depth / 8; i < pixels.Length; i += 4 * Depth / 8)
                    pixels.AsSpan(i, Depth / 8).Fill(255);
            if (Gray || GrayAlpha)
                for (var i = 0; i < pixels.Length; i += 4 * Depth / 8)
                {
                    var gray = pixels.AsSpan(i, Depth / 8);
                    gray.CopyTo(pixels.AsSpan(i + Depth / 8, Depth / 8));
                    gray.CopyTo(pixels.AsSpan(i + 2 * Depth / 8, Depth / 8));
                    if (Gray) pixels.AsSpan(i + 3 * Depth / 8, Depth / 8).Fill(255);
                }
            if (GrayTransparency) WarmPngGrayTransparencyInput.Project(pixels, Depth);
            if (RgbTransparency) WarmPngRgbTransparencyInput.Project(pixels, Depth);
            source = JpegBenchmarkCase.Buffer(pixels, HorizontalFlipInput.Width, HorizontalFlipInput.Height, Depth);
            if (EncodeCancelable)
            {
                cancellationSource = new CancellationTokenSource(); decodeCancellation = cancellationSource.Token;
                using var canceled = new CancellationTokenSource(); canceled.Cancel(); using var rejected = new MemoryStream();
                try { ImageCodec.EncodePng(source, rejected, encodeOptions, canceled.Token); throw new InvalidOperationException("PNG accepted a pre-canceled encode token."); }
                catch (OperationCanceledException error) when (error.CancellationToken == canceled.Token && rejected.Length == 0) { }
            }
            if (NoCompression && !Encode)
            {
                var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "png-store-" + Depth);
                encoded = File.ReadAllBytes(Path.Combine(directory, "input.png"));
                if (JpegBenchmarkCase.Hash(encoded) != (Depth == 16 ? "98b4c931725ffce5ac6ca86f42d2e2d44ca6f32ceac2463fcc4f63a24ae3b405" : "3adfd36021b7926dff47683690d7333871d669e99cf4dea50774d72187c3e03e")
                    || !File.ReadAllBytes(Path.Combine(directory, "expected.raw")).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Frozen stored PNG input differs.");
            }
            else if (Prepared)
            {
                var directory = Path.Combine(state.Root, "artifacts", "warm-reference", Paeth ? "png-paeth-" + Depth : "png-adam7-16");
                using var prepared = JsonDocument.Parse(File.ReadAllBytes(directory + ".json"));
                encoded = File.ReadAllBytes(Path.Combine(directory, "input.png"));
                if (prepared.RootElement.GetProperty("inputSha256").GetString() != JpegBenchmarkCase.Hash(encoded)
                    || prepared.RootElement.GetProperty("rasterSha256").GetString() != JpegBenchmarkCase.Hash(pixels)
                    || !File.ReadAllBytes(Path.Combine(directory, "expected.raw")).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Prepared PNG input/raster differs.");
            }
            else if (RgbTransparency) encoded = WarmPngRgbTransparencyInput.Build(pixels, Depth);
            else if (GrayTransparency) encoded = WarmPngGrayTransparencyInput.Build(pixels, Depth);
            else if (Up) encoded = WarmPngUpInput.Build(pixels, Depth);
            else if (Sub) encoded = WarmPngSubInput.Build(pixels, Depth);
            else if (Average) encoded = WarmPngAverageInput.Build(pixels, Depth);
            else if (Mono) encoded = WarmPngMonoInput.Build(pixels, CodedDepth);
            else if (Palette) encoded = WarmPngPaletteInput.Build(pixels, CodedDepth, PartialAlpha);
            else if (Case is WarmPngCase.DecodeRgb8 or WarmPngCase.DecodeGray8 or WarmPngCase.DecodeGrayAlpha8)
            {
                // Decoder workload bytes remain frozen when encoder policy changes.
                var name = Rgb ? "png-rgb8" : GrayAlpha ? "png-gray-alpha8" : "png-gray8";
                encoded = File.ReadAllBytes(Path.Combine(state.Root, "artifacts", "warm-reference", name, "input.png"));
                var expectedHash = Rgb ? "ad3af25e172e383b77969917599c4a66cea2581dd64b0cd13f0610ac54816855"
                    : GrayAlpha ? "0bfdcc9aa57f0b6009a3ca31b1ab200d2d1bb59c1395495afaad584c5f6e630d"
                    : "8990a1cb03abac292d32deb02a97003b17b7b8347afe02e8370194de01bbc235";
                if (JpegBenchmarkCase.Hash(encoded) != expectedHash) throw new InvalidOperationException("Frozen byte PNG decoder input changed.");
            }
            else using (var output = new MemoryStream())
            {
                EncodeInto(output); encoded = output.ToArray();
            }
            if (Cancelable)
            {
                if (JpegBenchmarkCase.Hash(encoded) != "3e4b64d778c395392451d5331d2a11b541d262e2f39826e1322d22d6f03bb4a8")
                    throw new InvalidOperationException("Frozen original word PNG changed.");
                cancellationSource = new CancellationTokenSource();
                decodeCancellation = cancellationSource.Token;
                using var canceled = new CancellationTokenSource();
                canceled.Cancel();
                try
                {
                    using var rejected = ImageCodec.Decode(encoded, cancellationToken: canceled.Token);
                    throw new InvalidOperationException("PNG accepted an already-canceled token.");
                }
                catch (OperationCanceledException exception) when (exception.CancellationToken == canceled.Token) { }
            }
            if (Tight)
            {
                if (JpegBenchmarkCase.Hash(encoded) != "3e4b64d778c395392451d5331d2a11b541d262e2f39826e1322d22d6f03bb4a8")
                    throw new InvalidOperationException("Frozen original word PNG changed.");
                tightWorkingBytes = encoded.LongLength + IdatBytes(encoded) + pixels.LongLength + 256 * 1024L + source.Stride * 2L;
                decodeOptions = new DecodeOptions { Limits = new ImageLimits { MaxWorkingBytes = tightWorkingBytes } };
                try
                {
                    using var rejected = ImageCodec.Decode(encoded,
                        decodeOptions with { Limits = decodeOptions.Limits with { MaxWorkingBytes = tightWorkingBytes - 1 } }, decodeCancellation);
                    throw new InvalidOperationException("PNG decoded below the exact working boundary.");
                }
                catch (ImageLimitException) { }
            }
            if (ForwardOutput && !File.ReadAllBytes(Path.Combine(state.Root, "artifacts", "warm-reference", "png-fast16-fast2", "input.png")).AsSpan().SequenceEqual(encoded))
                throw new InvalidOperationException("Forward-only output differs from the frozen stock packet.");
            ValidatePng(encoded); ValidateManaged();
            if (Cancelable || Tight || Fastest || NoCompression || Rgb || Gray || GrayAlpha || Palette || Up || Sub || Average)
            {
                var directory = Path.Combine(state.Root, "artifacts", "warm-reference", Tight && Cancelable ? "png-tight-cancel16" : Cancelable ? "png-cancel16" : Tight ? "png-tight16" : Fastest ? "png-fast16-fast2" + (EncodeCancelable ? "-cancel" : ForwardOutput ? "-forward" : "") : RgbTransparency ? "png-rgb" + Depth + "-trns" : GrayTransparency ? Depth == 16 ? "png-gray16-trns32639" : "png-gray8-trns127" : NoCompression ? "png-store-" + Depth : Up ? "png-up-" + Depth : Sub ? "png-sub-" + Depth : Average ? "png-average-" + Depth : Mono ? "png-gray" + CodedDepth : PartialAlpha ? "png-palette8-alpha2" : Palette ? "png-palette" + CodedDepth : GrayAlpha ? "png-gray-alpha" + Depth : Gray ? "png-gray" + Depth : "png-rgb" + Depth);
                if (MatchedByteEncoding) directory += "-optimal9";
                Directory.CreateDirectory(directory);
                foreach (var (name, bytes) in new[] { ("input.png", encoded), ("expected.raw", pixels) })
                {
                    var path = Path.Combine(directory, name);
                    if (File.Exists(path))
                    {
                        if (!File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes))
                            throw new InvalidOperationException("Frozen projected PNG input differs; preserve it before changing preparation.");
                    }
                    else File.WriteAllBytes(path, bytes);
                }
            }
            var rawPath = Path.Combine(state.DirectoryPath, "expected.raw");
            File.WriteAllBytes(rawPath, pixels); File.WriteAllBytes(state.Input, Encode ? pixels : encoded);
            native = new NativeWarmReference(state, Depth, cpu, Encode ? Fastest ? "png-fast16-encode" : NoCompression ? Depth == 16 ? "png-store16-encode" : "png-store-encode" : GrayAlpha ? "png-gray-alpha-encode" : Rgb ? "png-rgb-encode" : Gray ? "png-gray-encode" : "png-encode" : Interlaced ? "png-adam7-decode" : "png-decode", rawPath);
            if (native.Ready.GetProperty("colorType").GetInt32() != ColorType
                || native.Ready.GetProperty("codedDepth").GetInt32() != CodedDepth
                || native.Ready.GetProperty("conversionThreads").GetInt32() != (Encode && GrayAlpha || RgbTransparency ? 0 : Rgb || Gray || GrayAlpha || Palette ? 1 : 0)
                || native.Ready.GetProperty("rgbExpansionTimed").GetBoolean() != (Rgb && !Encode && !RgbTransparency)
                || native.Ready.GetProperty("rgbPackingTimed").GetBoolean() != (Rgb && Encode)
                || native.Ready.GetProperty("grayExpansionTimed").GetBoolean() != (Gray && !Encode)
                || native.Ready.GetProperty("grayPackingTimed").GetBoolean() != (Gray && Encode)
                || native.Ready.GetProperty("grayTransparencyExpansionTimed").GetBoolean() != GrayTransparency
                || native.Ready.GetProperty("rgbTransparencyExpansionTimed").GetBoolean() != RgbTransparency
                || native.Ready.GetProperty("grayPlaneExtractionTimed").GetBoolean() != (Gray && Encode && Depth == 16)
                || native.Ready.GetProperty("grayFilterOutputAllocationTimed").GetBoolean() != (Gray && Encode && Depth == 16)
                || native.Ready.GetProperty("grayAlphaExpansionTimed").GetBoolean() != (GrayAlpha && !Encode)
                || native.Ready.GetProperty("grayAlphaPackingTimed").GetBoolean() != (GrayAlpha && Encode)
                || native.Ready.GetProperty("grayAlphaStoragePackingTimed").GetBoolean() != (GrayAlpha && Encode)
                || native.Ready.GetProperty("paletteExpansionTimed").GetBoolean() != Palette)
                throw new InvalidOperationException("Native PNG color/expansion boundary differs.");
            ValidateNative();
            if (Fastest)
            {
                var file = Path.Combine(state.Root, "artifacts", "warm-reference", "png-fast16-fast2" + (EncodeCancelable ? "-cancel" : ForwardOutput ? "-forward" : ""), "native.png");
                var bytes = File.ReadAllBytes(state.NativeOutput);
                if (File.Exists(file))
                {
                    if (!File.ReadAllBytes(file).AsSpan().SequenceEqual(bytes)) throw new InvalidOperationException("Frozen Fastest native PNG changed.");
                }
                else File.WriteAllBytes(file, bytes);
            }
            if (!Encode)
            {
                var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-cpuflags", "0",
                    "-noautorotate", "-f", "image2pipe", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1",
                    "-pix_fmt", Depth == 16 ? "rgba64le" : "rgba", "-f", "rawvideo", "pipe:1" };
                if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), encoded).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Scalar native CLI PNG samples differ.");
                arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
                if (!OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), encoded).AsSpan().SequenceEqual(pixels))
                    throw new InvalidOperationException("Normal native CLI PNG samples differ.");
            }
            if (Encode)
            {
                var arguments = new List<string> { "-v", "error", "-threads", "1", "-filter_threads", "1", "-cpuflags", "0",
                    "-f", "rawvideo", "-pix_fmt", GrayAlpha ? Depth == 16 ? "ya16be" : "ya8" : Depth == 16 ? "rgba64le" : "rgba", "-s", "513x259", "-i", "pipe:0",
                    "-frames:v", "1", "-c:v", "png", "-threads", "1", "-compression_level", EncoderLevel.ToString(), "-pred", "none",
                    "-pix_fmt", GrayAlpha ? Depth == 16 ? "ya16be" : "ya8" : Gray ? Depth == 16 ? "gray16be" : "gray" : Rgb ? Depth == 16 ? "rgb48be" : "rgb24" : Depth == 16 ? "rgba64be" : "rgba", "-f", "image2pipe", "pipe:1" };
                if (Gray && Depth == 16) arguments.InsertRange(arguments.IndexOf("-frames:v"), ["-vf", "extractplanes=r,scale=iw:ih:flags=neighbor+bitexact:threads=1,format=gray16be"]);
                var oracleInput = pixels;
                if (GrayAlpha)
                {
                    oracleInput = new byte[pixels.Length / 2];
                    var sampleBytes = Depth / 8;
                    for (var i = 0; i < pixels.Length / (4 * sampleBytes); i++)
                    for (var b = 0; b < sampleBytes; b++)
                    {
                        var sourceByte = Depth == 16 ? b ^ 1 : b;
                        oracleInput[(i * 2) * sampleBytes + b] = pixels[(i * 4) * sampleBytes + sourceByte];
                        oracleInput[(i * 2 + 1) * sampleBytes + b] = pixels[(i * 4 + 3) * sampleBytes + sourceByte];
                    }
                }
                var scalar = OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), oracleInput);
                ValidatePng(scalar, nativePacket: true);
                using (var decoded = ImageCodec.Decode(scalar)) ValidatePixels(decoded);
                arguments.RemoveRange(arguments.IndexOf("-cpuflags"), 2);
                var normal = OracleProcess.Run(state.Ffmpeg, arguments.ToArray(), oracleInput);
                ValidatePng(normal, nativePacket: true);
                using (var decoded = ImageCodec.Decode(normal)) ValidatePixels(decoded);
            }
            before = native.Measure(smoke, sampleCount: sampleCount);
            ready = true;
        }
        catch { native?.Dispose(); source?.Dispose(); cancellationSource?.Dispose(); state.Dispose(); throw; }
    }

    private void ValidatePixels(ImageBuffer value)
    {
        if ((value.Width, value.Height, value.BitDepth, value.Orientation) != (source.Width, source.Height, Depth, ImageOrientation.Normal)
            || !OriginalPixels.Bytes(value).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("PNG samples/precision/orientation differ, including zero-alpha hidden RGB.");
    }
    private void EncodeInto(MemoryStream output)
    {
        if (ForwardOutput)
        {
            using var forward = new ForwardOnlyPngOutput(output);
            ImageCodec.EncodePng(source, forward, encodeOptions, EncodeCancellation);
        }
        else ImageCodec.EncodePng(source, output, encodeOptions, EncodeCancellation);
    }

    private void ValidateManaged()
    {
        if ((Cancelable || EncodeCancelable) && (!decodeCancellation.CanBeCanceled || decodeCancellation.IsCancellationRequested))
            throw new InvalidOperationException("Warm operation requires a live, never-signaled cancelable token.");
        if (Encode)
        {
            using var first = new MemoryStream(); using var second = new MemoryStream();
            EncodeInto(first); EncodeInto(second);
            if (!first.CanWrite || !second.CanWrite || ReferenceEquals(first.GetBuffer(), second.GetBuffer()) || !first.ToArray().AsSpan().SequenceEqual(encoded)
                || !second.ToArray().AsSpan().SequenceEqual(encoded)) throw new InvalidOperationException("Managed PNG results are not independent/deterministic.");
            managedOutputBytes = checked((int)first.Length);
        }
        else
        {
            using var first = ImageCodec.Decode(encoded, decodeOptions, decodeCancellation); using var second = ImageCodec.Decode(encoded, decodeOptions, decodeCancellation);
            ValidatePixels(first); ValidatePixels(second);
            if (Depth == 8) first.GetRow8(0)[0] ^= 255; else first.GetRow16(0)[0] ^= 65535;
            ValidatePixels(second); managedOutputBytes = pixels.Length;
        }
        if (!OriginalPixels.Bytes(source).AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Managed source changed.");
    }
    private void ValidateNative()
    {
        var output = File.ReadAllBytes(state.NativeOutput); nativeOutputBytes = output.Length;
        if (Encode)
        {
            ValidatePng(output, nativePacket: true);
            using var decoded = ImageCodec.Decode(output); ValidatePixels(decoded);
            // Pin declared backend levels/filter and retain size evidence. A large payload
            // discrepancy is a preparation mismatch rather than a scored speed win.
            var nativeIdat = IdatBytes(output); var managedIdat = IdatBytes(encoded);
            if (Math.Abs(nativeIdat - (double)managedIdat) / Math.Max(nativeIdat, managedIdat) > 0.05)
                throw new InvalidOperationException("PNG compressed payload sizes differ by more than 5%; resolve effort matching before timing.");
        }
        else if (!output.AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Native owned RGBA pixels differ.");
    }
    private static int IdatBytes(byte[] bytes)
    {
        var total = 0;
        for (var position = 8; position < bytes.Length;)
        {
            var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(position)));
            if (bytes.AsSpan(position + 4, 4).SequenceEqual("IDAT"u8)) total = checked(total + length);
            position = checked(position + length + 12);
        }
        return total;
    }
    private void ValidatePng(byte[] bytes, bool nativePacket = false)
    {
        var info = ImageCodec.Identify(bytes);
        if (info.Format != ImageFormat.Png || info.Width != source.Width || info.Height != source.Height || info.BitDepth != CodedDepth
            || bytes[25] != ColorType || bytes[28] != (Interlaced ? 1 : 0)) throw new InvalidOperationException("PNG profile differs.");
        using var compressed = new MemoryStream();
        for (var position = 8; position < bytes.Length;)
        {
            var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(position)));
            if (bytes.AsSpan(position + 4, 4).SequenceEqual("IDAT"u8)) compressed.Write(bytes.AsSpan(position + 8, length));
            position = checked(position + length + 12);
        }
        if (Fastest && (compressed.Length < 2 || compressed.GetBuffer()[1] >> 6 != (nativePacket ? 0 : 1))) throw new InvalidOperationException("PNG Fastest header differs from the declared numeric backend class.");
        if (NoCompression) WarmPngStoredInput.Validate(compressed.ToArray(), pixels, Depth);
        if (GrayTransparency)
        {
            var count = 0;
            for (var position = 8; position < bytes.Length;)
            {
                var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(position)));
                if (bytes.AsSpan(position + 4, 4).SequenceEqual("tRNS"u8))
                {
                    if (length != 2 || BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(position + 8, 2)) != GrayTransparencyKey)
                        throw new InvalidOperationException("Authored gray tRNS key differs.");
                    count++;
                }
                position = checked(position + length + 12);
            }
            if (count != 1 || !info.HasAlpha) throw new InvalidOperationException("Expected one gray tRNS key with alpha.");
        }
        if (RgbTransparency)
        {
            var count = 0;
            for (var position = 8; position < bytes.Length;)
            {
                var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(position)));
                if (bytes.AsSpan(position + 4, 4).SequenceEqual("tRNS"u8))
                {
                    if (length != 6 || !bytes.AsSpan(position + 8, 6).SequenceEqual(WarmPngRgbTransparencyInput.KeyFor(Depth)))
                        throw new InvalidOperationException("Authored RGB tRNS key differs.");
                    count++;
                }
                position = checked(position + length + 12);
            }
            if (count != 1 || !info.HasAlpha) throw new InvalidOperationException("Expected one RGB tRNS key with alpha.");
        }
        if (Palette)
        {
            var paletteBytes = 0; var alphaBytes = 0;
            for (var position = 8; position < bytes.Length;)
            {
                var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(position)));
                if (bytes.AsSpan(position + 4, 4).SequenceEqual("PLTE"u8)) paletteBytes = length;
                if (bytes.AsSpan(position + 4, 4).SequenceEqual("tRNS"u8))
                {
                    alphaBytes = length;
                    for (var i = 0; i < length; i++)
                        if (bytes[position + 8 + i] != i * (255 / ((1 << CodedDepth) - 1)))
                            throw new InvalidOperationException("Authored palette alpha values differ.");
                }
                position = checked(position + length + 12);
            }
            if (paletteBytes != (1 << CodedDepth) * 3 || alphaBytes != (PartialAlpha ? 2 : 1 << CodedDepth))
                throw new InvalidOperationException("Authored PLTE/tRNS lengths differ.");
        }
        compressed.Position = 0;
        using var inflater = new ZLibStream(compressed, CompressionMode.Decompress);
        foreach (var (startX, startY, stepX, stepY) in Interlaced ? WarmPngAdam7Input.Passes : [(0, 0, 1, 1)])
        {
            var bytesPerPixel = Depth / 8 * (Palette || Gray ? 1 : GrayAlpha ? 2 : Rgb ? 3 : 4);
            var passWidth = (source.Width - startX + stepX - 1) / stepX;
            var packed = CodedDepth < 8;
            var row = new byte[packed ? (passWidth * CodedDepth + 7) / 8 : passWidth * bytesPerPixel];
            for (var y = startY; y < source.Height; y += stepY)
            {
                if (inflater.ReadByte() != Filter) throw new InvalidOperationException("PNG row filter differs from the fixed profile.");
                inflater.ReadExactly(row);
                if (packed)
                {
                    var maximum = (1 << CodedDepth) - 1;
                    for (var x = 0; x < passWidth; x++)
                        if (((row[x * CodedDepth / 8] >> (8 - CodedDepth - x * CodedDepth % 8)) & maximum)
                            * (255 / maximum) != pixels[y * source.Stride + x * 4])
                            throw new InvalidOperationException("Packed PNG samples differ.");
                    if ((passWidth * CodedDepth & 7) != 0 && (row[^1] & ((1 << (8 - (passWidth * CodedDepth & 7))) - 1)) != 0)
                        throw new InvalidOperationException("Packed PNG padding differs.");
                    continue;
                }
                for (var x = startX; x < source.Width; x += stepX)
                for (var i = 0; i < bytesPerPixel; i++)
                {
                    var sampleByte = Depth == 16 ? i ^ 1 : i;
                    var sourceByte = GrayAlpha && sampleByte >= Depth / 8 ? sampleByte + 2 * Depth / 8 : sampleByte;
                    var original = pixels[y * source.Stride + x * Depth / 8 * 4 + sourceByte];
                    var prediction = 0;
                    if (Paeth)
                    {
                        var offset = x * bytesPerPixel + i;
                        int ByteAt(int rowY, int byteX) => rowY < 0 || byteX < 0 ? 0 : pixels[rowY * source.Stride + (Depth == 16 ? byteX ^ 1 : byteX)];
                        prediction = WarmPngPaethInput.Predict(ByteAt(y, offset - bytesPerPixel), ByteAt(y - 1, offset), ByteAt(y - 1, offset - bytesPerPixel));
                    }
                    else if (Up && y > 0) prediction = pixels[(y - 1) * source.Stride + x * bytesPerPixel + sampleByte];
                    else if (Sub && x > 0) prediction = pixels[y * source.Stride + (x - 1) * bytesPerPixel + sampleByte];
                    else if (Average) prediction = ((x == 0 ? 0 : pixels[y * source.Stride + (x - 1) * bytesPerPixel + sampleByte])
                        + (y == 0 ? 0 : pixels[(y - 1) * source.Stride + x * bytesPerPixel + sampleByte])) / 2;
                    if (row[(x - startX) / stepX * bytesPerPixel + i] != unchecked((byte)(original - prediction)))
                        throw new InvalidOperationException("PNG source/pass samples differ.");
                }
            }
        }
        if (inflater.ReadByte() != -1) throw new InvalidOperationException("PNG filtered length differs.");
    }

    [Benchmark]
    public int Managed()
    {
        if (Encode)
        {
            using var output = new MemoryStream(); EncodeInto(output);
            return checked((int)output.Length) ^ output.GetBuffer()[checked((int)output.Length) / 2];
        }
        using var result = ImageCodec.Decode(encoded, decodeOptions, decodeCancellation);
        return result.Stride * result.Height ^ (Depth == 8 ? result.GetRow8(0)[source.Stride / 2] : result.GetRow16(0)[source.Stride / 4]);
    }

    private sealed class ForwardOnlyPngOutput(MemoryStream target) : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => target.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Write(ReadOnlySpan<byte> data) => target.Write(data);
        public override void Write(byte[] data, int offset, int count) => target.Write(data, offset, count);
        public override void Flush() => target.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            if (!ready) return;
            ValidateManaged();
            var after = native.Measure(smoke, before[0].Operations, sampleCount);
            native.Check(); ValidateNative(); native.Stop();
            var path = Path.Combine(state.Root, "artifacts", "benchmark-validation", nameof(WarmPng) + "-" + Guid.NewGuid().ToString("N") + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new
            {
                schemaVersion = 1, name = nameof(WarmPng), caseId = Case.ToString(), smoke, nativeSampleCount = smoke ? 1 : sampleCount,
                runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),
                width = source.Width, height = source.Height, depth = Depth, cpu, managedAffinityCount = 1,
                processorCount = Environment.ProcessorCount, serverGc = GCSettings.IsServerGC, gcConfiguration = GC.GetConfigurationVariables(),
                runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier,
                jitPolicy = "Tiered compilation disabled; fully optimized JIT before measurement", sourceIdentity,
                inputSha256 = JpegBenchmarkCase.Hash(Encode ? pixels : encoded), rasterSha256 = JpegBenchmarkCase.Hash(pixels),
                managedOutputBytes, nativeOutputBytes, managedEncodedSha256 = JpegBenchmarkCase.Hash(encoded),
                nativeOutputSha256 = JpegBenchmarkCase.HashFile(state.NativeOutput),
                interlace = Interlaced ? 1 : 0,
                colorType = ColorType, codedDepth = CodedDepth,
                rasterRecipe = RgbTransparency && Depth == 16 ? "Existing original RGBA16 RGB words unchanged; six-byte tRNS key(12347,35806,59265) from original pixel1 gives alpha0 only on exact full RGB words, otherwise65535. Hidden RGB retained; upper-byte collisions stay opaque. No additional original image."
                    : RgbTransparency ? "Existing original RGBA8 RGB unchanged; six-byte tRNS key(48,139,231) gives alpha0 only on an exact RGB match, otherwise255. Hidden RGB retained. No additional original image."
                    : GrayTransparency && Depth == 16 ? "Existing original RGBA16 raster; gray=original full-word red; RGB equal gray; tRNS key0x7f7f gives alpha0, others65535. Hidden full-word RGB retained. No additional original image."
                    : GrayTransparency ? "Existing original RGBA8 raster; gray=original red; RGB equal gray; tRNS key127 gives alpha0, all other pixels alpha255. Hidden RGB127 preserved. No additional original image."
                    : PartialAlpha ? "Existing original RGBA8 raster; index i=original red; PLTE256 R=i,G=255-i,B=(73*i+19)%256; tRNS two bytes0/1, other entries opaque255. Original palette RGB/index samples unchanged; includes nonzero hidden RGB at i=0. No additional original image."
                    : Mono ? $"Existing original RGBA8 raster; gray=(red>>{8 - CodedDepth})*{255 / ((1 << CodedDepth) - 1)}; RGB equal gray, alpha 255. Packed MSB-first, unused low bits zero. No additional original image."
                    : Palette ? $"Existing original RGBA8 raster; index j=red>>{8 - CodedDepth}, i=j*{255 / ((1 << CodedDepth) - 1)}, coded MSB-first with unused low bits zero; original {1 << CodedDepth}-entry palette R=i,G=255-i,B=(73*i+19)%256,A=i. Includes nonzero hidden RGB at i=0. No additional original image."
                    : GrayAlpha ? $"Existing original RGBA{Depth} raster; red retained as gray, green/blue set to red, alpha unchanged including hidden RGB. No additional original image."
                    : Gray ? $"Existing original RGBA{Depth} raster; red retained as gray, green/blue set to red, alpha set to {(Depth == 16 ? 65535 : 255)}. No additional original image."
                    : Rgb ? $"Existing original RGBA{Depth} raster; RGB unchanged, alpha set to {(Depth == 16 ? 65535 : 255)} for RGB PNG. No additional original image." : "Existing original RGBA raster, all samples unchanged.",
                originalRasterSha256 = JpegBenchmarkCase.Hash(HorizontalFlipInput.Create(Depth)),
                filter = Filter,
                compression = RgbTransparency && Depth == 16 ? "Authored RGB16BE PNG; filter None, noninterlaced, six-byte tRNS key(12347,35806,59265); BCL Optimal level6; identical resident encoded input"
                    : RgbTransparency ? "Authored RGB8 PNG; filter None, noninterlaced, six-byte tRNS key(48,139,231); BCL Optimal level6; identical resident encoded input"
                    : GrayTransparency && Depth == 16 ? "Authored gray16BE PNG; filter None, noninterlaced, tRNS two-byte key0x7f7f; BCL Optimal level6; identical resident encoded input"
                    : GrayTransparency ? "Authored gray8 PNG; filter None, noninterlaced, tRNS two-byte key127; BCL Optimal level6; identical resident encoded input"
                    : Fastest ? "BCL streaming fast adaptive level2 / FFmpeg fast level1; complete row packing/compression/output timed; exact full RGBA16, filter None, unchanged5% IDAT-size gate"
                    : NoCompression ? "BCL NoCompression / FFmpeg level0; stored Deflate only, filter None, noninterlaced RGBA" + Depth
                    : Paeth ? $"Authored noninterlaced PNG RGBA{Depth}; every row Paeth; BCL Optimal level 6; identical resident encoded input"
                    : Up ? $"Authored noninterlaced PNG RGBA{Depth}; every row Up; BCL Optimal level 6; identical resident encoded input"
                    : Sub ? $"Authored noninterlaced PNG RGBA{Depth}; every row Sub; BCL Optimal level 6; identical resident encoded input"
                    : Average ? $"Authored noninterlaced PNG RGBA{Depth}; every row Average; BCL Optimal level 6; identical resident encoded input"
                    : Interlaced ? "Authored PNG Adam7 RGBA16; BCL Optimal level 6; filter None; identical resident encoded input"
                    : MatchedByteEncoding ? "Lokad.Image Optimal uses BCL level9 / FFmpeg level6; exact samples and unchanged5% IDAT-size gate; filter None; noninterlaced"
                    : $"BCL Optimal (zlib default level 6) / FFmpeg level 6; filter None; noninterlaced {(Mono ? "packed gray" + CodedDepth + ", opaque" : Palette ? $"palette{CodedDepth}, {1 << CodedDepth} entries and {(PartialAlpha ? "two-entry" : "full")} tRNS" : GrayAlpha ? "gray-alpha" : Gray ? Depth == 16 ? "gray16be" : "gray" : Rgb ? "RGB" : "RGBA")}",
                encoderLevel = Encode ? EncoderLevel : (int?)null, managedEncoderLevel = Encode ? ManagedEncoderLevel : (int?)null, nativeEncoderLevel = Encode ? EncoderLevel : (int?)null, storedDeflateBlocksExact = NoCompression,
                grayTransparencyKey = GrayTransparency ? (int?)GrayTransparencyKey : null,
                grayTransparencyBytes = GrayTransparency ? (int?)2 : null,
                rgbTransparencyKey = RgbTransparency ? WarmPngRgbTransparencyInput.KeySamples(Depth).ToArray() : null, rgbTransparencyBytes = RgbTransparency ? (int?)6 : null,
                managedIdatBytes = IdatBytes(encoded), nativeIdatBytes = Encode ? IdatBytes(File.ReadAllBytes(state.NativeOutput)) : IdatBytes(encoded),
                paletteEntries = Palette ? (1 << CodedDepth) : (int?)null, transparencyEntries = Palette ? (PartialAlpha ? 2 : 1 << CodedDepth) : (int?)null,
                forwardOnlyOutput = ForwardOutput, forwardPacketMatchesStock = ForwardOutput, callerOutputLeftOpen = Encode,
                cancellationCanBeCanceled = decodeCancellation.CanBeCanceled,
                cancellationRequested = decodeCancellation.IsCancellationRequested,
                alreadyCanceledRejected = Cancelable || EncodeCancelable ? true : (bool?)null,
                cancellationBoundary = Cancelable || EncodeCancelable
                    ? "Live cancelable token, never signaled; token/source setup and disposal outside timing. Native backend has no equivalent cancellation setting; complete codec operation, full sample conversion and owned output release remain timed."
                    : null,
                tightWorkingBytes = Tight ? tightWorkingBytes : (long?)null,
                tightWorkingBoundaryExact = Tight ? true : (bool?)null,
                oneByteBelowRejected = Tight ? true : (bool?)null,
                budgetExcludesOptionalFilteredAndReadBuffers = Tight ? true : (bool?)null,
                originalInflaterAllowanceBytes = Tight ? 256 * 1024 : (int?)null,
                originalTwoRowBytes = Tight ? source.Stride * 2 : (int?)null,
                allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true,
                nativeCliNormalScalarPixelsExact = true,
                nativeDecodeEndianConversionTimed = !Encode && Depth == 16, nativeEndianRepackingTimed = (Encode || RgbTransparency) && Depth == 16, nativeEndianWorkspaceReused = Encode && Depth == 16 && !Gray,
                nativeRgbPackingTimed = Encode && Rgb, nativeRgbWorkspaceReused = Encode && Rgb,
                nativeGrayPackingTimed = Encode && Gray, nativeGrayWorkspaceReused = Encode && Gray && Depth == 8,
                nativeGrayPlaneExtractionTimed = Encode && Gray && Depth == 16, nativeGrayFilterOutputAllocationTimed = Encode && Gray && Depth == 16,
                nativeGrayAlphaPackingTimed = Encode && GrayAlpha, nativeGrayAlphaWorkspaceReused = Encode && GrayAlpha,
                nativeGrayAlphaStoragePackingTimed = Encode && GrayAlpha,
                nativeSampleTargetMilliseconds = Environment.GetEnvironmentVariable("LOKAD_IMAGE_WARM_NATIVE_SAMPLE_MS") ?? "200",
                sourceUnchanged = true, independentOwnedOutput = true, nativeExitCode = 0,
                librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(WarmPng).Assembly.Location), nativeBuild = native.BuildIdentity,
                nativeReady = native.Ready, nativeBefore = before, nativeAfter = after,
                boundary = (ForwardOutput ? "Fresh forward-only caller Stream over independent MemoryStream; wrapper creation/disposal and all span callbacks included, Length/Position/Seek unavailable, caller remains open. Native direct independent packet acquisition needs no seeking or equivalent wrapper. " : "") + (Tight
                    ? "Warm identical frozen RGBA16 PNG bytes to independent owned RGBA16 pixels under exact managed working limit: encoded span + IDAT bytes + output + 256 KiB original inflater allowance + two rows. The optional whole-filter and 8 KiB read buffers cannot fit. Public Decode admission/framing/BCL inflation/word conversion/output acquisition/disposal timed; preconstructed immutable options excluded. Native equivalent complete decode plus owned BE-to-LE conversion/output acquisition/release, context/packet/frame wrappers reused. Native has no matching caller admission setting. Exact pixels/alpha/hidden RGB/source/two owners and one-byte-below rejection checked outside timing. Startup/IPC/file IO/hashes/validation excluded."
                    : RgbTransparency && Depth == 16
                    ? "Warm identical RGB16BE PNG bytes with six-byte full-word tRNS key to independent owned RGBA16 pixels: public managed Decode/allocation/disposal; native codec/input packet/frame wrappers reused, complete decode/owned output acquisition/release. Native decoder performs exact full RGB-key alpha expansion to owned RGBA64BE inside every operation, followed by included endian conversion and second owned RGBA64LE output acquisition/release. No colorspace conversion. Full RGB/alpha/hidden RGB exact. Startup/IPC/file IO/hashes/validation excluded."
                    : RgbTransparency
                    ? "Warm identical RGB8 PNG bytes with six-byte tRNS key to independent owned RGBA8 pixels: public managed Decode/allocation/disposal; native codec/input packet/frame wrappers reused, complete decode/owned RGBA acquisition/release. Native decoder performs exact RGB-key alpha expansion inside every operation; decoded RGBA8 is already owned, so no second conversion or allocation. Full RGB/alpha/hidden RGB exact. Startup/IPC/file IO/hashes/validation excluded."
                    : NoCompression && !Encode
                    ? $"Warm identical frozen stored-Deflate RGBA{Depth} PNG bytes to independent owned RGBA{Depth} pixels: public managed Decode/allocation/disposal; native codec/input packet/frame wrappers reused, complete decode/owned output acquisition/release. Full RGBA samples/alpha/hidden RGB exact; stored Deflate validated outside timing. {(Depth == 16 ? "Native full-frame BE-to-LE word conversion and second owned output acquisition/release are timed; no colorspace conversion." : "No native conversion.")} IPC/file IO/hashes/validation excluded."
                    : NoCompression
                    ? $"Warm original RGBA{Depth} to complete owned PNG: fresh managed MemoryStream creation/growth/public EncodePng/disposal, no redundant ToArray. Native codec/source/packet wrappers reused; real independent owned packet acquisition/encode/release included. Both NoCompression/level0/filter None/color6, only stored Deflate blocks, exact full RGBA samples. For RGBA16 the native LE-to-BE full-frame sample conversion is timed in every operation into a reused prepared frame; managed row packing/storage is included. IPC/file IO/hashes/validation excluded."
                    : Encode && GrayAlpha
                    ? $"Warm grayscale RGBA{Depth} input with original alpha and hidden RGB to complete owned gray-alpha{Depth} PNG: fresh managed MemoryStream creation/growth/public EncodePng/disposal, no redundant ToArray. Original adapter losslessly packs RGBA{Depth} red/alpha{(Depth == 16 ? " with LE-to-BE word conversion" : "")} into {(Depth == 16 ? "YA16BE" : "YA8")} on the pinned CPU inside every operation, into a reused prepared frame, then calls the FFmpeg PNG encoder; no native colorspace conversion; source/context/wrappers reused, real independent owned packet acquisition/encode/release included. Managed level{ManagedEncoderLevel}/FFmpeg level{EncoderLevel}/filter None/color4. Equal RGB fixed and independently validated; original alpha retained. Normal/scalar CLI encodes the identical canonical {(Depth == 16 ? "YA16BE" : "YA8")} bytes for untimed validation; native timed input stays {(Depth == 16 ? "RGBA64LE" : "RGBA8")}. IPC/file IO/hashes/validation excluded."
                    : Encode && Gray
                    ? $"Warm opaque grayscale RGBA{Depth} input to complete owned gray{Depth} PNG: fresh managed MemoryStream creation/growth/public EncodePng/disposal, no redundant ToArray. Native one-thread {(Depth == 16 ? "FFmpeg extractplanes red and GRAY16LE-to-GRAY16BE conversion with owned intermediate acquisition/release" : "full-range RGBA-to-GRAY8 conversion into a reused prepared frame")} runs inside every encode operation; source/context/frame wrappers reused; codec/packet wrappers reused, real independent owned packet acquisition/encode/release included. Managed level{ManagedEncoderLevel}/FFmpeg level{EncoderLevel}/filter None/color0. Equal RGB and opacity fixed and independently validated. IPC/file IO/hashes/validation excluded."
                    : Encode && Rgb
                    ? $"Warm opaque RGBA{Depth} input to complete owned RGB{Depth} PNG: fresh managed MemoryStream creation/growth/public EncodePng/disposal, no redundant ToArray. Native one-thread {(Depth == 16 ? "RGBA64LE-to-RGB48BE" : "RGBA-to-RGB24")} conversion runs inside every encode operation into a reused prepared frame; codec/packet wrappers reused, real independent owned packet acquisition/encode/release included. Managed level{ManagedEncoderLevel}/FFmpeg level{EncoderLevel}/filter None/color2. Opacity fixed and independently validated. IPC/file IO/hashes/validation excluded."
                    : Encode
                    ? $"Warm RGBA input to complete owned PNG: fresh managed MemoryStream (creation, growth, encode and disposal included; no redundant ToArray); native codec context/packet wrapper reused, real independently owned packet acquisition, encode and release included. Managed level{ManagedEncoderLevel}/native level{EncoderLevel}/filter None. For RGBA16, native LE-to-BE full-frame sample conversion runs inside every operation, using a reused prepared frame; managed conversion/row storage is included in public EncodePng. IPC, file IO, hashes and validation excluded."
                    : "Warm identical PNG bytes to independent owned RGBA pixels: public managed Decode/allocation/disposal; native codec/input packet/frame wrappers reused, complete decode/owned output/release. RGBA16 BE-to-LE, RGB48BE-to-RGBA64LE, GRAY16BE-to-RGBA64LE, YA16BE-to-RGBA64LE, RGB24-to-RGBA, GRAY8-to-RGBA, MONOBLACK-to-RGBA, YA8-to-RGBA or PAL8-to-RGBA FFmpeg conversion and second output allocation included natively; direct RGBA8 requires no conversion. Native padded rows exposed directly; packing/file IO/IPC/hashes excluded.")
            }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
            Console.WriteLine("Validated warm PNG pair: " + path);
        }
        finally { native?.Dispose(); source?.Dispose(); cancellationSource?.Dispose(); state?.Dispose(); ready = false; }
    }
}
