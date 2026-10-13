using System.Buffers.Binary;
using System.IO.Compression;

// Original PNG authoring, sections 7/9/11: RGB8 with one full six-byte tRNS key.
// This triplet occurs in the existing raster (pixel 1); RGB is never changed.
internal static class WarmPngRgbTransparencyInput
{
    internal static ReadOnlySpan<byte> Key => [0, 48, 0, 139, 0, 231];

    internal static void Project(Span<byte> pixels)
    {
        if (pixels.Length != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4)
            throw new ArgumentException("Expected the existing RGBA8 raster.");
        for (var i = 0; i < pixels.Length; i += 4)
            pixels[i + 3] = pixels[i] == 48 && pixels[i + 1] == 139 && pixels[i + 2] == 231 ? (byte)0 : (byte)255;
    }

    internal static byte[] Build(ReadOnlySpan<byte> pixels)
    {
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        if (pixels.Length != width * height * 4) throw new ArgumentException("Expected projected RGB8 pixels.");
        using var compressed = new MemoryStream(); var row = new byte[width * 3];
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var at = (y * width + x) * 4;
                    var alpha = pixels[at] == 48 && pixels[at + 1] == 139 && pixels[at + 2] == 231 ? 0 : 255;
                    if (pixels[at + 3] != alpha) throw new ArgumentException("RGB transparency alpha differs.");
                    pixels.Slice(at, 3).CopyTo(row.AsSpan(x * 3));
                }
                zlib.WriteByte(0); zlib.Write(row);
            }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = 8; header[9] = 2;
        WarmPngAdam7Input.Chunk(output, "IHDR"u8, header); WarmPngAdam7Input.Chunk(output, "tRNS"u8, Key);
        WarmPngAdam7Input.Chunk(output, "IDAT"u8, compressed.ToArray()); WarmPngAdam7Input.Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }
}
