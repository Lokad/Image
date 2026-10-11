using System.Numerics;
using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

/// <summary>Short-support filtering with geometry prepared before the pixel loop.</summary>
internal static class PreparedResize
{
    internal static void Apply<T>(ImageBuffer source, ImageBuffer result,
        ReadOnlySpan<double> horizontalWeights, ReadOnlySpan<int> horizontalOffsets,
        ReadOnlySpan<int> horizontalCounts, double scaleY, int radius, ResizeFilter filter,
        long rowWork, OperationContext context) where T : unmanaged, INumberBase<T>
    {
        // Byte and UInt16 specializations keep sample conversion and storage
        // outside runtime depth dispatch. Views remain local to this operation.
        ReadOnlySpan<T> input = MemoryMarshal.Cast<byte, T>(source.Pixels);
        var output = MemoryMarshal.Cast<byte, T>(result.Pixels);
        var sourceHeight = source.Height;
        var stride = source.Width * 4;
        var width = result.Width;
        var height = result.Height;
        var maximum = source.BitDepth == 8 ? 255 : 65535;
        Span<double> verticalWeights = stackalloc double[8];
        Span<int> verticalOffsets = stackalloc int[8];
        for (var y = 0; y < height; y++)
        {
            context.Work(rowWork);
            var center = (y + 0.5) * sourceHeight / height - 0.5;
            var first = (int)Math.Ceiling(center - radius * scaleY);
            var last = (int)Math.Floor(center + radius * scaleY);
            var count = last - first + 1;
            for (var tap = 0; tap < count; tap++)
            {
                verticalWeights[tap] = Kernel((first + tap - center) / scaleY, filter);
                verticalOffsets[tap] = Math.Clamp(first + tap, 0, sourceHeight - 1) * stride;
            }
            for (var x = 0; x < width; x++)
            {
                var baseX = x * 8;
                var weights = horizontalWeights.Slice(baseX, horizontalCounts[x]);
                var offsets = horizontalOffsets.Slice(baseX, weights.Length);
                double red = 0, green = 0, blue = 0, alpha = 0, weightSum = 0;
                for (var ty = 0; ty < count; ty++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var wy = verticalWeights[ty];
                    if (wy == 0) continue;
                    var rowOffset = verticalOffsets[ty];
                    for (var tx = 0; tx < weights.Length; tx++)
                    {
                        var weight = wy * weights[tx];
                        if (weight == 0) continue;
                        var offset = rowOffset + offsets[tx];
                        var a = double.CreateChecked(input[offset + 3]);
                        red += weight * a * double.CreateChecked(input[offset]);
                        green += weight * a * double.CreateChecked(input[offset + 1]);
                        blue += weight * a * double.CreateChecked(input[offset + 2]);
                        alpha += weight * a;
                        weightSum += weight;
                    }
                }
                var normalizedAlpha = alpha / weightSum;
                var roundedAlpha = normalizedAlpha > 0 && normalizedAlpha < 1
                    ? ResizeAlpha.Round(source, x, y, width, height, filter, context)
                    : Round(normalizedAlpha, maximum);
                var hasColor = roundedAlpha > 0 && alpha > 0;
                var destination = (y * width + x) * 4;
                output[destination] = T.CreateTruncating(Round(hasColor ? red / alpha : 0, maximum));
                output[destination + 1] = T.CreateTruncating(Round(hasColor ? green / alpha : 0, maximum));
                output[destination + 2] = T.CreateTruncating(Round(hasColor ? blue / alpha : 0, maximum));
                output[destination + 3] = T.CreateTruncating(roundedAlpha);
            }
        }
    }

    private static int Round(double value, int maximum)
        => (int)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, maximum);

    private static double Kernel(double distance, ResizeFilter filter)
    {
        var x = Math.Abs(distance);
        if (filter == ResizeFilter.Bilinear) return Math.Max(0, 1 - x);
        // Catmull-Rom B=0, C=1/2, derived from the cubic interpolation conditions.
        return x < 1 ? 1.5 * x * x * x - 2.5 * x * x + 1
            : x < 2 ? -0.5 * x * x * x + 2.5 * x * x - 4 * x + 2 : 0;
    }
}
