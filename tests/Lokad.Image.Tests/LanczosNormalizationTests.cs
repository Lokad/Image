using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class LanczosNormalizationTests
{
    private static readonly Int128 One = (Int128)1 << 60;
    private static void Check(Int128[] values, int maximum, long lowAdjustment = 0)
    {
        var sums = new Int128[4];
        if (Avx2.IsSupported)
        {
            var limbs = MemoryMarshal.Cast<Int128, long>(sums.AsSpan());
            for (var i = 0; i < 4; i++)
            {
                var low = unchecked((int)values[i]) + (i % 2 == 0 ? lowAdjustment : -lowAdjustment);
                limbs[i] = low; limbs[4 + i] = (long)((values[i] - low) >> 32);
                Assert.True(BigInteger.Abs(limbs[i]) <= (BigInteger)1 << 62);
                Assert.True(BigInteger.Abs(limbs[4 + i]) <= (BigInteger)1 << (i == 3 ? 46 : 62));
            }
        }
        else values.CopyTo(sums, 0);
        static int Round(BigInteger n, BigInteger d, int m) => n <= 0 ? 0 : (int)BigInteger.Min(m, (2 * n + d) / (2 * d));
        var alpha = Round((BigInteger)values[3], (BigInteger)One, maximum);
        var pixel = LanczosResize.Normalize(sums, 0, maximum);
        for (var i = 0; i < 4; i++)
            Assert.Equal(i == 3 ? alpha : alpha == 0 ? 0 : Round((BigInteger)values[i], (BigInteger)values[3], maximum), pixel.GetElement(i));
    }

    [Theory]
    [InlineData(255)] [InlineData(65535)]
    public void HalvesClippingAndAlphaThresholdKeepExactPixels(int maximum)
    {
        foreach (var d in new Int128[] { -1, 0, One / 2 - 1, One / 2, One / 2 + 1,
            One - 1, One, One + 1, maximum * One - 1, maximum * One, maximum * One + 1 })
        foreach (var q in new[] { -1, 0, 1, maximum / 2, maximum - 1, maximum, maximum + 1 })
        foreach (var delta in new[] { -1, 0, 1 })
            Check([q * d + d / 2 + delta, q * d - 1, maximum * d + 1, d], maximum);
    }

    [Theory]
    [InlineData(255)] [InlineData(65535)]
    public void LargeOpposingLimbsAndDiscardedBitsKeepExactPixels(int maximum)
    {
        foreach (var d in new Int128[] { One / 2 - 1, One / 2, One / 2 + 1,
            7 * One + One / 2 - 1, 7 * One + One / 2, 7 * One + One / 2 + 1, maximum * One + 123457 })
        foreach (var adjustment in new[] { -(1L << 62) + (1L << 32), -(1L << 53), 0, 1L << 53, (1L << 62) - (1L << 32) })
        foreach (var delta in new[] { -1, 0, 1 })
            Check([(maximum / 3) * d + d / 2 + delta, maximum * d + delta, d / 3, d], maximum, adjustment);
    }

    [Fact]
    public void DeterministicInteriorAndOvershootMatchIndependentRationals()
    {
        ulong state = 0x85B16E07A4C29D3F;
        ulong Next() => state = unchecked(state * 6364136223846793005 + 1442695040888963407);
        foreach (var maximum in new[] { 255, 65535 })
        for (var trial = 0; trial < 2048; trial++)
        {
            var d = (Int128)(Next() % (ulong)(maximum + 2)) * One + (Next() & ((1UL << 60) - 1));
            var values = new Int128[4]; values[3] = d;
            for (var i = 0; i < 3; i++)
            {
                values[i] = (Int128)(Next() % (ulong)(2 * maximum + 1)) * d + d * (uint)Next() / ((Int128)1 << 32);
                if ((Next() & 15) == 0) values[i] = -values[i];
            }
            Check(values, maximum, trial % 2 == 0 ? 1L << 61 : -(1L << 61));
        }
    }
}
