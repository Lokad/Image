using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCmykDcTests
{
    [Theory]
    [InlineData(1)] [InlineData(63)]
    public void NonzeroAcUsesTheEstablishedSampleTransformAndKeepsOpaqueOwnedPixels(int acTerms)
    {
        const int width = 65, height = 31;
        var input = AuthoredJpegCompatibility.Create(width, height, [17, 191, 250, 63], adobe: 0, vary: true, nonzeroAc: true, acTerms: acTerms);
        var canonical = new byte[width * height * 4]; ImageCodec.DecodeJpeg(input, canonical, JpegSampleFormat.Cmyk);
        Assert.NotEqual(canonical[0], canonical[7 * 4]);
        using var output = ImageCodec.Decode(input); using var survivor = ImageCodec.Decode(input);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var at = (y * width + x) * 4;
            for (var c = 0; c < 3; c++)
            {
                var expected = (byte)decimal.Round((decimal)(255 - canonical[at + c]) * (255 - canonical[at + 3]) / 255, 0, MidpointRounding.AwayFromZero);
                Assert.Equal(expected, output.GetRow8(y)[x * 4 + c]); Assert.Equal(expected, survivor.GetRow8(y)[x * 4 + c]);
            }
            Assert.Equal(255, output.GetRow8(y)[x * 4 + 3]);
        }
        var pixels = OriginalPixels.Bytes(survivor); output.GetRow8(0)[0] ^= 255; output.Dispose(); Array.Clear(input);
        Assert.Equal(pixels, OriginalPixels.Bytes(survivor));
    }
}
