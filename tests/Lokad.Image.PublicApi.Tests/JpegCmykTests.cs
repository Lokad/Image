using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCmykTests
{
    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void CmykConventionsHaveAnalyticalSamplesAndOpaqueRgb(bool progressive, bool adobe)
    {
        byte[] values = [230, 180, 100, 210];
        var data = AuthoredJpegCompatibility.Create(17, 9, values, adobe: adobe ? 0 : null, progressive: progressive, restart: 1);
        var original = data.ToArray();
        Assert.Equal((17, 9, false), (ImageCodec.Identify(data).Width, ImageCodec.Identify(data).Height, ImageCodec.Identify(data).HasAlpha));
        using var input = new MemoryStream(data); Assert.Equal(ImageCodec.Identify(data), ImageCodec.Identify(input)); Assert.True(input.CanRead);
        var cmyk = new byte[17 * 9 * 4];
        ImageCodec.DecodeJpeg(data, cmyk, JpegSampleFormat.Cmyk);
        var canonical = values.Select(v => adobe ? (byte)(255 - v) : v).ToArray();
        Assert.All(cmyk.Chunk(4), pixel => Assert.Equal(canonical, pixel));
        var expected = Enumerable.Range(0, 3).Select(c => (byte)(((255 - canonical[c]) * (255 - canonical[3]) + 127) / 255)).ToArray();
        using var rgba = ImageCodec.Decode(data);
        Assert.All(OriginalPixels.Bytes(rgba).Chunk(4), pixel => Assert.Equal(expected.Concat(new byte[] { 255 }), pixel));
        var rgb = new byte[17 * 9 * 3]; ImageCodec.DecodeJpeg(data, rgb, JpegSampleFormat.Rgb);
        Assert.All(rgb.Chunk(3), pixel => Assert.Equal(expected, pixel));
        var gray = new byte[17 * 9]; ImageCodec.DecodeJpeg(data, gray, JpegSampleFormat.Grayscale);
        var expectedGray = Quantize(.299m * (255 - canonical[0]) * (255 - canonical[3]) / 255
            + .587m * (255 - canonical[1]) * (255 - canonical[3]) / 255 + .114m * (255 - canonical[2]) * (255 - canonical[3]) / 255);
        Assert.All(gray, value => Assert.Equal(expectedGray, value));
        ImageCodec.DecodeJpeg(data, cmyk, JpegSampleFormat.Cmyk, options: new() { CmykInversion = JpegCmykInversion.None });
        Assert.All(cmyk.Chunk(4), pixel => Assert.Equal(values, pixel));
        ImageCodec.DecodeJpeg(data, cmyk, JpegSampleFormat.Cmyk, options: new() { CmykInversion = JpegCmykInversion.Invert });
        Assert.All(cmyk.Chunk(4), pixel => Assert.Equal(values.Select(v => (byte)(255 - v)), pixel));
        Assert.Equal(original, data);
    }

    [Theory]
    [InlineData(false, 1)] [InlineData(true, 1)] [InlineData(false, 2)] [InlineData(true, 2)]
    public void YcckCanonicalInkAndReferenceRgbHaveExplicitPrecision(bool progressive, int sampling)
    {
        byte[] values = [80, 170, 90, 200];
        var data = AuthoredJpegCompatibility.Create(17, 9, values, sampling, sampling, true, 2, progressive, 1);
        // Independent decimal evaluation of the documented matrix and inversion.
        decimal c = 80 + 1.402m * -38, m = 80 - .344136286m * 42 - .714136286m * -38, y = 80 + 1.772m * 42, k = 55;
        byte[] expectedCmyk = [Quantize(c), Quantize(m), Quantize(y), Quantize(k)];
        byte[] expectedRgb = [178, 128, 79]; // Frozen native YCCK444/420 control.
        var samples = new byte[17 * 9 * 4]; ImageCodec.DecodeJpeg(data, samples, JpegSampleFormat.Cmyk);
        Assert.All(samples.Chunk(4), pixel => Assert.Equal(expectedCmyk, pixel));
        var rgb = new byte[17 * 9 * 3]; ImageCodec.DecodeJpeg(data, rgb, JpegSampleFormat.Rgb);
        Assert.All(rgb.Chunk(3), pixel => Assert.Equal(expectedRgb, pixel));
        using var owned = ImageCodec.Decode(data);
        Assert.All(OriginalPixels.Bytes(owned).Chunk(4), pixel => Assert.Equal(expectedRgb.Concat(new byte[] { 255 }), pixel));
        ImageCodec.DecodeJpeg(data, samples, JpegSampleFormat.Cmyk,
            options: new() { SourceColorSpace = JpegColorSpace.Cmyk, CmykInversion = JpegCmykInversion.None });
        Assert.All(samples.Chunk(4), pixel => Assert.Equal(values, pixel));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SaturatedYcckMatchesTheNativeBlackBeforeColorBoundary(bool progressive)
    {
        var data = AuthoredJpegCompatibility.Create(8, 8, [0, 128, 0, 128], adobe: 2, progressive: progressive);
        // Original saturated control: compare complete native pixels, including
        // clipping after black adjustment and the integer color matrix.
        using var result = ImageCodec.Decode(data);
        JpegFourComponentExactTests.AssertNative(data, OriginalPixels.Bytes(result));
    }

    [Fact]
    public void CmykEncodingAndNonFourComponentCmykOutputFailWithStructuredFeatures()
    {
        using var output = new MemoryStream(); var source = new byte[16];
        var encoded = Assert.Throws<UnsupportedImageException>(() => ImageCodec.EncodeJpeg(source, 2, 2, JpegSampleFormat.Cmyk, output));
        Assert.Equal((UnsupportedImageFeature.ComponentLayout, ImageFormat.Jpeg), (encoded.Feature, encoded.Format)); Assert.Equal(0, output.Length);
        var data = AuthoredJpeg.DirectRgb();
        var failure = Assert.Throws<UnsupportedImageException>(() => ImageCodec.DecodeJpeg(data, new byte[256], JpegSampleFormat.Cmyk));
        Assert.Equal(UnsupportedImageFeature.ComponentLayout, failure.Feature);
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.DecodeJpeg(data, new byte[256], JpegSampleFormat.Rgb,
            options: new() { CmykInversion = (JpegCmykInversion)99 }));
    }

    private static byte Quantize(decimal value) => (byte)Math.Clamp(decimal.Round(value, 0, MidpointRounding.AwayFromZero), 0, 255);
}
