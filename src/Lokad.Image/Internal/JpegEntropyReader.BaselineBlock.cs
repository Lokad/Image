using System.Buffers.Binary;

namespace Lokad.Image.Internal;

internal ref partial struct JpegEntropyReader
{
    internal bool TryReadCancelableBaselineBlock(ReadOnlySpan<ushort> dcCodes, JpegHuffman ac,
        scoped Span<int> block, ref int predictor, ref int coefficient)
        => ReadCancelableBaselineWindow(dcCodes, ac, block, ref predictor, ref coefficient);

    internal bool TryReadPlainBaselineBlock(ReadOnlySpan<ushort> dcCodes, JpegHuffman ac,
        scoped Span<int> block, ref int predictor, ref int coefficient)
    {
        if (context.Cancellation.CanBeCanceled || data.Length - position < 4) return false;
        var word = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(position, 4));
        if ((unchecked(~word - 0x01010101u) & word & 0x80808080u) != 0) return false;
        var cache = ((ulong)bits << 32 | word) << (32 - available);
        var pending = available + 32; var cursor = position + 4;
        var dc = dcCodes[(int)(cache >> 56)]; var length = dc & 15; var category = dc >> 4;
        if (length == 0 || category > 11) return false;
        var delta = category == 0 ? 0 : ReadHighAlignedSigned(cache << length, category);
        cache <<= length + category; pending -= length + category;
        var next = (long)predictor + delta;
        if (next is < -32768 or > 32767) return false;

        // Only a fully validated DC commits. A later miss hands the remaining
        // AC traversal its exact logical cursor; nothing is replayed.
        predictor = (int)next; block[0] = predictor; var k = 1;
        // Plain production callers supply validated tables and a full block;
        // every normal exit publishes the same canonical cursor below.
        var pairs = ac.BaselinePairs;
        while (k <= 63)
        {
            if (pending < 18)
            {
                if (data.Length - cursor < 4) break;
                word = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(cursor, 4));
                if ((unchecked(~word - 0x01010101u) & word & 0x80808080u) != 0) break;
                cache |= (ulong)word << (32 - pending); pending += 32; cursor += 4;
            }
            if (pairs is not null)
            {
                var pair = pairs[(int)(cache >> 52)];
                var secondIndex = k + (int)((pair >> 8) & 63);
                if (pair != 0 && secondIndex <= 63)
                {
                    var firstIndex = k + (int)((pair >> 4) & 15);
                    block[JpegTransform.Zigzag[firstIndex]] = unchecked((sbyte)(pair >> 16));
                    var second = unchecked((int)pair) >> 24;
                    var consumed = (int)(pair & 15); cache <<= consumed; pending -= consumed;
                    if (second == 0)
                    {
                        if ((pair & 0x8000) != 0) { k = firstIndex + 1; continue; }
                        k = 64; break;
                    }
                    block[JpegTransform.Zigzag[secondIndex]] = second; k = secondIndex + 1;
                    continue;
                }
                if ((pair & 0xc000) != 0)
                {
                    var consumed = (int)(pair & 15);
                    if ((pair & 0x4000) != 0)
                    {
                        cache <<= consumed; pending -= consumed; k = 64; break;
                    }
                    var index = k + (int)((pair >> 4) & 15);
                    if (index > 63) break;
                    block[JpegTransform.Zigzag[index]] = unchecked((int)pair) >> 16;
                    cache <<= consumed; pending -= consumed; k = index + 1;
                    continue;
                }
            }
            var packed = ac.LookupBaselinePrefix(cache, pending); var count = packed & 15;
            var symbol = packed >> 4; var size = symbol & 15; var run = symbol >> 4;
            if (count == 0) break;
            if (symbol == 0) { cache <<= count; pending -= count; k = 64; break; }
            // T.81 F.2.2: leave zero runs, long codes and syntax failures
            // to the original traversal before consuming their prefix.
            if (size is < 1 or > 10 || run >= 64 - k) break;
            var amplitude = ReadHighAlignedSigned(cache << count, size);
            cache <<= count + size; pending -= count + size;
            k += run;
            block[JpegTransform.Zigzag[k++]] = amplitude;
        }
        var wholeBytes = pending >> 3;
        available = pending & 7; bits = (uint)((cache >> 1) >> (63 - available));
        position = cursor - wholeBytes; coefficient = k;
        return true;
    }
}
