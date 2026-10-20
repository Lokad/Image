namespace Lokad.Image.Internal;

internal sealed partial class OpenTypeGlyphClasses
{
    // OpenType otvarcommonformats ItemVariationStore. At normalized default
    // coordinates every valid region has scalar zero or one: an axis with a
    // nonzero peak excludes zero; a zero peak does not participate. No axis
    // selection, interpolation cache or additional font storage is introduced.
    internal void CheckDefaultVariation(int outer, int inner)
    {
        if (outer == 0xFFFF && inner == 0xFFFF) return; // Specified no-variation sentinel.
        if (table.Empty || table.U16(2) != 3)
            throw OpenTypeTable.Bad("Missing OpenType item variation store.");
        var store = table.Offset32(0, 14);
        if (table.U16(store) != 1) throw OpenTypeTable.Unsupported("Unsupported OpenType item variation store format.");
        var dataCount = table.Count(store + 6, 4);
        if ((uint)outer >= dataCount) throw OpenTypeTable.Bad("Invalid OpenType variation outer index.");
        var dataField = store + 8 + outer * 4;
        if (table.U32(dataField) == 0) return; // A null outer entry has no variation, regardless of inner index.
        var data = table.Offset32(store, dataField);
        var items = table.U16(data); table.Items(items);
        if ((uint)inner >= items) throw OpenTypeTable.Bad("Invalid OpenType variation inner index.");
        var words = table.U16(data + 2);
        if ((words & 0x8000) != 0) throw OpenTypeTable.Unsupported("Long-word OpenType positioning deltas are unsupported.");
        var columns = table.Count(data + 4, 2);
        if (words > columns) throw OpenTypeTable.Bad("Invalid OpenType variation word count.");
        var rows = data + 6 + columns * 2; var rowBytes = columns + words;
        table.Range(rows, (long)items * rowBytes);
        var row = checked(rows + inner * rowBytes);

        var regions = table.Offset32(store, store + 2);
        var axes = table.U16(regions); var regionCount = table.U16(regions + 2);
        table.Items(axes); table.Items(regionCount);
        if (axes == 0 || regionCount >= 0x8000) throw OpenTypeTable.Bad("Invalid OpenType variation region dimensions.");
        var cells = (long)axes * regionCount;
        if (cells > table.Context.Limits.MaxContainerItems)
            throw new ImageLimitException(ImageLimitKind.ContainerItems, "OpenType variation region limit exceeded.");
        table.Range(regions + 4, cells * 6);
        var delta = 0;
        for (var i = 0; i < columns; i++)
        {
            var index = table.U16(data + 6 + i * 2);
            if (index >= regionCount) throw OpenTypeTable.Bad("Invalid OpenType variation region index.");
            var active = true; var p = regions + 4 + index * axes * 6;
            for (var axis = 0; axis < axes; axis++, p += 6)
            {
                var start = table.I16(p); var peak = table.I16(p + 2); var end = table.I16(p + 4);
                if (start < -16384 || end > 16384 || start > peak || peak > end
                    || peak != 0 && start < 0 && end > 0)
                    throw OpenTypeTable.Bad("Invalid OpenType variation region coordinates.");
                active &= peak == 0;
            }
            if (active) delta += i < words ? table.I16(row + i * 2) : table.I8(row + words + i);
        }
        if (delta != 0) throw OpenTypeTable.Unsupported("Nonzero default-instance OpenType positioning deltas are unsupported.");
    }
}
