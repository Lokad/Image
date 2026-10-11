using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

/// <summary>Exact center selection with complete pixels and row-local coordinate state.</summary>
internal static class NearestResize
{
    internal static ImageBuffer Apply(ImageBuffer source, int width, int height, OperationContext context)
    {
        // Resize has already admitted the source and target geometry. No scratch
        // storage is needed, including under an exact two-buffer memory budget.
        var result = ImageBuffer.Allocate(width, height, source.BitDepth, context);
        try
        {
            if (source.BitDepth == 8)
                CopyRows<uint>(source, result, context);
            else
                CopyRows<ulong>(source, result, context);
            result.Orientation = source.Orientation;
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static void CopyRows<T>(ImageBuffer source, ImageBuffer result, OperationContext context) where T : unmanaged
    {
        // uint/ulong views copy the original four channels as bits, preserving
        // native-endian UInt16 samples and RGB hidden behind zero alpha.
        ReadOnlySpan<T> input = MemoryMarshal.Cast<byte, T>(source.Pixels);
        var output = MemoryMarshal.Cast<byte, T>(result.Pixels);
        var sourceWidth = source.Width;
        var width = result.Width;
        var denominator = 2L * width;
        var first = sourceWidth / denominator;
        var initialRemainder = sourceWidth % denominator;
        var step = 2L * sourceWidth / denominator;
        var remainderStep = 2L * sourceWidth % denominator;
        for (var y = 0; y < result.Height; y++)
        {
            var sy = ResizeSampling.Nearest(y, source.Height, result.Height);
            var row = input.Slice(sy * sourceWidth, sourceWidth);
            var destination = output.Slice(y * width, width);
            var sx = first;
            var remainder = initialRemainder;
            for (var x = 0; x < width; x++)
            {
                if ((x & 1023) == 0) context.Work(Math.Min(1024, width - x) * 4L);
                destination[x] = row[(int)sx];

                // Maintain quotient/remainder of (2*x+1)*sourceWidth over 2*width.
                // Each step adds 2*sourceWidth; both remainder terms are below
                // the denominator, so at most one carry occurs. Long arithmetic
                // also covers dimensions above the default admission limit.
                sx += step;
                remainder += remainderStep;
                if (remainder >= denominator) { remainder -= denominator; sx++; }
            }
        }
    }
}
