namespace Lokad.Image.Internal;

// Exact integer rearrangement of the RFC9649 color-transform search cost.
// The caller owns and admits the products; all spans remain borrowed.
internal static class Vp8lColorCost
{
    // Rearrange exact integer terms; no logarithm or rounding is changed.
    internal static long Prepare(ReadOnlySpan<int> history, int count, Span<long> products)
    {
        var total = 0; long cost = 0;
        for (var i = 0; i < 256; i++)
        {
            total += history[i];
            products[i] = Vp8lReferenceCost.SLog(history[i]);
            cost -= products[i];
        }
        return cost + Vp8lReferenceCost.SLog(count) + Vp8lReferenceCost.SLog(count + total);
    }

    internal static long Evaluate(ReadOnlySpan<int> histogram, ReadOnlySpan<int> history,
        ReadOnlySpan<long> products, long baseline, ReadOnlySpan<int> active)
    {
        var cost = baseline;
        foreach (var i in active)
        {
            var count = histogram[i];
            cost -= Vp8lReferenceCost.SLog(count) + Vp8lReferenceCost.SLog(count + history[i]) - products[i];
        }
        return cost + Vp8lReferenceCost.ResidualBias(histogram, 3, Vp8lReferenceCost.ColorBiasWeights);
    }
}
