using System.Buffers.Binary;
using System.Runtime.CompilerServices;

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
        // Arrays, prefix lookup and nested constructor scratch total 4056 bytes,
        // independent of declared dimensions or input.
        context.Reserve(4096);
        try
        {
            Span<int> lengths = stackalloc int[320]; Span<int> codeLengths = stackalloc int[19];
            Span<int> counts = stackalloc int[48]; Span<int> symbols = stackalloc int[339];
            Span<ushort> literalLookup = stackalloc ushort[512]; Span<ushort> distanceLookup = stackalloc ushort[32];
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
                BuildLiteralLookup(literals, literalLookup, context); BuildDistanceLookup(distances, distanceLookup, context);
                // Only reader bit charges occur in this region. A noncancelable
                // operation can check those exact charges locally, then publish
                // them on every exit before further parent-context work occurs.
                reader.BeginLocalWork();
                try
                {
                    while (true)
                    {
                        var symbol = DecodeLiteral(ref reader, literals, literalLookup);
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
                            var distanceSymbol = DecodeDistance(ref reader, distances, distanceLookup);
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
                }
                finally { reader.EndLocalWork(); }
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
        private ulong buffer;
        private int available;
        private long remainingWork;
        private long initialWork;
        public Bits(ReadOnlySpan<byte> data, OperationContext context) { this.data = data; this.context = context; remainingWork = -1; }
        public readonly int Position => position;

        public void BeginLocalWork()
        {
            if (!context.Cancellation.CanBeCanceled) initialWork = remainingWork = context.RemainingWork;
        }

        public void EndLocalWork()
        {
            if (remainingWork < 0) return;
            context.Work(initialWork - remainingWork); remainingWork = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Work(int count)
        {
            if (remainingWork < 0) { context.Work(count); return; }
            if (count < 0 || count > remainingWork) throw new ImageLimitException("Computational work limit exceeded.");
            remainingWork -= count;
        }

        public int Read(int count)
        {
            Work(count);
            Fill(count);
            var value = (int)(buffer & ((1u << count) - 1)); buffer >>= count; available -= count; return value;
        }

        public uint Peek(int count)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            Fill(count);
            return (uint)buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Fill(int count)
        {
            // Reads request at most 16 bits. A four-byte refill therefore
            // occupies at most 47 bits; the wider cache never drops high bits.
            while (available < count)
            {
                if (data.Length - position >= 4)
                {
                    buffer |= (ulong)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(position, 4)) << available;
                    position += 4; available += 32; continue;
                }
                if (position == data.Length) throw new ImageDataException("Truncated PNG Deflate stream.");
                buffer |= (ulong)data[position++] << available; available += 8;
            }
        }

        public void Consume(int count)
        {
            Work(count); buffer >>= count; available -= count;
        }

        public void Align()
        {
            // Prefix lookahead can include complete bytes beyond the symbol.
            // Discard only padding bits, returning unread bytes to the input.
            position -= available / 8; buffer = 0; available = 0;
        }
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

    private static void BuildLiteralLookup(scoped Huffman tree, scoped Span<ushort> lookup, OperationContext context)
    {
        // Conservative bounded construction charge: 512 clears/table writes, at most
        // 288 nine-bit reversals and canonical-symbol visits (RFC 1951 3.2.2).
        context.Work(8192); lookup.Clear();
        int code = 0, index = 0;
        for (var length = 1; length <= 9; length++)
        {
            var count = tree.Counts[length];
            for (var i = 0; i < count; i++, code++)
            {
                var reversed = 0;
                for (var bit = 0; bit < length; bit++) reversed = reversed * 2 + ((code >> bit) & 1);
                var entry = (ushort)(tree.Symbols[index + i] << 4 | length);
                for (var slot = reversed; slot < lookup.Length; slot += 1 << length) lookup[slot] = entry;
            }
            index += count; code *= 2;
        }
        // Zero entries include long prefixes and unassigned incomplete paths.
        // Both are handled by the full canonical decoder, never accepted here.
    }

    private static int DecodeLiteral(scoped ref Bits reader, scoped Huffman tree, scoped ReadOnlySpan<ushort> lookup)
    {
        var entry = lookup[(int)(reader.Peek(9) & 511)];
        var length = entry & 15;
        if (length == 0) return Decode(ref reader, tree.Counts, tree.Symbols, tree.Maximum);
        reader.Consume(length); return entry >> 4;
    }

    private static void BuildDistanceLookup(scoped Huffman tree, scoped Span<ushort> lookup, OperationContext context)
    {
        // RFC 1951 section 3.2.2 canonical construction, five-bit root only.
        // 32 clears/writes, at most 32 five-bit reversals and symbol visits
        // fit within the conservative 512-unit construction charge.
        context.Work(512); lookup.Clear();
        int code = 0, index = 0;
        for (var length = 1; length <= 5; length++)
        {
            var count = tree.Counts[length];
            for (var i = 0; i < count; i++, code++)
            {
                var reversed = 0;
                for (var bit = 0; bit < length; bit++) reversed = reversed * 2 + ((code >> bit) & 1);
                var entry = (ushort)(tree.Symbols[index + i] << 4 | length);
                for (var slot = reversed; slot < lookup.Length; slot += 1 << length) lookup[slot] = entry;
            }
            index += count; code *= 2;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int DecodeDistance(scoped ref Bits reader, scoped Huffman tree, scoped ReadOnlySpan<ushort> lookup)
    {
        var entry = lookup[(int)(reader.Peek(5) & 31)]; var length = entry & 15;
        if (length == 0) return Decode(ref reader, tree.Counts, tree.Symbols, tree.Maximum);
        reader.Consume(length); return entry >> 4;
    }

    private static int Decode(scoped ref Bits reader, scoped ReadOnlySpan<int> counts, scoped ReadOnlySpan<int> symbols, int maximum)
    {
        // A bounded lookahead and one work charge per prefix avoid repeating
        // admission/cancellation accounting for every individual code bit.
        // Consume still charges the exact matched length, never the lookahead.
        var bits = reader.Peek(maximum);
        int code = 0, first = 0, index = 0;
        for (var length = 1; length <= maximum; length++)
        {
            // Huffman code bits appear most significant first in the otherwise
            // least-significant-first Deflate bitstream (RFC 1951 section 3.1.1).
            code = code * 2 + (int)(bits & 1); bits >>= 1; var count = counts[length];
            if (code >= first && code - first < count)
            {
                reader.Consume(length); return symbols[index + code - first];
            }
            index += count; first = (first + count) * 2;
        }
        reader.Consume(maximum); throw new ImageDataException("Invalid PNG Deflate prefix code.");
    }
}
