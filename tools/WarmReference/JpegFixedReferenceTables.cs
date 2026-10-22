// Original generator for the format examples in ITU-T T.81 K.3.3, Tables K.3-K.6.
// https://www.w3.org/Graphics/JPEG/itu-t81.pdf#page=162
// These are optional example tables in the specification, not JPEG defaults.
// Short-code format data is stated below; the remaining legal AC symbols have
// length16 and follow ordinal order. No upstream implementation table is used.
using System;

public static class JpegFixedReferenceTables
{
    public static JpegScanParity.CodeTable Create(int family)
    {
        if (family < 0 || family > 3) throw new ArgumentOutOfRangeException(nameof(family));
        bool ac = family >= 2, chroma = (family & 1) != 0;
        var table = new JpegScanParity.CodeTable { Symbols = new byte[ac ? 162 : 12] };
        if (!ac)
        {
            for (int category = 0; category < 12; category++)
            {
                int length = chroma ? Math.Max(2, category)
                    : category == 0 ? 2 : category <= 5 ? 3 : category - 2;
                table.Counts[length - 1]++; table.Symbols[category] = (byte)category;
            }
        }
        else
        {
            // K.3.3.2: the counts and ordered values for lengths1..15 only.
            ReadOnlySpan<byte> shortCounts = chroma
                ? new byte[] { 0,2,1,2,4,4,3,4,7,5,4,4,0,1,2 }
                : new byte[] { 0,2,1,3,3,2,4,3,5,5,4,4,0,0,1 };
            ReadOnlySpan<byte> shortSymbols = chroma
                ? new byte[] { 0x00,0x01,0x02,0x03,0x11,0x04,0x05,0x21,0x31,0x06,
                    0x12,0x41,0x51,0x07,0x61,0x71,0x13,0x22,0x32,0x81,0x08,0x14,
                    0x42,0x91,0xa1,0xb1,0xc1,0x09,0x23,0x33,0x52,0xf0,0x15,0x62,
                    0x72,0xd1,0x0a,0x16,0x24,0x34,0xe1,0x25,0xf1 }
                : new byte[] { 0x01,0x02,0x03,0x00,0x04,0x11,0x05,0x12,0x21,0x31,
                    0x41,0x06,0x13,0x51,0x61,0x07,0x22,0x71,0x14,0x32,0x81,0x91,
                    0xa1,0x08,0x23,0x42,0xb1,0xc1,0x15,0x52,0xd1,0xf0,0x24,0x33,
                    0x62,0x72,0x82 };
            shortCounts.CopyTo(table.Counts); shortSymbols.CopyTo(table.Symbols);
            table.Counts[15] = (byte)(162 - shortSymbols.Length);
            Span<bool> used = stackalloc bool[256]; used.Clear();
            foreach (byte symbol in shortSymbols) used[symbol] = true;
            int at = shortSymbols.Length;
            for (int symbol = 0; symbol < 256; symbol++)
                if (!used[symbol] && (symbol == 0 || symbol == 240 || (symbol & 15) is >= 1 and <= 10))
                    table.Symbols[at++] = (byte)symbol;
            if (at != 162) throw new InvalidOperationException("Incomplete AC format alphabet.");
        }
        table.Prepare();
        return table;
    }
}
