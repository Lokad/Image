using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Word filtering and exact publication for the shared streaming driver.</summary>
internal static class StreamingWordBicubicResize
{
    private const int Capacity = BicubicMath.StreamingTapCapacity;

    internal static bool WriteBlock(ImageBuffer source, ImageBuffer result, int x, int blockStart,
        ReadOnlySpan<double> weights, ReadOnlySpan<ushort> offsets, int sourceFirst,
        double normalizerX, ReadOnlySpan<Vector256<double>> totals, ReadOnlySpan<double> normalizers,
        ReadOnlySpan<double> centers, ReadOnlySpan<int> first, ReadOnlySpan<int> last,
        double scaleY, OperationContext context)
    {
        var height = totals.Length;
        for (var y = 0; y < height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var normalizer = normalizerX * normalizers[y];
            if (!TryWrite(totals[y], normalizer, result.Pixels.Slice((blockStart + y) * result.Stride + x * 8, 8)))
            {
                if (!(normalizer >= .5 && totals[y].GetElement(3) / normalizer >= 2)
                    || !OriginalWord(source, result, x, blockStart + y, weights, offsets,
                        sourceFirst, centers[y], first[y], last[y], scaleY, context)) return false;
            }
        }
        return true;
    }

    internal static Vector256<double> FilterRow(ReadOnlySpan<byte> row,
        ReadOnlySpan<double> weights, ReadOnlySpan<ushort> offsets)
    {
        var sum = Vector256<double>.Zero;
        var t = 0;
        for (; t + 1 < weights.Length; t += 2)
        {
            var p0 = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(offsets[t] * 8, 8));
            var p1 = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(offsets[t + 1] * 8, 8));
            var samples = Avx2.ConvertToVector256Int32(Vector128.Create(p0, p1).AsUInt16());
            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
            var values = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors).AsUInt32());
            var w0 = weights[t]; var w1 = weights[t + 1];
            var products = Avx512F.Multiply(values, Vector512.Create(w0, w0, w0, w0, w1, w1, w1, w1));
            sum = Avx.Add(sum, products.GetLower());
            sum = Avx.Add(sum, products.GetUpper());
        }
        if (t < weights.Length)
        {
            var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(offsets[t] * 8, 8));
            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsUInt16());
            var alpha = (int)(packed >> 48);
            var products = Sse41.MultiplyLow(samples, Vector128.Create(alpha, alpha, alpha, 1)).AsUInt32();
            var values = Avx512F.ConvertToVector512Double(Vector256.Create(products, Vector128<uint>.Zero)).GetLower();
            sum = Avx.Add(sum, Avx.Multiply(values, Vector256.Create(weights[t])));
        }
        return sum;
    }

    private static bool TryWrite(Vector256<double> sum, double normalizer, Span<byte> output)
    {
        // 2^-40 * Capacity^2 covers the complete admitted support in both orders.
        const double unitError = (double)Capacity * Capacity / (1L << 40);
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
        ReadOnlySpan<double> weights, ReadOnlySpan<ushort> offsets, int sourceFirst,
        double centerY, int firstY, int lastY, double scaleY, OperationContext context)
    {
        double red = 0, green = 0, blue = 0, alphaSum = 0, normalizer = 0;
        for (var coordinate = firstY; coordinate <= lastY; coordinate++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var wy = ImageTransforms.Kernel((coordinate - centerY) / scaleY, ResizeFilter.Bicubic);
            if (wy == 0) continue;
            var row = source.GetRow16(Math.Clamp(coordinate, 0, source.Height - 1));
            for (var tx = 0; tx < weights.Length; tx++)
            {
                var w = wy * weights[tx]; if (w == 0) continue;
                var offset = (sourceFirst + offsets[tx]) * 4;
                var a = row[offset + 3];
                red += w * a * row[offset]; green += w * a * row[offset + 1]; blue += w * a * row[offset + 2];
                alphaSum += w * a; normalizer += w;
            }
        }
        var normalized = alphaSum / normalizer;
        // Decline before exact tiny-alpha replay can require the entire scratch.
        if (normalized > 0 && normalized < 1) return false;
        var aOut = BicubicMath.RoundWord(normalized);
        var color = aOut > 0 && alphaSum > 0;
        var destination = result.GetRow16(y).Slice(x * 4, 4);
        destination[0] = BicubicMath.RoundWord(color ? red / alphaSum : 0);
        destination[1] = BicubicMath.RoundWord(color ? green / alphaSum : 0);
        destination[2] = BicubicMath.RoundWord(color ? blue / alphaSum : 0);
        destination[3] = aOut;
        return true;
    }
}
