namespace Lokad.Image.Internal;

internal ref partial struct JpegEntropyReader
{
    internal void ReadSpectralAcWindow(ReadOnlySpan<ushort> shortCodes, scoped Span<int> block,
        ref int coefficient, int end, int low)
    {
        if (shortCodes.Length != 256 || block.Length != 64 || coefficient < 1
            || coefficient > end || end > 63 || low is < 0 or > 13 || available is < 0 or > 7)
            return;
        var cancellation = context.Cancellation;
        ulong cache = bits; var pending = available; var cursor = position; var k = coefficient;
        while (k <= end)
        {
            // T.81 G.1.2.2: ordinary bytes admit a complete short code and
            // amplitude without raising a future stuffing/marker error.
            if (pending < 18)
            {
                if (data.Length - cursor < 4) break;
                var word = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.Slice(cursor, 4));
                if ((unchecked(~word - 0x01010101u) & word & 0x80808080u) != 0) break;
                cache = cache << 32 | word; pending += 32; cursor += 4;
            }
            var packed = shortCodes[(int)((cache >> (pending - 8)) & 255)]; var count = packed & 15;
            var symbol = packed >> 4; var size = symbol & 15; var run = symbol >> 4;
            // EOB/ZRL, long codes, syntax failures and tails remain wholly
            // unconsumed for the original traversal.
            if (count is < 1 or > 8 || size is < 1 or > 10 || run > end - k) break;
            if (cancellation.IsCancellationRequested)
            {
                PublishLookahead(cache, pending, cursor); coefficient = k;
                cancellation.ThrowIfCancellationRequested();
            }
            pending -= count; k += run;
            // Scalar traversal advances to the destination before reading the
            // amplitude. Preserve that partial coefficient/cursor on cancellation.
            if (cancellation.IsCancellationRequested)
            {
                PublishLookahead(cache, pending, cursor); coefficient = k;
                cancellation.ThrowIfCancellationRequested();
            }
            var value = ReadHighAlignedSigned(cache << (64 - pending), size);
            pending -= size; block[k++] = value << low;
        }
        PublishLookahead(cache, pending, cursor); coefficient = k;
    }
}
