using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngUnfilterTests
{
    [Fact]
    public void UpMatchesIndependentByteSumsAtVectorTailsAndCancellationBoundaries()
    {
        foreach (var length in new[] { 0, 1, 15, 16, 17, 31, 32, 33, 63, 64, 65, 255, 256, 257, 65535, 65536, 65537 })
        {
            var row = Enumerable.Range(0, length + 19).Select(i => (byte)(53 * i + 211)).ToArray();
            var previous = Enumerable.Range(0, length + 23).Select(i => (byte)(97 * i + 173)).ToArray();
            var expected = row.ToArray(); var originalPrevious = previous.ToArray();
            for (var i = 0; i < length; i++) expected[i + 3] = unchecked((byte)(expected[i + 3] + previous[i + 7]));
            PngCodec.Unfilter(row.AsSpan(3, length), previous.AsSpan(7, length), 8, 2);
            Assert.Equal(expected, row); Assert.Equal(originalPrevious, previous);
        }
    }

    [Fact]
    public void UpOverlapRetainsSequentialScalarSemantics()
    {
        foreach (var (rowOffset, previousOffset) in new[] { (4, 4), (4, 5), (5, 4) })
        {
            var bytes = Enumerable.Range(0, 96).Select(i => (byte)(53 * i + 211)).ToArray();
            var expected = bytes.ToArray();
            for (var i = 0; i < 80; i++) expected[rowOffset + i] = unchecked((byte)(expected[rowOffset + i] + expected[previousOffset + i]));
            PngCodec.Unfilter(bytes.AsSpan(rowOffset, 80), bytes.AsSpan(previousOffset, 80), 8, 2);
            Assert.Equal(expected, bytes);
        }
    }

    [Fact]
    public void UpCancellationPreservesBytesAndEmptyRowBehavior()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        PngCodec.Unfilter([], [], 8, 2, cancellation.Token);
        foreach (var length in new[] { 1, 15, 16, 63, 64, 65536, 65537 })
        {
            var row = Enumerable.Range(0, length).Select(i => (byte)i).ToArray(); var original = row.ToArray();
            var previous = original.ToArray();
            var error = Assert.Throws<OperationCanceledException>(() => PngCodec.Unfilter(row, previous, 8, 2, cancellation.Token));
            Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(original, row);
        }
    }

    [Fact]
    public void NonePreservesBytesAndGuardsWithoutReadingNeighbourRows()
    {
        foreach (var length in new[] { 0, 1, 7, 8, 65535, 65536, 65537 })
        {
            var bytes = Enumerable.Range(0, length + 8).Select(i => (byte)(53 * i + 7)).ToArray();
            var original = bytes.ToArray();
            PngCodec.Unfilter(bytes.AsSpan(3, length), [], 8, 0);
            Assert.Equal(original, bytes);
        }
    }

    [Fact]
    public void NoneChecksCancellationWithoutChangingPixels()
    {
        var bytes = Enumerable.Range(0, 65537).Select(i => (byte)i).ToArray(); var original = bytes.ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => PngCodec.Unfilter(bytes, [], 4, 0, cancellation.Token));
        Assert.Equal(original, bytes);
    }
}
