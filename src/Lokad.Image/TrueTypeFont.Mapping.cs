using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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
            // The preceding byte extent proves count fits a managed index.
            // A noncancelable scan can classify private, immutable groups in
            // batches. Any failure returns to the original ordered validator.
            if (!context.Cancellation.CanBeCanceled && Avx2.IsSupported
                && TryValidateGroups12(bytes[16..], (int)count, GlyphCount)) return;
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

    private static bool TryValidateGroups12(ReadOnlySpan<byte> groups, int count, int glyphCount)
    {
        if (count < 8 || count > groups.Length / 12) return false;
        var swap = Vector256.Create((byte)3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12,
            3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12);
        var unicodeLimit = Vector256.Create(0x110000); var glyphLimit = Vector256.Create(glyphCount);
        var minusOne = Vector256.Create(-1); var previous = -1; var i = 0;
        for (; i <= count - 8; i += 8)
        {
            // Exactly eight 12-byte records; no expanded read or decoded table.
            var blocks = MemoryMarshal.Cast<byte, Vector256<byte>>(groups.Slice(i * 12, 96));
            var a = Avx2.Shuffle(blocks[0], swap).AsInt32();
            var b = Avx2.Shuffle(blocks[1], swap).AsInt32();
            var c = Avx2.Shuffle(blocks[2], swap).AsInt32();
            TransposeGroups12(a, b, c, out var first, out var last, out var glyph);
            var preceding = Avx2.PermuteVar8x32(last, Vector256.Create(0, 0, 1, 2, 3, 4, 5, 6)).WithElement(0, previous);
            // Signed order from previous=-1/a valid end, together with last>=first,
            // inductively establishes nonnegative starts/ends in every lane.
            var valid = Avx2.And(Avx2.CompareGreaterThan(first, preceding),
                Avx2.AndNot(Avx2.CompareGreaterThan(first, last), Avx2.CompareGreaterThan(unicodeLimit, last)));
            valid = Avx2.And(valid, Avx2.And(Avx2.CompareGreaterThan(glyph, minusOne), Avx2.CompareGreaterThan(glyphLimit, glyph)));
            // On accepted structural/glyph lanes, delta<=0x10ffff and glyph<65536,
            // so this addition fits Int32. Other wrapped lanes cannot pass valid.
            var finalGlyph = Avx2.Add(glyph, Avx2.Subtract(last, first));
            valid = Avx2.And(valid, Avx2.CompareGreaterThan(glyphLimit, finalGlyph));
            if (Avx2.MoveMask(valid.AsByte()) != -1) return false;
            previous = last.GetElement(7);
        }
        for (; i < count; i++)
        {
            var group = groups.Slice(i * 12, 12);
            var first = U32(group); var last = U32(group[4..]); var glyph = U32(group[8..]);
            if (last < first || first <= previous || last > 0x10FFFF || glyph + (ulong)last - first >= (ulong)glyphCount)
                return false;
            previous = (int)last;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransposeGroups12(Vector256<int> a, Vector256<int> b, Vector256<int> c,
        out Vector256<int> first, out Vector256<int> last, out Vector256<int> glyph)
    {
        first = Avx2.Blend(Avx2.Blend(
            Avx2.PermuteVar8x32(a, Vector256.Create(0, 3, 6, 0, 0, 0, 0, 0)),
            Avx2.PermuteVar8x32(b, Vector256.Create(0, 0, 0, 1, 4, 7, 0, 0)), 0x38),
            Avx2.PermuteVar8x32(c, Vector256.Create(0, 0, 0, 0, 0, 0, 2, 5)), 0xc0);
        last = Avx2.Blend(Avx2.Blend(
            Avx2.PermuteVar8x32(a, Vector256.Create(1, 4, 7, 0, 0, 0, 0, 0)),
            Avx2.PermuteVar8x32(b, Vector256.Create(0, 0, 0, 2, 5, 0, 0, 0)), 0x18),
            Avx2.PermuteVar8x32(c, Vector256.Create(0, 0, 0, 0, 0, 0, 3, 6)), 0xe0);
        glyph = Avx2.Blend(Avx2.Blend(
            Avx2.PermuteVar8x32(a, Vector256.Create(2, 5, 0, 0, 0, 0, 0, 0)),
            Avx2.PermuteVar8x32(b, Vector256.Create(0, 0, 0, 3, 6, 0, 0, 0)), 0x1c),
            Avx2.PermuteVar8x32(c, Vector256.Create(0, 0, 0, 0, 0, 1, 4, 7)), 0xe0);
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
