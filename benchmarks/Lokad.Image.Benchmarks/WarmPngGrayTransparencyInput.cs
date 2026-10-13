using System.Buffers.Binary;
using System.IO.Compression;

// Original fixture authoring, PNG sections 7/9/11: gray8/16 with a tRNS key.
// Existing raster's red channel only; no new original image or profile sweep.
internal static class WarmPngGrayTransparencyInput
{
    internal const ushort Key = 127;
    internal static ushort KeyFor(int depth) => depth == 16 ? (ushort)0x7f7f : depth == 8 ? Key : throw new ArgumentOutOfRangeException(nameof(depth));

    internal static void Project(Span<byte> pixels, int depth = 8)
    {
        var key = KeyFor(depth); var bytes = depth / 8;
        if (pixels.Length != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4 * bytes)
            throw new ArgumentException("Expected the existing RGBA raster.");
        for (var i = 0; i < pixels.Length; i += 4 * bytes)
        {
            var gray = bytes == 1 ? pixels[i] : BinaryPrimitives.ReadUInt16LittleEndian(pixels[i..]);
            pixels.Slice(i, bytes).CopyTo(pixels.Slice(i + bytes, bytes));
            pixels.Slice(i, bytes).CopyTo(pixels.Slice(i + 2 * bytes, bytes));
            pixels.Slice(i + 3 * bytes, bytes).Fill(gray == key ? (byte)0 : (byte)255);
        }
    }

    internal static byte[] Build(ReadOnlySpan<byte> pixels, int depth = 8)
    {
        var keyValue = KeyFor(depth); var bytes = depth / 8;
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        if (pixels.Length != width * height * 4 * bytes) throw new ArgumentException("Expected projected gray pixels.");
        using var compressed = new MemoryStream(); var row = new byte[width * bytes];
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var at = (y * width + x) * 4 * bytes;
                    var gray = bytes == 1 ? pixels[at] : BinaryPrimitives.ReadUInt16LittleEndian(pixels[at..]);
                    for (var c = 1; c < 3; c++)
                        if (!pixels.Slice(at, bytes).SequenceEqual(pixels.Slice(at + c * bytes, bytes)))
                            throw new ArgumentException("Gray transparency RGB projection differs.");
                    var alpha = gray == keyValue ? 0 : depth == 16 ? 65535 : 255;
                    var storedAlpha = bytes == 1 ? pixels[at + 3] : BinaryPrimitives.ReadUInt16LittleEndian(pixels[(at + 6)..]);
                    if (storedAlpha != alpha) throw new ArgumentException("Gray transparency alpha differs.");
                    if (bytes == 1) row[x] = (byte)gray;
                    else BinaryPrimitives.WriteUInt16BigEndian(row.AsSpan(x * 2), (ushort)gray);
                }
                zlib.WriteByte(0); zlib.Write(row);
            }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = (byte)depth;
        var key = new byte[2]; BinaryPrimitives.WriteUInt16BigEndian(key, keyValue);
        WarmPngAdam7Input.Chunk(output, "IHDR"u8, header); WarmPngAdam7Input.Chunk(output, "tRNS"u8, key);
        WarmPngAdam7Input.Chunk(output, "IDAT"u8, compressed.ToArray()); WarmPngAdam7Input.Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }
}
