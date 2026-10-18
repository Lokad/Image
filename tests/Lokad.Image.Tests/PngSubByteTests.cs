using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngSubByteTests
{
    [Fact]
    public void IndependentBytePrefixesPreserveTailsAlignmentAndModuloWrap()
    {
        foreach (var length in new[] { 0, 1, 3, 4, 5, 7, 8, 9, 15, 16, 17, 31, 32, 33, 2052, 65535, 65536, 65537 })
        foreach (var offset in new[] { 1, 7, 16 })
        foreach (var delta in new[] { 0, 1, 127, 128, 255 })
        {
            var bytes = Enumerable.Range(0, length + offset + 16)
                .Select(i => unchecked((byte)(delta + i * 79 + (i >> 4) * 47))).ToArray();
            var expected = bytes.ToArray();
            var previous = Enumerable.Range(0, length).Select(i => (byte)(i * 137 + 23)).ToArray();
            var unchanged = previous.ToArray();
            Reference(expected.AsSpan(offset, length), previous);
            PngCodec.Unfilter(bytes.AsSpan(offset, length), previous, 4, 1);
            Assert.Equal(expected, bytes); Assert.Equal(unchanged, previous);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(3)] [InlineData(4)] [InlineData(15)] [InlineData(23)]
    public void ShortPreviousRowsKeepOriginalPartialUpdates(int previousLength)
    {
        var bytes = Enumerable.Range(0, 24).Select(i => (byte)(i * 79 + 17)).ToArray();
        var expected = bytes.ToArray(); var previous = new byte[previousLength];
        Assert.Throws<IndexOutOfRangeException>(() => Reference(expected, previous));
        Assert.Throws<IndexOutOfRangeException>(() => PngCodec.Unfilter(bytes, previous, 4, 1));
        Assert.Equal(expected, bytes);
    }

    [Theory]
    [InlineData(-4)] [InlineData(-1)] [InlineData(0)] [InlineData(1)] [InlineData(4)]
    public void AliasedPreviousRowsKeepSequentialByteOrder(int relative)
    {
        var bytes = Enumerable.Range(0, 96).Select(i => (byte)(i * 79 + 17)).ToArray();
        var expected = bytes.ToArray();
        Reference(expected.AsSpan(24, 48), expected.AsSpan(24 + relative, 48));
        PngCodec.Unfilter(bytes.AsSpan(24, 48), bytes.AsSpan(24 + relative, 48), 4, 1);
        Assert.Equal(expected, bytes);
    }

    [Fact]
    public void CancellationKeepsBytesAndTokenWhileEmptyRowsStayEmpty()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        foreach (var length in new[] { 1, 3, 4, 15, 16, 17, 65536, 65537 })
        {
            var bytes = Enumerable.Range(0, length).Select(i => (byte)i).ToArray();
            var unchanged = bytes.ToArray();
            var error = Assert.Throws<OperationCanceledException>(() =>
                PngCodec.Unfilter(bytes, new byte[length], 4, 1, cancellation.Token));
            Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(unchanged, bytes);
        }
        PngCodec.Unfilter([], [], 4, 1, cancellation.Token);
    }

    private static void Reference(Span<byte> row, ReadOnlySpan<byte> previous)
    {
        for (var i = 0; i < row.Length; i++)
        {
            var left = i >= 4 ? row[i - 4] : 0; _ = previous[i];
            row[i] = unchecked((byte)(row[i] + left));
        }
    }
}
