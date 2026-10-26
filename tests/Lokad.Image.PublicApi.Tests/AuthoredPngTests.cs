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

    [Fact]
    public void EqualLengthCompressedInputsKeepSamplesIndependentAcrossFailure()
    {
        var frame = new AuthoredPng.Frame("equal-idat-leases", 6, 8, 2, 2, false);
        byte[] firstPixels = [1, 2, 3, 0, 11, 12, 13, 255, 21, 22, 23, 255, 31, 32, 33, 128];
        byte[] secondPixels = [101, 102, 103, 255, 111, 112, 113, 0, 121, 122, 123, 64, 131, 132, 133, 255];
        var firstRows = Rows(firstPixels); var secondRows = Rows(secondPixels);
        var firstCompressed = AuthoredPng.StoredZlib(firstRows); var secondCompressed = AuthoredPng.StoredZlib(secondRows);
        Assert.Equal(firstCompressed.Length, secondCompressed.Length);
        var firstEncoded = AuthoredPng.Wrap(frame, firstCompressed); var secondEncoded = AuthoredPng.Wrap(frame, secondCompressed);
        var firstOriginal = firstEncoded.ToArray(); var secondOriginal = secondEncoded.ToArray();
        using var first = ImageCodec.Decode(firstEncoded);
        var badRows = secondRows.ToArray(); badRows[9] = 5;
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(badRows))));
        using var second = ImageCodec.Decode(secondEncoded);
        Assert.Equal(firstPixels, OriginalPixels.Bytes(first)); Assert.Equal(secondPixels, OriginalPixels.Bytes(second));
        Assert.Equal(firstOriginal, firstEncoded); Assert.Equal(secondOriginal, secondEncoded);

        static byte[] Rows(byte[] pixels)
        {
            var result = new byte[18];
            for (var y = 0; y < 2; y++) pixels.AsSpan(y * 8, 8).CopyTo(result.AsSpan(y * 9 + 1));
            return result;
        }
    }

    [Theory]
    [InlineData(-1, false)] [InlineData(-1, true)]
    [InlineData(0, false)] [InlineData(0, true)]
    [InlineData(4, false)] [InlineData(4, true)]
    [InlineData(8, false)] [InlineData(8, true)]
    [InlineData(32, false)] [InlineData(32, true)]
    public void InflaterReadAheadRetainsFilterAndChecksumFailureOrder(int badRow, bool badChecksum)
    {
        var frame = new AuthoredPng.Frame("inflater-failure-order", 6, 8, 513, 33, false);
        var rows = AuthoredPng.FilteredRows(frame);
        if (badRow >= 0) rows[badRow * (513 * 4 + 1)] = 5;
        var compressed = AuthoredPng.StoredZlib(rows);
        if (badChecksum) compressed[^1] ^= 1;
        var encoded = AuthoredPng.Wrap(frame, compressed);
        using var cancellation = new CancellationTokenSource();
        var direct = Record.Exception(() => { using var image = ImageCodec.Decode(encoded); });
        var original = Record.Exception(() => { using var image = ImageCodec.Decode(encoded, cancellationToken: cancellation.Token); });
        Assert.Equal(original?.GetType(), direct?.GetType()); Assert.Equal(original?.Message, direct?.Message);
        if (badRow >= 0) Assert.Equal("Invalid PNG row filter.", direct?.Message);
        else if (!badChecksum) Assert.Null(direct);
    }

    [Fact]
    public void InflaterBufferPreservesOriginalMemoryAndThresholds()
    {
        var frame = new AuthoredPng.Frame("inflater-admission", 6, 8, 513, 9, false);
        var compressed = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame));
        var encoded = AuthoredPng.Wrap(frame, compressed); var expected = AuthoredPng.Expected(frame);
        var peak = encoded.Length + (long)compressed.Length + 513 * 9 * 4L + 256 * 1024 + 513 * 8;
        foreach (var extra in new[] { 0, 8191, 8192 })
        {
            using var image = ImageCodec.Decode(encoded, new DecodeOptions { Limits = ImageLimits.Default with { MaxWorkingBytes = peak + extra } });
            Assert.Equal(expected, OriginalPixels.Bytes(image));
        }
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded, new DecodeOptions { Limits = ImageLimits.Default with { MaxWorkingBytes = peak - 1 } }));

    }

    [Theory]
    [InlineData(0)] [InlineData(16)]
    public void InflaterReadAheadRetainsPaletteAndChecksumFailureOrder(int badRow)
    {
        var frame = new AuthoredPng.Frame("inflater-palette-order", 3, 8, 513, 17, false);
        var rows = new byte[(513 + 1) * 17]; rows[badRow * (513 + 1) + 1] = 1;
        var compressed = AuthoredPng.StoredZlib(rows); compressed[^1] ^= 1;
        var originalEncoded = AuthoredPng.Wrap(frame, compressed);
        using var output = new MemoryStream(); output.Write(originalEncoded.AsSpan(0, 8));
        for (var offset = 8; offset < originalEncoded.Length;)
        {
            var length = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(originalEncoded.AsSpan(offset));
            var name = Encoding.ASCII.GetString(originalEncoded.AsSpan(offset + 4, 4));
            var payload = originalEncoded.AsSpan(offset + 8, length);
            AuthoredPng.Chunk(output, name, name == "PLTE" ? payload[..3] : name == "tRNS" ? payload[..1] : payload);
            offset += length + 12;
        }
        var encoded = output.ToArray(); using var cancellation = new CancellationTokenSource();
        var direct = Record.Exception(() => { using var image = ImageCodec.Decode(encoded); });
        var original = Record.Exception(() => { using var image = ImageCodec.Decode(encoded, cancellationToken: cancellation.Token); });
        Assert.NotNull(original); Assert.Equal(original.GetType(), direct?.GetType()); Assert.Equal(original.Message, direct?.Message);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void WidePaletteRowsRetainExactPixelsMemoryThresholdAndFailure(bool interlace)
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
