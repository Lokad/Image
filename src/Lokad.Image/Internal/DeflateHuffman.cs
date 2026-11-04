namespace Lokad.Image.Internal;

// Original RFC1951 canonical-tree construction. Frequencies come from the
// current token block; no upstream coding or implementation tables are used.
internal static class DeflateHuffman
{
    internal readonly record struct Run(int Symbol, int Extra, int Bits);

    // Operation-owned preparation storage. All three RFC1951 alphabets fit the
    // literal/length capacity; returned trees have separate ownership.
    internal sealed class Workspace
    {
        internal readonly int[] Weights = new int[572], Depth = new int[572], Parent = new int[572];
        internal readonly int[] Heap = new int[286], Removal = new int[572], Bits = new int[572];
        internal readonly int[] BitCounts = new int[16];
        // Frequencies for the286/30/19alphabets coexist through block pricing.
        // Header codes finish before literal/distance codes use the same prefix.
        internal readonly int[] BlockFrequencies = new int[286 + 30 + 19];
        internal readonly int[] BlockCodes = new int[288 + 32];

        internal void Reset(int symbols, int maximumBits)
        {
            if ((uint)symbols > 286 || maximumBits is < 1 or > 15)
                throw new ArgumentException("Tree exceeds the Deflate workspace.");
            // Counts replace every leaf weight. Heap/removal prefixes, active
            // parents and internal weights/depths are assigned before use.
            // Inactive leaves still need zero lengths; the root is reset below.
            Array.Clear(Depth, 0, symbols); Array.Clear(Bits, 0, symbols);
            Array.Clear(BitCounts, 0, maximumBits + 1);
        }
    }

    internal static byte[] Lengths(ReadOnlySpan<int> counts, int maximumBits, OperationContext context, Workspace workspace)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        workspace.Reset(counts.Length, maximumBits);
        var weights = workspace.Weights; var depth = workspace.Depth; var parent = workspace.Parent;
        var heap = workspace.Heap; var removal = workspace.Removal;
        counts.CopyTo(weights);
        var size = 0; var largest = -1; var removed = 0;
        for (var symbol = 0; symbol < counts.Length; symbol++)
            if (counts[symbol] != 0) heap[size++] = largest = symbol;
        while (size < 2)
        {
            var dummy = largest < 2 ? ++largest : 0;
            heap[size++] = dummy; weights[dummy] = 1;
        }
        // A structural right-child tie and subtree depth make heap removal
        // deterministic, including when several symbols have equal weights.
        bool Before(int a, int b) => weights[a] < weights[b] || weights[a] == weights[b] && depth[a] <= depth[b];
        void Sink(int index)
        {
            var value = heap[index];
            for (var child = index * 2 + 1; child < size; child = index * 2 + 1)
            {
                if (child + 1 < size && Before(heap[child + 1], heap[child])) child++;
                if (Before(value, heap[child])) break;
                heap[index] = heap[child]; index = child;
            }
            heap[index] = value;
        }
        for (var index = size / 2 - 1; index >= 0; index--) Sink(index);
        var next = counts.Length;
        while (size > 1)
        {
            var a = heap[0]; heap[0] = heap[--size]; Sink(0);
            var b = heap[0]; removal[removed++] = a; removal[removed++] = b;
            parent[a] = parent[b] = next;
            weights[next] = weights[a] + weights[b]; depth[next] = Math.Max(depth[a], depth[b]) + 1;
            heap[0] = next++; Sink(0);
        }
        var bits = workspace.Bits; var bitCounts = workspace.BitCounts; var overflow = 0;
        bits[next - 1] = 0;
        for (var node = next - 2; node >= 0; node--)
        {
            if (weights[node] == 0) continue;
            var length = bits[parent[node]] + 1;
            if (length > maximumBits) { length = maximumBits; overflow++; }
            bits[node] = length;
            if (node < counts.Length) bitCounts[length]++;
        }
        if (overflow != 0)
        {
            while (overflow > 0)
            {
                var length = maximumBits - 1;
                while (bitCounts[length] == 0) length--;
                bitCounts[length]--; bitCounts[length + 1] += 2; bitCounts[maximumBits]--; overflow -= 2;
            }
            var at = 0;
            for (var length = maximumBits; length > 0; length--)
                for (var count = bitCounts[length]; count > 0; count--)
                {
                    while (removal[at] >= counts.Length) at++;
                    bits[removal[at++]] = length;
                }
        }
        var result = new byte[largest + 1];
        for (var symbol = 0; symbol <= largest; symbol++) result[symbol] = (byte)bits[symbol];
        return result;
    }

    internal static void Codes(ReadOnlySpan<byte> lengths, Span<int> codes)
    {
        codes = codes[..lengths.Length]; codes.Clear();
        // RFC1951 section3.2.2: each width's first code follows the last
        // shorter code. Assign equal-width codes in symbol order in one pass.
        Span<int> counts = stackalloc int[16]; counts.Clear();
        Span<int> next = stackalloc int[16];
        foreach (var width in lengths) if (width <= 15) counts[width]++;
        var value = 0; counts[0] = 0;
        for (var width = 1; width <= 15; width++)
        {
            value = (value + counts[width - 1]) << 1; next[width] = value;
        }
        for (var symbol = 0; symbol < lengths.Length; symbol++)
        {
            var width = lengths[symbol]; if (width is 0 or > 15) continue;
            var bits = (uint)next[width]++;
            // Reverse the low16bits, then retain the code's original width.
            bits = (bits >> 1 & 0x5555u) | (bits & 0x5555u) << 1;
            bits = (bits >> 2 & 0x3333u) | (bits & 0x3333u) << 2;
            bits = (bits >> 4 & 0x0f0fu) | (bits & 0x0f0fu) << 4;
            bits = (bits >> 8) | (bits & 255) << 8;
            codes[symbol] = (int)(bits >> (16 - width));
        }
    }

    internal static int Runs(byte[] lengths, Span<Run> output)
    {
        var written = 0;
        for (var at = 0; at < lengths.Length;)
        {
            var value = lengths[at]; var end = at + 1;
            while (end < lengths.Length && lengths[end] == value) end++;
            var count = end - at;
            if (value != 0) { output[written++] = new(value, 0, 0); count--; }
            while (count > 0)
            {
                if (value == 0 && count >= 11)
                { var take = Math.Min(138, count); output[written++] = new(18, take - 11, 7); count -= take; }
                else if (value == 0 && count >= 3)
                { var take = Math.Min(10, count); output[written++] = new(17, take - 3, 3); count -= take; }
                else if (value != 0 && count >= 3)
                { var take = Math.Min(6, count); output[written++] = new(16, take - 3, 2); count -= take; }
                else { output[written++] = new(value, 0, 0); count--; }
            }
            at = end;
        }
        return written;
    }
}
