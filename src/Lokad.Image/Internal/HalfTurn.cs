using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

/// <summary>Reverse complete pixels, keeping channel bytes and precision intact.</summary>
internal static class HalfTurn
{
    public static ImageBuffer Apply(ImageBuffer source, ImageLimits? limits, CancellationToken cancellationToken)
    {
        using var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.ReserveBorrowed(source.ByteLength);
        var result = ImageBuffer.AllocateForOverwrite(source.Width, source.Height, source.BitDepth, context);
        try
        {
            // A half turn reverses the tightly packed raster's complete pixels.
            // Channels within each pixel keep their exact original byte order.
            if (source.BitDepth == 8)
                PixelReversal.Reverse(MemoryMarshal.Cast<byte, uint>(source.Pixels), MemoryMarshal.Cast<byte, uint>(result.Pixels), context);
            else
                PixelReversal.Reverse(MemoryMarshal.Cast<byte, ulong>(source.Pixels), MemoryMarshal.Cast<byte, ulong>(result.Pixels), context);
            result.Orientation = source.Orientation;
            return result;
        }
        catch { result.Dispose(); throw; }
    }
}
