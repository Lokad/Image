using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCmykRowTests
{
    [Theory]
    [InlineData(false, false)] [InlineData(false, true)] [InlineData(true, false)] [InlineData(true, true)]
    public void FullResolutionCmykKeepsAnalyticalRgbAndIndependentOwners(bool adobe, bool progressive)
    {
        const int width = 65, height = 31; byte[] basis = [17, 191, 250, 63];
        var encoded = AuthoredJpegCompatibility.Create(width, height, basis, adobe: adobe ? 0 : null, progressive: progressive, vary: true);
        var unchanged = encoded.ToArray(); using var first = ImageCodec.Decode(encoded); using var survivor = ImageCodec.Decode(encoded);
        Assert.Equal((width, height, 8, ImageOrientation.Normal), (first.Width, first.Height, first.BitDepth, first.Orientation));
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var k = (basis[3] + x / 8 * 13 + y / 8 * 29) & 255; if (!adobe) k = 255 - k;
            for (var c = 0; c < 3; c++)
            {
                var channel = (basis[c] + x / 8 * 13 + y / 8 * 29) & 255; if (!adobe) channel = 255 - channel;
                var expected = (byte)decimal.Round((decimal)channel * k / 255, 0, MidpointRounding.AwayFromZero);
                Assert.Equal(expected, first.GetRow8(y)[x * 4 + c]); Assert.Equal(expected, survivor.GetRow8(y)[x * 4 + c]);
            }
            Assert.Equal(255, first.GetRow8(y)[x * 4 + 3]);
        }
        var expectedSurvivor = OriginalPixels.Bytes(survivor);
        first.GetRow8(0)[0] ^= 255; first.Dispose(); Assert.Equal(expectedSurvivor, OriginalPixels.Bytes(survivor));
        Assert.Equal(unchanged, encoded); Array.Clear(encoded); Assert.Equal(expectedSurvivor, OriginalPixels.Bytes(survivor));
    }
}
