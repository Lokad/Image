using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class FontHeaderValidationTests
{
    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void MixedHeadersAndTailPreserveOwnedSamples(bool longLocations, bool cancelable)
    {
        var input = Headers(longLocations); var original = input.ToArray();
        using var source = new CancellationTokenSource();
        var token = cancelable ? source.Token : default;
        using var root = new TestMemoryBudget(input.Length + 256L);
        var limits = new ImageLimits { MemoryBudget = root };
        using var font = TrueTypeFont.Load(input, limits, token);
        Assert.Equal(9, font.GlyphCount);
        Assert.Equal(font.StorageBytes, root.Bytes);
        input.AsSpan().Fill(0xa5);
        var metric = font.GetGlyphMetrics(8);
        Assert.Equal(short.MinValue, metric.XMin); Assert.Equal(short.MinValue, metric.YMin);
        Assert.Equal(short.MaxValue, metric.XMax); Assert.Equal(short.MaxValue, metric.YMax);
        Assert.True(metric.HasOutline);
        var refused = Assert.Throws<ImageLimitException>(() => TrueTypeFont.Load(original, limits, token));
        Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope);
        Assert.Equal(font.StorageBytes, root.Bytes);
        font.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using (var subsequent = TrueTypeFont.Load(original, limits, token)) Assert.Equal(9, subsequent.GlyphCount);
        Assert.Equal(0, root.Bytes);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void EarlierBoundsFailureWinsOverLaterLocationFailure(bool longLocations, bool cancelable)
    {
        var clean = Headers(longLocations); var input = clean.ToArray();
        using var source = new CancellationTokenSource();
        var token = cancelable ? source.Token : default;
        using var root = new TestMemoryBudget(input.Length + 256L);
        var limits = new ImageLimits { MemoryBudget = root };
        var glyph = Table(input, "glyf"u8);
        BinaryPrimitives.WriteInt16BigEndian(input.AsSpan(glyph + 2), short.MaxValue);
        BinaryPrimitives.WriteInt16BigEndian(input.AsSpan(glyph + 6), short.MinValue);
        var earlier = Assert.Throws<ImageDataException>(() => TrueTypeFont.Load(input, limits, token));
        Assert.Equal(0, root.Bytes);
        var locations = Table(input, "loca"u8);
        if (longLocations) BinaryPrimitives.WriteUInt32BigEndian(input.AsSpan(locations + 8), uint.MaxValue);
        else BinaryPrimitives.WriteUInt16BigEndian(input.AsSpan(locations + 4), ushort.MaxValue);
        var both = Assert.Throws<ImageDataException>(() => TrueTypeFont.Load(input, limits, token));
        Assert.Equal(earlier.Message, both.Message); Assert.Equal(0, root.Bytes);
        using (var subsequent = TrueTypeFont.Load(clean, limits, token)) Assert.Equal(9, subsequent.GlyphCount);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void TruncatedFinalHeaderFailsAndReleasesItsOwner(bool longLocations, bool cancelable)
    {
        var clean = Headers(longLocations); var input = clean.ToArray();
        using var source = new CancellationTokenSource();
        var token = cancelable ? source.Token : default;
        using var root = new TestMemoryBudget(input.Length + 256L);
        var limits = new ImageLimits { MemoryBudget = root };
        var locations = Table(input, "loca"u8);
        if (longLocations)
        {
            var start = BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(locations + 8 * 4));
            BinaryPrimitives.WriteUInt32BigEndian(input.AsSpan(locations + 9 * 4), start + 8);
        }
        else
        {
            var start = BinaryPrimitives.ReadUInt16BigEndian(input.AsSpan(locations + 8 * 2));
            BinaryPrimitives.WriteUInt16BigEndian(input.AsSpan(locations + 9 * 2), (ushort)(start + 4));
        }
        Assert.Throws<ImageDataException>(() => TrueTypeFont.Load(input, limits, token));
        Assert.Equal(0, root.Bytes);
        using (var subsequent = TrueTypeFont.Load(clean, limits, token)) Assert.Equal(9, subsequent.GlyphCount);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)] [InlineData(9)] [InlineData(15)] [InlineData(16)]
    [InlineData(17)] [InlineData(31)] [InlineData(32)] [InlineData(33)]
    public void LongLocationBatchBoundariesKeepEmptyAndZeroContourHeaders(int count)
    {
        var input = Headers(true, count);
        using var root = new TestMemoryBudget(input.Length + 256L);
        var limits = new ImageLimits { MemoryBudget = root };
        using var cancellation = new CancellationTokenSource();
        foreach (var token in new[] { CancellationToken.None, cancellation.Token })
        {
            using (var font = TrueTypeFont.Load(input, limits, token))
            {
                Assert.Equal(count, font.GlyphCount);
                for (var i = 0; i < count; i++)
                {
                    var metrics = font.GetGlyphMetrics(i);
                    Assert.Equal(i % 4 is not (1 or 2), metrics.HasOutline);
                    if (!metrics.HasOutline) continue;
                    Assert.Equal(short.MinValue, metrics.XMin);
                    Assert.Equal(short.MinValue, metrics.YMin);
                    Assert.Equal(short.MaxValue, metrics.XMax);
                    Assert.Equal(short.MaxValue, metrics.YMax);
                }
                Assert.Equal(font.StorageBytes, root.Bytes);
            }
            Assert.Equal(0, root.Bytes);
        }
        Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(17, 0)] [InlineData(17, 7)] [InlineData(17, 8)]
    [InlineData(33, 15)] [InlineData(33, 16)] [InlineData(33, 31)]
    public void EarlierBoundsFailureWinsAcrossLongLocationBatches(int count, int badGlyph)
    {
        var clean = Headers(true, count);
        using var root = new TestMemoryBudget(clean.Length + 256L);
        var limits = new ImageLimits { MemoryBudget = root };
        using var cancellation = new CancellationTokenSource();
        foreach (var token in new[] { CancellationToken.None, cancellation.Token })
        {
            var input = clean.ToArray();
            var locations = Table(input, "loca"u8);
            var start = BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(locations + badGlyph * 4));
            var header = Table(input, "glyf"u8) + checked((int)start);
            BinaryPrimitives.WriteInt16BigEndian(input.AsSpan(header + 2), short.MaxValue);
            BinaryPrimitives.WriteInt16BigEndian(input.AsSpan(header + 6), short.MinValue);
            var earlier = Assert.Throws<ImageDataException>(() => TrueTypeFont.Load(input, limits, token));
            Assert.Equal(0, root.Bytes);
            foreach (var later in new[] { 0x80000000u, uint.MaxValue })
            {
                BinaryPrimitives.WriteUInt32BigEndian(input.AsSpan(locations + (badGlyph + 2) * 4), later);
                var both = Assert.Throws<ImageDataException>(() => TrueTypeFont.Load(input, limits, token));
                Assert.Equal(earlier.Message, both.Message);
                Assert.Equal(0, root.Bytes);
            }
            using (var recovered = TrueTypeFont.Load(clean, limits, token)) Assert.Equal(count, recovered.GlyphCount);
            Assert.Equal(0, root.Bytes);
        }
        Assert.False(root.IsDisposed);
    }

    private static byte[] Headers(bool longLocations, int count = 9)
    {
        // Original header-only records: loading validates their eager headers,
        // not the deferred outline bodies. Include empty/zero-contour records,
        // signed extremes, two four-header groups and a final scalar tail.
        var glyphs = new Dictionary<int, byte[]>();
        for (var i = 0; i < count; i++)
        {
            if (i % 4 == 2) { glyphs.Add(i, []); continue; }
            var header = new byte[12];
            BinaryPrimitives.WriteInt16BigEndian(header, i % 4 == 1 ? (short)0 : (short)1);
            BinaryPrimitives.WriteInt16BigEndian(header.AsSpan(2), i % 4 == 1 ? short.MaxValue : short.MinValue);
            BinaryPrimitives.WriteInt16BigEndian(header.AsSpan(4), i % 4 == 1 ? short.MaxValue : short.MinValue);
            BinaryPrimitives.WriteInt16BigEndian(header.AsSpan(6), i % 4 == 1 ? short.MinValue : short.MaxValue);
            BinaryPrimitives.WriteInt16BigEndian(header.AsSpan(8), i % 4 == 1 ? short.MinValue : short.MaxValue);
            glyphs.Add(i, header);
        }
        return AuthoredTrueType.Build(longLocations: longLocations, glyphOverrides: glyphs);
    }

    private static int Table(ReadOnlySpan<byte> font, ReadOnlySpan<byte> tag)
    {
        var count = BinaryPrimitives.ReadUInt16BigEndian(font[4..]);
        for (var i = 0; i < count; i++)
        {
            var entry = font.Slice(12 + i * 16, 16);
            if (entry[..4].SequenceEqual(tag)) return checked((int)BinaryPrimitives.ReadUInt32BigEndian(entry[8..]));
        }
        throw new InvalidOperationException("Authored table missing.");
    }
}
