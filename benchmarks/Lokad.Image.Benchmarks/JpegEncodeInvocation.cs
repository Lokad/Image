using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>One process per subject: original RGBA8/RGBA16 raw file to baseline JPEG file.</summary>
[MemoryDiagnoser]
public class JpegEncodeInvocation
{
    [Params(129, 513)] public int Width { get; set; }
    [Params(8, 16)] public int Depth { get; set; }
    [Params(JpegSampling.YCbCr444, JpegSampling.YCbCr422, JpegSampling.YCbCr420)] public JpegSampling Sampling { get; set; }
    private JpegBenchmarkCase? state;
    private string[] nativeArguments = [], managedArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        state = new JpegBenchmarkCase(); var height = Width / 2 + 3;
        try
        {
            var source = OriginalPixels.Generate(Width, height, Depth, opaque: true); File.WriteAllBytes(state.Input, source);
            var reference = JpegQualityCorpus.Reference(source, Depth, null);
            if (Depth == 16 && !OracleProcess.Run(state.Ffmpeg, RgbaOracle.Normalize16Arguments(Width, height), source).AsSpan().SequenceEqual(reference))
                throw new InvalidOperationException("Native 16-bit input normalization differs.");
            nativeArguments = JpegBenchmarkCase.EncodeArguments(state.Input, state.NativeOutput, Width, height, Depth, Sampling);
            managedArguments = state.ManagedArguments("encode-jpeg", state.ManagedOutput, JpegBenchmarkCase.Number(Width), JpegBenchmarkCase.Number(height), JpegBenchmarkCase.Number(Depth), "85", Sampling.ToString());
            Ffmpeg(); Managed(); state.ValidateManagedEncode(source, Width, height, Depth, Sampling);
            state.Report(nameof(JpegEncodeInvocation), new { Width, height, Depth, Sampling, recipe = "asymmetric-v1", managedQuality = 85, nativeQscale = 5 }, managedArguments, nativeArguments,
                state.ValidateEncoded(state.ManagedOutput, reference, Width, height, Sampling), state.ValidateEncoded(state.NativeOutput, reference, Width, height, Sampling));
        }
        catch { state.Dispose(); throw; }
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
