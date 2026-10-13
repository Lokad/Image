using System.Buffers.Binary;
using System.IO.Compression;

// Original PNG authoring, sections 7/9/11: RGB8/16 with one six-byte tRNS key.
// Each full triplet comes from pixel 1 of the existing raster; RGB never changes.
internal static class WarmPngRgbTransparencyInput
{
    internal static ReadOnlySpan<byte> Key => [0, 48, 0, 139, 0, 231];
    internal static ReadOnlySpan<byte> KeyFor(int depth) => depth switch
    {
        8 => Key,
        16 => [0x30, 0x3b, 0x8b, 0xde, 0xe7, 0x81],
        _ => throw new ArgumentOutOfRangeException(nameof(depth))
    };
    internal static ReadOnlySpan<int> KeySamples(int depth) => depth switch
    {
        8 => [48, 139, 231],
        16 => [12347, 35806, 59265],
        _ => throw new ArgumentOutOfRangeException(nameof(depth))
    };

    internal static void Project(Span<byte> pixels, int depth = 8)
    {
        var key = KeySamples(depth); var sampleBytes = depth / 8;
        if (pixels.Length != HorizontalFlipInput.Width * HorizontalFlipInput.Height * 4 * sampleBytes)
            throw new ArgumentException("Expected the existing raster at the selected precision.");
        for (var i = 0; i < pixels.Length; i += 4 * sampleBytes)
        {
            var match = Read(pixels, i, depth) == key[0]
                && Read(pixels, i + sampleBytes, depth) == key[1]
                && Read(pixels, i + 2 * sampleBytes, depth) == key[2];
            pixels.Slice(i + 3 * sampleBytes, sampleBytes).Fill(match ? (byte)0 : (byte)255);
        }
    }

    internal static byte[] Build(ReadOnlySpan<byte> pixels, int depth = 8)
    {
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        var key = KeySamples(depth); var sampleBytes = depth / 8;
        if (pixels.Length != width * height * 4 * sampleBytes) throw new ArgumentException("Expected projected RGB pixels.");
        using var compressed = new MemoryStream(); var row = new byte[width * 3 * sampleBytes];
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var at = (y * width + x) * 4 * sampleBytes;
                    var match = Read(pixels, at, depth) == key[0]
                        && Read(pixels, at + sampleBytes, depth) == key[1]
                        && Read(pixels, at + 2 * sampleBytes, depth) == key[2];
                    if (Read(pixels, at + 3 * sampleBytes, depth) != (match ? 0 : depth == 16 ? 65535 : 255))
                        throw new ArgumentException("RGB transparency alpha differs.");
                    if (depth == 8) pixels.Slice(at, 3).CopyTo(row.AsSpan(x * 3));
                    else for (var channel = 0; channel < 3; channel++)
                        BinaryPrimitives.WriteUInt16BigEndian(row.AsSpan((x * 3 + channel) * 2),
                            (ushort)Read(pixels, at + channel * 2, depth));
                }
                zlib.WriteByte(0); zlib.Write(row);
            }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = (byte)depth; header[9] = 2;
        WarmPngAdam7Input.Chunk(output, "IHDR"u8, header); WarmPngAdam7Input.Chunk(output, "tRNS"u8, KeyFor(depth));
        WarmPngAdam7Input.Chunk(output, "IDAT"u8, compressed.ToArray()); WarmPngAdam7Input.Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }

    private static int Read(ReadOnlySpan<byte> pixels, int at, int depth) => depth == 8
        ? pixels[at] : BinaryPrimitives.ReadUInt16LittleEndian(pixels.Slice(at, 2));
}
