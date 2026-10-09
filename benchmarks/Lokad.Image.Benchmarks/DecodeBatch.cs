using System.Globalization;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

public enum BatchFormat { Png8, Png16, JpegBaseline420, JpegProgressive420, WebpLosslessAlpha, WebpLossyAlpha }

/// <summary>One process decodes 32 independent numbered still files to one raw file.</summary>
[MemoryDiagnoser]
public class DecodeBatch
{
    [Params(129, 513)] public int Width { get; set; }
    [Params(BatchFormat.Png8, BatchFormat.Png16, BatchFormat.JpegBaseline420, BatchFormat.JpegProgressive420,
        BatchFormat.WebpLosslessAlpha, BatchFormat.WebpLossyAlpha)] public BatchFormat Format { get; set; }
    public const int FrameCount = 32;
    private JpegBenchmarkCase? state;
    private string[] managedArguments = [], nativeArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        state = new JpegBenchmarkCase();
        try
        {
            var height = Width / 2 + 3; var depth = Format == BatchFormat.Png16 ? 16 : 8;
            var jpeg = Format is BatchFormat.JpegBaseline420 or BatchFormat.JpegProgressive420;
            var lossy = jpeg || Format == BatchFormat.WebpLossyAlpha;
            var extension = jpeg ? ".jpg" : Format is BatchFormat.Png8 or BatchFormat.Png16 ? ".png" : ".webp";
            var framesDirectory = Path.Combine(state.DirectoryPath, "frames"); Directory.CreateDirectory(framesDirectory);
            var receipts = new List<object>(); var references = new List<byte[]>(); var publicFrames = new List<byte[]>(); long encodedBytes = 0;
            for (var index = 0; index < FrameCount; index++)
            {
                var source = OriginalPixels.Generate(Width, height, depth, opaque: jpeg, phase: index);
                var encoded = Encode(source, Width, height, depth);
                var name = "frame-" + index.ToString("D4", CultureInfo.InvariantCulture) + extension;
                File.WriteAllBytes(Path.Combine(framesDirectory, name), encoded); encodedBytes += encoded.Length;
                var expected = jpeg ? JpegOracle.Decode(encoded, Width, height, JpegSampling.YCbCr420, state.Ffmpeg)
                    : Format == BatchFormat.WebpLossyAlpha ? WebpOracle.Decode(encoded, Width, height, alpha: true, state.Ffmpeg)
                    : source;
                references.Add(expected);
                using var decoded = ImageCodec.Decode(encoded);
                if ((decoded.Width, decoded.Height, decoded.BitDepth) != (Width, height, depth))
                    throw new InvalidOperationException("Public decode geometry or precision differs from its numbered input.");
                var publicFrame = OriginalPixels.Bytes(decoded); publicFrames.Add(publicFrame);
                receipts.Add(new { name, frame = index,
                    recipe = Format == BatchFormat.JpegProgressive420 ? "repeated-original-progressive-coefficient-scans-with-restarts" : "asymmetric-v1-phase: add phase*977+phase*channel*313 modulo 65536 before the declared depth reduction",
                    sourceSha256 = Format == BatchFormat.JpegProgressive420 ? null : JpegBenchmarkCase.Hash(source),
                    encodedSha256 = JpegBenchmarkCase.Hash(encoded), encodedBytes = encoded.Length, expectedSha256 = JpegBenchmarkCase.Hash(expected),
                    publicDecodedSha256 = JpegBenchmarkCase.Hash(publicFrame), decodedWidth = decoded.Width, decodedHeight = decoded.Height, decodedDepth = decoded.BitDepth });
            }
            // The common report's input hash identifies this exact numbered-file index.
            File.WriteAllText(state.Input, JsonSerializer.Serialize(receipts));
            managedArguments = [state.Probe, "decode-sequence", framesDirectory, state.ManagedOutput, extension, FrameCount.ToString(CultureInfo.InvariantCulture)];
            var filter = jpeg ? JpegOracle.Filter(Width, height, JpegSampling.YCbCr420)
                : Format == BatchFormat.WebpLossyAlpha ? WebpOracle.Filter(Width, height, alpha: true) : "null";
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-filter_complex_threads", "1", "-noautorotate",
                "-f", "image2", "-start_number", "0", "-i", Path.Combine(framesDirectory, "frame-%04d" + extension),
                "-filter_complex", filter, "-frames:v", FrameCount.ToString(CultureInfo.InvariantCulture), "-fps_mode", "passthrough",
                "-threads:v", "1", "-c:v", "rawvideo", "-pix_fmt", depth == 16 ? "rgba64le" : "rgba", "-f", "rawvideo", "-y", state.NativeOutput];
            Ffmpeg(); Managed();
            var managed = Validate(state.ManagedOutput, references, lossy, publicFrames);
            var native = Validate(state.NativeOutput, references, lossy);
            var nativeMemory = ChildProcessMemory.Measure(state.Ffmpeg, nativeArguments, state.DirectoryPath);
            var managedMemory = ChildProcessMemory.Measure("dotnet", managedArguments, state.DirectoryPath);
            // Counter wrapping must preserve the already validated complete output.
            Validate(state.NativeOutput, references, lossy); Validate(state.ManagedOutput, references, lossy, publicFrames);
            state.Report(nameof(DecodeBatch), new { Width, height, depth, Format, frames = FrameCount, opaque = jpeg,
                family = "amortized-startup still-image decode", codecThreads = 1, filterThreads = 1 }, managedArguments, nativeArguments, managed, native,
                method: "Untimed validation of a fixed 32-frame batch. Each subject launches one process, reads the same numbered still-image files exactly once, and streams all normalized decoded frames into one raw output file. Frame order/count/precision/alpha and per-frame decoder bounds are checked. No looped video, temporal reuse, input preloading, output suppression or managed PNG encode. Codec context, remaining process/JIT startup, filesystem and raw transport costs remain part of the measured batch; timings are not isolated in-process throughput. Divide batch latency by 32 for per-image figures. Child-memory counters come from separate untimed executions, outside benchmark methods.",
                inputs: new { numberedFiles = FrameCount, totalEncodedBytes = encodedBytes, indexSha256 = JpegBenchmarkCase.Hash(File.ReadAllBytes(state.Input)), frames = receipts },
                childMemory: new { native = nativeMemory, managed = managedMemory }, totalInputBytes: encodedBytes, inputIsIndex: true);
        }
        catch { state.Dispose(); throw; }
    }

    private byte[] Encode(byte[] source, int width, int height, int depth)
    {
        if (Format == BatchFormat.JpegProgressive420) return AuthoredJpeg.ProgressiveFrame(width, height, JpegSampling.YCbCr420, 4);
        if (Format == BatchFormat.JpegBaseline420) return OracleProcess.Run(state!.Ffmpeg, JpegOracle.EncodeArguments(width, height, JpegSampling.YCbCr420, 5), source);
        if (Format == BatchFormat.WebpLossyAlpha) return WebpOracle.Encode(source, width, height, 4, 75, state!.Ffmpeg);
        using var image = JpegBenchmarkCase.Buffer(source, width, height, depth); using var output = new MemoryStream();
        if (Format == BatchFormat.WebpLosslessAlpha) ImageCodec.EncodeWebp(image, output);
        else ImageCodec.EncodePng(image, output);
        return output.ToArray();
    }

    private static object Validate(string path, List<byte[]> references, bool lossy, List<byte[]>? publicFrames = null)
    {
        using var input = File.OpenRead(path);
        var size = references.Sum(frame => (long)frame.Length);
        if (input.Length != size) throw new InvalidOperationException("Decode batch frame count or sample precision differs.");
        var frame = new byte[references[0].Length]; var results = new List<object>();
        foreach (var expected in references)
        {
            input.ReadExactly(frame);
            if (publicFrames is not null && !frame.AsSpan().SequenceEqual(publicFrames[results.Count]))
                throw new InvalidOperationException("Numbered ImageProbe output differs from the exact public decode workflow.");
            if (!lossy && !frame.AsSpan().SequenceEqual(expected)) throw new InvalidOperationException("Lossless batch frame differs from original exact samples.");
            if (lossy)
            {
                // JPEG quality comparison assumes opaque alpha; VP8 requires exact source alpha separately.
                double squared = 0; var maximum = 0;
                for (var i = 0; i < frame.Length; i++)
                {
                    if (i % 4 == 3) { if (frame[i] != expected[i]) throw new InvalidOperationException("Decode batch alpha differs."); continue; }
                    var error = Math.Abs(frame[i] - expected[i]); maximum = Math.Max(maximum, error); squared += error * error;
                }
                var rmse = Math.Sqrt(squared / (frame.Length / 4 * 3));
                if (maximum > 4 || rmse > 1.5) throw new InvalidOperationException($"Decode batch RGB differs: maximum {maximum}, RMSE {rmse}.");
                results.Add(new { frame = results.Count, maximum, rgbRmse = rmse, alphaExact = true, actualSha256 = JpegBenchmarkCase.Hash(frame), referenceSha256 = JpegBenchmarkCase.Hash(expected) });
            }
            else results.Add(new { frame = results.Count, exact = true, actualSha256 = JpegBenchmarkCase.Hash(frame) });
        }
        if (input.ReadByte() != -1) throw new InvalidOperationException("Decode batch contains extra samples.");
        return new { outputBytes = size, outputSha256 = JpegBenchmarkCase.Hash(File.ReadAllBytes(path)), frameValidation = results };
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
