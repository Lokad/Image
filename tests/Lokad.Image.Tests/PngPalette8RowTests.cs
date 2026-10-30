using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngPalette8RowTests
{
    private static byte[] Palette()
    {
        var result = new byte[768];
        for (var i = 0; i < 256; i++)
        {
            result[i * 3] = (byte)i; result[i * 3 + 1] = (byte)(255 - i);
            result[i * 3 + 2] = unchecked((byte)(i * 73 + 19));
        }
        return result;
    }

    [Theory]
    [InlineData(1, 256)] [InlineData(7, 256)] [InlineData(8, 256)] [InlineData(9, 256)]
    [InlineData(16, 256)] [InlineData(17, 256)] [InlineData(256, 256)] [InlineData(513, 256)]
    [InlineData(4095, 256)] [InlineData(4096, 256)] [InlineData(4097, 256)]
    [InlineData(256, 0)] [InlineData(256, 17)]
    [InlineData(255, 256)] [InlineData(257, 256)] [InlineData(1025, 256)]
    [InlineData(513, 0)] [InlineData(513, 255)]
    [InlineData(513, 1)] [InlineData(513, 2)]
    [InlineData(513, 63)] [InlineData(513, 64)] [InlineData(513, 65)]
    [InlineData(513, 127)] [InlineData(513, 128)] [InlineData(513, 129)]
    [InlineData(513, 191)] [InlineData(513, 192)] [InlineData(513, 193)]
    public void PreservesAllIndicesChannelsAndPartialAlphaWithUnalignedGuards(int width, int alphaEntries)
    {
        var input = new byte[width + 2];
        for (var x = 0; x < width; x++) input[x + 1] = (byte)x;
        var original = input.ToArray(); var palette = Palette(); var originalPalette = palette.ToArray();
        var alpha = Enumerable.Range(0, alphaEntries).Select(i => (byte)i).ToArray(); var originalAlpha = alpha.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
        PngCodec.CopyPalette8Row(input.AsSpan(1, width), output.AsSpan(5, width * 4), palette, alpha, new PngCodec.Palette8Lookup(palette, alpha), default);
        for (var x = 0; x < width; x++)
        {
            var index = (byte)x;
            Assert.Equal(index, output[5 + x * 4]); Assert.Equal((byte)(255 - index), output[6 + x * 4]);
            Assert.Equal(unchecked((byte)(index * 73 + 19)), output[7 + x * 4]);
            Assert.Equal(index < alphaEntries ? index : (byte)255, output[8 + x * 4]);
        }
        Assert.Equal(original, input); Assert.Equal(originalPalette, palette); Assert.Equal(originalAlpha, alpha);
        Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 4 + 5), value => Assert.Equal((byte)0xa5, value));
    }

    [Fact]
    public void FullPaletteHandlesMixedIndicesAndUnrelatedChannels()
    {
        const int width = 513;
        var row = new byte[width]; var palette = new byte[768]; var alpha = new byte[256];
        for (var i = 0; i < 256; i++)
        {
            palette[i * 3] = unchecked((byte)(i * 37 + 41));
            palette[i * 3 + 1] = unchecked((byte)(i * i + 17));
            palette[i * 3 + 2] = (byte)(i ^ 0xb6); alpha[i] = unchecked((byte)(i * 101));
        }
        for (var x = 0; x < width; x++) row[x] = unchecked((byte)(x * 173 + 127));
        var output = new byte[width * 4];
        PngCodec.CopyPalette8Row(row, output, palette, alpha, new PngCodec.Palette8Lookup(palette, alpha), default);
        for (var x = 0; x < width; x++)
        {
            Assert.Equal(palette.AsSpan(row[x] * 3, 3).ToArray(), output.AsSpan(x * 4, 3).ToArray());
            Assert.Equal(alpha[row[x]], output[x * 4 + 3]);
        }
    }

    [Fact]
    public void RejectsFirstInvalidIndexBeforeWritingItOrLaterPixels()
    {
        byte[] palette = [11, 22, 33, 44, 55, 66];
        var output = Enumerable.Repeat((byte)0xa5, 16).ToArray();
        var error = Assert.Throws<ImageDataException>(() =>
            PngCodec.CopyPalette8Row(new byte[] { 0, 1, 2, 0 }, output, palette, new byte[] { 0 }, new PngCodec.Palette8Lookup(palette, new byte[] { 0 }), default));
        Assert.Equal("PNG palette index out of range.", error.Message);
        Assert.Equal(new byte[] { 11, 22, 33, 0, 44, 55, 66, 255 }, output[..8]);
        Assert.All(output.Skip(8), value => Assert.Equal((byte)0xa5, value));
    }

    [Fact]
    public void PartialAlphaChecksCancellationBeforeVectorOutput()
    {
        var output = Enumerable.Repeat((byte)0xa5, 513 * 4).ToArray();
        Assert.Throws<OperationCanceledException>(() => PngCodec.CopyPalette8Row(
            new byte[513], output, Palette(), new byte[] { 0, 1 }, new PngCodec.Palette8Lookup(Palette(), new byte[] { 0, 1 }), new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(1)] [InlineData(8)]
    public void ChecksCancellationBeforeEvenAnInvalidIndex(int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 4).ToArray();
        Assert.Throws<OperationCanceledException>(() =>
            PngCodec.CopyPalette8Row(new byte[width], output, Array.Empty<byte>(), Array.Empty<byte>(), default, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
