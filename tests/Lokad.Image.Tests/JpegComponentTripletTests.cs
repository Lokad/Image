using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegComponentTripletTests
{
    [Theory]
    [InlineData(1)] [InlineData(-1)]
    public void ThreeWayChoiceEscapesAnAdjacentPairMinimumWithoutChangingDcOrInputs(int sign)
    {
        var (source, values, q, prices) = CoupledCase(sign); var original = values.ToArray();
        var sourceBefore = source.ToArray(); var pricesBefore = prices.ToArray(); var maximum = Error(source, values);
        using var pairContext = new OperationContext(ImageLimits.Default, default);
        var state = JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 100, pairContext, maximum);
        // Every adjacent one/two-component move increases error here. The joint
        // Y-1/Cb+1/Cr+1 move lowers error and Y's amplitude category together.
        Assert.Equal(original, values); Assert.Equal(maximum, state.Error);
        Assert.Equal((1792, 1792, 1792), (state.YPrice, state.CbPrice, state.CrPrice)); // three ZRLs, symbol, three amplitude bits.
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineComponentTriplets444(source, q, q, values, prices, 100, context, maximum, state);
        Assert.Equal(sign * 3, values[63]); Assert.Equal(sign * 5, values[127]); Assert.Equal(sign * 5, values[191]);
        Assert.True(Error(source, values) < maximum);
        for (var c = 0; c < 3; c++) { Assert.Equal(sign * 17, values[c * 64]); for (var i = 1; i < 63; i++) Assert.Equal(0, values[c * 64 + i]); }
        Assert.Equal(sourceBefore, source); Assert.Equal(pricesBefore, prices); Assert.All(q, x => Assert.Equal(1, x));
        Assert.InRange(context.ConsumedWork, 1, JpegEncoder.ComponentTripletWorkPerMcu);
    }

    [Theory]
    [InlineData(25, false)] [InlineData(89, false)] [InlineData(409, false)]
    [InlineData(617, false)] [InlineData(620, false)] [InlineData(621, true)]
    public void RefusalCannotCommitAnUnselectedTriplet(int allowance, bool firstSelected)
    {
        var (source, values, q, prices) = CoupledCase(1); var maximum = Error(source, values);
        using var pair = new OperationContext(ImageLimits.Default, default);
        var state = JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 100, pair, maximum);
        using var context = new OperationContext(new() { MaxWork = allowance }, default);
        var error = Assert.Throws<ImageLimitException>(() => JpegEncoder.RefineComponentTriplets444(source, q, q, values, prices, 100, context, maximum, state));
        Assert.Equal(ImageLimitKind.Work, error.Limit);
        Assert.Equal(firstSelected ? (3, 5, 5) : (4, 4, 4), (values[63], values[127], values[191]));
        Assert.InRange(context.ConsumedWork, 0, allowance);
        var (nextSource, nextValues, nextQ, nextPrices) = CoupledCase(1);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineComponentTriplets444(nextSource, nextQ, nextQ, nextValues, nextPrices, 100, next, maximum, state);
        Assert.Equal((3, 5, 5), (nextValues[63], nextValues[127], nextValues[191]));
    }

    [Fact]
    public void AllValidTrialsFit39123WithoutRepeatingTheSuppliedStateScans()
    {
        var source = Enumerable.Repeat(512d, 192).ToArray(); var values = Enumerable.Repeat(512, 192).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); var prices = Enumerable.Repeat(256, 512).ToArray();
        // Each AC is nonzero: 63*(one symbol + ten amplitude bits)*256.
        var state = new JpegEncoder.Joint444State(0, 177408, 177408, 177408);
        using var context = new OperationContext(new() { MaxWork = 39123 }, default);
        JpegEncoder.RefineComponentTriplets444(source, q, q, values, prices, 100, context, 0, state);
        Assert.Equal(39123, context.ConsumedWork); Assert.All(values, x => Assert.Equal(512, x));
    }

    [Theory]
    [InlineData(127)] [InlineData(-127)]
    public void EffectiveGridBoundsExcludeInvalidNeighborsBeforeAddingPrices(int level)
    {
        var values = Enumerable.Repeat(level, 192).ToArray(); var source = values.Select(x => x * 8d).ToArray();
        var q = Enumerable.Repeat((ushort)8, 64).ToArray(); var limits = Enumerable.Repeat((ushort)127, 128).ToArray();
        var prices = Enumerable.Repeat(256, 512).ToArray();
        var state = new JpegEncoder.Joint444State(0, 129024, 129024, 129024); // 63*(symbol + seven amplitude bits)*256.
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineComponentTriplets444(source, q, q, values, prices, 100, context, 0, state, limits);
        Assert.All(values, x => Assert.Equal(level, x)); Assert.InRange(context.ConsumedWork, 1, 39123);
    }

    [Fact]
    public void CancellationPrecedesAnyMutationAndTheNextOperationSucceeds()
    {
        var (source, values, q, prices) = CoupledCase(1); var original = values.ToArray(); var maximum = Error(source, values);
        using var pair = new OperationContext(ImageLimits.Default, default);
        var state = JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 100, pair, maximum);
        using var token = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, token.Token);
        token.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => JpegEncoder.RefineComponentTriplets444(source, q, q, values, prices, 100, context, maximum, state));
        Assert.Equal(token.Token, error.CancellationToken); Assert.Equal(original, values); Assert.Equal(0, context.ConsumedWork);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineComponentTriplets444(source, q, q, values, prices, 100, next, maximum, state);
        Assert.Equal((3, 5, 5), (values[63], values[127], values[191]));
    }

    [Fact]
    public void DisabledTailLeavesValuesAndWorkUnchanged()
    {
        var (source, values, q, prices) = CoupledCase(1); var original = values.ToArray();
        using var context = new OperationContext(new() { MaxWork = 1 }, default);
        JpegEncoder.RefineComponentTriplets444(source, q, q, values, prices, 0, context, 0, default);
        Assert.Equal(original, values); Assert.Equal(0, context.ConsumedWork);
    }

    private static (double[], int[], ushort[], int[]) CoupledCase(int sign)
    {
        var source = new double[192]; var values = new int[192];
        for (var c = 0; c < 3; c++) { source[c * 64] = values[c * 64] = sign * 17; values[c * 64 + 63] = sign * 4; }
        source[63] = sign * 3.2937551643482448; source[127] = sign * 4.4430380400607525; source[191] = sign * 4.5250117985302518;
        return (source, values, Enumerable.Repeat((ushort)1, 64).ToArray(), Enumerable.Repeat(256, 512).ToArray());
    }

    private static double Error(double[] source, int[] values)
    {
        var error = 0d;
        for (var i = 0; i < 64; i++)
        {
            var y = source[i] - values[i]; var cb = source[64 + i] - values[64 + i]; var cr = source[128 + i] - values[128 + i];
            var red = y + 1.402 * cr; var green = y - .344136286 * cb - .714136286 * cr; var blue = y + 1.772 * cb;
            error += (red * red + green * green + blue * blue) / 3;
        }
        return error;
    }
}
