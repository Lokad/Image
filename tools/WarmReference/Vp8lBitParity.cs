#nullable enable
using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

// Independent RFC 9649 writer for a cache-free auxiliary raster.
internal sealed class Vp8lParityBits
{
    private readonly List<byte> bytes = new();
    internal int Count { get; private set; }
    internal void Put(int value, int count)
    {
        for (var bit = 0; bit < count; bit++, Count++)
        {
            if ((Count & 7) == 0) bytes.Add(0);
            bytes[Count >> 3] |= (byte)(((value >> bit) & 1) << (Count & 7));
        }
    }
    internal byte[] Finish() => bytes.ToArray();
}

internal static class Vp8lBitParity
{
    private sealed class Code
    {
        internal readonly byte[] Lengths;
        private readonly int[] words;
        internal Code(byte[] lengths, bool data = false)
        {
            Lengths = lengths.ToArray(); words = new int[lengths.Length];
            var counts = new int[16]; foreach (var length in lengths) counts[length]++;
            var next = new int[16]; var code = 0;
            for (var depth = 1; depth < 16; depth++)
            { code = (code + (depth == 1 ? 0 : counts[depth - 1])) << 1; next[depth] = code; }
            for (var symbol = 0; symbol < lengths.Length; symbol++)
            {
                var depth = lengths[symbol]; if (depth == 0) continue;
                var value = next[depth]++; var reversed = 0;
                for (var i = 0; i < depth; i++) { reversed = reversed * 2 + (value & 1); value >>= 1; }
                words[symbol] = reversed;
            }
            if (data && lengths.Count(x => x != 0) <= 1) Array.Clear(Lengths);
        }
        internal void Write(Vp8lParityBits bits, int symbol) => bits.Put(words[symbol], Lengths[symbol]);
    }
    private readonly record struct Run(int Symbol, int Extra = 0)
    {
        internal int Width => Symbol == 16 ? 2 : Symbol == 17 ? 3 : Symbol == 18 ? 7 : 0;
    }
    private static List<Run> Runs(byte[] lengths)
    {
        var result = new List<Run>(); var previous = 8;
        for (var at = 0; at < lengths.Length;)
        {
            var value = lengths[at]; var end = at + 1;
            while (end < lengths.Length && lengths[end] == value) end++;
            var remaining = end - at; at = end;
            if (value != 0 && value != previous) { result.Add(new(value)); remaining--; }
            if (value != 0) previous = value;
            while (remaining > 0)
            {
                if (remaining < 3) { result.Add(new(value)); remaining--; continue; }
                var count = Math.Min(remaining, value == 0 ? 138 : 6);
                var symbol = value != 0 ? 16 : count < 11 ? 17 : 18;
                result.Add(new(symbol, count - (symbol == 18 ? 11 : 3))); remaining -= count;
            }
        }
        return result;
    }
    private static void Tree(Vp8lParityBits bits, byte[] lengths)
    {
        var leaves = Enumerable.Range(0, lengths.Length).Where(x => lengths[x] != 0).ToArray();
        if (leaves.Length == 0) { bits.Put(1, 4); return; }
        if (leaves.Length <= 2 && leaves[^1] < 256)
        {
            bits.Put(1, 1); bits.Put(leaves.Length - 1, 1);
            var wide = leaves[0] > 1; bits.Put(wide ? 1 : 0, 1); bits.Put(leaves[0], wide ? 8 : 1);
            if (leaves.Length == 2) bits.Put(leaves[1], 8);
            return;
        }
        var runs = Runs(lengths); var frequencies = new int[19];
        foreach (var run in runs) frequencies[run.Symbol]++;
        var headerLengths = Vp8lPrefixParity.Lengths(frequencies, 7);
        var header = new Code(headerLengths, true);
        // Normative code-length alphabet order, RFC 9649 section 5.2.2.
        int[] order = [17, 18, 0, 1, 2, 3, 4, 5, 16, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        var used = 19; while (used > 4 && headerLengths[order[used - 1]] == 0) used--;
        bits.Put(0, 1); bits.Put(used - 4, 4);
        for (var i = 0; i < used; i++) bits.Put(headerLengths[order[i]], 3);
        var keep = runs.Count; var saved = 0;
        while (keep > 0 && runs[keep - 1].Symbol is 0 or 17 or 18)
        { var run = runs[--keep]; saved += header.Lengths[run.Symbol] + run.Width; }
        var trim = keep > 1 && saved > 12;
        bits.Put(trim ? 1 : 0, 1);
        if (trim)
        {
            var pairs = keep == 2 ? 1 : (int)BitOperations.Log2((uint)(keep - 2)) / 2 + 1;
            bits.Put(pairs - 1, 3); bits.Put(keep - 2, 2 * pairs);
        }
        else keep = runs.Count;
        foreach (var run in runs.Take(keep)) { header.Write(bits, run.Symbol); bits.Put(run.Extra, run.Width); }
    }
    private static void Prefix(Vp8lParityBits bits, Code tree, int value, int offset)
    {
        if (value <= 4) { tree.Write(bits, offset + value - 1); return; }
        var high = (int)BitOperations.Log2((uint)(value - 1)); var extra = high - 1;
        var code = 2 * high + ((value - 1) >> extra & 1);
        tree.Write(bits, offset + code); bits.Put((value - 1) & ((1 << extra) - 1), extra);
    }
    internal static (byte[] Bytes, int BitCount, int TokenStart) Encode(IReadOnlyList<Vp8lParityToken> tokens, int width, byte[][] lengths)
    {
        var bits = new Vp8lParityBits(); bits.Put(0, 1);
        foreach (var tree in lengths) Tree(bits, tree);
        var start = bits.Count; var codes = lengths.Select(x => new Code(x, true)).ToArray();
        foreach (var token in tokens)
        {
            if (token.Copy)
            { Prefix(bits, codes[0], token.Length, 256); Prefix(bits, codes[4], Vp8lLzParity.Plane(token.Distance, width), 0); }
            else
            {
                codes[0].Write(bits, (int)(token.Pixel >> 8 & 255)); codes[1].Write(bits, (int)(token.Pixel >> 16 & 255));
                codes[2].Write(bits, (int)(token.Pixel & 255)); codes[3].Write(bits, (int)(token.Pixel >> 24));
            }
        }
        return (bits.Finish(), bits.Count, start);
    }
}
