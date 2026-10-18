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
        Compare(OriginalPixels.Bytes(actual), expected);
    }

    [Theory]
    [InlineData(false, 1)] [InlineData(true, 1)] [InlineData(false, 2)] [InlineData(true, 2)]
    public void QuarterWidthLayoutsInteroperate(bool progressive, int vertical)
    {
        const int width = 67, height = 19;
        // Constant planes give an independent native color/bitstream check.
        // Authored decimal controls separately cover varying centered chroma.
        var data = AuthoredJpegCompatibility.Create(width, height, [90, 110, 150], 4, vertical,
            progressive: progressive, restart: 1);
        using var actual = ImageCodec.Decode(data);
        Compare(OriginalPixels.Bytes(actual), JpegOracle.Decode(data, width, height, JpegSampling.YCbCr444));
    }

    private static void Compare(byte[] actual, byte[] expected)
    {
        Assert.Equal(expected.Length, actual.Length);
        int maximum = 0; double squared = 0;
        for (var i = 0; i < actual.Length; i++)
        {
            if (i % 4 == 3) { Assert.Equal(255, actual[i]); Assert.Equal(255, expected[i]); continue; }
            var error = Math.Abs(actual[i] - expected[i]); maximum = Math.Max(maximum, error); squared += error * error;
        }
        var rmse = Math.Sqrt(squared / (actual.Length / 4 * 3));
        Assert.True(maximum <= 4 && rmse <= 1.5, $"JPEG compatibility: maximum {maximum}, RMSE {rmse:F4}");
    }
}
