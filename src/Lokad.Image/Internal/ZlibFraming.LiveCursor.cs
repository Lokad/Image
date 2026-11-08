using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal static partial class ZlibFraming
{
    private ref partial struct Bits
    {
        public int InflateLiveCursor(scoped Huffman literals, scoped Huffman distances,
            scoped ReadOnlySpan<ushort> literalLookup, scoped ReadOnlySpan<ushort> distanceLookup,
            scoped Span<byte> destination, int output, int window)
        {
            var bytes = data; var cached = buffer; var bits = available; var offset = position;
            var cancellation = this.cancellation;
            // Walk discards this private reader on failure. Keep completed state
            // exact; outer framing/codec owners still clean up every exception.
            while (true)
            {
                // Exactly the original Peek(9), then Consume on a root hit.
                // A miss delegates its full lookahead and polling unchanged.
                cancellation.ThrowIfCancellationRequested();
                FillCursor(bytes, ref cached, ref bits, ref offset, 9);
                var entry = literalLookup[(int)(cached & 1023)];
                var codeBits = entry & 15;
                int symbol;
                if (codeBits is > 0 and <= 9)
                {
                    cancellation.ThrowIfCancellationRequested();
                    symbol = entry >> 4; cached >>= codeBits; bits -= codeBits;
                }
                else if (codeBits == 10 && bits >= literals.Maximum)
                {
                    // The original long decoder Peeks its full maximum depth
                    // before trusting the tenth bit. That lookahead is already
                    // cached here; retain its Peek and Consume checkpoints.
                    cancellation.ThrowIfCancellationRequested();
                    symbol = entry >> 4;
                    cancellation.ThrowIfCancellationRequested();
                    cached >>= 10; bits -= 10;
                }
                else
                {
                    buffer = cached; available = bits; position = offset;
                    symbol = DecodeLongLiteral(ref this, literals, literalLookup);
                    cached = buffer; bits = available; offset = position;
                }
                if (symbol == 256)
                {
                    buffer = cached; available = bits; position = offset;
                    return output;
                }
                if (symbol < 256)
                {
                    if (output == destination.Length) throw new ImageDataException("PNG contains excess decompressed pixels.");
                    destination[output++] = (byte)symbol;
                    // Borrow the unchanged literal-run kernel, including its
                    // two polls, short-tail behavior and exact partial stores.
                    buffer = cached; available = bits; position = offset;
                    output += CopyStreamingLiteralRun(literalLookup, destination[output..]);
                    cached = buffer; bits = available; offset = position;
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
                        + ReadLiveCursor(bytes, cancellation, ref cached, ref bits, ref offset, extra);
                }
                cancellation.ThrowIfCancellationRequested();
                FillCursor(bytes, ref cached, ref bits, ref offset, 5);
                entry = distanceLookup[(int)(cached & 31)]; codeBits = entry & 15;
                int distanceSymbol;
                if (codeBits != 0)
                {
                    cancellation.ThrowIfCancellationRequested();
                    distanceSymbol = entry >> 4; cached >>= codeBits; bits -= codeBits;
                }
                else
                {
                    buffer = cached; available = bits; position = offset;
                    distanceSymbol = Decode(ref this, distances.Counts, distances.Symbols, distances.Maximum);
                    cached = buffer; bits = available; offset = position;
                }
                if (distanceSymbol > 29) throw new ImageDataException("Invalid PNG Deflate distance code.");
                var distance = distanceSymbol + 1;
                if (distanceSymbol >= 4)
                {
                    var extra = distanceSymbol / 2 - 1;
                    distance = 1 + ((2 + (distanceSymbol & 1)) << extra)
                        + ReadLiveCursor(bytes, cancellation, ref cached, ref bits, ref offset, extra);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ReadLiveCursor(scoped ReadOnlySpan<byte> bytes, CancellationToken cancellation,
            ref ulong cached, ref int bits, ref int offset, int count)
        {
            // Read has one checkpoint, unlike the separate Peek/Consume pair.
            cancellation.ThrowIfCancellationRequested();
            return ReadCursor(bytes, ref cached, ref bits, ref offset, count);
        }
    }
}
