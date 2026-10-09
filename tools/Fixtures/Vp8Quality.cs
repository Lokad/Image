using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal static class Vp8Quality
{
    public static void Generate(string output, string executable, string executableHash, string version)
    {
        const int width = 129, height = 67;
        var entries = new List<object>();
        foreach (var pattern in new[] { "gray-ramp", "color-tiles", "soft-gradient", "asymmetric-v1" })
        {
            var pixels = Pixels(pattern, width, height);
            using var image = ImageBuffer.Create(width, height, 8);
            for (var y = 0; y < height; y++) pixels.AsSpan(y * width * 4, width * 4).CopyTo(image.GetRow8(y));
            foreach (var quality in new[] { 1, 50, 85, 100 })
                foreach (var effort in new[] { 0, 4, 6 })
                    foreach (var subject in new[] { "Lokad.Image", "FFmpeg-libwebp" })
                    {
                        byte[] encoded; string[]? encode = null;
                        if (subject == "Lokad.Image")
                        {
                            using var stream = new MemoryStream();
                            ImageCodec.EncodeWebp(image, stream, new WebpEncodeOptions { Lossless = false, Quality = quality, Effort = effort });
                            encoded = stream.ToArray();
                        }
                        else { encode = WebpOracle.EncodeArguments(width, height, effort, quality); encoded = OracleProcess.Run(executable, encode, pixels); }
                        var info = ImageCodec.Identify(encoded);
                        if (info.Width != width || info.Height != height || info.HasAlpha) throw new InvalidOperationException("Quality corpus geometry/opacity differs.");
                        var decode = WebpOracle.DecodeArguments(width, height, false);
                        var actual = OracleProcess.Run(executable, decode, encoded);
                        if (actual.Length != pixels.Length) throw new InvalidOperationException("Quality oracle raster differs.");
                        double squares = 0; var maximum = 0;
                        for (var i = 0; i < pixels.Length; i++)
                        {
                            if (i % 4 == 3) { if (actual[i] != 255) throw new InvalidOperationException("Quality corpus alpha differs."); continue; }
                            var error = Math.Abs(pixels[i] - actual[i]); squares += error * error; maximum = Math.Max(maximum, error);
                        }
                        var rmse = Math.Sqrt(squares / (width * height * 3));
                        entries.Add(new
                        {
                            subject, pattern, width, height, quality, effort,
                            inputHash = Hash(pixels), encodedBytes = encoded.Length, encodedHash = Hash(encoded),
                            rgbMaximum = maximum, rgbRmse = rmse, rgbPsnr = rmse == 0 ? (double?)null : 20 * Math.Log10(255 / rmse),
                            encode, decode
                        });
                    }
        }
        File.WriteAllText(Path.Combine(output, "vp8-quality.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, platform = RuntimeInformation.RuntimeIdentifier, ffmpegSha256 = executableHash,
            ffmpegVersion = version.Replace("\r\n", "\n"),
            method = "Opaque original RGBA8; FFmpeg independently decodes both subjects with explicit limited-range BT.601 and centered bilinear expansion, saturating through RGBA16 before nondithered RGBA8 reduction. Whole-raster RGB RMSE/PSNR versus source. Numeric quality/effort values are subject-specific, not equivalent quality or speed. No timing is recorded.",
            entries
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"Recorded {entries.Count} original VP8 quality/size cases; no timing measurements.");
    }

    private static byte[] Pixels(string pattern, int width, int height)
    {
        if (pattern == "asymmetric-v1") return OriginalPixels.Generate(width, height, 8, opaque: true);
        byte[][] colors = [[16,32,200], [220,32,16], [32,200,48], [220,180,32], [32,180,200], [200,32,180]];
        var result = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var index = (y * width + x) * 4; var r = x * 255 / (width - 1);
                if (pattern == "gray-ramp") result[index] = result[index + 1] = result[index + 2] = (byte)r;
                else if (pattern == "soft-gradient")
                {
                    result[index] = (byte)r; result[index + 1] = (byte)(y * 255 / (height - 1));
                    result[index + 2] = (byte)((x + y) * 255 / (width + height - 2));
                }
                else colors[(x / 16 + y / 16 * 3) % colors.Length].CopyTo(result, index);
                result[index + 3] = 255;
            }
        return result;
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
