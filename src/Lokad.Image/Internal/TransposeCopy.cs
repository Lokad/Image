using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

/// <summary>Transpose complete pixels in bounded cache-local tiles.</summary>
internal static class TransposeCopy
{
    public static ImageBuffer Apply(ImageBuffer source, ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.Reserve(source.ByteLength);
        var result = ImageBuffer.AllocateForOverwrite(source.Height, source.Width, source.BitDepth, context);
        try
        {
            if (source.BitDepth == 8)
                Copy(MemoryMarshal.Cast<byte, uint>(source.Pixels), MemoryMarshal.Cast<byte, uint>(result.Pixels),
                    source.Width, source.Height, context);
            else
                Copy(MemoryMarshal.Cast<byte, ulong>(source.Pixels), MemoryMarshal.Cast<byte, ulong>(result.Pixels),
                    source.Width, source.Height, context);
            result.Orientation = source.Orientation;
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static void Copy<T>(ReadOnlySpan<T> source, Span<T> destination, int width, int height,
        OperationContext context) where T : unmanaged
    {
        // A tile contains at most 1024 pixels; both working regions stay small.
        // Full-pixel assignments retain channel byte order at either sample depth.
        for (var y = 0; y < height; y += 32)
        for (var x = 0; x < width; x += 32)
        {
            var rows = Math.Min(32, height - y);
            var columns = Math.Min(32, width - x);
            context.Work(rows * columns * 4L);
            for (var column = x; column < x + columns; column++)
            {
                var input = y * width + column;
                var output = destination.Slice(column * height + y, rows);
                for (var row = 0; row < rows; row++)
                {
                    output[row] = source[input];
                    input += width;
                }
            }
        }
    }
}
