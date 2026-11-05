namespace Lokad.Image.Internal;

// Only ALPH predictor histograms: alpha bins0/1 and singleton red/blue.
// Recursive merging/history preserve this invariant; all storage stays borrowed.
internal static partial class Vp8lReferenceCost
{
    internal static long PredictionGreen(ReadOnlySpan<int> current, ReadOnlySpan<int> previous, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        // Red/blue each have a single nonzero bin: entropy cancels exactly.
        var cost = -(current[256] * Scale + 5) / 10 - (current[768] * Scale + 5) / 10;
        var zero = current[0]; var one = current[1];
        var priorZero = previous[0]; var priorOne = previous[1];
        var weighted = zero * Scale + (94 * Scale * one + 50) / 100;
        cost -= (weighted + 5) / 10;
        cost -= SLog(zero) + SLog(zero + priorZero) + SLog(one) + SLog(one + priorOne);
        cost += SLog(zero + one) + SLog(zero + one + priorZero + priorOne);
        var green = current.Slice(512, 256); var prior = previous.Slice(512, 256);
        return cost + ResidualBias(green, 1, PredictionBiasWeights) + PredictionEntropy(green, prior);
    }

    // All fourteen modes see one fixed history and only two alpha populations.
    // Products borrow the unused padded histogram tail until this visit ends.
    internal static (long Black, long Other) PrepareGreen(ReadOnlySpan<int> previous,
        int samples, int blackOnes, int originOnes, Span<long> products, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        var baseline = Vp8lColorCost.Prepare(previous[..256], samples, products);
        // Red/blue each have a single nonzero bin: entropy cancels exactly.
        // Keep their residual-bias rounding once per plane.
        var singletonCost = -(samples * Scale + 5) / 10;
        baseline += singletonCost + singletonCost;
        return (baseline + AlphaCost(samples - blackOnes, blackOnes, previous[256], previous[257]),
            baseline + AlphaCost(samples - originOnes, originOnes, previous[256], previous[257]));
    }

    private static long AlphaCost(int zero, int one, int priorZero, int priorOne)
    {
        var weighted = zero * Scale + (94 * Scale * one + 50) / 100;
        var cost = -(weighted + 5) / 10;
        cost -= SLog(zero) + SLog(zero + priorZero) + SLog(one) + SLog(one + priorOne);
        cost += SLog(zero + one) + SLog(zero + one + priorZero + priorOne);
        return cost;
    }

    internal static long PredictionGreen(ReadOnlySpan<int> green, ReadOnlySpan<int> previous,
        ReadOnlySpan<long> products, long baseline, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        return ResidualBias(green, 1, PredictionBiasWeights)
            + PreparedPredictionEntropy(green, previous, products, baseline);
    }
}
