using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Place complete pixels counterclockwise in bounded cache-local tiles.</summary>
internal static class CounterclockwiseTurn
{
    public static ImageBuffer Apply(ImageBuffer source, ImageLimits? limits, CancellationToken cancellationToken)
    {
        using var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.ReserveBorrowed(source.ByteLength);
        var result = ImageBuffer.AllocateForOverwrite(source.Height, source.Width, source.BitDepth, context);
        try
        {
            if (source.BitDepth == 8)
                TransposeCopy.Copy<uint, TransposeCopy.ReverseRows>(MemoryMarshal.Cast<byte, uint>(source.Pixels), MemoryMarshal.Cast<byte, uint>(result.Pixels),
                    source.Width, source.Height, context);
            else if (Avx2.IsSupported)
                TransposeCopy.CopyWords<TransposeCopy.ReverseRows>(MemoryMarshal.Cast<byte, ulong>(source.Pixels),
                    MemoryMarshal.Cast<byte, ulong>(result.Pixels), source.Width, source.Height, context);
            else
                TransposeCopy.Copy<ulong, TransposeCopy.ReverseRows>(MemoryMarshal.Cast<byte, ulong>(source.Pixels), MemoryMarshal.Cast<byte, ulong>(result.Pixels),
                    source.Width, source.Height, context);
            result.Orientation = source.Orientation;
            return result;
        }
        catch { result.Dispose(); throw; }
    }
}
