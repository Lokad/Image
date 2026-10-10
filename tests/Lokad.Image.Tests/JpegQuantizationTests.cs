using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegQuantizationTests
{
    [Fact]
    public void EveryEncoderQuantizerKeepsBothSignsOfEveryHalfTieNeighborhood()
    {
        // Original scalar policy at all half-integer quotient boundaries within
        // the conservative [-2048,2048] transform range. Input neighbors matter:
        // dividing by an odd quantizer can itself round onto a quotient half tie.
        var coefficients = new double[64]; var quantizers = new ushort[64];
        var actual = new int[64]; var expected = new int[64];
        for (var divisor = 1; divisor <= 255; divisor++)
        {
            Array.Fill(quantizers, (ushort)divisor);
            var values = new List<double> { -0.0, 0.0, -double.Epsilon, double.Epsilon, -2048, 2048 };
            for (var integer = -2048 / divisor - 1; integer <= 2048 / divisor; integer++)
            {
                var half = (integer + 0.5) * divisor;
                if (Math.Abs(half) <= 2048)
                    values.AddRange([Math.BitDecrement(half), half, Math.BitIncrement(half)]);
            }
            for (var start = 0; start < values.Count; start += 64)
            {
                Array.Clear(coefficients);
                for (var lane = 0; lane < Math.Min(64, values.Count - start); lane++) coefficients[lane] = values[start + lane];
                for (var lane = 0; lane < 64; lane++)
                    expected[lane] = (int)Math.Round(coefficients[lane] / divisor, MidpointRounding.AwayFromZero);
                JpegEncoder.Quantize(coefficients, quantizers, actual);
                Assert.Equal(expected, actual);
            }
        }
    }

    [Fact]
    public void MixedQuantizersKeepLaneOrderInputsAndOutputGuards()
    {
        ushort[] choices = [1, 2, 3, 7, 17, 31, 127, 255];
        for (var phase = 0; phase < 8; phase++)
        {
            var coefficients = Enumerable.Range(0, 64).Select(i => (i * 7919 % 4097 - 2048) + (i & 3) * 0.25).ToArray();
            var quantizers = Enumerable.Range(0, 64).Select(i => choices[(i + phase) & 7]).ToArray();
            var originalCoefficients = coefficients.ToArray(); var originalQuantizers = quantizers.ToArray();
            var expected = Enumerable.Range(0, 64).Select(i => (int)Math.Round(coefficients[i] / quantizers[i], MidpointRounding.AwayFromZero)).ToArray();
            var guarded = Enumerable.Repeat(12345, 66).ToArray();
            JpegEncoder.Quantize(coefficients, quantizers, guarded.AsSpan(1, 64));
            Assert.Equal(expected, guarded.AsSpan(1, 64).ToArray());
            Assert.Equal(12345, guarded[0]); Assert.Equal(12345, guarded[^1]);
            Assert.Equal(originalCoefficients, coefficients); Assert.Equal(originalQuantizers, quantizers);
        }
    }

    [Theory]
    [InlineData(63, 64, 64)] [InlineData(65, 64, 64)] [InlineData(64, 63, 64)]
    [InlineData(64, 65, 64)] [InlineData(64, 64, 63)] [InlineData(64, 64, 65)]
    public void IncompleteBlocksRejectBeforeWriting(int coefficients, int quantizers, int result)
    {
        var output = Enumerable.Repeat(12345, result).ToArray();
        Assert.Throws<ArgumentException>(() => JpegEncoder.Quantize(new double[coefficients], new ushort[quantizers], output));
        Assert.All(output, value => Assert.Equal(12345, value));
    }
}
