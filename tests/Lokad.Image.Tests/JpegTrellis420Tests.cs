using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegTrellis420Tests
{
    [Theory]
    [InlineData(1, 3)] [InlineData(4, 4)]
    public void SamplingCellWeightControlsSmallerAmplitudeCategory(int area, int expected)
    {
        var source = new double[64]; source[0] = 17; source[JpegTransform.Zigzag[1]] = 3.9;
        var values = new int[64]; values[0] = 17; values[JpegTransform.Zigzag[1]] = 4;
        var before = source.ToArray(); var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.Refine420(source, q, values, 1, area, context);
        Assert.Equal(expected, values[JpegTransform.Zigzag[1]]); Assert.Equal(17, values[0]);
        Assert.Equal(before, source); Assert.All(q, v => Assert.Equal((ushort)1, v));
        for (var scan = 2; scan < 64; scan++) Assert.Equal(0, values[JpegTransform.Zigzag[scan]]);
    }

    [Fact]
    public void AllTenCategoriesReachTheDeclaredWorkBound()
    {
        var source = Enumerable.Repeat(513d, 64).ToArray(); var values = Enumerable.Repeat(513, 64).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 21937 }, default);
        JpegEncoder.Refine420(source, q, values, 100, 1, context);
        Assert.Equal(21937, context.ConsumedWork); Assert.Equal(513, values[0]);
        Assert.All(values.Skip(1), v => Assert.Equal(511, v));
    }

    [Theory]
    [InlineData(191)] [InlineData(255)] [InlineData(319)] [InlineData(21936)]
    public void RefusalLeavesEveryUnselectedCoefficientUntouched(int allowance)
    {
        var source = Enumerable.Repeat(513d, 64).ToArray(); var values = Enumerable.Repeat(513, 64).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = allowance }, default);
        var error = Assert.Throws<ImageLimitException>(() => JpegEncoder.Refine420(source, q, values, 100, 1, context));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.All(values, v => Assert.Equal(513, v));
        Assert.InRange(context.ConsumedWork, 0, allowance);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.Refine420(source, q, values, 100, 1, next); Assert.Equal(511, values[63]);
    }

    [Fact]
    public void CancellationPrecedesAnyPathWork()
    {
        var source = new double[64]; var values = new int[64]; values[63] = 1;
        var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var token = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, token.Token); token.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => JpegEncoder.Refine420(source, q, values, 100, 1, context));
        Assert.Equal(token.Token, error.CancellationToken); Assert.Equal(1, values[63]); Assert.Equal(0, context.ConsumedWork);
    }

    [Fact]
    public void ZeroLambdaDoesNotChargeOrChangeValues()
    {
        var source = new double[64]; var values = new int[64]; values[63] = 1;
        var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 1 }, default);
        JpegEncoder.Refine420(source, q, values, 0, 1, context);
        Assert.Equal(1, values[63]); Assert.Equal(0, context.ConsumedWork);
    }
}
