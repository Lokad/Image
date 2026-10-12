using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

[CollectionDefinition("PNG storage ownership", DisableParallelization = true)]
public sealed class PngStorageOwnershipCollection { }

[Collection("PNG storage ownership")]
public sealed class AuthoredPngTests
{
    [Fact]
    public void EveryLegalColorDepthAndInterlaceProfileMatchesFrozenFfmpegAndAnalyticalSamples()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Lokad.Image.slnx"))) root = root.Parent;
        Assert.NotNull(root);
        var directory = Path.Combine(root.FullName, "tests", "Fixtures");
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, "png-authored-manifest.json")));
        Assert.Equal(2, manifest.RootElement.GetProperty("schemaVersion").GetInt32());
        var frames = AuthoredPng.Cases().ToDictionary(frame => "png-authored-" + frame.Name); Assert.Equal(72, frames.Count);
        Assert.Equal(frames.Count, manifest.RootElement.GetProperty("fixtures").GetArrayLength());
        var names = new HashSet<string>();
        foreach (var fixture in manifest.RootElement.GetProperty("fixtures").EnumerateArray())
        {
            var name = fixture.GetProperty("name").GetString()!; Assert.True(names.Add(name)); var frame = frames[name];
            var encoded = File.ReadAllBytes(Path.Combine(directory, name + ".png"));
            var expected = File.ReadAllBytes(Path.Combine(directory, name + ".rgba"));
            Assert.Equal(fixture.GetProperty("encodedHash").GetString(), Hash(encoded));
            Assert.Equal(fixture.GetProperty("expectedHash").GetString(), Hash(expected));
            Assert.Equal(fixture.GetProperty("inputHash").GetString(), Hash(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(frame))));
            Assert.Equal(frame, JsonSerializer.Deserialize<AuthoredPng.Frame>(fixture.GetProperty("syntax").GetRawText()));
            Assert.Equal(encoded, AuthoredPng.Build(frame)); var analytical = AuthoredPng.Expected(frame);
            Assert.Equal(fixture.GetProperty("analyticalHash").GetString(), Hash(analytical));
            Assert.Equal("exact-rgba-samples", fixture.GetProperty("comparison").GetString());
            Assert.Equal(expected, analytical);
            if (fixture.GetProperty("directExpectedHash").ValueKind != JsonValueKind.Null)
            {
                Assert.True(frame.ColorType == 0 && frame.Depth < 8 && frame.Transparency);
                var direct = File.ReadAllBytes(Path.Combine(directory, name + ".direct.rgba"));
                Assert.Equal(fixture.GetProperty("directExpectedHash").GetString(), Hash(direct)); Assert.Equal(expected.Length, direct.Length);
                Assert.Contains("-vf", fixture.GetProperty("decode").EnumerateArray().Select(value => value.GetString()));
                if (fixture.GetProperty("nativeDecoderIgnoresTransparency").GetBoolean())
                    for (var i = 0; i < expected.Length; i++) Assert.Equal(i % 4 == 3 ? (byte)255 : expected[i], direct[i]);
                else Assert.Equal(expected, direct);
            }
            // Force the next complete-overwrite lease to contain nonzero data,
            // including alpha and unused Adam7 destinations before reconstruction.
            using (var zero = ImageBuffer.Create(frame.Width, frame.Height, frame.Depth == 16 ? 16 : 8))
            using (var dirty = zero.Clone()) Fill(dirty, 0xa5);
            using var decoded = ImageCodec.Decode(encoded);
            Assert.Equal((frame.Width, frame.Height, frame.Depth == 16 ? 16 : 8), (decoded.Width, decoded.Height, decoded.BitDepth));
            Assert.Equal(analytical, OriginalPixels.Bytes(decoded));
            using var held = ImageCodec.Decode(encoded);
            Fill(decoded, 0x3c);
            Assert.Equal(analytical, OriginalPixels.Bytes(held));
            Assert.All(OriginalPixels.Bytes(decoded), value => Assert.Equal((byte)0x3c, value));
        }
    }

    [Fact]
    public void FailedPartialDecodeNeverPublishesDirtyStorageToTheNextOwner()
    {
        var frame = new AuthoredPng.Frame("failed-owned-output", 6, 8, 1, 2, false);
        byte[] filtered = [0, 1, 2, 3, 0, 5, 6, 7, 8, 255]; // Invalid second-row filter.
        var compressed = AuthoredDeflate.Fixed(filtered.Select(value => new AuthoredDeflate.Token(value)), filtered);
        using (var zero = ImageBuffer.Create(1, 2, 8))
        using (var dirty = zero.Clone()) Fill(dirty, 0xa5);
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, compressed)));
        var valid = AuthoredPng.Build(frame);
        using var first = ImageCodec.Decode(valid);
        using var second = ImageCodec.Decode(valid);
        Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(first));
        Fill(first, 0x3c);
        Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(second));
        Assert.All(OriginalPixels.Bytes(first), value => Assert.Equal((byte)0x3c, value));
    }

    private static void Fill(ImageBuffer image, byte value)
    {
        for (var y = 0; y < image.Height; y++)
            if (image.BitDepth == 8) image.GetRow8(y).Fill(value);
            else image.GetRow16(y).Fill((ushort)(value * 257));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void WidePaletteRowsRetainExactPixelsMemoryThresholdAndWorkFailure(bool interlace)
    {
        var frame = new AuthoredPng.Frame("wide-palette-budget", 3, 8, 513, 5, interlace);
        var compressed = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame));
        var encoded = AuthoredPng.Wrap(frame, compressed); var expected = AuthoredPng.Expected(frame);
        // Input, IDAT, full PLTE, half-length tRNS, result, inflater and both
        // maximum-width pass rows. Preserve the original exact memory threshold.
        var peak = encoded.Length + (long)compressed.Length + 768 + 128 + 513 * 5 * 4L + 256 * 1024 + 2 * 513;
        foreach (var extra in new[] { 0, 1024 })
        {
            using var image = ImageCodec.Decode(encoded, new DecodeOptions { Limits = ImageLimits.Default with { MaxWorkingBytes = peak + extra } });
            Assert.Equal(expected, OriginalPixels.Bytes(image));
        }
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, new DecodeOptions { Limits = ImageLimits.Default with { MaxWorkingBytes = peak - 1 } }));
        using var cancellation = new CancellationTokenSource();
        long low = 1, high = ImageLimits.Default.MaxWork;
        while (low < high)
        {
            var middle = low + (high - low) / 2;
            var options = new DecodeOptions { Limits = ImageLimits.Default with { MaxWorkingBytes = peak + 1024, MaxWork = middle } };
            try { using var image = ImageCodec.Decode(encoded, options, cancellation.Token); high = middle; }
            catch (ImageLimitException) { low = middle + 1; }
        }
        foreach (var work in new[] { low - 1, low })
        {
            var options = new DecodeOptions { Limits = ImageLimits.Default with { MaxWorkingBytes = peak + 1024, MaxWork = work } };
            var direct = Record.Exception(() => { using var image = ImageCodec.Decode(encoded, options); });
            var scalar = Record.Exception(() => { using var image = ImageCodec.Decode(encoded, options, cancellation.Token); });
            Assert.Equal(scalar?.GetType(), direct?.GetType()); Assert.Equal(scalar?.Message, direct?.Message);
        }
    }

    [Theory]
    [InlineData("vpAg", false)]
    [InlineData("vpbg", false)]
    [InlineData("Vpbg", true)]
    public void UnknownFutureChunksFollowTheirAncillaryBit(string chunk, bool critical)
    {
        var frame = new AuthoredPng.Frame("future", 0, 8, 1, 1, false);
        var encoded = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), (chunk, new byte[] { 1, 2, 3 }));
        if (critical) Assert.Throws<UnsupportedImageException>(() => ImageCodec.Decode(encoded));
        else { using var image = ImageCodec.Decode(encoded); Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(image)); }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(127)]
    public void UnusedFinalIdatBytesDoNotReplaceTheZlibChecksum(int trailing)
    {
        var frame = new AuthoredPng.Frame("trailing", 6, 16, 3, 2, true);
        var zlib = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame));
        var padding = Enumerable.Range(0, trailing).Select(i => (byte)(i * 43 + 9)).ToArray();
        using var image = ImageCodec.Decode(AuthoredPng.Wrap(frame, [.. zlib, .. padding]));
        Assert.Equal(AuthoredPng.Expected(frame), OriginalPixels.Bytes(image));
        var damaged = zlib.ToArray(); damaged[^1] ^= 1;
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, [.. damaged, .. zlib[^4..]])));
    }

    [Fact]
    public void MissingFinalBlocksAndTruncatedTrailersFailEvenWhenAllPixelsArePresent()
    {
        var frame = new AuthoredPng.Frame("incomplete", 0, 8, 1, 1, false);
        var zlib = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame));
        var missingFinal = zlib.ToArray(); missingFinal[2] = 0;
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, missingFinal)));
        for (var length = 0; length < zlib.Length; length++)
            Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, zlib[..length])));
        var badLength = zlib.ToArray(); badLength[5] ^= 1;
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, badLength)));
        var excess = AuthoredPng.StoredZlib([0, 17, 99]);
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, excess)));
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
