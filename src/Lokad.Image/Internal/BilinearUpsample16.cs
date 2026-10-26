using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Fixed two-by-two RGBA16 triangle filtering with rounding-boundary fallback.</summary>
internal static class BilinearUpsample16
{
    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        ReadOnlySpan<double> weights, ReadOnlySpan<int> offsets, ReadOnlySpan<int> counts,
OperationContext context)
    {
        var width = result.Width;
        var cacheElements = (long)width * 16;
        var cacheBytes = cacheElements * sizeof(long) + 64;
        if (cacheElements > Array.MaxLength || cacheBytes > context.RemainingWorkingBytes - ResizeAlpha.ScratchBytes)
            return false;
        context.Reserve(cacheBytes);
        try
        {
            var cache = new long[(int)cacheElements];
            Span<int> cachedRows = stackalloc int[4];
            cachedRows.Fill(-1);
            var height = result.Height;
            var input = source.Pixels;
            var output = result.Pixels;
            var dy = 2L * height;
            // Output array admission bounds 4*width*height below Int32.MaxValue.
            // Integer premultiplied sums are consequently below 2^62. Integer accumulation is exact; final
            // binary64 conversion is covered by the rounding-boundary fallback.
            var denominator = result.ByteLength / 2;
            // Bound the original binary64 coordinate/weight/sum/division error.
            // Ambiguous quantization and tiny-alpha admission use that original path.
            var error = (Math.Max(source.Width, source.Height) + 1d) * Math.ScaleB(1d, -48);
            var alphaError = 16 * 65535 * error;
            var colorError = 16 * 65535d * 65535 * error;

            for (var y = 0; y < height; y++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var cy = (2L * y + 1) * source.Height - height;
                var firstY = cy < 0 ? -1 : (int)(cy / dy);
                var fy = (int)(cy - firstY * dy);
                var wy0 = (int)dy - fy;
                var row0 = Prepare(source, firstY, cache, cachedRows, offsets, width, context);
                var row1 = Prepare(source, firstY + 1, cache, cachedRows, offsets, width, context);
                var x = 0;
                for (; x + 3 < width; x += 4)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var at = x * 4;
                    var first = Avx512F.Add(Avx512DQ.MultiplyLow(Read(cache, row0 + at), Vector512.Create((long)wy0)),
                        Avx512DQ.MultiplyLow(Read(cache, row1 + at), Vector512.Create((long)fy)));
                    var second = Avx512F.Add(Avx512DQ.MultiplyLow(Read(cache, row0 + at + 8), Vector512.Create((long)wy0)),
                        Avx512DQ.MultiplyLow(Read(cache, row1 + at + 8), Vector512.Create((long)fy)));
                    var sums01 = Avx512DQ.ConvertToVector512Double(first);
                    var sums23 = Avx512DQ.ConvertToVector512Double(second);
                    var a0 = sums01.GetElement(3); var a1 = sums01.GetElement(7);
                    var a2 = sums23.GetElement(3); var a3 = sums23.GetElement(7);
                    var v01 = Avx512F.Divide(sums01, Vector512.Create(a0, a0, a0, (double)denominator,
                        a1, a1, a1, (double)denominator));
                    var v23 = Avx512F.Divide(sums23, Vector512.Create(a2, a2, a2, (double)denominator,
                        a3, a3, a3, (double)denominator));
                    var mask01 = Ambiguous(v01, Errors(v01, colorError, alphaError));
                    var mask23 = Ambiguous(v23, Errors(v23, colorError, alphaError));
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
                    var pixels = Vector256.Create(words01, words23);
                    MemoryMarshal.Write(output.Slice((y * width + x) * 8, 32), in pixels);
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
        finally { context.Release(cacheBytes); }
    }

    private static int Prepare(ImageBuffer source, int row, Span<long> cache, Span<int> cachedRows,
        ReadOnlySpan<int> offsets, int width, OperationContext context)
    {
        row = Math.Clamp(row, 0, source.Height - 1);
        var slot = row & 3;
        var start = slot * width * 4;
        if (cachedRows[slot] == row) return start;
        var input = source.Pixels;
        var rowOffset = row * source.Width * 8;
        var dx = 2 * width;
        var x = 0;
        for (; x + 3 < width; x += 4)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var at = x * 8;
            var f0 = offsets[at + 3]; var f1 = offsets[at + 11];
            var f2 = offsets[at + 19]; var f3 = offsets[at + 27];
            var first = Vector512<long>.Zero;
            var second = Vector512<long>.Zero;
            Tap(ref first, ref second, Gather(input, rowOffset,
                offsets[at + 4], offsets[at + 12], offsets[at + 20], offsets[at + 28]),
                dx - f0, dx - f1, dx - f2, dx - f3);
            Tap(ref first, ref second, Gather(input, rowOffset,
                offsets[at + 5], offsets[at + 13], offsets[at + 21], offsets[at + 29]), f0, f1, f2, f3);
            MemoryMarshal.Write(MemoryMarshal.AsBytes(cache.Slice(start + x * 4, 8)), in first);
            MemoryMarshal.Write(MemoryMarshal.AsBytes(cache.Slice(start + x * 4 + 8, 8)), in second);
        }
        for (; x < width; x++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var at = x * 8;
            var f = offsets[at + 3];
            var p0 = BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(rowOffset + offsets[at + 4] * 2, 8));
            var p1 = BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(rowOffset + offsets[at + 5] * 2, 8));
            var a0 = (long)(p0 >> 48) * (dx - f); var a1 = (long)(p1 >> 48) * f;
            var target = start + x * 4;
            cache[target] = a0 * (long)(p0 & 65535) + a1 * (long)(p1 & 65535);
            cache[target + 1] = a0 * (long)(p0 >> 16 & 65535) + a1 * (long)(p1 >> 16 & 65535);
            cache[target + 2] = a0 * (long)(p0 >> 32 & 65535) + a1 * (long)(p1 >> 32 & 65535);
            cache[target + 3] = a0 + a1;
        }
        cachedRows[slot] = row;
        return start;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<long> Read(ReadOnlySpan<long> cache, int at)
        => MemoryMarshal.Read<Vector512<long>>(MemoryMarshal.AsBytes(cache.Slice(at, 8)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ulong> Gather(ReadOnlySpan<byte> input, int row, int x0, int x1, int x2, int x3)
        => Vector256.Create(BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(row + x0 * 2, 8)),
            BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(row + x1 * 2, 8)),
            BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(row + x2 * 2, 8)),
            BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(row + x3 * 2, 8)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Tap(ref Vector512<long> first, ref Vector512<long> second,
        Vector256<ulong> pixels, int w0, int w1, int w2, int w3)
    {
        var samples = Avx512F.ConvertToVector512Int32(pixels.AsUInt16());
        var alphas = Avx512F.PermuteVar16x32(samples,
            Vector512.Create(3, 3, 3, 3, 7, 7, 7, 7, 11, 11, 11, 11, 15, 15, 15, 15));
        var colors = Avx512F.Or(Avx512F.And(samples,
            Vector512.Create(-1, -1, -1, 0, -1, -1, -1, 0, -1, -1, -1, 0, -1, -1, -1, 0)),
            Vector512.Create(0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1));
        var premultiplied = Avx512F.MultiplyLow(colors, alphas);
        first = Avx512F.Add(first, Avx512DQ.MultiplyLow(Avx512F.ConvertToVector512Int64(premultiplied.GetLower().AsUInt32()),
            Vector512.Create((long)w0, w0, w0, w0, w1, w1, w1, w1)));
        second = Avx512F.Add(second, Avx512DQ.MultiplyLow(Avx512F.ConvertToVector512Int64(premultiplied.GetUpper().AsUInt32()),
            Vector512.Create((long)w2, w2, w2, w2, w3, w3, w3, w3)));
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<double> Errors(Vector512<double> values, double colorError, double alphaError)
    {
        var alpha = Avx512F.PermuteVar8x64(values, Vector512.Create(3L, 3, 3, 3, 7, 7, 7, 7));
        var safeAlpha = Avx512F.Subtract(alpha, Vector512.Create(alphaError));
        var exponent = Avx512F.And(Avx512F.ShiftRightLogical(safeAlpha.AsUInt64(), 52), Vector512.Create(2047UL));
        // The reciprocal of alpha's lower power-of-two bounds 1/alpha above.
        // Tiny/negative lanes are discarded by the original-alpha fallback.
        var reciprocal = Avx512F.ShiftLeftLogical(Avx512F.Subtract(Vector512.Create(2046UL), exponent), 52).AsDouble();
        return Avx512F.Multiply(Vector512.Create(colorError), reciprocal).WithElement(3, alphaError).WithElement(7, alphaError);
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
            var row = source.GetRow16(Math.Clamp(sy, 0, source.Height - 1));
            for (var tap = 0; tap < counts[x]; tap++)
            {
                var weight = wy * weights[x * 8 + tap];
                if (weight == 0) continue;
                var pixel = BinaryPrimitives.ReadUInt64LittleEndian(MemoryMarshal.AsBytes(row.Slice(offsets[x * 8 + tap], 4)));
                var weighted = weight * (pixel >> 48);
                r += weighted * (pixel & 65535); g += weighted * (pixel >> 16 & 65535); b += weighted * (pixel >> 32 & 65535);
                a += weighted; total += weight;
            }
        }
        var normalizedAlpha = a / total;
        var roundedAlpha = normalizedAlpha > 0 && normalizedAlpha < 1
            ? ResizeAlpha.Round(source, x, y, width, height, ResizeFilter.Bilinear, context) : Round(normalizedAlpha);
        var packed = (ulong)(uint)roundedAlpha << 48;
        if (roundedAlpha > 0 && a > 0)
            packed |= (ulong)(uint)Round(r / a) | ((ulong)(uint)Round(g / a) << 16) | ((ulong)(uint)Round(b / a) << 32);
        BinaryPrimitives.WriteUInt64LittleEndian(output.Slice((y * width + x) * 8, 8), packed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Round(double value) => (int)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 65535);
}
