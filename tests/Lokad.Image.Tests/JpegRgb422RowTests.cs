using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegRgb422RowTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(7)]
    [InlineData(8)] [InlineData(9)] [InlineData(15)] [InlineData(16)]
    [InlineData(17)] [InlineData(31)] [InlineData(513)] [InlineData(4097)]
    public void CenteredHorizontalPlanesMatchOriginalBinary64RoundingWithGuards(int width)
    {
        var count = (width + 1) / 2;
        var red = new byte[width + 2]; var green = new byte[count + 2]; var blue = new byte[count + 2];
        for (var x = 0; x < width; x++) red[x + 1] = unchecked((byte)(x * 73 + 19));
        for (var k = 0; k < count; k++)
        {
            green[k + 1] = unchecked((byte)(k * 101 ^ 139));
            blue[k + 1] = k % 4 == 0 ? (byte)0 : k % 4 == 1 ? (byte)255 : unchecked((byte)(k * 73 + 2));
        }
        var originalRed = red.ToArray(); var originalGreen = green.ToArray(); var originalBlue = blue.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
        JpegDecoder.RenderRgb422Row(red.AsSpan(1, width), green.AsSpan(1, count), blue.AsSpan(1, count), output.AsSpan(5, width * 4));
        for (var x = 0; x < width; x++)
        {
            // The preexisting decoder's binary64 coordinate, clamping,
            // interpolation and away-from-zero rounding, independently evaluated.
            var coordinate = (x + 0.5) / 2 - 0.5;
            var left = (int)Math.Floor(coordinate); var fraction = coordinate - left;
            var a = Math.Clamp(left, 0, count - 1) + 1; var b = Math.Clamp(left + 1, 0, count - 1) + 1;
            var g = (byte)Math.Round(green[a] * (1 - fraction) + green[b] * fraction, MidpointRounding.AwayFromZero);
            var bl = (byte)Math.Round(blue[a] * (1 - fraction) + blue[b] * fraction, MidpointRounding.AwayFromZero);
            Assert.Equal(red[x + 1], output[5 + x * 4]); Assert.Equal(g, output[6 + x * 4]);
            Assert.Equal(bl, output[7 + x * 4]); Assert.Equal(255, output[8 + x * 4]);
        }
        Assert.Equal(originalRed, red); Assert.Equal(originalGreen, green); Assert.Equal(originalBlue, blue);
        Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(5 + width * 4), value => Assert.Equal((byte)0xa5, value));
    }
}
