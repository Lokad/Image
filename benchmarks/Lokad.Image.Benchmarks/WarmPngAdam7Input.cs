using System.Buffers.Binary;
using System.IO.Compression;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

// Original untimed fixture authoring: PNG sections 8, 9 and 11,
// https://www.w3.org/TR/png-3/. Uses the existing performance raster.
internal static class WarmPngAdam7Input
{
    internal static readonly (int X, int Y, int Dx, int Dy)[] Passes =
        [(0, 0, 8, 8), (4, 0, 8, 8), (0, 4, 4, 8), (2, 0, 4, 4),
         (0, 2, 2, 4), (1, 0, 2, 2), (0, 1, 1, 2)];

    internal static byte[] Build(byte[] pixels)
    {
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        if (pixels.Length != width * height * 8) throw new ArgumentException("Expected the original RGBA16 raster.");
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            foreach (var (startX, startY, stepX, stepY) in Passes)
            {
                var row = new byte[((width - startX + stepX - 1) / stepX) * 8];
                for (var y = startY; y < height; y += stepY)
                {
                    for (var x = startX; x < width; x += stepX)
                    for (var channel = 0; channel < 4; channel++)
                    {
                        var source = ((y * width + x) * 4 + channel) * 2;
                        var target = ((x - startX) / stepX * 4 + channel) * 2;
                        row[target] = pixels[source + 1]; row[target + 1] = pixels[source];
                    }
                    zlib.WriteByte(0); zlib.Write(row);
                }
            }
        }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);
        header[8] = 16; header[9] = 6; header[12] = 1;
        Chunk(output, "IHDR"u8, header); Chunk(output, "IDAT"u8, compressed.ToArray()); Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }

    internal static void Chunk(Stream output, ReadOnlySpan<byte> type, ReadOnlySpan<byte> bytes)
    {
        Span<byte> field = stackalloc byte[4]; BinaryPrimitives.WriteInt32BigEndian(field, bytes.Length);
        output.Write(field); output.Write(type); output.Write(bytes);
        uint crc = uint.MaxValue;
        foreach (var value in type) Append(value);
        foreach (var value in bytes) Append(value);
        BinaryPrimitives.WriteUInt32BigEndian(field, ~crc); output.Write(field);
        void Append(byte value)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++) crc = crc >> 1 ^ ((crc & 1) != 0 ? 0xedb88320u : 0);
        }
    }

    internal static byte[] BuildGray1(byte[] pixels)
    {
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        if (pixels.Length != width * height * 4) throw new ArgumentException("Expected the existing projected RGBA8 raster.");
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            foreach (var (startX, startY, stepX, stepY) in Passes)
            {
                var passWidth = (width - startX + stepX - 1) / stepX;
                var row = new byte[(passWidth + 7) / 8];
                for (var y = startY; y < height; y += stepY)
                {
                    row.AsSpan().Clear();
                    for (var i = 0; i < passWidth; i++)
                        if (pixels[(y * width + startX + i * stepX) * 4] == 255)
                            row[i / 8] |= (byte)(1 << (7 - i % 8));
                    zlib.WriteByte(0); zlib.Write(row);
                }
            }
        }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = 1; header[12] = 1;
        Chunk(output, "IHDR"u8, header); Chunk(output, "IDAT"u8, compressed.ToArray()); Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }

    internal static void Prepare()
    {
        using var state = new JpegBenchmarkCase();
        var pixels = HorizontalFlipInput.Create(16); var encoded = Build(pixels);
        using var image = ImageCodec.Decode(encoded);
        if (image.BitDepth != 16 || !OriginalPixels.Bytes(image).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("Authored Adam7 samples differ.");
        var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "png-adam7-16");
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(Path.Combine(directory, "input.png"), encoded);
        File.WriteAllBytes(Path.Combine(directory, "expected.raw"), pixels);
        File.WriteAllText(directory + ".json", JsonSerializer.Serialize(new
        {
            schemaVersion = 1, project = "Lokad.Image", correctnessOnly = true,
            recipe = "Original HorizontalFlipInput RGBA16 raster; PNG Adam7 seven passes; filter None; BCL Optimal level 6; one IDAT. No raster, geometry, filter or effort enumeration.",
            width = image.Width, height = image.Height, depth = 16, interlace = 1,
            rasterSha256 = JpegBenchmarkCase.Hash(pixels), inputSha256 = JpegBenchmarkCase.Hash(encoded),
            encodedBytes = encoded.Length, allRgbaSamplesExact = true
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine("Prepared one original-raster Adam7 RGBA16 input; no timing measurement.");
    }
}
