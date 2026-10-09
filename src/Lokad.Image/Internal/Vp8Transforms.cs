namespace Lokad.Image.Internal;

/// <summary>Owned separable integer transforms with RFC 6386 sections 14.3/14.4 rounding.</summary>
internal static class Vp8Transforms
{
    public static void InverseDct(Span<short> block)
    {
        Span<short> scratch = stackalloc short[16];
        for (var x = 0; x < 4; x++)
        {
            DctLine(block[x], block[4 + x], block[8 + x], block[12 + x], out var a, out var b, out var c, out var d);
            scratch[x] = unchecked((short)a); scratch[4 + x] = unchecked((short)b);
            scratch[8 + x] = unchecked((short)c); scratch[12 + x] = unchecked((short)d);
        }
        for (var y = 0; y < 4; y++)
        {
            var start = y * 4;
            DctLine(scratch[start], scratch[start + 1], scratch[start + 2], scratch[start + 3], out var a, out var b, out var c, out var d);
            block[start] = unchecked((short)((a + 4) >> 3)); block[start + 1] = unchecked((short)((b + 4) >> 3));
            block[start + 2] = unchecked((short)((c + 4) >> 3)); block[start + 3] = unchecked((short)((d + 4) >> 3));
        }
    }

    private static void DctLine(int zero, int first, int second, int third, out int a, out int b, out int c, out int d)
    {
        // Round(2^16 * sqrt(2)*sin(pi/8)) = 35468;
        // Round(2^16 * (sqrt(2)*cos(pi/8)-1)) = 20091, per the specified approximation.
        var evenSum = zero + second; var evenDifference = zero - second;
        var oddDifference = (first * 35468 >> 16) - third - (third * 20091 >> 16);
        var oddSum = first + (first * 20091 >> 16) + (third * 35468 >> 16);
        a = evenSum + oddSum; b = evenDifference + oddDifference;
        c = evenDifference - oddDifference; d = evenSum - oddSum;
    }

    public static void InverseWalsh(Span<short> block)
    {
        Span<short> scratch = stackalloc short[16];
        for (var x = 0; x < 4; x++)
        {
            WalshLine(block[x], block[4 + x], block[8 + x], block[12 + x], out var a, out var b, out var c, out var d);
            scratch[x] = unchecked((short)a); scratch[4 + x] = unchecked((short)b);
            scratch[8 + x] = unchecked((short)c); scratch[12 + x] = unchecked((short)d);
        }
        for (var y = 0; y < 4; y++)
        {
            var start = y * 4;
            WalshLine(scratch[start], scratch[start + 1], scratch[start + 2], scratch[start + 3], out var a, out var b, out var c, out var d);
            block[start] = unchecked((short)((a + 3) >> 3)); block[start + 1] = unchecked((short)((b + 3) >> 3));
            block[start + 2] = unchecked((short)((c + 3) >> 3)); block[start + 3] = unchecked((short)((d + 3) >> 3));
        }
    }

    private static void WalshLine(int zero, int first, int second, int third, out int a, out int b, out int c, out int d)
    {
        var outerSum = zero + third; var innerSum = first + second;
        var outerDifference = zero - third; var innerDifference = first - second;
        a = outerSum + innerSum; b = outerDifference + innerDifference;
        c = outerSum - innerSum; d = outerDifference - innerDifference;
    }
}
