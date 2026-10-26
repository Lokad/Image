// Independently implemented tree/priority-queue form of T.81 K.2. A reserved
// unit-weight leaf excludes the all-ones code. Equal weights prefer the larger
// surviving root identity; canonical symbols retain original depth/ordinal order.
// https://www.w3.org/Graphics/JPEG/itu-t81.pdf#page=148
namespace Lokad.Image.Internal;

internal static class JpegGrayReferenceTables
{
    private sealed record Branch(long Weight, int Identity, int Symbol, Branch? Left, Branch? Right);

    internal static JpegEncodingTable Huffman(int[] histogram, OperationContext context)
    {
        if (histogram.Length != 256) throw new ArgumentException("Byte histogram required.");
        int active = 0;
        foreach (int count in histogram)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(histogram));
            if (count != 0) active++;
        }
        // One temporary owner covers the complete tree, queue/stack backing
        // capacity and old/new array overlap. The caller owns surviving tables.
        long scratch = 65536L + (active + 1) * 2048L;
        context.Reserve(scratch);
        try { context.Cancellation.ThrowIfCancellationRequested(); return Build(histogram, context); }
        finally { context.Release(scratch); }
    }

    private static JpegEncodingTable Build(int[] histogram, OperationContext context)
    {
        if (histogram.Length != 256) throw new ArgumentException("Byte histogram required.");
        var queue = new PriorityQueue<Branch, (long Weight, int ReverseIdentity)>();
        int active = 0;
        for (int i = 0; i < 256; i++)
        {
            if (histogram[i] < 0) throw new ArgumentOutOfRangeException(nameof(histogram));
            if (histogram[i] == 0) continue;
            queue.Enqueue(new Branch(histogram[i], i, i, null, null), (histogram[i], -i)); active++;
        }
        if (active == 0) throw new ArgumentException("Nonempty histogram required.");
        queue.Enqueue(new Branch(1, 256, 256, null, null), (1, -256));
        while (queue.Count > 1)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            Branch left = queue.Dequeue(), right = queue.Dequeue();
            var parent = new Branch(left.Weight + right.Weight, left.Identity, -1, left, right);
            queue.Enqueue(parent, (parent.Weight, -parent.Identity));
        }
        var depths = new int[257];
        var pending = new Stack<(Branch Branch, int Depth)>(); pending.Push((queue.Dequeue(), 0));
        while (pending.Count != 0)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var item = pending.Pop();
            if (item.Branch.Symbol >= 0) depths[item.Branch.Symbol] = item.Depth;
            else
            {
                pending.Push((item.Branch.Left!, item.Depth + 1));
                pending.Push((item.Branch.Right!, item.Depth + 1));
            }
        }
        var counts = new int[257];
        foreach (int depth in depths) if (depth != 0) counts[depth]++;
        for (int depth = 256; depth > 16; depth--)
            while (counts[depth] != 0)
            {
                int prefix = depth - 2;
                while (counts[prefix] == 0) prefix--;
                counts[depth] -= 2; counts[depth - 1]++;
                counts[prefix]--; counts[prefix + 1] += 2;
            }
        int longest = 16;
        while (counts[longest] == 0) longest--;
        counts[longest]--; // Remove the reserved final code point.

        var encodedCounts = new byte[16]; var symbols = new byte[active];
        for (int depth = 1; depth <= 16; depth++) encodedCounts[depth - 1] = checked((byte)counts[depth]);
        int at = 0;
        for (int depth = 1; depth <= 256; depth++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            for (int symbol = 0; symbol < 256; symbol++)
                if (depths[symbol] == depth) symbols[at++] = (byte)symbol;
        }
        var codes = new ushort[256]; var lengths = new byte[256];
        int code = 0, position = 0;
        for (int length = 1; length <= 16; length++)
        {
            for (int i = 0; i < encodedCounts[length - 1]; i++)
            {
                if (position >= symbols.Length || code >= (1 << length) - 1)
                    throw new InvalidOperationException("Invalid grayscale reference Huffman tree.");
                int symbol = symbols[position++]; codes[symbol] = (ushort)code++; lengths[symbol] = (byte)length;
            }
            code <<= 1;
        }
        if (position != symbols.Length) throw new InvalidOperationException("Grayscale reference symbols differ.");
        return new JpegEncodingTable(encodedCounts, symbols, codes, lengths);
    }
}
