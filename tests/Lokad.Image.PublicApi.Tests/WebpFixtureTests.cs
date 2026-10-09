using System.Security.Cryptography;
using System.Text.Json;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpFixtureTests
{
    [Fact]
    public void FrozenLosslessFfmpegExpectationsCoverTheVp8lBranches()
    {
        var directory = FindFixtures();
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, "webp-manifest.json")));
        var fixtures = manifest.RootElement.GetProperty("fixtures"); Assert.Equal(60, fixtures.GetArrayLength());
        foreach (var fixture in fixtures.EnumerateArray())
        {
            var name = fixture.GetProperty("name").GetString()!;
            var encoded = File.ReadAllBytes(Path.Combine(directory, name + ".webp"));
            var expected = File.ReadAllBytes(Path.Combine(directory, name + ".rgba"));
            Assert.Equal(fixture.GetProperty("encodedHash").GetString(), Hash(encoded));
            Assert.Equal(fixture.GetProperty("expectedHash").GetString(), Hash(expected));
            using var actual = ImageCodec.Decode(encoded);
            Assert.Equal((fixture.GetProperty("width").GetInt32(), fixture.GetProperty("height").GetInt32(), 8),
                (actual.Width, actual.Height, actual.BitDepth));
            Assert.Equal(actual.Width * actual.Height * 4, expected.Length);
            for (var y = 0; y < actual.Height; y++)
                Assert.Equal(expected.AsSpan(y * actual.Width * 4, actual.Width * 4).ToArray(), actual.GetRow8(y).ToArray());
        }
    }

    private static string FindFixtures()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "tests", "Fixtures");
            if (File.Exists(Path.Combine(path, "webp-manifest.json"))) return path;
        }
        throw new InvalidOperationException("Original WebP fixture corpus not found.");
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
