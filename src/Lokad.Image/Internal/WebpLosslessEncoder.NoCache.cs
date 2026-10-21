namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    // RFC9649 section3.6.1 permits an absent color cache. Its full literal
    // distribution must be fitted independently of the cached literal misses.
    private static CachedPlan SelectNoCachePlan(ImageBuffer image, int effort, uint[] cache,
        CachedPlan baseline, OperationContext context)
    {
        var tiles = ((image.Width + 7) >> 3) * ((image.Height + 7) >> 3);
        var work = (long)image.Width * image.Height * 4096 + tiles * 32768L + 1_000_000;
        // Select only from configured local allowances, before shared admission.
        // Raw/cache-heavy input keeps its original path; one predicted alternative
        // retains the same transforms and copy parse, with no capacity search.
        if (effort < 4 || !baseline.Predict || CachedWorkingBytes > context.RemainingWorkingBytes
            || work > context.RemainingWork) return baseline;
        var candidate = PrepareCachedTables(image, effort, cache, baseline.Predict, context,
            baseline.Modes, baseline.ModeTable, baseline.Color, useCache: false) with { Alpha = baseline.Alpha };
        candidate = SelectEntropyPlan(image, effort, cache, candidate, context);
        // Both plans share the predictor AND color owners. Only the losing
        // coding tables/group map end ownership at this comparison boundary.
        if (PayloadUpper(image, candidate, context) + 2 < PayloadUpper(image, baseline, context))
        {
            ReleaseCodingPlan(baseline, context); return candidate;
        }
        ReleaseCodingPlan(candidate, context); return baseline;
    }
}
