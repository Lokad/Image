using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegJointPriceTests
{
    // Sparse storage order; the canonical oracle visits symbols numerically.
    private static int[] Symbols(int length) => new[] { 0, 240 }
        .Concat(Enumerable.Range(1, 14).Select(e => e << 4)).Concat(new[] { -1 }).Concat(
        Enumerable.Range(0, Math.Min(16, length)).SelectMany(r => Enumerable.Range(1, 10).Select(n => r * 16 + n))).ToArray();

    private static Dictionary<int, int> Oracle(ReadOnlySpan<ushort> model, int length, OperationContext context)
    {
        var symbols = Symbols(length); var frequencies = new long[256];
        foreach (var symbol in Symbols(63).Where(s => s >= 0)) frequencies[symbol] = 1;
        for (var i = 0; i < symbols.Length; i++) if (symbols[i] >= 0) frequencies[symbols[i]] += model[i];
        var table = JpegHuffmanBuilder.Build(frequencies, context);
        var expected = symbols.Where(s => s >= 0).ToDictionary(s => s, s => table.Lengths[s] * 256);
        long blocks = model[16], bits = 0;
        foreach (var symbol in symbols.Where(s => s >= 0 && s != 240 && (s & 15) == 0))
        {
            var count = frequencies[symbol] - 1; var exponent = symbol >> 4;
            bits += count * (table.Lengths[symbol] + exponent);
        }
        if (blocks != 0) expected[0] = (int)((bits * 256 + blocks / 2) / blocks);
        return expected;
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void SmoothedSourceLengthsAndAmortizedEobMatchSeparateCanonicalBuilder(int pattern)
    {
        var symbols = Symbols(63); var counts = new ushort[354];
        for (var group = 0; group < 2; group++)
        {
            var model = counts.AsSpan(group * 177, 177);
            if (pattern == 1) model[17 + group] = 8064;
            if (pattern == 2) { model[17..].Fill(49); model[0] = model[16] = 32; }
            if (pattern == 3) for (var i = 17; i < 177; i++) model[i] = (ushort)((i * 17 + group) % 47);
            if (pattern == 4) for (var i = 0; i < 12; i++) model[17 + i + group] = (ushort)((1 << i) - 1);
            if (pattern == 5) { model[0] = model[16] = 64; model[17 + group] = 128; }
            if (pattern == 6) { model[7] = (ushort)(group + 1); model[16] = (ushort)((group + 1) * 64); } // separate runs of64, pooled chroma only
            if (pattern == 7) { model[0] = model[2] = model[3] = 1; model[16] = 7; }
        }
        using var context = new OperationContext(ImageLimits.Default, default);
        var expected = new[] { Oracle(counts.AsSpan(0, 177), 63, context), Oracle(counts.AsSpan(177, 177), 63, context) };
        var beforeWork = context.ConsumedWork;
        JpegEncoder.FinishSourceBandPrices(counts, default, context);
        var offsets = new ushort[128]; JpegEncoder.SourcePriceOffsets444(default, offsets);
        var prices = new JpegEncoder.JointAcPrices(counts, offsets);
        for (var group = 0; group < 2; group++)
        {
            var model = prices.ForComponent(group);
            foreach (var symbol in symbols.Where(s => s >= 0))
            {
                var value = model.At(symbol, 1);
                Assert.Equal(expected[group][symbol], value);
                Assert.InRange(value, 1, 4096);
                if (symbol != 0) Assert.Equal(0, value % 256);
            }
            if (pattern == 6) Assert.InRange(model.At(0, 1), 1, 255);
            if (pattern == 7) Assert.NotEqual(0, model.At(0, 1) % 256);
        }
        Assert.Equal(90112, context.ConsumedWork - beforeWork);
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
                var symbols = Symbols(length); var model = counts.AsSpan(at, symbols.Length);
                for (var i = 17; i < symbols.Length; i++) model[i] = (ushort)((i * 7 + first + group) % 19);
                if (length > 16) model[1] = 3;
                model[0] = (ushort)(16 + group); model[2] = 4; model[3] = 2; model[16] = (ushort)(32 + group);
                foreach (var (symbol, value) in Oracle(model, length, oracle)) expected[(group, first, symbol)] = value;
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
            Assert.InRange(shape.Slots, 52, JpegEncoder.SourcePriceCapacity);
            JpegEncoder.SourcePriceOffsets444(actual, offsets);
        }
        Assert.Equal(900, maximum);
        // MCU2304 + arena1860 + descriptors256 + pending192 + builder2532 + pixels64 + levels128.
        Assert.Equal(7336, 2304 + JpegEncoder.SourcePriceCapacity * 2 + 128 * 2 + 192 + 2532 + 64 + 128);
        Assert.True(7336 < 8192);
        var control = JpegEncoder.SourcePriceShape444(JpegEncoder.PlannedSourcePriceBands444(69));
        Assert.Equal(613, control.Slots); Assert.Equal(405504, control.FinishWork); Assert.Equal(13, control.ComponentBands);
    }

    [Fact]
    public void SharedChromaDoesNotJoinEobRunsAcrossComponents()
    {
        var counts = new ushort[354]; var offsets = new ushort[128]; var pending = new byte[192]; var values = new int[64];
        JpegEncoder.SourcePriceOffsets444(default, offsets);
        for (var sample = 0; sample < 64; sample++)
        for (var component = 0; component < 3; component++)
            JpegEncoder.CountSourceBandAc(values, counts, offsets.AsSpan(component == 0 ? 0 : 64, 64), pending.AsSpan(component * 64, 64), 0);
        Assert.All(counts.Where((x, i) => i % 177 != 16), x => Assert.Equal(0, x));
        Assert.Equal(64, counts[16]); Assert.Equal(128, counts[177 + 16]);
        for (var component = 0; component < 3; component++)
            JpegEncoder.FlushSourceBandEobRuns(counts, offsets.AsSpan(component == 0 ? 0 : 64, 64), pending.AsSpan(component * 64, 64), 0);
        Assert.Equal(1, counts[7]); Assert.Equal(2, counts[177 + 7]); // EOBRUN exponent6
        Assert.Equal(0, counts[177 + 8]); Assert.All(pending, x => Assert.Equal(0, x));
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.FinishSourceBandPrices(counts, default, context);
    }

    [Fact]
    public void NonzeroPrefixFlushesPreviousRunAndTrailingZerosStartAnother()
    {
        var counts = new ushort[354]; var offsets = new ushort[128]; var pending = new byte[64]; var values = new int[64];
        JpegEncoder.SourcePriceOffsets444(default, offsets);
        JpegEncoder.CountSourceBandAc(values, counts, offsets.AsSpan(0, 64), pending, 0);
        JpegEncoder.CountSourceBandAc(values, counts, offsets.AsSpan(0, 64), pending, 0);
        values[JpegTransform.Zigzag[1]] = 1;
        JpegEncoder.CountSourceBandAc(values, counts, offsets.AsSpan(0, 64), pending, 0);
        values[JpegTransform.Zigzag[1]] = 0; values[JpegTransform.Zigzag[63]] = 1;
        JpegEncoder.CountSourceBandAc(values, counts, offsets.AsSpan(0, 64), pending, 0);
        JpegEncoder.FlushSourceBandEobRuns(counts, offsets.AsSpan(0, 64), pending, 0);
        Assert.Equal(1, counts[2]); Assert.Equal(1, counts[0]); // run2 and run1
        Assert.Equal(3, counts[1]); Assert.Equal(1, counts[17]); Assert.Equal(1, counts[157]);
        Assert.Equal(3, counts[16]); Assert.Equal(10, counts.Sum(x => (int)x)); Assert.All(pending, x => Assert.Equal(0, x));
    }

    [Fact]
    public void ThreeBlockRunUsesItsActualLengthForAmortization()
    {
        var counts = new ushort[354]; var offsets = new ushort[128]; var pending = new byte[64]; var values = new int[64];
        JpegEncoder.SourcePriceOffsets444(default, offsets);
        for (var block = 0; block < 3; block++)
            JpegEncoder.CountSourceBandAc(values, counts, offsets.AsSpan(0, 64), pending, 0);
        JpegEncoder.FlushSourceBandEobRuns(counts, offsets.AsSpan(0, 64), pending, 0);
        Assert.Equal(1, counts[2]); Assert.Equal(3, counts[16]); // exponent1, actual length3
        using var oracle = new OperationContext(ImageLimits.Default, default);
        var expected = Oracle(counts.AsSpan(0, 177), 63, oracle)[0];
        using var context = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.FinishSourceBandPrices(counts, default, context);
        Assert.Equal(expected, counts[0]); Assert.Equal(683, counts[0]);
    }

    [Fact]
    public void PriceWorkRefusalPrecedesHistogramMutationAndNextOperationSucceeds()
    {
        var counts = new ushort[354]; counts[0] = counts[16] = 64; var before = counts.ToArray();
        using var context = new OperationContext(ImageLimits.Default with { MaxWork = 90111 }, default);
        var error = Assert.Throws<ImageLimitException>(() => JpegEncoder.FinishSourceBandPrices(counts, default, context));
        Assert.Equal(ImageLimitKind.Work, error.Limit); Assert.Equal(before, counts); Assert.Equal(0, context.ConsumedWork);
        using var next = new OperationContext(ImageLimits.Default, default);
        JpegEncoder.FinishSourceBandPrices(counts, default, next); Assert.InRange(counts[0], 1, 4096);
    }

    [Theory]
    [InlineData(0, 65535)] [InlineData(17, 8065)] [InlineData(194, 8065)]
    [InlineData(0, 65)] [InlineData(2, 33)] [InlineData(8, 1)]
    public void InvalidHistogramDoesNotChangeAnyModel(int index, int value)
    {
        var counts = new ushort[354]; counts[17] = 64; counts[index] = (ushort)value; var before = counts.ToArray();
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
        var counts = new ushort[354]; counts[17] = counts[18] = 4033; var before = counts.ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegEncoder.FinishSourceBandPrices(counts, default, context)); Assert.Equal(before, counts);
    }

    [Fact]
    public void CancellationPrecedesPriceMutation()
    {
        var counts = new ushort[354]; counts[0] = counts[16] = 64; var before = counts.ToArray();
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
