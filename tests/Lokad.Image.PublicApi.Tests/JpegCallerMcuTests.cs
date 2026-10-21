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
        var unchanged = samples.ToArray(); var options = new JpegEncodeOptions { Quality = 85, Sampling = JpegSampling.YCbCr420, BackgroundRgb = 0 };
        using var rgbaPacket = new MemoryStream(); using var rgbPacket = new MemoryStream();
        ImageCodec.EncodeJpeg(source, rgbaPacket, options);
        ImageCodec.EncodeJpeg(samples, width, height, JpegSampleFormat.Rgb, rgbPacket, stride, options);
        Assert.Equal(rgbaPacket.ToArray(), rgbPacket.ToArray()); Assert.Equal(unchanged, samples);
        // Original padded RGB pattern, requalified after source-DCT AC decisions; no corpus bytes.
        Assert.Equal("5e402e6caefe7cf182980fc22b600f2319fea42755f494dbea94719abbf9853d",
            Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(rgbPacket.ToArray())));
    }
}
