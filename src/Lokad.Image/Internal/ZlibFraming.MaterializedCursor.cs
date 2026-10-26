using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal static partial class ZlibFraming
{
    private ref partial struct Bits
    {
        public readonly bool CanInflateMaterializedCursor() => !cancellation.CanBeCanceled;

        public int InflateMaterializedCursor(scoped Huffman literals, scoped Huffman distances,
            scoped ReadOnlySpan<ushort> literalLookup, scoped ReadOnlySpan<ushort> distanceLookup,
            scoped Span<byte> destination, int output, int window)
        {
            var bytes = data; var cached = buffer; var bits = available;
            var offset = position;             try
            {
                while (true)
                {
                    FillCursor(bytes, ref cached, ref bits, ref offset, 9);
                    var entry = literalLookup[(int)(cached & 511)];
                    var codeBits = entry & 15;
                    int symbol;
                    if (codeBits != 0)
                    {
                        symbol = entry >> 4;
                        cached >>= codeBits; bits -= codeBits;                     }
                    else
                    {
                        // Keep the original canonical decoder and exact error/cursor
                        // behavior for long prefixes, publishing only at this boundary.
                        buffer = cached; available = bits; position = offset;                         try { symbol = DecodeLongLiteral(ref this, literals); }
                        finally { cached = buffer; bits = available; offset = position; }
                    }
                    if (symbol == 256) return output;
                    if (symbol < 256)
                    {
                        if (output == destination.Length) throw new ImageDataException("PNG contains excess decompressed pixels.");
                        destination[output++] = (byte)symbol;
                        continue;
                    }
                    if (symbol > 285) throw new ImageDataException("Invalid PNG Deflate length code.");
                    int length;
                    if (symbol < 265) length = symbol - 254;
                    else if (symbol == 285) length = 258;
                    else
                    {
                        var extra = (symbol - 265) / 4 + 1;
                        length = 3 + (1 << (extra + 2)) + ((symbol - 265) % 4 << extra)
                            + ReadCursor(bytes, ref cached, ref bits, ref offset, extra);
                    }
                    FillCursor(bytes, ref cached, ref bits, ref offset, 5);
                    entry = distanceLookup[(int)(cached & 31)]; codeBits = entry & 15;
                    int distanceSymbol;
                    if (codeBits != 0)
                    {
                        distanceSymbol = entry >> 4;
                        cached >>= codeBits; bits -= codeBits;                     }
                    else
                    {
                        buffer = cached; available = bits; position = offset;                         try { distanceSymbol = Decode(ref this, distances.Counts, distances.Symbols, distances.Maximum); }
                        finally { cached = buffer; bits = available; offset = position; }
                    }
                    if (distanceSymbol > 29) throw new ImageDataException("Invalid PNG Deflate distance code.");
                    var distance = distanceSymbol + 1;
                    if (distanceSymbol >= 4)
                    {
                        var extra = distanceSymbol / 2 - 1;
                        distance = 1 + ((2 + (distanceSymbol & 1)) << extra)
                            + ReadCursor(bytes, ref cached, ref bits, ref offset, extra);
                    }
                    if (distance > output || distance > window) throw new ImageDataException("PNG Deflate distance exceeds history or window.");
                    if (length > destination.Length - output) throw new ImageDataException("PNG contains excess decompressed pixels.");
                    if (distance == 1) destination.Slice(output, length).Fill(destination[output - 1]);
                    else
                    {
                        var copied = Math.Min(length, distance);
                        destination.Slice(output - distance, copied).CopyTo(destination.Slice(output, copied));
                        while (copied < length)
                        {
                            var count = Math.Min(copied, length - copied);
                            destination.Slice(output, count).CopyTo(destination.Slice(output + copied, count));
                            copied += count;
                        }
                    }
                    output += length;
                }
            }
            finally { buffer = cached; available = bits; position = offset; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void FillCursor(scoped ReadOnlySpan<byte> bytes, ref ulong cached,
            ref int bits, ref int offset, int count)
        {
            // count<=16: a four-byte refill occupies at most15+32=47 bits.
            // Full prefix lookahead and short-tail behavior match the original.
            while (bits < count)
            {
                if (bytes.Length - offset >= 4)
                {
                    cached |= (ulong)BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(offset, 4)) << bits;
                    offset += 4; bits += 32;
                }
                else
                {
                    if (offset == bytes.Length) throw new ImageDataException("Truncated PNG Deflate stream.");
                    cached |= (ulong)bytes[offset++] << bits; bits += 8;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ReadCursor(scoped ReadOnlySpan<byte> bytes, ref ulong cached,
            ref int bits, ref int offset, int count)
        {
            FillCursor(bytes, ref cached, ref bits, ref offset, count);
            var value = (int)(cached & ((1u << count) - 1)); cached >>= count; bits -= count;
            return value;
        }
    }
}
