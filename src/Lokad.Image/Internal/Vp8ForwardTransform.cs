namespace Lokad.Image.Internal;

// Encoder-selected forward arithmetic, independently factored from separable
// integer matrix projections. RFC6386 section14 specifies the inverse/geometry;
// forward rounding is pinned separately by native coefficient observations.
// Used by the public lossy encoder's prediction and reconstruction search.
internal static class Vp8ForwardTransform
{
    private static readonly int Sine = (int)Math.Round(4096 * Math.Sqrt(2) * Math.Sin(Math.PI / 8));
    private static readonly int Cosine = (int)Math.Round(4096 * Math.Sqrt(2) * Math.Cos(Math.PI / 8));

    // Only sixteen9-bit residuals are consumed, then sixteen coefficients are
    // written. Input/output may overlap; trailing caller samples stay untouched.
    // The bounded64-byte stack scratch has no owner/cache/heap lifetime. The
    // encoder admits work and checks cancellation at its block boundary.
    internal static void Dct(ReadOnlySpan<short> residual, Span<short> coefficients)
    {
        if (residual.Length < 16 || coefficients.Length < 16) throw new ArgumentException("A4x4 transform requires16samples.");
        for (var p = 0; p < 16; p++)
            if (residual[p] is < -255 or > 255) throw new ArgumentOutOfRangeException(nameof(residual), "VP8 prediction residuals must fit9bits.");
        Span<int> rows = stackalloc int[16];
        for (var y = 0; y < 4; y++)
        {
            var at = y * 4;
            var outerSum = residual[at] + residual[at + 3];
            var innerSum = residual[at + 1] + residual[at + 2];
            var outerDifference = residual[at] - residual[at + 3];
            var innerDifference = residual[at + 1] - residual[at + 2];
            rows[at] = (outerSum + innerSum) * 8;
            rows[at + 2] = (outerSum - innerSum) * 8;
            rows[at + 1] = (Cosine * outerDifference + Sine * innerDifference + 1812) >> 9;
            rows[at + 3] = (Sine * outerDifference - Cosine * innerDifference + 937) >> 9;
        }
        for (var x = 0; x < 4; x++)
        {
            var outerSum = rows[x] + rows[12 + x]; var innerSum = rows[4 + x] + rows[8 + x];
            var outerDifference = rows[x] - rows[12 + x]; var innerDifference = rows[4 + x] - rows[8 + x];
            coefficients[x] = checked((short)((outerSum + innerSum + 7) >> 4));
            coefficients[8 + x] = checked((short)((outerSum - innerSum + 7) >> 4));
            var odd = (Cosine * outerDifference + Sine * innerDifference + 12000) >> 16;
            coefficients[4 + x] = checked((short)(odd + (outerDifference != 0 ? 1 : 0)));
            coefficients[12 + x] = checked((short)((Sine * outerDifference - Cosine * innerDifference + 51000) >> 16));
        }
    }

    // Sequency-ordered4-point Hadamard basis: reverse the two Gray-code bits,
    // then take the parity of their intersection with the sample position.
    private static int WalshWeight(int frequency, int position)
    {
        var gray = frequency ^ (frequency >> 1); var mask = (gray >> 1) | ((gray & 1) << 1);
        var bits = mask & position; return ((bits ^ (bits >> 1)) & 1) == 0 ? 1 : -1;
    }

    // Sixteen unquantized lumaDCs in block-row order, with the native signed
    // half-scale after both projections. Uses the same borrowing/stack contract
    // as Dct; no per-block owner or budget allowance is introduced.
    internal static void WalshDc(ReadOnlySpan<short> dc, Span<short> coefficients)
    {
        if (dc.Length < 16 || coefficients.Length < 16) throw new ArgumentException("Y2Walsh requires16DCsamples.");
        for (var p = 0; p < 16; p++)
            if (dc[p] is < -2040 or > 2040) throw new ArgumentOutOfRangeException(nameof(dc), "Unquantized8bit lumaDC exceeds its range.");
        Span<int> rows = stackalloc int[16];
        for (var y = 0; y < 4; y++)
        for (var u = 0; u < 4; u++)
        {
            var sum = 0;
            for (var x = 0; x < 4; x++) sum += WalshWeight(u, x) * dc[y * 4 + x];
            rows[y * 4 + u] = sum;
        }
        for (var v = 0; v < 4; v++)
        for (var u = 0; u < 4; u++)
        {
            var sum = 0;
            for (var y = 0; y < 4; y++) sum += WalshWeight(v, y) * rows[y * 4 + u];
            coefficients[v * 4 + u] = checked((short)(sum >> 1));
        }
    }
}
