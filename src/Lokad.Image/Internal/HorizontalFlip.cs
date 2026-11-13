using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

/// <summary>Reverse complete pixels, keeping channel bytes and precision intact.</summary>
internal static class HorizontalFlip
{
    public static ImageBuffer Apply(ImageBuffer source, ImageLimits? limits, CancellationToken cancellationToken)
    {
        using var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.ReserveBorrowed(source.ByteLength);
        var result = ImageBuffer.AllocateForOverwrite(source.Width, source.Height, source.BitDepth, context);
        try
        {
            var stride = source.Stride;
            for (var y = 0; y < source.Height; y++)
            {
                var input = source.Pixels.Slice(y * stride, stride);
                var output = result.Pixels.Slice(y * stride, stride);
                if (source.BitDepth == 8)
                    PixelReversal.Reverse(MemoryMarshal.Cast<byte, uint>(input), MemoryMarshal.Cast<byte, uint>(output), context);
                else
                    PixelReversal.Reverse(MemoryMarshal.Cast<byte, ulong>(input), MemoryMarshal.Cast<byte, ulong>(output), context);
            }
            result.Orientation = source.Orientation;
            return result;
        }
        catch { result.Dispose(); throw; }
    }
}
