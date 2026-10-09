using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

[MemoryDiagnoser]
public class WebpLosslessFlipInvocation
{
    [Params(false, true)] public bool RepeatedColor { get; set; }
    private const int Width = 128, Height = 96;
    private JpegBenchmarkCase? state;
    private string[] nativeArguments = [], managedArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        state = new JpegBenchmarkCase(".png");
        try
        {
            using var pixels = OriginalPixels.Buffer(Width, Height, 8);
            if (RepeatedColor)
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                    new byte[] { 10, 20, 30, 40 }.CopyTo(pixels.GetRow8(y)[(x * 4)..]);
            var source = OriginalPixels.Bytes(pixels);
            var encoded = OracleProcess.Run(state.Ffmpeg,
                ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-filter_threads", "1", "-f", "rawvideo",
                 "-pix_fmt", "rgba", "-s", "128x96", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-c:v", "libwebp",
                 "-lossless", "1", "-compression_level", "6", "-f", "webp", "pipe:1"], source);
            File.WriteAllBytes(state.Input, encoded);
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-filter_threads", "1", "-noautorotate", "-i", state.Input,
                "-vf", "hflip", "-frames:v", "1", "-threads:v", "1", "-c:v", "png", "-pix_fmt", "rgba", "-pred", "none", "-y", state.NativeOutput];
            managedArguments = state.ManagedArguments("flip", state.ManagedOutput);
            Ffmpeg(); Managed();
            var nativeDecoded = OracleProcess.Run(state.Ffmpeg,
                ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0",
                 "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"], encoded);
            var changedHiddenPixels = InvocationValidation.LosslessInput(source, nativeDecoded);
            using var decoded = ImageCodec.Decode(encoded);
            if (!OriginalPixels.Bytes(decoded).AsSpan().SequenceEqual(nativeDecoded))
                throw new InvalidOperationException("Lossless WebP invocation input decoder samples differ.");
            var reference = InvocationValidation.Flip(nativeDecoded, Width, Height, 8);
            InvocationValidation.RequirePngWorkflow(state, reference, Width, Height, 8);
            state.Report(nameof(WebpLosslessFlipInvocation), new { Width, Height, RepeatedColor, frames = 1, codecThreads = 1 },
                managedArguments, nativeArguments,
                InvocationValidation.Png(state, state.ManagedOutput, Width, Height, 8, reference),
                InvocationValidation.Png(state, state.NativeOutput, Width, Height, 8, reference),
                stages: new { sourceSha256 = JpegBenchmarkCase.Hash(source), decodedSha256 = JpegBenchmarkCase.Hash(nativeDecoded),
                    flippedSha256 = JpegBenchmarkCase.Hash(reference), changedHiddenPixels,
                    inputPolicy = "original native-encoded VP8L input retained; only zero-alpha RGB may change in native fixture encoding; both subjects must preserve every sample in the actual encoded input" },
                method: InvocationValidation.FlipMethod);
        }
        catch { state.Dispose(); throw; }
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
