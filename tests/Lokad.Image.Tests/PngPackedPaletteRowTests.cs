using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngPackedPaletteRowTests
{
    public static IEnumerable<object[]> Rows()
    {
        foreach (var depth in new[] { 1, 2, 4 })
        foreach (var width in new[] { 1, 7, 15, 16, 17, 256, 513, 4095, 4096, 4097 })
            yield return new object[] { depth, width, 1 << depth };
        foreach (var depth in new[] { 1, 2, 4 })
        foreach (var width in new[] { 17, 513 })
        foreach (var alpha in new[] { 0, 1 })
            yield return new object[] { depth, width, alpha };
    }

    [Theory]
    [MemberData(nameof(Rows))]
    public void PreservesIndicesRgbAlphaPaddingAndUnalignedGuards(int depth, int width, int alphaEntries)
    {
        var count = (width * depth + 7) / 8; var input = new byte[count + 2];
        for (var i = 0; i < count; i++) input[i + 1] = unchecked((byte)(i * 73 + 19));
        var original = input.ToArray(); var colors = 1 << depth;
        var palette = new byte[colors * 3]; var alpha = new byte[alphaEntries];
        for (var i = 0; i < colors; i++)
        {
            palette[i * 3] = (byte)(i * 11 + 5); palette[i * 3 + 1] = (byte)(255 - i * 13);
            palette[i * 3 + 2] = unchecked((byte)(i * 73 + 19));
            if (i < alphaEntries) alpha[i] = (byte)(i * 255 / (colors - 1));
        }
        var originalPalette = palette.ToArray(); var originalAlpha = alpha.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
        PngCodec.CopyPackedPaletteRow(input.AsSpan(1, count), output.AsSpan(5, width * 4), width, depth, palette, alpha, default);
        for (var x = 0; x < width; x++)
        {
            var index = original[1 + x * depth / 8] >> (8 - depth - x * depth % 8) & (colors - 1);
            Assert.Equal((byte)(index * 11 + 5), output[5 + x * 4]);
            Assert.Equal((byte)(255 - index * 13), output[6 + x * 4]);
            Assert.Equal(unchecked((byte)(index * 73 + 19)), output[7 + x * 4]);
            Assert.Equal(index < alphaEntries ? (byte)(index * 255 / (colors - 1)) : (byte)255, output[8 + x * 4]);
        }
        Assert.Equal(original, input); Assert.Equal(originalPalette, palette); Assert.Equal(originalAlpha, alpha);
        Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 4 + 5), value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(4)]
    public void RejectsFirstInvalidIndexBeforeWritingIt(int depth)
    {
        // First index zero is valid; second index one exceeds a one-entry PLTE.
        var input = new byte[] { (byte)(1 << (8 - 2 * depth)) };
        var output = Enumerable.Repeat((byte)0xa5, 8).ToArray();
        var error = Assert.Throws<ImageDataException>(() =>
            PngCodec.CopyPackedPaletteRow(input, output, 2, depth, new byte[] { 11, 22, 33 }, new byte[] { 0 }, default));
        Assert.Equal("PNG palette index out of range.", error.Message);
        Assert.Equal(new byte[] { 11, 22, 33, 0 }, output[..4]);
        Assert.All(output.Skip(4), value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(1, 1)] [InlineData(1, 16)] [InlineData(1, 4097)]
    [InlineData(2, 1)] [InlineData(2, 16)] [InlineData(2, 4097)]
    [InlineData(4, 1)] [InlineData(4, 16)] [InlineData(4, 4097)]
    public void ChecksCancellationBeforeWriting(int depth, int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 4).ToArray();
        Assert.Throws<OperationCanceledException>(() =>
            PngCodec.CopyPackedPaletteRow(new byte[(width * depth + 7) / 8], output, width, depth,
                new byte[48], new byte[16], new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
