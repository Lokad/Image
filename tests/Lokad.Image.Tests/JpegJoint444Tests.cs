using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegJoint444Tests
{
    [Theory]
    [InlineData(1)] [InlineData(-1)]
    public void CoupledResidualsCanShrinkNonzeroCategoriesAndReduceRgbError(int sign)
    {
        var source = new double[192]; var values = new int[192];
        var quantizers = Enumerable.Repeat((ushort)1, 64).ToArray();
        for (var c = 0; c < 3; c++)
        {
            source[c * 64] = values[c * 64] = sign * 24;
            source[c * 64 + 63] = sign * 3.51; values[c * 64 + 63] = sign * 4;
        }
        var original = source.ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecideJoint444(source, quantizers, quantizers, values, 100, context);
        // Initially all three residuals are -0.49 (or their negation). Moving Y
        // and Cb into the next smaller category reduces their coupled RGB error.
        Assert.Equal(sign * 3, values[63]); Assert.Equal(sign * 3, values[127]); Assert.Equal(sign * 4, values[191]);
        for (var c = 0; c < 3; c++) Assert.Equal(sign * 24, values[c * 64]);
        Assert.Equal(original, source); Assert.All(quantizers, q => Assert.Equal(1, q));
        Assert.Equal(960, context.ConsumedWork); // 384 setup + three three-category searches.
    }

    [Fact]
    public void ExactReconstructionRetainsSeparateLumaChromaTablesAndDc()
    {
        var source = new double[192]; var values = new int[192];
        var luma = Enumerable.Range(1, 64).Select(x => (ushort)x).ToArray();
        var chroma = Enumerable.Range(1, 64).Select(x => (ushort)(65 - x)).ToArray();
        for (var c = 0; c < 3; c++)
        for (var i = 0; i < 64; i++)
        {
            values[c * 64 + i] = (i % 7 - 3) * (c + 1);
            source[c * 64 + i] = values[c * 64 + i] * (c == 0 ? luma[i] : chroma[i]);
        }
        var before = values.ToArray(); var lumaBefore = luma.ToArray(); var chromaBefore = chroma.ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecideJoint444(source, luma, chroma, values, 100, context);
        Assert.Equal(before, values); Assert.Equal(lumaBefore, luma); Assert.Equal(chromaBefore, chroma);
    }

    [Fact]
    public void ZeroLambdaDoesNotChargeOrMutateJointPlanning()
    {
        var source = new double[192]; var values = Enumerable.Repeat(1, 192).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 1 }, default);
        JpegEncoder.DecideJoint444(source, q, q, values, 0, context);
        Assert.All(values, v => Assert.Equal(1, v)); Assert.Equal(0, context.ConsumedWork);
    }

    [Theory]
    [InlineData(191)] [InlineData(383)] [InlineData(447)]
    public void WorkRefusalPrecedesCapPricesAndFirstTrialMutation(int allowance)
    {
        var source = new double[192]; var values = new int[192];
        var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        for (var c = 0; c < 3; c++) { source[c * 64 + 63] = 3.51; values[c * 64 + 63] = 4; }
        var before = values.ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = allowance }, default);
        var error = Assert.Throws<ImageLimitException>(() => JpegEncoder.DecideJoint444(source, q, q, values, 100, context));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(before, values);
        Assert.Equal(allowance < 192 ? 0 : allowance < 384 ? 192 : 384, context.ConsumedWork);
    }

    [Fact]
    public void LaterRefusalRestoresAnUnselectedTrialRatherThanLeakingIt()
    {
        var source = new double[192]; var values = new int[192];
        var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        for (var c = 0; c < 3; c++) { source[c * 64 + 63] = 2.51; values[c * 64 + 63] = 3; }
        var before = values.ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 511 }, default);
        Assert.Throws<ImageLimitException>(() => JpegEncoder.DecideJoint444(source, q, q, values, 100, context));
        Assert.Equal(before, values); Assert.Equal(448, context.ConsumedWork);
    }

    [Fact]
    public void MaximumBaselineCategoriesFitTheDeclared121344WorkPerMcu()
    {
        var source = Enumerable.Repeat(512d, 192).ToArray(); var values = Enumerable.Repeat(512, 192).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 121344 }, default);
        JpegEncoder.DecideJoint444(source, q, q, values, 100, context);
        Assert.All(values, v => Assert.Equal(512, v)); Assert.Equal(121344, context.ConsumedWork);
    }

    [Fact]
    public void CancellationPrecedesAllJointPlanningAndMutation()
    {
        var source = new double[192]; var values = Enumerable.Repeat(1, 192).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => JpegEncoder.DecideJoint444(source, q, q, values, 100, context));
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.All(values, v => Assert.Equal(1, v));
        Assert.Equal(0, context.ConsumedWork);
    }
}
