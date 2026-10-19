using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCallerRgb420Tests
{
    [Fact]
    public void ExifOrientationRemainsOptInForWideCallerRgb()
    {
        const int width = 65, height = 31;
        using var source = OriginalPixels.Buffer(width, height, 8, opaque: true);
        using var output = new MemoryStream(); ImageCodec.EncodeJpeg(source, output, new() { Quality = 85, Sampling = JpegSampling.YCbCr420 });
        var encoded = OriginalExif.Attach(new(output.ToArray(), null, width, height, MetadataProfile.JpegBaseline420), ImageOrientation.Rotate90, true);
        var raw = new byte[width * height * 3]; var oriented = new byte[raw.Length];
        var rawInfo = ImageCodec.DecodeJpeg(encoded, raw, JpegSampleFormat.Rgb);
        var orientedInfo = ImageCodec.DecodeJpeg(encoded, oriented, JpegSampleFormat.Rgb, options: new() { ApplyOrientation = true });
        Assert.Equal((width, height, ImageOrientation.Rotate90), (rawInfo.Width, rawInfo.Height, rawInfo.Orientation));
        Assert.Equal((height, width, ImageOrientation.Normal), (orientedInfo.Width, orientedInfo.Height, orientedInfo.Orientation));
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            Assert.Equal(raw.AsSpan((y * width + x) * 3, 3).ToArray(), oriented.AsSpan((x * height + height - 1 - y) * 3, 3).ToArray());
    }

    [Theory]
    [InlineData(32, 17, false)] [InlineData(33, 17, true)]
    [InlineData(65, 31, false)] [InlineData(513, 259, false)]
    public void PackedAndPaddedSamplesKeepQualifiedPixelsAndIndependentOwners(int width, int height, bool progressive)
    {
        byte[] encoded;
        if (progressive) encoded = AuthoredJpeg.ProgressiveFrame(width, height, JpegSampling.YCbCr420, 4);
        else
        {
            using var image = OriginalPixels.Buffer(width, height, 8, opaque: true);
            using var output = new MemoryStream(); ImageCodec.EncodeJpeg(image, output, new() { Quality = 85, Sampling = JpegSampling.YCbCr420 });
            encoded = output.ToArray();
        }
        var original = encoded.ToArray(); var stride = width * 3 + 7;
        using var established = ImageCodec.Decode(encoded);
        var padded = Enumerable.Repeat((byte)165, stride * height + 17).ToArray();
        var info = ImageCodec.DecodeJpeg(encoded, padded, JpegSampleFormat.Rgb, stride);
        Assert.Equal((width, height, 8, false), (info.Width, info.Height, info.BitDepth, info.HasAlpha));
        using var token = new CancellationTokenSource();
        var survivor = new byte[width * height * 3]; ImageCodec.DecodeJpeg(encoded, survivor, JpegSampleFormat.Rgb, cancellationToken: token.Token);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var expected = established.GetRow8(y).Slice(x * 4, 3).ToArray();
                Assert.Equal(expected, padded.AsSpan(y * stride + x * 3, 3).ToArray());
                Assert.Equal(expected, survivor.AsSpan((y * width + x) * 3, 3).ToArray());
            }
            Assert.All(padded.AsSpan(y * stride + width * 3, 7).ToArray(), value => Assert.Equal(165, value));
        }
        Assert.All(padded.AsSpan(stride * height).ToArray(), value => Assert.Equal(165, value));
        padded[0] ^= 255; Array.Clear(padded);
        Assert.Equal(established.GetRow8(0).Slice(0, 3).ToArray(), survivor.AsSpan(0, 3).ToArray());
        Assert.Equal(original, encoded); Array.Clear(encoded);
        Assert.Equal(established.GetRow8(height - 1).Slice((width - 1) * 4, 3).ToArray(), survivor.AsSpan(survivor.Length - 3).ToArray());
    }
}
