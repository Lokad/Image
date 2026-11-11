using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class BilinearVectorPublicationTests
{
    [Fact]
    public void NormalizationUsesEachPixelsAlphaAndTheExactAreaDenominator()
    {
        if (!Avx512F.IsSupported || !Avx512DQ.IsSupported) return;
        var random = new Random(804);
        Span<double> sums = stackalloc double[8];
        foreach (var denominator in new[] { 4d, 84, 2_132_052, 1_073_741_820 })
        for (var trial = 0; trial < 128; trial++)
        {
            for (var pixel = 0; pixel < 2; pixel++)
            {
                var alpha = trial % 13 == 0 ? 0 : random.NextInt64(1, (long)(65535 * denominator));
                var at = pixel * 4;
                sums[at] = alpha == 0 ? 0 : random.NextInt64(alpha * 65535);
                sums[at + 1] = alpha == 0 ? 0 : random.NextInt64(alpha * 65535);
                sums[at + 2] = alpha == 0 ? 0 : random.NextInt64(alpha * 65535);
                sums[at + 3] = alpha;
            }
            var actual = BilinearUpsample16.Normalize(Vector512.Create(sums), denominator);
            for (var lane = 0; lane < 8; lane++)
            {
                var expected = sums[lane] / (lane % 4 == 3 ? denominator : sums[(lane / 4) * 4 + 3]);
                if (double.IsNaN(expected)) Assert.True(double.IsNaN(actual.GetElement(lane)));
                else Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual.GetElement(lane)));
            }
        }
    }

    [Fact]
    public void RepairBitsMatchScalarCertificatesAtTinyAlphaAndRoundingBoundaries()
    {
        if (!Avx512F.IsSupported || !Avx512DQ.IsSupported) return;
        Span<double> lanes = stackalloc double[8];
        foreach (var extent in new[] { 2, 514, 32768, 1_000_000 })
        {
            var error = extent * Math.ScaleB(1d, -48);
            var alphaError = 16 * 65535 * error;
            var colorError = 16 * 65535d * 65535 * error;
            var alphas = new[] { 0d, double.Epsilon, Math.BitDecrement(1 + alphaError), 1 + alphaError,
                Math.BitIncrement(1 + alphaError), 2, 32767.5, 65535, double.NaN };
            var colors = new[] { 0d, .5, Math.BitDecrement(.5), Math.BitIncrement(.5), 1.5,
                32767.5, Math.BitDecrement(32767.5), Math.BitIncrement(32767.5), 65535, double.NaN };
            foreach (var alpha0 in alphas)
            foreach (var alpha1 in alphas)
            for (var sample = 0; sample < colors.Length; sample++)
            {
                for (var lane = 0; lane < 8; lane++) lanes[lane] = colors[(sample + lane) % colors.Length];
                lanes[3] = alpha0; lanes[7] = alpha1;
                var expected = 0;
                for (var lane = 0; lane < 8; lane++)
                {
                    var alpha = lanes[(lane / 4) * 4 + 3];
                    var exponent = (BitConverter.DoubleToUInt64Bits(alpha - alphaError) >> 52) & 2047;
                    var reciprocal = BitConverter.UInt64BitsToDouble(unchecked((2046UL - exponent) << 52));
                    var margin = lane % 4 == 3 ? alphaError : colorError * reciprocal;
                    var fraction = lanes[lane] - Math.Floor(lanes[lane]);
                    if (Math.Abs(fraction - .5) <= margin || alpha <= 1 + alphaError) expected |= 1 << lane;
                }
                Assert.Equal(expected, BilinearUpsample16.Classify(Vector512.Create(lanes), colorError, alphaError));
            }
        }
    }
}
