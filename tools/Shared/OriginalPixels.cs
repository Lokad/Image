using System.Buffers.Binary;

namespace Lokad.Image.Tooling;

internal static class OriginalPixels
{
    public static byte[] Generate(int width, int height, int depth, bool grayscale = false, bool opaque = false, int phase = 0)
    {
        var bytes = new byte[width * height * 4 * (depth / 8)];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var channel = 0; channel < 4; channel++)
        {
            // Phase zero retains the original asymmetric-v1 recipe exactly.
            // Numbered still-image batches add the declared phase polynomial.
            var value = channel == 3 && opaque ? 65535
                : ((x * 12347 + y * 7919 + (grayscale && channel < 3 ? 0 : channel) * 23459 + x * y * 101
                    + phase * 977 + phase * (grayscale && channel < 3 ? 0 : channel) * 313) & 65535);
            var position = ((y * width + x) * 4 + channel) * (depth / 8);
            if (depth == 8) bytes[position] = (byte)((value + 128) / 257);
            else BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(position), (ushort)value);
        }
        return bytes;
    }

    public static ImageBuffer Buffer(int width, int height, int depth, bool grayscale = false, bool opaque = false, int phase = 0)
    {
        var source = Generate(width, height, depth, grayscale, opaque, phase);
        var result = ImageBuffer.Create(width, height, depth);
        for (var y = 0; y < height; y++)
        {
            if (depth == 8) source.AsSpan(y * width * 4, width * 4).CopyTo(result.GetRow8(y));
            else
                for (var i = 0; i < width * 4; i++)
                    result.GetRow16(y)[i] = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan((y * width * 4 + i) * 2));
        }
        return result;
    }

    public static byte[] Bytes(ImageBuffer image)
    {
        var result = new byte[image.Width * image.Height * 4 * (image.BitDepth / 8)];
        for (var y = 0; y < image.Height; y++)
        {
            if (image.BitDepth == 8) image.GetRow8(y).CopyTo(result.AsSpan(y * image.Width * 4));
            else
                for (var i = 0; i < image.Width * 4; i++)
                    BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan((y * image.Width * 4 + i) * 2), image.GetRow16(y)[i]);
        }
        return result;
    }
}
