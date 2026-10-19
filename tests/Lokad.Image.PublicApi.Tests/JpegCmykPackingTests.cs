using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCmykPackingTests
{
    [Theory]
    [InlineData(false, false)] [InlineData(false, true)] [InlineData(true, false)] [InlineData(true, true)]
    public void FullResolutionCallerInkKeepsExplicitInversionPaddingAndIndependentOwners(bool adobe, bool progressive)
    {
        const int width = 65, height = 17, stride = width * 4 + 5; byte[] basis = [3, 81, 220, 137];
        var packet = AuthoredJpegCompatibility.Create(width, height, basis, adobe: adobe ? 0 : null, progressive: progressive, restart: 1, vary: true);
        var unchanged = packet.ToArray(); var first = Enumerable.Repeat((byte)173, stride * height + 11).ToArray(); var second = first.ToArray();
        ImageCodec.DecodeJpeg(packet, first, JpegSampleFormat.Cmyk, stride); ImageCodec.DecodeJpeg(packet, second, JpegSampleFormat.Cmyk, stride);
        foreach (var inversion in new[] { JpegCmykInversion.Auto, JpegCmykInversion.None, JpegCmykInversion.Invert })
        {
            Array.Fill(first, (byte)173);
            ImageCodec.DecodeJpeg(packet, first, JpegSampleFormat.Cmyk, stride, new() { CmykInversion = inversion });
            var invert = inversion == JpegCmykInversion.Invert || (inversion == JpegCmykInversion.Auto && adobe);
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                for (var c = 0; c < 4; c++)
                {
                    var value = (basis[c] + x / 8 * 13 + y / 8 * 29) & 255;
                    Assert.Equal((byte)(invert ? 255 - value : value), first[y * stride + x * 4 + c]);
                }
                Assert.All(first.AsSpan(y * stride + width * 4, 5).ToArray(), b => Assert.Equal(173, b));
            }
            Assert.All(first.AsSpan(stride * height).ToArray(), b => Assert.Equal(173, b));
        }
        var survivor = second.ToArray(); Array.Clear(first); Assert.Equal(survivor, second);
        Assert.Equal(unchanged, packet); Array.Clear(packet); Assert.Equal(survivor, second);
    }
}
