namespace Lokad.Image.Internal;

internal static class Vp8lReferenceMainParsing
{
    internal static Vp8lTokenBuffer Encode(ReadOnlySpan<uint> pixels, ReadOnlySpan<uint> matches, int width,
        bool disableCache, int maximumCacheBits, OperationContext context, bool traceBackwards = true)
    {
        Validate(pixels.Length, matches.Length, width, maximumCacheBits);
        Vp8lTokenBuffer? standard = null, repeated = null, trace = null;
        try
        {
            standard = Vp8lReferenceLz.Greedy(pixels, matches, context);
            repeated = Vp8lReferenceLz.Repeats(pixels, width, context);
            var standardBits = disableCache ? 0 : Vp8lReferenceCache.BestBits(pixels, standard.Tokens, maximumCacheBits, context);
            var repeatedBits = disableCache ? 0 : Vp8lReferenceCache.BestBits(pixels, repeated.Tokens, maximumCacheBits, context);
            Vp8lReferenceCache.Apply(pixels, standard, standardBits, context);
            Vp8lReferenceCache.Apply(pixels, repeated, repeatedBits, context);
            var standardCost = Vp8lReferenceLz.Estimate(standard.Tokens, width, context, standardBits);
            var repeatedCost = Vp8lReferenceLz.Estimate(repeated.Tokens, width, context, repeatedBits);
            if (repeatedCost < standardCost) { var result = repeated; repeated = null; return result; }
            repeated.Dispose(); repeated = null;
            if (!traceBackwards) { var direct = standard; standard = null; return direct; }
            trace = Vp8lReferenceLz.Trace(pixels, matches, width, standard.Tokens, context, standardBits);
            if (Vp8lReferenceLz.Estimate(trace.Tokens, width, context, standardBits) < standardCost)
            { var result = trace; trace = null; return result; }
            var chosen = standard; standard = null; return chosen;
        }
        finally { standard?.Dispose(); repeated?.Dispose(); trace?.Dispose(); }
    }

    internal static Vp8lTokenBuffer EncodeBox(ReadOnlySpan<uint> pixels, ReadOnlySpan<uint> matches, int width,
        bool disableCache, int maximumCacheBits, OperationContext context, bool traceBackwards = true)
    {
        Validate(pixels.Length, matches.Length, width, maximumCacheBits);
        Vp8lTokenBuffer? seed = null, trace = null;
        try
        {
            seed = Vp8lReferenceLz.Greedy(pixels, matches, context);
            var bits = disableCache ? 0 : Vp8lReferenceCache.BestBits(pixels, seed.Tokens, maximumCacheBits, context);
            Vp8lReferenceCache.Apply(pixels, seed, bits, context);
            var cost = Vp8lReferenceLz.Estimate(seed.Tokens, width, context, bits);
            if (!traceBackwards) { var direct = seed; seed = null; return direct; }
            trace = Vp8lReferenceLz.Trace(pixels, matches, width, seed.Tokens, context, bits);
            if (Vp8lReferenceLz.Estimate(trace.Tokens, width, context, bits) < cost)
            { var result = trace; trace = null; return result; }
            var chosen = seed; seed = null; return chosen;
        }
        finally { seed?.Dispose(); trace?.Dispose(); }
    }

    private static void Validate(int count, int matches, int width, int maximumBits)
    {
        if (width is < 1 or > 16384 || count == 0 || count % width != 0 || count / width > 16384 || matches != count)
            throw new ArgumentException("Expected complete source/index rows.");
        if (maximumBits is < 0 or > 10) throw new ArgumentOutOfRangeException(nameof(maximumBits));
    }
}
