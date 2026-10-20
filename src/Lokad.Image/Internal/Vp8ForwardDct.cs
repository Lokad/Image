using System.Runtime.Intrinsics;

namespace Lokad.Image.Internal;

/// <summary>Original separable mathematical DCT, scaled for the VP8 inverse transform.</summary>
internal static class Vp8ForwardDct
{
    private static readonly double[] Basis = CreateBasis();
    private static readonly Vector256<double> Column0 = Vector256.Create(Basis[0], Basis[4], Basis[8], Basis[12]);
    private static readonly Vector256<double> Column1 = Vector256.Create(Basis[1], Basis[5], Basis[9], Basis[13]);
    private static readonly Vector256<double> Column2 = Vector256.Create(Basis[2], Basis[6], Basis[10], Basis[14]);
    private static readonly Vector256<double> Column3 = Vector256.Create(Basis[3], Basis[7], Basis[11], Basis[15]);

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
        if (Vector256.IsHardwareAccelerated)
        {
            QuantizeVector(residual, coefficients, dc, ac);
            return;
        }
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

    // Four independent frequencies share instructions, while every lane keeps
    // the scalar multiplication/addition order. Do not fuse or reassociate:
    // even a last-bit change can cross a quantization midpoint.
    private static void QuantizeVector(ReadOnlySpan<short> residual, Span<short> coefficients, int dc, int ac)
    {
        Span<double> horizontal = stackalloc double[16];
        for (var y = 0; y < 4; y++)
        {
            var sum = Vector256<double>.Zero;
            sum = Vector256.Add(sum, Vector256.Multiply(Vector256.Create((double)residual[y * 4]), Column0));
            sum = Vector256.Add(sum, Vector256.Multiply(Vector256.Create((double)residual[y * 4 + 1]), Column1));
            sum = Vector256.Add(sum, Vector256.Multiply(Vector256.Create((double)residual[y * 4 + 2]), Column2));
            sum = Vector256.Add(sum, Vector256.Multiply(Vector256.Create((double)residual[y * 4 + 3]), Column3));
            sum.CopyTo(horizontal.Slice(y * 4, 4));
        }
        var row0 = Vector256.Create<double>(horizontal[..4]); var row1 = Vector256.Create<double>(horizontal.Slice(4, 4));
        var row2 = Vector256.Create<double>(horizontal.Slice(8, 4)); var row3 = Vector256.Create<double>(horizontal.Slice(12, 4));
        Span<double> values = stackalloc double[4];
        for (var v = 0; v < 4; v++)
        {
            var sum = Vector256<double>.Zero;
            sum = Vector256.Add(sum, Vector256.Multiply(row0, Vector256.Create(Basis[v * 4])));
            sum = Vector256.Add(sum, Vector256.Multiply(row1, Vector256.Create(Basis[v * 4 + 1])));
            sum = Vector256.Add(sum, Vector256.Multiply(row2, Vector256.Create(Basis[v * 4 + 2])));
            sum = Vector256.Add(sum, Vector256.Multiply(row3, Vector256.Create(Basis[v * 4 + 3])));
            sum.CopyTo(values);
            for (var u = 0; u < 4; u++)
                coefficients[v * 4 + u] = checked((short)Math.Round(values[u] / (2 * (u + v == 0 ? dc : ac)), MidpointRounding.AwayFromZero));
        }
    }
}
