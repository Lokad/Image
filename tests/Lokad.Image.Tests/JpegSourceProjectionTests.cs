using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegSourceProjectionTests
{
    [Fact]
    public void OriginalQuantizersSelectTheLargestAdmittedPointTransform()
    {
        var quantize = typeof(JpegEncoder).GetMethod("Quantization", BindingFlags.Static | BindingFlags.NonPublic,
            null, new[] { typeof(int), typeof(bool) }, null)!;
        using var context = new OperationContext(ImageLimits.Default, default);
        var levels = new byte[128];
        for (var quality = 1; quality <= 100; quality++)
        {
            var y = (ushort[])quantize.Invoke(null, new object[] { quality, false })!;
            var c = (ushort[])quantize.Invoke(null, new object[] { quality, true })!;
            var originalY = y.ToArray(); var originalC = c.ToArray();
            var scale = quality < 50 ? 5000 / quality : 200 - 2 * quality;
            var precision = (16 * scale + 50) / 100;
            JpegEncoder.SourcePointLevels444(y, c, precision * precision, levels, context);
            for (var family = 0; family < 2; family++)
            for (var i = 0; i < 64; i++)
            {
                var q = (family == 0 ? y : c)[i]; var level = levels[family * 64 + i];
                Assert.InRange(level, (byte)0, (byte)13);
                Assert.True(level == 0 || (long)q * (1 << level) <= precision);
                Assert.True(level == 13 || (long)q * (1 << (level + 1)) > precision);
            }
            Assert.Equal(originalY, y); Assert.Equal(originalC, c);
        }
        Assert.Equal(100 * 1920, context.ConsumedWork);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SetupRefusalPrecedesDestinationMutationAndNextOperationSucceeds(bool cancel)
    {
        var q = Enumerable.Repeat((ushort)1, 64).ToArray(); var levels = Enumerable.Repeat((byte)165, 128).ToArray();
        var before = levels.ToArray(); using var token = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 1919 }, token.Token);
        if (cancel) token.Cancel();
        if (cancel) Assert.ThrowsAny<OperationCanceledException>(() => JpegEncoder.SourcePointLevels444(q, q, 100, levels, context));
        else Assert.Equal(ImageLimitKind.Work, Assert.Throws<ImageLimitException>(() => JpegEncoder.SourcePointLevels444(q, q, 100, levels, context)).Limit);
        Assert.Equal(before, levels); Assert.All(q, v => Assert.Equal((ushort)1, v)); Assert.Equal(0, context.ConsumedWork);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.SourcePointLevels444(q, q, 100, levels, next);
        Assert.All(levels, v => Assert.Equal((byte)3, v)); Assert.Equal(1920, next.ConsumedWork);
    }

    [Theory]
    [InlineData(-3, -1)] [InlineData(-1, -1)] [InlineData(0, 0)]
    [InlineData(1, 0)] [InlineData(3, 0)] [InlineData(4, 1)]
    public void HistogramUsesSignedCodedAmplitudesWithoutChangingTrainingChoices(int fine, int coded)
    {
        var values = new int[64]; values[JpegTransform.Zigzag[1]] = fine; var original = values.ToArray();
        var levels = Enumerable.Repeat((byte)2, 64).ToArray(); var counts = new ushort[354];
        var offsets = new ushort[128]; var pending = new byte[64];
        JpegEncoder.SourcePriceOffsets444(default, offsets);
        JpegEncoder.CountSourceBandAc(values, counts, offsets.AsSpan(0, 64), pending, 0, levels);
        JpegEncoder.FlushSourceBandEobRuns(counts, offsets.AsSpan(0, 64), pending, 0);
        Assert.Equal(coded == 0 ? 0 : 1, counts[17]); // run0/category1
        Assert.Equal(1, counts[0]); Assert.Equal(1, counts[16]);
        Assert.Equal(coded == 0 ? 2 : 3, counts.Sum(x => (int)x));
        Assert.Equal(original, values); Assert.All(levels, v => Assert.Equal((byte)2, v)); Assert.All(pending, v => Assert.Equal((byte)0, v));
    }

    [Fact]
    public void ProjectedZerosExtendRunsAndCoalesceTerminalBlocks()
    {
        var values = new int[64]; values[JpegTransform.Zigzag[1]] = 1; values[JpegTransform.Zigzag[2]] = -3;
        var levels = Enumerable.Repeat((byte)2, 64).ToArray(); var counts = new ushort[354];
        var offsets = new ushort[128]; var pending = new byte[64]; JpegEncoder.SourcePriceOffsets444(default, offsets);
        JpegEncoder.CountSourceBandAc(values, counts, offsets.AsSpan(0, 64), pending, 0, levels);
        JpegEncoder.FlushSourceBandEobRuns(counts, offsets.AsSpan(0, 64), pending, 0);
        Assert.Equal(1, counts[27]); Assert.Equal(0, counts[17]); // run1/category1
        Array.Clear(counts); values[JpegTransform.Zigzag[2]] = 0;
        for (var i = 0; i < 64; i++) JpegEncoder.CountSourceBandAc(values, counts, offsets.AsSpan(0, 64), pending, 0, levels);
        JpegEncoder.FlushSourceBandEobRuns(counts, offsets.AsSpan(0, 64), pending, 0);
        Assert.Equal(1, counts[7]); Assert.Equal(64, counts[16]); // exponent6 / actual64blocks
        Assert.Equal(65, counts.Sum(x => (int)x));
    }

    [Fact]
    public void IncompleteProjectionIsRejectedBeforeAnyHistogramMutation()
    {
        var values = new int[64]; values[1] = 1; var counts = new ushort[354]; counts[0] = 11;
        var original = counts.ToArray(); var offsets = new ushort[128]; var pending = new byte[64];
        JpegEncoder.SourcePriceOffsets444(default, offsets);
        Assert.Throws<ArgumentException>(() => JpegEncoder.CountSourceBandAc(values, counts, offsets.AsSpan(0, 64), pending, 0, new byte[63]));
        Assert.Equal(original, counts); Assert.All(pending, v => Assert.Equal((byte)0, v));
    }
}
