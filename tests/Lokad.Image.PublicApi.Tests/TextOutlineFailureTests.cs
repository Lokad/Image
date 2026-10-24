using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class TextOutlineFailureTests
{
    private static TextOptions Options => new() { FontSize = 10 };
    private static TrueTypeFont Font(Dictionary<int, byte[]> glyphs) => TrueTypeFont.Load(AuthoredTrueType.Build(glyphOverrides: glyphs));
    private static byte[] Triangle() => AuthoredTrueType.Outline([[(50, 0, true), (300, 700, true), (550, 0, true)]], compact: false);

    [Theory]
    [InlineData("endpoints")] [InlineData("repeat")] [InlineData("coordinates")]
    [InlineData("instructions")] [InlineData("design-grid")]
    public void MalformedSimpleGlyphsFailDuringLayoutWithoutPoisoningTheFace(string field)
    {
        var glyph = Triangle();
        switch (field)
        {
            case "endpoints": AuthoredTrueType.W16(glyph, 0, 2); AuthoredTrueType.W16(glyph, 12, 0); break;
            case "repeat": glyph[14] = 9; glyph[15] = 255; break;
            case "coordinates": glyph = glyph[..^2]; break;
            case "instructions": AuthoredTrueType.W16(glyph, 12, 65535); break;
            default: AuthoredTrueType.W16(glyph, 17, 32767); AuthoredTrueType.W16(glyph, 19, 32767); break;
        }
        var font = Font(new() { [2] = glyph });
        Assert.Throws<ImageDataException>(() => TextLayout.Create(font, "A", Options));
        Assert.Equal(1, TextLayout.Create(font, "V", Options).GlyphCount);
        Assert.Equal(2, font.GetGlyphIndex(new System.Text.Rune('A')));
    }

    [Theory]
    [InlineData("missing-child")] [InlineData("first-point")] [InlineData("point-range")]
    [InlineData("matrix")] [InlineData("offset")] [InlineData("truncated")]
    [InlineData("self-cycle")] [InlineData("indirect-cycle")] [InlineData("instructions")]
    public void MalformedCompositeGraphsAndRecordsStayWithinPublicFailureCategories(string field)
    {
        var glyphs = new Dictionary<int, byte[]>(); byte[] raw;
        switch (field)
        {
            case "missing-child": raw = AuthoredTrueType.Compound([new(99, 0, 0)]); break;
            case "first-point": raw = AuthoredTrueType.Compound([new(2, 0, 0, Flags: 1)]); break;
            case "point-range": raw = AuthoredTrueType.Compound([new(2, 0, 0), new(6, 65535, 0, Flags: 1)]); break;
            case "matrix": raw = AuthoredTrueType.Compound([new(2, 0, 0)]); AuthoredTrueType.W16(raw, 10, 3 | 8 | 64); break;
            case "offset": raw = AuthoredTrueType.Compound([new(2, 0, 0, Flags: 3 | 0x1800)]); break;
            case "truncated": raw = AuthoredTrueType.Compound([new(2, 0, 0, Flags: 3 | 128)]); raw = raw[..^3]; break;
            case "self-cycle": raw = AuthoredTrueType.Compound([new(5, 0, 0)]); break;
            case "indirect-cycle": raw = AuthoredTrueType.Compound([new(6, 0, 0)]); glyphs[6] = AuthoredTrueType.Compound([new(5, 0, 0)]); break;
            default: raw = [.. AuthoredTrueType.Compound([new(2, 0, 0, Flags: 3 | 256)]), 255, 255]; break;
        }
        glyphs[5] = raw; var font = Font(glyphs);
        Assert.Throws<ImageDataException>(() => TextLayout.Create(font, "\u00C1", Options));
        Assert.Equal(1, TextLayout.Create(font, "A", Options).GlyphCount);
    }

    [Theory]
    [InlineData(63, true)] [InlineData(64, false)]
    public void BoundedNestingSupportsSixtyFourGlyphNodesAndRejectsDeeperProfiles(int last, bool supported)
    {
        // root 5, then glyphs 8..(last+6), then leaf 2.
        var glyphs = new Dictionary<int, byte[]> { [5] = AuthoredTrueType.Compound([new(8, 0, 0)]) };
        for (var id = 8; id <= last + 6; id++) glyphs[id] = AuthoredTrueType.Compound([new(id == last + 6 ? 2 : id + 1, 0, 0)]);
        var font = Font(glyphs);
        if (supported) Assert.Equal(1, TextLayout.Create(font, "\u00C1", Options).GlyphCount);
        else Assert.Equal(UnsupportedImageFeature.FontOutline,
            Assert.Throws<UnsupportedImageException>(() => TextLayout.Create(font, "\u00C1", Options)).Feature);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void PointAndEmptyComponentAmplificationConsumeBoundedItems(bool empty)
    {
        var glyphs = new Dictionary<int, byte[]>
        {
            [5] = AuthoredTrueType.Compound([new(6, 0, 0), new(6, 0, 0)]),
            [6] = AuthoredTrueType.Compound([new(7, 0, 0), new(7, 0, 0)]),
            [7] = AuthoredTrueType.Compound([new(empty ? 1 : 2, 0, 0), new(empty ? 1 : 2, 0, 0)])
        };
        var font = Font(glyphs); var limits = new ImageLimits { MaxContainerItems = empty ? 10 : 20 };
        var failure = Assert.Throws<ImageLimitException>(() => TextLayout.Create(font, "\u00C1", Options with { Limits = limits }));
        Assert.Equal(ImageLimitKind.ContainerItems, failure.Limit);
        Assert.Equal(1, TextLayout.Create(font, "A", Options).GlyphCount);
    }

    [Fact]
    public void NegativeCompositeContourCountUsesTheCompositeGrammar()
    {
        var raw = AuthoredTrueType.Compound([new(2, 0, 0)]); AuthoredTrueType.W16(raw, 0, -2);
        var layout = TextLayout.Create(Font(new() { [5] = raw }), "\u00C1", Options);
        Assert.Equal(new TextBounds(.5, 1, 5, 7), layout.InkBounds);
    }

    [Theory]
    [InlineData("version", true)] [InlineData("format", true)] [InlineData("pairs", false)] [InlineData("glyph", false)]
    public void VersionZeroKerningValidationIsExplicitAndCanBeDisabled(string field, bool unsupported)
    {
        var bytes = AuthoredTrueType.Build(); var offset = AuthoredTrueType.Offset(bytes, "kern");
        switch (field)
        {
            case "version": AuthoredTrueType.W16(bytes, offset, 1); break;
            case "format": AuthoredTrueType.W16(bytes, offset + 8, 0x201); break;
            case "pairs": AuthoredTrueType.W16(bytes, offset + 10, 65535); break;
            default: AuthoredTrueType.W16(bytes, offset + 18, 100); break;
        }
        var font = TrueTypeFont.Load(bytes);
        if (unsupported) Assert.Equal(UnsupportedImageFeature.FontTable, Assert.Throws<UnsupportedImageException>(() => TextLayout.Create(font, "AV", Options)).Feature);
        else Assert.Throws<ImageDataException>(() => TextLayout.Create(font, "AV", Options));
        Assert.Equal(12, TextLayout.Create(font, "AV", Options with { Kerning = false }).LayoutBounds.Width);
    }
}
