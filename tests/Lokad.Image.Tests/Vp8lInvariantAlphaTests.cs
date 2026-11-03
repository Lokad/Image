using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lInvariantAlphaTests
{
    [Theory]
    [InlineData(1)] [InlineData(64)] [InlineData(65536)] [InlineData(262144)]
    public void SharedAlphaAndRgbKeepEveryNumericScoreAgainstTheIndependentDenseOracle(int samples)
    {
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var histogram = new int[14 * 1024]; var history = new int[1024];
        // Prior tiles need not be opaque; only the actual current histograms
        // establish the reusable plane. Include medium and logarithmic terms.
        for (var i = 0; i < 1024; i++) history[i] = i % 7 == 0 ? 65536 : i % 7 == 1 ? 257 : 0;
        for (var mode = 0; mode < 14; mode++)
        {
            histogram[mode * 1024] = samples;
            for (var plane = 1; plane < 4; plane++)
            {
                var offset = mode * 1024 + plane * 256;
                var at = (mode * 17 + plane * 3) & 255;
                var first = (int)((long)samples * mode / 14);
                histogram[offset + at] = first;
                histogram[offset + ((at + 1) & 255)] = samples - first;
            }
        }
        var original = histogram.ToArray(); var prior = history.ToArray();
        var products = Enumerable.Repeat(-31337L, 1026).ToArray();
        var baselines = Enumerable.Repeat(-31337L, 6).ToArray();
        var prepared = products.AsSpan(1, 1024); var bases = baselines.AsSpan(1, 4);
        Vp8lReferenceCost.PreparePrediction(histogram.AsSpan(0, 1024), history, prepared, bases, context);
        var alpha = Vp8lReferenceCost.Prediction(histogram.AsSpan(0, 256), history.AsSpan(0, 256), prepared[..256], bases[..1], context);
        Assert.True(Vp8lReferencePredictor.HasSharedZeroAlpha(histogram, samples));
        Assert.Equal(Vp8lPredictionCostTests.Expected(histogram[..256], history[..256]), alpha);
        for (var mode = 0; mode < 14; mode++)
        {
            var candidate = histogram.AsSpan(mode * 1024, 1024);
            var rgb = Vp8lReferenceCost.Prediction(candidate[256..], history.AsSpan(256), prepared[256..], bases[1..], context);
            Assert.Equal(Vp8lPredictionCostTests.Expected(candidate[256..].ToArray(), history[256..]), rgb);
            var expected = Vp8lPredictionCostTests.Expected(candidate.ToArray(), history);
            Assert.Equal(expected, Vp8lReferenceCost.Prediction(candidate, history, prepared, bases, context));
            Assert.Equal(expected, alpha + rgb);
        }
        Assert.Equal(original, histogram); Assert.Equal(prior, history);
        Assert.Equal(-31337, products[0]); Assert.Equal(-31337, products[^1]);
        Assert.Equal(-31337, baselines[0]); Assert.Equal(-31337, baselines[^1]);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts);
    }

    [Fact]
    public void EveryModeMustHaveZeroAlphaResiduals()
    {
        var histogram = new int[14 * 1024];
        for (var mode = 0; mode < 14; mode++) histogram[mode * 1024] = 17;
        Assert.True(Vp8lReferencePredictor.HasSharedZeroAlpha(histogram, 17));
        for (var mode = 0; mode < 14; mode++)
        {
            histogram[mode * 1024]--; histogram[mode * 1024 + 1]++;
            Assert.False(Vp8lReferencePredictor.HasSharedZeroAlpha(histogram, 17));
            histogram[mode * 1024]++; histogram[mode * 1024 + 1]--;
        }
    }

    [Fact]
    public void LocallyOpaqueTileWithTranslucentNeighboursUsesTheFullCost()
    {
        const int width = 9, height = 7;
        var pixels = Enumerable.Repeat(0xff123456u, width * height).ToArray();
        for (var y = 0; y < height; y++) pixels[y * width + 3] &= 0x00ffffff;
        for (var x = 0; x < width; x++) pixels[3 * width + x] &= 0x7fffffff;
        using var context = new OperationContext(ImageLimits.Default, default);
        var histogram = new int[14 * 1024];
        Vp8lPredictorHistogram.Build(pixels, width, height, 4, 4, 2, histogram, context);
        Assert.Equal(12, histogram[0]); // Mode zero alone would admit this tile.
        Assert.True(histogram[1024] < 12);
        Assert.False(Vp8lReferencePredictor.HasSharedZeroAlpha(histogram, 12));
    }

    [Fact]
    public void ClippedLeafAndAggregatedParentHistogramsUseTheirActualSampleCounts()
    {
        const int width = 9, height = 7;
        var pixels = Enumerable.Range(0, width * height).Select(i => 0xff000000u | (uint)(i * 1031)).ToArray();
        using var context = new OperationContext(ImageLimits.Default, default);
        var leaf = new int[14 * 1024]; var parent = new int[14 * 1024];
        for (var y = 0; y < height; y += 4) for (var x = 0; x < width; x += 4)
        {
            Array.Clear(leaf);
            Vp8lPredictorHistogram.Build(pixels, width, height, x, y, 2, leaf, context);
            var samples = Math.Min(4, width - x) * Math.Min(4, height - y);
            Assert.True(Vp8lReferencePredictor.HasSharedZeroAlpha(leaf, samples));
            for (var i = 0; i < leaf.Length; i++) parent[i] += leaf[i];
        }
        Assert.True(Vp8lReferencePredictor.HasSharedZeroAlpha(parent, width * height));
    }
}
