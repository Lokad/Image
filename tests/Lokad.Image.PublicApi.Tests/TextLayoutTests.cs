using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class TextLayoutTests
{
    private static TextOptions Options => new() { FontSize = 10 };
    private static TrueTypeFont Font(IReadOnlyDictionary<int, byte[]>? changes = null) => TrueTypeFont.Load(AuthoredTrueType.Build(glyphOverrides: changes));
    private static void Bounds(TextBounds actual, double x, double y, double width, double height)
    { Assert.Equal(x, actual.X, 10); Assert.Equal(y, actual.Y, 10); Assert.Equal(width, actual.Width, 10); Assert.Equal(height, actual.Height, 10); }

    [Fact]
    public void PixelMetricsKerningAndQuadraticExtremaUseDecodedGeometry()
    {
        var font = Font(); var layout = TextLayout.Create(font, "AV", Options);
        Assert.Equal((2, 1, 2), (layout.GlyphCount, layout.LineCount, layout.SourceLength));
        Bounds(layout.LayoutBounds, 0, 0, 11.2, 10); Bounds(layout.InkBounds, .5, 1, 10.2, 7);
        Assert.Equal(5.2, layout.GetGlyph(1).X, 10); Assert.Equal(8, layout.GetGlyph(1).BaselineY);
        Assert.Equal(11.2, layout.GetLine(0).AdvanceWidth, 10);
        var unkerned = TextLayout.Create(font, "AV", Options with { Kerning = false }); Bounds(unkerned.LayoutBounds, 0, 0, 12, 10);
        var curve = TextLayout.Create(font, "g", Options); Bounds(curve.InkBounds, 0, 4, 6, 5);
        var declared = font.GetGlyphMetrics(4); Assert.Equal((-200, 800), (declared.YMin, declared.YMax));
        var accent = TextLayout.Create(font, "\u00C1", Options); Bounds(accent.InkBounds, .5, -.5, 5, 8.5);
        Bounds(accent.Bounds, 0, -.5, 6, 10.5);
        Assert.False(accent.Bounds.FitsInside(6, 11)); Assert.True(accent.Bounds.Translate(0, .5).FitsInside(6, 11));
    }

    [Fact]
    public void WrappingDropsAutomaticSeparatorsAndSplitsOverlongWordsWithoutCrossLineKerning()
    {
        var layout = TextLayout.Create(Font(), "AV AVg", Options with { WrapWidth = 12 });
        Assert.Equal((5, 3), (layout.GlyphCount, layout.LineCount)); Bounds(layout.LayoutBounds, 0, 0, 11.2, 34);
        Assert.Equal(new TextLine(0, 2, 8, 11.2), layout.GetLine(0));
        Assert.Equal(new TextLine(2, 2, 20, 11.2), layout.GetLine(1));
        Assert.Equal(new TextLine(4, 1, 32, 6), layout.GetLine(2));
        Assert.Equal((3, 0.0, 1), (layout.GetGlyph(2).SourceOffset, layout.GetGlyph(2).X, layout.GetGlyph(2).LineIndex));
        var single = TextLayout.Create(Font(), "AV", Options with { WrapWidth = 1 });
        Assert.Equal(2, single.LineCount); Bounds(single.LayoutBounds, 0, 0, 6, 22);
        var data = AuthoredTrueType.Build(); var kern = AuthoredTrueType.Offset(data, "kern");
        AuthoredTrueType.W16(data, kern + 18, 3); AuthoredTrueType.W16(data, kern + 20, 2);
        var reset = TextLayout.Create(TrueTypeFont.Load(data), "VA", Options with { WrapWidth = 6 });
        Assert.Equal(0, reset.GetGlyph(1).X); Assert.Equal(1, reset.GetGlyph(1).LineIndex);
    }

    [Fact]
    public void ExplicitBreaksTabsSpacingAndTrailingWhitespaceHaveDeclaredMetrics()
    {
        var font = Font(); var lines = TextLayout.Create(font, "A\r\nV\u2028g\u2029", Options);
        Assert.Equal((3, 4, 7), (lines.GlyphCount, lines.LineCount, lines.SourceLength));
        Bounds(lines.LayoutBounds, 0, 0, 6, 46); Assert.Equal(5, lines.GetGlyph(2).SourceOffset);
        Assert.Equal(new TextLine(3, 0, 44, 0), lines.GetLine(3));
        var tab = TextLayout.Create(font, "A\t\tV", Options); Assert.Equal(2, tab.GlyphCount); Assert.Equal(24, tab.GetGlyph(1).X);
        Bounds(tab.LayoutBounds, 0, 0, 30, 10);
        var spaced = TextLayout.Create(font, "A\nA", Options with { LineSpacing = 2 });
        Assert.Equal(24, spaced.LineAdvance); Assert.Equal(32, spaced.GetGlyph(1).BaselineY); Bounds(spaced.LayoutBounds, 0, 0, 6, 34);
        Bounds(TextLayout.Create(font, "A  ", Options).LayoutBounds, 0, 0, 12, 10);
        Bounds(TextLayout.Create(font, "  A", Options).LayoutBounds, 0, 0, 12, 10);
    }

    [Fact]
    public void EmptyMissingAndSupplementaryGlyphsAndValueOwnershipRemainExplicit()
    {
        var font = Font(); var empty = TextLayout.Create(font, "", Options);
        Assert.Equal((0, 0), (empty.GlyphCount, empty.LineCount)); Bounds(empty.Bounds, 0, 0, 0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => empty.GetGlyph(0)); Assert.Throws<ArgumentOutOfRangeException>(() => empty.GetLine(-1));
        var layout = TextLayout.Create(font, "X\U0001F642", Options); Assert.Equal(3, layout.SourceLength);
        Assert.Equal((0, 0, 1), (layout.GetGlyph(0).GlyphIndex, layout.GetGlyph(0).SourceOffset, layout.GetGlyph(0).SourceLength));
        Assert.Equal((7, 1, 2), (layout.GetGlyph(1).GlyphIndex, layout.GetGlyph(1).SourceOffset, layout.GetGlyph(1).SourceLength));
        var changed = layout.GetGlyph(0) with { X = 1000 }; Assert.Equal(1000, changed.X); Assert.Equal(0, layout.GetGlyph(0).X);
        Assert.True(layout.StorageBytes > font.StorageBytes); Assert.Same(font, layout.Font);
    }

    [Theory]
    [InlineData(FontStyle.Regular)] [InlineData(FontStyle.Bold)] [InlineData(FontStyle.Italic)] [InlineData(FontStyle.Bold | FontStyle.Italic)]
    public void SuppliedFaceAndFontSizeDetermineLayoutWithoutChangingOtherResults(FontStyle style)
    {
        var bytes = AuthoredTrueType.Build(style: style); var font = TrueTypeFont.Load(bytes);
        var small = TextLayout.Create(font, "Ag", Options); var large = TextLayout.Create(font, "Ag", Options with { FontSize = 20 });
        Assert.Equal(style, small.Font.Style); Assert.Equal(small.Bounds.Width * 2, large.Bounds.Width, 10);
        Assert.Equal(small.Bounds.Height * 2, large.Bounds.Height, 10);
        var saved = small.Bounds; bytes.AsSpan().Fill(0xA7);
        Parallel.For(0, 16, _ => Assert.Equal(saved, TextLayout.Create(font, "Ag", Options).Bounds));
        Assert.Equal(saved, small.Bounds);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CompactRepeatedFlagsAndImplicitEndpointsYieldTheSameCurves(bool compact)
    {
        var raw = AuthoredTrueType.Outline([[(0, 0, true), (0, 200, false), (200, 200, false), (200, 0, true)]], compact);
        var font = Font(new Dictionary<int, byte[]> { [2] = raw });
        var layout = TextLayout.Create(font, "A", Options); Bounds(layout.InkBounds, 0, 6, 2, 2);
        // All-off-curve contour creates four implied midpoints and rounded corners.
        raw = AuthoredTrueType.Outline([[(0, 0, false), (0, 200, false), (200, 200, false), (200, 0, false)]], compact);
        layout = TextLayout.Create(Font(new Dictionary<int, byte[]> { [2] = raw }), "A", Options);
        Bounds(layout.InkBounds, 0, 6, 2, 2);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ScaledOffsetsAndByteArgumentsFollowTheDeclaredMatrix(bool scaled)
    {
        var raw = AuthoredTrueType.Compound([new(2, 101, 100, Flags: 2 | 8 | 4 | (scaled ? 0x800 : 0), Xx: 8192)]);
        var layout = TextLayout.Create(Font(new Dictionary<int, byte[]> { [5] = raw }), "\u00C1", Options);
        Bounds(layout.InkBounds, scaled ? .755 : 1.26, scaled ? 4 : 3.5, 2.5, 3.5);
    }

    [Fact]
    public void TwoByTwoTransformAndPointAttachmentApplyBeforeAlignment()
    {
        var matrix = AuthoredTrueType.Compound([new(2, 0, 0, Flags: 3 | 128, Xx: 16384, Yx: -8192, Xy: 8192, Yy: 16384)]);
        var transformed = TextLayout.Create(Font(new Dictionary<int, byte[]> { [5] = matrix }), "\u00C1", Options);
        Bounds(transformed.InkBounds, .5, 2.5, 6, 8.25);
        var attached = AuthoredTrueType.Compound([new(2, 0, 0), new(6, 1, 0, Flags: 1 | 8, Xx: 8192)]);
        var aligned = TextLayout.Create(Font(new Dictionary<int, byte[]> { [5] = attached }), "\u00C1", Options);
        Bounds(aligned.InkBounds, .5, .65, 5, 7.35);
    }

    [Fact]
    public void MetricsAndPhantomAttachmentsWorkAfterAnEmptyFirstComponent()
    {
        var raw = AuthoredTrueType.Compound([new(1, 0, 0, Flags: 3 | 512), new(2, 1, 0, Flags: 1)]);
        var layout = TextLayout.Create(Font(new Dictionary<int, byte[]> { [5] = raw }), "\u00C1", Options);
        Assert.Equal(3, layout.GetGlyph(0).AdvanceWidth); Bounds(layout.InkBounds, 3, 1, 5, 7); Bounds(layout.Bounds, 0, 0, 8, 10);
    }

    [Fact]
    public void InheritedMetricsAreIndependentOfTheComponentTransformAndOffset()
    {
        var raw = AuthoredTrueType.Compound([new(2, 100, 0, Flags: 3 | 8 | 512, Xx: 8192)]);
        var bytes = AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]> { [6] = raw });
        AuthoredTrueType.W16(bytes, AuthoredTrueType.Offset(bytes, "head") + 16, 0);
        AuthoredTrueType.W16(bytes, AuthoredTrueType.Offset(bytes, "hmtx") + 10, 24);
        var font = TrueTypeFont.Load(bytes); Assert.Equal(0, font.GetGlyphMetrics(6).AdvanceWidth);
        var layout = TextLayout.Create(font, "\u0301", Options);
        Assert.Equal(6, layout.GetGlyph(0).AdvanceWidth); Bounds(layout.InkBounds, .99, 4.5, 2.5, 3.5);
    }

    [Theory]
    [InlineData(false, 128, 2, 7, 1)] [InlineData(true, 258, 3, 6.3, 1.7)]
    public void PointArgumentsAreUnsignedAndKeepOriginalPointNumbering(bool words, int attachment, double width, double y, double height)
    {
        (int X, int Y, bool On)[] square = [(0, 0, true), (100, 0, true), (100, 100, true), (0, 100, true)];
        var points = Enumerable.Range(0, 260).Select(i => square[i % 4]).ToArray();
        var baseGlyph = AuthoredTrueType.Outline([points]);
        var combined = AuthoredTrueType.Compound([new(2, 0, 0), new(6, attachment, 0, Flags: words ? 1 : 0)]);
        var layout = TextLayout.Create(Font(new Dictionary<int, byte[]> { [2] = baseGlyph, [5] = combined }), "\u00C1", Options);
        Bounds(layout.InkBounds, 0, y, width, height);
    }

    [Fact]
    public void InstructionsAreBoundedAndSkippedAndSideBearingSetsTheOrigin()
    {
        var simple = AuthoredTrueType.Outline([[(50, 0, true), (300, 700, true), (550, 0, true)]], instructions: [0xFF, 0x00, 0x2B]);
        var composite = AuthoredTrueType.Compound([new(2, 0, 0), new(6, 0, 0)], instructions: [0xFF, 0x01]);
        var bytes = AuthoredTrueType.Build(glyphOverrides: new Dictionary<int, byte[]> { [2] = simple, [5] = composite });
        AuthoredTrueType.W16(bytes, AuthoredTrueType.Offset(bytes, "head") + 16, 0);
        AuthoredTrueType.W16(bytes, AuthoredTrueType.Offset(bytes, "hmtx") + 2 * 4 + 2, 25);
        var layout = TextLayout.Create(TrueTypeFont.Load(bytes), "A", Options);
        Bounds(layout.InkBounds, .25, 1, 5, 7);
        Assert.Equal(1, TextLayout.Create(TrueTypeFont.Load(bytes), "\u00C1", Options).GlyphCount);
    }

    [Theory]
    [InlineData(ImageLimitKind.WorkingBytes)] [InlineData(ImageLimitKind.Work)]
    [InlineData(ImageLimitKind.Dimension)] [InlineData(ImageLimitKind.PixelCount)] [InlineData(ImageLimitKind.ContainerItems)]
    public void LayoutLimitsFailWithTypedInformationAndLeaveFontReusable(ImageLimitKind kind)
    {
        var font = Font(); var limits = kind switch
        {
            ImageLimitKind.WorkingBytes => new ImageLimits { MaxWorkingBytes = font.StorageBytes + 100 },
            ImageLimitKind.Work => new ImageLimits { MaxWork = 1 },
            ImageLimitKind.Dimension => new ImageLimits { MaxDimension = 9 },
            ImageLimitKind.PixelCount => new ImageLimits { MaxPixels = 50 },
            _ => new ImageLimits { MaxContainerItems = 1 }
        };
        var failure = Assert.Throws<ImageLimitException>(() => TextLayout.Create(font, "AA", Options with { Limits = limits }));
        Assert.Equal(kind, failure.Limit); Assert.Equal(ImageLimitScope.Operation, failure.Scope);
        Assert.Equal(2, font.GetGlyphIndex(new System.Text.Rune('A')));
        Bounds(TextLayout.Create(font, "A", Options).Bounds, 0, 0, 6, 10);
    }

    [Fact]
    public void ExactSimpleLayoutAdmissionCountsAllLiveCapacities()
    {
        var font = Font(); var limits = new ImageLimits { MaxWorkingBytes = font.StorageBytes + 2497 };
        var layout = TextLayout.Create(font, "A", Options with { Limits = limits });
        Assert.Equal(font.StorageBytes + 2316, layout.StorageBytes);
        var failure = Assert.Throws<ImageLimitException>(() => TextLayout.Create(font, "A", Options with { Limits = limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 } }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
    }

    [Fact]
    public void InvalidUnicodeOptionsPositionsAndPreCancellationStayExplicit()
    {
        var font = Font();
        Assert.Throws<ArgumentException>(() => TextLayout.Create(font, "\uD800", Options));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextLayout.Create(font, "A", Options with { FontSize = double.NaN }));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextLayout.Create(font, "A", Options with { WrapWidth = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(() => TextLayout.Create(font, "A", Options with { LineSpacing = 0 }));
        using var token = new CancellationTokenSource(); token.Cancel();
        var failure = Assert.ThrowsAny<OperationCanceledException>(() => TextLayout.Create(font, "A", Options, token.Token));
        Assert.Equal(token.Token, failure.CancellationToken);
        Assert.Throws<ArgumentOutOfRangeException>(() => default(TextBounds).Translate(double.PositiveInfinity, 0));
        Assert.False(new TextBounds(0, 0, -1, 2).FitsInside(10, 10));
    }
}
