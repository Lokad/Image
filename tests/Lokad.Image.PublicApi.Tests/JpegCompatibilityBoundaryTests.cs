using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCompatibilityBoundaryTests
{
    [Theory]
    [InlineData(false, 1)] [InlineData(true, 1)]
    [InlineData(false, 2)] [InlineData(true, 2)]
    public void QuarterWidthChromaMatchesNativePixelsAndResourceBounds(bool progressive, int vertical)
    {
        const int width = 67, height = 19;
        byte[] values = [60, 105, 145];
        var data = AuthoredJpegCompatibility.Create(width, height, values, 4, vertical,
            progressive: progressive, restart: 1, vary: true);
        var original = data.ToArray();
        using var owned = ImageCodec.Decode(data);
        var rgb = new byte[width * height * 3]; ImageCodec.DecodeJpeg(data, rgb, JpegSampleFormat.Rgb);
        JpegQuarterExactTests.AssertNative(OriginalPixels.Bytes(owned), vertical);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            Assert.Equal(owned.GetRow8(y).Slice(x * 4, 3).ToArray(), rgb.AsSpan((y * width + x) * 3, 3).ToArray());
            Assert.Equal(255, owned.GetRow8(y)[x * 4 + 3]);
        }
        var expectedBytes = OriginalPixels.Bytes(owned);
        var budget = new ImageBudget(16 * 1024 * 1024, 1_000_000_000);
        using (var survivor = budget.Decode(data, new() { Limits = new() { MaxWorkingBytes = budget.MaxWorkingBytes, MaxWork = budget.MaxWork } }))
        {
            owned.GetRow8(0)[0] ^= 255; owned.Dispose();
            Assert.Equal(expectedBytes, OriginalPixels.Bytes(survivor.Image));
            Assert.Equal(budget.MaxWorkingBytes - expectedBytes.Length, budget.RemainingWorkingBytes);
        }
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes);
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(data,
            new() { Limits = new() { MaxWork = budget.ConsumedWork - 1 } }));
        Assert.Equal(ImageLimitKind.Work, failure.Limit);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var canceled = Assert.Throws<OperationCanceledException>(() => ImageCodec.Decode(data, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, canceled.CancellationToken);
        Assert.Equal(original, data);
    }

    [Theory]
    [InlineData(ImageOrientation.Normal)] [InlineData(ImageOrientation.MirrorHorizontal)]
    [InlineData(ImageOrientation.Rotate180)] [InlineData(ImageOrientation.MirrorVertical)]
    [InlineData(ImageOrientation.Transpose)] [InlineData(ImageOrientation.Rotate90)]
    [InlineData(ImageOrientation.Transverse)] [InlineData(ImageOrientation.Rotate270)]
    public void CallerCmykOrientationPreservesPaddingAndIndependentLifetimes(ImageOrientation orientation)
    {
        const int width = 17, height = 9;
        var plain = AuthoredJpegCompatibility.Create(width, height, [220, 180, 100, 200], adobe: 0, vary: true);
        var data = OriginalExif.Attach(new(plain, null, width, height, MetadataProfile.JpegBaseline420), orientation, false);
        var original = data.ToArray(); var raw = new byte[width * height * 4];
        var info = ImageCodec.DecodeJpeg(data, raw, JpegSampleFormat.Cmyk);
        Assert.Equal(orientation, info.Orientation);
        var expected = OriginalExif.Permute(raw, width, height, 8, orientation);
        var w = (int)orientation >= 5 ? height : width; var h = (int)orientation >= 5 ? width : height;
        var stride = w * 4 + 3; var result = Enumerable.Repeat((byte)0xA7, stride * h + 5).ToArray();
        var options = new JpegDecodeOptions { ApplyOrientation = true };
        var actual = ImageCodec.DecodeJpeg(data, result, JpegSampleFormat.Cmyk, stride, options);
        Assert.Equal((w, h, ImageOrientation.Normal), (actual.Width, actual.Height, actual.Orientation));
        for (var y = 0; y < h; y++)
        {
            Assert.Equal(expected.AsSpan(y * w * 4, w * 4).ToArray(), result.AsSpan(y * stride, w * 4).ToArray());
            Assert.All(result.AsSpan(y * stride + w * 4, 3).ToArray(), sample => Assert.Equal(0xA7, sample));
        }
        Assert.All(result.AsSpan(stride * h).ToArray(), sample => Assert.Equal(0xA7, sample));
        using var source = ImageCodec.Decode(data);
        using var rotated = ImageTransforms.AutoOrient(source);
        using var directlyOriented = ImageCodec.Decode(data, new() { ApplyOrientation = true });
        Assert.Equal(OriginalPixels.Bytes(rotated), OriginalPixels.Bytes(directlyOriented));
        raw.AsSpan().Clear(); Assert.Equal(expected.AsSpan(0, w * 4).ToArray(), result.AsSpan(0, w * 4).ToArray());
        Assert.Equal(original, data);
    }

    [Theory]
    [InlineData(0)] [InlineData(255)]
    public void BlackComponentIsColorAndNeverTransparency(byte black)
    {
        var data = AuthoredJpegCompatibility.Create(2, 1, [75, 110, 190, black], adobe: 0);
        using var result = ImageCodec.Decode(data);
        // The pinned Q16 reciprocal keeps opaque alpha even at the black endpoints.
        byte[] expected = black == 0 ? [0, 0, 0, 255] : [74, 109, 189, 255];
        Assert.Equal(expected, result.GetRow8(0)[..4].ToArray());
    }

    [Fact]
    public void FourComponentAdmissionHasExactBoundsAndSharedCallerStorageOwnership()
    {
        const int width = 16, height = 8, pixels = width * height;
        var data = AuthoredJpegCompatibility.Create(width, height, [220, 180, 100, 200], adobe: 0);
        const long tables = 2 * (204 + 512) + 12 + 1;
        var peak = data.Length + pixels * 4L + 1024 + 128 + tables + 4 * (256 + 128) + pixels * 17L;
        var options = new JpegDecodeOptions { Limits = new() { MaxWorkingBytes = peak } };
        var result = new byte[pixels * 4];
        ImageCodec.DecodeJpeg(data, result, JpegSampleFormat.Cmyk, options: options);
        var expected = result.ToArray();
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.DecodeJpeg(data, result, JpegSampleFormat.Cmyk,
            options: options with { Limits = options.Limits with { MaxWorkingBytes = peak - 1 } }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
        var budget = new ImageBudget(peak + result.Length, options.Limits.MaxWork);
        using (budget.Reserve(result.Length))
        {
            budget.DecodeJpeg(data, result, JpegSampleFormat.Cmyk, options: options);
            Assert.Equal(peak, budget.RemainingWorkingBytes);
        }
        Assert.Equal(budget.MaxWorkingBytes, budget.RemainingWorkingBytes); Assert.Equal(expected, result);
        Assert.True(budget.ConsumedWork > 0); Assert.Equal(budget.MaxWork - budget.ConsumedWork, budget.RemainingWork);
        result.AsSpan().Fill(0xA7);
        var work = Assert.Throws<ImageLimitException>(() => ImageCodec.DecodeJpeg(data, result, JpegSampleFormat.Cmyk,
            options: options with { Limits = options.Limits with { MaxWork = budget.ConsumedWork - 1 } }));
        Assert.Equal(ImageLimitKind.Work, work.Limit);
        Assert.All(result.AsSpan((height - 1) * width * 4).ToArray(), sample => Assert.Equal(0xA7, sample));
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var canceled = Assert.Throws<OperationCanceledException>(() => ImageCodec.DecodeJpeg(data, result, JpegSampleFormat.Cmyk,
            options: options, cancellationToken: cancellation.Token)); Assert.Equal(cancellation.Token, canceled.CancellationToken);
        ImageCodec.DecodeJpeg(data, result, JpegSampleFormat.Cmyk, options: options); Assert.Equal(expected, result);
    }

}
