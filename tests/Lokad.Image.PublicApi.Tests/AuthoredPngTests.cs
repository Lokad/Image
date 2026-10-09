using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

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
            using var decoded = ImageCodec.Decode(encoded);
            Assert.Equal((frame.Width, frame.Height, frame.Depth == 16 ? 16 : 8), (decoded.Width, decoded.Height, decoded.BitDepth));
            Assert.Equal(analytical, OriginalPixels.Bytes(decoded));
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
