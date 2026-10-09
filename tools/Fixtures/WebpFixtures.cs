using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal static class WebpFixtures
{
    public static void Generate(string output, string executable, string executableHash, string version)
    {
        var fixtures = new List<object>();
        foreach (var size in new[] { (31, 23), (129, 67), (1, 1), (1, 17), (17, 1) })
        foreach (var effort in size.Item1 > 1 && size.Item2 > 1 ? new[] { 0, 6 } : new[] { 6 })
        foreach (var opaque in new[] { false, true })
        {
            var (width, height) = size; var pixels = OriginalPixels.Generate(width, height, 8, opaque: opaque);
            string[] encode = ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-f", "rawvideo", "-pixel_format", "rgba",
                "-video_size", $"{width}x{height}", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-c:v", "libwebp",
                "-lossless", "1", "-compression_level", effort.ToString(), "-f", "image2pipe", "pipe:1"];
            Register($"webp-ffmpeg-{width}x{height}-effort{effort}-{(opaque ? "opaque" : "alpha")}", width, height,
                OracleProcess.Run(executable, encode, pixels), "asymmetric-v1", pixels, encode);
        }
        foreach (var depth in new[] { 8, 16 })
        foreach (var effort in new[] { 0, 1, 4, 6 })
        {
            using var pixels = OriginalPixels.Buffer(31, 23, depth); using var encoded = new MemoryStream();
            ImageCodec.EncodeWebp(pixels, encoded, new WebpEncodeOptions { Effort = effort });
            Register($"webp-owned-depth{depth}-effort{effort}", 31, 23, encoded.ToArray(),
                $"asymmetric-v1; Lokad.Image lossless=true effort={effort} sourceDepth={depth}", OriginalPixels.Bytes(pixels), []);
        }
        for (var mode = 0; mode < 14; mode++)
            Register($"webp-authored-predictor{mode}", 3, 3, AuthoredWebp.Predictor(mode), $"authored-vp8l-predictor-v1; mode={mode}", null, []);
        Register("webp-authored-cross-color", 5, 3, AuthoredWebp.ColorTransform(), "authored-vp8l-cross-color-v1", null, []);
        Register("webp-authored-subtract-green", 3, 2, AuthoredWebp.SubtractGreen(), "authored-vp8l-subtract-green-v1; alphaHint=false", null, []);
        foreach (var count in new[] { 1, 2, 3, 4, 16, 17, 256 })
        foreach (var predictor in new[] { false, true })
            Register($"webp-authored-palette{count}-{(predictor ? "predicted" : "plain")}", 9, 1, AuthoredWebp.Palette(count, predictor),
                $"authored-vp8l-palette-v1; colors={count} predictor={predictor}", null, []);
        Register("webp-authored-cache", 2, 1, AuthoredWebp.CachedColor(), "authored-vp8l-color-cache-v1", null, []);
        Register("webp-authored-backref", 5, 1, AuthoredWebp.BackwardReference(), "authored-vp8l-overlapping-copy-v1", null, []);
        Register("webp-authored-entropy-groups", 8, 1, AuthoredWebp.EntropyGroups(), "authored-vp8l-spatial-groups-v1", null, []);
        Register("webp-authored-duplicate-simple", 1, 1, AuthoredWebp.DuplicateSimpleSymbols(), "authored-vp8l-duplicate-simple-v1", null, []);
        foreach (var defaultEight in new[] { false, true })
            Register($"webp-authored-repeats-{(defaultEight ? "16" : "17-18")}", defaultEight ? 1 : 5, 1,
                AuthoredWebp.CodeLengthRepeats(defaultEight), $"authored-vp8l-code-length-repeats-v1; defaultEight={defaultEight}", null, []);
        foreach (var metadataFirst in new[] { false, true })
            Register($"webp-authored-exif-{(metadataFirst ? "before" : "after")}", 9, 1, AuthoredWebp.OrientedPalette(metadataFirst),
                $"authored-vp8l-exif-v1; metadataFirst={metadataFirst}", null, []);
        File.WriteAllText(Path.Combine(output, "webp-manifest.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, platform = RuntimeInformation.RuntimeIdentifier, ffmpegSha256 = executableHash,
            ffmpegVersion = version.Replace("\r\n", "\n"), fixtures
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"Generated {fixtures.Count} original lossless WebP fixtures.");

        void Register(string name, int width, int height, byte[] encoded, string recipe, byte[]? input, string[] encode)
        {
            string[] decode = ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-noautorotate", "-f", "image2pipe", "-i", "pipe:0",
                "-frames:v", "1", "-threads:v", "1", "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"];
            var expected = OracleProcess.Run(executable, decode, encoded);
            if (expected.Length != width * height * 4) throw new InvalidOperationException("WebP oracle output geometry differs.");
            File.WriteAllBytes(Path.Combine(output, name + ".webp"), encoded);
            File.WriteAllBytes(Path.Combine(output, name + ".rgba"), expected);
            fixtures.Add(new { name, width, height, depth = 8, recipe, inputHash = input is null ? null : Hash(input),
                encode, decode, encodedHash = Hash(encoded), expectedHash = Hash(expected), comparison = "exact-rgba-samples" });
        }
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
