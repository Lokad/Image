using System.Buffers.Binary;
using System.IO.Compression;

// Original authoring: PNG section 9.2, filter type 2 Up, https://www.w3.org/TR/png-3/.
// One existing RGBA8 raster; no geometry/filter/effort enumeration.
internal static class WarmPngUpInput
{
    internal static byte[] Build(byte[] pixels)
    {
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        const int rowBytes = width * 4;
        if (pixels.Length != rowBytes * height) throw new ArgumentException("Expected the existing RGBA8 raster.");
        using var compressed = new MemoryStream(); var filtered = new byte[rowBytes];
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var y = 0; y < height; y++)
            {
                var row = pixels.AsSpan(y * rowBytes, rowBytes);
                for (var i = 0; i < rowBytes; i++)
                    filtered[i] = unchecked((byte)(row[i] - (y == 0 ? 0 : pixels[(y - 1) * rowBytes + i])));
                zlib.WriteByte(2); zlib.Write(filtered);
            }
        }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = 8; header[9] = 6;
        WarmPngAdam7Input.Chunk(output, "IHDR"u8, header);
        WarmPngAdam7Input.Chunk(output, "IDAT"u8, compressed.ToArray()); WarmPngAdam7Input.Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }
}
