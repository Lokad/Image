using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegQuarterExactTests
{
    [Theory] [InlineData(1)] [InlineData(2)]
    public async Task VaryingNativePixelsMatchArrayAndStreamsWithRetainedAdmission(int vertical)
    {
        var data = Input(vertical); var original = data.ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = ImageLimits.Default with { MemoryBudget = root } };
        using (var retained = ImageCodec.Decode(data, options))
        {
            Assert.Equal(PixelHash(vertical), Hash(OriginalPixels.Bytes(retained)));
            Assert.Equal(67 * 19 * 4, root.Bytes);
            using (var input = new MemoryStream(data, writable: false))
            {
                using var sync = ImageCodec.Decode(input, options);
                Assert.Equal(PixelHash(vertical), Hash(OriginalPixels.Bytes(sync))); Assert.True(input.CanRead);
            }
            using (var input = new MemoryStream(data, writable: false))
            {
                using var asynchronous = await ImageCodec.DecodeAsync(input, options);
                Assert.Equal(PixelHash(vertical), Hash(OriginalPixels.Bytes(asynchronous))); Assert.True(input.CanRead);
            }
            Assert.Equal(original, data); Array.Clear(data);
            Assert.Equal(PixelHash(vertical), Hash(OriginalPixels.Bytes(retained)));
            Assert.Equal(67 * 19 * 4, root.Bytes); Assert.False(root.IsDisposed);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1, false)] [InlineData(1, true)]
    [InlineData(2, false)] [InlineData(2, true)]
    public void CallerRgbAndGrayMatchNativeWithPaddingAndOptInOrientation(int vertical, bool orient)
    {
        var data = Input(vertical);
        var packet = OriginalExif.Attach(new(data, null, 67, 19, MetadataProfile.JpegBaseline420), ImageOrientation.Rotate90, true);
        var original = packet.ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new JpegDecodeOptions { ApplyOrientation = orient, Limits = ImageLimits.Default with { MemoryBudget = root } };
        var width = orient ? 19 : 67; var height = orient ? 67 : 19;
        var stride = width * 3 + 7; var output = Enumerable.Repeat((byte)165, stride * height + 11).ToArray();
        var info = ImageCodec.DecodeJpeg(packet, output, JpegSampleFormat.Rgb, stride, options);
        Assert.Equal((width, height, orient ? ImageOrientation.Normal : ImageOrientation.Rotate90), (info.Width, info.Height, info.Orientation));
        var rgba = new byte[67 * 19 * 4];
        for (var y = 0; y < 19; y++)
        for (var x = 0; x < 67; x++)
        {
            var dx = orient ? 18 - y : x; var dy = orient ? x : y;
            output.AsSpan(dy * stride + dx * 3, 3).CopyTo(rgba.AsSpan((y * 67 + x) * 4, 3));
            rgba[(y * 67 + x) * 4 + 3] = 255;
        }
        Assert.Equal(PixelHash(vertical), Hash(rgba));
        for (var y = 0; y < height; y++) Assert.All(output.AsSpan(y * stride + width * 3, 7).ToArray(), b => Assert.Equal(165, b));
        Assert.All(output.Skip(stride * height), b => Assert.Equal(165, b));
        stride = width + 7; var gray = Enumerable.Repeat((byte)165, stride * height + 11).ToArray();
        ImageCodec.DecodeJpeg(packet, gray, JpegSampleFormat.Grayscale, stride, options);
        var packed = new byte[67 * 19];
        for (var y = 0; y < 19; y++)
        for (var x = 0; x < 67; x++) packed[y * 67 + x] = gray[(orient ? x : y) * stride + (orient ? 18 - y : x)];
        Assert.Equal("373d0d5705ccba1e7ce7640e541c3fd1f37457dcaf5bbb28ec863e3485d1d167", Hash(packed));
        for (var y = 0; y < height; y++) Assert.All(gray.AsSpan(y * stride + width, 7).ToArray(), b => Assert.Equal(165, b));
        Assert.All(gray.Skip(stride * height), b => Assert.Equal(165, b));
        Assert.Equal(original, packet); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void NearIntegralLeadingTapIsRemovedBeforeColorQuantization()
    {
        var data = AuthoredJpegCompatibility.Create(513, 259, [128, 255, 128], 4, 2, restart: 1,
            blockSample: (c, x, y) => c == 1 ? (x < 16 ? 255 : 139) : 128);
        Assert.Equal("3cb32bf010f217fbe8320a2f6b138b80d3f49f9fe5117d0b39f7c47ae3e40638", Hash(data));
        using var image = ImageCodec.Decode(data);
        Assert.Equal("8770ce38bb893d8a545fe21e0f68885be77c8ab6b6e356ec1d4ed9c6ac0414d1", Hash(OriginalPixels.Bytes(image)));
        var rgb = new byte[513 * 259 * 3]; ImageCodec.DecodeJpeg(data, rgb, JpegSampleFormat.Rgb);
        for (var y = 0; y < 259; y++)
        for (var x = 0; x < 513; x++)
            Assert.Equal(image.GetRow8(y).Slice(x * 4, 3).ToArray(), rgb.AsSpan((y * 513 + x) * 3, 3).ToArray());
    }

    internal static byte[] Input(int vertical)
    {
        var data = AuthoredJpegCompatibility.Create(67, 19, [60, 105, 145], 4, vertical, restart: 1, vary: true);
        Assert.Equal(vertical == 1 ? "7d981d0a8234388240b3639e83a6abcafab10f75dce4517da283cd68a87e1c14"
            : "c63426bb37e723fddbf112e236af1fb5e88fb00b836d1280591d02f55d74b32a", Hash(data));
        return data;
    }

    internal static void AssertNative(byte[] pixels, int vertical) => Assert.Equal(PixelHash(vertical), Hash(pixels));
    private static string PixelHash(int vertical) => vertical == 1 ? "da994d5e09fdd9775c14b64341bd5684ee2a5f7675f46750f57f8d027b9b3c3f"
        : "5cce3dca30a43f41ddfe87cc42ec1b42ee8eaec3132dc2e0bc702e4ee7b49fbd";
    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
