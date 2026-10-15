using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegRgbRowTests
{
    [Theory]
    [InlineData(1)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    [InlineData(31)] [InlineData(513)] [InlineData(4097)]
    public void DirectPlanesPreserveEveryChannelAndOpaqueAlphaWithUnalignedGuards(int width)
    {
        var red = new byte[width + 2]; var green = new byte[width + 2]; var blue = new byte[width + 2];
        for (var x = 0; x < width; x++)
        {
            red[x + 1] = unchecked((byte)(x * 73 + 19));
            green[x + 1] = unchecked((byte)(x * 17 ^ 139));
            blue[x + 1] = unchecked((byte)(255 - x * 101));
        }
        var originalRed = red.ToArray(); var originalGreen = green.ToArray(); var originalBlue = blue.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
        JpegDecoder.RenderRgbRow(red.AsSpan(1, width), green.AsSpan(1, width), blue.AsSpan(1, width), output.AsSpan(5, width * 4));
        for (var x = 0; x < width; x++)
        {
            Assert.Equal(red[x + 1], output[5 + x * 4]); Assert.Equal(green[x + 1], output[6 + x * 4]);
            Assert.Equal(blue[x + 1], output[7 + x * 4]); Assert.Equal(255, output[8 + x * 4]);
        }
        Assert.Equal(originalRed, red); Assert.Equal(originalGreen, green); Assert.Equal(originalBlue, blue);
        Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(5 + width * 4), value => Assert.Equal((byte)0xa5, value));
    }
}
