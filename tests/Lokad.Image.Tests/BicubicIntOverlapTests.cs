using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class BicubicIntOverlapTests
{
    [Theory]
    [InlineData(513, 256, 124)] [InlineData(31, 16, 4)]
    [InlineData(13, 7, 0)] [InlineData(9, 5, 0)]
    public void PreparedWordWindowsPreserveEveryLiveOffsetAndGuards(int sourceWidth, int width, int expectedPairs)
    {
        var counts = new int[width]; var storage = new int[width * 10 + 2];
        storage.AsSpan().Fill(-99); var offsets = storage.AsSpan(1, width * 10);
        var scale = (double)sourceWidth / width;
        for (var x = 0; x < width; x++)
        {
            var center = (x + .5) * sourceWidth / width - .5;
            var first = (int)Math.Ceiling(center - 2 * scale);
            counts[x] = (int)Math.Floor(center + 2 * scale) - first + 1;
            for (var t = 0; t < counts[x]; t++) offsets[x * 10 + t] = Math.Clamp(first + t, 0, sourceWidth - 1);
        }
        var original = offsets.ToArray();
        BicubicMath.PrepareOverlappingPairs(offsets, counts, width, sourceWidth, 10, 8, 2, default);
        var pairs = 0;
        for (var x = 0; x < width; x++)
        {
            Assert.Equal(original.AsSpan(x * 10, counts[x]).ToArray(), offsets.Slice(x * 10, counts[x]).ToArray());
            if (offsets[x * 10 + 9] == 0) continue;
            pairs++;
            Assert.Equal(0, x & 1); Assert.True(x + 1 < width);
            Assert.Equal(8, counts[x]); Assert.Equal(8, counts[x + 1]);
            var first = offsets[x * 10 + 9] - 1;
            Assert.InRange(first, 0, sourceWidth - 10);
            for (var t = 0; t < 8; t++)
            {
                Assert.Equal(first + t, offsets[x * 10 + t]);
                Assert.Equal(first + 2 + t, offsets[(x + 1) * 10 + t]);
            }
        }
        Assert.Equal(expectedPairs, pairs); Assert.Equal(-99, storage[0]); Assert.Equal(-99, storage[^1]);
    }

    [Theory]
    [InlineData(300)] [InlineData(2147483637)]
    public void WordIndexesAreNotTruncatedAndMarkersDoNotOverflow(int first)
    {
        var offsets = new int[20];
        for (var t = 0; t < 8; t++) { offsets[t] = first + t; offsets[10 + t] = first + 2 + t; }
        BicubicMath.PrepareOverlappingPairs(offsets, new[] { 8, 8 }, 2, first + 10, 10, 8, 2, default);
        Assert.Equal(first + 1, offsets[9]); Assert.Equal(0, offsets[19]);
        for (var t = 0; t < 8; t++) { Assert.Equal(first + t, offsets[t]); Assert.Equal(first + 2 + t, offsets[10 + t]); }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void ClippedUnequalFullNegativeAndOutOfRangeWindowsKeepOriginalOffsets(int kind)
    {
        var counts = kind == 1 ? new[] { 7, 8 } : kind == 2 ? new[] { 10, 10 } : new[] { 8, 8 };
        var offsets = new int[20]; var first = kind == 3 ? -1 : 2;
        for (var t = 0; t < 10; t++) { offsets[t] = first + t; offsets[10 + t] = first + 2 + t; }
        if (kind == 0) offsets[1] = offsets[0];
        var original = offsets.ToArray();
        BicubicMath.PrepareOverlappingPairs(offsets, counts, 2, kind == 4 ? 10 : 20, 10, 8, 2, default);
        for (var x = 0; x < 2; x++)
        {
            Assert.Equal(original.AsSpan(x * 10, counts[x]).ToArray(), offsets.AsSpan(x * 10, counts[x]).ToArray());
            if (counts[x] < 10) Assert.Equal(0, offsets[x * 10 + 9]);
        }
    }

    [Fact]
    public void CancellationBeforePreparationDoesNotPublishMetadata()
    {
        var offsets = Enumerable.Repeat(-99, 20).ToArray();
        var cancellation = new CancellationToken(true);
        var error = Assert.Throws<OperationCanceledException>(() =>
            BicubicMath.PrepareOverlappingPairs(offsets, new[] { 8, 8 }, 2, 20, 10, 8, 2, cancellation));
        Assert.Equal(cancellation, error.CancellationToken); Assert.All(offsets, n => Assert.Equal(-99, n));
    }
}
