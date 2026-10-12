using System.Buffers.Binary;
using System.IO.Compression;

// Original fixture authoring, PNG sections 7/9/11, https://www.w3.org/TR/png-3/.
// One existing raster and one explicit palette projection; no corpus or sweep.
internal static class WarmPngPaletteInput
{
    internal static void Project(Span<byte> pixels)
    {
        if (pixels.Length != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4)
            throw new ArgumentException("Expected the existing RGBA8 raster.");
        for (var i = 0; i < pixels.Length; i += 4)
        {
            var index = pixels[i];
            pixels[i + 1] = (byte)(255 - index);
            pixels[i + 2] = unchecked((byte)(index * 73 + 19));
            pixels[i + 3] = index;
        }
    }

    internal static byte[] Build(ReadOnlySpan<byte> pixels)
    {
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        if (pixels.Length != width * height * 4) throw new ArgumentException("Expected the projected RGBA8 raster.");
        using var compressed = new MemoryStream(); var row = new byte[width];
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var offset = (y * width + x) * 4; var index = pixels[offset];
                    if (pixels[offset + 1] != 255 - index || pixels[offset + 2] != unchecked((byte)(index * 73 + 19))
                        || pixels[offset + 3] != index) throw new ArgumentException("Palette projection differs.");
                    row[x] = index;
                }
                zlib.WriteByte(0); zlib.Write(row);
            }
        }
        var palette = new byte[768]; var alpha = new byte[256];
        for (var i = 0; i < 256; i++)
        {
            palette[i * 3] = (byte)i; palette[i * 3 + 1] = (byte)(255 - i);
            palette[i * 3 + 2] = unchecked((byte)(i * 73 + 19)); alpha[i] = (byte)i;
        }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = 8; header[9] = 3;
        WarmPngAdam7Input.Chunk(output, "IHDR"u8, header);
        WarmPngAdam7Input.Chunk(output, "PLTE"u8, palette); WarmPngAdam7Input.Chunk(output, "tRNS"u8, alpha);
        WarmPngAdam7Input.Chunk(output, "IDAT"u8, compressed.ToArray()); WarmPngAdam7Input.Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }
}
