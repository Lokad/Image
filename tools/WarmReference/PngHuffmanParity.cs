// Original development-only Huffman stage. RFC1951 sections3.2.2/3.2.5/3.2.7.
// Trees are generated from source-derived tokens; no observed coding lengths,
// native tree, packet or upstream implementation table is an encoder input.
using System;
using System.Collections.Generic;

public static class PngHuffmanParity
{
    public sealed record Run(int Symbol, int Extra, int ExtraBits);
    public sealed record Block(byte[] Literals, byte[] Distances, byte[] Lengths, Run[] Runs);

    public static Block[] Generate(byte[] source)
    {
        byte[] tokens = PngDeflateParity.EncodeTokens(source);
        var blocks = new List<Block>();
        for (int first = 0; first < tokens.Length / 6; first += 16383)
        {
            int[] literal = new int[286], distance = new int[30];
            literal[256] = 1;
            int end = Math.Min(tokens.Length / 6, first + 16383);
            for (int token = first; token < end; token++)
            {
                int at = token * 6;
                int length = tokens[at + 2] | tokens[at + 3] << 8;
                int back = tokens[at + 4] | tokens[at + 5] << 8;
                if (length == 0) literal[tokens[at]]++;
                else { literal[LengthSymbol(length)]++; distance[DistanceSymbol(back)]++; }
            }
            byte[] ll = Tree(literal, 15), dd = Tree(distance, 15);
            var runs = new List<Run>();
            AddRuns(ll, runs); AddRuns(dd, runs);
            var frequencies = new int[19];
            foreach (Run run in runs) frequencies[run.Symbol]++;
            byte[] lengthTree = Tree(frequencies, 7);
            Array.Resize(ref lengthTree, 19);
            blocks.Add(new(ll, dd, lengthTree, runs.ToArray()));
        }
        return blocks.ToArray();
    }

    public static int LengthSymbol(int length)
    {
        if (length == 258) return 285;
        if (length <= 10) return length + 254;
        int extra = 1;
        while (length >= 3 + (8 << extra)) extra++;
        return 261 + extra * 4 + ((length - 3) >> extra) - 4;
    }

    public static int DistanceSymbol(int distance)
    {
        if (distance <= 4) return distance - 1;
        int extra = 1;
        while (distance > 4 << extra) extra++;
        return 2 + extra * 2 + ((distance - 1) >> extra) - 2;
    }

    public static byte[] Tree(int[] counts, int maximumBits)
    {
        // Reproducible binary-heap ordering: weight, subtree depth, then the
        // structural right child on equality. No frequency/configuration table.
        int capacity = counts.Length * 2;
        int[] weights = new int[capacity], depth = new int[capacity], parent = new int[capacity];
        int[] heap = new int[counts.Length], removal = new int[capacity];
        counts.CopyTo(weights, 0);
        int size = 0, largest = -1, removed = 0;
        for (int symbol = 0; symbol < counts.Length; symbol++)
            if (counts[symbol] != 0) heap[size++] = largest = symbol;
        while (size < 2)
        {
            int dummy = largest < 2 ? ++largest : 0;
            heap[size++] = dummy; weights[dummy] = 1;
        }
        bool Before(int a, int b) => weights[a] < weights[b] || weights[a] == weights[b] && depth[a] <= depth[b];
        void Sink(int index)
        {
            int value = heap[index];
            for (int child = index * 2 + 1; child < size; child = index * 2 + 1)
            {
                if (child + 1 < size && Before(heap[child + 1], heap[child])) child++;
                if (Before(value, heap[child])) break;
                heap[index] = heap[child]; index = child;
            }
            heap[index] = value;
        }
        for (int index = size / 2 - 1; index >= 0; index--) Sink(index);
        int next = counts.Length;
        while (size > 1)
        {
            int a = heap[0]; heap[0] = heap[--size]; Sink(0);
            int b = heap[0]; removal[removed++] = a; removal[removed++] = b;
            parent[a] = parent[b] = next;
            weights[next] = weights[a] + weights[b]; depth[next] = Math.Max(depth[a], depth[b]) + 1;
            heap[0] = next++; Sink(0);
        }
        int[] bits = new int[capacity], bitCounts = new int[maximumBits + 1];
        int overflow = 0;
        for (int node = next - 2; node >= 0; node--)
        {
            if (weights[node] == 0) continue;
            int length = bits[parent[node]] + 1;
            if (length > maximumBits) { length = maximumBits; overflow++; }
            bits[node] = length;
            if (node < counts.Length) bitCounts[length]++;
        }
        if (overflow != 0)
        {
            while (overflow > 0)
            {
                int length = maximumBits - 1;
                while (bitCounts[length] == 0) length--;
                bitCounts[length]--; bitCounts[length + 1] += 2; bitCounts[maximumBits]--; overflow -= 2;
            }
            int at = 0;
            for (int length = maximumBits; length > 0; length--)
                for (int count = bitCounts[length]; count > 0; count--)
                {
                    while (removal[at] >= counts.Length) at++;
                    bits[removal[at++]] = length;
                }
        }
        var result = new byte[largest + 1];
        for (int symbol = 0; symbol <= largest; symbol++) result[symbol] = (byte)bits[symbol];
        return result;
    }

    private static void AddRuns(byte[] lengths, List<Run> output)
    {
        for (int at = 0; at < lengths.Length;)
        {
            int value = lengths[at], end = at + 1;
            while (end < lengths.Length && lengths[end] == value) end++;
            int count = end - at;
            if (value != 0) { output.Add(new(value, 0, 0)); count--; }
            while (count > 0)
            {
                if (value == 0 && count >= 11)
                { int take = Math.Min(138, count); output.Add(new(18, take - 11, 7)); count -= take; }
                else if (value == 0 && count >= 3)
                { int take = Math.Min(10, count); output.Add(new(17, take - 3, 3)); count -= take; }
                else if (value != 0 && count >= 3)
                { int take = Math.Min(6, count); output.Add(new(16, take - 3, 2)); count -= take; }
                else { output.Add(new(value, 0, 0)); count--; }
            }
            at = end;
        }
    }
}
