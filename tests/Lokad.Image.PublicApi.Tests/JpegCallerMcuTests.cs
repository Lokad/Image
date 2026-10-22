namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegCallerMcuTests
{
    [Fact]
    public void LargePaddedRgbSamplesProduceExactlyTheOpaqueRgbaPacket()
    {
        const int width = 513, height = 259, stride = width * 3 + 7;
        using var source = ImageBuffer.Create(width, height, 8);
        var samples = Enumerable.Repeat((byte)213, stride * height).ToArray();
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var rgba = source.GetRow8(y).Slice(x * 4, 4); var rgb = samples.AsSpan(y * stride + x * 3, 3);
            rgba[0] = rgb[0] = (byte)(x * 37 + y * 11); rgba[1] = rgb[1] = (byte)(x * 3 + y * 61);
            rgba[2] = rgb[2] = (byte)(x * 23 + y * 7); rgba[3] = 255;
        }
        // Explicit background admits adaptive coding for the RGBA route too;
        // default null selects its fixed-table path, even for opaque pixels.
        var unchanged = samples.ToArray(); var options = new JpegEncodeOptions { QuantizationPolicy = JpegQuantizationPolicy.Original, Quality = 85, Sampling = JpegSampling.YCbCr420, BackgroundRgb = 0 };
        using var rgbaPacket = new MemoryStream(); using var rgbPacket = new MemoryStream();
        ImageCodec.EncodeJpeg(source, rgbaPacket, options);
        ImageCodec.EncodeJpeg(samples, width, height, JpegSampleFormat.Rgb, rgbPacket, stride, options);
        Assert.Equal(rgbaPacket.ToArray(), rgbPacket.ToArray()); Assert.Equal(unchanged, samples);
        // Original padded RGB pattern after bounded 4:2:0 AC path selection;
        // this is packet stability, not a matched-quality compression claim.
        Assert.Equal("3988bf5f87d394c24b7ca2e728562b06f7a4b65f3eb60e9bcb8e6f728c391713",
            Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(rgbPacket.ToArray())));
    }
}
