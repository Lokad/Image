using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegAdjacentLevelTests
{
    [Theory]
    [InlineData(0, 1)] [InlineData(0, -1)]
    [InlineData(2, 1)] [InlineData(2, -1)]
    public void PricedPassRecoversRgbErrorAtZeroOrWithinTheSameCategory(int original, int sign)
    {
        var (source, values, luma, chroma, prices) = Case(original, sign);
        var geometric = values.ToArray(); var sourceBefore = source.ToArray();
        using var reference = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecideJoint444(source, luma, chroma, geometric, 1, reference);
        Assert.Equal(sign * original, geometric[63]);
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecideJoint444(source, luma, chroma, values, 1, context, prices);
        Assert.Equal(sign * (original + 1), values[63]);
        Assert.Equal(0, values[127]); Assert.Equal(0, values[191]);
        for (var c = 0; c < 3; c++) Assert.Equal(0, values[c * 64]);
        Assert.Equal(sourceBefore, source);
    }

    [Theory]
    [InlineData(511, 0, 448)] [InlineData(575, 1, 512)]
    public void WorkRefusalKeepsOnlySelectedLevels(int allowance, int expected, int consumed)
    {
        var (source, values, luma, chroma, prices) = Case(0, 1);
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = allowance }, default);
        var error = Assert.Throws<ImageLimitException>(() =>
            JpegEncoder.DecideJoint444(source, luma, chroma, values, 1, context, prices));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(expected, values[63]);
        Assert.Equal(0, values[127]); Assert.Equal(0, values[191]); Assert.Equal(consumed, context.ConsumedWork);
    }

    [Fact]
    public void MaximumPricedCategoriesFitTheDeclared145536Work()
    {
        var source = Enumerable.Repeat(513d, 192).ToArray(); var values = Enumerable.Repeat(513, 192).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); var prices = Enumerable.Repeat((ushort)256, 512).ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 145536 }, default);
        JpegEncoder.DecideJoint444(source, q, q, values, 100, context, prices);
        Assert.All(values, v => Assert.Equal(513, v)); Assert.Equal(145536, context.ConsumedWork);
    }

    [Fact]
    public void CancellationPrecedesPricedNeighborPlanning()
    {
        var (source, values, luma, chroma, prices) = Case(0, 1); var before = values.ToArray();
        using var token = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, token.Token); token.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() =>
            JpegEncoder.DecideJoint444(source, luma, chroma, values, 1, context, prices));
        Assert.Equal(token.Token, error.CancellationToken); Assert.Equal(before, values); Assert.Equal(0, context.ConsumedWork);
    }

    private static (double[], int[], ushort[], ushort[], ushort[]) Case(int original, int sign)
    {
        var source = new double[192]; var values = new int[192];
        source[63] = sign * (original + .49); values[63] = sign * original; source[127] = sign * 31.4;
        return (source, values, Enumerable.Repeat((ushort)1, 64).ToArray(),
            Enumerable.Repeat((ushort)64, 64).ToArray(), Enumerable.Repeat((ushort)256, 512).ToArray());
    }
}
