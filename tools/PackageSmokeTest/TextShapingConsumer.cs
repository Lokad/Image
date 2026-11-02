using System.Runtime.InteropServices;
using System.Security.Cryptography;
using static Lokad.Image.Tooling.AuthoredOpenType;

namespace Lokad.Image.Tooling;

internal static class TextShapingConsumer
{
    internal static void Verify()
    {
        VerifyRasterIntervals();
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
        var defaultCorrection = Attach(Words(1, 0, 0x44, -50, 0), (2, Coverage(4)), (8, Words(0, 0, 0x8000)));
        var contextBytes = AuthoredTrueType.Build(tableOverrides: new Dictionary<string, byte[]>
        {
            ["GSUB"] = Substitution([new(5, 0, [SequenceCoverages([[2], [3]], (0, 1))]),
                new(1, 0, [Single([2], [4])])], [new("ccmp", [0])]),
            ["GPOS"] = Positioning([new(1, 0, [defaultCorrection])], [new("kern", [0])]),
            ["GDEF"] = DefinitionsWithVariation(ItemVariationStore([[[0, 16384, 16384]]], 1, [50]))
        });
        var contextFont = TrueTypeFont.Load(contextBytes, limits);
        using (var contextLayout = TextLayout.Create(contextFont, "AV", new() { FontSize = 10, Shaping = TextShaping.OpenType, Limits = limits }))
        {
            if (contextLayout.GlyphCount != 2 || contextLayout.GetGlyph(0).GlyphIndex != 4
                || contextLayout.GetGlyph(1).GlyphIndex != 3 || contextLayout.GetGlyph(1).SourceOffset != 1
                || contextLayout.GetGlyph(0).AdvanceWidth != 5.5)
                throw new Exception("Package contextual substitution differs.");
            contextFont.Dispose();
            using var canvas = ImageBuffer.Create(30, 20, 8, limits);
            ImageDrawing.DrawText(canvas, contextLayout, 0, 0, RgbaColor.From8(1, 2, 3), limits);
        }
        if (budget.Bytes != 0 || budget.IsDisposed) throw new Exception("Package contextual-layout ownership differs.");
    }
    private static void VerifyRasterIntervals()
    {
        foreach (var depth in new[] { 8, 16 })
        {
            var outline = AuthoredTrueType.Outline([
                [(0, 0, true), (750, 700, true), (50, 700, true), (700, 0, true)],
                [(100, 150, true), (100, 550, true), (650, 550, true), (650, 150, true)],
                [(100, 550, true), (450, 950, false), (650, 550, true), (400, -150, false)],
                [(100, 550, true), (100, 650, true), (300, 650, true), (300, 550, true)]]);
            using var font = TrueTypeFont.Load(AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]> { [2] = outline }));
            using var layout = TextLayout.Create(font, "A gA", new TextOptions { FontSize = 13.25 });
            using var canvas = ImageBuffer.Create(32, 24, depth);
            for (var y = 0; y < canvas.Height; y++)
            for (var x = 0; x < canvas.Width; x++)
            for (var c = 0; c < 4; c++)
            {
                var value = (x * 1237 + y * 979 + c * 16381) & 65535;
                if (depth == 8) canvas.GetRow8(y)[4 * x + c] = (byte)(value >> 8);
                else canvas.GetRow16(y)[4 * x + c] = (ushort)value;
            }
            ImageDrawing.DrawText(canvas, layout, -.375, 1.625, new(12345, 54321, 32769, 45678));
            ImageDrawing.DrawText(canvas, layout, 4.125, -2.375, new(65535, 1, 1234, 1));
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            for (var y = 0; y < canvas.Height; y++)
                hash.AppendData(depth == 8 ? canvas.GetRow8(y) : MemoryMarshal.AsBytes(canvas.GetRow16(y)));
            var actual = Convert.ToHexStringLower(hash.GetHashAndReset());
            var expected = depth == 8 ? "eb901bf157734333b82dd0d7b812b8df4492d4ff30fef5fd048e583d725ebe33"
                : "057386b4b48143380d16b74942c6ecc4f030ead377c81ffbc8b7dd9902c71ea0";
            if (actual != expected) throw new Exception("Packaged text interval preparation changed exact mixed-alpha pixels.");
        }
        Console.WriteLine("Packaged text interval preparation preserves exact clipped mixed-alpha byte/word frames.");
    }
}
