using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegAcBandTests
{
    [Fact]
    public void MasksFollowZigzagGridChangesAndIgnoreDc()
    {
        var limits = Enumerable.Repeat((ushort)1023, 64).ToArray(); limits[0] = 7;
        for (var scan = 1; scan <= 2; scan++) limits[JpegTransform.Zigzag[scan]] = 127;
        for (var scan = 3; scan <= 6; scan++) limits[JpegTransform.Zigzag[scan]] = 63;
        var luma = JpegEncoder.AcBandEnds444(limits);
        Assert.Equal((1UL << 2) | (1UL << 6) | (1UL << 63), luma);
        Array.Fill(limits, (ushort)1023); limits[JpegTransform.Zigzag[1]] = 511;
        var chroma = JpegEncoder.AcBandEnds444(limits); var bands = new JpegEncoder.JointAcBands(luma, chroma);
        Assert.Equal((1UL << 1) | (1UL << 63), chroma);
        Assert.Equal(luma, bands.ForComponent(0)); Assert.Equal(chroma, bands.ForComponent(1)); Assert.Equal(chroma, bands.ForComponent(2));
    }

    [Theory]
    [InlineData(63)] [InlineData(65)]
    public void MaskRequiresOneCompleteGrid(int length)
        => Assert.Throws<ArgumentException>(() => JpegEncoder.AcBandEnds444(new ushort[length]));

    [Fact]
    public void BandBoundariesFlushEobAndPreventCrossBandZrl()
    {
        var values = new int[64]; values[JpegTransform.Zigzag[18]] = -4;
        var prices = Enumerable.Range(0, 256).Select(i => (ushort)(5 + i * 37)).ToArray();
        var expected = prices[0] * 2 + prices[0x13] + 3 * 256;
        Assert.Equal(expected, JpegEncoder.JointAcPrice(values, prices, 1UL << 16));
        Assert.Equal(prices[0xF0] + prices[0x13] + 3 * 256 + prices[0], JpegEncoder.JointAcPrice(values, prices));
        Assert.Equal(Oracle(values, prices, 1UL << 16), expected);
        values.AsSpan().Clear();
        Assert.Equal(prices[0] * 4, JpegEncoder.JointAcPrice(values, prices, (1UL << 1) | (1UL << 2) | (1UL << 5)));
        Assert.Equal(prices[0], JpegEncoder.JointAcPrice(values, prices, 1UL << 63));
        values[JpegTransform.Zigzag[63]] = 2;
        Assert.Equal(prices[0] * 62 + prices[2] + 512, JpegEncoder.JointAcPrice(values, prices, ulong.MaxValue));
    }

    [Fact]
    public void ReplacementPricesMatchIndependentTokensWithoutMutatingInput()
    {
        var values = new int[64]; var prices = Enumerable.Range(0, 256).Select(i => (ushort)(17 + i * 19)).ToArray();
        for (var scan = 1; scan < 64; scan++) if (scan % 7 == 0) values[JpegTransform.Zigzag[scan]] = scan % 5 - 2;
        var original = values.ToArray(); var ends = (1UL << 2) | (1UL << 6) | (1UL << 9) | (1UL << 31);
        for (var scan = 1; scan < 64; scan++)
        {
            var index = JpegTransform.Zigzag[scan]; var trial = values.ToArray(); trial[index] = -3;
            Assert.Equal(Oracle(trial, prices, ends), JpegEncoder.PairAcPrice(values, prices, index, -3, ends));
            Assert.Equal(original, values);
        }
    }

    [Fact]
    public void TrellisTerminatesEachBandButSelectsOneWholeComponentPath()
    {
        var (source, values, q, prices) = EobCase(); var before = source.ToArray();
        var bands = new JpegEncoder.JointAcBands(1UL << 31, (1UL << 15) | (1UL << 31));
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, context, bands: bands);
        for (var c = 0; c < 3; c++) for (var scan = 0; scan < 64; scan++)
            Assert.Equal(scan == 31 || scan == 63 || (c != 0 && scan == 15) ? (c == 0 || (c == 1 && scan == 15) ? -1 : 1) : 0,
                values[c * 64 + JpegTransform.Zigzag[scan]]);
        var selected = values.ToArray();
        var state = JpegEncoder.RefineComponentPairs444(source, q, q, values, prices, 100, context, bands: bands);
        Assert.Equal(Oracle(values[..64], prices[..256], bands.Luma), state.YPrice);
        Assert.Equal(Oracle(values[64..128], prices[256..], bands.Chroma), state.CbPrice);
        Assert.Equal(Oracle(values[128..], prices[256..], bands.Chroma), state.CrPrice);
        JpegEncoder.RefineComponentTriplets444(source, q, q, values, prices, 100, context, state.Error, state, bands: bands);
        Assert.Equal(selected, values); Assert.Equal(before, source);
    }

    [Fact]
    public void DefaultAndExplicitSingleBandRetainLegacyDecisionsAndWork()
    {
        var (source, values, q, prices) = EobCase(); var explicitValues = values.ToArray();
        using var one = new OperationContext(ImageLimits.Default, default); using var two = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, one);
        JpegEncoder.RefineJoint444(source, q, q, explicitValues, prices, 100, two, bands: new(1UL << 63, 1UL << 63));
        Assert.Equal(values, explicitValues); Assert.Equal(12171, one.ConsumedWork); Assert.Equal(one.ConsumedWork, two.ConsumedWork);
    }

    [Fact]
    public void SplitBandsFitTheExistingEightSolveWorkBound()
    {
        var source = Enumerable.Repeat(513d, 192).ToArray(); var values = Enumerable.Repeat(513, 192).ToArray();
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); var prices = Enumerable.Repeat((ushort)256, 512).ToArray();
        using var context = new OperationContext(new() { MaxWork = JpegEncoder.Trellis444WorkPerMcu }, default);
        JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, context, bands: new(1UL << 31, (1UL << 15) | (1UL << 31)));
        Assert.All(values, v => Assert.Equal(513, v)); Assert.InRange(context.ConsumedWork, 1, JpegEncoder.Trellis444WorkPerMcu - 1);
    }

    [Theory]
    [InlineData(383)] [InlineData(775)] [InlineData(1000)]
    public void WorkRefusalPrecedesSelectionAndAReleasedOperationCanSucceed(int allowance)
    {
        var (source, values, q, prices) = EobCase(); var bands = new JpegEncoder.JointAcBands(1UL << 31, 1UL << 31);
        using var context = new OperationContext(new() { MaxWork = allowance }, default);
        Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, context, bands: bands)).Limit);
        Assert.All(values, v => Assert.Equal(0, v)); Assert.InRange(context.ConsumedWork, 0, allowance);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, next, bands: bands);
        Assert.Equal(-1, values[JpegTransform.Zigzag[31]]);
    }

    [Fact]
    public void CancellationPrecedesMutationAndSubsequentOperationSucceeds()
    {
        var (source, values, q, prices) = EobCase(); var bands = new JpegEncoder.JointAcBands(1UL << 31, 1UL << 31);
        using var token = new CancellationTokenSource(); using var context = new OperationContext(ImageLimits.Default, token.Token); token.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() => JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, context, bands: bands));
        Assert.All(values, v => Assert.Equal(0, v)); Assert.Equal(0, context.ConsumedWork);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.RefineJoint444(source, q, q, values, prices, 100, next, bands: bands);
        Assert.Equal(-1, values[JpegTransform.Zigzag[31]]);
    }

    private static int Oracle(int[] values, ushort[] prices, ulong ends)
    {
        var total = 0; var first = 1;
        for (var last = 1; last < 64; last++)
        {
            if (last != 63 && (ends & (1UL << last)) == 0) continue;
            var nonzero = Enumerable.Range(first, last - first + 1).Where(k => values[JpegTransform.Zigzag[k]] != 0).ToArray();
            var previous = first - 1;
            foreach (var scan in nonzero)
            {
                var gap = scan - previous - 1; var magnitude = Math.Abs(values[JpegTransform.Zigzag[scan]]);
                var size = System.Numerics.BitOperations.Log2((uint)magnitude) + 1;
                total += gap / 16 * prices[240] + prices[gap % 16 * 16 + size] + size * 256; previous = scan;
            }
            if (previous < last) total += prices[0]; first = last + 1;
        }
        return total;
    }

    private static (double[], int[], ushort[], ushort[]) EobCase()
    {
        var source = new double[192]; var values = new int[192]; var q = Enumerable.Repeat((ushort)1, 64).ToArray(); q[0] = 255;
        for (var c = 0; c < 3; c++) source[c * 64] = 127;
        var prices = Enumerable.Repeat((ushort)1, 512).ToArray(); prices[0] = prices[256] = 4096;
        return (source, values, q, prices);
    }
}
