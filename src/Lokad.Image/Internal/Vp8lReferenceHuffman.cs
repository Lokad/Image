using System.Numerics;

namespace Lokad.Image.Internal;

internal static class Vp8lReferenceHuffman
{
    // Normative code-length alphabet order, RFC 9649 section 5.2.2.
    private static ReadOnlySpan<byte> Order => [17, 18, 0, 1, 2, 3, 4, 5, 16, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
    private readonly record struct Run(byte Symbol, byte Extra)
    { internal int Width => Symbol == 16 ? 2 : Symbol == 17 ? 3 : Symbol == 18 ? 7 : 0; }

    internal static void WriteTree(WebpBitWriter writer, ReadOnlySpan<byte> lengths, OperationContext context)
    {
        var leaves = 0; var first = 0; var last = 0;
        for (var symbol = 0; symbol < lengths.Length; symbol++)
        {
            context.Work(1);
            if (lengths[symbol] == 0) continue;
            if (leaves++ == 0) first = symbol; last = symbol;
        }
        if (leaves == 0) { writer.Write(1, 4); return; }
        if (leaves <= 2 && last < 256)
        {
            writer.Write(1, 1); writer.Write(leaves - 1, 1); writer.Write(first > 1 ? 1 : 0, 1);
            writer.Write(first, first > 1 ? 8 : 1); if (leaves == 2) writer.Write(last, 8); return;
        }
        var reserved = lengths.Length * 2L + 64; context.Reserve(reserved);
        try
        {
            context.Work(0); var runs = new Run[lengths.Length]; var count = 0; byte previous = 8;
            Span<int> frequencies = stackalloc int[19]; frequencies.Clear();
            for (var at = 0; at < lengths.Length;)
            {
                context.Work(1); var value = lengths[at]; var end = at + 1;
                while (end < lengths.Length && lengths[end] == value) { context.Work(1); end++; }
                var remaining = end - at; at = end;
                if (value != 0 && value != previous) { runs[count++] = new(value, 0); frequencies[value]++; remaining--; }
                if (value != 0) previous = value;
                while (remaining != 0)
                {
                    context.Work(1);
                    if (remaining < 3) { runs[count++] = new(value, 0); frequencies[value]++; remaining--; }
                    else
                    {
                        var repeat = Math.Min(remaining, value == 0 ? 138 : 6);
                        byte symbol = value != 0 ? (byte)16 : repeat < 11 ? (byte)17 : (byte)18;
                        runs[count++] = new(symbol, (byte)(repeat - (symbol == 18 ? 11 : 3))); frequencies[symbol]++; remaining -= repeat;
                    }
                }
            }
            using var header = Vp8lHuffmanCode.Build(frequencies, context, 7);
            var headerLengths = header.Lengths; var used = 19;
            while (used > 4 && headerLengths[Order[used - 1]] == 0) used--;
            writer.Write(0, 1); writer.Write(used - 4, 4);
            for (var i = 0; i < used; i++) writer.Write(headerLengths[Order[i]], 3);
            var keep = count; var saved = 0; var headerSingle = 0;
            foreach (var length in headerLengths) if (length != 0) headerSingle++;
            while (keep > 0 && runs[keep - 1].Symbol is 0 or 17 or 18)
            {
                context.Work(1); var run = runs[--keep];
                saved += (headerSingle <= 1 ? 0 : headerLengths[run.Symbol]) + run.Width;
            }
            var trim = keep > 1 && saved > 12; writer.Write(trim ? 1 : 0, 1);
            if (trim)
            {
                var pairs = keep == 2 ? 1 : (int)BitOperations.Log2((uint)(keep - 2)) / 2 + 1;
                writer.Write(pairs - 1, 3); writer.Write(keep - 2, 2 * pairs);
            }
            else keep = count;
            for (var i = 0; i < keep; i++)
            { context.Work(1); var run = runs[i]; header.Write(writer, run.Symbol); writer.Write(run.Extra, run.Width); }
        }
        finally { context.Release(reserved); }
    }

    internal static void WritePrefix(WebpBitWriter writer, Vp8lHuffmanCode code, int value, int offset)
    {
        var prefix = Vp8lSymbols.Prefix(value); code.Write(writer, offset + prefix.Code);
        writer.Write((value - 1) & ((1 << prefix.Extra) - 1), prefix.Extra);
    }

    // Cache-free transform subimages have five trees and no entropy image. The
    // caller owns the output stream/writer, so several images can share bit state.
    internal static void WriteAuxiliary(WebpBitWriter writer, ReadOnlySpan<Vp8lToken> tokens, int width, OperationContext context)
    {
        var layout = new Vp8lHistogramLayout(0); var codes = new Vp8lHuffmanCode?[5];
        try
        {
            var reserved = layout.Total * 4L; context.Reserve(reserved);
            try
            {
                context.Work(0); var counts = new int[layout.Total]; layout.Fill(tokens, width, true, counts, context);
                codes[0] = Vp8lHuffmanCode.Build(counts.AsSpan(0, layout.GreenSize), context);
                codes[1] = Vp8lHuffmanCode.Build(counts.AsSpan(layout.Red, 256), context);
                codes[2] = Vp8lHuffmanCode.Build(counts.AsSpan(layout.Blue, 256), context);
                codes[3] = Vp8lHuffmanCode.Build(counts.AsSpan(layout.Alpha, 256), context);
                codes[4] = Vp8lHuffmanCode.Build(counts.AsSpan(layout.Distance, 40), context);
            }
            finally { context.Release(reserved); }
            writer.Write(0, 1);
            foreach (var code in codes) WriteTree(writer, code!.Lengths, context);
            foreach (var token in tokens)
            {
                context.Work(1);
                if (token.Code > 0)
                {
                    WritePrefix(writer, codes[0]!, token.Code, 256);
                    WritePrefix(writer, codes[4]!, Vp8lSymbols.Plane((int)token.Value, width), 0);
                }
                else
                {
                    if (token.Code != 0) throw new InvalidOperationException("An auxiliary image cannot use cache tokens.");
                    var pixel = token.Value;
                    codes[0]!.Write(writer, (int)(pixel >> 8 & 255)); codes[1]!.Write(writer, (int)(pixel >> 16 & 255));
                    codes[2]!.Write(writer, (int)(pixel & 255)); codes[3]!.Write(writer, (int)(pixel >> 24));
                }
            }
        }
        finally { for (var i = codes.Length - 1; i >= 0; i--) codes[i]?.Dispose(); }
    }
}
