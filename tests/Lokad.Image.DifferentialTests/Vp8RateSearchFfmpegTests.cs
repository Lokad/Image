using Lokad.Image.Tooling;

namespace Lokad.Image.DifferentialTests;

public sealed class Vp8RateSearchFfmpegTests
{
    // The older 129x67 controls do not admit the optional search cache. This
    // single geometry crosses that boundary, including exact 8/16-bit alpha.
    [Theory] [InlineData(8)] [InlineData(16)]
    public void AdmittedReconstructionSearchMatchesIndependentDecoding(int depth)
    {
        const int width = 129, height = 129;
        using var source = OriginalPixels.Buffer(width, height, depth);
        var unchanged = OriginalPixels.Bytes(source);
        using var packet = new MemoryStream();
        ImageCodec.EncodeWebp(source, packet, new WebpEncodeOptions { Lossless = false, Quality = 78, Effort = 4 });
        var encoded = packet.ToArray();
        Assert.True(ImageCodec.Identify(encoded).HasAlpha);
        var expected = WebpOracle.Decode(encoded, width, height, alpha: true);
        using var decoded = ImageCodec.Decode(encoded); var actual = OriginalPixels.Bytes(decoded);
        var maximum = 0; double squares = 0;
        for (var i = 0; i < actual.Length; i++)
        {
            if (i % 4 == 3)
            {
                var alpha = depth == 8 ? unchanged[i]
                    : (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(unchanged.AsSpan(i * 2)) + 128) / 257;
                Assert.Equal(alpha, expected[i]); Assert.Equal(alpha, actual[i]);
            }
            else
            {
                var error = Math.Abs(expected[i] - actual[i]); maximum = Math.Max(maximum, error); squares += error * error;
            }
        }
        Assert.True(maximum <= 4 && Math.Sqrt(squares / (width * height * 3)) <= 1.5);
        Assert.Equal(unchanged, OriginalPixels.Bytes(source));
    }
}
