using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegJointPriceTests
{
    private static int[] Symbols(int length) => new[] { 0, 240 }.Concat(
        Enumerable.Range(0, Math.Min(16, length)).SelectMany(r => Enumerable.Range(1, 10).Select(n => r * 16 + n))).ToArray();

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    public void SmoothedSourceLengthsMatchSeparateCanonicalBuilder(int pattern)
    {
        var symbols = Symbols(63); var counts = new ushort[324]; var expected = new int[512];
        for (var group = 0; group < 2; group++)
        {
            var model = counts.AsSpan(group * 162, 162);
            if (pattern == 1) model[group == 0 ? 0 : 1] = 8064;
            if (pattern == 2) model.Fill(49);
            if (pattern == 3) for (var i = 0; i < 162; i++) model[i] = (ushort)((i * 17 + group) % 47);
            if (pattern == 4) for (var i = 0; i < 12; i++) model[i + group] = (ushort)((1 << i) - 1);
            if (pattern == 5) { model[0] = 64; model[2 + group] = 128; }
        }
        using var context = new OperationContext(ImageLimits.Default, default);
        for (var group = 0; group < 2; group++)
        {
            var frequencies = new long[256];
            for (var i = 0; i < 162; i++) frequencies[symbols[i]] = counts[group * 162 + i] + 1;
            var table = JpegHuffmanBuilder.Build(frequencies, context);
            foreach (var symbol in symbols) expected[group * 256 + symbol] = table.Lengths[symbol] * 256;
        }
        var beforeWork = context.ConsumedWork;
        JpegEncoder.FinishSourceBandPrices(counts, default, context);
        var offsets = new ushort[128]; JpegEncoder.SourcePriceOffsets444(default, offsets);
        var prices = new JpegEncoder.JointAcPrices(counts, offsets);
        for (var group = 0; group < 2; group++)
        {
            var model = prices.ForComponent(group); var kraft = 0;
            foreach (var symbol in symbols)
            {
                var value = model.At(symbol, 1);
                Assert.Equal(expected[group * 256 + symbol], value);
                Assert.InRange(value, 256, 4096); Assert.Equal(0, value % 256);
                kraft += 1 << (16 - value / 256);
            }
            Assert.True(kraft < 65536);
        }
        Assert.Equal(108002, context.ConsumedWork - beforeWork);
    }

    [Fact]
    public void ShortBandsRetainPriorsForUnstoredSymbolsAndIndependentModels()
    {
        var bands = new JpegEncoder.JointAcBands((1UL << 1) | (1UL << 3), 1UL << 5);
        var shape = JpegEncoder.SourcePriceShape444(bands); var counts = new ushort[shape.Slots];
        var offsets = new ushort[128]; JpegEncoder.SourcePriceOffsets444(bands, offsets);
        var expected = new Dictionary<(int, int, int), int>(); var at = 0;
        using var oracle = new OperationContext(ImageLimits.Default, default);
        foreach (var (group, lengths) in new[] { (0, new[] { 1, 2, 60 }), (1, new[] { 5, 58 }) })
        {
            var first = 1;
            foreach (var length in lengths)
            {
                var symbols = Symbols(length); var frequencies = new long[256];
                foreach (var symbol in Symbols(63)) frequencies[symbol] = 1;
                for (var i = 0; i < symbols.Length; i++)
                {
                    var value = (ushort)(symbols[i] == 240 && length <= 16 ? 0 : (i * 7 + first + group) % 19);
                    counts[at + i] = value; frequencies[symbols[i]] += value;
                }
                var table = JpegHuffmanBuilder.Build(frequencies, oracle);
                foreach (var symbol in symbols) expected[(group, first, symbol)] = table.Lengths[symbol] * 256;
                at += symbols.Length; first += length;
            }
        }
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.FinishSourceBandPrices(counts, bands, context);
        var prices = new JpegEncoder.JointAcPrices(counts, offsets);
        foreach (var (key, value) in expected) Assert.Equal(value, prices.ForComponent(key.Item1).At(key.Item3, key.Item2));
        Assert.Equal(shape.FinishWork, context.ConsumedWork);
    }

    [Fact]
    public void EveryOriginalQualityFitsTheSparseArenaAndMatchesActualQuantizers()
    {
        var quantize = typeof(JpegEncoder).GetMethod("Quantization", BindingFlags.Static | BindingFlags.NonPublic,
            null, new[] { typeof(int), typeof(bool) }, null)!;
        var maximum = 0; var offsets = new ushort[128];
        using var context = new OperationContext(ImageLimits.Default, default);
        for (var quality = 1; quality <= 98; quality++)
        {
            var scale = quality < 50 ? 5000 / quality : 200 - quality * 2; var p = (16 * scale + 50) / 100;
            var y = (ushort[])quantize.Invoke(null, new object[] { quality, false })!;
            var c = (ushort[])quantize.Invoke(null, new object[] { quality, true })!;
            var actual = JpegEncoder.SourcePriceBands444(y, c, p * p, context);
            Assert.Equal(actual, JpegEncoder.PlannedSourcePriceBands444(quality));
            var shape = JpegEncoder.SourcePriceShape444(actual); maximum = Math.Max(maximum, shape.Slots);
            Assert.InRange(shape.Slots, 24, JpegEncoder.SourcePriceCapacity);
            JpegEncoder.SourcePriceOffsets444(actual, offsets);
        }
        Assert.Equal(750, maximum); Assert.True(750 * 2 + 128 * 2 < 2048);
        var control = JpegEncoder.SourcePriceShape444(JpegEncoder.PlannedSourcePriceBands444(69));
        Assert.Equal(478, control.Slots); Assert.Equal(416519, control.FinishWork);
    }

    [Fact]
    public void PriceWorkRefusalPrecedesHistogramMutationAndNextOperationSucceeds()
    {
        var counts = new ushort[324]; counts[0] = 64; var before = counts.ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 108001 }, default);
        var error = Assert.Throws<ImageLimitException>(() => JpegEncoder.FinishSourceBandPrices(counts, default, context));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(before, counts); Assert.Equal(0, context.ConsumedWork);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.FinishSourceBandPrices(counts, default, next); Assert.InRange(counts[0], 256, 4096);
    }

    [Theory]
    [InlineData(0, 65535)] [InlineData(0, 8065)] [InlineData(162, 8065)]
    public void InvalidHistogramDoesNotChangeAnyModel(int index, int value)
    {
        var counts = new ushort[324]; counts[2] = 64; counts[index] = (ushort)value; var before = counts.ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegEncoder.FinishSourceBandPrices(counts, default, context));
        Assert.Equal(before, counts);
    }

    [Fact]
    public void ImpossibleShortBandZrlIsRejectedBeforeMutation()
    {
        var bands = new JpegEncoder.JointAcBands(1UL << 1, 0); var counts = new ushort[JpegEncoder.SourcePriceShape444(bands).Slots];
        counts[1] = 1; var before = counts.ToArray(); using var context = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegEncoder.FinishSourceBandPrices(counts, bands, context)); Assert.Equal(before, counts);
    }

    [Fact]
    public void AggregateHistogramBoundIsCheckedBeforeMutation()
    {
        var counts = new ushort[324]; counts[0] = counts[2] = 4033; var before = counts.ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegEncoder.FinishSourceBandPrices(counts, default, context)); Assert.Equal(before, counts);
    }

    [Fact]
    public void CancellationPrecedesPriceMutation()
    {
        var counts = new ushort[324]; counts[0] = 64; var before = counts.ToArray();
        using var token = new CancellationTokenSource(); using var context = new OperationContext(ImageLimits.Default, token.Token); token.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => JpegEncoder.FinishSourceBandPrices(counts, default, context));
        Assert.Equal(token.Token, error.CancellationToken); Assert.Equal(before, counts); Assert.Equal(0, context.ConsumedWork);
    }

    [Theory]
    [InlineData(0, 1, 1, 0)] [InlineData(0, 64, 2145, 0)]
    [InlineData(31, 64, 2145, 1054)] [InlineData(32, 64, 2145, 1089)]
    [InlineData(63, 64, 2145, 2144)] [InlineData(63, 64, 67108864, 67108863)]
    public void SourceSketchIncludesEdgesAndUsesWidePositionArithmetic(int sample, int count, int mcus, int expected)
        => Assert.Equal(expected, JpegEncoder.JointPriceSamplePosition(sample, count, mcus));
}
