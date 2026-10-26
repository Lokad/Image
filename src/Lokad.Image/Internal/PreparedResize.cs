using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Short-support filtering with geometry prepared before the pixel loop.</summary>
internal static class PreparedResize
{
    internal static void Apply<T>(ImageBuffer source, ImageBuffer result,
        ReadOnlySpan<double> horizontalWeights, ReadOnlySpan<int> horizontalOffsets,
        ReadOnlySpan<int> horizontalCounts, double scaleY, int radius, ResizeFilter filter,
OperationContext context) where T : unmanaged, INumberBase<T>
    {
        // Byte and UInt16 specializations keep sample conversion and storage
        // outside runtime depth dispatch. Views remain local to this operation.
        ReadOnlySpan<T> input = MemoryMarshal.Cast<byte, T>(source.Pixels);
        var packed8 = typeof(T) == typeof(byte) && Avx.IsSupported && Sse41.IsSupported;
        var paired = (typeof(T) == typeof(byte) || typeof(T) == typeof(ushort))
            && Avx512F.IsSupported && Avx2.IsSupported && Avx.IsSupported && Sse41.IsSupported;
        if (paired && Avx512DQ.IsSupported && typeof(T) == typeof(byte) && filter == ResizeFilter.Bilinear
            && result.Width > source.Width && result.Height > source.Height
            && BilinearUpsample.TryApply(source, result, horizontalWeights, horizontalOffsets,
                horizontalCounts, context)) return;
        if (paired && Avx512DQ.IsSupported && typeof(T) == typeof(ushort) && filter == ResizeFilter.Bilinear
            && result.Width > source.Width && result.Height > source.Height
            && BilinearUpsample16.TryApply(source, result, horizontalWeights, horizontalOffsets,
                horizontalCounts, context)) return;
        if (paired && typeof(T) == typeof(ushort) && filter == ResizeFilter.Bilinear
            && BilinearDownsample16.TryApply(source, result, scaleY, context)) return;
        if (paired && typeof(T) == typeof(byte) && filter == ResizeFilter.Bilinear
            && BilinearDownsample8.TryApply(source, result, scaleY, context)) return;
        ReadOnlySpan<byte> packedInput = packed8 || paired ? MemoryMarshal.Cast<T, byte>(input) : default;
        var output = MemoryMarshal.Cast<byte, T>(result.Pixels);
        Span<byte> pairedOutput = paired ? MemoryMarshal.Cast<T, byte>(output) : default;
        var sourceHeight = source.Height;
        var stride = source.Width * 4;
        var width = result.Width;
        var height = result.Height;
        var maximum = source.BitDepth == 8 ? 255 : 65535;
        Span<double> verticalWeights = stackalloc double[8];
        Span<int> verticalOffsets = stackalloc int[8];
        for (var y = 0; y < height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (paired && PairedResize.TryRows<T>(source, result, packedInput, pairedOutput,
                horizontalWeights, horizontalOffsets, horizontalCounts, y, scaleY, radius, filter, context))
            {
                y++;
                continue;
            }
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
                var packedSums = Vector256<double>.Zero;
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
                        if (packed8)
                        {
                            // One bounded load widens RGBA together. A final lane
                            // of one accumulates weighted alpha alongside RGB.
                            var pixel = BinaryPrimitives.ReadUInt32LittleEndian(packedInput.Slice(offset, 4));
                            var weightedAlpha = weight * (pixel >> 24);
                            var lanes = Vector128.CreateScalar((pixel & 0x00ffffffu) | 0x01000000u).AsByte();
                            var values = Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(lanes));
                            // Keep each lane's original multiply/add and tap order.
                            packedSums = Avx.Add(packedSums, Avx.Multiply(Vector256.Create(weightedAlpha), values));
                            weightSum += weight;
                            continue;
                        }
                        var a = double.CreateChecked(input[offset + 3]);
                        red += weight * a * double.CreateChecked(input[offset]);
                        green += weight * a * double.CreateChecked(input[offset + 1]);
                        blue += weight * a * double.CreateChecked(input[offset + 2]);
                        alpha += weight * a;
                        weightSum += weight;
                    }
                }
                if (packed8)
                {
                    red = packedSums.GetElement(0);
                    green = packedSums.GetElement(1);
                    blue = packedSums.GetElement(2);
                    alpha = packedSums.GetElement(3);
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
