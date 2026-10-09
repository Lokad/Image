using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

// Both families reuse these two original rasters; batch frames are exact repeats.
internal static class HorizontalFlipInput
{
    public const int Width = 513, Height = 259, Frames = 32;

    public static byte[] Create(int depth)
    {
        var bytes = OriginalPixels.Generate(Width, Height, depth);
        // Explicit hidden RGB at zero alpha, at both ends of the first row.
        bytes.AsSpan(3 * (depth / 8), depth / 8).Clear();
        bytes.AsSpan((Width * 4 - 1) * (depth / 8), depth / 8).Clear();
        return bytes;
    }

    public static byte[] Expected(byte[] source, int depth)
    {
        var pixelBytes = depth / 8 * 4;
        var result = new byte[source.Length];
        // Independent source-to-destination placement, outside measured methods.
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
            source.AsSpan((y * Width + x) * pixelBytes, pixelBytes)
                .CopyTo(result.AsSpan((y * Width + Width - 1 - x) * pixelBytes, pixelBytes));
        return result;
    }

    public static void Validate(ImageBuffer source, byte[] expected)
    {
        using var result = ImageTransforms.FlipHorizontal(source);
        if ((result.Width, result.Height, result.BitDepth, result.Orientation)
                != (source.Width, source.Height, source.BitDepth, source.Orientation)
            || !OriginalPixels.Bytes(result).AsSpan().SequenceEqual(expected))
            throw new InvalidOperationException("Horizontal flip differs from independent whole-pixel placement.");
    }
}

/// <summary>Warm public API, including result allocation; no native parity ratio.</summary>
[MemoryDiagnoser]
public class HorizontalFlipPrimitive
{
    [Params(8, 16)] public int Depth { get; set; }
    private ImageBuffer source = null!;
    private byte[] input = [], expected = [];

    [GlobalSetup]
    public void Setup()
    {
        if (Depth is not (8 or 16)) throw new ArgumentOutOfRangeException(nameof(Depth));
        input = HorizontalFlipInput.Create(Depth);
        expected = HorizontalFlipInput.Expected(input, Depth);
        source = JpegBenchmarkCase.Buffer(input, HorizontalFlipInput.Width, HorizontalFlipInput.Height, Depth);
        try
        {
            Validate();
            using var identity = new JpegBenchmarkCase();
            var directory = Path.Combine(identity.Root, "artifacts", "benchmark-validation");
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, nameof(HorizontalFlipPrimitive) + "-" + Guid.NewGuid().ToString("N") + ".json"),
                JsonSerializer.Serialize(new
                {
                    schemaVersion = 1, name = nameof(HorizontalFlipPrimitive),
                    runId = Environment.GetEnvironmentVariable("LOKAD_IMAGE_BENCHMARK_RUN_ID"),
                    width = source.Width, height = source.Height, depth = Depth,
                    method = "Warm public horizontal flip including allocation and disposal. Input prepared outside measurement. Exact independent forward placement before and after measurement, including alpha and hidden RGB. Diagnostic only; no FFmpeg parity claim.",
                    sourceSha256 = JpegBenchmarkCase.Hash(input), outputSha256 = JpegBenchmarkCase.Hash(expected),
                    librarySha256 = JpegBenchmarkCase.HashFile(typeof(ImageBuffer).Assembly.Location),
                    benchmarkAssemblySha256 = JpegBenchmarkCase.HashFile(typeof(HorizontalFlipPrimitive).Assembly.Location),
                    runtime = RuntimeInformation.FrameworkDescription, platform = RuntimeInformation.RuntimeIdentifier
                }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        }
        catch { source.Dispose(); throw; }
    }

    private void Validate()
    {
        HorizontalFlipInput.Validate(source, expected);
        if (!OriginalPixels.Bytes(source).AsSpan().SequenceEqual(input))
            throw new InvalidOperationException("Horizontal flip changed its source.");
    }

    [Benchmark]
    public int Managed()
    {
        using var result = ImageTransforms.FlipHorizontal(source);
        return result.Stride * result.Height;
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        try { Validate(); }
        finally { source.Dispose(); }
    }
}

/// <summary>One process per subject, identical repeated raw-file workload.</summary>
public class HorizontalFlipBatch
{
    [Params(8, 16)] public int Depth { get; set; }
    private JpegBenchmarkCase state = null!;
    private string[] managedArguments = [], nativeArguments = [];
    private string managedHash = "", nativeHash = "";
    private long outputBytes;
    private bool setupComplete;

    [GlobalSetup]
    public void Setup()
    {
        if (Depth is not (8 or 16)) throw new ArgumentOutOfRangeException(nameof(Depth));
        setupComplete = false;
        state = new JpegBenchmarkCase();
        try
        {
            var input = HorizontalFlipInput.Create(Depth);
            var expected = HorizontalFlipInput.Expected(input, Depth);
            using (var source = JpegBenchmarkCase.Buffer(input, HorizontalFlipInput.Width, HorizontalFlipInput.Height, Depth))
                HorizontalFlipInput.Validate(source, expected);
            var reference = Path.Combine(state.DirectoryPath, "reference.raw");
            using (var raw = File.Create(state.Input))
            using (var output = File.Create(reference))
                for (var frame = 0; frame < HorizontalFlipInput.Frames; frame++)
                { raw.Write(input); output.Write(expected); }
            outputBytes = (long)input.Length * HorizontalFlipInput.Frames;
            managedArguments = [state.Probe, "edit-sequence", state.Input, state.ManagedOutput,
                JpegBenchmarkCase.Number(HorizontalFlipInput.Width), JpegBenchmarkCase.Number(HorizontalFlipInput.Height),
                JpegBenchmarkCase.Number(Depth), JpegBenchmarkCase.Number(HorizontalFlipInput.Frames), "FlipHorizontal"];
            var format = Depth == 8 ? "rgba" : "rgba64le";
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-f", "rawvideo",
                "-pixel_format", format, "-video_size", "513x259", "-framerate", "25", "-i", state.Input,
                "-filter_threads", "1", "-vf", "hflip", "-frames:v", "32", "-fps_mode", "passthrough",
                "-threads:v", "1", "-c:v", "rawvideo", "-pix_fmt", format, "-f", "rawvideo", "-y", state.NativeOutput];
            Ffmpeg(); Managed();
            var managed = RawFrameEvidence.Validate(state.ManagedOutput, reference, HorizontalFlipInput.Frames, input.Length);
            var native = RawFrameEvidence.Validate(state.NativeOutput, reference, HorizontalFlipInput.Frames, input.Length);
            managedHash = JpegBenchmarkCase.HashFile(state.ManagedOutput);
            nativeHash = JpegBenchmarkCase.HashFile(state.NativeOutput);
            state.Report(nameof(HorizontalFlipBatch), new
                {
                    width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height, depth = Depth,
                    frames = HorizontalFlipInput.Frames, distinctInputs = 1, codecThreads = 1, filterThreads = 1,
                    cleanupOutputHashCheck = true
                }, managedArguments, nativeArguments, managed, native,
                method: "One process per subject streams 32 exact repeats of the same original RGBA raster from one raw file and writes every edited frame. Exact independent forward-placement validation includes alpha and hidden RGB. File transport, process/JIT startup and context reuse are included; this is amortized batch latency, not isolated kernel throughput. Separate untimed child-memory counters and final output hash checks.",
                inputs: new { recipe = "asymmetric-v1 with zero alpha at both first-row ends", rawFrameSha256 = JpegBenchmarkCase.Hash(input),
                    referenceFrameSha256 = JpegBenchmarkCase.Hash(expected), distinctInputs = 1, repeatedFrames = HorizontalFlipInput.Frames });
            RequireOutputs();
            setupComplete = true;
        }
        catch { state.Dispose(); throw; }
    }

    private void RequireOutputs()
    {
        RawFrameEvidence.RequireOutput(state.ManagedOutput, outputBytes, managedHash);
        RawFrameEvidence.RequireOutput(state.NativeOutput, outputBytes, nativeHash);
    }

    private long Run(string executable, string[] arguments, string output)
    {
        var bytes = state.Run(executable, arguments, output);
        if (bytes != outputBytes) throw new InvalidOperationException("Horizontal flip batch output length differs.");
        return bytes;
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => Run("dotnet", managedArguments, state.ManagedOutput);

    [GlobalCleanup]
    public void Cleanup()
    {
        try { if (setupComplete) RequireOutputs(); }
        finally { state?.Dispose(); setupComplete = false; }
    }
}
