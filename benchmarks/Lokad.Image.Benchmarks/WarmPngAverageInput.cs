using System.Buffers.Binary;
using System.IO.Compression;

// Original authoring: PNG sections 9.2/9.3, type 3 Average, https://www.w3.org/TR/png-3/.
// Existing original RGBA8/16 rasters; no geometry/filter/effort enumeration.
internal static class WarmPngAverageInput
{
    internal static byte[] Build(byte[] pixels, int depth = 8)
    {
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        if (depth is not (8 or 16)) throw new ArgumentOutOfRangeException(nameof(depth));
        var stride = 4 * (depth / 8); var rowBytes = width * stride;
        if (pixels.Length != rowBytes * height) throw new ArgumentException("Expected the existing RGBA raster at the declared depth.");
        using var compressed = new MemoryStream(); var filtered = new byte[rowBytes];
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var y = 0; y < height; y++)
            {
                var row = pixels.AsSpan(y * rowBytes, rowBytes);
                for (var i = 0; i < rowBytes; i++)
                {
                    var sampleByte = depth == 16 ? i ^ 1 : i;
                    var left = i < stride ? 0 : row[sampleByte - stride];
                    var up = y == 0 ? 0 : pixels[(y - 1) * rowBytes + sampleByte];
                    filtered[i] = unchecked((byte)(row[sampleByte] - (left + up) / 2));
                }
                zlib.WriteByte(3); zlib.Write(filtered);
            }
        }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = (byte)depth; header[9] = 6;
        WarmPngAdam7Input.Chunk(output, "IHDR"u8, header);
        WarmPngAdam7Input.Chunk(output, "IDAT"u8, compressed.ToArray()); WarmPngAdam7Input.Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }
}
