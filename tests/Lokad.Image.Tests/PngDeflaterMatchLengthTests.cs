using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngDeflaterMatchLengthTests
{
    [Fact]
    public void EarliestMismatchAndExactLookaheadHoldAtContiguousHistoryEdgesAndOverlap()
    {
        const int capacity = 65536;
        (int Prior, int Current)[] layouts =
        [
            (0, 10007),
            (32768 - 29, 32768 + 5001),
            (50001, capacity - 258),
            (capacity - 262, capacity - 258),
            (0, 4)
        ];
        var history = new byte[capacity]; Array.Fill(history, (byte)0xa5);
        foreach (var (prior, current) in layouts)
        {
            for (var mismatch = 8; mismatch <= 258; mismatch++)
            {
                var index = current + mismatch; var bit = (byte)(1 << (mismatch & 7));
                if (mismatch < 258) history[index] ^= bit;
                var expected = 8;
                while (expected < 258 && history[prior + expected] == history[current + expected]) expected++;
                Assert.Equal(expected, PngDeflater.MatchLength(history, prior, current, 8, 258));
                // The changed byte just beyond the requested limit is invisible.
                Assert.Equal(mismatch, PngDeflater.MatchLength(history, prior, current, 8, mismatch));
                if (mismatch < 258) history[index] ^= bit;
            }
            Assert.All(history, b => Assert.Equal(0xa5, b));
        }
    }
}
