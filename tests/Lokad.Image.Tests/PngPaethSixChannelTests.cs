using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngPaethSixChannelTests
{
    [Fact]
    public void EveryPredictorTripleRetainsTieOrderAndByteOverflow()
    {
        // All256^3 byte triples, six lanes per call; the final group repeats
        // two upper-left values. A third pixel permits the bounded wider load.
        Span<byte> row = stackalloc byte[18]; Span<byte> previous = stackalloc byte[18];
        row.Clear(); previous.Clear();
        for (var left = 0; left < 256; left++)
        for (var up = 0; up < 256; up++)
        for (var baseUpperLeft = 0; baseUpperLeft < 256; baseUpperLeft += 6)
        {
            for (var lane = 0; lane < 6; lane++)
            {
                var upperLeft = (baseUpperLeft + lane) & 255;
                previous[lane] = (byte)upperLeft; previous[6 + lane] = (byte)up;
                row[lane] = unchecked((byte)(left - upperLeft));
                row[6 + lane] = (byte)((left + up * 3 + upperLeft * 7) & 255);
            }
            PngCodec.Unfilter(row, previous, 6, 4);
            for (var lane = 0; lane < 6; lane++)
            {
                var upperLeft = (baseUpperLeft + lane) & 255;
                var delta = (left + up * 3 + upperLeft * 7) & 255;
                var expected = unchecked((byte)(delta + Predict(left, up, upperLeft)));
                if (row[lane] != left || row[6 + lane] != expected)
                    throw new InvalidOperationException($"Paeth differs at left{left}/up{up}/upperLeft{upperLeft}.");
            }
        }
    }

    [Fact]
    public void TailsChunksFirstRowsAndGuardBytesMatchScalarReconstruction()
    {
        foreach (var length in new[] { 0, 1, 5, 6, 7, 8, 9, 11, 12, 13, 17, 65531, 65532, 65533, 65535, 65536, 65537, 65540, 65541, 131063, 131064, 131065 })
        foreach (var firstRow in new[] { false, true })
        {
            var row = Enumerable.Range(0, length + 16).Select(i => (byte)(i * 79 + (i >> 4) * 47)).ToArray();
            var expected = row.ToArray();
            var previous = Enumerable.Range(0, length).Select(i => firstRow ? (byte)0 : (byte)(i * 137 + 23)).ToArray();
            var unchanged = previous.ToArray();
            Reconstruct(expected.AsSpan(8, length), previous, 6);
            PngCodec.Unfilter(row.AsSpan(8, length), previous, 6, 4);
            Assert.Equal(expected, row); Assert.Equal(unchanged, previous);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(11)]
    public void TruncatedPreviousRowKeepsScalarPartialUpdateAndException(int previousLength)
    {
        var row = Enumerable.Range(0, 24).Select(i => (byte)(i * 79 + 17)).ToArray(); var expected = row.ToArray();
        var previous = Enumerable.Range(0, previousLength).Select(i => (byte)(i * 137 + 23)).ToArray();
        Assert.Throws<IndexOutOfRangeException>(() => Reconstruct(expected, previous, 6));
        Assert.Throws<IndexOutOfRangeException>(() => PngCodec.Unfilter(row, previous, 6, 4));
        Assert.Equal(expected, row);
    }

    [Theory]
    [InlineData(-6)] [InlineData(-1)] [InlineData(0)] [InlineData(1)] [InlineData(6)]
    public void OverlappingRowsKeepScalarReadWriteOrder(int relativeOffset)
    {
        var bytes = Enumerable.Range(0, 80).Select(i => (byte)(i * 79 + 17)).ToArray(); var expected = bytes.ToArray();
        Reconstruct(expected.AsSpan(24, 32), expected.AsSpan(24 + relativeOffset, 32), 6);
        PngCodec.Unfilter(bytes.AsSpan(24, 32), bytes.AsSpan(24 + relativeOffset, 32), 6, 4);
        Assert.Equal(expected, bytes);
    }

    [Fact]
    public void PreCancellationKeepsNonemptyRowUntouchedAndEmptyRowBehavior()
    {
        var row = Enumerable.Range(0, 65537).Select(i => (byte)i).ToArray(); var unchanged = row.ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => PngCodec.Unfilter(row, new byte[row.Length], 6, 4, cancellation.Token));
        Assert.Equal(unchanged, row);
        PngCodec.Unfilter([], [], 6, 4, cancellation.Token);
    }

    private static int Predict(int left, int up, int upperLeft)
    {
        var p = left + up - upperLeft;
        var a = Math.Abs(p - left); var b = Math.Abs(p - up); var c = Math.Abs(p - upperLeft);
        return a <= b && a <= c ? left : b <= c ? up : upperLeft;
    }

    private static void Reconstruct(Span<byte> row, ReadOnlySpan<byte> previous, int stride)
    {
        for (var i = 0; i < row.Length; i++)
            row[i] = unchecked((byte)(row[i] + Predict(i >= stride ? row[i - stride] : 0,
                previous[i], i >= stride ? previous[i - stride] : 0)));
    }
}
