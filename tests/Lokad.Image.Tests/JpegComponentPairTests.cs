using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegComponentPairTests
{
    [Theory]
    [InlineData(1)] [InlineData(-1)]
    public void JointMovesLowerCategoriesWhileRetainingDcSourceAndQuantizers(int sign)
    {
        var (source, values, q, prices) = CoupledCase(sign); var before = source.ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 100, context);
        // The second pair exchanges a Y category for Cr while reducing coupled
        // RGB error at equal rate. The final Cb/Cr categories save two bits.
        Assert.Equal(sign * 4, values[63]); Assert.Equal(sign * 3, values[127]); Assert.Equal(sign * 3, values[191]);
        for (var c = 0; c < 3; c++)
        {
            Assert.Equal(sign * 17, values[c * 64]);
            for (var i = 1; i < 63; i++) Assert.Equal(0, values[c * 64 + i]);
        }
        Assert.Equal(before, source); Assert.All(q, x => Assert.Equal((ushort)1, x));
        Assert.Equal(196002, context.ConsumedWork);
    }

    [Theory]
    [InlineData(383, 4, 4)] [InlineData(575, 4, 4)] [InlineData(583, 4, 4)]
    [InlineData(711, 4, 4)] [InlineData(839, 4, 4)] [InlineData(1609, 4, 4)]
    [InlineData(1610, 3, 3)]
    public void RefusalNeverCommitsAnUnselectedPair(int allowance, int expectedY, int expectedCb)
    {
        var (source, values, q, prices) = CoupledCase(1);
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = allowance }, default);
        var error = Assert.Throws<ImageLimitException>(() => JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 100, context));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(expectedY, values[63]); Assert.Equal(expectedCb, values[127]);
        Assert.Equal(4, values[191]); Assert.InRange(context.ConsumedWork, 0, allowance);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 100, next);
        Assert.Equal(3, values[191]);
    }

    [Fact]
    public void AllValidAdjacentTrialsFitTheExact196002Bound()
    {
        var source = Enumerable.Repeat(512d, 192).ToArray(); var values = Enumerable.Repeat(512, 192).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); var prices = Enumerable.Repeat(256, 512).ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 196002 }, default);
        JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 100, context);
        Assert.Equal(196002, context.ConsumedWork); Assert.All(values, v => Assert.Equal(512, v));
    }

    [Theory]
    [InlineData(1023)] [InlineData(-1023)]
    public void BaselineAmplitudeEdgesSkipOutOfRangeNeighbors(int level)
    {
        var values = Enumerable.Repeat(level, 192).ToArray(); var source = values.Select(v => (double)v).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); var prices = Enumerable.Repeat(256, 512).ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 100, context);
        Assert.All(values, v => Assert.Equal(level, v)); Assert.Equal(75042, context.ConsumedWork);
    }

    [Fact]
    public void CancellationPrecedesInitialErrorAndPricing()
    {
        var (source, values, q, prices) = CoupledCase(1); var before = values.ToArray();
        using var token = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, token.Token); token.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 100, context));
        Assert.Equal(token.Token, error.CancellationToken); Assert.Equal(before, values); Assert.Equal(0, context.ConsumedWork);
    }

    [Fact]
    public void ZeroLambdaLeavesCoefficientsAndWorkUnchanged()
    {
        var (source, values, q, prices) = CoupledCase(1); var before = values.ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 1 }, default);
        JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 0, context);
        Assert.Equal(before, values); Assert.Equal(0, context.ConsumedWork);
    }

    private static (double[], int[], ushort[], int[]) CoupledCase(int sign)
    {
        var source = new double[192]; var values = new int[192];
        for (var c = 0; c < 3; c++) { source[c * 64] = values[c * 64] = sign * 17; source[c * 64 + 63] = sign * 3.51; values[c * 64 + 63] = sign * 4; }
        return (source, values, Enumerable.Repeat((ushort)1, 64).ToArray(), Enumerable.Repeat(256, 512).ToArray());
    }
}
