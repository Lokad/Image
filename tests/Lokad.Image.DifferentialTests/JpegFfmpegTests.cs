using Lokad.Image.Tooling;
using Xunit.Abstractions;

namespace Lokad.Image.DifferentialTests;

public sealed class JpegFfmpegTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ExplicitRgbDeclarationsInteroperate(bool adobe)
    {
        var encoded = AuthoredJpeg.DirectRgb(adobe);
        using var actual = ImageCodec.Decode(encoded);
        Compare(OriginalPixels.Bytes(actual), JpegOracle.Decode(encoded, 8, 8, JpegSampling.YCbCr444), 64, "explicit RGB");
    }

    [Theory]
    [InlineData("yuvj444p", 31, 23)] [InlineData("yuvj422p", 31, 23)] [InlineData("yuvj420p", 31, 23)]
    [InlineData("yuvj444p", 1, 1)] [InlineData("yuvj422p", 1, 17)] [InlineData("yuvj420p", 17, 1)]
    public void BaselineDecodeMatchesConfiguredFfmpeg(string pixelFormat, int width, int height)
    {
        var encoded = OracleProcess.Run(OracleProcess.Ffmpeg,
            ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-f", "rawvideo", "-pixel_format", "rgba",
             "-video_size", $"{width}x{height}", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-c:v", "mjpeg",
             "-pix_fmt", pixelFormat, "-q:v", "3", "-f", "image2pipe", "pipe:1"], OriginalPixels.Generate(width, height, 8, opaque: true));
        using var actual = ImageCodec.Decode(encoded);
        var sampling = pixelFormat switch { "yuvj444p" => JpegSampling.YCbCr444, "yuvj422p" => JpegSampling.YCbCr422, _ => JpegSampling.YCbCr420 };
        Compare(OriginalPixels.Bytes(actual), JpegOracle.Decode(encoded, width, height, sampling), width * height, pixelFormat);
    }

    public static IEnumerable<object[]> EncodingProfiles()
    {
        foreach (var sampling in Enum.GetValues<JpegSampling>())
        foreach (var quality in new[] { 1, 50, 90, 100 })
        foreach (var depth in new[] { 8, 16 }) yield return [sampling, quality, depth];
    }

    [Theory]
    [MemberData(nameof(EncodingProfiles))]
    public void ManagedEncoderInteroperatesIncludingRestartMarkers(JpegSampling sampling, int quality, int depth)
    {
        using var image = OriginalPixels.Buffer(31, 23, depth, grayscale: sampling == JpegSampling.Grayscale, opaque: true);
        using var encoded = new MemoryStream();
        ImageCodec.EncodeJpeg(image, encoded, new JpegEncodeOptions { Sampling = sampling, Quality = quality, RestartInterval = 2 });
        using var actual = ImageCodec.Decode(encoded.ToArray());
        Compare(OriginalPixels.Bytes(actual), JpegOracle.Decode(encoded.ToArray(), image.Width, image.Height, sampling), image.Width * image.Height, sampling.ToString());
        Assert.True(encoded.CanWrite);
    }

    [Theory]
    [InlineData(JpegSampling.YCbCr444, 2)]
    [InlineData(JpegSampling.YCbCr422, 3)]
    [InlineData(JpegSampling.YCbCr420, 3)]
    [InlineData(JpegSampling.Grayscale, 3)]
    public void ReferenceRestartsInteroperateAtNonRowBoundaries(JpegSampling sampling, int interval)
    {
        using var image = OriginalPixels.Buffer(18, 19, 8, opaque: true);
        using var encoded = new MemoryStream();
        ImageCodec.EncodeJpeg(image, encoded, new JpegEncodeOptions
        {
            Sampling = sampling, RestartInterval = interval
        });
        using var actual = ImageCodec.Decode(encoded.ToArray());
        // Historical decoder interoperability tolerance, not an exact decoder
        // qualification or a reference-packet claim for these new MCU intervals.
        Compare(OriginalPixels.Bytes(actual), JpegOracle.Decode(encoded.ToArray(), 18, 19, sampling), 18 * 19, "Reference restart " + sampling);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void AuthoredProgressiveRefinementInteroperates(bool negativeDc)
    {
        var encoded = AuthoredJpeg.RefinedBlock(negativeDc);
        using var actual = ImageCodec.Decode(encoded);
        Compare(OriginalPixels.Bytes(actual), JpegOracle.Decode(encoded, 8, 8, JpegSampling.Grayscale), 64, "progressive refinement");
    }

    [Theory]
    [InlineData(0)] [InlineData(1)]
    public void AuthoredProgressiveEobRunsInteroperate(int restartInterval)
    {
        var encoded = AuthoredJpeg.EobRun(restartInterval);
        using var actual = ImageCodec.Decode(encoded);
        Compare(OriginalPixels.Bytes(actual), JpegOracle.Decode(encoded, 24, 8, JpegSampling.Grayscale), 192, "progressive EOB/restart");
    }

    [Theory]
    [InlineData(JpegSampling.YCbCr444, 31, 23, 0)] [InlineData(JpegSampling.YCbCr422, 31, 23, 2)]
    [InlineData(JpegSampling.YCbCr420, 31, 23, 1)] [InlineData(JpegSampling.YCbCr420, 17, 1, 1)]
    [InlineData(JpegSampling.YCbCr422, 1, 17, 1)]
    public void AuthoredProgressiveColorScansInteroperate(JpegSampling sampling, int width, int height, int restartInterval)
    {
        var encoded = AuthoredJpeg.ProgressiveColor(width, height, sampling, restartInterval);
        using var actual = ImageCodec.Decode(encoded);
        Compare(OriginalPixels.Bytes(actual), JpegOracle.Decode(encoded, width, height, sampling), width * height, "progressive " + sampling);
    }

    [Theory]
    [InlineData(1, 1, 0)] [InlineData(1, 17, 1)] [InlineData(31, 23, 2)]
    [InlineData(129, 67, 4)] [InlineData(513, 259, 17)]
    public void AuthoredProgressiveGrayScansInteroperateAtBenchmarkAndEdgeDimensions(int width, int height, int restartInterval)
    {
        var encoded = AuthoredJpeg.ProgressiveFrame(width, height, JpegSampling.Grayscale, restartInterval);
        using var actual = ImageCodec.Decode(encoded);
        Compare(OriginalPixels.Bytes(actual), JpegOracle.Decode(encoded, width, height, JpegSampling.Grayscale), width * height, "progressive gray");
    }

    [Fact]
    public void SplitSpectralBandsAndEmptyEobRunsInteroperate()
    {
        foreach (var (encoded, width) in new[] { (AuthoredJpeg.SparseBands(), 8), (AuthoredJpeg.EmptyEobRun(), 24) })
        {
            using var actual = ImageCodec.Decode(encoded);
            Compare(OriginalPixels.Bytes(actual), JpegOracle.Decode(encoded, width, 8, JpegSampling.Grayscale), width * 8, "spectral bands/EOB");
        }
    }

    private void Compare(byte[] actual, byte[] expected, int pixels, string profile)
    {
        Assert.Equal(pixels * 4, actual.Length); Assert.Equal(actual.Length, expected.Length);
        int max = 0; double squared = 0;
        for (var i = 0; i < actual.Length; i++)
        {
            if (i % 4 == 3) { Assert.Equal(255, actual[i]); continue; }
            var error = Math.Abs(actual[i] - expected[i]); max = Math.Max(max, error); squared += error * error;
        }
        var rmse = Math.Sqrt(squared / (pixels * 3));
        output.WriteLine($"{profile}: maximum channel error {max}, RMSE {rmse:F4}");
        Assert.True(max <= 4 && rmse <= 1.5, $"{profile}: maximum {max}, RMSE {rmse:F4}");
    }
}
