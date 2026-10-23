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
        JpegFourComponentExactTests.AssertNative(input, OriginalPixels.Bytes(output));
        JpegFourComponentExactTests.AssertNative(input, OriginalPixels.Bytes(survivor));
        var pixels = OriginalPixels.Bytes(survivor); output.GetRow8(0)[0] ^= 255; output.Dispose(); Array.Clear(input);
        Assert.Equal(pixels, OriginalPixels.Bytes(survivor));
    }
}
