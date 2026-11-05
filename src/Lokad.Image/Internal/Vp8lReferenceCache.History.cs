using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

internal static partial class Vp8lReferenceCache
{
    internal static long HistoryBytes(int pixelCount, int maximumBits)
    {
        var entries = (1 << (maximumBits + 1)) - 2;
        return pixelCount + (entries + (long)HistogramValues(maximumBits)) * 4 + 128;
    }

    // Prefix folding has fixed O(bits * 2^bits) preparation. Keep short seeds
    // on direct counting; this choice precedes admission and uses no fixture or
    // shared remaining-capacity information.
    private static int PrefixMassWords(int tokenCount, int maximumBits)
    {
        if ((uint)maximumBits > 10) throw new ArgumentOutOfRangeException(nameof(maximumBits));
        return maximumBits >= 4 && tokenCount >= 8 * (1 << maximumBits) ? maximumBits * (1 << maximumBits) : 0;
    }

    internal static bool TryBestBitsWithHistory(ReadOnlySpan<uint> pixels, ReadOnlySpan<Vp8lToken> first,
        ReadOnlySpan<Vp8lToken> second, int maximumBits, OperationContext context, out int firstBits, out int secondBits,
        Vp8lTokenBuffer? scratch = null, bool greenOnly = false)
    {
        firstBits = secondBits = 0;
        // These cases need the original individual cancellation checks.
        if (maximumBits == 0 || context.Cancellation.CanBeCanceled) return false;
        var massWords = Math.Max(PrefixMassWords(first.Length, maximumBits), PrefixMassWords(second.Length, maximumBits));
        var reserved = HistoryBytes(pixels.Length, maximumBits) + (massWords == 0 ? 0 : massWords * 4L + 32);
        if (reserved > context.RemainingWorkingBytes)
            return false;
        // Eligibility uses only declared local allowances. Shared refusal below
        // is terminal and never selects a second, lower-memory attempt.
        var cacheWords = (1 << (maximumBits + 1)) - 2;
        var countWords = HistogramValues(maximumBits);
        var alignedPixels = checked((pixels.Length + 3) & ~3);
        var payloadBytes = checked(alignedPixels + (cacheWords + countWords + massWords) * 4);
        var workspace = scratch is null ? default : scratch.BorrowTailBytes(context, payloadBytes);
        var borrowed = !workspace.IsEmpty;
        if (borrowed && (workspace.Overlaps(MemoryMarshal.AsBytes(pixels))
            || workspace.Overlaps(MemoryMarshal.AsBytes(first)) || workspace.Overlaps(MemoryMarshal.AsBytes(second))))
            throw new ArgumentException("History scratch must not overlap live source or token samples.", nameof(scratch));
        context.Cancellation.ThrowIfCancellationRequested();
        if (!borrowed) context.Reserve(reserved);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested();
            Span<byte> minimum; Span<uint> cache; Span<int> counts, mass;
            if (borrowed)
            {
                minimum = workspace[..pixels.Length];
                var words = MemoryMarshal.Cast<byte, int>(workspace[alignedPixels..]);
                cache = MemoryMarshal.Cast<int, uint>(words[..cacheWords]);
                counts = words.Slice(cacheWords, countWords); mass = words[(cacheWords + countWords)..];
                // Fresh arrays were zeroed. Token tails contain arbitrary old
                // values; every other workspace region is initialized by users.
                cache.Clear();
            }
            else
            {
                minimum = new byte[pixels.Length]; cache = new uint[cacheWords];
                counts = new int[countWords]; mass = massWords == 0 ? Array.Empty<int>() : new int[massWords];
            }
            if (greenOnly) BuildForwardHistory(pixels, maximumBits, cache, minimum);
            else BuildHistory(pixels, maximumBits, cache, minimum);
            firstBits = ScoreHistory(pixels, first, maximumBits, minimum, counts, mass, context);
            secondBits = ScoreHistory(pixels, second, maximumBits, minimum, counts, mass, context);
            return true;
        }
        finally { if (!borrowed) context.Release(reserved); }
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

    // Normalized ALPH commonly hits small prefix widths. Discover the first hit
    // while updating missed cells, instead of searching then traversing again.
    // Full-color callers retain the maximum-width probe for cheap complete misses.
    internal static void BuildForwardHistory(ReadOnlySpan<uint> pixels, int maximumBits, Span<uint> cache, Span<byte> minimum)
    {
        if (maximumBits == 0) { minimum.Fill(1); return; }
        for (var position = 0; position < pixels.Length; position++)
        {
            var pixel = pixels[position]; var hash = unchecked(pixel * 0x1e35a7bdu);
            var index = (int)(hash >> 31); var bits = 1;
            for (; bits <= maximumBits; bits++)
            {
                if (cache[index] == pixel) break;
                cache[index] = pixel;
                hash <<= 1; index = 2 * index + 2 + (int)(hash >> 31);
            }
            // Higher prefix cells already contain pixel at the first hit.
            minimum[position] = (byte)bits;
        }
    }

    private static int ScoreHistory(ReadOnlySpan<uint> pixels, ReadOnlySpan<Vp8lToken> seed, int maximumBits,
        ReadOnlySpan<byte> minimum, Span<int> counts, Span<int> mass, OperationContext context)
    {
        FillAllFromHistory(pixels, seed, minimum, maximumBits, counts,
            PrefixMassWords(seed.Length, maximumBits) == 0 ? default : mass);
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
        ReadOnlySpan<byte> minimum, int maximumBits, Span<int> counts, Span<int> prefixMass = default)
    {
        if (!prefixMass.IsEmpty && (maximumBits == 0 || prefixMass.Length != maximumBits * (1 << maximumBits)))
            throw new ArgumentException("Prefix mass capacity differs from the cache alphabets.", nameof(prefixMass));
        counts[..HistogramValues(maximumBits)].Clear(); prefixMass.Clear();
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
                if (prefixMass.IsEmpty)
                {
                    for (var bits = threshold; bits <= maximumBits; bits++)
                        counts[offsets[bits] + 280 + (int)(hash >> (32 - bits))]++;
                }
                else if (threshold <= maximumBits)
                    prefixMass[((threshold - 1) << maximumBits) + (int)(hash >> (32 - maximumBits))]++;
            }
            position += token.Length;
        }
        if (position != pixels.Length) throw new ArgumentException("Token seed does not cover its source.", nameof(seed));
        if (!prefixMass.IsEmpty) FoldPrefixMass(prefixMass, maximumBits, offsets, counts);
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

    private static void FoldPrefixMass(ReadOnlySpan<int> mass, int maximumBits, ReadOnlySpan<int> offsets, Span<int> counts)
    {
        var size = 1 << maximumBits;
        // The maximum-width cache bank is initially zero and becomes the final
        // cumulative leaf population. It supplies scratch without another array.
        var leaves = counts.Slice(offsets[maximumBits] + 280, size);
        for (var bits = 1; bits <= maximumBits; bits++)
        {
            var row = mass.Slice((bits - 1) * size, size);
            for (var value = 0; value < size; value++) leaves[value] += row[value];
            if (bits == maximumBits) break;
            var output = counts.Slice(offsets[bits] + 280, 1 << bits); var shift = maximumBits - bits;
            for (var prefix = 0; prefix < output.Length; prefix++)
            {
                var sum = 0; var start = prefix << shift; var end = (prefix + 1) << shift;
                for (var value = start; value < end; value++) sum += leaves[value];
                output[prefix] = sum;
            }
        }
    }

}
