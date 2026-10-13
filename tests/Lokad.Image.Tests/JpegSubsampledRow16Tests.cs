using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegSubsampledRow16Tests
{
    [Theory]
    [InlineData(1, 1)] [InlineData(1, 2)]
    [InlineData(2, 1)] [InlineData(2, 2)]
    public void AveragesUnroundedComponentsBeforeTheSingleFinalRounding(int rows, int component)
    {
        // Black has chroma 128. Blue=257 has Cb 128.5; red=257 has Cr 128.5.
        // Each pair averages to 128.25 -> 128. Rounding taps first gives 129.
        var top = new ushort[64];
        for (var x = 0; x < 16; x++)
        {
            top[x * 4 + 3] = 65535;
            if ((x & 1) != 0) top[x * 4 + (component == 1 ? 2 : 0)] = 257;
        }
        var output = new byte[8];
        JpegEncoder.SampleSubsampledComponentRow16(top, rows == 2 ? top : [], rows, component, null, output);
        Assert.All(output, value => Assert.Equal((byte)128, value));
    }

    [Theory]
    [InlineData(1, 1)] [InlineData(1, 2)]
    [InlineData(2, 1)] [InlineData(2, 2)]
    public void RetainsTapOrderAlphaProductsFullRowsAndGuardBytes(int rows, int component)
    {
        var top = new ushort[66]; var bottom = new ushort[66];
        foreach (var background in new uint[] { 0, 0xFFFFFF, 0x0000FF, 0xFF0000, 0x00FF00, 0x204060, 0x0101FE, 0xFF7F80 })
        for (var phase = 0; phase < 4096; phase++)
        {
            for (var y = 0; y < 2; y++)
            for (var x = 0; x < 16; x++)
            for (var c = 0; c < 4; c++)
                (y == 0 ? top : bottom)[1 + x * 4 + c] = unchecked((ushort)(c == 3 ? phase * 16 + x + y * 32767 : phase * (c * 7919 + 1) + x * (c * 16383 + 73) + y * 32767));
            var originalTop = top.ToArray(); var originalBottom = bottom.ToArray();
            var output = Enumerable.Repeat((byte)123, 10).ToArray();
            JpegEncoder.SampleSubsampledComponentRow16(top.AsSpan(1, 64), rows == 2 ? bottom.AsSpan(1, 64) : [],
                rows, component, background, output.AsSpan(1, 8));
            for (var x = 0; x < 8; x++)
            {
                double sum = 0;
                for (var y = 0; y < rows; y++)
                for (var dx = 0; dx < 2; dx++)
                {
                    var source = y == 0 ? top : bottom; var at = 1 + (x * 2 + dx) * 4; var a = source[at + 3];
                    const long maximum = 65535, denominator = maximum * maximum;
                    var r = (source[at] * (long)a * 255 + (int)(background >> 16 & 255) * (maximum - a) * maximum + denominator / 2) / denominator;
                    var g = (source[at + 1] * (long)a * 255 + (int)(background >> 8 & 255) * (maximum - a) * maximum + denominator / 2) / denominator;
                    var b = (source[at + 2] * (long)a * 255 + (int)(background & 255) * (maximum - a) * maximum + denominator / 2) / denominator;
                    sum += component == 1 ? 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b
                        : 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b;
                }
                Assert.Equal((byte)Math.Clamp(Math.Round(sum / (rows * 2), MidpointRounding.AwayFromZero), 0, 255), output[x + 1]);
            }
            Assert.Equal(123, output[0]); Assert.Equal(123, output[^1]);
            Assert.Equal(originalTop, top); Assert.Equal(originalBottom, bottom);
        }
    }

    [Theory]
    [InlineData(63, 0, 1, 1, 8)] [InlineData(65, 0, 1, 1, 8)]
    [InlineData(64, 63, 2, 1, 8)] [InlineData(64, 64, 1, 1, 8)]
    [InlineData(64, 0, 0, 1, 8)] [InlineData(64, 0, 3, 1, 8)]
    [InlineData(64, 0, 1, 0, 8)] [InlineData(64, 0, 1, 3, 8)]
    [InlineData(64, 0, 1, 1, 7)] [InlineData(64, 0, 1, 1, 9)]
    public void RejectsIncompleteRowsAndInvalidGeometryBeforeWriting(int top, int bottom, int rows, int component, int size)
    {
        var output = Enumerable.Repeat((byte)123, size).ToArray();
        Assert.Throws<ArgumentException>(() => JpegEncoder.SampleSubsampledComponentRow16(
            new ushort[top], new ushort[bottom], rows, component, 0, output));
        Assert.All(output, value => Assert.Equal((byte)123, value));
    }

    [Theory]
    [InlineData(1)] [InlineData(2)]
    public void RejectsTransparencyWithoutBackgroundInEverySourceLane(int component)
    {
        for (var lane = 0; lane < 32; lane++)
        {
            var top = Enumerable.Repeat((ushort)65535, 64).ToArray(); var bottom = top.ToArray();
            (lane < 16 ? top : bottom)[(lane % 16) * 4 + 3] = 65534;
            var error = Assert.Throws<ArgumentException>(() =>
                JpegEncoder.SampleSubsampledComponentRow16(top, bottom, 2, component, null, new byte[8]));
            Assert.Equal("image", error.ParamName);
            Assert.StartsWith("JPEG requires opaque pixels or an explicit RGB background.", error.Message);
        }
    }
}
