using System.Numerics;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class LanczosRoundingTests
{
    private static void Check(Int128 numerator, Int128 denominator, int maximum)
    {
        var n = (BigInteger)numerator; var d = (BigInteger)denominator;
        var expected = n <= 0 ? 0 : (int)BigInteger.Min(maximum, (2 * n + d) / (2 * d));
        Assert.Equal(expected, LanczosResize.Round(numerator, denominator, maximum));
    }

    [Theory]
    [InlineData(255)] [InlineData(65535)]
    public void ExactIntegerAndHalfBoundariesRetainLowBitsBeyondBinary64(int maximum)
    {
        foreach (var bits in new[] { 0, 1, 2, 8, 30, 52, 60, 78, 110 })
        foreach (var delta in new[] { -1, 0, 1 })
        {
            var denominator = ((Int128)1 << bits) + delta; if (denominator <= 0) continue;
            foreach (var quotient in new[] { 0, 1, maximum / 3, maximum / 2, maximum - 1, maximum })
            foreach (var offset in new Int128[] { -1, 0, 1, denominator / 2 - 1, denominator / 2, denominator / 2 + 1, denominator - 1 })
                Check(quotient * denominator + offset, denominator, maximum);
        }
    }

    [Theory]
    [InlineData(255)] [InlineData(65535)]
    public void DeterministicWideFractionsMatchIndependentBigIntegerRounding(int maximum)
    {
        ulong state = 0x9B12D4EC5367A809;
        ulong Next() => state = unchecked(state * 6364136223846793005 + 1442695040888963407);
        for (var i = 0; i < 4096; i++)
        {
            var bits = 1 + i % 110; // Products with either sample maximum fit Int128.
            var denominator = (Int128)((((UInt128)Next() << 64) | Next()) & (((UInt128)1 << bits) - 1)) + 1;
            var quotient = (int)(Next() % (ulong)maximum);
            var numerator = denominator * quotient + (Int128)((((UInt128)Next() << 64) | Next()) % (UInt128)denominator);
            Check(numerator, denominator, maximum);
        }
    }

    [Fact]
    public void WiderArithmeticRemainsExactWhenTheProjectionIsZeroOrMisleading()
    {
        foreach (var maximum in new[] { 255, 65535 })
        {
            var denominator = ((Int128)1 << 100) + ((Int128)1 << 32);
            Check(7 * denominator + ((Int128)1 << 95), denominator, maximum);
            denominator = (Int128)1 << 100;
            Check(7 * denominator + denominator / 2 - 1, denominator, maximum);
            Check(7 * denominator + denominator / 2, denominator, maximum);
            Check(7 * denominator + denominator / 2 + 1, denominator, maximum);
        }
    }
}
