using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8ForwardDctVectorTests
{
    [Fact]
    public void QuantizedCoefficientsMatchTheOriginalScalarOrderAtEveryFormatQuantizer()
    {
        var basis = new double[16];
        for (var f = 0; f < 4; f++)
        for (var x = 0; x < 4; x++)
            basis[f * 4 + x] = f == 0 ? 1 : Math.Sqrt(2) * Math.Cos((2 * x + 1) * f * Math.PI / 8);
        var random = new Random(19039); var residual = new short[16];
        var expected = new short[16]; var actual = new short[18];
        for (var q = 0; q < 128; q++)
        for (var vector = 0; vector < 64; vector++)
        {
            for (var i = 0; i < 16; i++)
                residual[i] = vector switch
                {
                    0 => 0, 1 => 255, 2 => -255, 3 => 1, 4 => -1,
                    < 21 => (short)(i == vector - 5 ? 255 : 0),
                    < 37 => (short)(i == vector - 21 ? -255 : 0),
                    37 => (short)((i & 1) == 0 ? 255 : -255),
                    38 => (short)((i / 4 & 1) == 0 ? 255 : -255),
                    _ => (short)random.Next(-255, 256)
                };
            var dc = (int)Vp8Tables.DcQuantizers[q]; var ac = (int)Vp8Tables.AcQuantizers[q];
            Reference(residual, expected, dc, ac, basis);
            Array.Fill(actual, (short)12345);
            Vp8ForwardDct.Quantize(residual, actual.AsSpan(1, 16), dc, ac);
            Assert.Equal(expected, actual.AsSpan(1, 16).ToArray());
            Assert.Equal(12345, actual[0]); Assert.Equal(12345, actual[17]);
        }
    }

    // Preserve the original accumulation order as an independent scalar oracle.
    private static void Reference(short[] input, short[] output, int dc, int ac, double[] basis)
    {
        Span<double> horizontal = stackalloc double[16];
        for (var y = 0; y < 4; y++)
        for (var u = 0; u < 4; u++)
        {
            var sum = 0.0;
            for (var x = 0; x < 4; x++) sum += input[y * 4 + x] * basis[u * 4 + x];
            horizontal[y * 4 + u] = sum;
        }
        for (var v = 0; v < 4; v++)
        for (var u = 0; u < 4; u++)
        {
            var sum = 0.0;
            for (var y = 0; y < 4; y++) sum += horizontal[y * 4 + u] * basis[v * 4 + y];
            output[v * 4 + u] = checked((short)Math.Round(sum / (2 * (u + v == 0 ? dc : ac)), MidpointRounding.AwayFromZero));
        }
    }
}
