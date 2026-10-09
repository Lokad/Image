using BenchmarkDotNet.Attributes;
using Lokad.Image.Tooling;

/// <summary>Equivalent raw-file editing boundaries, for single images and 32-image batches.</summary>
[MemoryDiagnoser]
public class GeometryRawFile
{
    [Params(129, 513, 1025, 2049)] public int Width { get; set; }
    [Params(8, 16)] public int Depth { get; set; }
    [Params(1, 32)] public int Frames { get; set; }
    [ParamsAllValues] public GeometryOperation Operation { get; set; }
    private JpegBenchmarkCase? state;
    private string[] managedArguments = [], nativeArguments = [];
    private string referencePath = "", managedHash = "", nativeHash = "";
    private long expectedOutputBytes;
    private bool setupComplete;
    private TimeSpan ExecutionTimeout => Width >= 1025 ? TimeSpan.FromMinutes(10) : TimeSpan.FromMinutes(1);

    [GlobalSetup]
    public void Setup()
    {
        setupComplete = false;
        if (Width is not (129 or 513 or 1025 or 2049) || Depth is not (8 or 16) || Frames is not (1 or 32) || !Enum.IsDefined(Operation))
            throw new ArgumentException("Invalid raw geometry benchmark profile.");
        state = new JpegBenchmarkCase();
        try
        {
            var height = Width / 2 + 3;
            var (targetWidth, targetHeight, cropX, cropY) = Geometry();
            var pixelBytes = 4 * (Depth / 8); var frameBytes = checked(targetWidth * targetHeight * pixelBytes);
            expectedOutputBytes = checked((long)frameBytes * Frames);
            referencePath = Path.Combine(state.DirectoryPath, "reference.raw");
            var receipts = new List<object>();
            using (var input = File.Create(state.Input))
            using (var reference = File.Create(referencePath))
            {
                for (var frame = 0; frame < Frames; frame++)
                {
                    var pixels = OriginalPixels.Generate(Width, height, Depth, phase: frame); input.Write(pixels);
                    var expected = MapForward(pixels, height, targetWidth, targetHeight, cropX, cropY);
                    using var image = JpegBenchmarkCase.Buffer(pixels, Width, height, Depth);
                    using var actual = GeometryWorkload.Apply(image, Operation);
                    var publicPixels = OriginalPixels.Bytes(actual);
                    if ((actual.Width, actual.Height, actual.BitDepth) != (targetWidth, targetHeight, Depth)
                        || !publicPixels.AsSpan().SequenceEqual(expected))
                        throw new InvalidOperationException("Public edit workflow differs from independent forward-coordinate samples.");
                    reference.Write(expected);
                    receipts.Add(new { frame, rawOffset = (long)frame * pixels.Length, rawBytes = pixels.Length,
                        recipe = "asymmetric-v1-phase: add phase*977+phase*channel*313 modulo 65536 before the declared depth reduction",
                        rawSha256 = JpegBenchmarkCase.Hash(pixels), expectedSha256 = JpegBenchmarkCase.Hash(expected),
                        publicEditedSha256 = JpegBenchmarkCase.Hash(publicPixels) });
                }
            }
            managedArguments = [state.Probe, "edit-sequence", state.Input, state.ManagedOutput,
                JpegBenchmarkCase.Number(Width), JpegBenchmarkCase.Number(height), JpegBenchmarkCase.Number(Depth),
                JpegBenchmarkCase.Number(Frames), Operation.ToString()];
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-f", "rawvideo",
                "-pixel_format", Depth == 16 ? "rgba64le" : "rgba", "-video_size", Width + "x" + height,
                "-framerate", "25", "-i", state.Input, "-filter_threads", "1", "-vf", NativeFilter(targetWidth, targetHeight, cropX, cropY),
                "-frames:v", JpegBenchmarkCase.Number(Frames), "-fps_mode", "passthrough", "-threads:v", "1",
                "-c:v", "rawvideo", "-pix_fmt", Depth == 16 ? "rgba64le" : "rgba", "-f", "rawvideo", "-y", state.NativeOutput];
            Ffmpeg(); Managed();
            var managed = RawFrameEvidence.Validate(state.ManagedOutput, referencePath, Frames, frameBytes);
            var native = RawFrameEvidence.Validate(state.NativeOutput, referencePath, Frames, frameBytes);
            managedHash = JpegBenchmarkCase.HashFile(state.ManagedOutput); nativeHash = JpegBenchmarkCase.HashFile(state.NativeOutput);
            var childMemory = new
            {
                native = ChildProcessMemory.Measure(state.Ffmpeg, nativeArguments, state.DirectoryPath, timeout: ExecutionTimeout),
                managed = ChildProcessMemory.Measure("dotnet", managedArguments, state.DirectoryPath, timeout: ExecutionTimeout)
            };
            RequireOutputHashes();
            state.Report(nameof(GeometryRawFile), new { Width, height, depth = Depth, frames = Frames, Operation,
                targetWidth, targetHeight, cropX, cropY, outputFrameBytes = frameBytes,
                family = Frames == 1 ? "raw-file edit invocation" : "amortized-startup raw-file edit",
                codecThreads = 1, filterThreads = 1, operationTimeoutSeconds = (int)ExecutionTimeout.TotalSeconds,
                cleanupOutputHashCheck = true, referenceValidation = "streamed-original-reference-file" }, managedArguments, nativeArguments, managed, native,
                method: "Untimed validation of exact RGBA8/RGBA16LE geometry. Both subjects launch one process, stream every source frame once from the same raw input file, and write all edited frames to one raw output file. No codec decode/encode, source preloading, looping or output suppression. Independent source-to-destination coordinate placement and exact public API samples prove every channel, including alpha and hidden RGB; frame count, order, geometry and precision are explicit. Native cropping uses exact=1 and explicit rectangles; a half turn uses hflip,vflip. Remaining startup/JIT, filter context and filesystem/raw transport costs are included. One frame is invocation latency; 32-frame latency is an amortized batch, divided by 32 for per-image figures. Kernel child-memory counters run separately outside benchmark methods.",
                inputs: new { rawFrames = Frames, rawFileBytes = new FileInfo(state.Input).Length,
                    rawFileSha256 = JpegBenchmarkCase.HashFile(state.Input), referenceFileBytes = expectedOutputBytes,
                    referenceFileSha256 = JpegBenchmarkCase.HashFile(referencePath), frames = receipts }, childMemory: childMemory);
            setupComplete = true;
        }
        catch { state.Dispose(); throw; }
    }

    private (int Width, int Height, int X, int Y) Geometry()
    {
        var height = Width / 2 + 3;
        return Operation switch
        {
            GeometryOperation.Crop => (Width / 2, height / 2, 3, 5),
            GeometryOperation.CropCentered => (Width / 2, height / 2, (Width - Width / 2) / 2, (height - height / 2) / 2),
            GeometryOperation.CropClamped => (Width / 2, height / 2, Width - Width / 2, 0),
            GeometryOperation.Clockwise90 or GeometryOperation.CounterClockwise90 or GeometryOperation.Transpose or GeometryOperation.Transverse => (height, Width, 0, 0),
            _ => (Width, height, 0, 0)
        };
    }

    // Forward placement contrasts with the library's destination-to-source map.
    private byte[] MapForward(byte[] source, int height, int width, int targetHeight, int cropX, int cropY)
    {
        var pixelBytes = 4 * (Depth / 8); var output = new byte[width * targetHeight * pixelBytes];
        var cropped = Operation is GeometryOperation.Crop or GeometryOperation.CropCentered or GeometryOperation.CropClamped;
        for (var sy = 0; sy < height; sy++)
        for (var sx = 0; sx < Width; sx++)
        {
            if (cropped && (sx < cropX || sy < cropY || sx >= cropX + width || sy >= cropY + targetHeight)) continue;
            var (dx, dy) = Operation switch
            {
                GeometryOperation.Crop or GeometryOperation.CropCentered or GeometryOperation.CropClamped => (sx - cropX, sy - cropY),
                GeometryOperation.FlipHorizontal => (Width - sx - 1, sy),
                GeometryOperation.FlipVertical => (sx, height - sy - 1),
                GeometryOperation.Clockwise90 => (height - sy - 1, sx),
                GeometryOperation.Half => (Width - sx - 1, height - sy - 1),
                GeometryOperation.CounterClockwise90 => (sy, Width - sx - 1),
                GeometryOperation.Transpose => (sy, sx),
                GeometryOperation.Transverse => (height - sy - 1, Width - sx - 1),
                _ => throw new InvalidOperationException("Unexpected exact edit profile.")
            };
            source.AsSpan((sy * Width + sx) * pixelBytes, pixelBytes).CopyTo(output.AsSpan((dy * width + dx) * pixelBytes, pixelBytes));
        }
        return output;
    }

    private string NativeFilter(int width, int height, int x, int y)
        => Operation switch
        {
            GeometryOperation.Crop or GeometryOperation.CropCentered or GeometryOperation.CropClamped => $"crop=w={width}:h={height}:x={x}:y={y}:exact=1",
            GeometryOperation.FlipHorizontal => "hflip",
            GeometryOperation.FlipVertical => "vflip",
            GeometryOperation.Clockwise90 => "transpose=dir=clock:passthrough=none",
            GeometryOperation.Half => "hflip,vflip",
            GeometryOperation.CounterClockwise90 => "transpose=dir=cclock:passthrough=none",
            GeometryOperation.Transpose => "transpose=dir=cclock_flip:passthrough=none",
            GeometryOperation.Transverse => "transpose=dir=clock_flip:passthrough=none",
            _ => throw new InvalidOperationException("Unexpected exact edit profile.")
        };

    private void RequireOutputHashes()
    {
        RawFrameEvidence.RequireOutput(state!.ManagedOutput, expectedOutputBytes, managedHash);
        RawFrameEvidence.RequireOutput(state.NativeOutput, expectedOutputBytes, nativeHash);
    }

    private long Run(string executable, string[] arguments, string output)
    {
        OracleProcess.Run(executable, arguments, timeout: ExecutionTimeout);
        var bytes = new FileInfo(output).Length;
        if (bytes != expectedOutputBytes) throw new InvalidOperationException("Raw geometry output length differs.");
        return bytes;
    }
    [Benchmark(Baseline = true)] public long Ffmpeg() => Run(state!.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => Run("dotnet", managedArguments, state!.ManagedOutput);
    internal static void ValidateCleanupControls() => RawFrameEvidence.ValidateCleanupControls(nameof(GeometryRawFile), () =>
    {
        var workload = new GeometryRawFile { Width = 129, Depth = 16, Frames = 1, Operation = GeometryOperation.FlipHorizontal };
        return (workload.Setup, workload.Cleanup, () => workload.state!.DirectoryPath,
            () => workload.state!.NativeOutput, () => workload.state!.ManagedOutput);
    });
    [GlobalCleanup] public void Cleanup()
    {
        try { if (setupComplete) RequireOutputHashes(); }
        finally { state?.Dispose(); setupComplete = false; }
    }
}
