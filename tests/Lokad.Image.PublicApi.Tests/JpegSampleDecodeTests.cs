using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegSampleDecodeTests
{
    public static IEnumerable<object[]> Profiles()
    {
        foreach (var sampling in Enum.GetValues<JpegSampling>()) yield return [sampling, false];
        yield return [JpegSampling.Grayscale, true];
        yield return [JpegSampling.YCbCr420, true];
        yield return [JpegSampling.YCbCr444, true];
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void DirectRgbMatchesEstablishedPixelsAndPreservesPaddingAndIndependentBuffers(JpegSampling sampling, bool progressive)
    {
        const int width = 31, height = 23, stride = width * 3 + 5;
        byte[] encoded;
        if (progressive) encoded = AuthoredJpeg.ProgressiveFrame(width, height, sampling, 1);
        else
        {
            using var source = OriginalPixels.Buffer(width, height, 8, opaque: true);
            using var output = new MemoryStream();
            ImageCodec.EncodeJpeg(source, output, new() { Sampling = sampling, RestartInterval = 1 });
            encoded = output.ToArray();
        }
        var original = encoded.ToArray();
        using var expected = ImageCodec.Decode(encoded);
        var actual = Enumerable.Repeat((byte)0xA7, stride * height + 9).ToArray();
        var info = ImageCodec.DecodeJpeg(encoded, actual, JpegSampleFormat.Rgb, stride);
        Assert.Equal((width, height, 8, false), (info.Width, info.Height, info.BitDepth, info.HasAlpha));
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                Assert.Equal(expected.GetRow8(y).Slice(x * 4, 3).ToArray(), actual.AsSpan(y * stride + x * 3, 3).ToArray());
            Assert.All(actual.AsSpan(y * stride + width * 3, 5).ToArray(), value => Assert.Equal(0xA7, value));
        }
        Assert.All(actual.AsSpan(stride * height).ToArray(), value => Assert.Equal(0xA7, value));
        var other = new byte[width * height * 3];
        using var liveToken = new CancellationTokenSource();
        ImageCodec.DecodeJpeg(encoded, other, JpegSampleFormat.Rgb, cancellationToken: liveToken.Token);
        actual[0] ^= 255;
        Assert.Equal(expected.GetRow8(0)[0], other[0]);
        Assert.Equal(original, encoded);
    }

    [Theory]
    [InlineData(ImageOrientation.Normal)] [InlineData(ImageOrientation.MirrorHorizontal)]
    [InlineData(ImageOrientation.Rotate180)] [InlineData(ImageOrientation.MirrorVertical)]
    [InlineData(ImageOrientation.Transpose)] [InlineData(ImageOrientation.Rotate90)]
    [InlineData(ImageOrientation.Transverse)] [InlineData(ImageOrientation.Rotate270)]
    public void OrientationIsOptInAndWritesTheOrientedCallerLayout(ImageOrientation orientation)
    {
        const int width = 17, height = 9;
        var data = AuthoredJpeg.GrayscaleDcGrid(width, height, false);
        var encoded = OriginalExif.Attach(new(data, null, width, height, MetadataProfile.JpegBaseline420), orientation, true);
        var raw = new byte[width * height];
        var rawInfo = ImageCodec.DecodeJpeg(encoded, raw, JpegSampleFormat.Grayscale);
        Assert.Equal((width, height, orientation), (rawInfo.Width, rawInfo.Height, rawInfo.Orientation));
        var analytical = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var value = (byte)(x / 8 + y / 8 * 3);
            Assert.Equal(value, raw[y * width + x]);
            new byte[] { value, value, value, 255 }.CopyTo(analytical, (y * width + x) * 4);
        }
        var expected = OriginalExif.Permute(analytical, width, height, 8, orientation);
        var outputWidth = (int)orientation >= 5 ? height : width;
        var outputHeight = (int)orientation >= 5 ? width : height;
        var stride = outputWidth + 3;
        var actual = Enumerable.Repeat((byte)0xA7, outputHeight * stride).ToArray();
        var info = ImageCodec.DecodeJpeg(encoded, actual, JpegSampleFormat.Grayscale, stride,
            new() { ApplyOrientation = true });
        Assert.Equal((outputWidth, outputHeight, ImageOrientation.Normal), (info.Width, info.Height, info.Orientation));
        for (var y = 0; y < outputHeight; y++)
        {
            for (var x = 0; x < outputWidth; x++) Assert.Equal(expected[(y * outputWidth + x) * 4], actual[y * stride + x]);
            Assert.All(actual.AsSpan(y * stride + outputWidth, 3).ToArray(), value => Assert.Equal(0xA7, value));
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ColorInterpretationCanOverrideMarkersAndGrayscaleUsesItsDefinedMatrix(bool adobe)
    {
        var encoded = AuthoredJpeg.DirectRgb(adobe);
        var rgb = new byte[8 * 8 * 3]; var gray = new byte[8 * 8];
        ImageCodec.DecodeJpeg(encoded, rgb, JpegSampleFormat.Rgb);
        Assert.All(rgb.Chunk(3), pixel => Assert.Equal(new byte[] { 45, 105, 205 }, pixel));
        ImageCodec.DecodeJpeg(encoded, gray, JpegSampleFormat.Grayscale);
        Assert.All(gray, value => Assert.Equal(98, value));
        var options = new JpegDecodeOptions { SourceColorSpace = JpegColorSpace.YCbCr };
        ImageCodec.DecodeJpeg(encoded, rgb, JpegSampleFormat.Rgb, options: options);
        Assert.All(rgb.Chunk(3), pixel => Assert.Equal(new byte[] { 153, 0, 4 }, pixel));
        ImageCodec.DecodeJpeg(encoded, gray, JpegSampleFormat.Grayscale, options: options);
        Assert.All(gray, value => Assert.Equal(45, value));
        var error = Assert.Throws<UnsupportedImageException>(() => ImageCodec.DecodeJpeg(encoded, rgb,
            JpegSampleFormat.Rgb, options: new() { SourceColorSpace = JpegColorSpace.Grayscale }));
        Assert.Equal((UnsupportedImageFeature.ComponentLayout, ImageFormat.Jpeg), (error.Feature, error.Format));
    }

    [Fact]
    public void AuthoredProgressiveRefinementMatchesFrozenNativeCallerSamples()
    {
        var data = AuthoredJpeg.RefinedBlock();
        var expected = JpegFixtureTests.NativePixels("jpeg-progressive-gray-positive", data);
        var gray = new byte[64];
        ImageCodec.DecodeJpeg(data, gray, JpegSampleFormat.Grayscale);
        for (var i = 0; i < 64; i++) Assert.Equal(expected[i * 4], gray[i]);
        var rgb = new byte[64 * 3];
        ImageCodec.DecodeJpeg(data, rgb, JpegSampleFormat.Rgb);
        for (var i = 0; i < 64; i++) Assert.Equal(expected.AsSpan(i * 4, 3).ToArray(), rgb.AsSpan(i * 3, 3).ToArray());
    }

    [Fact]
    public void ExactAdmissionIncludesCallerSamplesAndOrientationDoesNotRequireRgbaFrames()
    {
        const int width = 16, height = 8, pixels = width * height;
        var encoded = OriginalExif.Attach(new(AuthoredJpeg.GrayscaleDcGrid(width, height, false), null,
            width, height, MetadataProfile.JpegBaseline420), ImageOrientation.Rotate90, false);
        // Independent T.81 MCU geometry and the documented parser/table charges:
        // two 8x8 blocks, coefficient+sample overlap; destination is live throughout.
        const int tables = 2 * (204 + 512) + 12 + 176;
        var peak = encoded.Length + pixels + 1024 + 128 + tables + 256 + 128 + pixels * 5;
        var options = new JpegDecodeOptions { ApplyOrientation = true, Limits = new() { MaxWorkingBytes = peak } };
        var result = new byte[pixels];
        ImageCodec.DecodeJpeg(encoded, result, JpegSampleFormat.Grayscale, options: options);
        for (var y = 0; y < width; y++)
        for (var x = 0; x < height; x++) Assert.Equal(y / 8, result[y * height + x]);
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.DecodeJpeg(encoded, result,
            JpegSampleFormat.Grayscale, options: options with { Limits = options.Limits with { MaxWorkingBytes = peak - 1 } }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
        // The established RGBA/orientation path needs two full pixel results;
        // the direct path above fits its smaller, analytically derived allowance.
        Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(encoded,
            new() { ApplyOrientation = true, Limits = options.Limits }));
    }

    [Fact]
    public void InvalidLayoutsAndOverlapFailBeforeWritingAndDisjointSlicesAreAllowed()
    {
        var data = AuthoredJpeg.DirectRgb(); var unchanged = data.ToArray();
        var result = Enumerable.Repeat((byte)0xA7, 200).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.DecodeJpeg(data, result, (JpegSampleFormat)2));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.DecodeJpeg(data, result, JpegSampleFormat.Rgb, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.DecodeJpeg(data, result, JpegSampleFormat.Rgb, 23));
        Assert.Throws<ArgumentException>(() => ImageCodec.DecodeJpeg(data, result.AsSpan(0, 191), JpegSampleFormat.Rgb));
        Assert.Throws<ArgumentException>(() => ImageCodec.DecodeJpeg(data, result, JpegSampleFormat.Rgb, int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageCodec.DecodeJpeg(data, result, JpegSampleFormat.Rgb,
            options: new() { SourceColorSpace = (JpegColorSpace)99 }));
        Assert.Throws<ArgumentException>(() => ImageCodec.DecodeJpeg(data, data.AsSpan(), JpegSampleFormat.Rgb));
        Assert.Equal(unchanged, data); Assert.All(result, value => Assert.Equal(0xA7, value));
        var shared = new byte[data.Length + 192]; data.CopyTo(shared, 0);
        ImageCodec.DecodeJpeg(shared.AsSpan(0, data.Length), shared.AsSpan(data.Length), JpegSampleFormat.Rgb);
        Assert.Equal(data, shared.AsSpan(0, data.Length).ToArray());
        Assert.All(shared.AsSpan(data.Length).ToArray().Chunk(3), pixel => Assert.Equal(new byte[] { 45, 105, 205 }, pixel));
    }
}
