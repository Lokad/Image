using static Lokad.Image.Tooling.AuthoredOpenType;

namespace Lokad.Image.Tooling;

internal static class TextShapingConsumer
{
    internal static void Verify()
    {
        var bytes = AuthoredTrueType.Build(tableOverrides: new Dictionary<string, byte[]>
        {
            ["cmap"] = AuthoredTrueType.ScalarMapping(('A', 2), ('V', 3), (' ', 1), (0x628, 2)),
            ["GSUB"] = Substitution([new(1, 0, [Single([2], [4])]), new(1, 0, [Single([2], [5])])],
                [new("init", [0]), new("fina", [1])], [("arab", Script([0, 1]))])
        });
        using var budget = new TestMemoryBudget(1_000_000); var limits = new ImageLimits { MemoryBudget = budget };
        var font = TrueTypeFont.Load(bytes, limits);
        using var layout = TextLayout.Create(font, "A بب", new() { Shaping = TextShaping.OpenType, FontSize = 10, Limits = limits });
        if (layout.GlyphCount != 4 || layout.GetGlyph(2).GlyphIndex != 5 || layout.GetGlyph(3).GlyphIndex != 4
            || layout.GetGlyph(2).SourceOffset != 3 || layout.GetLine(0).Direction != TextDirection.LeftToRight)
            throw new Exception("Package OpenType joining/direction/source positions differ.");
        font.Dispose();
        using (var canvas = ImageBuffer.Create(50, 20, 16, limits)) ImageDrawing.DrawText(canvas, layout, 0, 0, RgbaColor.From8(1, 2, 3), limits);
        layout.Dispose(); if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Package shaped-layout ownership differs.");
        using var plain = TrueTypeFont.Load(AuthoredTrueType.Build());
        using var wrapped = TextLayout.Create(plain, "AV", new() { Shaping = TextShaping.OpenType, FontSize = 10, WrapWidth = 1 });
        using var aligned = TextLayout.Create(plain, "A", new() { FontSize = 10, WrapWidth = 30, Alignment = TextAlignment.Center });
        if (wrapped.LineCount != 2 || wrapped.GetLine(0).AdvanceWidth != 6 || aligned.GetGlyph(0).X != 12)
            throw new Exception("Package line-boundary positioning/alignment differ.");
        if (!TextLayout.TryFit(plain, "AV", 20, 5, out var fitted, new() { FontSize = 10, Shaping = TextShaping.OpenType, Limits = limits },
            minimumFontSize: 4, fontSizeStep: 1)) throw new Exception("Package fitting unexpectedly failed.");
        using (fitted)
            if (fitted.FontSize != 5 || !fitted.Bounds.Translate(-fitted.Bounds.X, -fitted.Bounds.Y).FitsInside(20, 5))
                throw new Exception("Package fitting size/bounds differ.");
        if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Package fitting ownership differs.");
        var contextBytes = AuthoredTrueType.Build(tableOverrides: new Dictionary<string, byte[]>
        {
            ["GSUB"] = Substitution([new(5, 0, [SequenceCoverages([[2], [3]], (0, 1))]),
                new(1, 0, [Single([2], [4])])], [new("ccmp", [0])])
        });
        var contextFont = TrueTypeFont.Load(contextBytes, limits);
        using (var contextLayout = TextLayout.Create(contextFont, "AV", new() { FontSize = 10, Shaping = TextShaping.OpenType, Limits = limits }))
        {
            if (contextLayout.GlyphCount != 2 || contextLayout.GetGlyph(0).GlyphIndex != 4
                || contextLayout.GetGlyph(1).GlyphIndex != 3 || contextLayout.GetGlyph(1).SourceOffset != 1)
                throw new Exception("Package contextual substitution differs.");
            contextFont.Dispose();
            using var canvas = ImageBuffer.Create(30, 20, 8, limits);
            ImageDrawing.DrawText(canvas, contextLayout, 0, 0, RgbaColor.From8(1, 2, 3), limits);
        }
        if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Package contextual-layout ownership differs.");
    }
}
