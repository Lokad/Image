using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegSourceBandPriceTests
{
    [Fact]
    public void CountingPreservesFineAmplitudesAndResetsRunsAtBandEnds()
    {
        var values = new int[64]; values[JpegTransform.Zigzag[18]] = -9; var before = values.ToArray();
        var bands = new int[256]; var single = new int[256];
        JpegEncoder.CountJointAc(values, bands, 1UL << 16);
        JpegEncoder.CountJointAc(values, single);
        // Fine coefficient -9 stays category4. No rescaling to effective units.
        Assert.Equal(2, bands[0]); Assert.Equal(1, bands[0x14]); Assert.Equal(3, bands.Sum());
        Assert.Equal(0, bands[240]); Assert.Equal(1, single[0]); Assert.Equal(1, single[240]); Assert.Equal(1, single[0x14]);
        Assert.Equal(before, values);
    }

    [Fact]
    public void EmptySingletonBandsCountEveryEobButNoDcAndNoDuplicateFinalEob()
    {
        var values = new int[64]; values[0] = 1023; var counts = new int[256];
        JpegEncoder.CountJointAc(values, counts, ulong.MaxValue);
        Assert.Equal(63, counts[0]); Assert.Equal(63, counts.Sum());
        var single = new int[256]; JpegEncoder.CountJointAc(values, single, 1UL << 63);
        Assert.Equal(1, single[0]); Assert.Equal(1, single.Sum());
        counts.AsSpan().Clear(); values[JpegTransform.Zigzag[63]] = 3;
        JpegEncoder.CountJointAc(values, counts, ulong.MaxValue);
        Assert.Equal(62, counts[0]); Assert.Equal(1, counts[2]); Assert.Equal(63, counts.Sum());
    }

    [Fact]
    public void SourceMasksUseQuantizerFactorsWithoutChangingTheirInputs()
    {
        var (luma, chroma) = Quantizers(); var y = luma.ToArray(); var cbcr = chroma.ToArray();
        using var context = new OperationContext(new() { MaxWork = 1920 }, default);
        var bands = JpegEncoder.SourcePriceBands444(luma, chroma, 100, context);
        Assert.Equal((1UL << 2) | (1UL << 6) | (1UL << 8) | (1UL << 9) | (1UL << 63), bands.Luma);
        Assert.Equal((1UL << 3) | (1UL << 4) | (1UL << 5) | (1UL << 63), bands.Chroma);
        Assert.Equal(1920, context.ConsumedWork); Assert.Equal(y, luma); Assert.Equal(cbcr, chroma);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void MaskWorkOrCancellationRefusalPrecedesStorageAndSubsequentWorkSucceeds(bool cancel)
    {
        var (luma, chroma) = Quantizers(); var y = luma.ToArray(); var cbcr = chroma.ToArray();
        using var token = new CancellationTokenSource(); using var context = new OperationContext(new() { MaxWork = 1919 }, token.Token);
        if (cancel) token.Cancel();
        if (cancel) Assert.ThrowsAny<OperationCanceledException>(() => JpegEncoder.SourcePriceBands444(luma, chroma, 100, context));
        else Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => JpegEncoder.SourcePriceBands444(luma, chroma, 100, context)).Limit);
        Assert.Equal(0, context.ConsumedWork); Assert.Equal(y, luma); Assert.Equal(cbcr, chroma);
        using var next = new OperationContext(ImageLimits.Default, default);
        Assert.NotEqual(default, JpegEncoder.SourcePriceBands444(luma, chroma, 100, next));
    }

    [Fact]
    public void DisabledSourceBandsDoNotChargeOrChangeInputs()
    {
        var (luma, chroma) = Quantizers(); var y = luma.ToArray(); var cbcr = chroma.ToArray();
        using var context = new OperationContext(new() { MaxWork = 1 }, default);
        Assert.Equal(default, JpegEncoder.SourcePriceBands444(luma, chroma, 0, context));
        Assert.Equal(0, context.ConsumedWork); Assert.Equal(y, luma); Assert.Equal(cbcr, chroma);
    }

    private static (ushort[], ushort[]) Quantizers()
    {
        var y = Enumerable.Repeat((ushort)20, 64).ToArray(); var cbcr = y.ToArray(); y[0] = 255;
        for (var i = 1; i <= 2; i++) y[JpegTransform.Zigzag[i]] = 1;
        for (var i = 3; i <= 6; i++) y[JpegTransform.Zigzag[i]] = 2;
        y[JpegTransform.Zigzag[9]] = 3;
        for (var i = 1; i <= 3; i++) cbcr[JpegTransform.Zigzag[i]] = 2;
        cbcr[JpegTransform.Zigzag[5]] = 5;
        return (y, cbcr);
    }
}
