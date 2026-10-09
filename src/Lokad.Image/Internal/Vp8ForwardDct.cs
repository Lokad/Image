namespace Lokad.Image.Internal;

/// <summary>Original separable mathematical DCT, scaled for the VP8 inverse transform.</summary>
internal static class Vp8ForwardDct
{
    private static readonly double[] Basis = CreateBasis();

    private static double[] CreateBasis()
    {
        var result = new double[16];
        for (var frequency = 0; frequency < 4; frequency++)
            for (var position = 0; position < 4; position++)
                // Scale the whole basis by sqrt(2), keeping the DC basis exactly one.
                // Dividing the two-dimensional result by two preserves exact DC half ties.
                result[frequency * 4 + position] = frequency == 0 ? 1
                    : Math.Sqrt(2) * Math.Cos((2 * position + 1) * frequency * Math.PI / 8);
        return result;
    }

    public static void Quantize(ReadOnlySpan<short> residual, Span<short> coefficients, int dc, int ac)
    {
        Span<double> horizontal = stackalloc double[16];
        for (var y = 0; y < 4; y++)
            for (var u = 0; u < 4; u++)
            {
                var sum = 0.0;
                for (var x = 0; x < 4; x++) sum += residual[y * 4 + x] * Basis[u * 4 + x];
                horizontal[y * 4 + u] = sum;
            }
        for (var v = 0; v < 4; v++)
            for (var u = 0; u < 4; u++)
            {
                var sum = 0.0;
                for (var y = 0; y < 4; y++) sum += horizontal[y * 4 + u] * Basis[v * 4 + y];
                coefficients[v * 4 + u] = checked((short)Math.Round(sum / (2 * (u + v == 0 ? dc : ac)), MidpointRounding.AwayFromZero));
            }
    }
}
