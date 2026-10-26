using Lokad.Image.Internal;

namespace Lokad.Image;

public sealed partial class TrueTypeFont
{
    // Microsoft OpenType cmap: independent bounds-checked format 4/12 lookup.
    // Directory search hints are derived, never trusted as indexes.
    private TableRange SelectMapping(OperationContext context)
    {
        var table = Required("cmap"u8, context); var bytes = Span(table);
        if (bytes.Length < 4 || U16(bytes) != 0) throw new ImageDataException("Invalid font character mapping header.");
        var records = U16(bytes[2..]); Items(records, context);
        if (records * 8L + 4 > bytes.Length) throw new ImageDataException("Truncated font character mapping records.");
        var chosen = -1; var score = -1;
        for (var i = 0; i < records; i++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var entry = bytes.Slice(4 + i * 8, 8);
            var platform = U16(entry); var encoding = U16(entry[2..]); var offset = U32(entry[4..]);
            if (offset < 4 + records * 8 || offset > bytes.Length - 2L) throw new ImageDataException("Invalid font character mapping offset.");
            var format = U16(bytes[(int)offset..]);
            if (!(platform == 0 && encoding != 5 || platform == 3 && encoding is 1 or 10) || format is not (4 or 12)) continue;
            var candidateScore = (format == 12 ? 100 : 0) + (platform == 3 ? 1 : 0);
            if (candidateScore > score) { chosen = (int)offset; score = candidateScore; }
        }
        if (chosen < 0) throw new UnsupportedImageException(UnsupportedImageFeature.FontMapping, "A Unicode cmap format 4 or 12 is required.");
        var formatNumber = U16(bytes[chosen..]);
        if (bytes.Length - chosen < 16) throw new ImageDataException("Truncated selected font mapping.");
        var length = formatNumber == 4 ? U16(bytes[(chosen + 2)..]) : U32(bytes[(chosen + 4)..]);
        if (length < 16 || length > bytes.Length - (long)chosen) throw new ImageDataException("Invalid selected font mapping length.");
        return new(table.Offset + chosen, (int)length);
    }

    private void ValidateMapping(OperationContext context)
    {
        var bytes = Span(cmap);
        if (cmapFormat == 12)
        {
            var count = U32(bytes[12..]); Items(count, context);
            if (U16(bytes[2..]) != 0 || 16L + count * 12L > bytes.Length) throw new ImageDataException("Invalid Unicode group table.");
            long previous = -1;
            for (var i = 0; i < count; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var group = bytes.Slice(16 + i * 12, 12);
                var first = U32(group); var last = U32(group[4..]); var glyph = U32(group[8..]);
                if (last < first || first <= previous || last > 0x10FFFF || glyph + (ulong)last - first >= (ulong)GlyphCount)
                    throw new ImageDataException("Invalid Unicode mapping group.");
                previous = last;
            }
        }
        else
        {
            var twice = U16(bytes[6..]); var count = twice / 2; Items(count, context);
            if (twice == 0 || (twice & 1) != 0 || 16L + count * 8L > bytes.Length || U16(bytes[(14 + count * 2)..]) != 0)
                throw new ImageDataException("Invalid Unicode segment table.");
            var previous = -1;
            for (var i = 0; i < count; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var end = U16(bytes[(14 + i * 2)..]); var start = U16(bytes[(16 + count * 2 + i * 2)..]);
                var field = 16 + count * 6 + i * 2; var offset = U16(bytes[field..]);
                if (start > end || start <= previous || (offset & 1) != 0 || offset != 0 &&
                    (field + offset < 16 + count * 8 || field + (long)offset + (end - start) * 2L + 2 > bytes.Length))
                    throw new ImageDataException("Invalid Unicode segment range.");
                for (var code = (int)start; code <= end; code++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    if (MapSegment(bytes, count, i, code) >= GlyphCount) throw new ImageDataException("Font mapping refers to an absent glyph.");
                }
                previous = end;
            }
            if (previous != 0xFFFF) throw new ImageDataException("Unicode segment table has no terminal segment.");
        }
    }

    private int Map(int scalar, OperationContext context)
    {
        var bytes = Span(cmap); var low = 0; var high = mappingCount - 1;
        if (cmapFormat == 4)
        {
            if (scalar > 0xFFFF) { context.Cancellation.ThrowIfCancellationRequested(); return 0; }
            while (low <= high)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var mid = (low + high) / 2;
                if (U16(bytes[(14 + mid * 2)..]) < scalar) low = mid + 1; else high = mid - 1;
            }
            return low == mappingCount || scalar < U16(bytes[(16 + mappingCount * 2 + low * 2)..]) ? 0 : MapSegment(bytes, mappingCount, low, scalar);
        }
        while (low <= high)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var mid = (low + high) / 2; var group = bytes.Slice(16 + mid * 12, 12);
            var first = U32(group); var last = U32(group[4..]);
            if (scalar < first) high = mid - 1;
            else if (scalar > last) low = mid + 1;
            else return (int)(U32(group[8..]) + scalar - first);
        }
        return 0;
    }

    private static int MapSegment(ReadOnlySpan<byte> bytes, int count, int segment, int scalar)
    {
        var delta = I16(bytes[(16 + count * 4 + segment * 2)..]); var field = 16 + count * 6 + segment * 2;
        var offset = U16(bytes[field..]);
        if (offset == 0) return (scalar + delta) & 0xFFFF;
        var start = U16(bytes[(16 + count * 2 + segment * 2)..]);
        var glyph = U16(bytes[(field + offset + (scalar - start) * 2)..]);
        return glyph == 0 ? 0 : (glyph + delta) & 0xFFFF;
    }
}
