using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Lokad.Image;
using Lokad.Image.Tooling;

/// <summary>File-to-raw decode with EXIF orientation; native transform selection is prepared.</summary>
[MemoryDiagnoser]
public class ExifDecodeRawFile
{
    [Params(129, 513)] public int Width { get; set; }
    [Params(1, 32)] public int Frames { get; set; }
    [ParamsAllValues] public MetadataProfile Profile { get; set; }
    [ParamsAllValues] public ImageOrientation Orientation { get; set; }
    private JpegBenchmarkCase? state;
    private string[] managedArguments = [], nativeArguments = [];

    [GlobalSetup]
    public void Setup()
    {
        if (Frames is not (1 or 32)) throw new ArgumentOutOfRangeException(nameof(Frames));
        state = new JpegBenchmarkCase();
        try
        {
            var height = Width / 2 + 3;
            var framesDirectory = Path.Combine(state.DirectoryPath, "frames"); Directory.CreateDirectory(framesDirectory);
            var references = new List<byte[]>(); var publicFrames = new List<byte[]>(); var receipts = new List<object>(); long encodedBytes = 0;
            MetadataFixture? fixture = null;
            for (var frame = 0; frame < Frames; frame++)
            {
                fixture = OriginalExif.Create(Profile, Width, height, frame);
                var bigEndian = frame % 2 != 0;
                var encoded = OriginalExif.Attach(fixture, Orientation, bigEndian);
                var name = "frame-" + frame.ToString("D4", System.Globalization.CultureInfo.InvariantCulture) + fixture.Extension;
                File.WriteAllBytes(Path.Combine(framesDirectory, name), encoded); encodedBytes += encoded.Length;
                var expectedInfo = new ImageInfo(fixture.Format, Width, height, fixture.Depth, fixture.Alpha, Orientation);
                if (ImageCodec.Identify(encoded) != expectedInfo) throw new InvalidOperationException("EXIF input inspection differs from the original profile.");
                var nativeRaw = ExifOracle.Decode(encoded, fixture, ImageOrientation.Normal, state.Ffmpeg);
                if (!fixture.Lossy && !nativeRaw.AsSpan().SequenceEqual(fixture.Source))
                    throw new InvalidOperationException("Native lossless raw decode differs from original source pixels.");
                var expected = OriginalExif.Permute(nativeRaw, Width, height, fixture.Depth, Orientation); references.Add(expected);
                using var control = ImageCodec.Decode(encoded);
                var publicRaw = OriginalPixels.Bytes(control);
                using var decoded = ImageCodec.Decode(encoded, new DecodeOptions { ApplyOrientation = true });
                var publicFrame = OriginalPixels.Bytes(decoded);
                if ((decoded.Width, decoded.Height, decoded.BitDepth, decoded.Orientation)
                    != (expectedInfo.OrientedWidth, expectedInfo.OrientedHeight, fixture.Depth, ImageOrientation.Normal)
                    || !publicFrame.AsSpan().SequenceEqual(OriginalExif.Permute(publicRaw, Width, height, fixture.Depth, Orientation)))
                    throw new InvalidOperationException("Public oriented decode differs from independent pixel placement.");
                ExifOracle.Check(expected, publicFrame, fixture.Lossy); publicFrames.Add(publicFrame);
                receipts.Add(new { name, frame, exifByteOrder = bigEndian ? "MM" : "II", Orientation,
                    recipe = Profile == MetadataProfile.JpegProgressive420 ? "repeated-original-progressive-coefficient-scans-with-restarts" : "asymmetric-v1-phase with first-pixel alpha zero; owned encoding",
                    sourceSha256 = fixture.Source is null ? null : JpegBenchmarkCase.Hash(fixture.Source), encodedBytes = encoded.Length,
                    encodedSha256 = JpegBenchmarkCase.Hash(encoded), rawNativeSha256 = JpegBenchmarkCase.Hash(nativeRaw),
                    expectedOrientedSha256 = JpegBenchmarkCase.Hash(expected), publicOrientedSha256 = JpegBenchmarkCase.Hash(publicFrame) });
            }
            File.WriteAllText(state.Input, JsonSerializer.Serialize(receipts));
            managedArguments = [state.Probe, "decode-oriented-sequence", framesDirectory, state.ManagedOutput, fixture!.Extension, JpegBenchmarkCase.Number(Frames)];
            nativeArguments = ["-hide_banner", "-loglevel", "error", "-threads:v", "1", "-filter_complex_threads", "1", "-noautorotate",
                "-f", "image2", "-start_number", "0", "-i", Path.Combine(framesDirectory, "frame-%04d" + fixture.Extension),
                "-filter_complex", ExifOracle.Filter(fixture, Orientation), "-frames:v", JpegBenchmarkCase.Number(Frames), "-fps_mode", "passthrough",
                "-threads:v", "1", "-c:v", "rawvideo", "-pix_fmt", fixture.Depth == 16 ? "rgba64le" : "rgba", "-f", "rawvideo", "-y", state.NativeOutput];
            Ffmpeg(); Managed();
            var managed = Validate(state.ManagedOutput, references, fixture.Lossy, publicFrames);
            var native = Validate(state.NativeOutput, references, lossy: false);
            state.Report(nameof(ExifDecodeRawFile), new { Width, height, fixture.Depth, Profile, Orientation, Frames,
                orientedWidth = (int)Orientation >= 5 ? height : Width, orientedHeight = (int)Orientation >= 5 ? Width : height,
                codecThreads = 1, filterThreads = 1 }, managedArguments, nativeArguments, managed, native,
                method: "Untimed validation. Each subject launches one process, reads the same one/32 numbered independent still files and writes every oriented raw frame. The managed child reads EXIF and applies orientation during Decode. FFmpeg autorotation is disabled; the equivalent explicit RGB transform is selected outside timing and follows the recorded color/chroma conversion. These rows compare complete decode/orient file workloads, not EXIF parser speed. Normal orientation remains a decode-only control. Frame order/count/depth/alpha and geometry are checked exactly; managed lossy RGB retains maximum 4/RMSE 1.5 versus the native decoder. The native transform is exact versus independent placement of its raw pixels. Process/JIT startup, file IO, color conversion and full raw transport remain included. Linux child memory is an additional untimed execution, unavailable elsewhere.",
                inputs: new { numberedFiles = Frames, totalEncodedBytes = encodedBytes, frames = receipts }, totalInputBytes: encodedBytes, inputIsIndex: true);
        }
        catch { state.Dispose(); throw; }
    }

    private static object Validate(string path, List<byte[]> references, bool lossy, List<byte[]>? publicFrames = null)
    {
        using var input = File.OpenRead(path); var size = references.Sum(frame => (long)frame.Length);
        if (input.Length != size) throw new InvalidOperationException("Oriented output frame count or depth differs.");
        var frame = new byte[references[0].Length]; var receipts = new List<object>();
        foreach (var reference in references)
        {
            input.ReadExactly(frame); var difference = ExifOracle.Check(reference, frame, lossy);
            if (publicFrames is not null && !frame.AsSpan().SequenceEqual(publicFrames[receipts.Count]))
                throw new InvalidOperationException("ImageProbe oriented output differs from its exact public workflow.");
            receipts.Add(new { frame = receipts.Count, difference.Maximum, difference.RgbRmse, alphaExact = true,
                actualSha256 = JpegBenchmarkCase.Hash(frame), referenceSha256 = JpegBenchmarkCase.Hash(reference) });
        }
        return new { outputBytes = size, outputSha256 = JpegBenchmarkCase.Hash(File.ReadAllBytes(path)), frameValidation = receipts };
    }

    [Benchmark(Baseline = true)] public long Ffmpeg() => state!.Run(state.Ffmpeg, nativeArguments, state.NativeOutput);
    [Benchmark] public long Managed() => state!.Run("dotnet", managedArguments, state.ManagedOutput);
    [GlobalCleanup] public void Cleanup() => state?.Dispose();
}
