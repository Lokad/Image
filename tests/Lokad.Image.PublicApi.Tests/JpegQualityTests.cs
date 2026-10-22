using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegQualityTests
{
    [Fact]
    public void FrozenOriginalReportKeepsSourceAndQuantizersWhileCurrentEntropyModesAgree()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Lokad.Image.slnx"))) root = root.Parent;
        Assert.NotNull(root);
        using var report = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root.FullName, "tests", "Quality", "jpeg-quality.json")));
        var entries = report.RootElement.GetProperty("entries"); Assert.Equal(320, entries.GetArrayLength());
        var names = new HashSet<string>(); var ownedCount = 0;
        foreach (var entry in entries.EnumerateArray())
        {
            var pattern = entry.GetProperty("pattern").GetString()!; var sampling = Enum.Parse<JpegSampling>(entry.GetProperty("requestedSampling").GetString()!);
            var depth = entry.GetProperty("sourceDepth").GetInt32(); var setting = entry.GetProperty("setting").GetInt32(); var subject = entry.GetProperty("subject").GetString()!;
            Assert.Contains(pattern, JpegQualityCorpus.Patterns); Assert.Contains(depth, new[] { 8, 16 });
            Assert.True(Enum.IsDefined(sampling));
            Assert.True(names.Add($"{subject}/{pattern}/{sampling}/{depth}/{setting}"));
            Assert.Equal((JpegQualityCorpus.Width, JpegQualityCorpus.Height), (entry.GetProperty("width").GetInt32(), entry.GetProperty("height").GetInt32()));
            var source = JpegQualityCorpus.Source(pattern, depth, sampling); var background = JpegQualityCorpus.Background(pattern);
            var reference = JpegQualityCorpus.Reference(source, depth, background);
            Assert.Equal(entry.GetProperty("inputHash").GetString(), Hash(source)); Assert.Equal(entry.GetProperty("referenceInputHash").GetString(), Hash(reference));
            Assert.InRange(entry.GetProperty("decoderMaximum").GetInt32(), 0, 4); Assert.InRange(entry.GetProperty("decoderRmse").GetDouble(), 0, 1.5);
            Assert.InRange(entry.GetProperty("rgbMaximum").GetInt32(), 0, 255); Assert.InRange(entry.GetProperty("rgbRmse").GetDouble(), 0, 255);
            Assert.True(entry.GetProperty("encodedBytes").GetInt32() > 0);
            if (subject == "FFmpeg-mjpeg")
            {
                Assert.Contains(setting, JpegQualityCorpus.NativeQScales); Assert.Equal("FFmpeg-qscale", entry.GetProperty("settingKind").GetString());
                Assert.Equal(3, entry.GetProperty("components").GetInt32());
                continue;
            }
            Assert.Equal("Lokad.Image", subject); Assert.Contains(setting, JpegQualityCorpus.ManagedQualities); ownedCount++;
            Assert.Equal("Lokad.Image-quality-1..100", entry.GetProperty("settingKind").GetString());
            using var image = JpegQualityCorpus.Buffer(source, depth); using var encoded = new MemoryStream();
            var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Sampling = sampling, Quality = setting, BackgroundRgb = background };
            // Retain the historical report and all its source/DQT/sampling checks.
            // Original color AC decisions may change lossy pixels. Grayscale and
            // quality100 keep frozen pixels; all current entropy modes agree.
            // Matched-quality compression gates are qualified separately, without
            // treating this historical quality-number grid as matched pairs.
            using var historicalToken = new CancellationTokenSource();
            ImageCodec.EncodeJpeg(image, encoded, options, historicalToken.Token);
            var bytes = encoded.ToArray();
            var header = JpegQualitySyntax.Read(bytes); Assert.Equal(entry.GetProperty("quantizationHash").GetString(), header.QuantizationHash);
            Assert.Equal(entry.GetProperty("codedSampling").GetString(), header.Sampling); Assert.Equal(sampling == JpegSampling.Grayscale ? 1 : 3, header.Components);
            using var decoded = ImageCodec.Decode(bytes); var currentPixelHash = Hash(OriginalPixels.Bytes(decoded));
            if (sampling == JpegSampling.Grayscale || setting == 100)
                Assert.Equal(entry.GetProperty("managedDecodedHash").GetString(), currentPixelHash);
            using var current = new MemoryStream(); ImageCodec.EncodeJpeg(image, current, options);
            var currentHeader = JpegQualitySyntax.Read(current.ToArray());
            Assert.Equal(header.QuantizationHash, currentHeader.QuantizationHash); Assert.Equal(header.Sampling, currentHeader.Sampling);
            using var currentDecoded = ImageCodec.Decode(current.ToArray());
            Assert.Equal(currentPixelHash, Hash(OriginalPixels.Bytes(currentDecoded)));
        }
        Assert.Equal(160, ownedCount);
    }

    [Theory]
    [InlineData(1, 50, 150, 255, 100, 250, 255)]
    [InlineData(50, 1, 3, 78, 2, 5, 93)]
    [InlineData(85, 1, 1, 23, 1, 2, 28)]
    [InlineData(100, 1, 1, 1, 1, 1, 1)]
    public void DeclaredQualityPolicyHasFrozenDcAcAndHighFrequencyEndpoints(int quality, int lumaDc, int lumaAc, int lumaLast, int chromaDc, int chromaAc, int chromaLast)
    {
        using var source = ImageBuffer.Create(1, 1, 8); new byte[] { 128, 128, 128, 255 }.CopyTo(source.GetRow8(0));
        foreach (var sampling in Enum.GetValues<JpegSampling>())
        {
            using var output = new MemoryStream(); ImageCodec.EncodeJpeg(source, output, new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Sampling = sampling, Quality = quality });
            var header = JpegQualitySyntax.Read(output.ToArray());
            Assert.Equal(sampling == JpegSampling.Grayscale ? 1 : 3, header.Components);
            Assert.Equal(sampling switch { JpegSampling.Grayscale => "1x1", JpegSampling.YCbCr444 => "1x1,1x1,1x1", JpegSampling.YCbCr422 => "2x1,1x1,1x1", _ => "2x2,1x1,1x1" }, header.Sampling);
            var luma = header.QuantizationTables[0]; Assert.Equal(0, luma[0]);
            Assert.Equal(new[] { lumaDc, lumaAc, lumaLast }, new[] { (int)luma[1], luma[2], luma[64] });
            if (sampling != JpegSampling.Grayscale)
            {
                var chroma = header.QuantizationTables[1]; Assert.Equal(1, chroma[0]);
                Assert.Equal(new[] { chromaDc, chromaAc, chromaLast }, new[] { (int)chroma[1], chroma[2], chroma[64] });
            }
        }
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void AlphaFlatteningAndDepthReductionMatchTheExplicitReferenceOnce(int depth)
    {
        foreach (var sampling in Enum.GetValues<JpegSampling>())
        {
            const string pattern = "transparent-soft-gradient"; var source = JpegQualityCorpus.Source(pattern, depth, sampling);
            var reference = JpegQualityCorpus.Reference(source, depth, JpegQualityCorpus.Background(pattern));
            using var original = JpegQualityCorpus.Buffer(source, depth); using var normalized = JpegQualityCorpus.Buffer(reference, 8);
            using var first = new MemoryStream(); using var second = new MemoryStream();
            ImageCodec.EncodeJpeg(original, first, new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Sampling = sampling, Quality = 85, BackgroundRgb = JpegQualityCorpus.Background(pattern) });
            ImageCodec.EncodeJpeg(normalized, second, new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Sampling = sampling, Quality = 85, BackgroundRgb = JpegQualityCorpus.Background(pattern) });
            Assert.Equal(second.ToArray(), first.ToArray());
        }
    }

    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
