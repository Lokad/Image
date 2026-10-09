using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lokad.Image.Tooling;

internal static class AuthoredPngFixtures
{
    public static void Generate(string output, string executable, string executableHash, string version)
    {
        var fixtures = new List<object>();
        foreach (var frame in AuthoredPng.Cases())
        {
            var name = "png-authored-" + frame.Name; var encoded = AuthoredPng.Build(frame);
            var depth = frame.Depth == 16 ? 16 : 8; var decode = PngOracle.DecodeArguments(depth, encoded);
            var expected = OracleProcess.Run(executable, decode, encoded);
            var analytical = AuthoredPng.Expected(frame);
            if (!expected.SequenceEqual(analytical)) throw new InvalidOperationException($"PNG reference differs: {frame.Name}.");
            string[]? directDecode = null; string? directExpectedHash = null; bool? nativeDecoderIgnoresTransparency = null;
            if (PngOracle.PackedGrayKey(encoded).HasValue)
            {
                directDecode = PngOracle.DecodeArguments(depth); var direct = OracleProcess.Run(executable, directDecode, encoded);
                nativeDecoderIgnoresTransparency = !direct.SequenceEqual(expected);
                if (direct.Length != expected.Length) throw new InvalidOperationException("PNG direct raster differs.");
                if (nativeDecoderIgnoresTransparency.Value)
                    for (var i = 0; i < direct.Length; i++)
                        if (i % 4 == 3 ? direct[i] != 255 : direct[i] != expected[i]) throw new InvalidOperationException("PNG direct decoder differs beyond its declared alpha limitation.");
                directExpectedHash = Hash(direct); File.WriteAllBytes(Path.Combine(output, name + ".direct.rgba"), direct);
            }
            File.WriteAllBytes(Path.Combine(output, name + ".png"), encoded);
            File.WriteAllBytes(Path.Combine(output, name + ".rgba"), expected);
            fixtures.Add(new
            {
                name, width = frame.Width, height = frame.Height, depth, syntax = frame,
                recipe = "authored-png-v1", authoring = "AuthoredPng.Build",
                inputHash = Hash(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(frame))), decode,
                encodedHash = Hash(encoded), expectedHash = Hash(expected), analyticalHash = Hash(analytical), comparison = "exact-rgba-samples",
                directDecode, directExpectedHash, nativeDecoderIgnoresTransparency
            });
        }
        File.WriteAllText(Path.Combine(output, "png-authored-manifest.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 2, platform = RuntimeInformation.RuntimeIdentifier,
            ffmpegSha256 = executableHash, ffmpegVersion = version.Replace("\r\n", "\n"), fixtures
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"Generated {fixtures.Count} authored PNG cases with exact native RGBA; direct packed-gray alpha evidence is retained.");
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
