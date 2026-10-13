namespace Lokad.Image.Internal;

// Original T.81 Annex C/K canonical encoder tables, limited to 16 bits.
internal sealed record JpegEncodingTable(byte[] Counts, byte[] Symbols, ushort[] Codes, byte[] Lengths);

internal static class JpegHuffmanBuilder
{
    internal static JpegEncodingTable Build(ReadOnlySpan<long> histogram, OperationContext context)
    {
        if (histogram.Length != 256) throw new ArgumentException("Expected a byte-symbol histogram.");
        context.Work(65536);
        var frequencies = histogram.ToArray(); var active = new List<int>(); long total = 1;
        var queue = new PriorityQueue<int, (long Weight, int Order)>();
        for (var symbol = 0; symbol < 256; symbol++)
        {
            if (frequencies[symbol] < 0) throw new ArgumentOutOfRangeException(nameof(histogram));
            if (frequencies[symbol] == 0) continue;
            total = checked(total + frequencies[symbol]); active.Add(symbol);
            queue.Enqueue(symbol, (frequencies[symbol], symbol));
        }
        if (active.Count == 0) throw new ArgumentException("At least one symbol is required.");
        // One reserved leaf prevents an all-ones data code. Its value is never
        // emitted, and the longest canonical slot is removed after limiting.
        queue.Enqueue(256, (1, 256));
        Span<int> left = stackalloc int[513], right = stackalloc int[513]; var next = 257;
        while (queue.Count > 1)
        {
            queue.TryDequeue(out var first, out var firstPriority);
            queue.TryDequeue(out var second, out var secondPriority);
            left[next] = first; right[next] = second;
            queue.Enqueue(next, (checked(firstPriority.Weight + secondPriority.Weight), next)); next++;
        }
        Span<int> counts = stackalloc int[257]; counts.Clear();
        Span<int> nodes = stackalloc int[513], depths = stackalloc int[513];
        nodes[0] = queue.Dequeue(); depths[0] = 0; var pending = 1; var maximumDepth = 0;
        while (pending > 0)
        {
            pending--; var node = nodes[pending]; var depth = depths[pending];
            if (node < 257) { counts[depth]++; maximumDepth = Math.Max(maximumDepth, depth); }
            else
            {
                nodes[pending] = left[node]; depths[pending] = depth + 1; pending++;
                nodes[pending] = right[node]; depths[pending] = depth + 1; pending++;
            }
        }
        // Rebalance the full binary tree's length counts while retaining Kraft
        // equality. This is length-limited adaptive coding, not a claim of the
        // globally optimal length-limited weighted cost.
        for (var depth = maximumDepth; depth > 16; depth--)
            while (counts[depth] > 0)
            {
                if (counts[depth] < 2) throw new InvalidOperationException("Invalid full-tree leaf counts.");
                var shorter = depth - 2;
                while (shorter > 0 && counts[shorter] == 0) shorter--;
                if (shorter == 0) throw new InvalidOperationException("No admissible shorter prefix.");
                counts[depth] -= 2; counts[depth - 1]++;
                counts[shorter]--; counts[shorter + 1] += 2;
            }
        var longest = Math.Min(16, maximumDepth);
        while (counts[longest] == 0) longest--;
        counts[longest]--;
        active.Sort((a, b) => { var order = frequencies[b].CompareTo(frequencies[a]); return order == 0 ? a.CompareTo(b) : order; });
        var encodedCounts = new byte[16]; var symbols = new byte[active.Count];
        var codes = new ushort[256]; var lengths = new byte[256]; var code = 0; var at = 0;
        for (var width = 1; width <= 16; width++)
        {
            if (counts[width] > 255 || code + counts[width] >= 1 << width)
                throw new InvalidOperationException("Invalid canonical table or reserved all-ones code.");
            encodedCounts[width - 1] = (byte)counts[width];
            for (var index = 0; index < counts[width]; index++)
            {
                var symbol = active[at]; symbols[at++] = (byte)symbol;
                codes[symbol] = (ushort)code++; lengths[symbol] = (byte)width;
            }
            code <<= 1;
        }
        if (at != active.Count) throw new InvalidOperationException("Symbol count differs.");
        return new JpegEncodingTable(encodedCounts, symbols, codes, lengths);
    }
}
