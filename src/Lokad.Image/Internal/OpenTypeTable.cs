using System.Buffers.Binary;

namespace Lokad.Image.Internal;

// Format facts: Microsoft OpenType chapter2 (Script/LangSys/Feature/Lookup,
// Coverage and ClassDef) and GDEF. This reader borrows immutable font storage.
internal sealed class OpenTypeTable(ReadOnlyMemory<byte> data, int glyphCount, OperationContext context)
{
    internal bool Empty => data.IsEmpty;
    internal int Length => data.Length;
    internal int GlyphCount => glyphCount;
    internal OperationContext Context => context;

    internal void Range(int offset, long bytes)
    {
        if (offset < 0 || bytes < 0 || bytes > data.Length - (long)offset)
            throw Bad("Truncated OpenType layout table.");
    }
    internal int U16(int offset)
    {
        context.Work(1); Range(offset, 2);
        return BinaryPrimitives.ReadUInt16BigEndian(data.Span[offset..]);
    }
    internal int I16(int offset) => (short)U16(offset);
    internal uint U32(int offset)
    {
        context.Work(1); Range(offset, 4);
        return BinaryPrimitives.ReadUInt32BigEndian(data.Span[offset..]);
    }
    internal int Offset16(int origin, int field, bool optional = false)
    {
        var delta = U16(field);
        if (delta == 0)
        {
            if (optional) return 0;
            throw Bad("Missing required OpenType layout offset.");
        }
        var target = (long)origin + delta;
        if (target > int.MaxValue) throw Bad("Invalid OpenType layout offset.");
        Range((int)target, 2); return (int)target;
    }
    internal int Offset32(int origin, int field)
    {
        var delta = U32(field); var target = (long)origin + delta;
        if (delta == 0 || target > int.MaxValue) throw Bad("Invalid OpenType layout offset.");
        Range((int)target, 2); return (int)target;
    }
    internal int Count(int field, int itemBytes)
    {
        var count = U16(field); Items(count); Range(field + 2, (long)count * itemBytes); return count;
    }
    internal void Items(int count)
    {
        if (count > context.Limits.MaxContainerItems)
            throw new ImageLimitException(ImageLimitKind.ContainerItems, "OpenType layout item limit exceeded.");
    }
    internal int Glyph(int value)
    {
        if ((uint)value >= glyphCount) throw Bad("OpenType layout references an absent glyph.");
        return value;
    }
    internal static uint Tag(char a, char b, char c, char d) => (uint)a << 24 | (uint)b << 16 | (uint)c << 8 | d;
    internal static ImageDataException Bad(string message) => new(message);
    internal static UnsupportedImageException Unsupported(string message) => new(UnsupportedImageFeature.FontTable, message);

    internal int Coverage(int offset, int glyph)
    {
        var format = U16(offset); var count = Count(offset + 2, format == 1 ? 2 : 6);
        var lo = 0; var hi = count - 1;
        if (format is not (1 or 2)) throw Unsupported("Unsupported OpenType coverage format.");
        while (lo <= hi)
        {
            var mid = (lo + hi) / 2;
            if (format == 1)
            {
                var actual = Glyph(U16(offset + 4 + mid * 2));
                if (glyph < actual) hi = mid - 1;
                else if (glyph > actual) lo = mid + 1;
                else return mid;
            }
            else
            {
                var p = offset + 4 + mid * 6; var start = Glyph(U16(p)); var end = Glyph(U16(p + 2));
                if (end < start) throw Bad("Invalid OpenType coverage range.");
                if (glyph < start) hi = mid - 1;
                else if (glyph > end) lo = mid + 1;
                else return U16(p + 4) + glyph - start;
            }
        }
        return -1;
    }

    internal int Class(int offset, int glyph)
    {
        if (offset == 0) return 0;
        var format = U16(offset);
        if (format == 1)
        {
            var start = U16(offset + 2); var count = Count(offset + 4, 2);
            if (start + count > glyphCount) throw Bad("OpenType class range exceeds the glyph set.");
            return glyph >= start && glyph - start < count ? U16(offset + 6 + (glyph - start) * 2) : 0;
        }
        if (format != 2) throw Unsupported("Unsupported OpenType class definition.");
        var ranges = Count(offset + 2, 6); var lo = 0; var hi = ranges - 1;
        while (lo <= hi)
        {
            var mid = (lo + hi) / 2; var p = offset + 4 + mid * 6;
            var first = Glyph(U16(p)); var last = Glyph(U16(p + 2));
            if (last < first) throw Bad("Invalid OpenType class range.");
            if (glyph < first) hi = mid - 1;
            else if (glyph > last) lo = mid + 1;
            else return U16(p + 4);
        }
        return 0;
    }

    internal int LookupList()
    {
        if (U16(0) != 1 || U16(2) > 1) throw Unsupported("Unsupported OpenType layout version.");
        if (U16(2) == 1 && U32(10) != 0)
            throw Unsupported("OpenType layout feature variations are unsupported.");
        var list = Offset16(0, 8); Count(list, 2); return list;
    }

    // Select only the requested script/language's features, never all features
    // sharing a tag in the global list. Lookup-list order controls execution.
    internal bool Select(uint script, uint language, uint feature, Span<byte> selected, bool required = false, bool append = false)
    {
        if (!append) selected.Clear();
        var scripts = Offset16(0, 4); var features = Offset16(0, 6);
        var scriptTable = Tagged(scripts, script);
        if (scriptTable == 0) scriptTable = Tagged(scripts, Tag('D', 'F', 'L', 'T'));
        if (scriptTable == 0) return false;
        var lang = language == 0 ? 0 : Tagged(scriptTable + 2, language, scriptTable);
        if (lang == 0) lang = Offset16(scriptTable, scriptTable, optional: true);
        if (lang == 0) return false;
        if (U16(lang) != 0) throw Bad("Invalid OpenType language-system lookup order.");
        var featureCount = Count(features, 6); var langCount = Count(lang + 4, 2);
        var found = false;
        if (required)
        {
            var index = U16(lang + 2);
            if (index != 0xFFFF) { SelectFeature(features, featureCount, index, selected); found = true; }
        }
        else
            for (var i = 0; i < langCount; i++)
            {
                var index = U16(lang + 6 + i * 2);
                if (index >= featureCount) throw Bad("Invalid OpenType feature index.");
                if (U32(features + 2 + index * 6) == feature) { SelectFeature(features, featureCount, index, selected); found = true; }
            }
        return found;
    }

    private void SelectFeature(int features, int featureCount, int index, Span<byte> selected)
    {
        if (index >= featureCount) throw Bad("Invalid required OpenType feature index.");
        var f = Offset16(features, features + 6 + index * 6);
        var count = Count(f + 2, 2);
        for (var j = 0; j < count; j++)
        {
            var lookup = U16(f + 4 + j * 2);
            if (lookup >= selected.Length) throw Bad("Invalid OpenType lookup index.");
            selected[lookup] = 1;
        }
    }

    private int Tagged(int list, uint tag, int origin = -1)
    {
        var count = Count(list, 6);
        for (var i = 0; i < count; i++)
        {
            var record = list + 2 + i * 6;
            if (U32(record) == tag) return Offset16(origin < 0 ? list : origin, record + 4);
        }
        return 0;
    }
}
