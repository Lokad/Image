using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegChromaPairTests
{
    [Fact]
    public void SharedChromaKeepsTheQualifiedAcDecisionPacketAndSource()
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var original = OriginalPixels.Bytes(source);
        using var output = new MemoryStream();
        ImageCodec.EncodeJpeg(source, output, new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 69, Sampling = JpegSampling.YCbCr420, BackgroundRgb = 0x204060 });
        Assert.Equal("7fb75df22ca227bf8017a491cf8f1187127c53c4f3847c4058f22fb32254f58c",
            Convert.ToHexString(SHA256.HashData(output.ToArray())).ToLowerInvariant());
        Assert.Equal(original, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(69, 0)] [InlineData(100, 0)] [InlineData(100, 1)]
    public void OptionalScratchKeepsPacketsAcrossAdmissionBoundaryAndRestartTails(int quality, int restart)
    {
        using var source = OriginalPixels.Buffer(129, 129, 8); var original = OriginalPixels.Bytes(source);
        const int mcus = 9 * 9, blocks = mcus * 6;
        var baseBytes = original.Length + 8192L + (blocks * 64L + (restart == 0 ? 0 : mcus)) * 4 + 65536;
        var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = quality, Sampling = JpegSampling.YCbCr420, RestartInterval = restart, BackgroundRgb = 0x204060 };
        byte[]? reference = null;
        foreach (var extra in new[] { 0, 63, 64, 65 })
        {
            using var output = new MemoryStream();
            ImageCodec.EncodeJpeg(source, output, options with { Limits = ImageLimits.Default with { MaxWorkingBytes = baseBytes + extra } });
            var packet = output.ToArray(); reference ??= packet; Assert.Equal(reference, packet);
            Assert.Equal(original, OriginalPixels.Bytes(source));
        }
    }
}
