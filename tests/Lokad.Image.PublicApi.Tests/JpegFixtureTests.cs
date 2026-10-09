using System.Security.Cryptography;
using System.Text.Json;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegFixtureTests
{
    [Fact]
    public void FrozenFfmpegJpegExpectationsCoverBaselineAndProgressiveProfiles()
    {
        var directory = FindFixtures();
        using var manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, "jpeg-manifest.json")));
        var fixtures = manifest.RootElement.GetProperty("fixtures"); Assert.Equal(30, fixtures.GetArrayLength());
        foreach (var fixture in fixtures.EnumerateArray())
        {
            var name = fixture.GetProperty("name").GetString()!;
            var encoded = File.ReadAllBytes(Path.Combine(directory, name + ".jpg"));
            var expected = File.ReadAllBytes(Path.Combine(directory, name + ".rgba"));
            Assert.Equal(fixture.GetProperty("encodedHash").GetString(), Hash(encoded));
            Assert.Equal(fixture.GetProperty("expectedHash").GetString(), Hash(expected));
            using var actual = ImageCodec.Decode(encoded);
            Assert.Equal((fixture.GetProperty("width").GetInt32(), fixture.GetProperty("height").GetInt32(), 8),
                (actual.Width, actual.Height, actual.BitDepth));
            Assert.Equal(actual.Width * actual.Height * 4, expected.Length);
            var maximum = 0; double squared = 0;
            for (var y = 0; y < actual.Height; y++)
            {
                var row = actual.GetRow8(y);
                for (var x = 0; x < row.Length; x++)
                {
                    if (x % 4 == 3) { Assert.Equal(255, row[x]); Assert.Equal(255, expected[y * row.Length + x]); continue; }
                    var error = Math.Abs(row[x] - expected[y * row.Length + x]);
                    maximum = Math.Max(maximum, error); squared += error * error;
                }
            }
            var rmse = Math.Sqrt(squared / (actual.Width * actual.Height * 3));
            Assert.True(maximum <= 4 && rmse <= 1.5, $"{name}: maximum {maximum}, RMSE {rmse:F4}");
        }
    }

    private static string FindFixtures()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, "tests", "Fixtures");
            if (File.Exists(Path.Combine(path, "jpeg-manifest.json"))) return path;
        }
        throw new InvalidOperationException("Original JPEG fixture corpus not found.");
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
