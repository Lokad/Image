using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>One still file to a captured header result; FFmpeg's ffprobe is the baseline.</summary>
[MemoryDiagnoser]
public class IdentifyInvocation
{
    [Params(129, 513)] public int Width { get; set; }
    [ParamsAllValues] public MetadataProfile Profile { get; set; }
    private JpegBenchmarkCase? state;
    private string[] managedArguments = [], nativeArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        state = new JpegBenchmarkCase();
        try
        {
            var height = Width / 2 + 3; var fixture = OriginalExif.Create(Profile, Width, height);
            File.WriteAllBytes(state.Input, fixture.Encoded);
            var expected = new ImageInfo(fixture.Format, Width, height, fixture.Depth, fixture.Alpha, ImageOrientation.Normal);
            managedArguments = [state.Probe, "identify", state.Input];
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-skip_frame", "all",
                "-show_entries", "stream=codec_name,width,height,pix_fmt,bits_per_raw_sample", "-of", "json", state.Input];
            var managedBytes = OracleProcess.Run("dotnet", managedArguments);
            var nativeBytes = OracleProcess.Run(state.Ffprobe, nativeArguments);
            var managedInfo = JsonSerializer.Deserialize<ImageInfo>(managedBytes)
                ?? throw new InvalidOperationException("ImageProbe returned no inspection result.");
            using var nativeDocument = JsonDocument.Parse(nativeBytes);
            var streams = nativeDocument.RootElement.GetProperty("streams");
            if (streams.GetArrayLength() != 1) throw new InvalidOperationException("ffprobe returned an unexpected stream count.");
            var stream = streams[0]; var codec = stream.GetProperty("codec_name").GetString(); var pixelFormat = stream.GetProperty("pix_fmt").GetString();
            var format = codec switch { "png" => ImageFormat.Png, "mjpeg" => ImageFormat.Jpeg, "webp" => ImageFormat.WebP,
                _ => throw new InvalidOperationException("ffprobe returned an unsupported inspection codec.") };
            var (depth, alpha) = pixelFormat switch
            {
                "rgba" or "argb" => (8, true), "rgba64be" or "rgba64le" => (16, true),
                "yuvj420p" => (8, false), "yuva420p" => (8, true),
                _ => throw new InvalidOperationException($"Unexpected inspection pixel format: {pixelFormat}.")
            };
            var nativeInfo = new ImageInfo(format, stream.GetProperty("width").GetInt32(), stream.GetProperty("height").GetInt32(),
                depth, alpha, ImageOrientation.Normal);
            if (managedInfo != expected || nativeInfo != expected) throw new InvalidOperationException("Inspection subjects disagree with the original coded profile.");
            if (stream.TryGetProperty("bits_per_raw_sample", out var rawDepth)
                && int.Parse(rawDepth.GetString()!, System.Globalization.CultureInfo.InvariantCulture) != depth)
                throw new InvalidOperationException("ffprobe sample depth disagrees with its pixel format.");
            File.WriteAllBytes(state.ManagedOutput, managedBytes); File.WriteAllBytes(state.NativeOutput, nativeBytes);
            var nativeMemory = ChildProcessMemory.Measure(state.Ffprobe, nativeArguments, state.DirectoryPath, nativeBytes);
            var managedMemory = ChildProcessMemory.Measure("dotnet", managedArguments, state.DirectoryPath, managedBytes);
            state.Report(nameof(IdentifyInvocation), new { Width, height, Profile, fixture.Depth, fixture.Alpha,
                frames = 1, metadata = "no EXIF; coded header fields only", codecThreads = 1 }, managedArguments, nativeArguments,
                new { stdoutBytes = managedBytes.Length, stdoutSha256 = JpegBenchmarkCase.Hash(managedBytes), codedInfo = managedInfo },
                new { stdoutBytes = nativeBytes.Length, stdoutSha256 = JpegBenchmarkCase.Hash(nativeBytes), codec, pixelFormat,
                    codedInfo = nativeInfo, depthSource = "pixel-format precision, cross-checked with bits_per_raw_sample when present" },
                method: "Untimed validation of one file to a captured JSON header result. Timed methods launch one process, read the same original encoded still file and capture complete stdout into a bounded byte buffer. Schemas and byte counts differ; their format/coded dimensions/sample precision/alpha projections are checked against the known original profile outside timing. Output files are evidence copies written only during setup. No comparable EXIF discovery or oriented-geometry claim: inputs contain no EXIF and the projection is coded geometry. ffprobe requests skip_frame=all but demuxer/decoder initialization and codec-specific parsing or pixel work can remain; these are complete inspection invocations, not isolated header-parser throughput. There is no batch row or claim that one ffprobe stream result identifies multiple files. Linux kernel child-memory samples are separate untimed executions whose stdout must remain identical.",
                childMemory: new { native = nativeMemory, managed = managedMemory }, nativeExecutable: state.Ffprobe);
        }
        catch { state.Dispose(); throw; }
    }

    [Benchmark(Baseline = true)] public long Ffprobe() => OracleProcess.Run(state!.Ffprobe, nativeArguments).Length;
    [Benchmark] public long Managed() => OracleProcess.Run("dotnet", managedArguments).Length;
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
