using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class JpegSampleFfmpegTests
{
    [Theory]
    [InlineData(JpegSampling.Grayscale, false)]
    [InlineData(JpegSampling.YCbCr444, false)]
    [InlineData(JpegSampling.YCbCr422, false)]
    [InlineData(JpegSampling.YCbCr420, false)]
    [InlineData(JpegSampling.Grayscale, true)]
    [InlineData(JpegSampling.YCbCr420, true)]
    public void CallerSamplesMatchLiveIndependentDecode(JpegSampling sampling, bool progressive)
    {
        const int width = 31, height = 23;
        var encoded = progressive ? AuthoredJpeg.ProgressiveFrame(width, height, sampling, 1)
            : sampling == JpegSampling.Grayscale ? AuthoredJpeg.GrayscaleDcGrid(width, height, false)
            : OracleProcess.Run(OracleProcess.Ffmpeg, JpegOracle.EncodeArguments(width, height, sampling, 3),
                OriginalPixels.Generate(width, height, 8, opaque: true));
        var rgba = JpegOracle.Decode(encoded, width, height, sampling);
        var rgb = new byte[width * height * 3];
        ImageCodec.DecodeJpeg(encoded, rgb, JpegSampleFormat.Rgb);
        var expectedRgb = new byte[rgb.Length];
        for (var i = 0; i < width * height; i++) rgba.AsSpan(i * 4, 3).CopyTo(expectedRgb.AsSpan(i * 3, 3));
        Assert.Equal(expectedRgb, rgb);

        // Extract decoded Y without RGB conversion or a range remap.
        var gray = new byte[width * height];
        ImageCodec.DecodeJpeg(encoded, gray, JpegSampleFormat.Grayscale);
        var expectedGray = OracleProcess.Run(OracleProcess.Ffmpeg,
            ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0",
             "-filter_threads", "1", "-vf", "extractplanes=y", "-frames:v", "1", "-threads:v", "1",
             "-pix_fmt", "gray", "-f", "rawvideo", "pipe:1"], encoded);
        Assert.Equal(expectedGray, gray);
    }

}
