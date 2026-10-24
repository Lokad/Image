using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegDirectRgbExactTests
{
    [Theory] [InlineData(422)] [InlineData(420)]
    public async Task OriginalDirectRgbPixelsMatchAcrossArraysAndStreams(int sampling)
    {
        var control = OriginalDirectRgbControls.Load(sampling); var data = control.Encoded; var original = data.ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = ImageLimits.Default with { MemoryBudget = root } };
        using (var retained = ImageCodec.Decode(data, options))
        {
            Assert.Equal(control.RgbaSha256, OriginalDirectRgbControls.Hash(OriginalPixels.Bytes(retained)));
            Assert.Equal(513 * 259 * 4, root.Bytes);
            using (var input = new MemoryStream(data, writable: false))
            {
                using var streamed = ImageCodec.Decode(input, options);
                Assert.Equal(control.RgbaSha256, OriginalDirectRgbControls.Hash(OriginalPixels.Bytes(streamed))); Assert.True(input.CanRead);
            }
            using (var input = new MemoryStream(data, writable: false))
            {
                using var asynchronous = await ImageCodec.DecodeAsync(input, options);
                Assert.Equal(control.RgbaSha256, OriginalDirectRgbControls.Hash(OriginalPixels.Bytes(asynchronous))); Assert.True(input.CanRead);
            }
            Assert.Equal(original, data); Array.Clear(data);
            Assert.Equal(control.RgbaSha256, OriginalDirectRgbControls.Hash(OriginalPixels.Bytes(retained)));
            Assert.Equal(513 * 259 * 4, root.Bytes); Assert.False(root.IsDisposed);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(422, false)] [InlineData(422, true)]
    [InlineData(420, false)] [InlineData(420, true)]
    public void CallerRgbAndGrayMatchNativeWithOrientationPaddingAndBorrowedAdmission(int sampling, bool orient)
    {
        var control = OriginalDirectRgbControls.Load(sampling);
        var data = OriginalExif.Attach(new(control.Encoded, null, 513, 259, MetadataProfile.JpegBaseline420), ImageOrientation.Rotate90, true);
        var original = data.ToArray(); using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new JpegDecodeOptions { ApplyOrientation = orient, Limits = ImageLimits.Default with { MemoryBudget = root } };
        var width = orient ? 259 : 513; var height = orient ? 513 : 259;
        foreach (var format in new[] { JpegSampleFormat.Rgb, JpegSampleFormat.Grayscale })
        {
            var channels = (int)format; var stride = width * channels + 7;
            var destination = Enumerable.Repeat((byte)165, stride * height + 11).ToArray();
            var info = ImageCodec.DecodeJpeg(data, destination, format, stride, options);
            Assert.Equal((width, height, orient ? ImageOrientation.Normal : ImageOrientation.Rotate90), (info.Width, info.Height, info.Orientation));
            var packed = new byte[513 * 259 * channels];
            for (var y = 0; y < 259; y++)
            for (var x = 0; x < 513; x++)
            {
                var dx = orient ? 258 - y : x; var dy = orient ? x : y;
                destination.AsSpan(dy * stride + dx * channels, channels).CopyTo(packed.AsSpan((y * 513 + x) * channels, channels));
            }
            Assert.Equal(format == JpegSampleFormat.Rgb ? control.RgbSha256 : control.GraySha256, OriginalDirectRgbControls.Hash(packed));
            for (var y = 0; y < height; y++) Assert.All(destination.AsSpan(y * stride + width * channels, 7).ToArray(), b => Assert.Equal(165, b));
            Assert.All(destination.Skip(stride * height), b => Assert.Equal(165, b));
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
        Assert.Equal(original, data);
    }

    [Theory] [InlineData(422)] [InlineData(420)]
    public void CallerGrayFitsTheRgbPlaneAdmissionPeakWithoutAnRgbaIntermediate(int sampling)
    {
        var control = OriginalDirectRgbControls.Load(sampling);
        using var reference = new TestMemoryBudget(4 * 1024 * 1024);
        var rgb = new byte[513 * 259 * 3];
        ImageCodec.DecodeJpeg(control.Encoded, rgb, JpegSampleFormat.Rgb,
            options: new() { Limits = ImageLimits.Default with { MemoryBudget = reference } });
        Assert.Equal(0, reference.Bytes);
        using var root = new TestMemoryBudget(reference.Peak);
        var gray = new byte[513 * 259];
        ImageCodec.DecodeJpeg(control.Encoded, gray, JpegSampleFormat.Grayscale,
            options: new() { Limits = ImageLimits.Default with { MemoryBudget = root } });
        Assert.Equal(control.GraySha256, OriginalDirectRgbControls.Hash(gray));
        Assert.True(root.Peak <= reference.Peak); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

}
