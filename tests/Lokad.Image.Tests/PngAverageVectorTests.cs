using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngAverageVectorTests
{
    [Theory]
    [InlineData(4)] [InlineData(8)]
    public void EveryPredictorPairPreservesFloorAndByteOverflow(int stride)
    {
        Span<byte> row = stackalloc byte[stride * 2];
        Span<byte> previous = stackalloc byte[stride * 2];
        for (var left = 0; left < 256; left++)
        for (var up = 0; up < 256; up++)
        {
            for (var lane = 0; lane < stride; lane++)
            {
                previous[lane] = 0; previous[stride + lane] = (byte)up;
                row[lane] = (byte)left;
                row[stride + lane] = unchecked((byte)(left * 17 + up * 31 + lane * 53));
            }
            PngCodec.Unfilter(row, previous, stride, 3);
            for (var lane = 0; lane < stride; lane++)
            {
                var delta = unchecked((byte)(left * 17 + up * 31 + lane * 53));
                Assert.Equal((byte)left, row[lane]);
                Assert.Equal(unchecked((byte)(delta + (left + up) / 2)), row[stride + lane]);
            }
        }
    }

    [Theory]
    [InlineData(4)] [InlineData(8)]
    public void TailsGuardsFirstRowsAndOverlapKeepByteRecurrence(int stride)
    {
        foreach (var length in new[] { 0, 1, 3, 4, 5, 7, 8, 9, 15, 16, 17, 65535, 65536, 65537 })
        foreach (var firstRow in new[] { false, true })
        {
            var row = Enumerable.Range(0, length + 11).Select(i => (byte)(i * 79 + 211)).ToArray();
            var previous = Enumerable.Range(0, length + 13).Select(i => firstRow ? (byte)0 : (byte)(i * 137 + 173)).ToArray();
            var expected = row.ToArray(); var unchangedPrevious = previous.ToArray();
            Reconstruct(expected.AsSpan(3, length), previous.AsSpan(5, length), stride);
            PngCodec.Unfilter(row.AsSpan(3, length), previous.AsSpan(5, length), stride, 3);
            Assert.Equal(expected, row); Assert.Equal(unchangedPrevious, previous);
        }
        foreach (var relative in new[] { -stride, -1, 0, 1, stride })
        {
            var bytes = Enumerable.Range(0, 96).Select(i => (byte)(i * 79 + 17)).ToArray();
            var expected = bytes.ToArray();
            Reconstruct(expected.AsSpan(24, 48), expected.AsSpan(24 + relative, 48), stride);
            PngCodec.Unfilter(bytes.AsSpan(24, 48), bytes.AsSpan(24 + relative, 48), stride, 3);
            Assert.Equal(expected, bytes);
        }
    }

    [Theory]
    [InlineData(4)] [InlineData(8)]
    public void CancellationAndTruncatedPreviousKeepOriginalBytesAndErrors(int stride)
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        PngCodec.Unfilter([], [], stride, 3, cancellation.Token);
        foreach (var length in new[] { 1, stride - 1, stride, stride + 1, 65537 })
        {
            var row = Enumerable.Range(0, length).Select(i => (byte)i).ToArray(); var original = row.ToArray();
            var error = Assert.Throws<OperationCanceledException>(() => PngCodec.Unfilter(row, new byte[length], stride, 3, cancellation.Token));
            Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(original, row);
        }
        foreach (var length in new[] { 0, 1, 3, 4, 7, 8, 9 })
        {
            var row = Enumerable.Range(0, 24).Select(i => (byte)(i * 79 + 17)).ToArray(); var expected = row.ToArray();
            var previous = Enumerable.Range(0, length).Select(i => (byte)(i * 137 + 23)).ToArray();
            Assert.Throws<IndexOutOfRangeException>(() => Reconstruct(expected, previous, stride));
            Assert.Throws<IndexOutOfRangeException>(() => PngCodec.Unfilter(row, previous, stride, 3));
            Assert.Equal(expected, row);
        }
    }

    private static void Reconstruct(Span<byte> row, ReadOnlySpan<byte> previous, int stride)
    {
        for (var i = 0; i < row.Length; i++)
        {
            var left = i >= stride ? row[i - stride] : 0;
            row[i] = unchecked((byte)(row[i] + (left + previous[i]) / 2));
        }
    }
}
