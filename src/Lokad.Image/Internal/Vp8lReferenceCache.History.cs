namespace Lokad.Image.Internal;

internal static partial class Vp8lReferenceCache
{
    internal static long HistoryBytes(int pixelCount, int maximumBits)
    {
        var maximum = new Vp8lHistogramLayout(maximumBits);
        var entries = (1 << (maximumBits + 1)) - 2;
        return pixelCount + (entries + (long)maximum.Total) * 4 + 128;
    }

    internal static long HistoryWorkBound(int pixels, int firstCount, int secondCount, int maximumBits)
    {
        var work = 4L * (firstCount + (long)secondCount) * (maximumBits + 1) + 2L * maximumBits * pixels;
        for (var bits = 0; bits <= maximumBits; bits++)
        {
            var layout = new Vp8lHistogramLayout(bits);
            work += 2L * layout.CacheSize + 6L * layout.Total;
        }
        return work;
    }

    internal static bool TryBestBitsWithHistory(ReadOnlySpan<uint> pixels, ReadOnlySpan<Vp8lToken> first,
        ReadOnlySpan<Vp8lToken> second, int maximumBits, OperationContext context, out int firstBits, out int secondBits)
    {
        firstBits = secondBits = 0;
        // These cases need the original individual work/cancellation checks.
        if (maximumBits == 0 || context.Cancellation.CanBeCanceled) return false;
        var reserved = HistoryBytes(pixels.Length, maximumBits);
        if (reserved > context.RemainingWorkingBytes
            || HistoryWorkBound(pixels.Length, first.Length, second.Length, maximumBits) > context.RemainingWork)
            return false;
        // Eligibility uses only declared local allowances. Shared refusal below
        // is terminal and never selects a second, lower-memory attempt.
        var maximum = new Vp8lHistogramLayout(maximumBits);
        context.Work(0); context.Reserve(reserved);
        try
        {
            context.Work(0);
            var minimum = new byte[pixels.Length];
            var cache = new uint[(1 << (maximumBits + 1)) - 2];
            var counts = new int[maximum.Total];
            BuildHistory(pixels, maximumBits, cache, minimum);
            firstBits = ScoreHistory(pixels, first, maximumBits, minimum, counts, context);
            secondBits = ScoreHistory(pixels, second, maximumBits, minimum, counts, context);
            return true;
        }
        finally { context.Release(reserved); }
    }

    // Each width has its own segment. Cache hits are monotone across widths:
    // larger prefix caches can only exclude collisions. Zero entries initially
    // represent pixel zero exactly as in the independent per-width caches.
    internal static void BuildHistory(ReadOnlySpan<uint> pixels, int maximumBits, Span<uint> cache, Span<byte> minimum)
    {
        if (maximumBits == 0) { minimum.Fill(1); return; }
        var maximumOffset = (1 << maximumBits) - 2;
        for (var position = 0; position < pixels.Length; position++)
        {
            var pixel = pixels[position]; var hash = unchecked(pixel * 0x1e35a7bdu);
            var threshold = maximumBits + 1;
            if (cache[maximumOffset + (int)(hash >> (32 - maximumBits))] == pixel)
            {
                var low = 1; var high = maximumBits;
                while (low < high)
                {
                    var middle = (low + high) / 2;
                    if (cache[(1 << middle) - 2 + (int)(hash >> (32 - middle))] == pixel) high = middle;
                    else low = middle + 1;
                }
                threshold = low;
            }
            minimum[position] = (byte)threshold;
            // Larger entries already contain this pixel; rewriting is a no-op.
            for (var bits = 1; bits < threshold; bits++)
                cache[(1 << bits) - 2 + (int)(hash >> (32 - bits))] = pixel;
        }
    }

    private static int ScoreHistory(ReadOnlySpan<uint> pixels, ReadOnlySpan<Vp8lToken> seed, int maximumBits,
        ReadOnlySpan<byte> minimum, Span<int> counts, OperationContext context)
    {
        var best = 0; var bestCost = long.MaxValue; long copied = 0;
        foreach (var token in seed) if (token.Code > 0) copied += token.Length;
        for (var bits = 0; bits <= maximumBits; bits++)
        {
            var layout = new Vp8lHistogramLayout(bits);
            context.Work(layout.CacheSize + layout.Total); counts[..layout.Total].Clear();
            // The exclusive noncancellable operation proved the full original
            // work upper fits before admission. Publish exactly those charges.
            context.Work(4L * seed.Length + (bits == 0 ? 0 : copied));
            FillFromHistory(pixels, seed, minimum, layout, counts);
            var cost = layout.Population(counts[..layout.Total], context);
            if (cost < bestCost) { bestCost = cost; best = bits; }
        }
        return best;
    }

    // Source/seeds/history stay borrowed and read-only. Counts are admitted and
    // cleared by the owner. Copy distances do not enter cache-size selection.
    internal static void FillFromHistory(ReadOnlySpan<uint> pixels, ReadOnlySpan<Vp8lToken> seed,
        ReadOnlySpan<byte> minimum, Vp8lHistogramLayout layout, Span<int> counts)
    {
        var position = 0;
        foreach (var token in seed)
        {
            if (token.Code > 0) counts[256 + Vp8lSymbols.Prefix(token.Code).Code]++;
            else
            {
                var pixel = pixels[position];
                if (layout.CacheBits != 0 && minimum[position] <= layout.CacheBits)
                    counts[280 + Key(pixel, layout.CacheBits)]++;
                else
                {
                    counts[(int)(pixel >> 8 & 255)]++; counts[layout.Red + (int)(pixel >> 16 & 255)]++;
                    counts[layout.Blue + (int)(pixel & 255)]++; counts[layout.Alpha + (int)(pixel >> 24)]++;
                }
            }
            position += token.Length;
        }
        if (position != pixels.Length) throw new ArgumentException("Token seed does not cover its source.", nameof(seed));
    }
}
