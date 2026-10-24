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
        ReadOnlySpan<long> products, long baseline)
    {
        var cost = baseline;
        for (var i = 0; i < 256; i++)
        {
            var count = histogram[i];
            if (count != 0)
                cost -= Vp8lReferenceCost.SLog(count) + Vp8lReferenceCost.SLog(count + history[i]) - products[i];
        }
        var weighted = histogram[0] * 3 * Vp8lReferenceCost.Scale;
        var weight = 240 * Vp8lReferenceCost.Scale;
        for (var magnitude = 1; magnitude < 16; magnitude++)
        {
            weighted += (weight * (histogram[magnitude] + histogram[256 - magnitude]) + 50) / 100;
            weight = (weight * 6 + 5) / 10;
        }
        return cost - (weighted + 5) / 10;
    }
}

