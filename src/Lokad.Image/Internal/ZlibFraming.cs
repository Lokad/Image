namespace Lokad.Image.Internal;

// Independently written framing walk, not an inflater. RFC 1950 section 2.2 and
// RFC 1951 sections 3.1 and 3.2 define the bits, canonical trees and numeric ranges.
// This establishes BFINAL, the exact checksum position and the bounded output
// count before the BCL inflater runs. It never materializes decompressed bytes.
internal static class ZlibFraming
{
    public static int Validate(ReadOnlySpan<byte> data, long expectedBytes, OperationContext context)
    {
        if (data.Length < 6 || (data[0] & 15) != 8 || data[0] >> 4 > 7
            || ((data[0] << 8) + data[1]) % 31 != 0 || (data[1] & 32) != 0)
            throw new ImageDataException("Invalid PNG zlib header.");
        var window = 1 << ((data[0] >> 4) + 8);
        // Fixed maximum stack scratch, independent of declared dimensions or input.
        context.Reserve(4096);
        try
        {
            Span<int> lengths = stackalloc int[320]; Span<int> codeLengths = stackalloc int[19];
            Span<int> counts = stackalloc int[48]; Span<int> symbols = stackalloc int[339];
            var reader = new Bits(data, context); reader.SkipBytes(2); long output = 0;
            bool final;
            do
            {
                context.Work(1); final = reader.Read(1) != 0; var type = reader.Read(2);
                if (type == 0)
                {
                    reader.Align(); var length = reader.Read(16); var complement = reader.Read(16);
                    if ((length ^ complement) != 65535) throw new ImageDataException("Invalid PNG Deflate stored length.");
                    if (length > expectedBytes - output) throw new ImageDataException("PNG contains excess decompressed pixels.");
                    reader.SkipBytes(length); context.Work(length); output += length; continue;
                }
                if (type == 3) throw new ImageDataException("Invalid PNG Deflate block type.");
                int literalCount, distanceCount;
                if (type == 1)
                {
                    literalCount = 288; distanceCount = 32;
                    for (var i = 0; i < 288; i++) lengths[i] = i < 144 ? 8 : i < 256 ? 9 : i < 280 ? 7 : 8;
                    lengths[288..].Fill(5);
                }
                else
                {
                    literalCount = reader.Read(5) + 257; distanceCount = reader.Read(5) + 1; var codeCount = reader.Read(4) + 4;
                    if (literalCount > 286) throw new ImageDataException("Invalid PNG Deflate literal alphabet.");
                    codeLengths.Clear();
                    ReadOnlySpan<int> order = [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15];
                    for (var i = 0; i < codeCount; i++) codeLengths[order[i]] = reader.Read(3);
                    var codes = new Huffman(codeLengths, counts[..16], symbols[..19], context);
                    var position = 0; var total = literalCount + distanceCount;
                    while (position < total)
                    {
                        var symbol = Decode(ref reader, codes.Counts, codes.Symbols, codes.Maximum);
                        if (symbol < 16) lengths[position++] = symbol;
                        else
                        {
                            if (symbol == 16 && position == 0) throw new ImageDataException("Invalid PNG Deflate first code repeat.");
                            var repeat = symbol == 16 ? reader.Read(2) + 3 : symbol == 17 ? reader.Read(3) + 3 : reader.Read(7) + 11;
                            if (repeat > total - position) throw new ImageDataException("PNG Deflate code repeat exceeds alphabet.");
                            var value = symbol == 16 ? lengths[position - 1] : 0;
                            lengths.Slice(position, repeat).Fill(value); position += repeat; context.Work(repeat);
                        }
                    }
                }
                if (lengths[256] == 0) throw new ImageDataException("PNG Deflate block has no end code.");
                var literals = new Huffman(lengths[..literalCount], counts.Slice(16, 16), symbols.Slice(19, 288), context);
                var distances = new Huffman(lengths.Slice(literalCount, distanceCount), counts.Slice(32, 16), symbols.Slice(307, 32), context);
                while (true)
                {
                    var symbol = Decode(ref reader, literals.Counts, literals.Symbols, literals.Maximum);
                    if (symbol == 256) break;
                    int length;
                    if (symbol < 256) length = 1;
                    else
                    {
                        if (symbol > 285) throw new ImageDataException("Invalid PNG Deflate length code.");
                        if (symbol < 265) length = symbol - 254;
                        else if (symbol == 285) length = 258;
                        else
                        {
                            var extra = (symbol - 265) / 4 + 1;
                            length = 3 + (1 << (extra + 2)) + ((symbol - 265) % 4 << extra) + reader.Read(extra);
                        }
                        var distanceSymbol = Decode(ref reader, distances.Counts, distances.Symbols, distances.Maximum);
                        if (distanceSymbol > 29) throw new ImageDataException("Invalid PNG Deflate distance code.");
                        var distance = distanceSymbol + 1;
                        if (distanceSymbol >= 4)
                        {
                            var extra = distanceSymbol / 2 - 1;
                            distance = 1 + ((2 + (distanceSymbol & 1)) << extra) + reader.Read(extra);
                        }
                        if (distance > output || distance > window) throw new ImageDataException("PNG Deflate distance exceeds history or window.");
                    }
                    if (length > expectedBytes - output) throw new ImageDataException("PNG contains excess decompressed pixels.");
                    output += length;
                }
            } while (!final);
            if (output != expectedBytes) throw new ImageDataException("Truncated PNG pixel data.");
            reader.Align(); var checksum = reader.Position; reader.SkipBytes(4);
            // PNG section 11.2.3 permits unused trailing bytes in the final IDAT.
            return checksum + 4;
        }
        finally { context.Release(4096); }
    }

    private ref struct Bits
    {
        private readonly ReadOnlySpan<byte> data;
        private readonly OperationContext context;
        private int position;
        private uint buffer;
        private int available;
        public Bits(ReadOnlySpan<byte> data, OperationContext context) { this.data = data; this.context = context; }
        public readonly int Position => position;

        public int Read(int count)
        {
            context.Work(count);
            while (available < count)
            {
                if (position == data.Length) throw new ImageDataException("Truncated PNG Deflate stream.");
                buffer |= (uint)data[position++] << available; available += 8;
            }
            var value = (int)(buffer & ((1u << count) - 1)); buffer >>= count; available -= count; return value;
        }

        public void Align() { buffer = 0; available = 0; }
        public void SkipBytes(int count)
        {
            if (count > data.Length - position) throw new ImageDataException("Truncated PNG zlib stream.");
            position += count;
        }
    }

    private readonly ref struct Huffman
    {
        public readonly ReadOnlySpan<int> Counts;
        public readonly ReadOnlySpan<int> Symbols;
        public readonly int Maximum;

        public Huffman(ReadOnlySpan<int> lengths, Span<int> counts, Span<int> symbols, OperationContext context)
        {
            Counts = counts; Symbols = symbols; counts.Clear(); Maximum = 0; context.Work(lengths.Length * 2L + 16);
            foreach (var length in lengths) if (length > 0) { counts[length]++; Maximum = Math.Max(Maximum, length); }
            var space = 1;
            for (var length = 1; length <= Maximum; length++)
            {
                space = space * 2 - counts[length];
                if (space < 0) throw new ImageDataException("Oversubscribed PNG Deflate prefix codes.");
            }
            // Incomplete trees are usable when every actual symbol has a code;
            // any unassigned path fails in Decode. Empty distances are legal for
            // literal-only dynamic blocks, and cannot decode a match.
            Span<int> offsets = stackalloc int[16]; offsets.Clear();
            for (var length = 1; length < 15; length++) offsets[length + 1] = offsets[length] + counts[length];
            for (var symbol = 0; symbol < lengths.Length; symbol++)
                if (lengths[symbol] != 0) symbols[offsets[lengths[symbol]]++] = symbol;
        }
    }

    private static int Decode(scoped ref Bits reader, scoped ReadOnlySpan<int> counts, scoped ReadOnlySpan<int> symbols, int maximum)
    {
        int code = 0, first = 0, index = 0;
        for (var length = 1; length <= maximum; length++)
        {
            // Huffman code bits appear most significant first in the otherwise
            // least-significant-first Deflate bitstream (RFC 1951 section 3.1.1).
            code = code * 2 + reader.Read(1); var count = counts[length];
            if (code >= first && code - first < count) return symbols[index + code - first];
            index += count; first = (first + count) * 2;
        }
        throw new ImageDataException("Invalid PNG Deflate prefix code.");
    }
}
