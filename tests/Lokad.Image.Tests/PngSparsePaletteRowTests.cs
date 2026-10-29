using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngSparsePaletteRowTests
{
    [Theory]
    [InlineData(8, 2, 513, 256)] [InlineData(8, 4, 17, 0)]
    [InlineData(8, 8, 1, 1)] [InlineData(8, 8, 4097, 2)]
    [InlineData(4, 2, 513, 16)] [InlineData(4, 4, 17, 1)] [InlineData(4, 8, 1, 0)]
    [InlineData(2, 2, 33, 4)] [InlineData(2, 4, 17, 1)] [InlineData(2, 8, 1, 0)]
    [InlineData(1, 2, 513, 2)] [InlineData(1, 4, 17, 1)] [InlineData(1, 8, 1, 0)]
    [InlineData(8, 2, 0, 0)]
    public void OnlyPassPixelsChangeWithExactPaletteAlphaPaddingAndUnalignedGuards(int depth, int step, int width, int alphaEntries)
    {
        var colors = 1 << depth; var palette = new byte[colors * 3];
        for (var i = 0; i < colors; i++)
        {
            palette[i * 3] = unchecked((byte)(17 + i * 73));
            palette[i * 3 + 1] = unchecked((byte)(29 + i * 137));
            palette[i * 3 + 2] = unchecked((byte)(43 + i * i));
        }
        var alpha = Enumerable.Range(0, alphaEntries).Select(i => unchecked((byte)(i * 101))).ToArray();
        var indices = Enumerable.Range(0, width).Select(x => (x * 7 + x / 13) & (colors - 1)).ToArray();
        var row = Pack(indices, depth); var originalRow = row.ToArray();
        var originalPalette = palette.ToArray(); var originalAlpha = alpha.ToArray();
        var bytes = width == 0 ? 0 : ((width - 1) * step + 1) * 4;
        var actual = Enumerable.Repeat((byte)0xa5, bytes + 14).ToArray(); var expected = actual.ToArray();
        for (var x = 0; x < width; x++)
        {
            var index = indices[x]; var pixel = 7 + x * step * 4;
            Array.Copy(palette, index * 3, expected, pixel, 3);
            expected[pixel + 3] = index < alpha.Length ? alpha[index] : (byte)255;
        }
        PngCodec.CopySparsePaletteRow(row.AsSpan(1, row.Length - 2), actual.AsSpan(7, bytes), width, depth, step, palette, alpha, default);
        Assert.Equal(expected, actual); Assert.Equal(originalRow, row);
        Assert.Equal(originalPalette, palette); Assert.Equal(originalAlpha, alpha);
    }

    [Theory]
    [InlineData(8, 16)] [InlineData(4, 3)] [InlineData(2, 3)] [InlineData(1, 1)]
    public void FirstInvalidIndexLeavesItAndLaterPassPixelsUntouched(int depth, int colors)
    {
        const int width = 17, bad = 9, step = 4;
        var indices = new int[width]; indices[bad] = colors;
        var row = Pack(indices, depth); var palette = new byte[colors * 3];
        palette[0] = 11; palette[1] = 22; palette[2] = 33;
        var actual = Enumerable.Repeat((byte)0xa5, width * step * 4).ToArray(); var expected = actual.ToArray();
        for (var x = 0; x < bad; x++) Array.Copy(new byte[] { 11, 22, 33, 0 }, 0, expected, x * step * 4, 4);
        var error = Assert.Throws<ImageDataException>(() => PngCodec.CopySparsePaletteRow(
            row.AsSpan(1, row.Length - 2), actual, width, depth, step, palette, new byte[] { 0 }, default));
        Assert.Equal("PNG palette index out of range.", error.Message); Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(1)] [InlineData(17)] [InlineData(4097)]
    public void PreCancellationWinsBeforeEvenAnInvalidPaletteAndNoPixelIsWritten(int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 8).ToArray();
        var token = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() => PngCodec.CopySparsePaletteRow(
            new byte[width], output, width, 8, 2, [], [], token));
        Assert.Equal(token, error.CancellationToken); Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }

    private static byte[] Pack(int[] indices, int depth)
    {
        var bytes = (indices.Length * depth + 7) / 8; var result = new byte[bytes + 2];
        result[0] = result[^1] = 0xa5;
        for (var x = 0; x < indices.Length; x++)
            result[1 + x * depth / 8] |= (byte)(indices[x] << (8 - depth - x * depth % 8));
        var used = indices.Length * depth % 8;
        if (used != 0) result[bytes] |= (byte)((1 << (8 - used)) - 1);
        return result;
    }
}
