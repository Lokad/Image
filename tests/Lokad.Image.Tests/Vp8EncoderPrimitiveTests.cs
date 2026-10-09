using System.Numerics;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8EncoderPrimitiveTests
{
    [Fact]
    public void ArithmeticWriterMatchesAnIndependentUnboundedIntegerInterval()
    {
        // This test keeps the entire interval endpoint instead of carrying through emitted bytes.
        // All 256 probabilities, partial-byte carries and long sequences are covered.
        var random = new Random(173); var low = BigInteger.Zero; var range = 255; var shifts = 0;
        using var writer = new Vp8BooleanWriter(new OperationContext(ImageLimits.Default, default), 1_000_000);
        var samples = new List<(int Bit, int Probability)>();
        for (var i = 0; i < 4096; i++)
        {
            var probability = i % 256; var bit = random.Next(2); samples.Add((bit, probability));
            writer.Write(bit, probability);
            var split = 1 + ((range - 1) * probability >> 8);
            if (bit == 0) range = split; else { low += split; range -= split; }
            while (range < 128) { range *= 2; low *= 2; shifts++; }
        }
        writer.Finish(); var length = (shifts + 8 + 7) / 8;
        low <<= length * 8 - shifts - 8;
        var expected = new byte[length + 2]; var significant = low.ToByteArray(isUnsigned: true, isBigEndian: true);
        significant.CopyTo(expected.AsSpan(length - significant.Length));
        Assert.Equal(expected, writer.Bytes.ToArray());
        var data = writer.Bytes.ToArray(); var reader = new Vp8BooleanReader(data, 0, data.Length);
        var context = new OperationContext(ImageLimits.Default, default);
        foreach (var sample in samples) Assert.Equal(sample.Bit, reader.Read(data, sample.Probability, context));
    }

    [Fact]
    public void ArithmeticOutputIsBoundedAndFinalizationIsSingleUse()
    {
        var context = new OperationContext(ImageLimits.Default, default);
        using var limited = new Vp8BooleanWriter(context, 1);
        Assert.Throws<ImageLimitException>(() => limited.Finish());
        using var valid = new Vp8BooleanWriter(context, 64); valid.Finish();
        Assert.Equal(new byte[] { 0, 0, 0 }, valid.Bytes.ToArray());
        Assert.Throws<InvalidOperationException>(() => valid.Finish());
        Assert.Throws<InvalidOperationException>(() => valid.Write(0));
        using var work = new Vp8BooleanWriter(new OperationContext(new ImageLimits { MaxWork = 1 }, default), 64);
        work.Write(0); Assert.Throws<ImageLimitException>(() => work.Write(0));
        using var memory = new Vp8BooleanWriter(new OperationContext(new ImageLimits { MaxWorkingBytes = 32 }, default), 64);
        Assert.Throws<ImageLimitException>(() => memory.Finish());
    }

    [Theory]
    [InlineData(1, 4, 2)] [InlineData(-1, 4, -2)] [InlineData(10, 8, 10)] [InlineData(-10, 8, -10)]
    [InlineData(1, 16, 1)] [InlineData(-1, 16, -1)]
    public void ConstantResidualHasOnlyTheAnalyticalDcCoefficient(short sample, int divisor, short dc)
    {
        var residual = Enumerable.Repeat(sample, 16).ToArray(); var coefficients = new short[16];
        Vp8ForwardDct.Quantize(residual, coefficients, divisor, divisor);
        Assert.Equal(dc, coefficients[0]); Assert.All(coefficients.Skip(1), value => Assert.Equal(0, value));
    }
}
