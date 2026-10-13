using System.Buffers.Binary;
using System.IO.Compression;

// Original fixture authoring, PNG sections 7/9/11: gray8 with a tRNS key.
// Existing raster's red channel only; no new original image or profile sweep.
internal static class WarmPngGrayTransparencyInput
{
    internal const ushort Key = 127;

    internal static void Project(Span<byte> pixels)
    {
        if (pixels.Length != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4)
            throw new ArgumentException("Expected the existing RGBA8 raster.");
        for (var i = 0; i < pixels.Length; i += 4)
        {
            var gray = pixels[i]; pixels[i + 1] = gray; pixels[i + 2] = gray;
            pixels[i + 3] = gray == Key ? (byte)0 : (byte)255;
        }
    }

    internal static byte[] Build(ReadOnlySpan<byte> pixels)
    {
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        if (pixels.Length != width * height * 4) throw new ArgumentException("Expected projected gray pixels.");
        using var compressed = new MemoryStream(); var row = new byte[width];
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var at = (y * width + x) * 4; var gray = pixels[at];
                    if (pixels[at + 1] != gray || pixels[at + 2] != gray || pixels[at + 3] != (gray == Key ? 0 : 255))
                        throw new ArgumentException("Gray transparency projection differs.");
                    row[x] = gray;
                }
                zlib.WriteByte(0); zlib.Write(row);
            }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = 8;
        var key = new byte[2]; BinaryPrimitives.WriteUInt16BigEndian(key, Key);
        WarmPngAdam7Input.Chunk(output, "IHDR"u8, header); WarmPngAdam7Input.Chunk(output, "tRNS"u8, key);
        WarmPngAdam7Input.Chunk(output, "IDAT"u8, compressed.ToArray()); WarmPngAdam7Input.Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }
}
