using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Two independent RGBA output rows sharing horizontal geometry.</summary>
internal static class PairedResize
{
    internal static bool TryRows<T>(ImageBuffer source, ImageBuffer result,
        ReadOnlySpan<byte> input, Span<byte> output, ReadOnlySpan<double> horizontalWeights,
        ReadOnlySpan<int> horizontalOffsets, ReadOnlySpan<int> horizontalCounts, int y,
        double scaleY, int radius, ResizeFilter filter, long rowWork, OperationContext context) where T : unmanaged
    {
        var height = result.Height;
        if (y + 1 >= height) return false;
        var sourceHeight = source.Height;
        var center0 = (y + 0.5) * sourceHeight / height - 0.5;
        var center1 = (y + 1.5) * sourceHeight / height - 0.5;
        var first0 = (int)Math.Ceiling(center0 - radius * scaleY);
        var first1 = (int)Math.Ceiling(center1 - radius * scaleY);
        var count = (int)Math.Floor(center0 + radius * scaleY) - first0 + 1;
        if ((int)Math.Floor(center1 + radius * scaleY) - first1 + 1 != count) return false;

        // The caller has charged the first row. Each accepted pair charges both.
        context.Work(rowWork);
        var width = result.Width;
        var wide16 = typeof(T) == typeof(ushort);
        var sampleBytes = wide16 ? 2 : 1;
        var stride = source.Width * 4 * sampleBytes;
        Span<double> verticalWeights = stackalloc double[16];
        Span<int> verticalOffsets = stackalloc int[16];
        for (var tap = 0; tap < count; tap++)
        {
            verticalWeights[tap] = Kernel((first0 + tap - center0) / scaleY, filter);
            verticalWeights[tap + 8] = Kernel((first1 + tap - center1) / scaleY, filter);
            verticalOffsets[tap] = Math.Clamp(first0 + tap, 0, sourceHeight - 1) * stride;
            verticalOffsets[tap + 8] = Math.Clamp(first1 + tap, 0, sourceHeight - 1) * stride;
        }
        for (var x = 0; x < width; x++)
        {
            var baseX = x * 8;
            var weights = horizontalWeights.Slice(baseX, horizontalCounts[x]);
            var offsets = horizontalOffsets.Slice(baseX, weights.Length);
            var quad = !wide16 && weights.Length == 4 && offsets[1] == offsets[0] + 4
                && offsets[2] == offsets[0] + 8 && offsets[3] == offsets[0] + 12;
            var quadWeights = quad
                ? MemoryMarshal.Read<Vector256<double>>(MemoryMarshal.AsBytes(weights))
                : Vector256<double>.Zero;
            var sums = Vector512<double>.Zero;
            double weightSum0 = 0, weightSum1 = 0;
            for (var ty = 0; ty < count; ty++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var wy0 = verticalWeights[ty];
                var wy1 = verticalWeights[ty + 8];
                if (wy0 == 0 && wy1 == 0) continue;
                var row0 = verticalOffsets[ty];
                var row1 = verticalOffsets[ty + 8];
                if (quad)
                {
                    // Consecutive support admits one bounded read per row.
                    // Interleaving keeps each pair's two output pixels independent.
                    var first = offsets[0];
                    var pixels0 = MemoryMarshal.Read<Vector128<uint>>(input.Slice(row0 + first, 16));
                    var pixels1 = MemoryMarshal.Read<Vector128<uint>>(input.Slice(row1 + first, 16));
                    var low = Sse2.UnpackLow(pixels0, pixels1);
                    var high = Sse2.UnpackHigh(pixels0, pixels1);
                    var weights0 = Avx.Multiply(Vector256.Create(wy0), quadWeights);
                    var weights1 = Avx.Multiply(Vector256.Create(wy1), quadWeights);
                    var weighted0 = Avx.Multiply(weights0,
                        Avx.ConvertToVector256Double(Sse2.ShiftRightLogical(pixels0, 24).AsInt32()));
                    var weighted1 = Avx.Multiply(weights1,
                        Avx.ConvertToVector256Double(Sse2.ShiftRightLogical(pixels1, 24).AsInt32()));
                    // Prepare four coefficients together; consume their taps in
                    // the original order, with separate multiply/add operations.
                    sums = Tap(sums, low, weighted0.GetElement(0), weighted1.GetElement(0));
                    weightSum0 += weights0.GetElement(0); weightSum1 += weights1.GetElement(0);
                    sums = Tap(sums, Sse2.ShiftRightLogical128BitLane(low.AsByte(), 8).AsUInt32(),
                        weighted0.GetElement(1), weighted1.GetElement(1));
                    weightSum0 += weights0.GetElement(1); weightSum1 += weights1.GetElement(1);
                    sums = Tap(sums, high, weighted0.GetElement(2), weighted1.GetElement(2));
                    weightSum0 += weights0.GetElement(2); weightSum1 += weights1.GetElement(2);
                    sums = Tap(sums, Sse2.ShiftRightLogical128BitLane(high.AsByte(), 8).AsUInt32(),
                        weighted0.GetElement(3), weighted1.GetElement(3));
                    weightSum0 += weights0.GetElement(3); weightSum1 += weights1.GetElement(3);
                }
                else for (var tx = 0; tx < weights.Length; tx++)
                {
                    var weight0 = wy0 * weights[tx];
                    var weight1 = wy1 * weights[tx];
                    if (weight0 == 0 && weight1 == 0) continue;
                    var offset = offsets[tx] * sampleBytes;
                    if (wide16)
                    {
                        // Each bounded UInt64 load contains one full RGBA16 pixel.
                        // Two pixels occupy separate four-channel double lanes;
                        // no tap reassociation or fused multiply-add is introduced.
                        var pixel16a = BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(row0 + offset, 8));
                        var pixel16b = BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(row1 + offset, 8));
                        var pixels16 = Vector128.Create(
                            (pixel16a & 0x0000ffffffffffffUL) | 0x0001000000000000UL,
                            (pixel16b & 0x0000ffffffffffffUL) | 0x0001000000000000UL).AsUInt16();
                        var samples16 = Avx512F.ConvertToVector512Double(Avx2.ConvertToVector256Int32(pixels16));
                        var weighted16 = Avx512F.InsertVector256(Vector512.Create(weight0 * (pixel16a >> 48)),
                            Vector256.Create(weight1 * (pixel16b >> 48)), 1);
                        sums = Avx512F.Add(sums, Avx512F.Multiply(weighted16, samples16));
                        weightSum0 += weight0; weightSum1 += weight1;
                        continue;
                    }
                    var pixel0 = BinaryPrimitives.ReadUInt32LittleEndian(input.Slice(row0 + offset, 4));
                    var pixel1 = BinaryPrimitives.ReadUInt32LittleEndian(input.Slice(row1 + offset, 4));
                    var pixels = Vector128.CreateScalar(
                        ((pixel0 | ((ulong)pixel1 << 32)) & 0x00ffffff00ffffffUL) | 0x0100000001000000UL).AsByte();
                    var samples = Avx512F.ConvertToVector512Double(Avx2.ConvertToVector256Int32(pixels));
                    var weighted = Avx512F.InsertVector256(Vector512.Create(weight0 * (pixel0 >> 24)),
                        Vector256.Create(weight1 * (pixel1 >> 24)), 1);
                    // Separate lanes preserve each pixel's original tap and
                    // multiply/add order. An inactive lane only adds zero.
                    sums = Avx512F.Add(sums, Avx512F.Multiply(weighted, samples));
                    weightSum0 += weight0;
                    weightSum1 += weight1;
                }
                context.Cancellation.ThrowIfCancellationRequested();
            }
            Store<T>(source, output, sums.GetLower(), weightSum0, x, y, width, height, filter, context);
            Store<T>(source, output, sums.GetUpper(), weightSum1, x, y + 1, width, height, filter, context);
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<double> Tap(Vector512<double> sums, Vector128<uint> pixels,
        double weighted0, double weighted1)
    {
        var weighted = Avx512F.InsertVector256(Vector512.Create(weighted0),
            Vector256.Create(weighted1), 1);
        var colors = Sse2.Or(Sse2.And(pixels.AsUInt64(), Vector128.Create(0x00ffffff00ffffffUL)),
            Vector128.Create(0x0100000001000000UL)).AsByte();
        var samples = Avx512F.ConvertToVector512Double(Avx2.ConvertToVector256Int32(colors));
        return Avx512F.Add(sums, Avx512F.Multiply(weighted, samples));
    }

    private static void Store<T>(ImageBuffer source, Span<byte> output, Vector256<double> sums,
        double weightSum, int x, int y, int width, int height, ResizeFilter filter, OperationContext context) where T : unmanaged
    {
        if (typeof(T) == typeof(ushort))
        {
            Store16(source, output, sums, weightSum, x, y, width, height, filter, context);
            return;
        }
        var alpha = sums.GetElement(3);
        var normalizedAlpha = alpha / weightSum;
        var roundedAlpha = normalizedAlpha > 0 && normalizedAlpha < 1
            ? ResizeAlpha.Round(source, x, y, width, height, filter, context)
            : Round(normalizedAlpha, 255);
        var hasColor = roundedAlpha > 0 && alpha > 0;
        var destination = (y * width + x) * 4;
        output[destination] = (byte)Round(hasColor ? sums.GetElement(0) / alpha : 0, 255);
        output[destination + 1] = (byte)Round(hasColor ? sums.GetElement(1) / alpha : 0, 255);
        output[destination + 2] = (byte)Round(hasColor ? sums.GetElement(2) / alpha : 0, 255);
        output[destination + 3] = (byte)roundedAlpha;
    }

    private static void Store16(ImageBuffer source, Span<byte> output, Vector256<double> sums,
        double weightSum, int x, int y, int width, int height, ResizeFilter filter, OperationContext context)
    {
        var alpha = sums.GetElement(3);
        var normalizedAlpha = alpha / weightSum;
        var tinyAlpha = normalizedAlpha > 0 && normalizedAlpha < 1;
        var alphaValue = tinyAlpha ? ResizeAlpha.Round(source, x, y, width, height, filter, context) : normalizedAlpha;
        var hasColor = alpha > 0 && alphaValue >= 1;
        var values = hasColor ? Avx.Divide(sums, Vector256.Create(alpha)) : Vector256<double>.Zero;
        values = values.WithElement(3, alphaValue);
        var packed = Round16(values);
        BinaryPrimitives.WriteUInt64LittleEndian(output.Slice((y * width + x) * 8, 8), packed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Round16(Vector256<double> values)
    {
        values = Avx.Min(Avx.Max(values, Vector256<double>.Zero), Vector256.Create(65535d));
        var integral = Avx.RoundToNegativeInfinity(values);
        // Subtract the integral part before testing halves. Adding 0.5 first
        // can incorrectly round a representable value just below a half.
        var fraction = Avx.Subtract(values, integral);
        var increment = Avx.And(Avx.Compare(fraction, Vector256.Create(0.5), FloatComparisonMode.OrderedGreaterThanOrEqualNonSignaling),
            Vector256.Create(1d));
        var rounded = Avx.ConvertToVector128Int32WithTruncation(Avx.Add(integral, increment));
        return Sse41.PackUnsignedSaturate(rounded, Vector128<int>.Zero).AsUInt64().GetElement(0);
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
