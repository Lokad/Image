using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

internal static class JpegQuality
{
    public static void Generate(string output, string executable, string executableHash, string version)
    {
        var entries = new List<object>(); const int width = JpegQualityCorpus.Width, height = JpegQualityCorpus.Height;
        foreach (var pattern in JpegQualityCorpus.Patterns)
        foreach (var sampling in Enum.GetValues<JpegSampling>())
        foreach (var depth in new[] { 8, 16 })
        {
            var source = JpegQualityCorpus.Source(pattern, depth, sampling); var background = JpegQualityCorpus.Background(pattern);
            var reference = JpegQualityCorpus.Reference(source, depth, background); using var image = JpegQualityCorpus.Buffer(source, depth);
            for (var level = 0; level < 4; level++)
            foreach (var subject in new[] { "Lokad.Image", "FFmpeg-mjpeg" })
            {
                var owned = subject == "Lokad.Image"; var setting = owned ? JpegQualityCorpus.ManagedQualities[level] : JpegQualityCorpus.NativeQScales[level];
                string[]? encode = null; byte[] encoded;
                if (owned)
                {
                    using var stream = new MemoryStream(); ImageCodec.EncodeJpeg(image, stream, new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Sampling = sampling, Quality = setting, BackgroundRgb = background });
                    encoded = stream.ToArray();
                }
                else
                {
                    encode = JpegOracle.EncodeArguments(width, height, sampling, setting);
                    encoded = OracleProcess.Run(executable, encode, reference);
                }
                var header = JpegQualitySyntax.Read(encoded);
                if ((header.Width, header.Height, header.Depth) != (width, height, 8)) throw new InvalidOperationException("Quality frame profile differs.");
                var decode = JpegOracle.DecodeArguments(width, height, sampling); var native = OracleProcess.Run(executable, decode, encoded);
                using var decoded = ImageCodec.Decode(encoded); var managed = OriginalPixels.Bytes(decoded);
                var difference = JpegQualityCorpus.Compare(native, managed);
                if (difference.Maximum > 4 || difference.Rmse > 1.5) throw new InvalidOperationException($"JPEG decoder differs: {subject}/{pattern}/{sampling}/depth{depth}/setting{setting}: max {difference.Maximum}, RMSE {difference.Rmse}.");
                var quality = JpegQualityCorpus.Compare(reference, native);
                entries.Add(new
                {
                    subject, pattern, width, height, sourceDepth = depth, requestedSampling = sampling.ToString(), setting,
                    settingKind = owned ? "Lokad.Image-quality-1..100" : "FFmpeg-qscale", backgroundRgb = background,
                    inputHash = Hash(source), referenceInputHash = Hash(reference), encodedBytes = encoded.Length, encodedHash = Hash(encoded),
                    components = header.Components, codedSampling = header.Sampling, quantizationHash = header.QuantizationHash,
                    rgbMaximum = quality.Maximum, rgbRmse = quality.Rmse, rgbPsnr = quality.Psnr,
                    nativeDecodedHash = Hash(native), managedDecodedHash = Hash(managed),
                    decoderMaximum = difference.Maximum, decoderRmse = difference.Rmse, encode, decode
                });
            }
            Console.WriteLine($"JPEG quality validated {pattern}/{sampling}/depth{depth}.");
        }
        File.WriteAllText(Path.Combine(output, "jpeg-quality.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = 1, platform = RuntimeInformation.RuntimeIdentifier, ffmpegSha256 = executableHash,
            ffmpegVersion = version.Replace("\r\n", "\n"),
            recipe = "original-jpeg-quality-v1", method = "Original RGBA8/RGBA16. Grayscale profiles start with gray samples. Transparency is flattened over 0x202020 with one rounding to opaque RGBA8. Native encoding consumes that explicit reference; managed encoding consumes the original source. Both outputs are independently decoded by FFmpeg with full-range BT.601 and centered bilinear chroma expansion. Source error is distinct from decoder agreement (maximum 4, RMSE 1.5). Settings are encoder-specific and are not matched-quality or speed claims. FFmpeg emits neutral-chroma 4:4:4 for grayscale-source comparisons; actual components and factors are recorded. No timing is recorded.",
            entries
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"Recorded {entries.Count} original JPEG quality/size cases; no timing measurements.");
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
