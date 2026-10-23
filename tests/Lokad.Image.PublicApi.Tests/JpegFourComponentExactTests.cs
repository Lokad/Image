using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegFourComponentExactTests
{
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task VaryingNativePixelsMatchArrayAndStreamsWithRetainedAdmission(bool ycck)
    {
        var data = Input(ycck); var original = data.ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = ImageLimits.Default with { MemoryBudget = root } };
        using (var retained = ImageCodec.Decode(data, options))
        {
            Assert.Equal(PixelHash(ycck), Hash(OriginalPixels.Bytes(retained)));
            Assert.Equal(65 * 31 * 4, root.Bytes);
            using (var input = new MemoryStream(data, writable: false))
            {
                using var sync = ImageCodec.Decode(input, options);
                Assert.Equal(PixelHash(ycck), Hash(OriginalPixels.Bytes(sync))); Assert.True(input.CanRead);
            }
            using (var input = new MemoryStream(data, writable: false))
            {
                using var asynchronous = await ImageCodec.DecodeAsync(input, options);
                Assert.Equal(PixelHash(ycck), Hash(OriginalPixels.Bytes(asynchronous))); Assert.True(input.CanRead);
            }
            Assert.Equal(original, data); Array.Clear(data);
            Assert.Equal(PixelHash(ycck), Hash(OriginalPixels.Bytes(retained)));
            Assert.Equal(65 * 31 * 4, root.Bytes); Assert.False(root.IsDisposed);
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void CallerRgbAndGrayMatchNativeWithPaddingAndOptInOrientation(bool ycck, bool orient)
    {
        var data = Input(ycck);
        var packet = OriginalExif.Attach(new(data, null, 65, 31, MetadataProfile.JpegBaseline420), ImageOrientation.Rotate90, true);
        var original = packet.ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new JpegDecodeOptions { ApplyOrientation = orient, Limits = ImageLimits.Default with { MemoryBudget = root } };
        var width = orient ? 31 : 65; var height = orient ? 65 : 31;
        var stride = width * 3 + 7; var output = Enumerable.Repeat((byte)165, stride * height + 11).ToArray();
        var info = ImageCodec.DecodeJpeg(packet, output, JpegSampleFormat.Rgb, stride, options);
        Assert.Equal((width, height, orient ? ImageOrientation.Normal : ImageOrientation.Rotate90), (info.Width, info.Height, info.Orientation));
        var rgba = new byte[65 * 31 * 4];
        for (var y = 0; y < 31; y++)
        for (var x = 0; x < 65; x++)
        {
            var dx = orient ? 30 - y : x; var dy = orient ? x : y;
            output.AsSpan(dy * stride + dx * 3, 3).CopyTo(rgba.AsSpan((y * 65 + x) * 4, 3));
            rgba[(y * 65 + x) * 4 + 3] = 255;
        }
        Assert.Equal(PixelHash(ycck), Hash(rgba));
        for (var y = 0; y < height; y++) Assert.All(output.AsSpan(y * stride + width * 3, 7).ToArray(), b => Assert.Equal(165, b));
        Assert.All(output.Skip(stride * height), b => Assert.Equal(165, b));
        stride = width + 7; var gray = Enumerable.Repeat((byte)165, stride * height + 11).ToArray();
        ImageCodec.DecodeJpeg(packet, gray, JpegSampleFormat.Grayscale, stride, options);
        var packed = new byte[65 * 31];
        for (var y = 0; y < 31; y++)
        for (var x = 0; x < 65; x++) packed[y * 65 + x] = gray[(orient ? x : y) * stride + (orient ? 30 - y : x)];
        Assert.Equal(ycck ? "04282e0fb862d9360eb9ed1138b3bd343b675818ee2bba2f292254b5e90bca15"
            : "34e9c10cdde85e632e61821274846ed642d1f5c575295f42eb212e5aa4c5aaad", Hash(packed));
        for (var y = 0; y < height; y++) Assert.All(gray.AsSpan(y * stride + width, 7).ToArray(), b => Assert.Equal(165, b));
        Assert.All(gray.Skip(stride * height), b => Assert.Equal(165, b));
        Assert.Equal(original, packet); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    internal static byte[] Input(bool ycck)
    {
        var data = AuthoredJpegCompatibility.Create(65, 31, [17, 191, 250, 63], ycck ? 2 : 1, ycck ? 2 : 1,
            ycck, ycck ? 2 : 0, restart: ycck ? 1 : 0, vary: true);
        Assert.Equal(ycck ? "eadc9fd3c13268da1978cb41ce31deae5b0f8b0fd04e57ee6077dd87c4e84728"
            : "ded06cb1a81aec05e8eff6f4dee6bdf46a019a10179f04b7d2be198a9389161b", Hash(data));
        return data;
    }

    internal static string NativeHash(byte[] data) => Hash(data) switch
    {
        "e6ec2c7bdabfdeaee6519365108b2b59a16e6cc20957d180a47885f1101159ff" => "b33e64062eaaa5bcdd31293b2df53d03c841fcf4ab6dbb04a7360819727067e3",
        "e5a5087914f95d39290819c320bb6ad82a4a0ea6e2e97b69205b63eb6ddfae96" => "edd938efb97f63eb23589b69cecac988083bdb6752bc5101f72582d70a4fe222",
        "eadc9fd3c13268da1978cb41ce31deae5b0f8b0fd04e57ee6077dd87c4e84728" => "adabc4449f31ac8f77fb941686c4cbbc1f178ba919d373220bdf1991a07b71cc",
        "18800b54ec100212cddc9013f71a866bc091b1bded4261969951c99345013339" => "a77ca3a0286c38caaa18b225944c367b7ef53d9c596466f840618f8ad64d8821",
        "cd67b21b16fc74c7d9bd744e02be1a5b71bede67a7efca515bcb76dad073443b" => "82bb422a37a94efc69bd0ac2b6b2bd2d1985aa38ef8232c353effee2258edae6",
        "bbc0bc0f43f81298d65d713015648fb2a2b75cdae8b46705ae6fbca0e00925a3" => "82bb422a37a94efc69bd0ac2b6b2bd2d1985aa38ef8232c353effee2258edae6",
        "1ab4709a317499cbf0517f4a033e5510581364c975aaad21be22406dfdf28753" => "97612f0764dc27a90a2829f1caae65ef25758b2d83516bc98993e1304f7c1f0b",
        "7354a9d0740a6a3e27e2819b1c08440a6fb99ecdfe52fb92ed3d1e6855f9225d" => "d5697f2a925d35ec7972e5b5fa8f7396f0c1de0f84bbaca1e71313f0ed43a5ab",
        _ => throw new InvalidOperationException("Original four-component control provenance differs.")
    };

    internal static void AssertNative(byte[] data, byte[] rgba) => Assert.Equal(NativeHash(data), Hash(rgba));

    private static string PixelHash(bool ycck) => ycck ? "adabc4449f31ac8f77fb941686c4cbbc1f178ba919d373220bdf1991a07b71cc"
        : "8deae32e317740b58b94ddcbbb0270c6437616391d9d559f25aef7984413fd44";
    private static string Hash(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
