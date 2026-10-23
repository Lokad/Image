using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegExactDecodeTests
{
    [Fact]
    public async Task FrozenFullResolutionPixelsMatchAcrossArrayAndStreamBoundaries()
    {
        var (encoded, expected) = Load(); var original = encoded.ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = ImageLimits.Default with { MemoryBudget = root } };
        using (var retained = ImageCodec.Decode(encoded, options))
        {
            Assert.Equal(expected, OriginalPixels.Bytes(retained));
            Assert.Equal(expected.LongLength, root.Bytes);
            using (var input = new MemoryStream(encoded, writable: false))
            {
                using var streamed = ImageCodec.Decode(input, options);
                Assert.Equal(expected, OriginalPixels.Bytes(streamed)); Assert.True(input.CanRead);
            }
            using (var input = new MemoryStream(encoded, writable: false))
            {
                using var token = new CancellationTokenSource();
                using var asynchronous = await ImageCodec.DecodeAsync(input, options, token.Token);
                Assert.Equal(expected, OriginalPixels.Bytes(asynchronous)); Assert.True(input.CanRead);
            }
            Assert.Equal(original, encoded); Array.Clear(encoded);
            Assert.Equal(expected, OriginalPixels.Bytes(retained));
            Assert.Equal(expected.LongLength, root.Bytes); Assert.False(root.IsDisposed);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public void CallerRgbMatchesNativePixelsWithPaddingAndOptInOrientation(bool orient)
    {
        var (data, native) = Load();
        var encoded = OriginalExif.Attach(new(data, null, 31, 23, MetadataProfile.JpegBaseline420), ImageOrientation.Rotate90, true);
        var original = encoded.ToArray();
        var expected = orient ? OriginalExif.Permute(native, 31, 23, 8, ImageOrientation.Rotate90) : native;
        var width = orient ? 23 : 31; var height = orient ? 31 : 23; var stride = width * 3 + 5;
        var output = Enumerable.Repeat((byte)165, stride * height + 7).ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var info = ImageCodec.DecodeJpeg(encoded, output, JpegSampleFormat.Rgb, stride,
            new() { ApplyOrientation = orient, Limits = ImageLimits.Default with { MemoryBudget = root } });
        Assert.Equal((width, height, orient ? ImageOrientation.Normal : ImageOrientation.Rotate90), (info.Width, info.Height, info.Orientation));
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                Assert.Equal(expected.AsSpan((y * width + x) * 4, 3).ToArray(), output.AsSpan(y * stride + x * 3, 3).ToArray());
            Assert.All(output.AsSpan(y * stride + width * 3, 5).ToArray(), value => Assert.Equal(165, value));
        }
        Assert.All(output.Skip(stride * height), value => Assert.Equal(165, value));
        Assert.Equal(original, encoded); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CallerGrayscaleMatchesTheExactNativeLumaPlane()
    {
        var (encoded, _) = Load(); var output = new byte[31 * 23];
        var info = ImageCodec.DecodeJpeg(encoded, output, JpegSampleFormat.Grayscale);
        Assert.Equal((31, 23, 8), (info.Width, info.Height, info.BitDepth));
        Assert.Equal("7c208d4a6353d69de6f9da697fe23efe603fef3fb2499568c47bd08f52752a98", Hash(output));
    }

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static (byte[] Encoded, byte[] Native) Load()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "tests", "Fixtures", "jpeg-manifest.json")))
            directory = directory.Parent;
        Assert.NotNull(directory);
        var prefix = Path.Combine(directory.FullName, "tests", "Fixtures", "jpeg-ffmpeg-YCbCr444-31x23");
        var encoded = File.ReadAllBytes(prefix + ".jpg"); var native = File.ReadAllBytes(prefix + ".rgba");
        Assert.Equal("ad1d9d11e42273349e85cfebc9b73c89231b595b883cd25d2b7832f62c353573", Hash(encoded));
        Assert.Equal("fbf313b3c7bc442832b67d3b08bf228da503aefc142d4ac2ed8d2bfc720b76e4", Hash(native));
        return (encoded, native);
    }
}
