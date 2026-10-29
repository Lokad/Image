using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngPaethThreeChannelTests
{
    [Fact]
    public void EveryPredictorTripleRetainsTieOrderAndByteOverflow()
    {
        // All256^3 triples, three independent channels per call. A third pixel
        // permits the bounded four-byte load; its first raw byte must survive.
        Span<byte> row = stackalloc byte[9]; Span<byte> previous = stackalloc byte[9];
        for (var left = 0; left < 256; left++)
        for (var up = 0; up < 256; up++)
        for (var baseUpperLeft = 0; baseUpperLeft < 256; baseUpperLeft += 3)
        {
            for (var lane = 0; lane < 3; lane++)
            {
                var upperLeft = (baseUpperLeft + lane) & 255;
                previous[lane] = (byte)upperLeft; previous[3 + lane] = (byte)up;
                previous[6 + lane] = (byte)(up ^ upperLeft);
                row[lane] = unchecked((byte)(left - upperLeft));
                row[3 + lane] = unchecked((byte)(left + up * 3 + upperLeft * 7));
                row[6 + lane] = (byte)(left ^ up ^ upperLeft);
            }
            var lastRaw = row[6];
            PngCodec.Unfilter(row, previous, 3, 4);
            for (var lane = 0; lane < 3; lane++)
            {
                var upperLeft = (baseUpperLeft + lane) & 255;
                var delta = (left + up * 3 + upperLeft * 7) & 255;
                var expected = unchecked((byte)(delta + Predict(left, up, upperLeft)));
                if (row[lane] != left || row[3 + lane] != expected)
                    throw new InvalidOperationException($"Paeth differs at left{left}/up{up}/upperLeft{upperLeft}.");
            }
            if (unchecked((byte)(lastRaw + Predict(row[3], previous[6], previous[3]))) != row[6])
                throw new InvalidOperationException("The wider load changed a later raw byte.");
        }
    }

    [Fact]
    public void TailsUnalignedRowsFirstRowsAndGuardBytesMatchScalarReconstruction()
    {
        foreach (var length in new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12, 13, 17, 65531, 65532, 65533, 65534, 65535, 65536, 65537, 65540, 131069, 131070, 131071 })
        foreach (var firstRow in new[] { false, true })
        {
            var row = Enumerable.Range(0, length + 16).Select(i => (byte)(i * 79 + (i >> 4) * 47)).ToArray();
            var expected = row.ToArray();
            var previous = Enumerable.Range(0, length + 2).Select(i => firstRow ? (byte)0 : (byte)(i * 137 + 23)).ToArray();
            var unchanged = previous.ToArray();
            Reconstruct(expected.AsSpan(7, length), previous.AsSpan(1, length));
            PngCodec.Unfilter(row.AsSpan(7, length), previous.AsSpan(1, length), 3, 4);
            Assert.Equal(expected, row); Assert.Equal(unchanged, previous);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(7)]
    public void TruncatedPreviousRowKeepsScalarPartialUpdateAndException(int previousLength)
    {
        var row = Enumerable.Range(0, 24).Select(i => (byte)(i * 79 + 17)).ToArray(); var expected = row.ToArray();
        var previous = Enumerable.Range(0, previousLength).Select(i => (byte)(i * 137 + 23)).ToArray();
        Assert.Throws<IndexOutOfRangeException>(() => Reconstruct(expected, previous));
        Assert.Throws<IndexOutOfRangeException>(() => PngCodec.Unfilter(row, previous, 3, 4));
        Assert.Equal(expected, row);
    }

    [Theory]
    [InlineData(-3)] [InlineData(-1)] [InlineData(0)] [InlineData(1)] [InlineData(3)]
    public void OverlappingRowsKeepScalarReadWriteOrder(int relativeOffset)
    {
        var bytes = Enumerable.Range(0, 80).Select(i => (byte)(i * 79 + 17)).ToArray(); var expected = bytes.ToArray();
        Reconstruct(expected.AsSpan(24, 32), expected.AsSpan(24 + relativeOffset, 32));
        PngCodec.Unfilter(bytes.AsSpan(24, 32), bytes.AsSpan(24 + relativeOffset, 32), 3, 4);
        Assert.Equal(expected, bytes);
    }

    [Fact]
    public void PreCancellationKeepsNonemptyRowUntouchedAndEmptyRowBehavior()
    {
        var row = Enumerable.Range(0, 65537).Select(i => (byte)i).ToArray(); var unchanged = row.ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => PngCodec.Unfilter(row, new byte[row.Length], 3, 4, cancellation.Token));
        Assert.Equal(unchanged, row);
        PngCodec.Unfilter([], [], 3, 4, cancellation.Token);
    }

    private static int Predict(int left, int up, int upperLeft)
    {
        var p = left + up - upperLeft;
        var a = Math.Abs(p - left); var b = Math.Abs(p - up); var c = Math.Abs(p - upperLeft);
        return a <= b && a <= c ? left : b <= c ? up : upperLeft;
    }

    private static void Reconstruct(Span<byte> row, ReadOnlySpan<byte> previous)
    {
        for (var i = 0; i < row.Length; i++)
            row[i] = unchecked((byte)(row[i] + Predict(i >= 3 ? row[i - 3] : 0,
                previous[i], i >= 3 ? previous[i - 3] : 0)));
    }
}
