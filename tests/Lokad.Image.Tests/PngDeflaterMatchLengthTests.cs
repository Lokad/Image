using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngDeflaterMatchLengthTests
{
    [Fact]
    public void EarliestMismatchAndExactLookaheadHoldAcrossBothRingEdgesAndOverlap()
    {
        const int ring = 65536, mask = ring - 1;
        (long Prior, long Current)[] layouts =
        [
            (3L * ring + 101, 3L * ring + 10007),
            (3L * ring + ring - 29, 4L * ring + 5001),
            (3L * ring + 50001, 3L * ring + ring - 17),
            (3L * ring + ring - 23, 3L * ring + ring - 19)
        ];
        var history = new byte[ring]; Array.Fill(history, (byte)0xa5);
        foreach (var (prior, current) in layouts)
        {
            for (var mismatch = 8; mismatch <= 258; mismatch++)
            {
                var index = (int)((current + mismatch) & mask); var bit = (byte)(1 << (mismatch & 7));
                if (mismatch < 258) history[index] ^= bit;
                var expected = 8;
                while (expected < 258 && history[(int)((prior + expected) & mask)] == history[(int)((current + expected) & mask)]) expected++;
                Assert.Equal(expected, PngDeflater.MatchLength(history, prior, current, 8, 258));
                // The changed byte just beyond the requested limit is invisible.
                Assert.Equal(mismatch, PngDeflater.MatchLength(history, prior, current, 8, mismatch));
                if (mismatch < 258) history[index] ^= bit;
            }
            Assert.All(history, b => Assert.Equal(0xa5, b));
        }
    }
}
