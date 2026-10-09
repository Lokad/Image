using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

[MemoryDiagnoser]
public class PngFlipInvocation
{
    [Params(8, 16)] public int Depth { get; set; }
    private const int Width = 256, Height = 192;
    private JpegBenchmarkCase? state;
    private string[] nativeArguments = [], managedArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        state = new JpegBenchmarkCase(".png");
        try
        {
            using var pixels = OriginalPixels.Buffer(Width, Height, Depth);
            using (var output = File.Create(state.Input)) ImageCodec.EncodePng(pixels, output);
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-filter_threads", "1", "-noautorotate", "-i", state.Input, "-vf", "hflip",
                "-frames:v", "1", "-threads:v", "1", "-c:v", "png", "-pix_fmt", Depth == 16 ? "rgba64be" : "rgba", "-pred", "none", "-y", state.NativeOutput];
            managedArguments = state.ManagedArguments("flip", state.ManagedOutput);
            Ffmpeg(); Managed();
            var source = OriginalPixels.Bytes(pixels); var expected = InvocationValidation.Flip(source, Width, Height, Depth);
            InvocationValidation.RequirePngWorkflow(state, expected, Width, Height, Depth);
            state.Report(nameof(PngFlipInvocation), new { Width, Height, Depth, frames = 1, codecThreads = 1 }, managedArguments, nativeArguments,
                InvocationValidation.Png(state, state.ManagedOutput, Width, Height, Depth, expected),
                InvocationValidation.Png(state, state.NativeOutput, Width, Height, Depth, expected),
                stages: new { sourceSha256 = JpegBenchmarkCase.Hash(source), flippedSha256 = JpegBenchmarkCase.Hash(expected),
                    reference = "independent horizontal source-to-destination pixel placement; all samples including hidden RGB exact" },
                method: InvocationValidation.FlipMethod);
        }
        catch { state.Dispose(); throw; }
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
