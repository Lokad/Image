namespace Lokad.Image.Internal;

internal static partial class Vp8lReferenceCache
{
    internal static long HistoryBytes(int pixelCount, int maximumBits)
    {
        var entries = (1 << (maximumBits + 1)) - 2;
        return pixelCount + (entries + (long)HistogramValues(maximumBits)) * 4 + 128;
    }

    internal static bool TryBestBitsWithHistory(ReadOnlySpan<uint> pixels, ReadOnlySpan<Vp8lToken> first,
        ReadOnlySpan<Vp8lToken> second, int maximumBits, OperationContext context, out int firstBits, out int secondBits)
    {
        firstBits = secondBits = 0;
        // These cases need the original individual cancellation checks.
        if (maximumBits == 0 || context.Cancellation.CanBeCanceled) return false;
        var reserved = HistoryBytes(pixels.Length, maximumBits);
        if (reserved > context.RemainingWorkingBytes)
            return false;
        // Eligibility uses only declared local allowances. Shared refusal below
        // is terminal and never selects a second, lower-memory attempt.
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(reserved);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var minimum = new byte[pixels.Length];
            var cache = new uint[(1 << (maximumBits + 1)) - 2];
            var counts = new int[HistogramValues(maximumBits)];
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
        FillAllFromHistory(pixels, seed, minimum, maximumBits, counts);
        var best = 0; var bestCost = long.MaxValue;
        for (var bits = 0; bits <= maximumBits; bits++)
        {
            var layout = new Vp8lHistogramLayout(bits);
            context.Cancellation.ThrowIfCancellationRequested();
            var cost = layout.Population(counts.Slice(HistogramOffset(bits), layout.Total), context);
            if (cost < bestCost) { bestCost = cost; best = bits; }
        }
        return best;
    }

    internal static int HistogramOffset(int bits)
        => bits * new Vp8lHistogramLayout(0).Total + (bits == 0 ? 0 : (1 << bits) - 2);

    internal static int HistogramValues(int maximumBits)
        => HistogramOffset(maximumBits) + new Vp8lHistogramLayout(maximumBits).Total;

    // A literal misses precisely the widths below its first cache hit. Count
    // its channels once at the last miss, then form lower-width suffix sums.
    // Copy-length populations are independent of cache width.
    internal static void FillAllFromHistory(ReadOnlySpan<uint> pixels, ReadOnlySpan<Vp8lToken> seed,
        ReadOnlySpan<byte> minimum, int maximumBits, Span<int> counts)
    {
        counts[..HistogramValues(maximumBits)].Clear();
        Span<int> offsets = stackalloc int[11];
        for (var bits = 0; bits <= maximumBits; bits++) offsets[bits] = HistogramOffset(bits);
        var position = 0;
        foreach (var token in seed)
        {
            if (token.Code > 0) counts[256 + Vp8lSymbols.Prefix(token.Code).Code]++;
            else
            {
                var pixel = pixels[position]; var threshold = minimum[position];
                var lastMiss = threshold - 1; var layout = new Vp8lHistogramLayout(lastMiss);
                var offset = offsets[lastMiss];
                counts[offset + (int)(pixel >> 8 & 255)]++;
                counts[offset + layout.Red + (int)(pixel >> 16 & 255)]++;
                counts[offset + layout.Blue + (int)(pixel & 255)]++;
                counts[offset + layout.Alpha + (int)(pixel >> 24)]++;
                var hash = unchecked(pixel * 0x1e35a7bdu);
                for (var bits = threshold; bits <= maximumBits; bits++)
                    counts[offsets[bits] + 280 + (int)(hash >> (32 - bits))]++;
            }
            position += token.Length;
        }
        if (position != pixels.Length) throw new ArgumentException("Token seed does not cover its source.", nameof(seed));
        for (var bits = maximumBits - 1; bits >= 0; bits--)
        {
            var lower = new Vp8lHistogramLayout(bits); var upper = new Vp8lHistogramLayout(bits + 1);
            var a = counts.Slice(offsets[bits], lower.Total); var b = counts.Slice(offsets[bits + 1], upper.Total);
            for (var value = 0; value < 256; value++)
            {
                a[value] += b[value]; a[lower.Red + value] += b[upper.Red + value];
                a[lower.Blue + value] += b[upper.Blue + value]; a[lower.Alpha + value] += b[upper.Alpha + value];
            }
        }
        for (var bits = 1; bits <= maximumBits; bits++)
            counts.Slice(256, 24).CopyTo(counts.Slice(offsets[bits] + 256, 24));
    }
}
