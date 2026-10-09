using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lokad.Image.Tooling;

internal static class AuthoredVp8Fixtures
{
    public static void Generate(string output, string executable, string executableHash, string version)
    {
        var fixtures = new List<object>();
        foreach (var frame in AuthoredVp8.Cases())
        {
            var name = "vp8-authored-" + frame.Name; var encoded = AuthoredVp8.Build(frame);
            var decode = WebpOracle.DecodeArguments(frame.Width, frame.Height, false); var yuvDecode = WebpOracle.YuvArguments(false);
            var expected = OracleProcess.Run(executable, decode, encoded); var yuv = OracleProcess.Run(executable, yuvDecode, encoded);
            if (expected.Length != frame.Width * frame.Height * 4 || yuv.Length != frame.Width * frame.Height + 2 * ((frame.Width + 1) / 2) * ((frame.Height + 1) / 2))
                throw new InvalidOperationException("Authored VP8 oracle raster differs.");
            File.WriteAllBytes(Path.Combine(output, name + ".webp"), encoded);
            File.WriteAllBytes(Path.Combine(output, name + ".rgba"), expected); File.WriteAllBytes(Path.Combine(output, name + ".yuv"), yuv);
            fixtures.Add(new
            {
                name, width = frame.Width, height = frame.Height, alpha = false,
                authoring = "AuthoredVp8.Build", recipe = "authored-vp8-v1", syntax = frame,
                inputHash = Hash(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(frame))), decode, yuvDecode,
                encodedHash = Hash(encoded), expectedHash = Hash(expected), yuvHash = Hash(yuv),
                comparison = "exact-yuv; rgb-max4-rmse1.5-after-explicit-conversion"
            });
        }
        File.WriteAllText(Path.Combine(output, "vp8-authored-manifest.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, platform = RuntimeInformation.RuntimeIdentifier, ffmpegSha256 = executableHash,
            ffmpegVersion = version.Replace("\r\n", "\n"), fixtures
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"Generated {fixtures.Count} original authored VP8 syntax cases.");
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
