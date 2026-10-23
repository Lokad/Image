namespace Lokad.Image.Internal;

// Encoder-selected forward arithmetic, independently expressed as separable
// integer matrix projections. RFC6386 section14 specifies the inverse/geometry;
// forward rounding is pinned separately by native coefficient observations.
// This private preparation is not yet the public lossy encoder's transform.
internal static class Vp8ForwardTransform
{
    private static readonly int Sine = (int)Math.Round(4096 * Math.Sqrt(2) * Math.Sin(Math.PI / 8));
    private static readonly int Cosine = (int)Math.Round(4096 * Math.Sqrt(2) * Math.Cos(Math.PI / 8));

    private static int Weight(int frequency, int position) => frequency switch
    {
        0 => 1,
        2 => position is 0 or 3 ? 1 : -1,
        1 => position switch { 0 => Cosine, 1 => Sine, 2 => -Sine, _ => -Cosine },
        _ => position switch { 0 => Sine, 1 => -Cosine, 2 => Cosine, _ => -Sine }
    };

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
        for (var frequency = 0; frequency < 4; frequency++)
        {
            var projection = 0;
            for (var x = 0; x < 4; x++) projection += Weight(frequency, x) * residual[y * 4 + x];
            rows[y * 4 + frequency] = (frequency & 1) == 0 ? projection * 8
                : (projection + (frequency == 1 ? 1812 : 937)) >> 9;
        }
        for (var v = 0; v < 4; v++)
        for (var u = 0; u < 4; u++)
        {
            var projection = 0;
            for (var y = 0; y < 4; y++) projection += Weight(v, y) * rows[y * 4 + u];
            var value = (v & 1) == 0 ? (projection + 7) >> 4
                : (projection + (v == 1 ? 12000 : 51000)) >> 16;
            if (v == 1 && rows[u] != rows[12 + u]) value++;
            coefficients[v * 4 + u] = checked((short)value);
        }
    }
}
