using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class JpegCompatibilityFfmpegTests
{
    [Theory]
    [InlineData(false, false, 1)] [InlineData(false, true, 1)]
    [InlineData(true, false, 1)] [InlineData(true, true, 1)]
    [InlineData(true, false, 2)] [InlineData(true, true, 2)]
    public void FourComponentRgbMatchesLiveReference(bool ycck, bool progressive, int sampling)
    {
        const int width = 17, height = 9;
        var data = AuthoredJpegCompatibility.Create(width, height, ycck ? [80, 170, 90, 200] : [230, 180, 100, 210],
            sampling, sampling, true, ycck ? 2 : 0, progressive, 1);
        using var actual = ImageCodec.Decode(data);
        var expected = JpegOracle.Decode(data, width, height, JpegSampling.YCbCr444);
        Assert.Equal(expected, OriginalPixels.Bytes(actual));
        var caller = new byte[width * height * 3]; ImageCodec.DecodeJpeg(data, caller, JpegSampleFormat.Rgb);
        for (var i = 0; i < width * height; i++) Assert.Equal(expected.AsSpan(i * 4, 3).ToArray(), caller.AsSpan(i * 3, 3).ToArray());
    }

    [Theory]
    [InlineData(false, 1)] [InlineData(true, 1)] [InlineData(false, 2)] [InlineData(true, 2)]
    public void QuarterWidthLayoutsInteroperate(bool progressive, int vertical)
    {
        const int width = 67, height = 19;
        // Constant planes give an independent native color/bitstream check.
        // Existing varying controls separately cover expansion/filter precision.
        var data = AuthoredJpegCompatibility.Create(width, height, [90, 110, 150], 4, vertical,
            progressive: progressive, restart: 1);
        using var actual = ImageCodec.Decode(data);
        Assert.Equal(JpegOracle.Decode(data, width, height, JpegSampling.YCbCr444), OriginalPixels.Bytes(actual));
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void ExistingVaryingFourComponentRgbAndGrayMatchNativeExactly(bool ycck)
    {
        const int width = 65, height = 31;
        var data = AuthoredJpegCompatibility.Create(width, height, [17, 191, 250, 63], ycck ? 2 : 1, ycck ? 2 : 1,
            ycck, ycck ? 2 : 0, restart: ycck ? 1 : 0, vary: true);
        var expected = JpegOracle.Decode(data, width, height, JpegSampling.YCbCr444);
        using var owned = ImageCodec.Decode(data); Assert.Equal(expected, OriginalPixels.Bytes(owned));
        var rgb = new byte[width * height * 3]; ImageCodec.DecodeJpeg(data, rgb, JpegSampleFormat.Rgb);
        for (var i = 0; i < width * height; i++) Assert.Equal(expected.AsSpan(i * 4, 3).ToArray(), rgb.AsSpan(i * 3, 3).ToArray());
        var gray = new byte[width * height]; ImageCodec.DecodeJpeg(data, gray, JpegSampleFormat.Grayscale);
        var args = JpegOracle.DecodeArguments(width, height, JpegSampling.YCbCr444);
        args[Array.IndexOf(args, "rgba")] = "gray";
        Assert.Equal(OracleProcess.Run(OracleProcess.Ffmpeg, args, data), gray);
    }

    [Theory] [InlineData(1)] [InlineData(2)]
    public void ExistingVaryingQuarterWidthRgbAndGrayMatchNativeExactly(int vertical)
    {
        const int width = 67, height = 19;
        var data = AuthoredJpegCompatibility.Create(width, height, [60, 105, 145], 4, vertical, restart: 1, vary: true);
        var expected = JpegOracle.Decode(data, width, height, JpegSampling.YCbCr444);
        using var image = ImageCodec.Decode(data); Assert.Equal(expected, OriginalPixels.Bytes(image));
        var rgb = new byte[width * height * 3]; ImageCodec.DecodeJpeg(data, rgb, JpegSampleFormat.Rgb);
        for (var i = 0; i < width * height; i++) Assert.Equal(expected.AsSpan(i * 4, 3).ToArray(), rgb.AsSpan(i * 3, 3).ToArray());
        var gray = new byte[width * height]; ImageCodec.DecodeJpeg(data, gray, JpegSampleFormat.Grayscale);
        var args = JpegOracle.DecodeArguments(width, height, JpegSampling.YCbCr444); args[Array.IndexOf(args, "rgba")] = "gray";
        Assert.Equal(OracleProcess.Run(OracleProcess.Ffmpeg, args, data), gray);
    }

    [Fact]
    public void QuarterWidthNearIntegralFilterTapMatchesNativeExactly()
    {
        var data = AuthoredJpegCompatibility.Create(513, 259, [128, 255, 128], 4, 2, restart: 1,
            blockSample: (c, x, y) => c == 1 ? (x < 16 ? 255 : 139) : 128);
        using var image = ImageCodec.Decode(data);
        Assert.Equal(JpegOracle.Decode(data, 513, 259, JpegSampling.YCbCr444), OriginalPixels.Bytes(image));
    }

}
