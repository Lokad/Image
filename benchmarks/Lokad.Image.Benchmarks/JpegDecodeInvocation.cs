using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>One process per subject: JPEG file to tightly packed RGBA8 file.</summary>
[MemoryDiagnoser]
public class JpegDecodeInvocation
{
    [Params(129, 513)] public int Width { get; set; }
    [Params(false, true)] public bool Progressive { get; set; }
    [Params(JpegSampling.Grayscale, JpegSampling.YCbCr444, JpegSampling.YCbCr422, JpegSampling.YCbCr420)] public JpegSampling Sampling { get; set; }
    private JpegBenchmarkCase? state;
    private string[] nativeArguments = [], managedArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        state = new JpegBenchmarkCase(); var height = Width / 2 + 3;
        try
        {
            state.CreateInput(Width, height, Sampling, Progressive);
            nativeArguments = JpegBenchmarkCase.DecodeArguments(state.Input, state.NativeOutput, Width, height, Sampling);
            managedArguments = state.ManagedArguments("decode-raw", state.ManagedOutput);
            Ffmpeg(); Managed();
            var reference = JpegOracle.Decode(File.ReadAllBytes(state.Input), Width, height, Sampling, state.Ffmpeg);
            state.Report(nameof(JpegDecodeInvocation), new { Width, height, Progressive, Sampling, output = "tightly-packed-RGBA8" }, managedArguments, nativeArguments,
                state.ValidateRaw(state.ManagedOutput, reference), state.ValidateRaw(state.NativeOutput, reference));
        }
        catch { state.Dispose(); throw; }
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
