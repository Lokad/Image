using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8FixtureTests
{
    [Theory]
    [InlineData("vp8-manifest.json", 110)]
    [InlineData("vp8-authored-manifest.json", 54)]
    public void FrozenFfmpegYuvReconstructionAndAlphaAreExact(string manifestName, int count)
    {
        var directory = FindFixtures();
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, manifestName)));
        var fixtures = manifest.RootElement.GetProperty("fixtures"); Assert.Equal(count, fixtures.GetArrayLength());
        foreach (var fixture in fixtures.EnumerateArray())
        {
            var name = fixture.GetProperty("name").GetString()!;
            var encoded = File.ReadAllBytes(Path.Combine(directory, name + ".webp")); var expected = File.ReadAllBytes(Path.Combine(directory, name + ".yuv"));
            Assert.Equal(fixture.GetProperty("encodedHash").GetString(), Hash(encoded));
            Assert.Equal(fixture.GetProperty("yuvHash").GetString(), Hash(expected));
            ReadOnlySpan<byte> pixels = default, alphaPayload = default;
            for (var offset = 12; offset < encoded.Length;)
            {
                var length = BinaryPrimitives.ReadInt32LittleEndian(encoded.AsSpan(offset + 4));
                if (encoded.AsSpan(offset, 4).SequenceEqual("VP8 "u8)) pixels = encoded.AsSpan(offset + 8, length);
                if (encoded.AsSpan(offset, 4).SequenceEqual("ALPH"u8)) alphaPayload = encoded.AsSpan(offset + 8, length);
                offset += 8 + length + (length & 1);
            }
            var context = new OperationContext(ImageLimits.Default, default);
            var actual = new Vp8Decoder(pixels, context).DecodePlanes(); var position = 0;
            foreach (var (plane, width, height, stride) in new[] { (actual.Y,actual.Width,actual.Height,actual.Stride),
                (actual.U,(actual.Width+1)/2,(actual.Height+1)/2,actual.Stride/2), (actual.V,(actual.Width+1)/2,(actual.Height+1)/2,actual.Stride/2) })
                for (var y = 0; y < height; y++)
                {
                    Assert.Equal(expected.AsSpan(position, width).ToArray(), plane.AsSpan(y * stride, width).ToArray()); position += width;
                }
            if (!alphaPayload.IsEmpty)
            {
                var alpha = WebpAlpha.Decode(alphaPayload, actual.Width, actual.Height, context);
                Assert.Equal(expected.AsSpan(position).ToArray(), alpha); position += alpha.Length;
            }
            Assert.Equal(expected.Length, position);
        }
    }

    private static string FindFixtures()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "tests", "Fixtures");
            if (File.Exists(Path.Combine(path, "vp8-manifest.json"))) return path;
        }
        throw new InvalidOperationException("Original VP8 fixture corpus not found.");
    }
    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
