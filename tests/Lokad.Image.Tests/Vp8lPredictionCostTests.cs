using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lPredictionCostTests
{
    // Independent expression of the four-plane bias and two Shannon terms.
    // Dense summation remains independent of sparse-mask traversal.
    private static long Expected(int[] current, int[] previous)
    {
        long result = 0;
        for (var plane = 0; plane < 4; plane++)
        {
            var offset = plane * 256;
            var weight = 94 * Vp8lReferenceCost.Scale;
            var bias = current[offset] * Vp8lReferenceCost.Scale;
            for (var distance = 1; distance < 16; distance++)
            {
                bias += (weight * (current[offset + distance] + current[offset + 256 - distance]) + 50) / 100;
                weight = (weight * 6 + 5) / 10;
            }
            var counts = Enumerable.Range(offset, 256);
            var sumCurrent = checked((int)counts.Sum(i => (long)current[i]));
            var sumCombined = checked((int)counts.Sum(i => (long)current[i] + previous[i]));
            result += -(bias + 5) / 10
                + Vp8lReferenceCost.SLog(sumCurrent) + Vp8lReferenceCost.SLog(sumCombined)
                - counts.Sum(i => Vp8lReferenceCost.SLog(current[i]))
                - counts.Sum(i => Vp8lReferenceCost.SLog(current[i] + previous[i]));
        }
        return result;
    }

    internal static long PreparedCost(ReadOnlySpan<int> current, ReadOnlySpan<int> previous, OperationContext context)
    {
        Span<long> products = stackalloc long[1024]; Span<long> baselines = stackalloc long[4];
        Vp8lReferenceCost.PreparePrediction(current, previous, products, baselines, context);
        return Vp8lReferenceCost.Prediction(current, previous, products, baselines, context);
    }

    [Fact]
    public void GeneralFourPlaneHistogramsKeepIndependentCostsAndBorrowedInputs()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var current = new int[1024]; var previous = new int[1024]; uint state = 0x6d2b79f5;
        int[] thresholds = [1, 255, 256, 257, 4095, 4096, 65535, 65536, 65537];
        for (var sample = 0; sample < 512; sample++)
        {
            for (var i = 0; i < current.Length; i++)
            {
                current[i] = Next(sample % 5); previous[i] = Next((sample / 5) % 5);
            }
            var before = current.ToArray(); var prior = previous.ToArray();
            Assert.Equal(Expected(current, previous), PreparedCost(current, previous, context));
            Assert.Equal(before, current); Assert.Equal(prior, previous);
        }
        int Next(int density)
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            if ((state & 15) >= density * 4) return 0;
            return thresholds[(state >> 8) % (uint)thresholds.Length];
        }
    }

    [Fact]
    public void MaskEdgesAndUnalignedSlicesPreserveFullWidthCounts()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        foreach (var edge in new[] { 0, 1, 7, 8, 15, 16, 31, 32, 127, 128, 254, 255 })
        foreach (var plane in Enumerable.Range(0, 4))
        foreach (var currentOnly in new[] { false, true })
        {
            var current = new int[1024]; var previous = new int[1024];
            current[plane * 256 + edge] = currentOnly ? 1 << 27 : 0;
            previous[plane * 256 + edge] = currentOnly ? 0 : 1 << 27;
            current[plane * 256 + ((edge + 1) & 255)] = 257;
            var a = Enumerable.Repeat(12345, 1026).ToArray(); var b = a.ToArray();
            current.CopyTo(a, 1); previous.CopyTo(b, 1);
            Assert.Equal(Expected(current, previous),
                PreparedCost(a.AsSpan(1, 1024), b.AsSpan(1, 1024), context));
            Assert.Equal(12345, a[0]); Assert.Equal(12345, b[^1]);
        }
    }

    [Fact]
    public void OnePreparedHistoryPricesFourteenDifferentCandidatesWithoutChangingBorrowedStorage()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var previous = new int[1024];
        for (var i = 0; i < previous.Length; i++) previous[i] = i % 7 == 0 ? 65536 : i % 7 == 1 ? 257 : 0;
        var original = previous.ToArray();
        foreach (var total in new[] { 0, 1, 64, 257, 65536, 1 << 27 })
        {
            var candidates = new int[14][];
            for (var mode = 0; mode < 14; mode++)
            {
                var candidate = candidates[mode] = new int[1024];
                for (var plane = 0; plane < 4; plane++)
                {
                    var at = plane * 256 + ((mode * 17 + plane * 3) & 255);
                    var first = (int)((long)total * mode / 14);
                    candidate[at] = first; candidate[plane * 256 + ((at + 1) & 255)] = total - first;
                }
            }
            var products = Enumerable.Repeat(-31337L, 1026).ToArray();
            var baselines = Enumerable.Repeat(-31337L, 6).ToArray();
            Vp8lReferenceCost.PreparePrediction(candidates[0], previous, products.AsSpan(1, 1024), baselines.AsSpan(1, 4), context);
            var productSnapshot = products.ToArray(); var baselineSnapshot = baselines.ToArray();
            foreach (var candidate in candidates)
            {
                var before = candidate.ToArray();
                Assert.Equal(Expected(candidate, previous), Vp8lReferenceCost.Prediction(candidate, previous,
                    products.AsSpan(1, 1024), baselines.AsSpan(1, 4), context));
                Assert.Equal(before, candidate);
            }
            Assert.Equal(original, previous); Assert.Equal(productSnapshot, products); Assert.Equal(baselineSnapshot, baselines);
            Assert.Equal(-31337, products[0]); Assert.Equal(-31337, products[^1]);
            Assert.Equal(-31337, baselines[0]); Assert.Equal(-31337, baselines[^1]);
        }
    }

    [Fact]
    public void ScoringAllocatesNothingAndCancellationPrecedesInputAccess()
    {
        var current = new int[1024]; var previous = new int[1024];
        for (var i = 0; i < 1024; i += 17) { current[i] = i + 1; previous[1023 - i] = 65536; }
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        long checksum = 0;
        for (var i = 0; i < 512; i++) checksum ^= PreparedCost(current, previous, context);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 512; i++) checksum ^= PreparedCost(current, previous, context);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before); Assert.Equal(0, checksum);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.GrowthAttempts); Assert.False(root.IsDisposed);
        using var cancellation = new CancellationTokenSource();
        using var canceled = new OperationContext(ImageLimits.Default, cancellation.Token); cancellation.Cancel();
        var error = Assert.Throws<OperationCanceledException>(() => Vp8lReferenceCost.Prediction([], [], [], [], canceled));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceCost.PreparePrediction([], [], [], [], canceled));
    }
}
