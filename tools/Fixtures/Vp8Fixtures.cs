using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lokad.Image;
using Lokad.Image.Tooling;

internal static class Vp8Fixtures
{
    public static void Generate(string output, string executable, string executableHash, string version)
    {
        var fixtures = new List<object>();
        foreach (var size in new[] { (1, 1), (1, 17), (17, 1), (31, 23), (129, 67) })
            foreach (var effort in new[] { 0, 6 })
                foreach (var quality in new[] { 1, 75, 100 })
                    foreach (var opaque in new[] { false, true })
                    {
                        var (width, height) = size;
                        Register($"vp8-ffmpeg-{width}x{height}-effort{effort}-quality{quality}-{(opaque ? "opaque" : "alpha")}",
                            width, height, effort, quality, OriginalPixels.Generate(width, height, 8, opaque: opaque), "asymmetric-v1");
                    }
        foreach (var pattern in new[] { "blue", "gray", "horizontal", "vertical" })
        {
            const int width = 33, height = 19; var pixels = new byte[width * height * 4];
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var position = (y * width + x) * 4;
                    var value = pattern == "horizontal" ? x * 255 / (width - 1) : pattern == "vertical" ? y * 255 / (height - 1) : 123;
                    pixels[position] = pattern == "blue" ? (byte)10 : (byte)value;
                    pixels[position + 1] = pattern == "blue" ? (byte)40 : (byte)value;
                    pixels[position + 2] = pattern == "blue" ? (byte)120 : (byte)value; pixels[position + 3] = 255;
                }
            Register($"vp8-ffmpeg-{pattern}", width, height, 6, 75, pixels, $"vp8-pattern-v1; pattern={pattern}");
        }
        for (var filter = 0; filter < 4; filter++)
        {
            var source = OriginalPixels.Generate(31,23,8,opaque:true);
            var opaque = WebpOracle.Encode(source,31,23,6,75,executable);
            Register($"vp8-authored-raw-alpha-filter{filter}",31,23,6,75,source,$"authored-alph-v1; filter={filter}; alpha=(23*x+37*y+7*x*y)%256",
                AuthoredAlpha.Wrap(opaque,31,23,filter));
        }
        foreach (var size in new[] { (1, 1), (1, 17), (17, 1), (31, 23), (129, 67) })
            foreach (var effort in new[] { 0, 6 })
                foreach (var quality in new[] { 1, 100 })
                    foreach (var opaque in new[] { false, true })
                    {
                        var (width, height) = size;
                        Register($"vp8-managed-{width}x{height}-effort{effort}-quality{quality}-{(opaque ? "opaque" : "alpha")}",
                            width, height, effort, quality, OriginalPixels.Generate(width, height, 8, opaque: opaque), "asymmetric-v1", managedDepth: 8);
                    }
        foreach (var opaque in new[] { false, true })
            Register($"vp8-managed-31x23-depth16-effort4-quality85-{(opaque ? "opaque" : "alpha")}",
                31, 23, 4, 85, OriginalPixels.Generate(31, 23, 16, opaque: opaque), "asymmetric-v1", managedDepth: 16);
        File.WriteAllText(Path.Combine(output, "vp8-manifest.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            platform = RuntimeInformation.RuntimeIdentifier,
            ffmpegSha256 = executableHash,
            ffmpegVersion = version.Replace("\r\n", "\n"),
            fixtures
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"Generated {fixtures.Count} original lossy WebP fixtures.");

        void Register(string name, int width, int height, int effort, int quality, byte[] pixels, string recipe, byte[]? authored = null, int managedDepth = 0)
        {
            var encode = managedDepth == 0 ? WebpOracle.EncodeArguments(width, height, effort, quality) : null;
            byte[] encoded;
            if (managedDepth == 0) encoded = authored ?? OracleProcess.Run(executable, encode!, pixels);
            else
            {
                using var source = ImageBuffer.Create(width, height, managedDepth);
                for (var y = 0; y < height; y++)
                {
                    var row = managedDepth == 8 ? source.GetRow8(y) : MemoryMarshal.AsBytes(source.GetRow16(y));
                    pixels.AsSpan(y * source.Stride, source.Stride).CopyTo(row);
                }
                using var stream = new MemoryStream();
                ImageCodec.EncodeWebp(source, stream, new WebpEncodeOptions { Lossless = false, Quality = quality, Effort = effort });
                encoded = stream.ToArray();
            }
            var alpha = ImageCodec.Identify(encoded).HasAlpha;
            var decode = WebpOracle.DecodeArguments(width, height, alpha); var yuvDecode = WebpOracle.YuvArguments(alpha);
            var expected = OracleProcess.Run(executable, decode, encoded); var yuv = OracleProcess.Run(executable, yuvDecode, encoded);
            if (expected.Length != width * height * 4 || yuv.Length != width * height * (alpha ? 2 : 1) + 2 * ((width + 1) / 2) * ((height + 1) / 2))
                throw new InvalidOperationException("VP8 oracle output geometry differs.");
            File.WriteAllBytes(Path.Combine(output, name + ".webp"), encoded);
            File.WriteAllBytes(Path.Combine(output, name + ".rgba"), expected); File.WriteAllBytes(Path.Combine(output, name + ".yuv"), yuv);
            fixtures.Add(new
            {
                name,
                width,
                height,
                alpha,
                effort,
                quality,
                recipe,
                inputHash = Hash(pixels),
                authoring = managedDepth != 0 ? "Lokad.Image.EncodeWebp" : authored is null ? null : "AuthoredAlpha.Wrap",
                managedOptions = managedDepth == 0 ? null : new { bitDepth = managedDepth, lossless = false, quality, effort },
                alphaSourceHash = authored is null ? null : Hash(AuthoredAlpha.Samples(width,height)),
                encode,
                decode,
                yuvDecode,
                encodedHash = Hash(encoded),
                expectedHash = Hash(expected),
                yuvHash = Hash(yuv),
                comparison = "exact-yuv-and-alpha; rgb-max4-rmse1.5-after-explicit-conversion",
                chromaPreparation = managedDepth == 0 ? null : "encoded-alpha-weighted-2x2-v1"
            });
        }
    }

    public static void RefreshManaged(string output, string executable, string executableHash)
    {
        var path = Path.Combine(output, "vp8-manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        if (manifest["ffmpegSha256"]!.GetValue<string>() != executableHash)
            throw new InvalidOperationException("Managed fixture refresh requires its registered original FFmpeg oracle.");
        var count = 0; var changed = 0;
        foreach (var node in manifest["fixtures"]!.AsArray())
        {
            var fixture = node!.AsObject();
            if (fixture["authoring"]?.GetValue<string>() != "Lokad.Image.EncodeWebp") continue;
            count++;
            var name = fixture["name"]!.GetValue<string>();
            if (Path.GetFileName(name) != name || fixture["recipe"]!.GetValue<string>() != "asymmetric-v1")
                throw new InvalidOperationException("Unexpected managed fixture provenance.");
            var options = fixture["managedOptions"]!.AsObject();
            var width = fixture["width"]!.GetValue<int>(); var height = fixture["height"]!.GetValue<int>();
            using var source = OriginalPixels.Buffer(width, height, options["bitDepth"]!.GetValue<int>(),
                opaque: !fixture["alpha"]!.GetValue<bool>());
            if (Hash(OriginalPixels.Bytes(source)) != fixture["inputHash"]!.GetValue<string>())
                throw new InvalidOperationException("Original managed fixture input changed.");
            using var stream = new MemoryStream();
            ImageCodec.EncodeWebp(source, stream, new WebpEncodeOptions { Lossless = options["lossless"]!.GetValue<bool>(),
                Quality = options["quality"]!.GetValue<int>(), Effort = options["effort"]!.GetValue<int>() });
            var encoded = stream.ToArray();
            if (ImageCodec.Identify(encoded).HasAlpha != fixture["alpha"]!.GetValue<bool>())
                throw new InvalidOperationException("Fixture alpha geometry changed.");
            var decode = fixture["decode"]!.AsArray().Select(value => value!.GetValue<string>()).ToArray();
            var yuvDecode = fixture["yuvDecode"]!.AsArray().Select(value => value!.GetValue<string>()).ToArray();
            var expected = OracleProcess.Run(executable, decode, encoded); var yuv = OracleProcess.Run(executable, yuvDecode, encoded);
            if (Hash(encoded) != fixture["encodedHash"]!.GetValue<string>()) changed++;
            WriteIfChanged(name + ".webp", encoded); WriteIfChanged(name + ".rgba", expected); WriteIfChanged(name + ".yuv", yuv);
            fixture["encodedHash"] = Hash(encoded); fixture["expectedHash"] = Hash(expected); fixture["yuvHash"] = Hash(yuv);
            fixture["chromaPreparation"] = "encoded-alpha-weighted-2x2-v1";
        }
        if (count != 42) throw new InvalidOperationException("Unexpected managed fixture count.");
        File.WriteAllText(path, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"Refreshed {count} existing managed VP8 fixtures; {changed} bitstreams changed. Original inputs/options and native fixtures retained.");

        void WriteIfChanged(string name, byte[] bytes)
        {
            var target = Path.Combine(output, name);
            if (!File.ReadAllBytes(target).AsSpan().SequenceEqual(bytes)) File.WriteAllBytes(target, bytes);
        }
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
