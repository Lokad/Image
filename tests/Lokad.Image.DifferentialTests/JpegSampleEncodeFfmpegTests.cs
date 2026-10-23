using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class JpegSampleEncodeFfmpegTests
{
    [Theory]
    [InlineData(JpegSampling.Grayscale)] [InlineData(JpegSampling.YCbCr444)]
    [InlineData(JpegSampling.YCbCr422)] [InlineData(JpegSampling.YCbCr420)]
    public void CallerSamplePacketsInteroperateWithLiveFfmpeg(JpegSampling sampling)
    {
        const int width = 129, height = 129;
        var format = sampling == JpegSampling.Grayscale ? JpegSampleFormat.Grayscale : JpegSampleFormat.Rgb;
        var rgba = OriginalPixels.Generate(width, height, 8, grayscale: format == JpegSampleFormat.Grayscale, opaque: true);
        var stride = width * (int)format + 3;
        var samples = Enumerable.Repeat((byte)0xA7, height * stride).ToArray();
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            rgba.AsSpan((y * width + x) * 4, (int)format).CopyTo(samples.AsSpan(y * stride + x * (int)format, (int)format));
        var original = samples.ToArray();
        using var output = new MemoryStream();
        ImageCodec.EncodeJpeg(samples, width, height, format, output, stride,
            new() { Sampling = sampling, Quality = 86, RestartInterval = 3 });
        var encoded = output.ToArray();
        Assert.True(encoded.Length > 0); Assert.Equal(original, samples); Assert.True(output.CanWrite);
        using var owned = ImageCodec.Decode(encoded);
        var actual = OriginalPixels.Bytes(owned); var expected = JpegOracle.Decode(encoded, width, height, sampling);
        Assert.Equal(expected, actual);
    }
}
