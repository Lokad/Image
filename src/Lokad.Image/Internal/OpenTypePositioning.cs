using System.Numerics;

namespace Lokad.Image.Internal;

// Independent unhinted horizontal GPOS interpreter. Format facts:
// https://learn.microsoft.com/en-us/typography/opentype/spec/gpos
// Font tables remain borrowed. Relative mark anchors are resolved only after
// advance adjustments, so subsequent movement of a parent moves its marks too.
internal sealed class OpenTypePositioning : IDisposable
{
    private readonly OpenTypeTable table;
    private readonly OpenTypeGlyphClasses classes;
    private readonly OperationContext context;
    private readonly int lookupList;
    private byte[] selected;
    private bool disposed;
    private int runStart, runEnd;

    internal OpenTypePositioning(ReadOnlyMemory<byte> positioning, ReadOnlyMemory<byte> definitions,
        int glyphCount, OperationContext context)
    {
        this.context = context; table = new(positioning, glyphCount, context);
        classes = new(positioning.IsEmpty ? ReadOnlyMemory<byte>.Empty : definitions, glyphCount, context);
        if (table.Empty) { selected = []; return; }
        lookupList = table.LookupList(); var count = table.Count(lookupList, 2);
        context.Reserve(count + 32L);
        try { selected = new byte[count]; }
        catch { context.Release(count + 32L); throw; }
    }

    private void Check(ShapingBuffer glyphs, int start, int length)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!ReferenceEquals(glyphs.Context, context)) throw new ArgumentException("Shaping storage must share the operation context.", nameof(glyphs));
        if (start < 0 || start > glyphs.Count || length < -1 || length > glyphs.Count - start)
            throw new ArgumentOutOfRangeException(nameof(length));
        context.Work(0);
    }
    internal bool ApplyFeature(uint script, uint language, uint feature, ShapingBuffer glyphs,
        uint mask = 0, int start = 0, int length = -1, bool required = false)
    {
        Check(glyphs, start, length);
        if (table.Empty || !table.Select(script, language, feature, selected, required)) return false;
        ApplySelected(glyphs, mask, start, length); return true;
    }
    internal bool HasFeature(uint script, uint language, uint feature)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return !table.Empty && (table.RequiredTag(script, language) == feature || table.Select(script, language, feature, selected));
    }
    internal bool ApplyStandardFeatures(uint script, uint language, ShapingBuffer glyphs, bool kerning = true, int start = 0, int length = -1)
    {
        Check(glyphs, start, length);
        if (table.Empty) return false;
        selected.AsSpan().Clear();
        var found = table.Select(script, language, 0, selected, required: true, append: true);
        ReadOnlySpan<uint> features = [OpenTypeTable.Tag('d', 'i', 's', 't'), kerning ? OpenTypeTable.Tag('k', 'e', 'r', 'n') : 0,
            OpenTypeTable.Tag('m', 'a', 'r', 'k'), OpenTypeTable.Tag('m', 'k', 'm', 'k'), OpenTypeTable.Tag('c', 'u', 'r', 's')];
        foreach (var feature in features)
            if (feature != 0) found |= table.Select(script, language, feature, selected, append: true);
        if (found) ApplySelected(glyphs, 0, start, length);
        return found;
    }
    private void ApplySelected(ShapingBuffer glyphs, uint mask, int start, int length)
    {
        runStart = start; runEnd = length < 0 ? glyphs.Count : start + length;
        for (var lookup = 0; lookup < selected.Length; lookup++)
        {
            if (selected[lookup] == 0) continue;
            for (var at = start; at < runEnd;)
            {
                context.Work(1);
                if (mask != 0 && (glyphs[at].Features & mask) == 0) { at++; continue; }
                ApplyAt(lookup, glyphs, at, out var next); at = Math.Max(at + 1, next);
            }
        }
    }

    private bool ApplyAt(int lookup, ShapingBuffer glyphs, int at, out int next)
    {
        next = at + 1;
        if (Boundary(glyphs[at])) return false;
        var p = table.Offset16(lookupList, lookupList + 2 + lookup * 2);
        var type = table.U16(p); var flags = table.U16(p + 2); var count = table.Count(p + 4, 2);
        if ((flags & 0xE0) != 0) throw OpenTypeTable.Unsupported("Reserved OpenType lookup flags are unsupported.");
        var filteringSet = (flags & 16) != 0 ? table.U16(p + 6 + count * 2) : 0;
        if (classes.Ignored(glyphs[at], flags, filteringSet)) return false;
        for (var i = 0; i < count; i++)
        {
            var sub = table.Offset16(p, p + 6 + i * 2);
            if (ApplySubtable(type, sub, glyphs, at, flags, filteringSet, out next)) return true;
        }
        next = at + 1; return false;
    }
    private bool ApplySubtable(int type, int sub, ShapingBuffer glyphs, int at, int flags, int filteringSet, out int next)
    {
        next = at + 1;
        switch (type)
        {
            case 1: return Single(sub, glyphs, at);
            case 2: return Pair(sub, glyphs, at, flags, filteringSet, out next);
            case 4 or 5 or 6: return Mark(type, sub, glyphs, at, flags, filteringSet);
            case 9:
                if (table.U16(sub) != 1) throw OpenTypeTable.Unsupported("Unsupported OpenType positioning extension format.");
                var target = table.U16(sub + 2);
                if (target == 9) throw OpenTypeTable.Bad("Recursive OpenType positioning extension.");
                return ApplySubtable(target, table.Offset32(sub, sub + 4), glyphs, at, flags, filteringSet, out next);
            default: throw OpenTypeTable.Unsupported("Unsupported OpenType positioning lookup type.");
        }
    }

    private static int ValueBytes(int format)
    {
        if ((format & ~255) != 0) throw OpenTypeTable.Unsupported("Reserved OpenType positioning value fields are unsupported.");
        return BitOperations.PopCount((uint)format) * 2;
    }
    private void Value(int offset, int format, ShapingBuffer glyphs, int at)
    {
        table.Range(offset, ValueBytes(format));
        double x = 0, y = 0, advance = 0;
        for (var bit = 1; bit <= 128; bit <<= 1)
        {
            if ((format & bit) == 0) continue;
            var value = table.I16(offset); offset += 2;
            switch (bit)
            {
                case 1: x = value; break;
                case 2: y = value; break;
                case 4: advance = value; break;
                case 8: break; // yAdvance belongs to vertical layout, not horizontal text.
                default:
                    if (value != 0) throw OpenTypeTable.Unsupported("OpenType device and variation positioning adjustments are unsupported.");
                    break;
            }
        }
        glyphs[at].OffsetX += x; glyphs[at].OffsetY += y; glyphs[at].Advance += advance;
    }
    private bool Single(int sub, ShapingBuffer glyphs, int at)
    {
        var format = table.U16(sub);
        if (format is not (1 or 2)) throw OpenTypeTable.Unsupported("Unsupported OpenType single positioning format.");
        var index = table.Coverage(table.Offset16(sub, sub + 2), glyphs[at].Glyph);
        if (index < 0) return false;
        var valueFormat = table.U16(sub + 4); var size = ValueBytes(valueFormat);
        var value = sub + 6;
        if (format == 2)
        {
            var count = table.Count(sub + 6, size);
            if (index >= count) throw OpenTypeTable.Bad("OpenType single positioning coverage exceeds its value array.");
            value = sub + 8 + index * size;
        }
        Value(value, valueFormat, glyphs, at); return true;
    }

    private bool Pair(int sub, ShapingBuffer glyphs, int at, int flags, int filteringSet, out int next)
    {
        next = at + 1; var format = table.U16(sub);
        if (format is not (1 or 2)) throw OpenTypeTable.Unsupported("Unsupported OpenType pair positioning format.");
        var covered = table.Coverage(table.Offset16(sub, sub + 2), glyphs[at].Glyph);
        if (covered < 0) return false;
        var second = Next(glyphs, at + 1, 1, flags, filteringSet);
        if (second < 0) return false;
        var firstFormat = table.U16(sub + 4); var secondFormat = table.U16(sub + 6);
        var firstSize = ValueBytes(firstFormat); var secondSize = ValueBytes(secondFormat); var size = firstSize + secondSize;
        int value;
        if (format == 1)
        {
            var sets = table.Count(sub + 8, 2);
            if (covered >= sets) throw OpenTypeTable.Bad("OpenType pair coverage exceeds its set array.");
            var set = table.Offset16(sub, sub + 10 + covered * 2); var count = table.Count(set, size + 2);
            var low = 0; var high = count - 1; value = -1;
            while (low <= high)
            {
                var mid = (low + high) / 2; var p = set + 2 + mid * (size + 2); var glyph = table.Glyph(table.U16(p));
                if (glyphs[second].Glyph < glyph) high = mid - 1;
                else if (glyphs[second].Glyph > glyph) low = mid + 1;
                else { value = p + 2; break; }
            }
            if (value < 0) return false;
        }
        else
        {
            var firstClass = table.Class(table.Offset16(sub, sub + 8), glyphs[at].Glyph);
            var secondClass = table.Class(table.Offset16(sub, sub + 10), glyphs[second].Glyph);
            var firstCount = table.U16(sub + 12); var secondCount = table.U16(sub + 14);
            table.Items(firstCount); table.Items(secondCount);
            var cells = (long)firstCount * secondCount;
            if (cells > context.Limits.MaxContainerItems)
                throw new ImageLimitException(ImageLimitKind.ContainerItems, "OpenType positioning class-pair limit exceeded.");
            table.Range(sub + 16, cells * size);
            if (firstClass >= firstCount || secondClass >= secondCount) throw OpenTypeTable.Bad("OpenType positioning class exceeds its pair matrix.");
            value = checked(sub + 16 + (firstClass * secondCount + secondClass) * size);
        }
        Value(value, firstFormat, glyphs, at); Value(value + firstSize, secondFormat, glyphs, second);
        next = secondFormat == 0 ? second : second + 1; return true;
    }

    private (int X, int Y) Anchor(int offset)
    {
        var format = table.U16(offset);
        if (format is < 1 or > 3) throw OpenTypeTable.Unsupported("Unsupported OpenType anchor format.");
        table.Range(offset, format == 3 ? 10 : format == 2 ? 8 : 6);
        var x = table.I16(offset + 2); var y = table.I16(offset + 4);
        // Anchor contour points refine hinted outlines. The unhinted path uses
        // design coordinates; nonzero device/variation corrections need support.
        if (format == 3 && (table.U16(offset + 6) != 0 || table.U16(offset + 8) != 0))
            throw OpenTypeTable.Unsupported("OpenType device and variation anchors are unsupported.");
        return (x, y);
    }
    private bool Mark(int type, int sub, ShapingBuffer glyphs, int at, int flags, int filteringSet)
    {
        if (table.U16(sub) != 1) throw OpenTypeTable.Unsupported("Unsupported OpenType mark attachment format.");
        var covered = table.Coverage(table.Offset16(sub, sub + 2), glyphs[at].Glyph);
        if (covered < 0) return false;
        var parent = type == 6 ? Next(glyphs, at - 1, -1, flags, filteringSet) : PreviousBase(glyphs, at);
        if (parent < 0) return false;
        if (type == 6 && (glyphs[parent].Ligature != glyphs[at].Ligature
            || glyphs[at].Ligature != 0 && glyphs[parent].Component != glyphs[at].Component)) return false;
        var parentIndex = table.Coverage(table.Offset16(sub, sub + 4), glyphs[parent].Glyph);
        if (parentIndex < 0) return false;
        var classCount = table.U16(sub + 6); table.Items(classCount);
        if (classCount == 0) throw OpenTypeTable.Bad("OpenType mark attachment has no classes.");
        var marks = table.Offset16(sub, sub + 8); var markCount = table.Count(marks, 4);
        if (covered >= markCount) throw OpenTypeTable.Bad("OpenType mark coverage exceeds its record array.");
        var record = marks + 2 + covered * 4; var markClass = table.U16(record);
        if (markClass >= classCount) throw OpenTypeTable.Bad("Invalid OpenType mark attachment class.");
        var markAnchor = table.Offset16(marks, record + 2);
        var parents = table.Offset16(sub, sub + 10);
        var parentCount = table.U16(parents); table.Items(parentCount);
        if (parentIndex >= parentCount) throw OpenTypeTable.Bad("OpenType attachment coverage exceeds its parent array.");
        int parentAnchor;
        if (type == 5)
        {
            table.Range(parents + 2, parentCount * 2L);
            var attach = table.Offset16(parents, parents + 2 + parentIndex * 2); var components = table.U16(attach);
            table.Items(components);
            if (components == 0) throw OpenTypeTable.Bad("An OpenType attachment ligature has no components.");
            var cells = (long)components * classCount; CheckCells(cells); table.Range(attach + 2, cells * 2);
            var component = glyphs[at].Ligature != 0 && glyphs[at].Ligature == glyphs[parent].Ligature
                ? Math.Clamp(glyphs[at].Component, 0, components - 1) : components - 1;
            parentAnchor = table.Offset16(attach, attach + 2 + (component * classCount + markClass) * 2, optional: true);
        }
        else
        {
            var cells = (long)parentCount * classCount; CheckCells(cells); table.Range(parents + 2, cells * 2);
            parentAnchor = table.Offset16(parents, parents + 2 + (parentIndex * classCount + markClass) * 2, optional: true);
        }
        if (parentAnchor == 0) return false;
        var mark = Anchor(markAnchor); var target = Anchor(parentAnchor);
        glyphs[at].AttachTo = parent + 1;
        glyphs[at].AttachX = target.X - glyphs[parent].OriginX - (mark.X - glyphs[at].OriginX);
        glyphs[at].AttachY = target.Y - mark.Y;
        // Anchors override earlier placement of this mark; later value records
        // add to it. Parent adjustments remain live until final resolution.
        glyphs[at].OffsetX = 0; glyphs[at].OffsetY = 0; return true;
    }
    private void CheckCells(long count)
    {
        if (count > context.Limits.MaxContainerItems)
            throw new ImageLimitException(ImageLimitKind.ContainerItems, "OpenType attachment matrix limit exceeded.");
    }
    private int PreviousBase(ShapingBuffer glyphs, int at)
    {
        // Parent discovery for mark-to-base/ligature skips marks, not a nearer
        // unrelated base/ligature merely because it lacks a covered anchor.
        for (var i = at - 1; i >= runStart; i--)
        {
            context.Work(1); if (Boundary(glyphs[i])) return -1;
            if (classes.Class(glyphs[i]) != 3) return i;
        }
        return -1;
    }
    private static bool Boundary(ShapingGlyph glyph) => glyph.Glyph < 0 || glyph.Kind is 2 or 3;
    private int Next(ShapingBuffer glyphs, int at, int step, int flags, int filteringSet)
    {
        while (at >= runStart && at < runEnd)
        {
            context.Work(1); if (Boundary(glyphs[at])) return -1;
            if (!classes.Ignored(glyphs[at], flags, filteringSet)) return at;
            at += step;
        }
        return -1;
    }

    internal void ResolveAttachments(ShapingBuffer glyphs, bool rightToLeft = false, int start = 0, int length = -1)
    {
        Check(glyphs, start, length);
        var count = length < 0 ? glyphs.Count - start : length; var end = start + count;
        var any = false;
        for (var i = start; i < end; i++) { context.Work(1); any |= glyphs[i].AttachTo != 0; }
        if (!any) return;
        var bytes = count * 8L + 32; context.Reserve(bytes);
        try
        {
            var origins = new double[count]; double pen = 0;
            if (rightToLeft)
                for (var i = start; i < end; i++) { context.Work(1); pen += glyphs[i].Advance; }
            for (var i = start; i < end; i++)
            {
                context.Work(1);
                if (rightToLeft) { pen -= glyphs[i].Advance; origins[i - start] = pen; }
                else { origins[i - start] = pen; pen += glyphs[i].Advance; }
            }
            for (var i = start; i < end; i++)
            {
                context.Work(1); var parent = glyphs[i].AttachTo - 1;
                if (parent == -1) continue;
                if (parent < start || parent >= i) throw OpenTypeTable.Bad("Invalid or cyclic OpenType mark attachment.");
                glyphs[i].OffsetX += origins[parent - start] - origins[i - start] + glyphs[parent].OffsetX + glyphs[i].AttachX;
                glyphs[i].OffsetY += glyphs[parent].OffsetY + glyphs[i].AttachY;
                glyphs[i].AttachTo = 0; glyphs[i].AttachX = glyphs[i].AttachY = 0;
            }
        }
        finally { context.Release(bytes); }
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true; if (!table.Empty) context.Release(selected.Length + 32L); selected = [];
    }
}
