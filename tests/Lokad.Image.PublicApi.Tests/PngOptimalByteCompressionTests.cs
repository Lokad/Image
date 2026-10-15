using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngOptimalByteCompressionTests
{
    [Theory]
    [InlineData(PngColorType.Grayscale, 72304)]
    [InlineData(PngColorType.Rgb, 168159)]
    [InlineData(PngColorType.GrayscaleAlpha, 119940)]
    public void DefaultCompressionPreservesSamplesOwnersAndComparablePayload(PngColorType color, int referenceIdat)
    {
        // Existing asymmetric-v1 raster and documented PNG projections. The
        // reference sizes come from FFmpeg level6/None on these same scanlines.
        using var source = OriginalPixels.Buffer(513, 259, 8,
            grayscale: color != PngColorType.Rgb, opaque: color != PngColorType.GrayscaleAlpha);
        if (color == PngColorType.GrayscaleAlpha)
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
        Assert.InRange(Math.Abs(payload - (double)referenceIdat) / Math.Max(payload, referenceIdat), 0, .05);
        using var decoded = ImageCodec.Decode(png);
        Assert.Equal(expected, OriginalPixels.Bytes(decoded));
        Assert.Equal(expected, OriginalPixels.Bytes(source));
        first.GetBuffer()[first.Length / 2] ^= 255;
        Assert.Equal(png, second.ToArray());
    }
}
