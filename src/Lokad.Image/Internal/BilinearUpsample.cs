using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Fixed two-by-two RGBA8 triangle filtering with rounding-boundary fallback.</summary>
internal static class BilinearUpsample
{
    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        ReadOnlySpan<double> weights, ReadOnlySpan<int> offsets, ReadOnlySpan<int> counts,
OperationContext context)
    {
        var width = result.Width;
        var height = result.Height;
        var input = source.Pixels;
        var output = result.Pixels;
        var stride = source.Width * 4;
        var dx = 2L * width;
        var dy = 2L * height;
        // Horizontal premultiplied sums fit Int32 at these widths. Factoring
        // before widening preserves every integer lane, with four rather than
        // eight wide multiplies per group. Larger outputs retain the old path.
        var factorHorizontal = dx <= int.MaxValue / 65025;
        // Output array admission bounds 4*width*height below Int32.MaxValue.
        // Integer premultiplied sums are consequently below 2^47, exactly
        // representable in binary64. No intermediate pixel rounding occurs.
        var denominator = result.ByteLength;
        // Bound the original binary64 coordinate/weight/sum/division error.
        // Ambiguous quantization and tiny-alpha admission use that original path.
        var error = (Math.Max(source.Width, source.Height) + 1d) * Math.ScaleB(1d, -48);
        var alphaError = 16 * 255 * error;
        var colorError = 16 * 65025 * error;
        var errors = Vector512.Create(colorError, colorError, colorError, alphaError,
            colorError, colorError, colorError, alphaError);
        for (var y = 0; y < height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var cy = (2L * y + 1) * source.Height - height;
            var firstY = cy < 0 ? -1 : (int)(cy / dy);
            var fy = (int)(cy - firstY * dy);
            var wy0 = (int)dy - fy;
            var row0 = Math.Clamp(firstY, 0, source.Height - 1) * stride;
            var row1 = Math.Clamp(firstY + 1, 0, source.Height - 1) * stride;
            var x = 0;
            for (; x + 3 < width; x += 4)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var at = x * 8;
                var f0 = offsets[at + 3]; var f1 = offsets[at + 11];
                var f2 = offsets[at + 19]; var f3 = offsets[at + 27];
                var w0 = (int)dx - f0; var w1 = (int)dx - f1;
                var w2 = (int)dx - f2; var w3 = (int)dx - f3;
                var l0 = offsets[at + 4]; var l1 = offsets[at + 12];
                var l2 = offsets[at + 20]; var l3 = offsets[at + 28];
                var r0 = offsets[at + 5]; var r1 = offsets[at + 13];
                var r2 = offsets[at + 21]; var r3 = offsets[at + 29];
                var first = Vector512<long>.Zero;
                var second = Vector512<long>.Zero;
                if (factorHorizontal)
                {
                    var leftWeights = Vector512.Create(w0, w0, w0, w0, w1, w1, w1, w1,
                        w2, w2, w2, w2, w3, w3, w3, w3);
                    var rightWeights = Vector512.Create(f0, f0, f0, f0, f1, f1, f1, f1,
                        f2, f2, f2, f2, f3, f3, f3, f3);
                    var top = Horizontal(Gather(input, row0, l0, l1, l2, l3),
                        Gather(input, row0, r0, r1, r2, r3), leftWeights, rightWeights);
                    var bottom = Horizontal(Gather(input, row1, l0, l1, l2, l3),
                        Gather(input, row1, r0, r1, r2, r3), leftWeights, rightWeights);
                    var topWeight = Vector512.Create((long)wy0); var bottomWeight = Vector512.Create((long)fy);
                    first = Avx512F.Add(Avx512DQ.MultiplyLow(Avx512F.ConvertToVector512Int64(top.GetLower()), topWeight),
                        Avx512DQ.MultiplyLow(Avx512F.ConvertToVector512Int64(bottom.GetLower()), bottomWeight));
                    second = Avx512F.Add(Avx512DQ.MultiplyLow(Avx512F.ConvertToVector512Int64(top.GetUpper()), topWeight),
                        Avx512DQ.MultiplyLow(Avx512F.ConvertToVector512Int64(bottom.GetUpper()), bottomWeight));
                }
                else
                {
                    Tap(ref first, ref second, Gather(input, row0, l0, l1, l2, l3),
                        w0 * wy0, w1 * wy0, w2 * wy0, w3 * wy0);
                    Tap(ref first, ref second, Gather(input, row0, r0, r1, r2, r3),
                        f0 * wy0, f1 * wy0, f2 * wy0, f3 * wy0);
                    Tap(ref first, ref second, Gather(input, row1, l0, l1, l2, l3),
                        w0 * fy, w1 * fy, w2 * fy, w3 * fy);
                    Tap(ref first, ref second, Gather(input, row1, r0, r1, r2, r3),
                        f0 * fy, f1 * fy, f2 * fy, f3 * fy);
                }
                var sums01 = Avx512DQ.ConvertToVector512Double(first);
                var sums23 = Avx512DQ.ConvertToVector512Double(second);
                var a0 = sums01.GetElement(3); var a1 = sums01.GetElement(7);
                var a2 = sums23.GetElement(3); var a3 = sums23.GetElement(7);
                var v01 = Avx512F.Divide(sums01, Vector512.Create(a0, a0, a0, (double)denominator,
                    a1, a1, a1, (double)denominator));
                var v23 = Avx512F.Divide(sums23, Vector512.Create(a2, a2, a2, (double)denominator,
                    a3, a3, a3, (double)denominator));
                var mask01 = Ambiguous(v01, errors);
                var mask23 = Ambiguous(v23, errors);
                if (v01.GetElement(3) <= 1 + alphaError) mask01 |= 15;
                if (v01.GetElement(7) <= 1 + alphaError) mask01 |= 240;
                if (v23.GetElement(3) <= 1 + alphaError) mask23 |= 15;
                if (v23.GetElement(7) <= 1 + alphaError) mask23 |= 240;
                // All unambiguous nonnegative values round the same with
                // nearest-even and away-from-zero. Explicit rounding avoids
                // dependence on the host's floating-point rounding mode.
                var rounded01 = Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(v01, 0));
                var rounded23 = Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(v23, 0));
                var words01 = Sse41.PackUnsignedSaturate(rounded01.GetLower(), rounded01.GetUpper());
                var words23 = Sse41.PackUnsignedSaturate(rounded23.GetLower(), rounded23.GetUpper());
                var pixels = Sse2.PackUnsignedSaturate(words01.AsInt16(), words23.AsInt16());
                MemoryMarshal.Write(output.Slice((y * width + x) * 4, 16), in pixels);
                if ((mask01 & 15) != 0) Scalar(source, output, weights, offsets, counts, x, y, width, height, context);
                if ((mask01 & 240) != 0) Scalar(source, output, weights, offsets, counts, x + 1, y, width, height, context);
                if ((mask23 & 15) != 0) Scalar(source, output, weights, offsets, counts, x + 2, y, width, height, context);
                if ((mask23 & 240) != 0) Scalar(source, output, weights, offsets, counts, x + 3, y, width, height, context);
            }
            for (; x < width; x++)
                Scalar(source, output, weights, offsets, counts, x, y, width, height, context);
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<uint> Gather(ReadOnlySpan<byte> input, int row, int x0, int x1, int x2, int x3)
        => Vector128.Create(BinaryPrimitives.ReadUInt32LittleEndian(input.Slice(row + x0, 4)),
            BinaryPrimitives.ReadUInt32LittleEndian(input.Slice(row + x1, 4)),
            BinaryPrimitives.ReadUInt32LittleEndian(input.Slice(row + x2, 4)),
            BinaryPrimitives.ReadUInt32LittleEndian(input.Slice(row + x3, 4)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Tap(ref Vector512<long> first, ref Vector512<long> second,
        Vector128<uint> pixels, int w0, int w1, int w2, int w3)
    {
        var samples = Avx512F.ConvertToVector512Int32(pixels.AsByte());
        var alphas = Avx512F.PermuteVar16x32(samples,
            Vector512.Create(3, 3, 3, 3, 7, 7, 7, 7, 11, 11, 11, 11, 15, 15, 15, 15));
        var colors = Avx512F.Or(Avx512F.And(samples,
            Vector512.Create(-1, -1, -1, 0, -1, -1, -1, 0, -1, -1, -1, 0, -1, -1, -1, 0)),
            Vector512.Create(0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1));
        var premultiplied = Avx512F.MultiplyLow(colors, alphas);
        first = Avx512F.Add(first, Avx512DQ.MultiplyLow(Avx512F.ConvertToVector512Int64(premultiplied.GetLower()),
            Vector512.Create((long)w0, w0, w0, w0, w1, w1, w1, w1)));
        second = Avx512F.Add(second, Avx512DQ.MultiplyLow(Avx512F.ConvertToVector512Int64(premultiplied.GetUpper()),
            Vector512.Create((long)w2, w2, w2, w2, w3, w3, w3, w3)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<int> Horizontal(Vector128<uint> left, Vector128<uint> right,
        Vector512<int> leftWeights, Vector512<int> rightWeights)
        => Avx512F.Add(Avx512F.MultiplyLow(Premultiply(left), leftWeights),
            Avx512F.MultiplyLow(Premultiply(right), rightWeights));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<int> Premultiply(Vector128<uint> pixels)
    {
        var samples = Avx512F.ConvertToVector512Int32(pixels.AsByte());
        var alphas = Avx512F.PermuteVar16x32(samples,
            Vector512.Create(3, 3, 3, 3, 7, 7, 7, 7, 11, 11, 11, 11, 15, 15, 15, 15));
        var colors = Avx512F.Or(Avx512F.And(samples,
            Vector512.Create(-1, -1, -1, 0, -1, -1, -1, 0, -1, -1, -1, 0, -1, -1, -1, 0)),
            Vector512.Create(0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1));
        return Avx512F.MultiplyLow(colors, alphas);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Ambiguous(Vector512<double> values, Vector512<double> errors)
    {
        var fraction = Avx512F.Subtract(values, Avx512F.RoundScale(values, 1));
        var distance = Avx512F.AndNot(Vector512.Create(0x8000000000000000UL),
            Avx512F.Subtract(fraction, Vector512.Create(0.5)).AsUInt64()).AsDouble();
        var near = Avx512F.CompareLessThanOrEqual(distance, errors);
        return Avx.MoveMask(near.GetLower()) | (Avx.MoveMask(near.GetUpper()) << 4);
    }

    private static void Scalar(ImageBuffer source, Span<byte> output,
        ReadOnlySpan<double> weights, ReadOnlySpan<int> offsets, ReadOnlySpan<int> counts,
        int x, int y, int width, int height, OperationContext context)
    {
        var center = (y + 0.5) * source.Height / height - 0.5;
        var first = (int)Math.Ceiling(center - 1);
        var last = (int)Math.Floor(center + 1);
        double r = 0, g = 0, b = 0, a = 0, total = 0;
        for (var sy = first; sy <= last; sy++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var wy = Math.Max(0, 1 - Math.Abs(sy - center));
            if (wy == 0) continue;
            var row = source.GetRow8(Math.Clamp(sy, 0, source.Height - 1));
            for (var tap = 0; tap < counts[x]; tap++)
            {
                var weight = wy * weights[x * 8 + tap];
                if (weight == 0) continue;
                var pixel = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(offsets[x * 8 + tap], 4));
                var weighted = weight * (pixel >> 24);
                r += weighted * (pixel & 255); g += weighted * (pixel >> 8 & 255); b += weighted * (pixel >> 16 & 255);
                a += weighted; total += weight;
            }
        }
        var normalizedAlpha = a / total;
        var roundedAlpha = normalizedAlpha > 0 && normalizedAlpha < 1
            ? ResizeAlpha.Round(source, x, y, width, height, ResizeFilter.Bilinear, context) : Round(normalizedAlpha);
        var packed = (uint)roundedAlpha << 24;
        if (roundedAlpha > 0 && a > 0)
            packed |= (uint)Round(r / a) | ((uint)Round(g / a) << 8) | ((uint)Round(b / a) << 16);
        BinaryPrimitives.WriteUInt32LittleEndian(output.Slice((y * width + x) * 4, 4), packed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Round(double value) => (int)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
}
