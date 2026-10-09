using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngFramingTests
{
    [Fact]
    public void EveryFixedLiteralPrefixProducesItsAnalyticalSample()
    {
        // The leading zero is the filter byte; samples 1..255 exercise every
        // fixed literal category, including the full nine-bit range 144..255.
        var raw = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();
        var frame = new AuthoredPng.Frame("fixed-literals", 0, 8, 255, 1, false);
        var compressed = AuthoredDeflate.Fixed(raw.Select(value => new AuthoredDeflate.Token(value)), raw);
        using var image = ImageCodec.Decode(AuthoredPng.Wrap(frame, compressed));
        for (var x = 0; x < 255; x++)
        {
            var sample = (byte)(x + 1);
            Assert.Equal(new byte[] { sample, sample, sample, sample == 17 ? (byte)0 : (byte)255 }, image.GetRow8(0).Slice(x * 4, 4).ToArray());
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AuthoredFixedAndDynamicPrefixPathsHaveAnalyticalPixels(bool dynamic)
    {
        var frame = new AuthoredPng.Frame("prefix", 0, 8, 1, 1, false); byte[] raw = [0, 17];
        var compressed = dynamic ? AuthoredDeflate.DynamicLiteral(raw) : AuthoredDeflate.Fixed(raw.Select(value => new AuthoredDeflate.Token(value)), raw);
        using var image = ImageCodec.Decode(AuthoredPng.Wrap(frame, compressed));
        Assert.Equal(new byte[] { 17, 17, 17, 0 }, image.GetRow8(0).ToArray());
        var missingFinal = compressed.ToArray(); missingFinal[2] &= 0xFE;
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, missingFinal)));
        for (var length = 0; length < compressed.Length; length++)
            Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, compressed[..length])));
    }

    [Fact]
    public void FixedOverlappingMatchReconstructsAcrossRows()
    {
        var frame = new AuthoredPng.Frame("overlap", 0, 8, 1, 130, false);
        var raw = Enumerable.Range(0, 260).Select(i => i % 2 == 0 ? (byte)0 : (byte)17).ToArray();
        var compressed = AuthoredDeflate.Fixed([new(0), new(17), new(285, DistanceSymbol: 1)], raw);
        using var image = ImageCodec.Decode(AuthoredPng.Wrap(frame, compressed));
        for (var y = 0; y < 130; y++) Assert.Equal(new byte[] { 17, 17, 17, 0 }, image.GetRow8(y).ToArray());
    }

    [Theory]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(19)]
    [InlineData(15)]
    public void DynamicRepeatsCrossAlphabetAndMaximumDepthRemainValid(int profile)
    {
        byte[] raw = [0, 17]; var frame = new AuthoredPng.Frame("tree", 0, 8, 1, 1, false);
        var compressed = profile == 19 ? AuthoredDeflate.DynamicCrossAlphabet(raw) : profile == 15 ? AuthoredDeflate.DynamicDeep(raw) : AuthoredDeflate.DynamicLiteral(raw, repeat: profile);
        using var image = ImageCodec.Decode(AuthoredPng.Wrap(frame, compressed));
        Assert.Equal(new byte[] { 17, 17, 17, 0 }, image.GetRow8(0).ToArray());
    }

    [Theory]
    [InlineData(257, 0, 0, 3)]
    [InlineData(264, 0, 0, 10)]
    [InlineData(265, 1, 1, 12)]
    [InlineData(268, 1, 1, 18)]
    [InlineData(269, 3, 2, 22)]
    [InlineData(284, 30, 5, 257)]
    [InlineData(285, 0, 0, 258)]
    public void LengthCategoryEndpointsHaveAnalyticalCopies(int code, int extra, int bits, int length)
    {
        var raw = Enumerable.Range(0, length + 2).Select(i => i % 2 == 0 ? (byte)0 : (byte)17).ToArray();
        var frame = new AuthoredPng.Frame("length", 0, 8, raw.Length - 1, 1, false);
        var compressed = AuthoredDeflate.Fixed([new(0), new(17), new(code, extra, bits, 1)], raw);
        using var image = ImageCodec.Decode(AuthoredPng.Wrap(frame, compressed));
        for (var x = 0; x < image.Width; x++) Assert.Equal(new byte[] { raw[x + 1], raw[x + 1], raw[x + 1], raw[x + 1] == 17 ? (byte)0 : (byte)255 }, image.GetRow8(0).Slice(x * 4, 4).ToArray());
    }

    [Fact]
    public void MaximumDistanceUsesHistoryFromThePreviousRow()
    {
        var frame = new AuthoredPng.Frame("distance", 0, 8, 16512, 2, false);
        var raw = new byte[33026];
        var tokens = Enumerable.Repeat(new AuthoredDeflate.Token(0), 32768).Append(new(285, DistanceSymbol: 29, DistanceExtra: 8191, DistanceBits: 13));
        using var image = ImageCodec.Decode(AuthoredPng.Wrap(frame, AuthoredDeflate.Fixed(tokens, raw)));
        for (var y = 0; y < 2; y++) for (var x = 0; x < 16512; x++) Assert.Equal(new byte[] { 0, 0, 0, 255 }, image.GetRow8(y).Slice(x * 4, 4).ToArray());
    }

    [Fact]
    public void DynamicRepeatAndAlphabetFailuresRemainTyped()
    {
        var frame = new AuthoredPng.Frame("invalid", 0, 8, 1, 1, false);
        var invalid = AuthoredDeflate.InvalidDynamic().ToArray(); Assert.Equal(6, invalid.Length);
        foreach (var (_, compressed) in invalid) Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, compressed)));
    }

    [Fact]
    public void ReservedCodesInvalidHistoryAndDeclaredWindowsFail()
    {
        var frame = new AuthoredPng.Frame("reserved", 0, 8, 4, 1, false); byte[] raw = [0, 17, 0, 17, 0];
        foreach (var token in new[] { new AuthoredDeflate.Token(286), new(287), new(257, DistanceSymbol: 30), new(257, DistanceSymbol: 31), new(257, DistanceSymbol: 3) })
            Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, AuthoredDeflate.Fixed([new(0), new(17), token], raw))));
        // 257-byte match distance with 300 bytes of history, but a 256-byte zlib window.
        var prefix = new byte[300]; var expanded = new byte[558];
        var tokens = prefix.Select(value => new AuthoredDeflate.Token(value)).Append(new(285, DistanceSymbol: 16, DistanceExtra: 0, DistanceBits: 7));
        var limitedWindow = AuthoredDeflate.Fixed(tokens, expanded, window: 0);
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame with { Width = 557 }, limitedWindow)));
    }

    [Fact]
    public void DecodeWorkingBuffersAreReleasedBeforeExifOrientation()
    {
        var frame = new AuthoredPng.Frame("working", 6, 16, 257, 129, false);
        var compressed = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame));
        byte[] exif = [73, 73, 42, 0, 8, 0, 0, 0, 1, 0, 18, 1, 3, 0, 1, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0];
        var encoded = AuthoredPng.Wrap(frame, compressed, ("eXIf", exif));
        // Actual simultaneously admitted buffers: input, concatenated IDAT, output,
        // BCL workspace, and the current/previous eight-byte-per-pixel scanlines.
        var peak = encoded.Length + (long)compressed.Length + frame.Width * frame.Height * 8L + 256 * 1024 + frame.Width * 16;
        var limits = new ImageLimits { MaxWorkingBytes = peak };
        using var plain = ImageCodec.Decode(encoded, new DecodeOptions { Limits = limits });
        Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(plain));
        using var oriented = ImageCodec.Decode(encoded, new DecodeOptions { ApplyOrientation = true, Limits = limits });
        using var expected = ImageTransforms.Rotate(plain, QuarterTurn.Clockwise90);
        Assert.Equal((129, 257, ImageOrientation.Normal), (oriented.Width, oriented.Height, oriented.Orientation));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(oriented));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, new DecodeOptions { Limits = limits with { MaxWorkingBytes = peak - 1 } }));
    }

    [Fact]
    public void ComputationalChunkAndMetadataLimitsAreEnforced()
    {
        var frame = new AuthoredPng.Frame("limits", 6, 16, 19, 17, true);
        var encoded = AuthoredPng.Build(frame);
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, new DecodeOptions { Limits = new ImageLimits { MaxWork = encoded.Length } }));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, new DecodeOptions { Limits = new ImageLimits { MaxContainerItems = 3 } }));
        var withMetadata = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), ("vpAg", new byte[3]), ("vpBg", new byte[3]));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(withMetadata, new DecodeOptions { Limits = new ImageLimits { MaxMetadataBytes = 5 } }));
    }
}
