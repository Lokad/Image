using System.Buffers.Binary;
using System.IO.Compression;

// Original fixture authoring, PNG sections 7/9/11, https://www.w3.org/TR/png-3/.
// One existing raster and one explicit palette projection; no corpus or sweep.
internal static class WarmPngPaletteInput
{
    internal static void Project(Span<byte> pixels, int depth = 8)
    {
        if (depth is not (4 or 8)) throw new ArgumentOutOfRangeException(nameof(depth));
        if (pixels.Length != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4)
            throw new ArgumentException("Expected the existing RGBA8 raster.");
        for (var i = 0; i < pixels.Length; i += 4)
        {
            var index = depth == 4 ? (byte)((pixels[i] >> 4) * 17) : pixels[i];
            pixels[i] = index;
            pixels[i + 1] = (byte)(255 - index);
            pixels[i + 2] = unchecked((byte)(index * 73 + 19));
            pixels[i + 3] = index;
        }
    }

    internal static byte[] Build(ReadOnlySpan<byte> pixels, int depth = 8)
    {
        if (depth is not (4 or 8)) throw new ArgumentOutOfRangeException(nameof(depth));
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        if (pixels.Length != width * height * 4) throw new ArgumentException("Expected the projected RGBA8 raster.");
        using var compressed = new MemoryStream(); var row = new byte[(width * depth + 7) / 8];
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var y = 0; y < height; y++)
            {
                row.AsSpan().Clear();
                for (var x = 0; x < width; x++)
                {
                    var offset = (y * width + x) * 4; var index = pixels[offset];
                    if (pixels[offset + 1] != 255 - index || pixels[offset + 2] != unchecked((byte)(index * 73 + 19))
                        || pixels[offset + 3] != index) throw new ArgumentException("Palette projection differs.");
                    if (depth == 8) row[x] = index;
                    else
                    {
                        if (index % 17 != 0) throw new ArgumentException("Packed palette projection differs.");
                        row[x / 2] |= (byte)((index / 17) << (x % 2 == 0 ? 4 : 0));
                    }
                }
                zlib.WriteByte(0); zlib.Write(row);
            }
        }
        var entries = 1 << depth; var palette = new byte[entries * 3]; var alpha = new byte[entries];
        for (var i = 0; i < entries; i++)
        {
            var value = depth == 4 ? i * 17 : i;
            palette[i * 3] = (byte)value; palette[i * 3 + 1] = (byte)(255 - value);
            palette[i * 3 + 2] = unchecked((byte)(value * 73 + 19)); alpha[i] = (byte)value;
        }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = (byte)depth; header[9] = 3;
        WarmPngAdam7Input.Chunk(output, "IHDR"u8, header);
        WarmPngAdam7Input.Chunk(output, "PLTE"u8, palette); WarmPngAdam7Input.Chunk(output, "tRNS"u8, alpha);
        WarmPngAdam7Input.Chunk(output, "IDAT"u8, compressed.ToArray()); WarmPngAdam7Input.Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }
}
