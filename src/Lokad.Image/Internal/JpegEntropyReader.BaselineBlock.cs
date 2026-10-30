using System.Buffers.Binary;

namespace Lokad.Image.Internal;

internal ref partial struct JpegEntropyReader
{
    internal bool TryReadPlainBaselineBlock(ReadOnlySpan<ushort> dcCodes, ReadOnlySpan<ushort> acCodes,
        scoped Span<int> block, ref int predictor, ref int coefficient)
    {
        if (context.Cancellation.CanBeCanceled || data.Length - position < 4) return false;
        var word = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(position, 4));
        if ((unchecked(~word - 0x01010101u) & word & 0x80808080u) != 0) return false;
        var cache = (ulong)bits << 32 | word; var pending = available + 32; var cursor = position + 4;
        var dc = dcCodes[(int)((cache >> (pending - 8)) & 255)]; var length = dc & 15; var category = dc >> 4;
        if (length == 0 || category > 11) return false;
        pending -= length + category;
        var amplitude = (int)((cache >> pending) & ((1u << category) - 1));
        var delta = 0;
        if (category != 0)
        {
            var threshold = 1 << (category - 1);
            delta = amplitude + (((amplitude - threshold) >> 31) & (1 - (threshold << 1)));
        }
        var next = (long)predictor + delta;
        if (next is < -32768 or > 32767) return false;

        // Only a fully validated DC commits. A later miss hands the remaining
        // AC traversal its exact logical cursor; nothing is replayed.
        predictor = (int)next; block[0] = predictor; var k = 1;
        try
        {
            while (k <= 63)
            {
                if (pending < 18)
                {
                    if (data.Length - cursor < 4) return true;
                    word = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(cursor, 4));
                    if ((unchecked(~word - 0x01010101u) & word & 0x80808080u) != 0) return true;
                    cache = cache << 32 | word; pending += 32; cursor += 4;
                }
                var packed = acCodes[(int)((cache >> (pending - 8)) & 255)]; var count = packed & 15;
                var symbol = packed >> 4; var size = symbol & 15; var run = symbol >> 4;
                if (count == 0) return true;
                if (symbol == 0) { pending -= count; k = 64; return true; }
                // T.81 F.2.2: leave zero runs, long codes and syntax failures
                // to the original traversal before consuming their prefix.
                if (size is < 1 or > 10 || run >= 64 - k) return true;
                pending -= count + size;
                amplitude = (int)((cache >> pending) & ((1u << size) - 1)); var threshold = 1 << (size - 1);
                k += run;
                block[JpegTransform.Zigzag[k++]] = amplitude + (((amplitude - threshold) >> 31) & (1 - (threshold << 1)));
            }
            return true;
        }
        finally
        {
            var wholeBytes = pending >> 3;
            bits = unchecked((uint)(cache >> (wholeBytes * 8))); available = pending & 7;
            position = cursor - wholeBytes; coefficient = k;
        }
    }
}
