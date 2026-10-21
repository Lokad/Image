using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegPrecisionGrid444Tests
{
    [Theory]
    [InlineData(-1)] [InlineData(1)]
    public void CoupledChoicesKeepTheOriginalErrorAllowanceAndLegalGrid(int sign)
    {
        var q = Enumerable.Repeat((ushort)1, 64).ToArray();
        var source = new double[192]; var values = new int[192];
        for (var c = 0; c < 3; c++)
        {
            source[c * 64] = sign * 23.5; values[c * 64] = sign * 24;
            source[c * 64 + 63] = sign * 28.08; values[c * 64 + 63] = sign * 28;
        }
        var prices = Enumerable.Repeat(256, 512).ToArray();
        var sourceBefore = source.ToArray(); var qBefore = q.ToArray(); var pricesBefore = prices.ToArray();
        var fineError = Error(source, values);
        var coarse = values.Select(x => (int)Math.Round(x / 8d, MidpointRounding.AwayFromZero) * 8).ToArray();
        var coarseError = Error(source, coarse);
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecidePrecisionGrid444(source, q, q, values, prices, 100, context);
        // A coarser grid supplies a feasible floor, not a second 1/32 allowance.
        Assert.True(Error(source, values) <= Math.Max(coarseError, fineError + fineError / 32) + 1e-9);
        Assert.NotEqual(coarse, values); // The priced choices survive rescaling.
        Assert.All(values, x => Assert.Equal(0, x % 8));
        Assert.Equal(sourceBefore, source); Assert.Equal(qBefore, q); Assert.Equal(pricesBefore, prices);
        Assert.InRange(context.ConsumedWork, 2880, 3 * 49472 + JpegEncoder.Trellis444WorkPerMcu + JpegEncoder.ComponentPairWorkPerMcu);
    }

    [Theory]
    [InlineData(-1)] [InlineData(1)]
    public void EveryStageHonorsAmplitudeBoundsAndDistinctDcRange(int sign)
    {
        var source = Enumerable.Repeat(sign * 100000d, 192).ToArray();
        var values = Enumerable.Repeat(sign * 1023, 192).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); var prices = Enumerable.Repeat(256, 512).ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecidePrecisionGrid444(source, q, q, values, prices, 100, context);
        for (var c = 0; c < 3; c++)
        {
            Assert.Equal(sign < 0 ? -1024 : 1016, values[c * 64]);
            for (var i = 1; i < 64; i++) { Assert.InRange(values[c * 64 + i], -1016, 1016); Assert.Equal(0, values[c * 64 + i] % 8); }
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void RefusalOrCancellationPrecedesMutationAndSubsequentOperationSucceeds(bool cancel)
    {
        var source = new double[192]; var values = Enumerable.Repeat(17, 192).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); var prices = Enumerable.Repeat(256, 512).ToArray();
        using var token = new CancellationTokenSource(); using var context = new OperationContext(new() { MaxWork = 2879 }, token.Token);
        if (cancel) token.Cancel();
        if (cancel) Assert.ThrowsAny<OperationCanceledException>(() => JpegEncoder.DecidePrecisionGrid444(source, q, q, values, prices, 100, context));
        else Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => JpegEncoder.DecidePrecisionGrid444(source, q, q, values, prices, 100, context)).Limit);
        Assert.All(values, x => Assert.Equal(17, x)); Assert.Equal(0, context.ConsumedWork);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.DecidePrecisionGrid444(source, q, q, values, prices, 0, next); Assert.All(values, x => Assert.Equal(17, x));
        JpegEncoder.DecidePrecisionGrid444(source, q, q, values, prices, 100, next); Assert.All(values, x => Assert.Equal(0, x));
    }

    private static double Error(double[] source, int[] values)
    {
        var total = 0d;
        for (var i = 0; i < 64; i++)
        {
            var y = source[i] - values[i]; var cb = source[64 + i] - values[64 + i]; var cr = source[128 + i] - values[128 + i];
            var r = y + 1.402 * cr; var g = y - .344136286 * cb - .714136286 * cr; var b = y + 1.772 * cb;
            total += (r * r + g * g + b * b) / 3;
        }
        return total;
    }
}
