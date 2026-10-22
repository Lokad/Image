namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    private const int SourceHuffmanFinishWork = 45056;

    private static bool SourcePriceSymbol(int symbol)
        => (symbol & 15) is >= 0 and <= 10;

    private static int SourcePriceCount(ReadOnlySpan<ushort> model, int symbol)
    {
        var at = SourcePriceSlot(symbol);
        return at < model.Length ? model[at] : 0;
    }

    private static void SourceHuffmanPrices(Span<ushort> model)
    {
        // Source-only T.81 Annex K construction: all 176 progressive AC/EOB-run
        // symbols receive +1; a dummy reserves the all-ones code. Total <=8241,
        // depth <=27. Leaf IDs preserve symbol order, before internal-node ties.
        // 2532 stack bytes sequentially; source MCU, arena, map, pending runs and
        // pixels bring simultaneous substantial scratch to 7208 B, within 8 KiB.
        Span<ushort> weights = stackalloc ushort[353], parents = stackalloc ushort[353];
        Span<ushort> heap = stackalloc ushort[177], symbols = stackalloc ushort[177];
        Span<ushort> order = stackalloc ushort[176], eobCounts = stackalloc ushort[15];
        Span<byte> counts = stackalloc byte[28];
        parents.Fill(ushort.MaxValue); counts.Clear();
        var leaves = 0; var pending = 0;
        for (var symbol = 0; symbol < 256; symbol++)
        {
            if (!SourcePriceSymbol(symbol)) continue;
            weights[leaves] = (ushort)(SourcePriceCount(model, symbol) + 1); symbols[leaves] = (ushort)symbol;
            PushSourcePrice(heap, ref pending, weights, (ushort)leaves++);
        }
        weights[leaves] = 1; PushSourcePrice(heap, ref pending, weights, (ushort)leaves++);
        var next = leaves; var ordered = 0;
        while (pending > 1)
        {
            var first = PopSourcePrice(heap, ref pending, weights);
            var second = PopSourcePrice(heap, ref pending, weights);
            if (first < 176) order[ordered++] = first;
            if (second < 176) order[ordered++] = second;
            weights[next] = (ushort)(weights[first] + weights[second]);
            parents[first] = parents[second] = (ushort)next;
            PushSourcePrice(heap, ref pending, weights, (ushort)next++);
        }
        if (ordered != 176) throw new InvalidOperationException("Source Huffman leaf count differs.");
        var maximumDepth = 0;
        for (var leaf = 0; leaf < leaves; leaf++)
        {
            var depth = 0; var node = leaf;
            while (parents[node] != ushort.MaxValue) { node = parents[node]; depth++; }
            counts[depth]++; maximumDepth = Math.Max(maximumDepth, depth);
        }
        // The 45056-unit finish includes validation, <=705*8 heap visits,
        // 177*27 parent visits, <=11*88 redistributions with <=25 searches,
        // their fixed updates, setup/output and the linear leaf-order passes.
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
        // Positive weights make successive heap pops nondecreasing. Reverse the
        // leaf sequence, then each equal-weight group: descending frequency with
        // ascending-symbol ties, without the former quadratic insertion sort.
        order.Reverse();
        for (var first = 0; first < order.Length;)
        {
            var end = first + 1;
            while (end < order.Length && weights[order[end]] == weights[order[first]]) end++;
            order[first..end].Reverse(); first = end;
        }
        var eobBlocks = (int)model[16];
        for (var exponent = 0; exponent < 15; exponent++)
        {
            eobCounts[exponent] = model[SourcePriceSlot(exponent << 4)];
        }
        model.Clear(); var assigned = 0; var code = 0; var eobBits = 0;
        for (var width = 1; width <= 16; width++)
        {
            if (code + counts[width] >= 1 << width) throw new InvalidOperationException("Source Huffman all-ones code is not reserved.");
            for (var index = 0; index < counts[width]; index++)
            {
                var symbol = symbols[order[assigned++]]; var at = SourcePriceSlot(symbol);
                if (at < model.Length) model[at] = (ushort)(width * 256);
                if ((symbol & 15) == 0 && symbol != 240)
                {
                    var exponent = symbol >> 4;
                    eobBits += eobCounts[exponent] * (width + exponent);
                }
            }
            code = (code + counts[width]) << 1;
        }
        if (assigned != 176) throw new InvalidOperationException("Source Huffman symbol count differs.");
        if (eobBlocks != 0) model[0] = (ushort)((eobBits * 256 + eobBlocks / 2) / eobBlocks);
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
