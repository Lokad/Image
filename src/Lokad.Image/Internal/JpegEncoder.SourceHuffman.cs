namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    private static bool SourcePriceSymbol(int symbol)
        => symbol is 0 or 0xF0 || (symbol & 15) is >= 1 and <= 10;

    private static int SourcePriceCount(ReadOnlySpan<ushort> model, int symbol)
    {
        var at = SourcePriceSlot(symbol);
        return at < model.Length ? model[at] : 0;
    }

    private static void SourceHuffmanPrices(Span<ushort> model)
    {
        // Source-only T.81 Annex K length construction. The 162 legal AC symbols
        // receive +1 smoothing; one dummy leaf reserves the all-ones code.
        // Total weight <= 8064 + 163 = 8227. Dense leaf IDs preserve symbol-order
        // ties, and every internal node sorts after every leaf at equal weight.
        // Unreachable runs have implicit zero counts but retain their +1 prior.
        // 2116 stack bytes reused sequentially for all bands. The caller's
        // 2304-byte MCU, 1500-byte arena and 256-byte map fit the same 8 KiB.
        Span<ushort> weights = stackalloc ushort[325], parents = stackalloc ushort[325];
        Span<ushort> heap = stackalloc ushort[163], symbols = stackalloc ushort[163];
        Span<byte> counts = stackalloc byte[164];
        parents.Fill(ushort.MaxValue); counts.Clear();
        var leaves = 0; var pending = 0;
        for (var symbol = 0; symbol < 256; symbol++)
        {
            if (!SourcePriceSymbol(symbol)) continue;
            weights[leaves] = (ushort)(SourcePriceCount(model, symbol) + 1); symbols[leaves] = (ushort)symbol;
            PushSourcePrice(heap, ref pending, weights, (ushort)leaves++);
        }
        weights[leaves] = 1; PushSourcePrice(heap, ref pending, weights, (ushort)leaves++);
        var next = leaves;
        while (pending > 1)
        {
            var first = PopSourcePrice(heap, ref pending, weights);
            var second = PopSourcePrice(heap, ref pending, weights);
            weights[next] = (ushort)(weights[first] + weights[second]);
            parents[first] = parents[second] = (ushort)next;
            PushSourcePrice(heap, ref pending, weights, (ushort)next++);
        }
        var maximumDepth = 0;
        for (var leaf = 0; leaf < leaves; leaf++)
        {
            var depth = 0; var node = leaf;
            while (parents[node] != ushort.MaxValue) { node = parents[node]; depth++; }
            counts[depth]++; maximumDepth = Math.Max(maximumDepth, depth);
        }
        // Each grandparent has at least twice a node's weight. Total < 2^14
        // bounds depth by 27. 40960 fixed visits cover <=649*8 heap visits,
        // 163*27 parent visits, <=11*81 redistributions with <=25 searches
        // and fixed validation/initialization/output. Sorting is charged using
        // r*(323-r)/2, where at most r reachable symbols have nonzero counts.
        for (var depth = maximumDepth; depth > 16; depth--)
        while (counts[depth] > 0)
        {
            var shorter = depth - 2;
            while (shorter > 0 && counts[shorter] == 0) shorter--;
            if (counts[depth] < 2 || shorter == 0) throw new InvalidOperationException("Invalid source Huffman tree.");
            counts[depth] -= 2; counts[depth - 1]++;
            counts[shorter]--; counts[shorter + 1] += 2;
        }
        var longest = Math.Min(16, maximumDepth);
        while (counts[longest] == 0) longest--;
        counts[longest]--;
        // Stable frequency-descending/symbol-ascending order, before mutation.
        for (var index = 1; index < 162; index++)
        {
            var symbol = symbols[index]; var at = index;
            while (at > 0 && SourcePriceCount(model, symbols[at - 1]) < SourcePriceCount(model, symbol))
            { symbols[at] = symbols[at - 1]; at--; }
            symbols[at] = symbol;
        }
        model.Clear(); var assigned = 0; var code = 0;
        for (var width = 1; width <= 16; width++)
        {
            if (code + counts[width] >= 1 << width) throw new InvalidOperationException("Source Huffman all-ones code is not reserved.");
            for (var index = 0; index < counts[width]; index++)
            {
                var at = SourcePriceSlot(symbols[assigned++]);
                if (at < model.Length) model[at] = (ushort)(width * 256);
            }
            code = (code + counts[width]) << 1;
        }
        if (assigned != 162) throw new InvalidOperationException("Source Huffman symbol count differs.");
    }

    private static bool SourcePriceLess(ushort first, ushort second, ReadOnlySpan<ushort> weights)
        => weights[first] < weights[second] || weights[first] == weights[second] && first < second;

    private static void PushSourcePrice(Span<ushort> heap, ref int count, ReadOnlySpan<ushort> weights, ushort node)
    {
        var at = count++;
        while (at > 0)
        {
            var parent = (at - 1) / 2;
            if (!SourcePriceLess(node, heap[parent], weights)) break;
            heap[at] = heap[parent]; at = parent;
        }
        heap[at] = node;
    }

    private static ushort PopSourcePrice(Span<ushort> heap, ref int count, ReadOnlySpan<ushort> weights)
    {
        var result = heap[0]; var last = heap[--count]; var at = 0;
        while (at * 2 + 1 < count)
        {
            var child = at * 2 + 1;
            if (child + 1 < count && SourcePriceLess(heap[child + 1], heap[child], weights)) child++;
            if (!SourcePriceLess(heap[child], last, weights)) break;
            heap[at] = heap[child]; at = child;
        }
        heap[at] = last; return result;
    }
}
