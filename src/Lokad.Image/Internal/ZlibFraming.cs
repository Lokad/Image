using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

// Independently written framing walk. RFC 1950 section 2.2 and
// RFC 1951 sections 3.1 and 3.2 define the bits, canonical trees and numeric ranges.
// This establishes BFINAL, the exact checksum position and the bounded output
// count before raster processing. An optional private output span materializes
// the same symbols without a second Deflate walk; the original validator remains.
internal static partial class ZlibFraming
{
    public static int Validate(ReadOnlySpan<byte> data, long expectedBytes, OperationContext context)
        => Walk(data, expectedBytes, context, default, out _);

    public static int ValidateAndInflate(ReadOnlySpan<byte> data, Span<byte> destination,
        OperationContext context, out bool completeTrees)
        => Walk(data, destination.Length, context, destination, out completeTrees);

    internal static int ValidateAndInflateRgba(ReadOnlySpan<byte> data, long expectedBytes,
        OperationContext context, PngRgbaInflation destination, out bool completeTrees)
        => WalkRgba(data, expectedBytes, context, destination, out completeTrees);

    private static int Walk(ReadOnlySpan<byte> data, long expectedBytes, OperationContext context,
        Span<byte> destination, out bool completeTrees)
    {
        completeTrees = true;
        var materialize = !destination.IsEmpty;
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
                context.Cancellation.ThrowIfCancellationRequested(); final = reader.Read(1) != 0; var type = reader.Read(2);
                if (type == 0)
                {
                    reader.Align(); var length = reader.Read(16); var complement = reader.Read(16);
                    if ((length ^ complement) != 65535) throw new ImageDataException("Invalid PNG Deflate stored length.");
                    if (length > expectedBytes - output) throw new ImageDataException("PNG contains excess decompressed pixels.");
                    var start = reader.Position;
                    reader.SkipBytes(length); context.Cancellation.ThrowIfCancellationRequested();
                    if (materialize) data.Slice(start, length).CopyTo(destination.Slice((int)output, length));
                    output += length; continue;
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
                    completeTrees &= codes.Complete;
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
                            lengths.Slice(position, repeat).Fill(value); position += repeat; context.Cancellation.ThrowIfCancellationRequested();
                        }
                    }
                }
                if (lengths[256] == 0) throw new ImageDataException("PNG Deflate block has no end code.");
                var literals = new Huffman(lengths[..literalCount], counts.Slice(16, 16), symbols.Slice(19, 288), context);
                var distances = new Huffman(lengths.Slice(literalCount, distanceCount), counts.Slice(32, 16), symbols.Slice(307, 32), context);
                // Only complete trees certify the optional materialization path.
                // Single/empty/incomplete alphabets retain BCL acceptance behavior.
                completeTrees &= literals.Complete && distances.Complete;
                materialize &= completeTrees;
                BuildLiteralLookup(literals, literalLookup, context); BuildDistanceLookup(distances, distanceLookup, context);
                {
                    if (!materialize)
                    {
                        output = CountBlock(ref reader, literals, distances, literalLookup, distanceLookup,
                            output, expectedBytes, window);
                        continue;
                    }
                    if (context.Cancellation.CanBeCanceled)
                    {
                        output = InflateCancelableMaterializedBlock(ref reader, literals, distances, literalLookup,
                            distanceLookup, destination, output, expectedBytes, window);
                        continue;
                    }
                    if (expectedBytes == destination.Length && reader.CanInflateMaterializedCursor())
                    {
                        output = reader.InflateMaterializedCursor(literals, distances, literalLookup,
                            distanceLookup, destination, (int)output, window);
                        continue;
                    }
                    while (true)
                    {
                        output += materialize
                            ? reader.CopyLiteralRun(literalLookup, destination[(int)output..])
                            : reader.ReadLiteralRun(literalLookup, expectedBytes - output);
                        var symbol = DecodeLiteral(ref reader, literals, literalLookup);
                        if (symbol == 256) break;
                        int length, distance = 0;
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
                            distance = distanceSymbol + 1;
                            if (distanceSymbol >= 4)
                            {
                                var extra = distanceSymbol / 2 - 1;
                                distance = 1 + ((2 + (distanceSymbol & 1)) << extra) + reader.Read(extra);
                            }
                            if (distance > output || distance > window) throw new ImageDataException("PNG Deflate distance exceeds history or window.");
                        }
                        if (length > expectedBytes - output) throw new ImageDataException("PNG contains excess decompressed pixels.");
                        if (materialize)
                        {
                            var start = (int)output;
                            if (symbol < 256) destination[start] = (byte)symbol;
                            else if (distance == 1) destination.Slice(start, length).Fill(destination[start - 1]);
                            else
                            {
                                // RFC 1951 history copies may overlap. First copy
                                // available history, then double the valid prefix.
                                var copied = Math.Min(length, distance);
                                destination.Slice(start - distance, copied).CopyTo(destination.Slice(start, copied));
                                while (copied < length)
                                {
                                    var count = Math.Min(copied, length - copied);
                                    destination.Slice(start, count).CopyTo(destination.Slice(start + copied, count));
                                    copied += count;
                                }
                            }
                        }
                        output += length;
                    }
                }
            } while (!final);
            if (output != expectedBytes) throw new ImageDataException("Truncated PNG pixel data.");
            reader.Align(); var checksum = reader.Position; reader.SkipBytes(4);
            // PNG section 11.2.3 permits unused trailing bytes in the final IDAT.
            return checksum + 4;
        }
        finally { context.Release(4096); }
    }

    private static int WalkRgba(ReadOnlySpan<byte> data, long expectedBytes, OperationContext context,
        PngRgbaInflation destination, out bool completeTrees)
    {
        completeTrees = true;
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
                context.Cancellation.ThrowIfCancellationRequested(); final = reader.Read(1) != 0; var type = reader.Read(2);
                if (type == 0)
                {
                    reader.Align(); var length = reader.Read(16); var complement = reader.Read(16);
                    if ((length ^ complement) != 65535) throw new ImageDataException("Invalid PNG Deflate stored length.");
                    if (length > expectedBytes - output) throw new ImageDataException("PNG contains excess decompressed pixels.");
                    var start = reader.Position;
                    reader.SkipBytes(length); context.Cancellation.ThrowIfCancellationRequested();
                    if (completeTrees) destination.Stored(data.Slice(start, length));
                    output += length; continue;
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
                    completeTrees &= codes.Complete;
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
                            lengths.Slice(position, repeat).Fill(value); position += repeat; context.Cancellation.ThrowIfCancellationRequested();
                        }
                    }
                }
                if (lengths[256] == 0) throw new ImageDataException("PNG Deflate block has no end code.");
                var literals = new Huffman(lengths[..literalCount], counts.Slice(16, 16), symbols.Slice(19, 288), context);
                var distances = new Huffman(lengths.Slice(literalCount, distanceCount), counts.Slice(32, 16), symbols.Slice(307, 32), context);
                // Only complete trees certify the optional materialization path.
                // Single/empty/incomplete alphabets retain BCL acceptance behavior.
                completeTrees &= literals.Complete && distances.Complete;
                BuildLiteralLookup(literals, literalLookup, context); BuildDistanceLookup(distances, distanceLookup, context);
                {
                    output = completeTrees
                        ? InflateStreamingBlock(ref reader, literals, distances, literalLookup, distanceLookup,
                            output, expectedBytes, window, destination)
                        : CountBlock(ref reader, literals, distances, literalLookup, distanceLookup,
                            output, expectedBytes, window);
                }
            } while (!final);
            if (output != expectedBytes) throw new ImageDataException("Truncated PNG pixel data.");
            reader.Align(); var checksum = reader.Position; reader.SkipBytes(4);
            // PNG section 11.2.3 permits unused trailing bytes in the final IDAT.
            return checksum + 4;
        }
        finally { context.Release(4096); }
    }

    private static long InflateCancelableMaterializedBlock(scoped ref Bits reader, scoped Huffman literals, scoped Huffman distances,
        scoped ReadOnlySpan<ushort> literalLookup, scoped ReadOnlySpan<ushort> distanceLookup, scoped Span<byte> destination,
        long output, long expectedBytes, int window)
    {
        // Decode each symbol once. Only an admitted literal starts a batched
        // literal run, avoiding another literal probe before every match.
        while (true)
        {
            var symbol = DecodeLiteral(ref reader, literals, literalLookup);
            if (symbol == 256) return output;
            if (symbol < 256)
            {
                if (1 > expectedBytes - output) throw new ImageDataException("PNG contains excess decompressed pixels.");
                destination[(int)output++] = (byte)symbol;
                output += reader.CopyStreamingLiteralRun(literalLookup, destination[(int)output..]);
                continue;
            }
            if (symbol > 285) throw new ImageDataException("Invalid PNG Deflate length code.");
            int length;
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
            if (length > expectedBytes - output) throw new ImageDataException("PNG contains excess decompressed pixels.");
            var start = (int)output;
            if (distance == 1) destination.Slice(start, length).Fill(destination[start - 1]);
            else
            {
                var copied = Math.Min(length, distance);
                destination.Slice(start - distance, copied).CopyTo(destination.Slice(start, copied));
                while (copied < length)
                {
                    var count = Math.Min(copied, length - copied);
                    destination.Slice(start, count).CopyTo(destination.Slice(start + copied, count));
                    copied += count;
                }
            }
            output += length;
        }
    }

    private static long CountBlock(scoped ref Bits reader, scoped Huffman literals, scoped Huffman distances,
        scoped ReadOnlySpan<ushort> literalLookup, scoped ReadOnlySpan<ushort> distanceLookup,
        long output, long expectedBytes, int window)
    {
        // Framing needs counts and history bounds, without materialized output.
        // Keep this loop separate from output-copy spans and their lifetime so
        // the JIT can retain the reader's hot counting state independently.
        while (true)
        {
            output += reader.ReadLiteralRun(literalLookup, expectedBytes - output);
            var symbol = DecodeLiteral(ref reader, literals, literalLookup);
            if (symbol == 256) return output;
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

    private static long InflateStreamingBlock(scoped ref Bits reader, scoped Huffman literals, scoped Huffman distances,
        scoped ReadOnlySpan<ushort> literalLookup, scoped ReadOnlySpan<ushort> distanceLookup,
        long output, long expectedBytes, int window, PngRgbaInflation destination)
    {
        // Decode the same framing symbols and retain their exact charges. The
        // rolling output only reconstructs private rows; it cannot publish a
        // raster/filter/checksum decision before the complete framing walk.
        while (true)
        {
            var writable = destination.Writable;
            var count = reader.CopyStreamingLiteralRun(literalLookup,
                writable[..(int)Math.Min(writable.Length, expectedBytes - output)]);
            destination.Advance(count); output += count;
            var symbol = DecodeLiteral(ref reader, literals, literalLookup);
            if (symbol == 256) return output;
            int length, distance = 0;
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
                distance = distanceSymbol + 1;
                if (distanceSymbol >= 4)
                {
                    var extra = distanceSymbol / 2 - 1;
                    distance = 1 + ((2 + (distanceSymbol & 1)) << extra) + reader.Read(extra);
                }
                if (distance > output || distance > window) throw new ImageDataException("PNG Deflate distance exceeds history or window.");
            }
            if (length > expectedBytes - output) throw new ImageDataException("PNG contains excess decompressed pixels.");
            if (symbol < 256) destination.Literal((byte)symbol);
            else destination.Match(length, distance);
            output += length;
        }
    }

    private ref partial struct Bits
    {
        private readonly ReadOnlySpan<byte> data;
        private readonly OperationContext context;
        private readonly CancellationToken cancellation;
        private int position;
        private ulong buffer;
        private int available;
        public Bits(ReadOnlySpan<byte> data, OperationContext context)
        {
            this.data = data; this.context = context; cancellation = context.Cancellation;         }
        public readonly int Position => position;

        public long ReadLiteralRun(scoped ReadOnlySpan<ushort> lookup, long maximum)
        {
            // Framing needs only the output count for literal bytes. Walk their
            // RFC 1951 prefixes together, keeping the bit cache in locals rather
            // than re-entering the length/distance dispatcher for each byte.
            // Cancelable operations retain per-symbol checks in a separate loop.
            if (maximum <= 0) return 0;
            if (cancellation.CanBeCanceled) return ReadCancelableLiteralRun(lookup, maximum);
            var cached = buffer; var bits = available; var offset = position;
long count = 0;
            while (count < maximum)
            {
                if (bits < 9)
                {
                    // Short tails retain Fill's original truncation precedence.
                    if (data.Length - offset < 4) break;
                    cached |= (ulong)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset, 4)) << bits;
                    offset += 4; bits += 32;
                }
                var entry = lookup[(int)(cached & 511)]; var length = entry & 15;
                // Leave long/unassigned prefixes, EOB, matches and output overflow to the original dispatcher.
                if (length == 0 || entry >> 4 >= 256) break;
                cached >>= length; bits -= length; count++;
            }
            buffer = cached; available = bits; position = offset;             return count;
        }

        private long ReadCancelableLiteralRun(scoped ReadOnlySpan<ushort> lookup, long maximum)
        {
            var cached = buffer; var bits = available; var offset = position;
            var cancellation = this.cancellation;
long count = 0;
            while (count < maximum)
            {
                // Poll at the bounded prefix boundaries. On cancellation,
                // a short tail, commit the
                // preceding literals and let the original dispatcher fail.
                // No format error throws here.
                if (cancellation.IsCancellationRequested) break;
                if (bits < 9)
                {
                    if (data.Length - offset < 4) break;
                    cached |= (ulong)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset, 4)) << bits;
                    offset += 4; bits += 32;
                }
                var entry = lookup[(int)(cached & 511)]; var length = entry & 15;
                if (length == 0 || entry >> 4 >= 256) break;
                if (cancellation.IsCancellationRequested) break;
cached >>= length; bits -= length; count++;
            }
            buffer = cached; available = bits; position = offset;             return count;
        }

        public int CopyStreamingLiteralRun(scoped ReadOnlySpan<ushort> lookup, scoped Span<byte> destination)
            => cancellation.CanBeCanceled ? CopyCancelableLiteralRun(lookup, destination) : CopyLiteralRun(lookup, destination);

        private int CopyCancelableLiteralRun(scoped ReadOnlySpan<ushort> lookup, scoped Span<byte> destination)
        {
            var cached = buffer; var bits = available; var offset = position;
            var cancellation = this.cancellation;
var count = 0;
            while (count < destination.Length)
            {
                // Poll at the bounded prefix boundaries. On cancellation,
                // a short tail, commit the
                // preceding literals and let the original dispatcher fail.
                // No format error throws here.
                if (cancellation.IsCancellationRequested) break;
                if (bits < 9)
                {
                    if (data.Length - offset < 4) break;
                    cached |= (ulong)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset, 4)) << bits;
                    offset += 4; bits += 32;
                }
                var entry = lookup[(int)(cached & 511)]; var length = entry & 15;
                if (length == 0 || entry >> 4 >= 256) break;
                if (cancellation.IsCancellationRequested) break;
                destination[count++] = (byte)(entry >> 4);
cached >>= length; bits -= length;
            }
            buffer = cached; available = bits; position = offset;             return count;
        }

        public int CopyLiteralRun(scoped ReadOnlySpan<ushort> lookup, scoped Span<byte> destination)
        {
            if (cancellation.CanBeCanceled || destination.IsEmpty) return 0;
            var cached = buffer; var bits = available; var offset = position;
var count = 0;
            while (count < destination.Length)
            {
                if (bits < 9)
                {
                    if (data.Length - offset < 4) break;
                    cached |= (ulong)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset, 4)) << bits;
                    offset += 4; bits += 32;
                }
                var entry = lookup[(int)(cached & 511)]; var length = entry & 15;
                if (length == 0 || entry >> 4 >= 256) break;
                destination[count++] = (byte)(entry >> 4);
                cached >>= length; bits -= length;             }
            buffer = cached; available = bits; position = offset;             return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(int count)
        {
            cancellation.ThrowIfCancellationRequested();
            Fill(count);
            var value = (int)(buffer & ((1u << count) - 1)); buffer >>= count; available -= count; return value;
        }

        public uint Peek(int count)
        {
            cancellation.ThrowIfCancellationRequested();
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
            cancellation.ThrowIfCancellationRequested(); buffer >>= count; available -= count;
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
        public readonly int FirstLongCode;
        public readonly int FirstLongSymbol;
        public readonly bool Complete;

        public Huffman(ReadOnlySpan<int> lengths, Span<int> counts, Span<int> symbols, OperationContext context)
        {
            Counts = counts; Symbols = symbols; counts.Clear(); Maximum = 0; FirstLongCode = 0; context.Cancellation.ThrowIfCancellationRequested();
            foreach (var length in lengths) if (length > 0) { counts[length]++; Maximum = Math.Max(Maximum, length); }
            var space = 1;
            for (var length = 1; length <= Maximum; length++)
            {
                space = space * 2 - counts[length];
                if (space < 0) throw new ImageDataException("Oversubscribed PNG Deflate prefix codes.");
                // Assigned nine-bit prefixes occupy 512 - space positions.
                // The next canonical depth starts at twice that position.
                if (length == 9) FirstLongCode = 1024 - 2 * space;
            }
            Complete = space == 0;
            // Incomplete trees are usable when every actual symbol has a code;
            // any unassigned path fails in Decode. Empty distances are legal for
            // literal-only dynamic blocks, and cannot decode a match.
            Span<int> offsets = stackalloc int[16]; offsets.Clear();
            for (var length = 1; length < 15; length++) offsets[length + 1] = offsets[length] + counts[length];
            FirstLongSymbol = offsets[10];
            for (var symbol = 0; symbol < lengths.Length; symbol++)
                if (lengths[symbol] != 0) symbols[offsets[lengths[symbol]]++] = symbol;
        }
    }

    private static void BuildLiteralLookup(scoped Huffman tree, scoped Span<ushort> lookup, OperationContext context)
    {
        // Conservative bounded construction charge: 512 clears/table writes, at most
        // 288 nine-bit reversals and canonical-symbol visits (RFC 1951 3.2.2).
        context.Cancellation.ThrowIfCancellationRequested(); lookup.Clear();
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
        // Both are handled by the canonical fallback, never accepted here.
    }

    private static int DecodeLiteral(scoped ref Bits reader, scoped Huffman tree, scoped ReadOnlySpan<ushort> lookup)
    {
        var entry = lookup[(int)(reader.Peek(9) & 511)];
        var length = entry & 15;
        if (length == 0) return DecodeLongLiteral(ref reader, tree);
        reader.Consume(length); return entry >> 4;
    }

    private static int DecodeLongLiteral(scoped ref Bits reader, scoped Huffman tree)
    {
        if (tree.Maximum <= 9) return Decode(ref reader, tree.Counts, tree.Symbols, tree.Maximum);
        // A root miss has already excluded every assigned code through depth
        // nine. Keep the original full lookahead and exact matched-bit consumption,
        // but begin the canonical search at depth ten (RFC 1951 section 3.2.2).
        var bits = reader.Peek(tree.Maximum);
        var reversed = (uint)bits;
        reversed = (reversed & 0x55555555) << 1 | (reversed >> 1 & 0x55555555);
        reversed = (reversed & 0x33333333) << 2 | (reversed >> 2 & 0x33333333);
        reversed = (reversed & 0x0f0f0f0f) << 4 | (reversed >> 4 & 0x0f0f0f0f);
        var code = (int)(BinaryPrimitives.ReverseEndianness(reversed) >> 23);
        bits >>= 9;
        var first = tree.FirstLongCode; var index = tree.FirstLongSymbol;
        for (var length = 10; length <= tree.Maximum; length++)
        {
            code = code * 2 + (int)(bits & 1); bits >>= 1; var count = tree.Counts[length];
            if (code >= first && code - first < count)
            {
                reader.Consume(length); return tree.Symbols[index + code - first];
            }
            index += count; first = (first + count) * 2;
        }
        reader.Consume(tree.Maximum); throw new ImageDataException("Invalid PNG Deflate prefix code.");
    }

    private static void BuildDistanceLookup(scoped Huffman tree, scoped Span<ushort> lookup, OperationContext context)
    {
        // RFC 1951 section 3.2.2 canonical construction, five-bit root only.
        // The five-bit root has at most 32 entries.
        context.Cancellation.ThrowIfCancellationRequested(); lookup.Clear();
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
        // Bounded lookahead avoids refilling for each individual code bit.
        // Consume advances only the exact matched length, never the lookahead.
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
