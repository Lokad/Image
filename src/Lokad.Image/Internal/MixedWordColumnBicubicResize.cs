using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Vertical filtering reuses source columns for a small output-row band.</summary>
internal static class MixedWordColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, long rowWork, OperationContext context)
    {
        if (source.BitDepth != 16 || result.Width <= source.Width || result.Height >= source.Height
            || tapsX > 5 || tapsY > 10
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // These fixed arrays occupy 2,016 bytes. Intermediate values remain
            // unrounded; the frame returns before exact tiny-alpha scratch.
            Span<Vector256<double>> cache = stackalloc Vector256<double>[5 * 4];
            Span<int> columnIds = stackalloc int[5];
            Span<Vector256<double>> decoded = stackalloc Vector256<double>[24];
            Span<double> vertical = stackalloc double[4 * 10];
            Span<int> rowOffsets = stackalloc int[4 * 10];
            Span<int> counts = stackalloc int[4];
            Span<double> normalizers = stackalloc double[4];
            Span<double> horizontal = stackalloc double[5];
            Span<int> columns = stackalloc int[5];
            var input = source.Pixels;
            var inputStride = source.Stride;
            var output = result.Pixels;
            var outputStride = result.Stride;
            for (var tileY = 0; tileY < result.Height; tileY += 4)
            {
                var length = Math.Min(4, result.Height - tileY);
                var firstCenter = (tileY + .5) * source.Height / result.Height - .5;
                var sourceFirst = Math.Clamp((int)Math.Ceiling(firstCenter - 2 * scaleY), 0, source.Height - 1);
                for (var y = 0; y < length; y++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var center = (tileY + y + .5) * source.Height / result.Height - .5;
                    var first = (int)Math.Ceiling(center - 2 * scaleY);
                    var count = (int)Math.Floor(center + 2 * scaleY) - first + 1;
                    counts[y] = count;
                    double normalizer = 0;
                    for (var t = 0; t < count; t++)
                    {
                        var w = ImageTransforms.Kernel((first + t - center) / scaleY, ResizeFilter.Bicubic);
                        vertical[y * 10 + t] = w; normalizer += w;
                        var relative = Math.Clamp(first + t, 0, source.Height - 1) - sourceFirst;
                        if ((uint)relative >= 24) return false;
                        rowOffsets[y * 10 + t] = relative;
                    }
                    normalizers[y] = normalizer;
                }
                var sourceCount = rowOffsets[(length - 1) * 10 + counts[length - 1] - 1] + 1;
                columnIds.Fill(-1);
                for (var x = 0; x < result.Width; x++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var center = (x + .5) * source.Width / result.Width - .5;
                    // Enlargement fixes scaleX at one, so this keeps exactly the
                    // original binary64 coefficient values without a division.
                    var first = (int)Math.Ceiling(center - 2);
                    var count = (int)Math.Floor(center + 2) - first + 1;
                    double normalizerX = 0;
                    for (var t = 0; t < count; t++)
                    {
                        var w = ImageTransforms.Kernel(first + t - center, ResizeFilter.Bicubic);
                        horizontal[t] = w; normalizerX += w;
                        var sx = Math.Clamp(first + t, 0, source.Width - 1);
                        var slot = sx % 5; columns[t] = slot * 4;
                        if (columnIds[slot] == sx) continue;
                        columnIds[slot] = sx;
                        var sy = 0;
                        for (; sy + 1 < sourceCount; sy += 2)
                        {
                            var p0 = BinaryPrimitives.ReadUInt64LittleEndian(input.Slice((sourceFirst + sy) * inputStride + sx * 8, 8));
                            var p1 = BinaryPrimitives.ReadUInt64LittleEndian(input.Slice((sourceFirst + sy + 1) * inputStride + sx * 8, 8));
                            var samples = Avx2.ConvertToVector256Int32(Vector128.Create(p0, p1).AsUInt16());
                            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                            var products = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors).AsUInt32());
                            decoded[sy] = products.GetLower(); decoded[sy + 1] = products.GetUpper();
                        }
                        if (sy < sourceCount)
                        {
                            var packed = BinaryPrimitives.ReadUInt64LittleEndian(input.Slice((sourceFirst + sy) * inputStride + sx * 8, 8));
                            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsUInt16());
                            var alpha = (int)(packed >> 48);
                            var products = Sse41.MultiplyLow(samples, Vector128.Create(alpha, alpha, alpha, 1)).AsUInt32();
                            decoded[sy] = Avx512F.ConvertToVector512Double(Vector256.Create(products, Vector128<uint>.Zero)).GetLower();
                        }
                        for (var y = 0; y < length; y++)
                        {
                            var weights = vertical.Slice(y * 10, counts[y]);
                            var offsets = rowOffsets.Slice(y * 10, counts[y]);
                            var sum = Vector256<double>.Zero;
                            for (var ty = 0; ty < weights.Length; ty++)
                                sum = Avx.Add(sum, Avx.Multiply(decoded[offsets[ty]], Vector256.Create(weights[ty])));
                            cache[slot * 4 + y] = sum;
                        }
                    }
                    var outputY = 0;
                    for (; outputY + 1 < length; outputY += 2)
                    {
                        var sum = Vector512<double>.Zero;
                        for (var t = 0; t < count; t++)
                        {
                            var pair = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cache.Slice(columns[t] + outputY, 2)));
                            sum = Avx512F.Add(sum, Avx512F.Multiply(pair, Vector512.Create(horizontal[t])));
                        }
                        var n0 = normalizerX * normalizers[outputY]; var n1 = normalizerX * normalizers[outputY + 1];
                        // A packed pair is staged in existing decoded storage;
                        // its two rows then receive separate checked word stores.
                        var packed = MemoryMarshal.AsBytes(decoded)[..16];
                        if (TryWritePair(sum, n0, n1, packed))
                        {
                            BinaryPrimitives.WriteUInt64LittleEndian(output.Slice((tileY + outputY) * outputStride + x * 8, 8),
                                BinaryPrimitives.ReadUInt64LittleEndian(packed[..8]));
                            BinaryPrimitives.WriteUInt64LittleEndian(output.Slice((tileY + outputY + 1) * outputStride + x * 8, 8),
                                BinaryPrimitives.ReadUInt64LittleEndian(packed[8..]));
                        }
                        else if (!OriginalWord(source, result, x, tileY + outputY, center, first,
                            vertical.Slice(outputY * 10, counts[outputY]), rowOffsets.Slice(outputY * 10, counts[outputY]), sourceFirst, context)
                            || !OriginalWord(source, result, x, tileY + outputY + 1, center, first,
                                vertical.Slice((outputY + 1) * 10, counts[outputY + 1]), rowOffsets.Slice((outputY + 1) * 10, counts[outputY + 1]), sourceFirst, context)) return false;
                    }
                    for (var y = outputY; y < length; y++)
                    {
                        var sum = Vector256<double>.Zero;
                        for (var t = 0; t < count; t++)
                            sum = Avx.Add(sum, Avx.Multiply(cache[columns[t] + y], Vector256.Create(horizontal[t])));
                        var normalizer = normalizerX * normalizers[y];
                        if (!TryWriteSingle(sum, normalizer, output.Slice((tileY + y) * outputStride + x * 8, 8))
                            && !OriginalWord(source, result, x, tileY + y, center, first,
                                vertical.Slice(y * 10, counts[y]), rowOffsets.Slice(y * 10, counts[y]), sourceFirst, context)) return false;
                    }
                }
            }
            context.Work(rowWork * result.Height);
            return true;
        }
        finally { context.Release(WorkspaceBytes); }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
        const double maximum = 65535d;
        const double alphaError = unitError * maximum;
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 > alphaError && a1 > alphaError)) return false;
        var divisors = Vector512.Create(a0, a0, a0, n0, a1, a1, a1, n1);
        var values = Avx512F.Divide(sum, divisors);
        // Established 2^-40*16^2 word bounds cover every in-range
        // rounding threshold: alpha margin 1e-4, color below 2.0001/alpha.
        // Alpha's lower power of two gives a conservative reciprocal bound.
        var exponent0 = (int)((BitConverter.DoubleToUInt64Bits(a0) >> 52) & 2047);
        var exponent1 = (int)((BitConverter.DoubleToUInt64Bits(a1) >> 52) & 2047);
        var margin0 = 2.0001 * BitConverter.UInt64BitsToDouble((ulong)(2046 - exponent0) << 52);
        var margin1 = 2.0001 * BitConverter.UInt64BitsToDouble((ulong)(2046 - exponent1) << 52);
        if (margin0 >= .25 || margin1 >= .25
            || !(values.GetElement(3) >= 1 + 1e-4 && values.GetElement(7) >= 1 + 1e-4)) return false;
        var margins = Vector512.Create(margin0, margin0, margin0, 1e-4, margin1, margin1, margin1, 1e-4);
        var fractional = Avx512F.Subtract(values, Avx512F.RoundScale(values, 1));
        var distance = Avx512F.AndNot(Vector512.Create(long.MinValue),
            Avx512F.Subtract(fractional, Vector512.Create(.5)).AsInt64()).AsDouble();
        var inRange = Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values, Vector512<double>.Zero).AsInt64(),
            Avx512F.CompareLessThanOrEqual(values, Vector512.Create(maximum)).AsInt64());
        var ambiguous = Avx512F.And(inRange, Avx512F.CompareLessThanOrEqual(distance, margins).AsInt64());
        if (Vector512.ExtractMostSignificantBits(ambiguous) != 0) return false;
        var clipped = Avx512F.Min(Vector512.Create(maximum), Avx512F.Max(Vector512<double>.Zero, values));
        var ints = Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped, 0));
        var words = Sse41.PackUnsignedSaturate(ints.GetLower(), ints.GetUpper());
        MemoryMarshal.Write(output, in words);
        return true;
    }

    private static bool TryWriteSingle(Vector256<double> sum, double normalizer, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
        const double maximum = 65535d;
        const double alphaError = unitError * maximum;
        var alpha = sum.GetElement(3);
        if (!(normalizer >= .5 && alpha > alphaError)) return false;
        var values = Avx.Divide(sum, Vector256.Create(alpha, alpha, alpha, normalizer));
        var magnitude = Math.Max(Math.Abs(values.GetElement(0)),
            Math.Max(Math.Abs(values.GetElement(1)), Math.Abs(values.GetElement(2))));
        var colorMargin = 2 * (alphaError * maximum + magnitude * alphaError) / (alpha - alphaError);
        var alphaMargin = 2 * (alphaError + Math.Abs(values.GetElement(3)) * unitError) / (normalizer - unitError);
        if (colorMargin >= .25 || !(values.GetElement(3) >= 1 + alphaMargin)) return false;
        for (var c = 0; c < 4; c++)
        {
            var value = values.GetElement(c);
            if (value >= 0 && value <= maximum
                && Math.Abs(value - Math.Floor(value) - .5) <= (c == 3 ? alphaMargin : colorMargin)) return false;
        }
        var clipped = Avx.Min(Vector256.Create(maximum), Avx.Max(Vector256<double>.Zero, values));
        var ints = Avx.ConvertToVector128Int32WithTruncation(Avx.RoundToNearestInteger(clipped));
        var words = Sse41.PackUnsignedSaturate(ints, Vector128<int>.Zero).AsUInt64().GetElement(0);
        BinaryPrimitives.WriteUInt64LittleEndian(output, words);
        return true;
    }

    private static bool OriginalWord(ImageBuffer source, ImageBuffer result, int x, int y,
        double centerX, int firstX, ReadOnlySpan<double> vertical, ReadOnlySpan<int> rows,
        int sourceFirst, OperationContext context)
    {
        var lastX = (int)Math.Floor(centerX + 2);
        double red = 0, green = 0, blue = 0, alphaSum = 0, normalizer = 0;
        for (var ty = 0; ty < vertical.Length; ty++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var wy = vertical[ty]; if (wy == 0) continue;
            var row = source.GetRow16(sourceFirst + rows[ty]);
            for (var sx = firstX; sx <= lastX; sx++)
            {
                var w = wy * ImageTransforms.Kernel(sx - centerX, ResizeFilter.Bicubic);
                if (w == 0) continue;
                var offset = Math.Clamp(sx, 0, source.Width - 1) * 4;
                var alpha = row[offset + 3];
                red += w * alpha * row[offset]; green += w * alpha * row[offset + 1]; blue += w * alpha * row[offset + 2];
                alphaSum += w * alpha; normalizer += w;
            }
        }
        var normalized = alphaSum / normalizer;
        if (normalized > 0 && normalized < 1) return false;
        var alphaOut = RoundWord(normalized);
        var color = alphaOut > 0 && alphaSum > 0;
        var destination = result.GetRow16(y).Slice(x * 4, 4);
        destination[0] = RoundWord(color ? red / alphaSum : 0);
        destination[1] = RoundWord(color ? green / alphaSum : 0);
        destination[2] = RoundWord(color ? blue / alphaSum : 0);
        destination[3] = alphaOut;
        return true;
    }

    private static ushort RoundWord(double value)
        => (ushort)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 65535);
}
