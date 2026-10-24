namespace Lokad.Image.Internal;

// Only ALPH predictor histograms: alpha bins0/1 and singleton red/blue.
// Recursive merging/history preserve this invariant; all storage stays borrowed.
internal static partial class Vp8lReferenceCost
{
    internal static long PredictionGreen(ReadOnlySpan<int> current, ReadOnlySpan<int> previous, OperationContext context)
    {
        context.Work(1024);
        // Red/blue each have a single nonzero bin: entropy cancels exactly.
        var cost = -(current[256] * Scale + 5) / 10 - (current[768] * Scale + 5) / 10;
        var zero = current[0]; var one = current[1];
        var priorZero = previous[0]; var priorOne = previous[1];
        var weighted = zero * Scale + (94 * Scale * one + 50) / 100;
        cost -= (weighted + 5) / 10;
        cost -= SLog(zero) + SLog(zero + priorZero) + SLog(one) + SLog(one + priorOne);
        cost += SLog(zero + one) + SLog(zero + one + priorZero + priorOne);
        var green = current.Slice(512, 256); var prior = previous.Slice(512, 256);
        cost += SmallResidualBias(green);
        var sumCurrent = 0; var sumCombined = 0;
        for (var symbol = 0; symbol < 256; symbol++)
        {
            var count = green[symbol]; var combined = count + prior[symbol];
            sumCurrent += count; sumCombined += combined;
            cost -= SLog(count) + SLog(combined);
        }
        return cost + SLog(sumCurrent) + SLog(sumCombined);
    }
}
