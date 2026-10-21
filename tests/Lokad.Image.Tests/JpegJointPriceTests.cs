using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegJointPriceTests
{
    [Theory]
    [InlineData(1, 0)] [InlineData(2, 256)] [InlineData(3, 405)]
    [InlineData(5, 594)] [InlineData(7, 718)] [InlineData(1023, 2559)]
    [InlineData(65536, 4096)] [InlineData(1073741824, 7680)] [InlineData(int.MaxValue, 7935)]
    public void IntegerLogHasStableQ8ValuesWithoutOverflow(int value, int expected)
        => Assert.Equal(expected, JpegEncoder.JointLog2Q8(value));

    [Fact]
    public void SmoothingPricesAllLegalSymbolsAndKeepsModelsIndependent()
    {
        var prices = new int[512]; prices[0] = 64;
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.FinishJointPrices(prices, context);
        // +1 smoothing: first model total226/EOB65, second total162/each1.
        Assert.Equal(460, prices[0]); Assert.Equal(2001, prices[1]); Assert.Equal(1879, prices[256]);
        for (var i = 0; i < 512; i++)
        {
            var symbol = i % 256;
            if (symbol is 0 or 0xF0 || (symbol & 15) is >= 1 and <= 10) Assert.InRange(prices[i], 1, 4096);
            else Assert.Equal(0, prices[i]);
        }
        Assert.Equal(3632, context.ConsumedWork);
    }

    [Fact]
    public void PriceWorkRefusalPrecedesHistogramMutation()
    {
        var counts = new int[512]; counts[0] = 64; var before = counts.ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 3631 }, default);
        var error = Assert.Throws<ImageLimitException>(() => JpegEncoder.FinishJointPrices(counts, context));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(before, counts); Assert.Equal(0, context.ConsumedWork);
    }

    [Fact]
    public void CancellationPrecedesPriceMutation()
    {
        var counts = new int[512]; counts[0] = 64; var before = counts.ToArray();
        using var token = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, token.Token); token.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => JpegEncoder.FinishJointPrices(counts, context));
        Assert.Equal(token.Token, error.CancellationToken); Assert.Equal(before, counts); Assert.Equal(0, context.ConsumedWork);
    }

    [Theory]
    [InlineData(0, 1, 1, 0)] [InlineData(0, 64, 2145, 0)]
    [InlineData(31, 64, 2145, 1054)] [InlineData(32, 64, 2145, 1089)]
    [InlineData(63, 64, 2145, 2144)] [InlineData(63, 64, 67108864, 67108863)]
    public void SourceSketchIncludesEdgesAndUsesWidePositionArithmetic(int sample, int count, int mcus, int expected)
        => Assert.Equal(expected, JpegEncoder.JointPriceSamplePosition(sample, count, mcus));
}
