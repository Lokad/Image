using System.Globalization;
using System.Text;

namespace Lokad.Image.Internal;

internal sealed class OpenTypeGlyphClasses
{
    private readonly OpenTypeTable table;
    private readonly int glyphClasses, markClasses, markSets;

    internal OpenTypeGlyphClasses(ReadOnlyMemory<byte> definitions, int glyphCount, OperationContext context)
    {
        table = new(definitions, glyphCount, context);
        if (table.Empty) return;
        var minor = table.U16(2);
        if (table.U16(0) != 1 || minor is not (0 or 2 or 3))
            throw OpenTypeTable.Unsupported("Unsupported OpenType glyph-definition version.");
        table.Range(0, minor == 3 ? 18 : minor == 2 ? 14 : 12);
        glyphClasses = table.Offset16(0, 4, optional: true);
        markClasses = table.Offset16(0, 10, optional: true);
        markSets = minor >= 2 ? table.Offset16(0, 12, optional: true) : 0;
    }

    internal int Class(ShapingGlyph glyph)
    {
        var value = table.Class(glyphClasses, glyph.Glyph);
        if (value > 4) throw OpenTypeTable.Bad("Invalid OpenType glyph class.");
        if (value != 0) return value;
        if (glyph.ComponentCount > 1) return 2;
        if (!Rune.IsValid(glyph.Scalar)) return 0;
        var category = Rune.GetUnicodeCategory(new Rune(glyph.Scalar));
        if (category is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.EnclosingMark) return 3;
        return category is UnicodeCategory.Control or UnicodeCategory.Format ? 0 : 1;
    }

    internal bool Ignored(ShapingGlyph glyph, int flags, int filteringSet)
    {
        var kind = Class(glyph);
        if (kind == 1 && (flags & 2) != 0 || kind == 2 && (flags & 4) != 0 || kind == 3 && (flags & 8) != 0) return true;
        if (kind != 3) return false;
        if ((flags & 0x10) != 0)
        {
            if (markSets == 0) throw OpenTypeTable.Bad("Missing OpenType mark filtering sets.");
            if (table.U16(markSets) != 1) throw OpenTypeTable.Unsupported("Unsupported OpenType mark filtering sets.");
            var count = table.Count(markSets + 2, 4);
            if ((uint)filteringSet >= count) throw OpenTypeTable.Bad("Invalid OpenType mark filtering set index.");
            return table.Coverage(table.Offset32(markSets, markSets + 4 + filteringSet * 4), glyph.Glyph) < 0;
        }
        return (flags >> 8) != 0 && table.Class(markClasses, glyph.Glyph) != flags >> 8;
    }
}
