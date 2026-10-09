using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>Exact nearest RGBA8/16 file workloads, including transparent pixels and both scale directions.</summary>
[MemoryDiagnoser]
public class NearestResizeRawFile
{
    [Params(129, 513, 1025, 2049)] public int Width { get; set; }
    [Params(8, 16)] public int Depth { get; set; }
    [Params(1, 32)] public int Frames { get; set; }
    [Params(false, true)] public bool Upscale { get; set; }
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
        if (Width is not (129 or 513 or 1025 or 2049) || Depth is not (8 or 16) || Frames is not (1 or 32))
            throw new ArgumentException("Invalid nearest resize benchmark profile.");
        state = new JpegBenchmarkCase();
        try
        {
            var height = Width / 2 + 3;
            var targetWidth = Upscale ? Width * 2 - 1 : Width / 2;
            var targetHeight = Upscale ? height * 2 + 1 : height / 2;
            var frameBytes = checked(targetWidth * targetHeight * 4 * (Depth / 8));
            expectedOutputBytes = checked((long)frameBytes * Frames);
            referencePath = Path.Combine(state.DirectoryPath, "reference.raw");
            var receipts = new List<object>();
            using (var input = File.Create(state.Input))
            using (var reference = File.Create(referencePath))
            {
                for (var frame = 0; frame < Frames; frame++)
                {
                    var pixels = OriginalPixels.Generate(Width, height, Depth, phase: frame); input.Write(pixels);
                    var expected = ExactResizeReference.Resize(pixels, Width, height, Depth, targetWidth, targetHeight, ResizeFilter.Nearest);
                    using var image = JpegBenchmarkCase.Buffer(pixels, Width, height, Depth);
                    using var resized = ImageTransforms.Resize(image, targetWidth, targetHeight, ResizeFilter.Nearest);
                    var publicPixels = OriginalPixels.Bytes(resized);
                    if ((resized.Width, resized.Height, resized.BitDepth) != (targetWidth, targetHeight, Depth)
                        || !publicPixels.AsSpan().SequenceEqual(expected))
                        throw new InvalidOperationException("Public nearest workflow differs from exact integer source-cell coordinates.");
                    reference.Write(expected);
                    receipts.Add(new { frame, rawOffset = (long)frame * pixels.Length, rawBytes = pixels.Length,
                        recipe = "asymmetric-v1-phase: add phase*977+phase*channel*313 modulo 65536 before the declared depth reduction",
                        rawSha256 = JpegBenchmarkCase.Hash(pixels), expectedSha256 = JpegBenchmarkCase.Hash(expected),
                        publicResizedSha256 = JpegBenchmarkCase.Hash(publicPixels) });
                }
            }
            managedArguments = [state.Probe, "resize-sequence", state.Input, state.ManagedOutput,
                JpegBenchmarkCase.Number(Width), JpegBenchmarkCase.Number(height), JpegBenchmarkCase.Number(Depth),
                JpegBenchmarkCase.Number(Frames), JpegBenchmarkCase.Number(targetWidth), JpegBenchmarkCase.Number(targetHeight), "Nearest"];
            var native = ResizeOracle.NearestArguments(Width, height, targetWidth, targetHeight, Depth, Frames).ToList();
            native.RemoveRange(native.IndexOf("-cpuflags"), 2); // Normal native CPU capabilities in benchmark methods.
            native[native.IndexOf("-i") + 1] = state.Input;
            native.Insert(native.Count - 1, "-y"); native[^1] = state.NativeOutput;
            nativeArguments = native.ToArray();
            Ffmpeg(); Managed();
            var managedResult = RawFrameEvidence.Validate(state.ManagedOutput, referencePath, Frames, frameBytes);
            var nativeResult = RawFrameEvidence.Validate(state.NativeOutput, referencePath, Frames, frameBytes);
            managedHash = JpegBenchmarkCase.HashFile(state.ManagedOutput); nativeHash = JpegBenchmarkCase.HashFile(state.NativeOutput);
            var childMemory = new
            {
                native = ChildProcessMemory.Measure(state.Ffmpeg, nativeArguments, state.DirectoryPath, timeout: ExecutionTimeout),
                managed = ChildProcessMemory.Measure("dotnet", managedArguments, state.DirectoryPath, timeout: ExecutionTimeout)
            };
            RequireOutputHashes();
            state.Report(nameof(NearestResizeRawFile), new { Width, height, depth = Depth, frames = Frames, Upscale,
                targetWidth, targetHeight, filter = "Nearest", outputFrameBytes = frameBytes,
                family = Frames == 1 ? "raw-file resize invocation" : "amortized-startup raw-file resize",
                codecThreads = 1, filterThreads = 1, nativeScalarPlanes = 4,
                nativeResizeImplementation = "zscale-point-rgb-and-alpha", nativePointContexts = 2, nativeAlphaReplicatedPlanes = 3,
                operationTimeoutSeconds = (int)ExecutionTimeout.TotalSeconds,
                cleanupOutputHashCheck = true, referenceValidation = "streamed-original-reference-file" }, managedArguments, nativeArguments, managedResult, nativeResult,
                method: "Untimed exact nearest validation. Both subjects launch one process, read the same RGBA8/RGBA16LE raw file once and write every resized frame to one raw file. No image codec, looping, source preloading or output suppression. The independent reference selects source cells with integer rational center coordinates; public API and both complete outputs must match every RGBA sample, including alpha and hidden RGB. Native extraction and plane merging create separate integer RGB and replicated-alpha RGB inputs for two zscale point contexts with unchanged full-range GBR samples, no dithering and automatic zimg CPU dispatch. Separate alpha avoids clearing hidden RGB; explicit formats, extraction, SAR normalization, final merging and packed output preserve precision. All these filter/process/runtime/file costs belong to the baseline. This uses the native point scaler, not a coordinate-expression evaluator. One frame measures invocation latency; divide 32-frame batch latency by 32 for an amortized per-image figure. Child-memory counters run separately outside benchmark methods. Bilinear/bicubic native equivalence is not established by these cases.",
                inputs: new { rawFrames = Frames, rawFileBytes = new FileInfo(state.Input).Length,
                    rawFileSha256 = JpegBenchmarkCase.HashFile(state.Input), referenceFileBytes = expectedOutputBytes,
                    referenceFileSha256 = JpegBenchmarkCase.HashFile(referencePath), frames = receipts }, childMemory: childMemory);
            setupComplete = true;
        }
        catch { state.Dispose(); throw; }
    }

    private void RequireOutputHashes()
    {
        RawFrameEvidence.RequireOutput(state!.ManagedOutput, expectedOutputBytes, managedHash);
        RawFrameEvidence.RequireOutput(state.NativeOutput, expectedOutputBytes, nativeHash);
    }

    private long Run(string executable, string[] arguments, string output)
    {
        OracleProcess.Run(executable, arguments, timeout: ExecutionTimeout);
        var bytes = new FileInfo(output).Length;
        if (bytes != expectedOutputBytes) throw new InvalidOperationException("Nearest resize output length differs.");
        return bytes;
    }
    [Benchmark(Baseline = true)] public long Ffmpeg() => Run(state!.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => Run("dotnet", managedArguments, state!.ManagedOutput);
    internal static void ValidateCleanupControls() => RawFrameEvidence.ValidateCleanupControls(nameof(NearestResizeRawFile), () =>
    {
        var workload = new NearestResizeRawFile { Width = 129, Depth = 16, Frames = 1, Upscale = false };
        return (workload.Setup, workload.Cleanup, () => workload.state!.DirectoryPath,
            () => workload.state!.NativeOutput, () => workload.state!.ManagedOutput);
    });
    [GlobalCleanup] public void Cleanup()
    {
        try { if (setupComplete) RequireOutputHashes(); }
        finally { state?.Dispose(); setupComplete = false; }
    }
}
