using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngGrayAlpha8RowTests
{
    [Theory]
    [InlineData(1)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    [InlineData(16)] [InlineData(17)] [InlineData(256)] [InlineData(513)]
    [InlineData(4095)] [InlineData(4096)] [InlineData(4097)] [InlineData(65536)]
    public void ExpandsGrayAndRetainsEveryAlphaIncludingHiddenRgbAndRowGuards(int width)
    {
        var input = new byte[width * 2 + 2];
        for (var x = 0; x < width; x++) { input[x * 2 + 1] = (byte)x; input[x * 2 + 2] = (byte)(x >> 8); }
        var original = input.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
        PngCodec.CopyGrayAlpha8Row(input.AsSpan(1, width * 2), output.AsSpan(5, width * 4), default);
        for (var x = 0; x < width; x++)
        {
            for (var channel = 0; channel < 3; channel++) Assert.Equal((byte)x, output[5 + x * 4 + channel]);
            Assert.Equal((byte)(x >> 8), output[5 + x * 4 + 3]);
        }
        Assert.Equal(original, input);
        Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 4 + 5), value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(1)] [InlineData(8)]
    public void ChecksCancellationBeforeWriting(int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 4).ToArray();
        Assert.Throws<OperationCanceledException>(() =>
            PngCodec.CopyGrayAlpha8Row(new byte[width * 2], output, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
