using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

[MemoryDiagnoser]
public class Vp8FlipInvocation
{
    [Params(false, true)] public bool Opaque { get; set; }
    private const int Width = 129, Height = 67;
    private JpegBenchmarkCase? state;
    private string[] nativeArguments = [], managedArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        state = new JpegBenchmarkCase(".png");
        try
        {
            var source = OriginalPixels.Generate(Width, Height, 8, opaque: Opaque);
            var encoded = WebpOracle.Encode(source, Width, Height, 6, 75, state.Ffmpeg);
            File.WriteAllBytes(state.Input, encoded);
            // Expand and convert before flipping, preserving the specified grid at odd dimensions.
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-filter_complex_threads", "1", "-noautorotate", "-i", state.Input,
                "-filter_complex", WebpOracle.Filter(Width, Height, !Opaque) + ",hflip", "-frames:v", "1", "-threads:v", "1", "-c:v", "png",
                "-pix_fmt", "rgba", "-pred", "none", "-y", state.NativeOutput];
            managedArguments = state.ManagedArguments("flip", state.ManagedOutput);
            Ffmpeg(); Managed();
            var nativeDecoded = WebpOracle.Decode(encoded, Width, Height, !Opaque, state.Ffmpeg);
            var reference = InvocationValidation.Flip(nativeDecoded, Width, Height, 8);
            using var decoded = ImageCodec.Decode(encoded);
            var publicDecoded = OriginalPixels.Bytes(decoded); var publicFlipped = InvocationValidation.Flip(publicDecoded, Width, Height, 8);
            InvocationValidation.RequirePngWorkflow(state, publicFlipped, Width, Height, 8);
            state.Report(nameof(Vp8FlipInvocation), new { Width, Height, Opaque, frames = 1, codecThreads = 1 }, managedArguments, nativeArguments,
                InvocationValidation.Png(state, state.ManagedOutput, Width, Height, 8, reference, allowRgbDifference: true),
                InvocationValidation.Png(state, state.NativeOutput, Width, Height, 8, reference),
                stages: new { sourceSha256 = JpegBenchmarkCase.Hash(source), nativeDecodedSha256 = JpegBenchmarkCase.Hash(nativeDecoded),
                    publicDecodedSha256 = JpegBenchmarkCase.Hash(publicDecoded), nativeFlippedSha256 = JpegBenchmarkCase.Hash(reference),
                    publicFlippedSha256 = JpegBenchmarkCase.Hash(publicFlipped),
                    reference = "independent horizontal pixel placement after explicit limited-range BT.601 conversion through RGBA16; managed maximum RGB error 4/RMSE 1.5, alpha exact" },
                method: InvocationValidation.FlipMethod);
        }
        catch { state.Dispose(); throw; }
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
