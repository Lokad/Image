using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Streaming source rows feed a bounded set of output accumulators.</summary>
internal static class PackedStreamingBicubicResize
{
    private const int Capacity = 544;
    private const int OutputRowsPerBlock = 8;
    private const int WorkspaceBytes = 4096;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth != 8 || result.Width >= source.Width || result.Height >= source.Height
            || tapsX > Capacity || tapsY > Capacity
            || tapsX <= 304 && tapsY <= 304
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // Fixed arrays occupy 3,776 bytes. Only the last horizontal result
            // is retained, including repeated clamped edge rows.
            Span<uint> coefficientLow = stackalloc uint[Capacity];
            Span<ushort> coefficientHigh = stackalloc ushort[Capacity];
            Span<Vector256<double>> totals = stackalloc Vector256<double>[OutputRowsPerBlock];
            Span<double> centers = stackalloc double[OutputRowsPerBlock];
            Span<int> first = stackalloc int[OutputRowsPerBlock];
            Span<int> last = stackalloc int[OutputRowsPerBlock];
            Span<double> normalizers = stackalloc double[OutputRowsPerBlock];
            Span<double> verticalMagnitudes = stackalloc double[OutputRowsPerBlock];
            var input = source.Pixels;
            var stride = source.Stride;
            for (var x = 0; x < result.Width; x++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var centerX = (x + .5) * source.Width / result.Width - .5;
                var firstX = (int)Math.Ceiling(centerX - 2 * scaleX);
                var countX = (int)Math.Floor(centerX + 2 * scaleX) - firstX + 1;
                var sourceFirst = Math.Clamp(firstX, 0, source.Width - 1);
                double normalizerX = 0, horizontalMagnitude = 0, coefficientError = 0;
                for (var t = 0; t < countX; t++)
                {
                    var w = ImageTransforms.Kernel((firstX + t - centerX) / scaleX, ResizeFilter.Bicubic);
                    normalizerX += w; horizontalMagnitude += Math.Abs(w);
                    // Preserve sign/exponent and 36 fraction bits in six bytes.
                    // The omitted error is measured from the original coefficient.
                    var bits = BitConverter.DoubleToUInt64Bits(w);
                    coefficientLow[t] = (uint)(bits >> 16);
                    coefficientHigh[t] = (ushort)(bits >> 48);
                    coefficientError += Math.Abs(w - BicubicMath.Coefficient(coefficientLow[t], coefficientHigh[t]));
                }
                var sourceCount = Math.Clamp(firstX + countX - 1, 0, source.Width - 1) - sourceFirst + 1;
                // Reuse the same bounded arrays for each output-row block.
                // Original global coordinates and full tap order are preserved.
                for (var blockStart = 0; blockStart < result.Height; blockStart += OutputRowsPerBlock)
                {
                    var height = Math.Min(OutputRowsPerBlock, result.Height - blockStart);
                    for (var y = 0; y < height; y++)
                    {
                        centers[y] = (blockStart + y + .5) * source.Height / result.Height - .5;
                        first[y] = (int)Math.Ceiling(centers[y] - 2 * scaleY);
                        last[y] = (int)Math.Floor(centers[y] + 2 * scaleY);
                    }
                    totals.Clear(); normalizers.Clear(); verticalMagnitudes.Clear();
                    var previousRow = -1;
                    var horizontal = Vector256<double>.Zero;
                    // Unclamped coordinates are visited in increasing order. Each
                    // output receives exactly its original vertical taps, including
                    // separate contributions from duplicated clamped edge samples.
                    for (var coordinate = first[0]; coordinate <= last[height - 1]; coordinate++)
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        var sy = Math.Clamp(coordinate, 0, source.Height - 1);
                        if (sy != previousRow)
                        {
                            previousRow = sy;
                            var row = input.Slice(sy * stride + sourceFirst * 4, sourceCount * 4);
                            horizontal = FilterRow(row, coefficientLow[..countX], coefficientHigh[..countX], firstX, sourceFirst, source.Width);
                        }
                        for (var y = 0; y < height; y++)
                        {
                            if (coordinate < first[y] || coordinate > last[y]) continue;
                            var w = ImageTransforms.Kernel((coordinate - centers[y]) / scaleY, ResizeFilter.Bicubic);
                            normalizers[y] += w; verticalMagnitudes[y] += Math.Abs(w);
                            totals[y] = Avx.Add(totals[y], Avx.Multiply(horizontal, Vector256.Create(w)));
                        }
                    }
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
                        if (!TryWrite(totals[y], normalizer, unitError, result.Pixels.Slice((blockStart + y) * result.Stride + x * 4, 4)))
                        {
                            if (!(normalizer >= .5)
                                || !OriginalByte(source, result, x, blockStart + y, centerX, firstX, countX, scaleX,
                                    centers[y], first[y], last[y], scaleY, context)) return false;
                        }
                    }
                }
            }
            context.Cancellation.ThrowIfCancellationRequested();
            return true;
        }
        finally { context.Release(WorkspaceBytes); }
    }

    private static Vector256<double> FilterRow(ReadOnlySpan<byte> row,
        ReadOnlySpan<uint> low, ReadOnlySpan<ushort> high, int firstX, int sourceFirst, int sourceWidth)
    {
        var sum = Vector256<double>.Zero;
        var t = 0;
        for (; t + 1 < low.Length; t += 2)
        {
            var p0 = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice((Math.Clamp(firstX + t, 0, sourceWidth - 1) - sourceFirst) * 4, 4));
            var p1 = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice((Math.Clamp(firstX + t + 1, 0, sourceWidth - 1) - sourceFirst) * 4, 4));
            var samples = Avx2.ConvertToVector256Int32(Vector128.CreateScalar(p0 | ((ulong)p1 << 32)).AsByte());
            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
            var values = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors));
            var w0 = BicubicMath.Coefficient(low[t], high[t]); var w1 = BicubicMath.Coefficient(low[t + 1], high[t + 1]);
            var products = Avx512F.Multiply(values, Vector512.Create(w0, w0, w0, w0, w1, w1, w1, w1));
            sum = Avx.Add(sum, products.GetLower());
            sum = Avx.Add(sum, products.GetUpper());
        }
        if (t < low.Length)
        {
            var packed = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice((Math.Clamp(firstX + t, 0, sourceWidth - 1) - sourceFirst) * 4, 4));
            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsByte());
            var alpha = (int)(packed >> 24);
            var products = Sse41.MultiplyLow(samples, Vector128.Create(alpha, alpha, alpha, 1));
            var values = Avx.ConvertToVector256Double(products);
            sum = Avx.Add(sum, Avx.Multiply(values, Vector256.Create(BicubicMath.Coefficient(low[t], high[t]))));
        }
        return sum;
    }

    private static bool TryWrite(Vector256<double> sum, double normalizer, double unitError, Span<byte> output)
    {
        const double maximum = 255d;
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
        var shorts = Sse2.PackSignedSaturate(ints, Vector128<int>.Zero);
        var packed = Sse2.PackUnsignedSaturate(shorts, Vector128<short>.Zero).AsUInt32().GetElement(0);
        BinaryPrimitives.WriteUInt32LittleEndian(output, packed);
        return true;
    }

    private static bool OriginalByte(ImageBuffer source, ImageBuffer result, int x, int y,
        double centerX, int firstX, int countX, double scaleX,
        double centerY, int firstY, int lastY, double scaleY, OperationContext context)
    {
        double red = 0, green = 0, blue = 0, alphaSum = 0, normalizer = 0;
        for (var coordinate = firstY; coordinate <= lastY; coordinate++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var wy = ImageTransforms.Kernel((coordinate - centerY) / scaleY, ResizeFilter.Bicubic);
            if (wy == 0) continue;
            var row = source.GetRow8(Math.Clamp(coordinate, 0, source.Height - 1));
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
        var aOut = BicubicMath.RoundByte(normalized);
        var color = aOut > 0 && alphaSum > 0;
        var destination = result.GetRow8(y).Slice(x * 4, 4);
        destination[0] = BicubicMath.RoundByte(color ? red / alphaSum : 0);
        destination[1] = BicubicMath.RoundByte(color ? green / alphaSum : 0);
        destination[2] = BicubicMath.RoundByte(color ? blue / alphaSum : 0);
        destination[3] = aOut;
        return true;
    }
}
