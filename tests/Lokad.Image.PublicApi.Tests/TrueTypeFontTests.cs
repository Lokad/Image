using System.Buffers.Binary;
using System.Text;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class TrueTypeFontTests
{
    [Theory]
    [InlineData(false, false, false)] [InlineData(false, false, true)]
    [InlineData(true, false, false)] [InlineData(true, false, true)]
    [InlineData(false, true, true)] [InlineData(true, true, true)]
    public void OriginalFontMappingsAndMetricsMatchAuthoredGeometry(bool longLocations, bool unicode12, bool glyphArray)
    {
        var data = AuthoredTrueType.Build(longLocations, unicode12, glyphArray); var saved = data.ToArray();
        Assert.Equal(0xB1B0AFBAu, AuthoredTrueType.Checksum(data));
        var font = TrueTypeFont.Load(data);
        Assert.Equal((1000, 8, 800, -200, 200, FontStyle.Regular),
            (font.UnitsPerEm, font.GlyphCount, font.Ascender, font.Descender, font.LineGap, font.Style));
        Assert.Equal(new FontGlyphMetrics(2, 600, 50, 50, 0, 550, 700, true), font.GetGlyphMetrics(new Rune('A')));
        Assert.Equal(new FontGlyphMetrics(4, 600, 0, 0, -200, 600, 800, true), font.GetGlyphMetrics(new Rune('g')));
        Assert.Equal(new FontGlyphMetrics(5, 600, 50, 50, 0, 550, 850, true), font.GetGlyphMetrics(new Rune(193)));
        Assert.Equal(new FontGlyphMetrics(1, 300, 0, 0, 0, 0, 0, false), font.GetGlyphMetrics(new Rune(' ')));
        foreach (var (code, glyph) in new[] { (32, 1), (65, 2), (86, 3), (88, 0), (103, 4), (193, 5), (769, 6), (0xE000, 7) })
            Assert.Equal(glyph, font.GetGlyphIndex(new Rune(code)));
        foreach (var code in new[] { 0, 31, 33, 64, 85, 87, 89, 102, 104, 0xFFFF, 0x10FFFF })
            Assert.Equal(0, font.GetGlyphIndex(new Rune(code)));
        Assert.Equal(unicode12 ? 2 : 0, font.GetGlyphIndex(new Rune('B')));
        Assert.Equal(unicode12 ? 3 : 0, font.GetGlyphIndex(new Rune('C')));
        Assert.Equal(unicode12 ? 7 : 0, font.GetGlyphIndex(new Rune(0x1F642)));
        Assert.Equal(font.GetGlyphMetrics(0), font.GetGlyphMetrics(new Rune('X')));
        Assert.Equal(saved, data);
    }

    [Theory]
    [InlineData(FontStyle.Regular)] [InlineData(FontStyle.Bold)] [InlineData(FontStyle.Italic)]
    [InlineData(FontStyle.Bold | FontStyle.Italic)]
    public void SelectedSuppliedFaceDefinesStyleAndMetricStorage(FontStyle style)
    {
        var font = TrueTypeFont.Load(AuthoredTrueType.Build(style: style));
        Assert.Equal(style, font.Style);
        Assert.Equal((style & FontStyle.Bold) != 0 ? 660 : 600, font.GetGlyphMetrics(2).AdvanceWidth);
        Assert.Equal(0, font.GetGlyphMetrics(6).AdvanceWidth);
        var compact = TrueTypeFont.Load(AuthoredTrueType.Build(style: style, shortMetrics: true));
        Assert.Equal(font.GetGlyphMetrics(2).AdvanceWidth, compact.GetGlyphMetrics(6).AdvanceWidth);
        Assert.Equal(font.GetGlyphMetrics(6).LeftSideBearing, compact.GetGlyphMetrics(6).LeftSideBearing);
        Assert.Equal(font.GetGlyphMetrics(7), compact.GetGlyphMetrics(7));
    }

    [Fact]
    public void CopiedFaceAndValueMetricsSurviveCallerMutationOtherLoadsAndConcurrentReaders()
    {
        var data = AuthoredTrueType.Build(); var font = TrueTypeFont.Load(data); var metrics = font.GetGlyphMetrics(5);
        data.AsSpan().Fill(0xA7); var another = TrueTypeFont.Load(AuthoredTrueType.Build(style: FontStyle.Bold));
        Assert.Throws<UnsupportedImageException>(() => TrueTypeFont.Load(new byte[12]));
        Parallel.For(0, 32, _ =>
        {
            Assert.Equal(2, font.GetGlyphIndex(new Rune('A'))); Assert.Equal(metrics, font.GetGlyphMetrics(5));
            Assert.Equal(660, another.GetGlyphMetrics(2).AdvanceWidth);
        });
        Assert.Equal(new FontGlyphMetrics(5, 600, 50, 50, 0, 550, 850, true), metrics);
    }

    [Fact]
    public void CallerAndOwnedCopyPlusDescriptorFitExactlyAndRemainIndependent()
    {
        var data = AuthoredTrueType.Build();
        var limits = new ImageLimits { MaxInputBytes = data.Length, MaxWorkingBytes = 2L * data.Length + 256, MaxContainerItems = 11 };
        var font = TrueTypeFont.Load(data, limits);
        Assert.Equal(data.Length + 256L, font.StorageBytes);
        var failure = Assert.Throws<ImageLimitException>(() => TrueTypeFont.Load(data, limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
        var lookupLimits = limits with { MaxWorkingBytes = font.StorageBytes };
        Assert.Equal(2, font.GetGlyphIndex(new Rune('A'), lookupLimits));
        failure = Assert.Throws<ImageLimitException>(() => font.GetGlyphMetrics(2, lookupLimits with { MaxWorkingBytes = font.StorageBytes - 1 }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
        Assert.Equal(600, font.GetGlyphMetrics(2).AdvanceWidth);
    }

    [Theory]
    [InlineData(ImageLimitKind.InputBytes)] [InlineData(ImageLimitKind.Work)] [InlineData(ImageLimitKind.ContainerItems)]
    public void FontLimitsStayStructuredWithoutChangingTheInput(ImageLimitKind kind)
    {
        var data = AuthoredTrueType.Build(); var saved = data.ToArray();
        var limits = kind switch
        {
            ImageLimitKind.InputBytes => new ImageLimits { MaxInputBytes = data.Length - 1 },
            ImageLimitKind.Work => new ImageLimits { MaxWork = data.Length },
            _ => new ImageLimits { MaxContainerItems = 1 }
        };
        var failure = Assert.Throws<ImageLimitException>(() => TrueTypeFont.Load(data, limits));
        Assert.Equal(kind, failure.Limit); Assert.Equal(ImageLimitScope.Operation, failure.Scope); Assert.Equal(saved, data);
        Assert.Equal(2, TrueTypeFont.Load(data).GetGlyphIndex(new Rune('A')));
    }

    [Fact]
    public void CancellationWorkAndInvalidArgumentsRemainExplicit()
    {
        var data = AuthoredTrueType.Build(); using var token = new CancellationTokenSource(); token.Cancel();
        var failure = Assert.ThrowsAny<OperationCanceledException>(() => TrueTypeFont.Load(data, cancellationToken: token.Token));
        Assert.Equal(token.Token, failure.CancellationToken);
        var font = TrueTypeFont.Load(data);
        failure = Assert.ThrowsAny<OperationCanceledException>(() => font.GetGlyphMetrics(new Rune('A'), cancellationToken: token.Token));
        Assert.Equal(token.Token, failure.CancellationToken);
        var quota = Assert.Throws<ImageLimitException>(() => font.GetGlyphIndex(new Rune('A'), new() { MaxWork = 1 }));
        Assert.Equal(ImageLimitKind.Work, quota.Limit);
        Assert.Throws<ArgumentOutOfRangeException>(() => font.GetGlyphMetrics(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => font.GetGlyphMetrics(font.GlyphCount));
        Assert.Throws<ArgumentOutOfRangeException>(() => TrueTypeFont.Load(data, new() { MaxWork = 0 }));
    }

    [Theory]
    [InlineData("container", UnsupportedImageFeature.FontContainer)]
    [InlineData("version", UnsupportedImageFeature.FontTable)]
    [InlineData("locations", UnsupportedImageFeature.FontTable)]
    [InlineData("mapping", UnsupportedImageFeature.FontMapping)]
    public void UnsupportedFontFeaturesHaveTypedCategories(string field, UnsupportedImageFeature feature)
    {
        var data = AuthoredTrueType.Build(unicode12: false);
        switch (field)
        {
            case "container": "OTTO"u8.CopyTo(data); break;
            case "version": AuthoredTrueType.W16(data, AuthoredTrueType.Offset(data, "head"), 2); break;
            case "locations": AuthoredTrueType.W16(data, AuthoredTrueType.Offset(data, "head") + 50, 2); break;
            default: AuthoredTrueType.W16(data, AuthoredTrueType.Offset(data, "cmap") + 12, 6); break;
        }
        var failure = Assert.Throws<UnsupportedImageException>(() => TrueTypeFont.Load(data));
        Assert.Equal(feature, failure.Feature); Assert.Null(failure.Format);
    }

    [Theory]
    [InlineData("directory")] [InlineData("duplicate")] [InlineData("table-offset")] [InlineData("table-length")]
    [InlineData("magic")] [InlineData("units")] [InlineData("glyph-count")] [InlineData("metrics")]
    [InlineData("loca-range")] [InlineData("loca-order")] [InlineData("glyph-bounds")]
    [InlineData("map-offset")] [InlineData("map-length")] [InlineData("map-glyph")] [InlineData("map-order")]
    public void MalformedRangesHeadersAndMappingsAreRejectedAsFontData(string field)
    {
        var data = AuthoredTrueType.Build(); var head = AuthoredTrueType.Offset(data, "head"); var loca = AuthoredTrueType.Offset(data, "loca");
        var cm = AuthoredTrueType.Offset(data, "cmap"); var twelve = cm + (int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(cm + 16));
        switch (field)
        {
            case "directory": AuthoredTrueType.W16(data, 4, ushort.MaxValue); break;
            case "duplicate": data.AsSpan(12, 4).CopyTo(data.AsSpan(28)); break;
            case "table-offset": AuthoredTrueType.W32(data, 20, uint.MaxValue); break;
            case "table-length": AuthoredTrueType.W32(data, 24, uint.MaxValue); break;
            case "magic": AuthoredTrueType.W32(data, head + 12, 0); break;
            case "units": AuthoredTrueType.W16(data, head + 18, 15); break;
            case "glyph-count": AuthoredTrueType.W16(data, AuthoredTrueType.Offset(data, "maxp") + 4, 0); break;
            case "metrics": AuthoredTrueType.W16(data, AuthoredTrueType.Offset(data, "hhea") + 34, 9); break;
            case "loca-range": AuthoredTrueType.W32(data, loca + 32, uint.MaxValue); break;
            case "loca-order": AuthoredTrueType.W32(data, loca + 8, 1); break;
            case "glyph-bounds": AuthoredTrueType.W16(data, AuthoredTrueType.Offset(data, "glyf") + 2, 30000); break;
            case "map-offset": AuthoredTrueType.W32(data, cm + 16, uint.MaxValue); break;
            case "map-length": AuthoredTrueType.W32(data, twelve + 4, uint.MaxValue); break;
            case "map-glyph": AuthoredTrueType.W32(data, twelve + 24, 8); break;
            default: AuthoredTrueType.W32(data, twelve + 28, 32); break;
        }
        Assert.Throws<ImageDataException>(() => TrueTypeFont.Load(data));
    }

    [Theory]
    [InlineData("odd-offset")] [InlineData("out-of-range")] [InlineData("into-header")]
    [InlineData("absent-glyph")] [InlineData("terminal")] [InlineData("overlap")]
    public void SegmentMappingAddressesAndDeltaResultsAreChecked(string field)
    {
        var data = AuthoredTrueType.Build(unicode12: false); var table = AuthoredTrueType.Offset(data, "cmap") + 12;
        const int segments = 9;
        var rangeOffsets = table + 16 + segments * 6;
        switch (field)
        {
            case "odd-offset": AuthoredTrueType.W16(data, rangeOffsets, 1); break;
            case "out-of-range": AuthoredTrueType.W16(data, rangeOffsets, 65534); break;
            case "into-header": AuthoredTrueType.W16(data, rangeOffsets, 2); break;
            case "absent-glyph": AuthoredTrueType.W16(data, table + 16 + segments * 8, 8); break;
            case "terminal": AuthoredTrueType.W16(data, table + 14 + (segments - 1) * 2, 65534); break;
            default: AuthoredTrueType.W16(data, table + 16 + segments * 2 + 2, 32); break;
        }
        Assert.Throws<ImageDataException>(() => TrueTypeFont.Load(data));
    }

    [Fact]
    public void EveryTruncationAndBoundedMutationKeepFailuresWithinPublicCategories()
    {
        var data = AuthoredTrueType.Build();
        for (var length = 0; length < data.Length; length++)
        {
            var prefix = data.AsSpan(0, length).ToArray();
            Assert.Throws<ImageDataException>(() => TrueTypeFont.Load(prefix));
        }
        uint state = 0x4C4B494D;
        for (var i = 0; i < 64; i++)
        {
            var altered = data.ToArray(); state = unchecked(state * 1664525 + 1013904223);
            altered[state % (uint)altered.Length] ^= (byte)(1 << (i & 7));
            try
            {
                var font = TrueTypeFont.Load(altered);
                _ = font.GetGlyphMetrics(new Rune('A')); _ = font.GetGlyphMetrics(new Rune(0x1F642));
            }
            catch (ImageDataException) { }
            catch (ImageLimitException) { }
            catch (UnsupportedImageException) { }
        }
    }
}
