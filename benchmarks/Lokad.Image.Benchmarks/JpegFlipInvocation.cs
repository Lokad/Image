using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>Equal process/file boundaries: JPEG decode, horizontal flip, PNG encode.</summary>
[MemoryDiagnoser]
public class JpegFlipInvocation
{
    [Params(false, true)] public bool Progressive { get; set; }
    private const int Width = 128, Height = 96;
    private JpegBenchmarkCase? state;
    private string[] nativeArguments = [], managedArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        state = new JpegBenchmarkCase(".png");
        try
        {
            var encoded = Progressive ? AuthoredJpeg.ProgressiveColor(Width, Height, JpegSampling.YCbCr420, 4)
                : OracleProcess.Run(state.Ffmpeg,
                    ["-hide_banner", "-loglevel", "error", "-f", "rawvideo", "-pixel_format", "rgba", "-video_size", $"{Width}x{Height}",
                     "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-c:v", "mjpeg", "-pix_fmt", "yuvj420p", "-q:v", "3", "-f", "image2pipe", "pipe:1"],
                    OriginalPixels.Generate(Width, Height, 8, opaque: true));
            File.WriteAllBytes(state.Input, encoded);
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-filter_complex_threads", "1", "-noautorotate", "-i", state.Input,
                "-filter_complex", JpegOracle.Filter(Width, Height, JpegSampling.YCbCr420) + ",hflip",
                "-frames:v", "1", "-threads:v", "1", "-c:v", "png", "-pix_fmt", "rgba", "-pred", "none", "-y", state.NativeOutput];
            managedArguments = state.ManagedArguments("flip", state.ManagedOutput);
            Ffmpeg(); Managed();
            var nativeDecoded = JpegOracle.Decode(encoded, Width, Height, JpegSampling.YCbCr420, state.Ffmpeg);
            var reference = InvocationValidation.Flip(nativeDecoded, Width, Height, 8);
            using var decoded = ImageCodec.Decode(encoded);
            var publicDecoded = OriginalPixels.Bytes(decoded); var publicFlipped = InvocationValidation.Flip(publicDecoded, Width, Height, 8);
            InvocationValidation.RequirePngWorkflow(state, publicFlipped, Width, Height, 8);
            var header = JpegQualitySyntax.Read(encoded);
            if (header.Sampling != "2x2,1x1,1x1" || header.Components != 3 || header.Depth != 8)
                throw new InvalidOperationException("JPEG flip input profile differs.");
            state.Report(nameof(JpegFlipInvocation), new { Width, Height, Progressive, sampling = "YCbCr420", frames = 1, codecThreads = 1 },
                managedArguments, nativeArguments,
                InvocationValidation.Png(state, state.ManagedOutput, Width, Height, 8, reference, allowRgbDifference: true),
                InvocationValidation.Png(state, state.NativeOutput, Width, Height, 8, reference),
                stages: new { nativeDecodedSha256 = JpegBenchmarkCase.Hash(nativeDecoded), publicDecodedSha256 = JpegBenchmarkCase.Hash(publicDecoded),
                    nativeFlippedSha256 = JpegBenchmarkCase.Hash(reference), publicFlippedSha256 = JpegBenchmarkCase.Hash(publicFlipped),
                    header.QuantizationHash, header.Sampling, reference = "independent horizontal pixel placement after explicit JPEG conversion; managed maximum RGB error 4/RMSE 1.5, alpha exact" },
                method: InvocationValidation.FlipMethod);
        }
        catch { state.Dispose(); throw; }
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
