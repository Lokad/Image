using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngOptimalByteCompressionTests
{
    [Theory]
    [InlineData(PngColorType.Grayscale, 72586)]
    [InlineData(PngColorType.Rgb, 168729)]
    [InlineData(PngColorType.GrayscaleAlpha, 120366)]
    [InlineData(PngColorType.Rgba, 210973)]
    public async Task DefaultCompressionPreservesSamplesOwnersAndCompletePacketSize(PngColorType color, int referenceBytes)
    {
        // Existing asymmetric-v1 raster and documented PNG projections. The
        // reference sizes come from FFmpeg level6/None on these same scanlines.
        using var source = OriginalPixels.Buffer(513, 259, 8,
            grayscale: color is PngColorType.Grayscale or PngColorType.GrayscaleAlpha,
            opaque: color is PngColorType.Grayscale or PngColorType.Rgb);
        if (color is PngColorType.GrayscaleAlpha or PngColorType.Rgba)
            source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var expected = OriginalPixels.Bytes(source);
        using var first = new MemoryStream(); using var second = new MemoryStream();
        var options = new PngEncodeOptions { ColorType = color };
        ImageCodec.EncodePng(source, first, options); ImageCodec.EncodePng(source, second, options);
        var png = first.ToArray(); Assert.Equal(png, second.ToArray());
        var payload = 0;
        for (var p = 8; p < png.Length;)
        {
            var n = checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(p)));
            if (png.AsSpan(p + 4, 4).SequenceEqual("IDAT"u8)) payload += n;
            p = checked(p + n + 12);
        }
        Assert.True(payload > 0); Assert.InRange(png.Length, 1, referenceBytes);
        using var asynchronous = new MemoryStream(); await ImageCodec.EncodePngAsync(source, asynchronous, options);
        Assert.Equal(png, asynchronous.ToArray());
        using var decoded = ImageCodec.Decode(png);
        Assert.Equal(expected, OriginalPixels.Bytes(decoded));
        Assert.Equal(expected, OriginalPixels.Bytes(source));
        first.GetBuffer()[first.Length / 2] ^= 255;
        Assert.Equal(png, second.ToArray());
    }
}
