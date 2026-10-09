using System.Buffers.Binary;
using System.Text;

namespace Lokad.Image.Tests;

public sealed class ContainerHeaderTests
{
    private static byte[] Exif(int orientation)
        => [73, 73, 42, 0, 8, 0, 0, 0, 1, 0, 18, 1, 3, 0, 1, 0, 0, 0, (byte)orientation, 0, 0, 0, 0, 0, 0, 0];

    [Fact]
    public void JpegInspectHandlesStuffedBytesRestartsAndOrientation()
    {
        using var bytes = new MemoryStream(); bytes.Write(new byte[] { 255, 216 });
        Segment(bytes, 0xE1, "Exif\0\0"u8.ToArray().Concat(Exif(6)).ToArray());
        Segment(bytes, 0xC2, [8, 0, 2, 0, 3, 1, 1, 0x11, 0]);
        Segment(bytes, 0xDA, [1, 1, 0, 0, 0, 0]);
        bytes.Write(new byte[] { 1, 2, 255, 0, 3, 255, 208, 4, 255, 217 });
        var encoded = bytes.ToArray();
        var info = ImageCodec.Identify(encoded);
        Assert.Equal((3, 2, 2, 3), (info.Width, info.Height, info.OrientedWidth, info.OrientedHeight));
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(encoded.AsSpan(0, encoded.Length - 1)));
        Assert.Throws<ImageLimitException>(() => ImageCodec.Identify(encoded, new ImageLimits { MaxContainerItems = 1 }));
    }

    [Fact]
    public void WebpInspectChecksFlagsCanvasPaddingAndOrientation()
    {
        var extended = new byte[10]; extended[0] = 8; extended[4] = 2; extended[7] = 1;
        // Header-only VP8L, width 3 and height 2. Identify does not decode pixels.
        var pixels = new byte[5]; pixels[0] = 0x2F; BinaryPrimitives.WriteUInt32LittleEndian(pixels.AsSpan(1), 2u | 1u << 14);
        var data = Webp(("VP8X", extended), ("VP8L", pixels), ("EXIF", Exif(8)));
        var info = ImageCodec.Identify(data);
        Assert.Equal(ImageOrientation.Rotate270, info.Orientation);
        Assert.Equal((3, 2), (info.Width, info.Height));
        var badFlags = extended.ToArray(); badFlags[0] = 0;
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(Webp(("VP8X", badFlags), ("VP8L", pixels), ("EXIF", Exif(8)))));
        var badCanvas = extended.ToArray(); badCanvas[4] = 3;
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(Webp(("VP8X", badCanvas), ("VP8L", pixels), ("EXIF", Exif(8)))));
        var animated = extended.ToArray(); animated[0] |= 2;
        Assert.Throws<UnsupportedImageException>(() => ImageCodec.Identify(Webp(("VP8X", animated), ("VP8L", pixels))));
        var padding = Webp(("VP8L", pixels)); padding[^1] = 1;
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(padding));
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(Webp(("VP8L", pixels), ("VP8L", pixels))));
        var future = new byte[12]; extended.CopyTo(future, 0);
        future[0] |= 0xC1; future[1] = 17; future[2] = 23; future[3] = 45;
        future[10] = 67; future[11] = 89;
        Assert.Equal(info, ImageCodec.Identify(Webp(("VP8X", future), ("VP8L", pixels), ("EXIF", Exif(8)))));
        var oversizedCanvas = new byte[10]; oversizedCanvas.AsSpan(4, 6).Fill(255);
        Assert.Throws<ImageDataException>(() => ImageCodec.Identify(Webp(("VP8X", oversizedCanvas), ("VP8L", pixels))));
    }

    private static void Segment(Stream output, byte marker, byte[] payload)
    {
        output.WriteByte(255); output.WriteByte(marker);
        Span<byte> size = stackalloc byte[2]; BinaryPrimitives.WriteUInt16BigEndian(size, (ushort)(payload.Length + 2));
        output.Write(size); output.Write(payload);
    }

    private static byte[] Webp(params (string Type, byte[] Payload)[] chunks)
    {
        using var body = new MemoryStream(); body.Write("WEBP"u8);
        Span<byte> length = stackalloc byte[4];
        foreach (var (type, payload) in chunks)
        {
            body.Write(Encoding.ASCII.GetBytes(type));
            BinaryPrimitives.WriteInt32LittleEndian(length, payload.Length);
            body.Write(length); body.Write(payload); if ((payload.Length & 1) != 0) body.WriteByte(0);
        }
        using var output = new MemoryStream(); output.Write("RIFF"u8);
        Span<byte> size = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(size, (int)body.Length);
        output.Write(size); output.Write(body.ToArray()); return output.ToArray();
    }
}
