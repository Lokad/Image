using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal static class JpegFixtures
{
    public static void Generate(string output, string executable, string executableHash, string version)
    {
        var fixtures = new List<object>();
        foreach (var sampling in new[] { JpegSampling.YCbCr444, JpegSampling.YCbCr422, JpegSampling.YCbCr420 })
        foreach (var size in new[] { (31, 23), (9, 7), (17, 1), (1, 17), (1, 1) })
        {
            var (width, height) = size;
            var pixels = OriginalPixels.Generate(width, height, 8, opaque: true);
            var pixelFormat = sampling switch { JpegSampling.YCbCr444 => "yuvj444p", JpegSampling.YCbCr422 => "yuvj422p", _ => "yuvj420p" };
            string[] encode = ["-hide_banner", "-loglevel", "error", "-cpuflags", "0", "-f", "rawvideo", "-pixel_format", "rgba",
                "-video_size", $"{width}x{height}", "-i", "pipe:0", "-frames:v", "1", "-threads:v", "1", "-c:v", "mjpeg",
                "-pix_fmt", pixelFormat, "-q:v", "3", "-f", "image2pipe", "pipe:1"];
            Register($"jpeg-ffmpeg-{sampling}-{width}x{height}", width, height, sampling,
                OracleProcess.Run(executable, encode, pixels), "asymmetric-v1", pixels, encode);
        }
        foreach (var sampling in Enum.GetValues<JpegSampling>())
        {
            using var pixels = OriginalPixels.Buffer(31, 23, 8, grayscale: sampling == JpegSampling.Grayscale, opaque: true);
            using var encoded = new MemoryStream();
            ImageCodec.EncodeJpeg(pixels, encoded, new JpegEncodeOptions { Sampling = sampling, Quality = 90, RestartInterval = 2 });
            Register($"jpeg-owned-{sampling}-restart", 31, 23, sampling, encoded.ToArray(),
                "asymmetric-v1; Lokad.Image quality=90 restart=2", OriginalPixels.Bytes(pixels), []);
        }
        foreach (var negative in new[] { false, true })
            Register($"jpeg-progressive-gray-{(negative ? "negative" : "positive")}", 8, 8, JpegSampling.Grayscale,
                AuthoredJpeg.RefinedBlock(negative), $"authored-t81-refined-block-v1; negativeDc={negative}", null, []);
        foreach (var restart in new[] { 0, 1 })
            Register($"jpeg-progressive-eob-restart{restart}", 24, 8, JpegSampling.Grayscale,
                AuthoredJpeg.EobRun(restart), $"authored-t81-eob-v1; restart={restart}", null, []);
        foreach (var sampling in new[] { JpegSampling.YCbCr444, JpegSampling.YCbCr422, JpegSampling.YCbCr420 })
            Register($"jpeg-progressive-{sampling}-31x23", 31, 23, sampling,
                AuthoredJpeg.ProgressiveColor(31, 23, sampling, 1), "authored-t81-color-v1; restart=1", null, []);
        Register("jpeg-progressive-YCbCr420-17x1", 17, 1, JpegSampling.YCbCr420,
            AuthoredJpeg.ProgressiveColor(17, 1, JpegSampling.YCbCr420, 1), "authored-t81-color-v1; restart=1", null, []);
        Register("jpeg-progressive-YCbCr422-1x17", 1, 17, JpegSampling.YCbCr422,
            AuthoredJpeg.ProgressiveColor(1, 17, JpegSampling.YCbCr422, 1), "authored-t81-color-v1; restart=1", null, []);
        Register("jpeg-progressive-sparse-bands", 8, 8, JpegSampling.Grayscale,
            AuthoredJpeg.SparseBands(), "authored-t81-sparse-bands-v1", null, []);
        Register("jpeg-progressive-empty-eob", 24, 8, JpegSampling.Grayscale,
            AuthoredJpeg.EmptyEobRun(), "authored-t81-empty-eob-v1", null, []);
        File.WriteAllText(Path.Combine(output, "jpeg-manifest.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, platform = RuntimeInformation.RuntimeIdentifier, ffmpegSha256 = executableHash,
            ffmpegVersion = version.Replace("\r\n", "\n"), fixtures
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"Generated {fixtures.Count} original JPEG fixtures.");

        void Register(string name, int width, int height, JpegSampling sampling, byte[] encoded,
            string recipe, byte[]? input, string[] encode)
        {
            var decode = JpegOracle.DecodeArguments(width, height, sampling);
            var expected = OracleProcess.Run(executable, decode, encoded);
            if (expected.Length != width * height * 4) throw new InvalidOperationException("JPEG oracle output geometry differs.");
            File.WriteAllBytes(Path.Combine(output, name + ".jpg"), encoded);
            File.WriteAllBytes(Path.Combine(output, name + ".rgba"), expected);
            fixtures.Add(new
            {
                name, width, height, depth = 8, sampling = sampling.ToString(), recipe,
                inputHash = input is null ? null : Hash(input), encode, decode,
                encodedHash = Hash(encoded), expectedHash = Hash(expected), comparison = "rgb-max4-rmse1.5-alpha255"
            });
        }
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
