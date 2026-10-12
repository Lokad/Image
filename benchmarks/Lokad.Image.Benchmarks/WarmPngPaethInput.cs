using System.Buffers.Binary;
using System.IO.Compression;
using System.Text.Json;
using Lokad.Image;
using Lokad.Image.Tooling;

// Original fixture authoring: PNG sections 9.4/11, https://www.w3.org/TR/png-3/.
// One existing raster and one fixed filter/compression profile; no enumeration.
internal static class WarmPngPaethInput
{
    internal static int Predict(int left, int up, int upperLeft)
    {
        var p = left + up - upperLeft;
        var a = Math.Abs(p - left); var b = Math.Abs(p - up); var c = Math.Abs(p - upperLeft);
        return a <= b && a <= c ? left : b <= c ? up : upperLeft;
    }

    internal static byte[] Build(byte[] pixels, int depth = 16)
    {
        const int width = HorizontalFlipInput.Width, height = HorizontalFlipInput.Height;
        var bytesPerPixel = depth / 8 * 4;
        if (depth is not (8 or 16) || pixels.Length != width * height * bytesPerPixel)
            throw new ArgumentException("Expected one original RGBA raster at the declared precision.");
        var previous = new byte[width * bytesPerPixel]; var row = new byte[width * bytesPerPixel]; var filtered = new byte[row.Length];
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            for (var y = 0; y < height; y++)
            {
                for (var i = 0; i < row.Length; i++) row[i] = pixels[y * row.Length + (depth == 16 ? i ^ 1 : i)];
                for (var i = 0; i < row.Length; i++)
                    filtered[i] = unchecked((byte)(row[i] - Predict(i >= bytesPerPixel ? row[i - bytesPerPixel] : 0,
                        previous[i], i >= bytesPerPixel ? previous[i - bytesPerPixel] : 0)));
                zlib.WriteByte(4); zlib.Write(filtered); (previous, row) = (row, previous);
            }
        }
        using var output = new MemoryStream(); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = (byte)depth; header[9] = 6;
        WarmPngAdam7Input.Chunk(output, "IHDR"u8, header);
        WarmPngAdam7Input.Chunk(output, "IDAT"u8, compressed.ToArray()); WarmPngAdam7Input.Chunk(output, "IEND"u8, []);
        return output.ToArray();
    }

    internal static void Prepare(int depth = 16)
    {
        using var state = new JpegBenchmarkCase(); var pixels = HorizontalFlipInput.Create(depth); var encoded = Build(pixels, depth);
        using var image = ImageCodec.Decode(encoded);
        if (image.BitDepth != depth || !OriginalPixels.Bytes(image).AsSpan().SequenceEqual(pixels))
            throw new InvalidOperationException("Authored Paeth samples differ.");
        var directory = Path.Combine(state.Root, "artifacts", "warm-reference", "png-paeth-" + depth);
        Directory.CreateDirectory(directory); File.WriteAllBytes(Path.Combine(directory, "input.png"), encoded);
        File.WriteAllBytes(Path.Combine(directory, "expected.raw"), pixels);
        File.WriteAllText(directory + ".json", JsonSerializer.Serialize(new
        {
            schemaVersion = 1, project = "Lokad.Image", correctnessOnly = true,
            recipe = $"Original HorizontalFlipInput RGBA{depth} raster; noninterlaced PNG; every row Paeth; BCL Optimal level 6; one IDAT. No raster, geometry, filter or effort enumeration.",
            width = image.Width, height = image.Height, depth, interlace = 0, filter = 4,
            rasterSha256 = JpegBenchmarkCase.Hash(pixels), inputSha256 = JpegBenchmarkCase.Hash(encoded),
            encodedBytes = encoded.Length, allRgbaSamplesExact = true
        }, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"Prepared one original-raster Paeth RGBA{depth} input; no timing measurement.");
    }
}
