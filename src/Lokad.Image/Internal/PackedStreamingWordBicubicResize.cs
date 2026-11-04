using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Word filtering and exact publication for the shared streaming driver.</summary>
internal static class PackedStreamingWordBicubicResize
{
    private const int Capacity = BicubicMath.PackedStreamingTapCapacity;

    internal static bool WriteBlock(ImageBuffer source, ImageBuffer result, int x, int blockStart,
        double centerX, int firstX, int countX, double scaleX, double scaleY,
        double normalizerX, double horizontalMagnitude, double coefficientError,
        ReadOnlySpan<Vector256<double>> totals, ReadOnlySpan<double> normalizers,
        ReadOnlySpan<double> verticalMagnitudes, ReadOnlySpan<double> centers,
        ReadOnlySpan<int> first, ReadOnlySpan<int> last, OperationContext context)
    {
        var height = totals.Length;
        for (var y = 0; y < height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var normalizer = normalizerX * normalizers[y];
            // Eight-times-epsilon scaled by full support and actual L1
            // magnitudes covers both original and separable sums. Twice
            // the measured coefficient loss covers positive bound rounding.
            var unitError = (double)Capacity * Capacity / (1L << 40)
                * Math.Max(1, horizontalMagnitude * verticalMagnitudes[y] / 1024)
                + 2 * coefficientError * verticalMagnitudes[y];
            if (!TryWrite(totals[y], normalizer, unitError, result.Pixels.Slice((blockStart + y) * result.Stride + x * 8, 8)))
            {
                if (!(normalizer >= .5)
                    || !OriginalWord(source, result, x, blockStart + y, centerX, firstX, countX, scaleX,
                        centers[y], first[y], last[y], scaleY, context)) return false;
            }
        }
        return true;
    }

    internal static Vector256<double> FilterRow(ReadOnlySpan<byte> row,
        ReadOnlySpan<uint> low, ReadOnlySpan<ushort> high, int firstX, int sourceFirst, int sourceWidth)
    {
        var sum = Vector256<double>.Zero;
        var t = 0;
        for (; t + 1 < low.Length; t += 2)
        {
            var p0 = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice((Math.Clamp(firstX + t, 0, sourceWidth - 1) - sourceFirst) * 8, 8));
            var p1 = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice((Math.Clamp(firstX + t + 1, 0, sourceWidth - 1) - sourceFirst) * 8, 8));
            var samples = Avx2.ConvertToVector256Int32(Vector128.Create(p0, p1).AsUInt16());
            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
            var values = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors).AsUInt32());
            var w0 = BicubicMath.Coefficient(low[t], high[t]); var w1 = BicubicMath.Coefficient(low[t + 1], high[t + 1]);
            var products = Avx512F.Multiply(values, Vector512.Create(w0, w0, w0, w0, w1, w1, w1, w1));
            sum = Avx.Add(sum, products.GetLower());
            sum = Avx.Add(sum, products.GetUpper());
        }
        if (t < low.Length)
        {
            var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice((Math.Clamp(firstX + t, 0, sourceWidth - 1) - sourceFirst) * 8, 8));
            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsUInt16());
            var alpha = (int)(packed >> 48);
            var products = Sse41.MultiplyLow(samples, Vector128.Create(alpha, alpha, alpha, 1)).AsUInt32();
            var values = Avx512F.ConvertToVector512Double(Vector256.Create(products, Vector128<uint>.Zero)).GetLower();
            sum = Avx.Add(sum, Avx.Multiply(values, Vector256.Create(BicubicMath.Coefficient(low[t], high[t]))));
        }
        return sum;
    }

    private static bool TryWrite(Vector256<double> sum, double normalizer, double unitError, Span<byte> output)
    {
        const double maximum = 65535d;
        var alphaError = unitError * maximum;
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
        double centerX, int firstX, int countX, double scaleX,
        double centerY, int firstY, int lastY, double scaleY, OperationContext context)
    {
        double red = 0, green = 0, blue = 0, alphaSum = 0, normalizer = 0;
        for (var coordinate = firstY; coordinate <= lastY; coordinate++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var wy = ImageTransforms.Kernel((coordinate - centerY) / scaleY, ResizeFilter.Bicubic);
            if (wy == 0) continue;
            var row = source.GetRow16(Math.Clamp(coordinate, 0, source.Height - 1));
            for (var tx = 0; tx < countX; tx++)
            {
                var w = wy * ImageTransforms.Kernel((firstX + tx - centerX) / scaleX, ResizeFilter.Bicubic); if (w == 0) continue;
                var offset = Math.Clamp(firstX + tx, 0, source.Width - 1) * 4;
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
