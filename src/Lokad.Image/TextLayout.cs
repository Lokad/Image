using System.Buffers;
using System.Text;
using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Pixel layout of logical left-to-right Unicode glyphs from a supplied face.</summary>
public sealed record TextOptions
{
    public double FontSize { get; init; } = 16;
    public double WrapWidth { get; init; } = double.PositiveInfinity;
    public double LineSpacing { get; init; } = 1;
    public int TabSpaces { get; init; } = 4;
    /// <summary>Apply supported ordinary horizontal legacy kern tables; no OpenType shaping.</summary>
    public bool Kerning { get; init; } = true;
    public ImageLimits Limits { get; init; } = ImageLimits.Default;
}

/// <summary>Continuous pixel bounds, relative to the nominal text origin.</summary>
public readonly record struct TextBounds(double X, double Y, double Width, double Height)
{
    public double Right => X + Width;
    public double Bottom => Y + Height;
    public TextBounds Translate(double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(x));
        var result = this with { X = X + x, Y = Y + y };
        if (!double.IsFinite(result.X) || !double.IsFinite(result.Y)) throw new ArgumentOutOfRangeException(nameof(x));
        return result;
    }
    public bool FitsInside(double width, double height) => X >= 0 && Y >= 0 && Width >= 0 && Height >= 0
        && double.IsFinite(Right) && double.IsFinite(Bottom) && Right <= width && Bottom <= height;
}

/// <summary>A positioned glyph; source offsets/lengths use the original UTF-16 string.</summary>
public readonly record struct TextGlyph(int GlyphIndex, int SourceOffset, int SourceLength,
    int LineIndex, double X, double BaselineY, double AdvanceWidth);
public readonly record struct TextLine(int FirstGlyph, int GlyphCount, double BaselineY, double AdvanceWidth);

/// <summary>Immutable positioned glyphs and decoded outlines; independent of caller text storage.</summary>
/// <remarks>Unhinted default-instance outlines. No instruction execution, device-grid
/// fitting, bidirectional reordering, ligatures or GPOS shaping. Bounds use decoded
/// curves, not untrusted declared glyph bounds. Source strings are not retained.</remarks>
public sealed class TextLayout
{
    internal sealed record Shape(FontOutline Outline, double XMin, double YMin, double XMax, double YMax);
    internal struct Placement
    {
        internal Shape? Shape;
        internal int GlyphIndex, SourceOffset, SourceLength, LineIndex;
        internal double X, Y, Advance;
        internal byte Kind; // 0 word, 1 breakable separator, 2 tab, 3 newline.
    }
    private readonly Placement[] glyphs;
    private readonly TextLine[] lines;
    private readonly Dictionary<int, Shape> outlines;
    public TrueTypeFont Font { get; }
    public double FontSize { get; }
    public double LineAdvance { get; }
    public int SourceLength { get; }
    public int GlyphCount { get; }
    public int LineCount { get; }
    public TextBounds LayoutBounds { get; }
    public TextBounds InkBounds { get; }
    public TextBounds Bounds { get; }
    public long StorageBytes { get; }
    internal ReadOnlySpan<Placement> Placements => glyphs.AsSpan(0, GlyphCount);

    private TextLayout(TrueTypeFont font, TextOptions options, Builder b, int sourceLength, long storageBytes)
    {
        Font = font; FontSize = options.FontSize; LineAdvance = b.LineAdvance; SourceLength = sourceLength;
        glyphs = b.Glyphs; lines = b.Lines; outlines = b.Shapes; GlyphCount = b.Emitted; LineCount = b.LineCount;
        LayoutBounds = b.LayoutBounds; InkBounds = b.InkBounds; Bounds = b.Bounds; StorageBytes = storageBytes;
    }

    public TextGlyph GetGlyph(int index)
    {
        if ((uint)index >= GlyphCount) throw new ArgumentOutOfRangeException(nameof(index));
        var p = glyphs[index]; return new(p.GlyphIndex, p.SourceOffset, p.SourceLength, p.LineIndex, p.X, p.Y, p.Advance);
    }
    public TextLine GetLine(int index)
    {
        if ((uint)index >= LineCount) throw new ArgumentOutOfRangeException(nameof(index));
        return lines[index];
    }

    /// <summary>Decode needed outlines and lay out text in pixels, with optional wrapping and line spacing.</summary>
    public static TextLayout Create(TrueTypeFont font, string text, TextOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(font); ArgumentNullException.ThrowIfNull(text); options ??= new();
        if (!double.IsFinite(options.FontSize) || options.FontSize <= 0 || double.IsNaN(options.WrapWidth) || options.WrapWidth <= 0
            || !double.IsFinite(options.LineSpacing) || options.LineSpacing <= 0 || options.TabSpaces < 1)
            throw new ArgumentOutOfRangeException(nameof(options));
        var context = new OperationContext(options.Limits, cancellationToken);
        if (options.FontSize > context.Limits.MaxDimension) throw new ImageLimitException(ImageLimitKind.Dimension, "Font size exceeds the dimension limit.");
        var initial = context.RemainingWorkingBytes;
        context.Reserve(font.StorageBytes); context.Reserve(text.Length * 2L); context.Reserve(512);
        Builder? builder = null;
        try
        {
            var count = Count(text, context); builder = new(font, options, context, count);
            builder.Read(text); builder.Arrange(); builder.Measure();
            builder.ReleasePreparation(); context.Release(text.Length * 2L);
            return new(font, options, builder, text.Length, initial - context.RemainingWorkingBytes);
        }
        catch
        {
            builder?.ReleasePreparation();
            if (builder is not null) foreach (var shape in builder.Shapes.Values) shape.Outline.Dispose();
            context.Release(initial - context.RemainingWorkingBytes); throw;
        }
    }

    private static int Count(string text, OperationContext context)
    {
        var count = 0;
        for (var offset = 0; offset < text.Length;)
        {
            context.Work(1);
            if (Rune.DecodeFromUtf16(text.AsSpan(offset), out var rune, out var length) != OperationStatus.Done)
                throw new ArgumentException("Text must contain valid Unicode scalars.", nameof(text));
            if (rune.Value == '\r' && offset + length < text.Length && text[offset + length] == '\n') length++;
            offset += length;
            if (++count > context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "Text item limit exceeded.");
        }
        return count;
    }

    private sealed class Builder
    {
        private readonly TrueTypeFont font;
        private readonly TextOptions options;
        private readonly OperationContext context;
        private readonly double scale, bodyHeight;
        private readonly int itemCount;
        private TrueTypeFont.TextKerning? kerning;
        private double pen, lineMin, lineMax, allMin, allMax;
        private int lineFirst, previousGlyph = -1;
        private bool lineHasWord;
        internal readonly Placement[] Glyphs;
        internal TextLine[] Lines = [];
        internal readonly Dictionary<int, Shape> Shapes = new();
        internal int Emitted, LineCount;
        internal double LineAdvance;
        internal TextBounds LayoutBounds, InkBounds, Bounds;

        internal Builder(TrueTypeFont font, TextOptions options, OperationContext context, int count)
        {
            this.font = font; this.options = options; this.context = context; itemCount = count;
            scale = options.FontSize / font.UnitsPerEm; bodyHeight = (font.Ascender - (double)font.Descender) * scale;
            LineAdvance = (font.Ascender - (double)font.Descender + Math.Max(0, font.LineGap)) * scale * options.LineSpacing;
            if (scale == 0 || !double.IsFinite(LineAdvance) || LineAdvance == 0)
                throw new ArgumentOutOfRangeException(nameof(options), "Font size or line spacing is outside the numeric range.");
            var bytes = 32 + count * 64L;
            if (bytes > Array.MaxLength) throw new ImageLimitException(ImageLimitKind.BufferLength, "Text glyph storage exceeds the managed limit.");
            context.Reserve(bytes); context.Reserve(256); Glyphs = new Placement[count];
            if (count != 0 && options.Kerning) kerning = new(font, context);
        }

        private Shape Get(int glyph)
        {
            context.Work(1); if (Shapes.TryGetValue(glyph, out var shape)) return shape;
            context.Reserve(512); FontOutline? outline = null;
            try
            {
                outline = font.OutlineForText(glyph, context); var b = outline.TightBounds(context);
                shape = new(outline, b.XMin, b.YMin, b.XMax, b.YMax); Shapes.Add(glyph, shape); return shape;
            }
            catch { outline?.Dispose(); context.Release(512); throw; }
        }
        internal void Read(string text)
        {
            var index = 0;
            for (var offset = 0; offset < text.Length;)
            {
                context.Work(1); Rune.DecodeFromUtf16(text.AsSpan(offset), out var rune, out var length);
                if (rune.Value == '\r' && offset + length < text.Length && text[offset + length] == '\n') length++;
                var p = new Placement { SourceOffset = offset, SourceLength = length, GlyphIndex = -1 };
                if (rune.Value is '\r' or '\n' or 0x2028 or 0x2029) p.Kind = 3;
                else if (rune.Value == '\t') p.Kind = 2;
                else
                {
                    p.GlyphIndex = font.MapForText(rune.Value, context); p.Shape = Get(p.GlyphIndex);
                    p.Advance = p.Shape.Outline.Advance * scale;
                    p.Kind = Rune.IsWhiteSpace(rune) && rune.Value is not (0xA0 or 0x202F) ? (byte)1 : (byte)0;
                }
                Glyphs[index++] = p; offset += length;
            }
        }
        private double Kern(int left, int right) => (kerning?.Get(left, right) ?? 0) * scale;
        private double Baseline => font.Ascender * scale + LineCount * LineAdvance;

        internal void Arrange()
        {
            if (itemCount == 0) return;
            int i = 0, pendingStart = -1; double pendingPen = 0, pendingMin = 0, pendingMax = 0;
            while (i < itemCount)
            {
                var p = Glyphs[i];
                if (p.Kind == 3) { FinishLine(); i++; pendingStart = -1; continue; }
                if (p.Kind is 1 or 2)
                {
                    if (pendingStart < 0) { pendingStart = Emitted; pendingPen = pen; pendingMin = lineMin; pendingMax = lineMax; }
                    Place(p); i++; continue;
                }
                var end = i + 1; while (end < itemCount && Glyphs[end].Kind == 0) { context.Work(1); end++; }
                double width = 0; var prev = previousGlyph;
                for (var j = i; j < end; j++)
                { context.Work(1); var word = Glyphs[j]; width += Kern(prev, word.GlyphIndex) + word.Advance; prev = word.GlyphIndex; }
                if (lineHasWord && pen + width > options.WrapWidth)
                {
                    if (pendingStart >= 0) { Emitted = pendingStart; pen = pendingPen; lineMin = pendingMin; lineMax = pendingMax; }
                    FinishLine();
                }
                pendingStart = -1;
                while (i < end)
                {
                    p = Glyphs[i];
                    if (lineHasWord && pen + Kern(previousGlyph, p.GlyphIndex) + p.Advance > options.WrapWidth) FinishLine();
                    Place(p); lineHasWord = true; i++;
                }
            }
            FinishLine();
        }

        private void Place(Placement p)
        {
            context.Work(1);
            if (p.Kind == 2)
            {
                var space = Get(font.MapForText(' ', context)).Outline.Advance * scale;
                var stop = (space > 0 ? space : options.FontSize / 3) * options.TabSpaces;
                pen = (Math.Floor(pen / stop) + 1) * stop; previousGlyph = -1;
            }
            else
            {
                pen += Kern(previousGlyph, p.GlyphIndex); p.X = pen; p.Y = Baseline; p.LineIndex = LineCount;
                Glyphs[Emitted++] = p; lineMin = Math.Min(lineMin, pen); lineMax = Math.Max(lineMax, pen);
                pen += p.Advance; previousGlyph = p.GlyphIndex;
            }
            lineMin = Math.Min(lineMin, pen); lineMax = Math.Max(lineMax, pen);
        }
        private void FinishLine()
        {
            context.Work(1);
            if (LineCount == context.Limits.MaxContainerItems) throw new ImageLimitException(ImageLimitKind.ContainerItems, "Text line limit exceeded.");
            if (LineCount == Lines.Length)
            {
                var capacity = (int)Math.Min(context.Limits.MaxContainerItems, Math.Max(LineCount + 1L, Math.Max(4, Lines.Length * 2L)));
                var bytes = 32 + capacity * 64L;
                if (bytes > Array.MaxLength) throw new ImageLimitException(ImageLimitKind.BufferLength, "Text line storage exceeds the managed limit.");
                context.Reserve(bytes); TextLine[] next;
                try { context.Work(LineCount); next = new TextLine[capacity]; Lines.AsSpan(0, LineCount).CopyTo(next); }
                catch { context.Release(bytes); throw; }
                if (Lines.Length != 0) context.Release(32 + Lines.Length * 64L);
                Lines = next;
            }
            Lines[LineCount] = new(lineFirst, Emitted - lineFirst, Baseline, pen); LineCount++;
            allMin = Math.Min(allMin, lineMin); allMax = Math.Max(allMax, lineMax);
            pen = lineMin = lineMax = 0; lineFirst = Emitted; previousGlyph = -1; lineHasWord = false;
        }

        internal void Measure()
        {
            var hasInk = false; double minX = 0, minY = 0, maxX = 0, maxY = 0;
            for (var i = 0; i < Emitted; i++)
            {
                context.Work(1); var p = Glyphs[i]; var shape = p.Shape!;
                if (shape.XMin == shape.XMax || shape.YMin == shape.YMax) continue;
                var x0 = p.X + (shape.XMin - shape.Outline.Left.X) * scale; var x1 = p.X + (shape.XMax - shape.Outline.Left.X) * scale;
                var y0 = p.Y - shape.YMax * scale; var y1 = p.Y - shape.YMin * scale;
                if (!hasInk) { minX = x0; minY = y0; maxX = x1; maxY = y1; hasInk = true; }
                else { minX = Math.Min(minX, x0); minY = Math.Min(minY, y0); maxX = Math.Max(maxX, x1); maxY = Math.Max(maxY, y1); }
            }
            LayoutBounds = LineCount == 0 ? default : new(allMin, 0, allMax - allMin, bodyHeight + (LineCount - 1) * LineAdvance);
            InkBounds = hasInk ? new(minX, minY, maxX - minX, maxY - minY) : default;
            Bounds = hasInk ? new(Math.Min(LayoutBounds.X, minX), Math.Min(0, minY), Math.Max(LayoutBounds.Right, maxX) - Math.Min(LayoutBounds.X, minX),
                Math.Max(LayoutBounds.Bottom, maxY) - Math.Min(0, minY)) : LayoutBounds;
            var width = Math.Ceiling(Bounds.Width); var height = Math.Ceiling(Bounds.Height);
            if (!double.IsFinite(width) || !double.IsFinite(height) || width > context.Limits.MaxDimension || height > context.Limits.MaxDimension)
                throw new ImageLimitException(ImageLimitKind.Dimension, "Text extent exceeds the dimension limit.");
            if (width * height > context.Limits.MaxPixels) throw new ImageLimitException(ImageLimitKind.PixelCount, "Text extent exceeds the pixel limit.");
        }
        internal void ReleasePreparation() { kerning?.Dispose(); kerning = null; }
    }
}
