using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tooling.AuthoredOpenType;

namespace Lokad.Image.Tests;

public sealed class TextShapingTests
{
    private static TextOptions Options => new() { FontSize = 10, Shaping = TextShaping.OpenType };
    private static TrueTypeFont Font(params (string Tag, byte[] Bytes)[] extra)
        => TrueTypeFont.Load(AuthoredTrueType.Build(tableOverrides: extra.ToDictionary(e => e.Tag, e => e.Bytes)));
    private static byte[] Mapping() => AuthoredTrueType.ScalarMapping((' ', 1), ('A', 2), ('V', 3), ('g', 4), (0x301, 6),
        (0x5D0, 2), (0x5D1, 3), (0x628, 2), (0x627, 3), (0x28, 2), (0x29, 3), (0x2211, 2));
    private static TextGlyph[] Glyphs(TextLayout layout) => Enumerable.Range(0, layout.GlyphCount).Select(layout.GetGlyph).ToArray();

    [Fact]
    public void CompositionAndAnchorsShareTheRetainedOutlineMetricsAndClusters()
    {
        using var font = Font(("cmap", Mapping()),
            ("GSUB", Substitution([new(1, 0, [Single([6], [7])])], [new("ccmp", [0])])),
            ("GPOS", Positioning([new(4, 0, [MarkPosition(2, 7, Anchor(300, 700), Anchor(50, 0))])], [new("mark", [0])])),
            ("GDEF", Definitions(ClassRanges((2, 2, 1), (7, 7, 3)))));
        using var layout = TextLayout.Create(font, "A\u0301V", Options);
        Assert.Equal(new[] { 2, 7, 3 }, Glyphs(layout).Select(g => g.GlyphIndex));
        Assert.Equal(2.5, layout.GetGlyph(1).X, 8); Assert.Equal(1, layout.GetGlyph(1).BaselineY, 8);
        Assert.Equal(0, layout.GetGlyph(1).AdvanceWidth); Assert.Equal(6, layout.GetGlyph(2).X, 8);
        Assert.Equal((1, 1), (layout.GetGlyph(1).SourceOffset, layout.GetGlyph(1).SourceLength));
    }

    [Fact]
    public void RequiredLigaturesRunAfterJoiningFormsAndPreserveTheirSourceSpan()
    {
        var script = new[] { ("arab", Script([0, 1], required: 2)) };
        var program = Substitution([new(1, 0, [Single([2], [4])]), new(1, 0, [Single([3], [5])]),
            new(4, 0, [Ligature(4, (7, new[] { 5 }))])], [new("init", [0]), new("fina", [1]), new("rlig", [2])], script);
        using var font = Font(("cmap", Mapping()), ("GSUB", program));
        using var layout = TextLayout.Create(font, "با", Options);
        Assert.Equal(1, layout.GlyphCount); Assert.Equal(7, layout.GetGlyph(0).GlyphIndex);
        Assert.Equal((0, 2), (layout.GetGlyph(0).SourceOffset, layout.GetGlyph(0).SourceLength));
        Assert.Equal(TextDirection.RightToLeft, layout.GetLine(0).Direction);
    }

    [Theory]
    [InlineData("بب", new[] { 5, 4 })]
    [InlineData("ب\u200Dب", new[] { 5, 4 })]
    [InlineData("ب\u200Cب", new[] { 2, 2 })]
    [InlineData("Aب\u200Dب", new[] { 2, 5, 4 })]
    public void JoiningControlsSurviveDirectionRemovalWithoutBecomingVisible(string text, int[] expected)
    {
        using var font = Font(("cmap", Mapping()), ("GSUB", Substitution(
            [new(1, 0, [Single([2], [4])]), new(1, 0, [Single([2], [5])])],
            [new("init", [0]), new("fina", [1])], [("arab", Script([0, 1]))])));
        using var layout = TextLayout.Create(font, text, Options);
        Assert.Equal(expected, Glyphs(layout).Select(g => g.GlyphIndex));
        Assert.All(Glyphs(layout), g => Assert.NotEqual(0, g.GlyphIndex));
    }

    [Fact]
    public void ExplicitEmbeddingScopesAndLineDirectionPreserveSourceOffsets()
    {
        using var font = Font(("cmap", Mapping()));
        using var layout = TextLayout.Create(font, "A\u2067אב\u2069V\nאב", Options);
        Assert.Equal(new[] { 0, 3, 2, 5, 8, 7 }, Glyphs(layout).Select(g => g.SourceOffset));
        Assert.Equal(TextDirection.LeftToRight, layout.GetLine(0).Direction);
        Assert.Equal(TextDirection.RightToLeft, layout.GetLine(1).Direction);
    }

    [Fact]
    public void EncodedMirrorsAndFontProvidedMirrorsAreAppliedOnlyToRightToLeftGlyphs()
    {
        using var paired = Font(("cmap", Mapping()));
        using var layout = TextLayout.Create(paired, "א(A)", Options);
        Assert.Equal(new[] { 2, 2, 3, 2 }, Glyphs(layout).Select(g => g.GlyphIndex));
        var bytes = Substitution([new(1, 0, [Single([2], [7])])], [new("rtlm", [0])]);
        using var unencoded = Font(("cmap", Mapping()), ("GSUB", bytes));
        using var mirrored = TextLayout.Create(unencoded, "∑", Options with { Direction = TextDirection.RightToLeft });
        Assert.Equal(7, mirrored.GetGlyph(0).GlyphIndex);
        var unsupported = Assert.Throws<UnsupportedImageException>(() => TextLayout.Create(paired, "∑", Options with { Direction = TextDirection.RightToLeft }));
        Assert.Equal(UnsupportedImageFeature.FontTable, unsupported.Feature);
    }

    [Fact]
    public void WrappingNeverSplitsAClusterAndResetsKerningAtLineEdges()
    {
        using var font = Font(("cmap", Mapping()));
        using var clusters = TextLayout.Create(font, "A\u0301V", Options with { WrapWidth = 1 });
        Assert.Equal(2, clusters.LineCount); Assert.Equal(0, clusters.GetGlyph(1).LineIndex);
        using var kerned = TextLayout.Create(font, "AV", Options with { WrapWidth = 1 });
        Assert.Equal(2, kerned.LineCount); Assert.Equal(6, kerned.GetLine(0).AdvanceWidth, 8);
        Assert.Equal(0, kerned.GetGlyph(1).X);
        using var leading = TextLayout.Create(font, "  AV", Options with { WrapWidth = 1 });
        Assert.Equal(2, leading.LineCount); Assert.Equal(3, leading.GetLine(0).GlyphCount);
    }

    [Fact]
    public void LinePositioningRechecksWidthsAfterRemovingCrossLinePairAdjustments()
    {
        var kern = Positioning([new(2, 0, [PairPosition(2, 4, 0, (3, [-400], []))])], [new("kern", [0])]);
        using var font = Font(("GPOS", kern));
        using var layout = TextLayout.Create(font, "AAV", Options with { WrapWidth = 7 });
        Assert.Equal(3, layout.LineCount);
        Assert.All(Enumerable.Range(0, layout.LineCount), i => Assert.Equal(6, layout.GetLine(i).AdvanceWidth, 8));
        Assert.All(Glyphs(layout), glyph => Assert.Equal(0, glyph.X));
    }

    [Fact]
    public void UnsupportedScriptsJoiningProgramsAndLogicalDirectionAreExplicit()
    {
        using var font = Font(("cmap", Mapping()));
        Assert.Equal(UnsupportedImageFeature.FontTable, Assert.Throws<UnsupportedImageException>(() => TextLayout.Create(font, "بب", Options)).Feature);
        Assert.Equal(UnsupportedImageFeature.FontTable, Assert.Throws<UnsupportedImageException>(() => TextLayout.Create(font, "क", Options)).Feature);
        Assert.Throws<ArgumentException>(() => TextLayout.Create(font, "A", Options with { Shaping = TextShaping.Logical, Direction = TextDirection.RightToLeft }));
    }

    [Theory]
    [InlineData(TextAlignment.Center, 12)] [InlineData(TextAlignment.End, 24)]
    public void AlignmentUsesTheExplicitWrapBox(TextAlignment alignment, double expected)
    {
        using var font = Font();
        using var layout = TextLayout.Create(font, "A\nV", Options with { WrapWidth = 30, Alignment = alignment });
        Assert.Equal(expected, layout.GetGlyph(0).X); Assert.Equal(expected, layout.GetGlyph(1).X);
    }

    [Fact]
    public void LayoutStorageAndDrawingRemainIndependentAfterTheFontIsDisposed()
    {
        using var budget = new TestMemoryBudget(1_000_000); var limits = new ImageLimits { MemoryBudget = budget };
        var font = TrueTypeFont.Load(AuthoredTrueType.Build(tableOverrides: new Dictionary<string, byte[]> { ["cmap"] = Mapping() }), limits);
        using var layout = TextLayout.Create(font, "A\u0301 אב", Options with { Limits = limits }); var before = budget.Bytes;
        font.Dispose(); Assert.InRange(budget.Bytes, 1, before - 1);
        using (var canvas = ImageBuffer.Create(60, 30, 16, limits)) ImageDrawing.DrawText(canvas, layout, 0, 0, RgbaColor.From8(1, 2, 3), limits);
        layout.Dispose(); Assert.Equal(0, budget.Bytes); Assert.False(budget.IsDisposed);
    }

    [Fact]
    public void EveryGrowingAdmissionCanRefuseWithoutLeakingOrSelectingAnotherPath()
    {
        using var budget = new TestMemoryBudget(10_000_000);
        using var font = Font(("cmap", Mapping())); var limits = new ImageLimits { MemoryBudget = budget };
        using (var layout = TextLayout.Create(font, "AV\nאב A\u0301", Options with { Limits = limits })) Assert.True(budget.Bytes > 0);
        var growth = budget.GrowthAttempts; Assert.Equal(0, budget.Bytes);
        for (var i = 1; i <= growth; i++)
        {
            budget.RefuseAtGrowth = budget.GrowthAttempts + i;
            var error = Assert.Throws<ImageLimitException>(() => TextLayout.Create(font, "AV\nאב A\u0301", Options with { Limits = limits }));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(0, budget.Bytes);
        }
        budget.RefuseAtGrowth = 0; using (var next = TextLayout.Create(font, "AV", Options with { Limits = limits })) Assert.True(budget.Bytes > 0);
        Assert.Equal(0, budget.Bytes);
    }
}
