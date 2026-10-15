using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngShortPaletteRowTests
{
    private static byte[] Palette(int colors)
    {
        var palette = new byte[colors * 3];
        for (var i = 0; i < colors; i++)
        {
            palette[i * 3] = unchecked((byte)(i * 73 + 19));
            palette[i * 3 + 1] = unchecked((byte)(i * 17 ^ 139));
            palette[i * 3 + 2] = unchecked((byte)(255 - i * 101));
        }
        return palette;
    }

    [Theory]
    [InlineData(1, 1, 0)] [InlineData(15, 3, 2)] [InlineData(16, 16, 16)]
    [InlineData(17, 16, 0)] [InlineData(31, 15, 7)] [InlineData(513, 16, 16)]
    [InlineData(4095, 7, 3)] [InlineData(4096, 16, 16)] [InlineData(4097, 2, 1)]
    [InlineData(33, 1, 1)] [InlineData(32, 16, 15)] [InlineData(64, 4, 4)]
    public void PreservesIndependentChannelsPartialAlphaAndUnalignedGuards(int width, int colors, int alphaEntries)
    {
        var input = new byte[width + 2];
        for (var x = 0; x < width; x++) input[x + 1] = (byte)((x * 13 + 7) % colors);
        var palette = Palette(colors);
        var alpha = Enumerable.Range(0, alphaEntries).Select(i => unchecked((byte)(i * 101))).ToArray();
        var originalInput = input.ToArray(); var originalPalette = palette.ToArray(); var originalAlpha = alpha.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
        PngCodec.CopyPalette8Row(input.AsSpan(1, width), output.AsSpan(5, width * 4), palette, alpha, default);
        for (var x = 0; x < width; x++)
        {
            var index = input[x + 1];
            Assert.Equal(palette.AsSpan(index * 3, 3).ToArray(), output.AsSpan(5 + x * 4, 3).ToArray());
            Assert.Equal(index < alphaEntries ? alpha[index] : (byte)255, output[8 + x * 4]);
        }
        Assert.Equal(originalInput, input); Assert.Equal(originalPalette, palette); Assert.Equal(originalAlpha, alpha);
        Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(5 + width * 4), value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(0, 16)] [InlineData(15, 255)] [InlineData(16, 16)] [InlineData(31, 128)]
    public void RejectsInvalidGroupsAfterWritingOnlyTheOriginalValidPrefix(int badPosition, byte badIndex)
    {
        const int width = 48;
        var input = Enumerable.Range(0, width).Select(x => (byte)(x % 16)).ToArray(); input[badPosition] = badIndex;
        var palette = Palette(16); var alpha = Enumerable.Range(0, 16).Select(x => (byte)(x * 17)).ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4).ToArray();
        var error = Assert.Throws<ImageDataException>(() => PngCodec.CopyPalette8Row(input, output, palette, alpha, default));
        Assert.Equal("PNG palette index out of range.", error.Message);
        for (var x = 0; x < badPosition; x++)
        {
            Assert.Equal(palette.AsSpan(input[x] * 3, 3).ToArray(), output.AsSpan(x * 4, 3).ToArray());
            Assert.Equal(alpha[input[x]], output[x * 4 + 3]);
        }
        Assert.All(output.Skip(badPosition * 4), value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(16)] [InlineData(4097)]
    public void CancellationPrecedesTheFirstInvalidIndexWithoutWrites(int width)
    {
        var input = new byte[width]; input[0] = 7; var palette = Palette(7);
        var output = Enumerable.Repeat((byte)0xa5, width * 4).ToArray(); var token = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() => PngCodec.CopyPalette8Row(input, output, palette, [], token));
        Assert.Equal(token, error.CancellationToken);
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
