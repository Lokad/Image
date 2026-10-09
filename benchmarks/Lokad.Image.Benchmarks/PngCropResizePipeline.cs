using System.Buffers.Binary;
using System.Globalization;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>One process decodes, crops, resizes and encodes one or 32 independent PNG files.</summary>
[MemoryDiagnoser]
public class PngCropResizePipeline
{
    [Params(129, 513, 1025, 2049)] public int Width { get; set; }
    [Params(8, 16)] public int Depth { get; set; }
    [Params(1, 32)] public int Frames { get; set; }
    [Params(false, true)] public bool Upscale { get; set; }
    private const int CropX = 3, CropY = 5;
    private JpegBenchmarkCase? state;
    private string[] managedArguments = [], nativeArguments = [];
    private string inputDirectory = "", managedDirectory = "", nativeDirectory = "", referencePath = "";
    private string inputIndex = "", managedIndex = "", nativeIndex = "";
    private int targetWidth, targetHeight;
    private bool setupComplete;
    private readonly List<string> publicEncodedHashes = [];
    private TimeSpan ExecutionTimeout => Width >= 1025 ? TimeSpan.FromMinutes(10) : TimeSpan.FromMinutes(1);

    [GlobalSetup]
    public void Setup()
    {
        setupComplete = false;
        if (Width is not (129 or 513 or 1025 or 2049) || Depth is not (8 or 16) || Frames is not (1 or 32))
            throw new ArgumentException("Invalid PNG crop/nearest pipeline profile.");
        state = new JpegBenchmarkCase();
        try
        {
            var height = Width / 2 + 3; var cropWidth = Width - 8; var cropHeight = height - 10;
            targetWidth = Upscale ? cropWidth * 2 - 1 : cropWidth / 2;
            targetHeight = Upscale ? cropHeight * 2 + 1 : cropHeight / 2;
            inputDirectory = Path.Combine(state.DirectoryPath, "input-frames");
            managedDirectory = Path.Combine(state.DirectoryPath, "managed-frames");
            nativeDirectory = Path.Combine(state.DirectoryPath, "native-frames");
            Directory.CreateDirectory(inputDirectory); Directory.CreateDirectory(managedDirectory); Directory.CreateDirectory(nativeDirectory);
            var sourcePath = Path.Combine(state.DirectoryPath, "source.raw");
            referencePath = Path.Combine(state.DirectoryPath, "reference.raw");
            var receipts = new List<object>(); publicEncodedHashes.Clear();
            using (var source = File.Create(sourcePath))
            using (var reference = File.Create(referencePath))
            {
                for (var frame = 0; frame < Frames; frame++)
                {
                    var original = OriginalPixels.Generate(Width, height, Depth, phase: frame); source.Write(original);
                    using var image = JpegBenchmarkCase.Buffer(original, Width, height, Depth);
                    using var encoded = new MemoryStream(); ImageCodec.EncodePng(image, encoded);
                    var bytes = encoded.ToArray(); File.WriteAllBytes(FramePath(inputDirectory, frame), bytes);
                    using var decoded = ImageCodec.Decode(bytes);
                    if ((decoded.Width, decoded.Height, decoded.BitDepth) != (Width, height, Depth)
                        || !OriginalPixels.Bytes(decoded).AsSpan().SequenceEqual(original))
                        throw new InvalidOperationException("PNG input differs from its original samples.");
                    using var cropped = ImageTransforms.Crop(decoded, CropX, CropY, cropWidth, cropHeight);
                    using var resized = ImageTransforms.Resize(cropped, targetWidth, targetHeight, ResizeFilter.Nearest);
                    var expected = OriginalReference(cropWidth, cropHeight, frame);
                    var publicPixels = OriginalPixels.Bytes(resized);
                    if ((resized.Width, resized.Height, resized.BitDepth) != (targetWidth, targetHeight, Depth)
                        || !publicPixels.AsSpan().SequenceEqual(expected))
                        throw new InvalidOperationException("Public PNG crop/nearest workflow differs from original coordinate samples.");
                    reference.Write(expected);
                    using var publicOutput = new MemoryStream(); ImageCodec.EncodePng(resized, publicOutput);
                    var publicBytes = publicOutput.ToArray(); var publicHash = JpegBenchmarkCase.Hash(publicBytes);
                    publicEncodedHashes.Add(publicHash);
                    receipts.Add(new { frame, name = Path.GetFileName(FramePath(inputDirectory, frame)),
                        recipe = "asymmetric-v1-phase: add phase*977+phase*channel*313 modulo 65536 before the declared depth reduction",
                        inputBytes = bytes.Length, inputSha256 = JpegBenchmarkCase.Hash(bytes),
                        originalRawBytes = original.Length, originalRawSha256 = JpegBenchmarkCase.Hash(original),
                        referenceBytes = expected.Length, referenceSha256 = JpegBenchmarkCase.Hash(expected),
                        publicResizedSha256 = JpegBenchmarkCase.Hash(publicPixels),
                        publicEncodedBytes = publicBytes.Length, publicEncodedSha256 = publicHash });
                }
            }
            inputIndex = OutputIndex(inputDirectory);
            // The case input is an ordered file index; timed subjects read only the numbered PNG files.
            File.WriteAllBytes(state.Input, JsonSerializer.SerializeToUtf8Bytes(FileIndex(inputDirectory)));
            managedArguments = [state.Probe, "png-crop-nearest-sequence", inputDirectory, managedDirectory,
                JpegBenchmarkCase.Number(Width), JpegBenchmarkCase.Number(height), JpegBenchmarkCase.Number(Depth),
                JpegBenchmarkCase.Number(Frames), JpegBenchmarkCase.Number(CropX), JpegBenchmarkCase.Number(CropY),
                JpegBenchmarkCase.Number(cropWidth), JpegBenchmarkCase.Number(cropHeight),
                JpegBenchmarkCase.Number(targetWidth), JpegBenchmarkCase.Number(targetHeight), "Nearest"];
            var packed = Depth == 16 ? "rgba64le" : "rgba";
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-noautorotate",
                "-f", "image2", "-framerate", "25", "-start_number", "0", "-c:v", "png", "-i", Pattern(inputDirectory),
                "-filter_complex_threads", "1", "-filter_complex",
                $"format={packed},crop=w={cropWidth}:h={cropHeight}:x={CropX}:y={CropY}:exact=1," + ResizeOracle.NearestFilter(targetWidth, targetHeight, Depth),
                "-map_metadata", "-1", "-frames:v", JpegBenchmarkCase.Number(Frames), "-fps_mode", "passthrough",
                "-threads:v", "1", "-c:v", "png", "-pix_fmt", Depth == 16 ? "rgba64be" : "rgba",
                "-pred", "none", "-compression_level", "6", "-f", "image2", "-start_number", "0", "-y", Pattern(nativeDirectory)];
            var inputDecoded = DecodeNativeSequence(inputDirectory, sourcePath, Width, height, "input-decoded.raw");
            Ffmpeg(); Managed();
            var managedResult = Validate(managedDirectory, true);
            var nativeResult = Validate(nativeDirectory, false);
            managedIndex = OutputIndex(managedDirectory); nativeIndex = OutputIndex(nativeDirectory);
            var childMemory = new
            {
                native = ChildProcessMemory.Measure(state.Ffmpeg, nativeArguments, state.DirectoryPath, timeout: ExecutionTimeout),
                managed = ChildProcessMemory.Measure("dotnet", managedArguments, state.DirectoryPath, timeout: ExecutionTimeout)
            };
            RequireIndexes();
            state.Report(nameof(PngCropResizePipeline), new { Width, height, depth = Depth, frames = Frames, Upscale,
                cropX = CropX, cropY = CropY, cropWidth, cropHeight, targetWidth, targetHeight, filter = "Nearest",
                family = Frames == 1 ? "PNG decode/crop/resize/encode invocation" : "amortized-startup PNG decode/crop/resize/encode",
                codecThreads = 1, filterThreads = 1, nativePointContexts = 2, nativeAlphaReplicatedPlanes = 3,
                nativeResizeImplementation = "zscale-point-rgb-and-alpha", nativePngPredictor = "none", nativePngCompressionLevel = 6,
                managedPngOptions = "public defaults", operationTimeoutSeconds = (int)ExecutionTimeout.TotalSeconds,
                cleanupOutputHashCheck = true, cleanupInputHashCheck = true, referenceValidation = "streamed-original-coordinate-reference-file" },
                managedArguments, nativeArguments, managedResult, nativeResult,
                inputs: new { frames = receipts, encodedFileCount = Frames, inputIndexSha256 = inputIndex,
                    sourceFileBytes = new FileInfo(sourcePath).Length, sourceFileSha256 = JpegBenchmarkCase.HashFile(sourcePath),
                    referenceFileBytes = new FileInfo(referencePath).Length, referenceFileSha256 = JpegBenchmarkCase.HashFile(referencePath),
                    nativeInputDecoded = inputDecoded }, childMemory: childMemory,
                totalInputBytes: TotalBytes(inputDirectory), inputIsIndex: true,
                method: "Untimed exact PNG decode/crop/nearest-resize/PNG-encode validation. Each subject launches one process, reads the same one or 32 distinct numbered PNG files and writes every resulting numbered PNG file. No looping, source preloading, output suppression or animation. Inputs are original phased RGBA8/16 samples. Independent destination-to-source integer coordinates include the crop offset and select the original sample polynomial directly. Native and public decoders must reproduce every output sample, including alpha and hidden RGB. Managed encoded outputs must also equal the direct complete public workflow. Native uses full-range integer RGB and separate replicated alpha in two zscale point contexts with automatic zimg CPU dispatch; extraction, merging and PNG endian conversion are included. Native PNG predictor none/compression level 6 and owned default filter/compression choices are explicit independent settings; compressed sizes are recorded, not quality or compression-effort matching. One-frame invocation latency and 32-frame amortized latency remain distinct. Child-memory counters and full validation are untimed; cleanup rechecks all input/output hashes and lengths. Filtered resize and other codec pipelines require separate coverage. Execution timeouts are guards, not performance budgets.");
            setupComplete = true;
        }
        catch { state.Dispose(); throw; }
    }

    // This reference reads neither the decoded buffer nor production crop/resize helpers.
    private byte[] OriginalReference(int cropWidth, int cropHeight, int frame)
    {
        var result = new byte[checked(targetWidth * targetHeight * 4 * (Depth / 8))];
        for (var y = 0; y < targetHeight; y++)
        for (var x = 0; x < targetWidth; x++)
        {
            var sx = CropX + (2L * x + 1) * cropWidth / (2L * targetWidth);
            var sy = CropY + (2L * y + 1) * cropHeight / (2L * targetHeight);
            for (var channel = 0; channel < 4; channel++)
            {
                var value = (sx * (12347 + sy * 101) + sy * 7919 + channel * 23459 + frame * (977 + channel * 313)) & 65535;
                var offset = ((y * targetWidth + x) * 4 + channel) * (Depth / 8);
                if (Depth == 8) result[offset] = (byte)((value + 128) / 257);
                else BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(offset), (ushort)value);
            }
        }
        return result;
    }

    private object DecodeNativeSequence(string directory, string reference, int width, int height, string name)
    {
        var output = Path.Combine(state!.DirectoryPath, name);
        OracleProcess.Run(state.Ffmpeg, ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-threads:v", "1",
            "-noautorotate", "-f", "image2", "-framerate", "25", "-start_number", "0", "-c:v", "png", "-i", Pattern(directory),
            "-frames:v", JpegBenchmarkCase.Number(Frames), "-fps_mode", "passthrough", "-threads:v", "1",
            "-pix_fmt", Depth == 16 ? "rgba64le" : "rgba", "-f", "rawvideo", "-y", output], timeout: ExecutionTimeout);
        var evidence = RawFrameEvidence.Validate(output, reference, Frames, checked(width * height * 4 * (Depth / 8)));
        File.Delete(output);
        return evidence;
    }

    private object Validate(string directory, bool managed)
    {
        RequireFiles(directory);
        var nativeDecoded = DecodeNativeSequence(directory, referencePath, targetWidth, targetHeight,
            managed ? "managed-decoded.raw" : "native-decoded.raw");
        using var reference = File.OpenRead(referencePath);
        var expected = new byte[checked(targetWidth * targetHeight * 4 * (Depth / 8))];
        var frames = new List<object>();
        for (var frame = 0; frame < Frames; frame++)
        {
            reference.ReadExactly(expected);
            var path = FramePath(directory, frame); var bytes = File.ReadAllBytes(path);
            var hash = JpegBenchmarkCase.Hash(bytes);
            if (managed && hash != publicEncodedHashes[frame]) throw new InvalidOperationException("Managed PNG differs from the complete public pipeline.");
            var info = ImageCodec.Identify(bytes);
            if ((info.Format, info.Width, info.Height, info.BitDepth) != (ImageFormat.Png, targetWidth, targetHeight, Depth))
                throw new InvalidOperationException("PNG pipeline output profile differs.");
            using var decoded = ImageCodec.Decode(bytes); var pixels = OriginalPixels.Bytes(decoded);
            if ((decoded.Width, decoded.Height, decoded.BitDepth) != (targetWidth, targetHeight, Depth)
                || !pixels.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("PNG pipeline decoded samples differ from original coordinates.");
            frames.Add(new { frame, name = Path.GetFileName(path), encodedBytes = bytes.Length, encodedSha256 = hash,
                decodedWidth = info.Width, decodedHeight = info.Height, decodedDepth = info.BitDepth,
                exact = true, allRgbaSamplesExact = true, alphaExact = true, hiddenRgbExact = true,
                managedDecodedSha256 = JpegBenchmarkCase.Hash(pixels), referenceSha256 = JpegBenchmarkCase.Hash(expected),
                publicEncodedSha256 = managed ? publicEncodedHashes[frame] : null });
        }
        if (reference.ReadByte() != -1) throw new InvalidOperationException("PNG pipeline reference has trailing frames.");
        return new { outputFiles = Frames, outputBytes = TotalBytes(directory), outputIndexSha256 = OutputIndex(directory),
            frames, nativeDecoded };
    }

    private object[] FileIndex(string directory)
    {
        RequireFiles(directory);
        return Enumerable.Range(0, Frames).Select(frame =>
        {
            var path = FramePath(directory, frame); var length = new FileInfo(path).Length;
            if (length == 0) throw new InvalidOperationException("PNG pipeline contains an empty file.");
            return (object)new { frame, name = Path.GetFileName(path), encodedBytes = length, encodedSha256 = JpegBenchmarkCase.HashFile(path) };
        }).ToArray();
    }
    private string OutputIndex(string directory) => JpegBenchmarkCase.Hash(JsonSerializer.SerializeToUtf8Bytes(FileIndex(directory)));
    private void RequireFiles(string directory)
    {
        if (Directory.GetFileSystemEntries(directory).Length != Frames
            || Enumerable.Range(0, Frames).Any(frame => !File.Exists(FramePath(directory, frame))))
            throw new InvalidOperationException("PNG pipeline contains a missing or unexpected file.");
    }
    private long TotalBytes(string directory) => Enumerable.Range(0, Frames).Sum(frame => new FileInfo(FramePath(directory, frame)).Length);
    private static string FramePath(string directory, int frame) => Path.Combine(directory, "frame-" + frame.ToString("D4", CultureInfo.InvariantCulture) + ".png");
    private static string Pattern(string directory) => Path.Combine(directory, "frame-%04d.png");
    private void RequireIndexes()
    {
        if (OutputIndex(inputDirectory) != inputIndex || OutputIndex(managedDirectory) != managedIndex || OutputIndex(nativeDirectory) != nativeIndex)
            throw new InvalidOperationException("PNG pipeline execution changed a validated input or output file.");
    }
    private long Run(string executable, string[] arguments, string directory)
    {
        OracleProcess.Run(executable, arguments, timeout: ExecutionTimeout); RequireFiles(directory);
        return TotalBytes(directory);
    }
    [Benchmark(Baseline = true)] public long Ffmpeg() => Run(state!.Ffmpeg, nativeArguments, nativeDirectory);
    [Benchmark] public long Managed() => Run("dotnet", managedArguments, managedDirectory);

    internal static void ValidateCleanupControls()
    {
        var controls = 0;
        foreach (var managed in new[] { false, true })
        foreach (var extra in new[] { false, true })
        {
            var workload = new PngCropResizePipeline { Width = 129, Depth = 16, Frames = 1, Upscale = false };
            try
            {
                workload.Setup(); var directory = workload.state!.DirectoryPath;
                var output = managed ? workload.managedDirectory : workload.nativeDirectory;
                if (extra) File.WriteAllText(Path.Combine(output, "unexpected"), "owned cleanup control");
                else
                {
                    using var file = File.Open(FramePath(output, 0), FileMode.Open, FileAccess.ReadWrite);
                    var first = file.ReadByte(); file.Position = 0; file.WriteByte((byte)(first ^ 1));
                }
                var rejected = false;
                try { workload.Cleanup(); }
                catch (InvalidOperationException) { rejected = true; }
                if (!rejected || Directory.Exists(directory)) throw new InvalidOperationException("PNG pipeline cleanup failed to reject changed output and remove its owned directory.");
                controls++;
            }
            finally { workload.Cleanup(); }
        }
        Console.WriteLine($"All {controls} PNG pipeline cleanup controls rejected changed outputs and removed owned temporary directories.");
    }
    [GlobalCleanup] public void Cleanup()
    {
        try { if (setupComplete) RequireIndexes(); }
        finally { state?.Dispose(); setupComplete = false; }
    }
}
