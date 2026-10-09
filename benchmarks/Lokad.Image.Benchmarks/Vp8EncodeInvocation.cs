using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

[MemoryDiagnoser]
public class Vp8EncodeInvocation
{
    [Params(false, true)] public bool Opaque { get; set; }
    private const int Width = 129, Height = 67;
    private JpegBenchmarkCase? state;
    private string[] nativeArguments = [], managedArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        state = new JpegBenchmarkCase(".webp");
        try
        {
            using var image = OriginalPixels.Buffer(Width, Height, 8, opaque: Opaque); var source = OriginalPixels.Bytes(image);
            using (var stream = File.Create(state.Input)) ImageCodec.EncodePng(image, stream);
            // Same file/process boundary; these encoder-specific settings do not imply equal quality.
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-noautorotate", "-i", state.Input,
                "-frames:v", "1", "-threads:v", "1", "-c:v", "libwebp", "-lossless", "0", "-quality", "85",
                "-compression_level", "4", "-y", state.NativeOutput];
            managedArguments = state.ManagedArguments("webp", state.ManagedOutput, "85", "4");
            Ffmpeg(); Managed();
            using var publicOutput = new MemoryStream();
            ImageCodec.EncodeWebp(image, publicOutput, new WebpEncodeOptions { Lossless = false, Quality = 85, Effort = 4 });
            if (!publicOutput.ToArray().AsSpan().SequenceEqual(File.ReadAllBytes(state.ManagedOutput)))
                throw new InvalidOperationException("ImageProbe VP8 differs from the complete public PNG-decode/WebP-encode workflow.");
            state.Report(nameof(Vp8EncodeInvocation), new { Width, Height, Opaque, frames = 1, codecThreads = 1,
                managedQuality = 85, nativeQuality = 85, managedEffort = 4, nativeEffort = 4, matchedQuality = false },
                managedArguments, nativeArguments,
                InvocationValidation.Vp8(state, state.ManagedOutput, Width, Height, source, !Opaque),
                InvocationValidation.Vp8(state, state.NativeOutput, Width, Height, source, !Opaque),
                stages: new { sourceSha256 = JpegBenchmarkCase.Hash(source), publicEncodedSha256 = JpegBenchmarkCase.Hash(publicOutput.ToArray()) },
                method: "Untimed validation of one PNG still file to one lossy VP8 WebP file, with one process and equivalent file input/output per subject. Both outputs must have the original dimensions, a VP8 image chunk and exact source alpha. Each output is decoded independently by FFmpeg and the public API; decoder RGB agreement retains maximum 4/RMSE 1.5. Source RGB error and encoded size are recorded separately for each encoder. Quality 85 and effort 4 are encoder-specific, not matched quality or effort. Linux kernel child-memory samples run separately and must preserve each validated encoded output hash. No timing measurement or isolated encoder-throughput claim.");
        }
        catch { state.Dispose(); throw; }
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
