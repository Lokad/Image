using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class Vp8RateSearchFfmpegTests
{
    // The older 129x67 controls do not admit the optional search cache. This
    // single geometry crosses that boundary, including exact 8/16-bit alpha.
    [Theory] [InlineData(8, 78)] [InlineData(16, 78)] [InlineData(8, 100)] [InlineData(16, 100)]
    public void AdmittedReconstructionSearchMatchesIndependentDecoding(int depth, int quality)
    {
        const int width = 129, height = 129;
        using var source = OriginalPixels.Buffer(width, height, depth);
        var unchanged = OriginalPixels.Bytes(source);
        using var packet = new MemoryStream();
        ImageCodec.EncodeWebp(source, packet, new WebpEncodeOptions { Lossless = false, Quality = quality, Effort = 4 });
        var encoded = packet.ToArray();
        Assert.True(ImageCodec.Identify(encoded).HasAlpha);
        var expected = WebpOracle.Decode(encoded, width, height, alpha: true);
        using var decoded = ImageCodec.Decode(encoded); var actual = OriginalPixels.Bytes(decoded);
        Assert.Equal(expected, actual);
        for (var i = 0; i < actual.Length; i++)
        {
            if (i % 4 == 3)
            {
                var alpha = depth == 8 ? unchanged[i]
                    : (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(unchanged.AsSpan(i * 2)) + 128) / 257;
                Assert.Equal(alpha, expected[i]); Assert.Equal(alpha, actual[i]);
            }
        }
        Assert.Equal(unchanged, OriginalPixels.Bytes(source));
    }
}
