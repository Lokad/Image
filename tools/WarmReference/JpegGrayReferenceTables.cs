// Independently implemented tree/priority-queue form of T.81 K.2. A reserved
// unit-weight leaf excludes the all-ones code. Equal weights prefer the larger
// surviving root identity; canonical symbols retain original depth/ordinal order.
// https://www.w3.org/Graphics/JPEG/itu-t81.pdf#page=148
using System;
using System.Collections.Generic;

public static class JpegGrayReferenceTables
{
    private sealed record Branch(long Weight, int Identity, int Symbol, Branch Left, Branch Right);

    public static JpegScanParity.CodeTable Huffman(int[] histogram)
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
            Branch left = queue.Dequeue(), right = queue.Dequeue();
            var parent = new Branch(left.Weight + right.Weight, left.Identity, -1, left, right);
            queue.Enqueue(parent, (parent.Weight, -parent.Identity));
        }
        var depths = new int[257];
        var pending = new Stack<(Branch Branch, int Depth)>(); pending.Push((queue.Dequeue(), 0));
        while (pending.Count != 0)
        {
            var item = pending.Pop();
            if (item.Branch.Symbol >= 0) depths[item.Branch.Symbol] = item.Depth;
            else
            {
                pending.Push((item.Branch.Left, item.Depth + 1));
                pending.Push((item.Branch.Right, item.Depth + 1));
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

        var result = new JpegScanParity.CodeTable { Symbols = new byte[active] };
        for (int depth = 1; depth <= 16; depth++) result.Counts[depth - 1] = checked((byte)counts[depth]);
        int at = 0;
        for (int depth = 1; depth <= 256; depth++)
            for (int symbol = 0; symbol < 256; symbol++)
                if (depths[symbol] == depth) result.Symbols[at++] = (byte)symbol;
        result.Prepare();
        return result;
    }
}
