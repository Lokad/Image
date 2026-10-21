using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegAcDecisionTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(4)]
    public void SmallAcCanBecomeZeroWithoutChangingDcQuantizersOrLargeDetail(int area)
    {
        var coefficients = new double[64]; var divisors = Enumerable.Repeat((ushort)5, 64).ToArray();
        coefficients[0] = 320; coefficients[JpegTransform.Zigzag[1]] = 5;
        coefficients[JpegTransform.Zigzag[17]] = -200; coefficients[JpegTransform.Zigzag[63]] = 5;
        var original = coefficients.ToArray(); var quantizers = divisors.ToArray(); var values = new int[64];
        JpegEncoder.Quantize(coefficients, divisors, values);
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecideAc(coefficients, divisors, values, 100, area, context);
        Assert.Equal(64, values[0]); Assert.Equal(-40, values[JpegTransform.Zigzag[17]]);
        Assert.Equal(1, values[JpegTransform.Zigzag[1]]); // Removing it would add a ZRL before the large AC.
        Assert.Equal(0, values[JpegTransform.Zigzag[63]]);
        Assert.Equal(original, coefficients); Assert.Equal(quantizers, divisors);
        Assert.Equal(256, context.ConsumedWork); // Initial scan and three candidate scans.
    }

    [Fact]
    public void ZeroWeightKeepsInitialQuantizationWithoutExtraWork()
    {
        var coefficients = Enumerable.Range(0, 64).Select(i => (i - 32) * 7d).ToArray();
        var divisors = Enumerable.Repeat((ushort)3, 64).ToArray(); var values = new int[64];
        JpegEncoder.Quantize(coefficients, divisors, values); var before = values.ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 1 }, default);
        JpegEncoder.DecideAc(coefficients, divisors, values, 0, 4, context);
        Assert.Equal(before, values); Assert.Equal(0, context.ConsumedWork);
    }

    [Theory]
    [InlineData(63)] [InlineData(127)]
    public void WorkRefusalPrecedesAnUnadmittedCandidateAndDoesNotDisableLaterDecisions(int allowance)
    {
        var coefficients = new double[64]; coefficients[JpegTransform.Zigzag[63]] = 5;
        var divisors = Enumerable.Repeat((ushort)5, 64).ToArray(); var values = new int[64];
        JpegEncoder.Quantize(coefficients, divisors, values); var before = values.ToArray();
        using (var context = new OperationContext(ImageLimits.Default with { MaxWork = allowance }, default))
        {
            var failure = Assert.Throws<ImageLimitException>(() => JpegEncoder.DecideAc(coefficients, divisors, values, 100, 1, context));
            Assert.Equal(ImageLimitKind.Work, failure.Limit); Assert.Equal(before, values);
        }
        using var subsequent = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecideAc(coefficients, divisors, values, 100, 1, subsequent);
        Assert.Equal(0, values[JpegTransform.Zigzag[63]]);
    }

    [Fact]
    public void CancellationIsTerminalBeforePricingOrMutation()
    {
        var coefficients = new double[64]; var divisors = Enumerable.Repeat((ushort)1, 64).ToArray();
        var values = new int[64]; values[63] = 1;
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        var failure = Assert.Throws<OperationCanceledException>(() => JpegEncoder.DecideAc(coefficients, divisors, values, 100, 1, context));
        Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.Equal(1, values[63]); Assert.Equal(0, context.ConsumedWork);
    }
}
