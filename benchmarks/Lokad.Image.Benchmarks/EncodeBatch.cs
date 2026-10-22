using System.Buffers.Binary;
using System.Globalization;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum BatchEncoder { Png, Jpeg420, WebpLossless, WebpLossy }

/// <summary>One process reads 32 raw frames and writes 32 independent numbered still files.</summary>
[MemoryDiagnoser]
public class EncodeBatch
{
    [Params(129, 513, 1025, 2049)] public int Width { get; set; }
    [Params(8, 16)] public int Depth { get; set; }
    [Params(BatchEncoder.Png, BatchEncoder.Jpeg420, BatchEncoder.WebpLossless, BatchEncoder.WebpLossy)]
    public BatchEncoder Encoder { get; set; }
    public const int FrameCount = 32;
    private JpegBenchmarkCase? state;
    private string[] managedArguments = [], nativeArguments = [];
    private string managedDirectory = "", nativeDirectory = "", extension = "";
    private string managedIndex = "", nativeIndex = "";
    private bool setupComplete;
    private TimeSpan ExecutionTimeout => Width >= 1025 ? TimeSpan.FromMinutes(10) : TimeSpan.FromMinutes(1);
    private readonly List<byte[]> references = [];
    private readonly List<string> publicEncodedHashes = [];

    [GlobalSetup]
    public void Setup()
    {
        setupComplete = false;
        if (Width is not (129 or 513 or 1025 or 2049) || Depth is not (8 or 16) || !Enum.IsDefined(Encoder))
            throw new ArgumentException("Invalid encode batch profile.");
        state = new JpegBenchmarkCase();
        try
        {
            var height = Width / 2 + 3;
            var format = Encoder switch { BatchEncoder.Png => "png", BatchEncoder.Jpeg420 => "jpeg420",
                BatchEncoder.WebpLossless => "webp-lossless", _ => "webp-lossy" };
            extension = Encoder == BatchEncoder.Png ? ".png" : Encoder == BatchEncoder.Jpeg420 ? ".jpg" : ".webp";
            managedDirectory = Path.Combine(state.DirectoryPath, "managed-frames");
            nativeDirectory = Path.Combine(state.DirectoryPath, "native-frames");
            Directory.CreateDirectory(managedDirectory); Directory.CreateDirectory(nativeDirectory);
            var receipts = new List<object>(); references.Clear(); publicEncodedHashes.Clear();
            using (var rawInput = File.Create(state.Input))
            {
                for (var frame = 0; frame < FrameCount; frame++)
                {
                    var source = OriginalPixels.Generate(Width, height, Depth, opaque: Encoder == BatchEncoder.Jpeg420, phase: frame);
                    var raisedAlphaPixels = Encoder == BatchEncoder.WebpLossless ? RequireNonzeroByteAlpha(source, Depth) : 0;
                    rawInput.Write(source);
                    var expected = Encoder == BatchEncoder.Png ? source : Normalize(source, Depth);
                    references.Add(expected);
                    using var pixels = JpegBenchmarkCase.Buffer(source, Width, height, Depth);
                    using var encoded = new MemoryStream(); Encode(pixels, encoded);
                    var publicHash = JpegBenchmarkCase.Hash(encoded.ToArray()); publicEncodedHashes.Add(publicHash);
                    receipts.Add(new { frame, rawOffset = (long)frame * source.Length, rawBytes = source.Length,
                        recipe = "asymmetric-v1-phase: add phase*977+phase*channel*313 modulo 65536 before the declared depth reduction",
                        alphaPolicy = Encoder == BatchEncoder.WebpLossless
                            ? "raise byte-zero alpha to 1 (257 at source depth 16), preserving all original RGB; native libwebp exact-hidden-RGB control is unavailable"
                            : "original channel samples",
                        raisedZeroAlphaPixels = raisedAlphaPixels, rawSha256 = JpegBenchmarkCase.Hash(source),
                        referenceSha256 = JpegBenchmarkCase.Hash(expected), publicEncodedSha256 = publicHash });
                }
            }
            managedArguments = [state.Probe, "encode-sequence", state.Input, managedDirectory,
                JpegBenchmarkCase.Number(Width), JpegBenchmarkCase.Number(height), JpegBenchmarkCase.Number(Depth),
                format, JpegBenchmarkCase.Number(FrameCount), "85", "4"];
            nativeArguments = NativeArguments(height);
            Ffmpeg(); Managed();
            var managed = Validate(managedDirectory, height, managed: true);
            var native = Validate(nativeDirectory, height, managed: false);
            managedIndex = OutputIndex(managedDirectory); nativeIndex = OutputIndex(nativeDirectory);
            var nativeMemory = ChildProcessMemory.Measure(state.Ffmpeg, nativeArguments, state.DirectoryPath, timeout: ExecutionTimeout);
            var managedMemory = ChildProcessMemory.Measure("dotnet", managedArguments, state.DirectoryPath, timeout: ExecutionTimeout);
            RequireOutputIndexes();
            state.Report(nameof(EncodeBatch), new { Width, height, depth = Depth, Encoder, frames = FrameCount,
                family = "amortized-startup still-image encode", codecThreads = 1, filterThreads = 1,
                operationTimeoutSeconds = (int)ExecutionTimeout.TotalSeconds, cleanupOutputHashCheck = true,
                managedQuality = Encoder is BatchEncoder.Jpeg420 or BatchEncoder.WebpLossy ? (int?)85 : null,
                nativeJpegQScale = Encoder == BatchEncoder.Jpeg420 ? (int?)5 : null,
                nativeWebpQuality = Encoder is BatchEncoder.WebpLossless or BatchEncoder.WebpLossy ? (int?)85 : null,
                webpEffort = Encoder is BatchEncoder.WebpLossless or BatchEncoder.WebpLossy ? (int?)4 : null },
                managedArguments, nativeArguments, managed, native,
                method: "Untimed validation of 32 raw frames streamed from the same single tightly packed RGBA8/RGBA16LE file. Each subject launches one process and writes every numbered independent still bitstream. No input PNG decode, preloading, looping, animation or output suppression. Each bitstream is independently decoded with the native oracle and the public library; managed encoding must equal the exact public API workflow. PNG is exact at source precision; WebP reduces depth once and retains exact alpha; lossy decoder RGB agreement remains maximum 4/RMSE 1.5. Encoder source error and size are recorded independently. Managed JPEG quality 85/native qscale 5 and WebP quality 85/effort 4 are subject-specific settings, not matched quality or compression effort. PNG uses owned default optimal compression/native level 6 and filter none. Codec context reuse, remaining startup/JIT, filesystem and raw transport costs remain part of the batch. Divide batch latency by 32 for per-image figures. Child-memory counters are separate untimed executions.",
                inputs: new { rawFrames = FrameCount, rawFileBytes = new FileInfo(state.Input).Length,
                    rawFileSha256 = JpegBenchmarkCase.HashFile(state.Input), frames = receipts },
                childMemory: new { native = nativeMemory, managed = managedMemory });
            references.Clear(); publicEncodedHashes.Clear();
            setupComplete = true;
        }
        catch { state.Dispose(); throw; }
    }

    private void Encode(ImageBuffer image, Stream output)
    {
        switch (Encoder)
        {
            case BatchEncoder.Png: ImageCodec.EncodePng(image, output); break;
            case BatchEncoder.Jpeg420: ImageCodec.EncodeJpeg(image, output, new JpegEncodeOptions { Quality = 85, Sampling = JpegSampling.YCbCr420 }); break;
            default: ImageCodec.EncodeWebp(image, output, new WebpEncodeOptions { Lossless = Encoder == BatchEncoder.WebpLossless, Quality = 85, Effort = 4 }); break;
        }
    }

    private string[] NativeArguments(int height)
    {
        var arguments = new List<string> { "-hide_banner", "-loglevel", "error", "-threads:v", "1",
            "-f", "rawvideo", "-pixel_format", Depth == 16 ? "rgba64le" : "rgba",
            "-video_size", Width + "x" + height, "-framerate", "25", "-i", state!.Input, "-filter_threads", "1" };
        var normalization = Depth == 16 && Encoder != BatchEncoder.Png ? RgbaOracle.Normalize16Filter + "," : "";
        if (Encoder == BatchEncoder.Jpeg420)
        {
            var jpeg = JpegOracle.EncodeArguments(Width, height, JpegSampling.YCbCr420, 5);
            arguments.AddRange(["-vf", normalization + jpeg[Array.IndexOf(jpeg, "-vf") + 1],
                "-c:v", "mjpeg", "-huffman", "optimal", "-pix_fmt", "yuvj420p", "-q:v", "5"]);
        }
        else if (Encoder == BatchEncoder.Png)
            arguments.AddRange(["-c:v", "png", "-pix_fmt", Depth == 16 ? "rgba64be" : "rgba", "-pred", "none", "-compression_level", "6"]);
        else
            arguments.AddRange(["-vf", normalization + "format=bgra", "-c:v", "libwebp", "-pix_fmt", "bgra",
                "-lossless", Encoder == BatchEncoder.WebpLossless ? "1" : "0", "-quality", "85", "-compression_level", "4"]);
        arguments.AddRange(["-map_metadata", "-1", "-frames:v", JpegBenchmarkCase.Number(FrameCount), "-fps_mode", "passthrough",
            "-threads:v", "1", "-f", "image2", "-start_number", "0", "-y", Path.Combine(nativeDirectory, "frame-%04d" + extension)]);
        return arguments.ToArray();
    }

    private object Validate(string directory, int height, bool managed)
    {
        RequireFileCount(directory); var frames = new List<object>(); long outputBytes = 0;
        for (var frame = 0; frame < FrameCount; frame++)
        {
            var encoded = File.ReadAllBytes(FramePath(directory, frame));
            var encodedHash = JpegBenchmarkCase.Hash(encoded); outputBytes += encoded.Length;
            if (managed && encodedHash != publicEncodedHashes[frame])
                throw new InvalidOperationException("Numbered encode output differs from the exact public API workflow.");
            var info = ImageCodec.Identify(encoded);
            var outputDepth = Encoder == BatchEncoder.Png ? Depth : 8;
            var expectedFormat = Encoder == BatchEncoder.Png ? ImageFormat.Png : Encoder == BatchEncoder.Jpeg420 ? ImageFormat.Jpeg : ImageFormat.WebP;
            if ((info.Format, info.Width, info.Height, info.BitDepth) != (expectedFormat, Width, height, outputDepth))
                throw new InvalidOperationException("Encoded batch geometry, format or precision differs.");
            var jpeg = Encoder == BatchEncoder.Jpeg420 ? JpegQualitySyntax.Read(encoded) : null;
            if (jpeg is not null && (jpeg.Components != 3 || jpeg.Sampling != "2x2,1x1,1x1"))
                throw new InvalidOperationException("Encoded batch JPEG is not baseline 4:2:0.");
            byte[] native;
            if (Encoder == BatchEncoder.Jpeg420) native = JpegOracle.Decode(encoded, Width, height, JpegSampling.YCbCr420, state!.Ffmpeg);
            else if (Encoder == BatchEncoder.WebpLossy) native = WebpOracle.Decode(encoded, Width, height, info.HasAlpha, state!.Ffmpeg);
            else native = OracleProcess.Run(state!.Ffmpeg,
                ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0",
                    "-frames:v", "1", "-threads:v", "1", "-pix_fmt", outputDepth == 16 ? "rgba64le" : "rgba", "-f", "rawvideo", "pipe:1"], encoded);
            using var image = ImageCodec.Decode(encoded); var actual = OriginalPixels.Bytes(image);
            if ((image.Width, image.Height, image.BitDepth) != (Width, height, outputDepth) || actual.Length != references[frame].Length || native.Length != actual.Length)
                throw new InvalidOperationException("Encoded batch decoder geometry or precision differs.");
            var lossless = Encoder is BatchEncoder.Png or BatchEncoder.WebpLossless;
            object? decoderDifference = null; object? sourceError = null;
            if (lossless)
            {
                if (!actual.AsSpan().SequenceEqual(references[frame]) || !native.AsSpan().SequenceEqual(references[frame]))
                    throw new InvalidOperationException("Lossless encoded batch samples differ from the original reference.");
            }
            else
            {
                var agreement = Compare(native, actual); var quality = Compare(references[frame], native);
                if (agreement.Maximum > 4 || agreement.Rmse > 1.5)
                    throw new InvalidOperationException($"Encoded batch RGB disagreement: maximum {agreement.Maximum}, RMSE {agreement.Rmse}.");
                decoderDifference = JpegBenchmarkCase.Metrics(agreement); sourceError = JpegBenchmarkCase.Metrics(quality);
            }
            frames.Add(new { frame, name = Path.GetFileName(FramePath(directory, frame)), outputBytes = encoded.Length, outputSha256 = encodedHash,
                decodedWidth = image.Width, decodedHeight = image.Height, decodedDepth = image.BitDepth, alphaExact = true, exact = lossless,
                nativeDecodedSha256 = JpegBenchmarkCase.Hash(native), managedDecodedSha256 = JpegBenchmarkCase.Hash(actual),
                referenceSha256 = JpegBenchmarkCase.Hash(references[frame]), decoderDifference, sourceError,
                codedSampling = jpeg?.Sampling, quantizationSha256 = jpeg?.QuantizationHash });
        }
        return new { outputBytes, outputIndexSha256 = OutputIndex(directory),
            outputHashScope = "ordered list of numbered encoded files, their byte lengths and SHA256 hashes", frameValidation = frames };
    }

    private string OutputIndex(string directory)
    {
        RequireFileCount(directory);
        var files = Enumerable.Range(0, FrameCount).Select(frame =>
        {
            var path = FramePath(directory, frame); var bytes = File.ReadAllBytes(path);
            if (bytes.Length == 0) throw new InvalidOperationException("Encoded batch contains an empty frame.");
            return new { frame, name = Path.GetFileName(path), encodedBytes = bytes.Length, encodedSha256 = JpegBenchmarkCase.Hash(bytes) };
        });
        return JpegBenchmarkCase.Hash(JsonSerializer.SerializeToUtf8Bytes(files));
    }

    private void RequireFileCount(string directory)
    {
        if (Directory.GetFiles(directory).Length != FrameCount)
            throw new InvalidOperationException("Encoded batch frame count differs.");
    }

    private string FramePath(string directory, int frame) => Path.Combine(directory, "frame-" + frame.ToString("D4", CultureInfo.InvariantCulture) + extension);
    internal static void ValidateCleanupControls()
    {
        var controls = 0;
        foreach (var native in new[] { false, true })
        foreach (var extraFile in new[] { false, true })
        {
            var workload = new EncodeBatch { Width = 129, Depth = 8, Encoder = BatchEncoder.Png };
            try
            {
                workload.Setup();
                var directory = native ? workload.nativeDirectory : workload.managedDirectory;
                if (extraFile) File.WriteAllBytes(Path.Combine(directory, "unexpected.png"), [1]);
                else
                {
                    var path = workload.FramePath(directory, 0); var bytes = File.ReadAllBytes(path);
                    bytes[0] ^= 1; File.WriteAllBytes(path, bytes);
                }
                var rejected = false;
                try { workload.Cleanup(); }
                catch (InvalidOperationException) { rejected = true; }
                if (!rejected || Directory.Exists(workload.state!.DirectoryPath))
                    throw new InvalidOperationException("Encode batch cleanup did not reject and remove changed output files.");
                controls++;
            }
            finally { workload.Cleanup(); }
        }
        Console.WriteLine($"All {controls} encode batch cleanup mutation controls rejected changed outputs and removed their temporary directories.");
    }

    private void RequireOutputIndexes()
    {
        if (managedIndex != OutputIndex(managedDirectory) || nativeIndex != OutputIndex(nativeDirectory))
            throw new InvalidOperationException("Encoded batch execution changed a validated still file.");
    }

    private long Run(string executable, string[] arguments, string directory)
    {
        OracleProcess.Run(executable, arguments, timeout: ExecutionTimeout); RequireFileCount(directory);
        long bytes = 0;
        for (var frame = 0; frame < FrameCount; frame++) bytes += new FileInfo(FramePath(directory, frame)).Length;
        return bytes;
    }

    private static int RequireNonzeroByteAlpha(byte[] pixels, int depth)
    {
        var bytes = depth / 8; var count = 0;
        for (var offset = 0; offset < pixels.Length; offset += 4 * bytes)
        {
            var alpha = depth == 8 ? pixels[offset + 3] : (BinaryPrimitives.ReadUInt16LittleEndian(pixels.AsSpan(offset + 6)) + 128) / 257;
            if (alpha != 0) continue;
            if (depth == 8) pixels[offset + 3] = 1;
            else BinaryPrimitives.WriteUInt16LittleEndian(pixels.AsSpan(offset + 6), 257);
            count++;
        }
        return count;
    }

    private static byte[] Normalize(byte[] pixels, int depth)
    {
        if (depth == 8) return pixels;
        var result = new byte[pixels.Length / 2];
        for (var index = 0; index < result.Length; index++)
            result[index] = (byte)((BinaryPrimitives.ReadUInt16LittleEndian(pixels.AsSpan(index * 2)) + 128) / 257);
        return result;
    }

    private static (int Maximum, double Rmse, double? Psnr) Compare(byte[] reference, byte[] actual)
    {
        if (reference.Length != actual.Length || actual.Length % 4 != 0) throw new InvalidOperationException("Encoded batch raw sizes differ.");
        double squares = 0; var maximum = 0;
        for (var index = 0; index < actual.Length; index++)
        {
            if (index % 4 == 3)
            {
                if (reference[index] != actual[index]) throw new InvalidOperationException("Encoded batch alpha differs.");
                continue;
            }
            var error = Math.Abs(reference[index] - actual[index]); maximum = Math.Max(maximum, error); squares += error * error;
        }
        var rmse = Math.Sqrt(squares / (actual.Length / 4 * 3));
        return (maximum, rmse, rmse == 0 ? null : 20 * Math.Log10(255 / rmse));
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => Run(state!.Ffmpeg, nativeArguments, nativeDirectory);
    [Benchmark] public long Managed() => Run("dotnet", managedArguments, managedDirectory);
    [GlobalCleanup] public void Cleanup()
    {
        try { if (setupComplete) RequireOutputIndexes(); }
        finally
        {
            state?.Dispose(); references.Clear(); publicEncodedHashes.Clear(); setupComplete = false;
        }
    }
}
