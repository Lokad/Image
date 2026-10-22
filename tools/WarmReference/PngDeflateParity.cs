// Original source-only LZ parser for the frozen PNG level6 reference.
// RFC1951 sections2/3.2 define the byte-history/length/distance representation.
// The pinned parser uses a three-byte rolling fingerprint, bounded newest-first
// search and one-position lazy selection. No oracle tokens/tables or upstream
// implementation/configuration tables are encoder inputs.
using System;
using System.Collections.Generic;

public static class PngDeflateParity
{
    private const int Width = 513, Height = 259, Window = 1 << 15;
    private const int Minimum = 3, Maximum = 258, Lookahead = Maximum + Minimum + 1;

    public static byte[] FilteredRows(byte[] rgba)
    {
        if (rgba.Length != Width * Height * 4) throw new ArgumentException("Frozen RGBA8 geometry required.");
        var rows = new byte[rgba.Length + Height];
        for (int y = 0; y < Height; y++) rgba.AsSpan(y * Width * 4, Width * 4).CopyTo(rows.AsSpan(y * (Width * 4 + 1) + 1));
        return rows;
    }

    public static byte[] EncodeTokens(byte[] rgba)
    {
        byte[] input = FilteredRows(rgba);
        int[] heads = new int[Window], older = new int[Window];
        var tokens = new List<byte>(input.Length * 2);
        int position = 0, windowOrigin = 0;
        int deferredLength = Minimum - 1, deferredStart = 0;
        bool hasDeferredByte = false;

        int Insert(int at)
        {
            // Generate the three-byte fingerprint directly rather than retaining
            // a rolling value across dictionary insertion and match skips.
            int key = ((input[at] << 10) ^ (input[at + 1] << 5) ^ input[at + 2]) & (Window - 1);
            int prior = heads[key]; older[at & (Window - 1)] = prior; heads[key] = at;
            return prior;
        }
        void Emit(int value, int length, int distance)
        {
            foreach (int field in new[] { value, length, distance })
            { tokens.Add((byte)field); tokens.Add((byte)(field >> 8)); }
        }

        while (position < input.Length)
        {
            // Absolute dictionary positions avoid a copied/slid byte window.
            // Preserve the pinned parser's reserved index0 after each slide.
            if (position >= windowOrigin + 2 * Window - Lookahead + 1) windowOrigin += Window;
            int prior = position + Minimum <= input.Length ? Insert(position) : 0;
            int length = Minimum - 1, start = 0;
            if (prior > windowOrigin && position - prior <= Window - Lookahead && deferredLength < 16)
            {
                int budget = deferredLength >= 8 ? 32 : 128;
                int best = deferredLength, limit = Math.Max(windowOrigin, position - (Window - Lookahead));
                int available = Math.Min(Maximum, input.Length - position);
                int enough = Math.Min(128, available);
                do
                {
                    if (best < available && input[prior + best] == input[position + best]
                        && input[prior + best - 1] == input[position + best - 1]
                        && input[prior] == input[position] && input[prior + 1] == input[position + 1])
                    {
                        int found = 2;
                        while (found < available && input[prior + found] == input[position + found]) found++;
                        if (found > best) { best = found; start = prior; if (found >= enough) break; }
                    }
                    prior = older[prior & (Window - 1)];
                } while (prior > limit && --budget != 0);
                length = Math.Min(best, available);
                if (length == Minimum && position - start > Window / 8) length = Minimum - 1;
            }

            if (deferredLength >= Minimum && deferredLength >= length)
            {
                Emit(0, deferredLength, position - 1 - deferredStart);
                int after = position + deferredLength - 1;
                for (int at = position + 1; at < after && at + Minimum <= input.Length; at++) Insert(at);
                position = after; deferredLength = Minimum - 1; hasDeferredByte = false;
            }
            else
            {
                if (hasDeferredByte) Emit(input[position - 1], 0, 0);
                hasDeferredByte = true; deferredLength = length; deferredStart = start; position++;
            }
        }
        if (hasDeferredByte) Emit(input[position - 1], 0, 0);
        return tokens.ToArray();
    }
}
