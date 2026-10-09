using System.Buffers.Binary;
using Lokad.Image;

// Development file boundary: tightly packed RGBA8 or little-endian RGBA16.
internal static class RawPixels
{
    public static ImageBuffer Read(string path, int width, int height, int depth)
    {
        using var input = File.OpenRead(path);
        if (input.Length != checked((long)width * height * 4 * (depth / 8)))
            throw new InvalidDataException("Raw pixel file length differs from its declared geometry.");
        return Read(input, width, height, depth);
    }

    public static ImageBuffer Read(Stream input, int width, int height, int depth)
    {
        var image = ImageBuffer.Create(width, height, depth);
        try
        {
            var rowBytes = depth == 16 ? new byte[image.Stride] : null;
            for (var y = 0; y < height; y++)
            {
                if (depth == 8) input.ReadExactly(image.GetRow8(y));
                else
                {
                    input.ReadExactly(rowBytes!); var row = image.GetRow16(y);
                    for (var i = 0; i < row.Length; i++) row[i] = BinaryPrimitives.ReadUInt16LittleEndian(rowBytes.AsSpan(i * 2));
                }
            }
            return image;
        }
        catch { image.Dispose(); throw; }
    }

    public static void Write(ImageBuffer image, string path)
    {
        using var output = File.Create(path);
        Write(image, output);
    }

    public static void Write(ImageBuffer image, Stream output)
    {
        var rowBytes = image.BitDepth == 16 ? new byte[image.Stride] : null;
        for (var y = 0; y < image.Height; y++)
        {
            if (image.BitDepth == 8) output.Write(image.GetRow8(y));
            else
            {
                var row = image.GetRow16(y);
                for (var i = 0; i < row.Length; i++) BinaryPrimitives.WriteUInt16LittleEndian(rowBytes.AsSpan(i * 2), row[i]);
                output.Write(rowBytes!);
            }
        }
    }
}
