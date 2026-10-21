using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegTrellis444Tests
{
    [Fact]
    public void WholeRunPricesIncludeZrlAndTrailingEobWithStableLevelTies()
    {
        var (source, values, q, prices) = EobCase(); var sourceBefore = source.ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, context);
        // A final +/-1 costs three ZRLs + one symbol + one amplitude bit (260
        // Q8 units), replacing the deliberately expensive EOB (4096 units).
        // Y has a symmetric tie: the bank's -1 comes first. Coupled Cb/Cr use +1.
        Assert.Equal(-1, values[63]); Assert.Equal(1, values[127]); Assert.Equal(1, values[191]);
        for (var c = 0; c < 3; c++) for (var i = 0; i < 63; i++) Assert.Equal(0, values[c * 64 + i]);
        Assert.Equal(sourceBefore, source); Assert.Equal(12171, context.ConsumedWork);
    }

    [Theory]
    [InlineData(383, 0)] [InlineData(575, 0)] [InlineData(775, 0)]
    [InlineData(4249, 0)] [InlineData(8178, -1)]
    public void RefusalNeverCommitsAnUnselectedPath(int allowance, int selectedY)
    {
        var (source, values, q, prices) = EobCase();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = allowance }, default);
        var error = Assert.Throws<ImageLimitException>(() => JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, context));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(selectedY, values[63]);
        Assert.Equal(0, values[127]); Assert.Equal(0, values[191]);
        for (var c = 0; c < 3; c++) Assert.Equal(0, values[c * 64]);
        Assert.InRange(context.ConsumedWork, 0, allowance);
    }

    [Fact]
    public void EightMaximumCategorySolvesFitTheDeclared521904Bound()
    {
        var source = Enumerable.Repeat(513d, 192).ToArray(); var values = Enumerable.Repeat(513, 192).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); var prices = Enumerable.Repeat(256, 512).ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 521904 }, default);
        JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, context);
        Assert.All(values, v => Assert.Equal(513, v)); Assert.Equal(521904, context.ConsumedWork);
    }

    [Fact]
    public void ZeroLambdaDoesNotChargeOrChangeValues()
    {
        var (source, values, q, prices) = EobCase();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 1 }, default);
        JpegEncoder.RefineJoint444(source, q, q, values, prices, 0, context);
        Assert.All(values, v => Assert.Equal(0, v)); Assert.Equal(0, context.ConsumedWork);
    }

    [Fact]
    public void CancellationPrecedesInitialErrorAndPathWork()
    {
        var (source, values, q, prices) = EobCase();
        using var token = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, token.Token); token.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, context));
        Assert.Equal(token.Token, error.CancellationToken); Assert.All(values, v => Assert.Equal(0, v));
        Assert.Equal(0, context.ConsumedWork);
    }

    private static (double[], int[], ushort[], int[]) EobCase()
    {
        var source = new double[192]; var values = new int[192];
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); q[0] = 255;
        for (var c = 0; c < 3; c++) source[c * 64] = 127;
        var prices = Enumerable.Repeat(1, 512).ToArray(); prices[0] = prices[256] = 4096;
        return (source, values, q, prices);
    }
}
