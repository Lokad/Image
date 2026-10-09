using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>One process per subject: baseline/progressive JPEG, opaque downsize, baseline JPEG.</summary>
[MemoryDiagnoser]
public class JpegResizeTranscodeInvocation
{
    [Params(129, 513)] public int Width { get; set; }
    [Params(false, true)] public bool Progressive { get; set; }
    [Params(ResizeFilter.Nearest, ResizeFilter.Bilinear, ResizeFilter.Bicubic)] public ResizeFilter Filter { get; set; }
    private JpegBenchmarkCase? state;
    private string[] nativeArguments = [], managedArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        state = new JpegBenchmarkCase(); var height = Width / 2 + 3; var targetWidth = Width / 2; var targetHeight = height / 2;
        try
        {
            state.CreateInput(Width, height, JpegSampling.YCbCr420, Progressive);
            nativeArguments = JpegBenchmarkCase.ResizeEncodeArguments(state.Input, state.NativeOutput, Width, height, targetWidth, targetHeight, Filter);
            managedArguments = state.ManagedArguments("resize-jpeg", state.ManagedOutput, JpegBenchmarkCase.Number(targetWidth), JpegBenchmarkCase.Number(targetHeight), Filter.ToString(), "85", nameof(JpegSampling.YCbCr420));
            var nativeInputPath = Path.Combine(state.DirectoryPath, "native-decoded.rgba"); var nativeStagePath = Path.Combine(state.DirectoryPath, "native-resized.rgba");
            OracleProcess.Run(state.Ffmpeg, JpegBenchmarkCase.DecodeArguments(state.Input, nativeInputPath, Width, height, JpegSampling.YCbCr420));
            OracleProcess.Run(state.Ffmpeg, JpegBenchmarkCase.DecodeArguments(state.Input, nativeStagePath, Width, height, JpegSampling.YCbCr420, ResizeOracle.Filter(targetWidth, targetHeight, Filter)));
            var nativeInput = File.ReadAllBytes(nativeInputPath); var nativeStage = File.ReadAllBytes(nativeStagePath);
            using var inputPixels = JpegBenchmarkCase.Buffer(nativeInput, Width, height);
            using var expectedNativeResize = ImageTransforms.Resize(inputPixels, targetWidth, targetHeight, Filter);
            var resizeDifference = JpegQualityCorpus.Compare(OriginalPixels.Bytes(expectedNativeResize), nativeStage);
            if (resizeDifference.Maximum > (Filter == ResizeFilter.Nearest ? 0 : 2) || resizeDifference.Rmse > 0.5)
                throw new InvalidOperationException($"Native opaque resize profile differs: maximum {resizeDifference.Maximum}, RMSE {resizeDifference.Rmse}.");
            using var managedInput = ImageCodec.Decode(File.ReadAllBytes(state.Input));
            using var managedResize = ImageTransforms.Resize(managedInput, targetWidth, targetHeight, Filter);
            var managedStage = OriginalPixels.Bytes(managedResize);
            Ffmpeg(); Managed(); state.ValidateManagedEncode(managedStage, targetWidth, targetHeight, 8, JpegSampling.YCbCr420);
            state.Report(nameof(JpegResizeTranscodeInvocation), new { Width, height, targetWidth, targetHeight, Progressive, Filter, sampling = nameof(JpegSampling.YCbCr420), managedQuality = 85, nativeQscale = 5 },
                managedArguments, nativeArguments, state.ValidateEncoded(state.ManagedOutput, managedStage, targetWidth, targetHeight, JpegSampling.YCbCr420),
                state.ValidateEncoded(state.NativeOutput, nativeStage, targetWidth, targetHeight, JpegSampling.YCbCr420),
                new
                {
                    nativeInputSha256 = JpegBenchmarkCase.Hash(nativeInput), nativeStageSha256 = JpegBenchmarkCase.Hash(nativeStage), managedStageSha256 = JpegBenchmarkCase.Hash(managedStage),
                    nativeResizeDifference = JpegBenchmarkCase.Metrics(resizeDifference), crossSubjectStageDifference = JpegBenchmarkCase.Metrics(JpegQualityCorpus.Compare(nativeStage, managedStage)),
                    qualityReference = "Each encoder's validated resized RGBA8; source error does not include its JPEG input's earlier loss."
                });
        }
        catch { state.Dispose(); throw; }
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
