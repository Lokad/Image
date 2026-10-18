using System.Buffers.Binary;
using System.Text;
using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Style supplied by the selected font face; no synthetic styling or system-font lookup.</summary>
[Flags]
public enum FontStyle { Regular = 0, Bold = 1, Italic = 2 }

/// <summary>Unscaled horizontal metrics and control-point bounds in font units. Empty glyphs have zero bounds.</summary>
public readonly record struct FontGlyphMetrics(int GlyphIndex, int AdvanceWidth, int LeftSideBearing,
    int XMin, int YMin, int XMax, int YMax, bool HasOutline);

/// <summary>An immutable, independently owned standalone TrueType face.</summary>
/// <remarks>Unicode cmap formats 4/12 and short/long loca are supported. Font loading
/// validates table ranges, selected mapping and glyph headers, not all outlines or
/// checksums. No font instructions are executed. Caller input is copied once.</remarks>
public sealed partial class TrueTypeFont
{
    private const int DescriptorBytes = 256;
    private readonly byte[] data;
    private readonly int tableCount, metricCount, locationWidth, cmapFormat, mappingCount;
    private readonly TableRange glyf, loca, hmtx, cmap;
    private readonly record struct TableRange(int Offset, int Length);
    public int UnitsPerEm { get; }
    public int GlyphCount { get; }
    public int Ascender { get; }
    public int Descender { get; }
    public int LineGap { get; }
    public FontStyle Style { get; }
    /// <summary>Owned font bytes plus a coarse descriptor allowance, for caller lifetime reservations.</summary>
    public long StorageBytes => data.LongLength + DescriptorBytes;

    /// <summary>Copy and validate a supplied standalone TrueType face within finite admission limits.</summary>
    public static TrueTypeFont Load(ReadOnlySpan<byte> input, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        var context = InputContext(input.Length, limits, cancellationToken);
        return new TrueTypeFont(input.ToArray(), context);
    }

    internal static OperationContext InputContext(int length, ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        if (length > context.Limits.MaxInputBytes) throw new ImageLimitException(ImageLimitKind.InputBytes, "Font input limit exceeded.");
        if (length > Array.MaxLength) throw new ImageLimitException(ImageLimitKind.BufferLength, "Owned font exceeds the managed array limit.");
        context.Reserve(length);
        context.Reserve(length + (long)DescriptorBytes);
        context.Work(length);
        return context;
    }

    /// <summary>Map a Unicode scalar to a glyph index, returning zero for an absent character.</summary>
    public int GetGlyphIndex(Rune character, ImageLimits? limits = null, CancellationToken cancellationToken = default)
        => Map(character.Value, Context(limits, cancellationToken));

    /// <summary>Read a Unicode scalar's unscaled metrics, using glyph zero when absent.</summary>
    public FontGlyphMetrics GetGlyphMetrics(Rune character, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        var context = Context(limits, cancellationToken);
        return Metrics(Map(character.Value, context), context);
    }

    /// <summary>Read unscaled metrics by glyph index.</summary>
    public FontGlyphMetrics GetGlyphMetrics(int glyphIndex, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        if ((uint)glyphIndex >= GlyphCount) throw new ArgumentOutOfRangeException(nameof(glyphIndex));
        return Metrics(glyphIndex, Context(limits, cancellationToken));
    }

    private OperationContext Context(ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.Reserve(StorageBytes); return context;
    }

    // Format facts: Microsoft OpenType specifications otff/head/hhea/maxp/hmtx/loca/glyf.
    // All offsets are range-checked before conversion to managed indexes.
    private TrueTypeFont(byte[] data, OperationContext context)
    {
        this.data = data;
        if (data.Length < 12) throw new ImageDataException("Truncated font directory.");
        var version = U32(data);
        if (version != 0x00010000 && version != Tag("true"u8))
            throw new UnsupportedImageException(UnsupportedImageFeature.FontContainer, "Expected a standalone TrueType outline font.");
        tableCount = U16(data.AsSpan(4));
        if (tableCount == 0 || 12L + tableCount * 16L > data.Length) throw new ImageDataException("Invalid font table directory.");
        Items(tableCount, context);
        uint previous = 0;
        for (var i = 0; i < tableCount; i++)
        {
            context.Work(1); var entry = data.AsSpan(12 + i * 16, 16);
            var tag = U32(entry); var offset = U32(entry[8..]); var length = U32(entry[12..]);
            if (i != 0 && tag <= previous || offset > data.Length || length > data.Length - (long)offset
                || length != 0 && (offset < 12 + tableCount * 16 || (offset & 3) != 0))
                throw new ImageDataException("Invalid font table range or ordering.");
            previous = tag;
        }
        var head = Table("head"u8, context); var hhea = Table("hhea"u8, context); var maxp = Table("maxp"u8, context);
        if (head.Length < 54 || hhea.Length < 36 || maxp.Length < 32) throw new ImageDataException("Truncated font metrics header.");
        if (U16(head) != 1 || U16(hhea) != 1 || U16(maxp) != 1 || I16(head[52..]) != 0 || I16(hhea[32..]) != 0)
            throw new UnsupportedImageException(UnsupportedImageFeature.FontTable, "Unsupported TrueType metrics or glyph table version.");
        if (U32(head[12..]) != 0x5F0F3CF5) throw new ImageDataException("Invalid TrueType header identifier.");
        UnitsPerEm = U16(head[18..]); GlyphCount = U16(maxp[4..]); metricCount = U16(hhea[34..]);
        Ascender = I16(hhea[4..]); Descender = I16(hhea[6..]); LineGap = I16(hhea[8..]);
        Style = (FontStyle)(U16(head[44..]) & 3);
        if (UnitsPerEm is < 16 or > 16384 || GlyphCount == 0 || metricCount == 0 || metricCount > GlyphCount || Ascender <= Descender)
            throw new ImageDataException("Invalid TrueType units or horizontal metrics.");
        Items(GlyphCount, context);
        var locationFormat = I16(head[50..]);
        if (locationFormat is not (0 or 1)) throw new UnsupportedImageException(UnsupportedImageFeature.FontTable, "Unsupported glyph location format.");
        locationWidth = locationFormat == 0 ? 2 : 4;
        glyf = Required("glyf"u8, context); loca = Required("loca"u8, context); hmtx = Required("hmtx"u8, context);
        if ((GlyphCount + 1L) * locationWidth > loca.Length || metricCount * 4L + (GlyphCount - metricCount) * 2L > hmtx.Length)
            throw new ImageDataException("Truncated glyph locations or horizontal metrics.");
        var first = Location(0);
        if (first > glyf.Length) throw new ImageDataException("Invalid glyph location.");
        for (var i = 0; i < GlyphCount; i++)
        {
            context.Work(1); var last = Location(i + 1);
            if (last < first || last > glyf.Length) throw new ImageDataException("Invalid glyph location ordering.");
            if (last != first) Header(data.AsSpan(glyf.Offset + (int)first, (int)(last - first)));
            first = last;
        }
        cmap = SelectMapping(context); cmapFormat = U16(Span(cmap));
        ValidateMapping(context);
        mappingCount = cmapFormat == 4 ? U16(Span(cmap)[6..]) / 2 : (int)U32(Span(cmap)[12..]);
    }

    private FontGlyphMetrics Metrics(int glyphIndex, OperationContext context)
    {
        context.Work(1); var metrics = Span(hmtx);
        var advance = U16(metrics[(Math.Min(glyphIndex, metricCount - 1) * 4)..]);
        var bearing = I16(metrics[(glyphIndex < metricCount ? glyphIndex * 4 + 2 : metricCount * 4 + (glyphIndex - metricCount) * 2)..]);
        var glyph = Glyph(glyphIndex);
        return glyph.Length == 0 || I16(glyph) == 0
            ? new(glyphIndex, advance, bearing, 0, 0, 0, 0, false)
            : new(glyphIndex, advance, bearing, I16(glyph[2..]), I16(glyph[4..]), I16(glyph[6..]), I16(glyph[8..]), true);
    }

    private ReadOnlySpan<byte> Glyph(int glyphIndex)
    {
        var first = Location(glyphIndex); var last = Location(glyphIndex + 1);
        return data.AsSpan(glyf.Offset + (int)first, (int)(last - first));
    }
    private uint Location(int index) => locationWidth == 2 ? (uint)U16(Span(loca)[(index * 2)..]) * 2 : U32(Span(loca)[(index * 4)..]);
    private static void Header(ReadOnlySpan<byte> glyph)
    {
        if (glyph.Length < 10) throw new ImageDataException("Invalid glyph header.");
        if (I16(glyph) != 0 && (I16(glyph[2..]) > I16(glyph[6..]) || I16(glyph[4..]) > I16(glyph[8..])))
            throw new ImageDataException("Invalid glyph bounds.");
    }

    private ReadOnlySpan<byte> Table(ReadOnlySpan<byte> tag, OperationContext context) => Span(Required(tag, context));
    private TableRange Required(ReadOnlySpan<byte> tag, OperationContext context)
        => Find(tag, context) ?? throw new ImageDataException("Required TrueType table is missing.");
    private TableRange? Find(ReadOnlySpan<byte> tag, OperationContext context)
    {
        var target = Tag(tag); var low = 0; var high = tableCount - 1;
        while (low <= high)
        {
            context.Work(1); var mid = (low + high) / 2; var entry = data.AsSpan(12 + mid * 16, 16); var actual = U32(entry);
            if (target < actual) high = mid - 1;
            else if (target > actual) low = mid + 1;
            else return new((int)U32(entry[8..]), (int)U32(entry[12..]));
        }
        return null;
    }
    private ReadOnlySpan<byte> Span(TableRange range) => data.AsSpan(range.Offset, range.Length);
    private static void Items(long count, OperationContext context)
    {
        if (count > context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "Font item limit exceeded.");
    }
    private static uint Tag(ReadOnlySpan<byte> tag) => U32(tag);
    private static ushort U16(ReadOnlySpan<byte> bytes) => BinaryPrimitives.ReadUInt16BigEndian(bytes);
    private static short I16(ReadOnlySpan<byte> bytes) => BinaryPrimitives.ReadInt16BigEndian(bytes);
    private static uint U32(ReadOnlySpan<byte> bytes) => BinaryPrimitives.ReadUInt32BigEndian(bytes);
}
