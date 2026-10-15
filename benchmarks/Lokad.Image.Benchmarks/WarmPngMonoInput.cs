using System.Buffers.Binary;
using System.IO.Compression;

// Original fixture authoring, PNG sections 7/9/11, https://www.w3.org/TR/png-3/.
// One existing raster projected to packed gray1/2/4; no geometry/effort sweep.
internal static class WarmPngMonoInput
{
    internal static void Project(Span<byte> pixels, int depth = 1)
    {
        if (depth is not (1 or 2 or 4)) throw new ArgumentOutOfRangeException(nameof(depth));
        if (pixels.Length != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4)
            throw new ArgumentException("Expected the existing RGBA8 raster.");
        for (var i = 0; i < pixels.Length; i += 4)
        {
            var gray = (byte)((pixels[i] >> (8 - depth)) * (255 / ((1 << depth) - 1)));
            pixels[i] = gray; pixels[i + 1] = gray; pixels[i + 2] = gray; pixels[i + 3] = 255;
        }
    }

    internal static byte[] Build(ReadOnlySpan<byte> pixels, int depth = 1, bool transparentZero = false)
    {
        if (depth is not (1 or 2 or 4)) throw new ArgumentOutOfRangeException(nameof(depth));
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        if (pixels.Length != width * height * 4) throw new ArgumentException("Expected projected gray pixels.");
        var maximum = (1 << depth) - 1; var scale = 255 / maximum;
        using var compressed = new MemoryStream(); var row = new byte[(width * depth + 7) / 8];
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var y = 0; y < height; y++)
            {
                row.AsSpan().Clear();
                for (var x = 0; x < width; x++)
                {
                    var offset = (y * width + x) * 4; var gray = pixels[offset];
                    if (gray % scale != 0 || pixels[offset + 1] != gray || pixels[offset + 2] != gray
                        || pixels[offset + 3] != (transparentZero && gray == 0 ? 0 : 255)) throw new ArgumentException("Packed gray projection differs.");
                    row[x * depth / 8] |= (byte)((gray / scale) << (8 - depth - x * depth % 8));
                }
                zlib.WriteByte(0); zlib.Write(row);
            }
        }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = (byte)depth;
        WarmPngAdam7Input.Chunk(output, "IHDR"u8, header);
        if (transparentZero) WarmPngAdam7Input.Chunk(output, "tRNS"u8, new byte[2]);
        WarmPngAdam7Input.Chunk(output, "IDAT"u8, compressed.ToArray()); WarmPngAdam7Input.Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }
}
