namespace Lokad.Image.Internal;

// Original RFC1951 canonical-tree construction. Frequencies come from the
// current token block; no upstream coding or implementation tables are used.
internal static class DeflateHuffman
{
    internal readonly record struct Run(int Symbol, int Extra, int Bits);

    internal static byte[] Lengths(int[] counts, int maximumBits, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        var capacity = counts.Length * 2;
        var weights = new int[capacity]; var depth = new int[capacity]; var parent = new int[capacity];
        var heap = new int[counts.Length]; var removal = new int[capacity];
        counts.CopyTo(weights, 0);
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
        var bits = new int[capacity]; var bitCounts = new int[maximumBits + 1]; var overflow = 0;
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

    internal static int[] Codes(byte[] lengths)
    {
        var codes = new int[lengths.Length]; var next = 0;
        for (var width = 1; width <= 15; width++)
        {
            for (var symbol = 0; symbol < lengths.Length; symbol++)
            {
                if (lengths[symbol] != width) continue;
                var reverse = 0; var value = next++;
                for (var bit = 0; bit < width; bit++) { reverse = reverse << 1 | value & 1; value >>= 1; }
                codes[symbol] = reverse;
            }
            next <<= 1;
        }
        return codes;
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
