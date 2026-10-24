using System.Security.Cryptography;
using System.Text.Json;

namespace Lokad.Image.Tooling;

internal sealed record OriginalDirectRgbControl(byte[] Encoded, string RgbaSha256, string RgbSha256, string GraySha256);

internal static class OriginalDirectRgbControls
{
    internal static OriginalDirectRgbControl Load(int sampling, string? fixturesDirectory = null)
    {
        if (fixturesDirectory is null)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "tests", "Fixtures", "jpeg-direct-rgb-manifest.json")))
                directory = directory.Parent;
            if (directory is null) throw new InvalidOperationException("Original direct-RGB controls not found.");
            fixturesDirectory = Path.Combine(directory.FullName, "tests", "Fixtures");
        }
        using var doc = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(fixturesDirectory, "jpeg-direct-rgb-manifest.json")));
        var fixture = doc.RootElement.GetProperty("fixtures").EnumerateArray()
            .Single(item => item.GetProperty("sampling").GetString() == sampling.ToString());
        var data = File.ReadAllBytes(Path.Combine(fixturesDirectory, fixture.GetProperty("name").GetString()!));
        if (Hash(data) != fixture.GetProperty("encodedSha256").GetString())
            throw new InvalidOperationException("Original direct-RGB packet provenance differs.");
        return new(data, fixture.GetProperty("rgbaSha256").GetString()!, fixture.GetProperty("rgbSha256").GetString()!,
            fixture.GetProperty("graySha256").GetString()!);
    }

    internal static byte[] Center422(ReadOnlySpan<byte> rgba)
    {
        const int width = 513, height = 259;
        if (rgba.Length != width * height * 4) throw new ArgumentException("Expected the original513x259 control.");
        var result = rgba.ToArray();
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var k = x / 2; var odd = (x & 1) != 0;
            var first = Math.Clamp(odd ? k : k - 1, 0, width / 2);
            var second = Math.Clamp(odd ? k + 1 : k, 0, width / 2);
            for (var c = 1; c <= 2; c++)
                result[(y * width + x) * 4 + c] = (byte)(((odd ? 3 : 1) * rgba[(y * width + first * 2) * 4 + c]
                    + (odd ? 1 : 3) * rgba[(y * width + second * 2) * 4 + c] + 2) / 4);
        }
        return result;
    }

    internal static string Hash(ReadOnlySpan<byte> data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
