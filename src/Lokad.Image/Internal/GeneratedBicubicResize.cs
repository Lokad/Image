using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Streaming source rows feed a bounded set of output accumulators.</summary>
internal static class GeneratedBicubicResize
{
    private const int OutputRowsPerBlock = 16;
    private const int SourceRowsPerBlock = 16;
    private const int WorkspaceBytes = 4096;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth != 8 || result.Width >= source.Width || result.Height >= source.Height
            || (double)tapsX * tapsY > (1L << 40)
            || tapsX <= 544 && tapsY <= 544
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // Fixed arrays occupy 1,824 bytes inside the existing 4 KiB charge.
            // A bounded group of rows shares coefficient/index preparation;
            // no full-height intermediate or coefficient table is retained.
            Span<Vector256<double>> totals = stackalloc Vector256<double>[OutputRowsPerBlock];
            Span<double> centers = stackalloc double[OutputRowsPerBlock];
            Span<int> first = stackalloc int[OutputRowsPerBlock];
            Span<int> last = stackalloc int[OutputRowsPerBlock];
            Span<double> normalizers = stackalloc double[OutputRowsPerBlock];
            Span<double> verticalMagnitudes = stackalloc double[OutputRowsPerBlock];
            Span<Vector256<double>> horizontal = stackalloc Vector256<double>[SourceRowsPerBlock];
            Span<Vector512<double>> pairWeights = stackalloc Vector512<double>[4];
            Span<int> offsets = stackalloc int[8];
            var input = source.Pixels;
            var stride = source.Stride;
            for (var x = 0; x < result.Width; x++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var centerX = (x + .5) * source.Width / result.Width - .5;
                var firstX = (int)Math.Ceiling(centerX - 2 * scaleX);
                var countX = (int)Math.Floor(centerX + 2 * scaleX) - firstX + 1;
                double normalizerX = 0, horizontalMagnitude = 0;
                // Verify every generated coefficient against the original
                // binary64 evaluation before using this column. No table is kept.
                for (var t = 0; t < countX; t += 8)
                {
                    if ((t & 255) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    var generated = BicubicMath.Coefficients(firstX + t, centerX, scaleX);
                    var count = Math.Min(8, countX - t);
                    for (var lane = 0; lane < count; lane++)
                    {
                        var original = ImageTransforms.Kernel((firstX + t + lane - centerX) / scaleX, ResizeFilter.Bicubic);
                        if (BitConverter.DoubleToUInt64Bits(generated.GetElement(lane)) != BitConverter.DoubleToUInt64Bits(original)) return false;
                        normalizerX += original; horizontalMagnitude += Math.Abs(original);
                    }
                }
                // Each bounded block uses the same complete original tap order.
                // No full-height accumulator array or intermediate pixels are kept.
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
                    // Unclamped coordinates are visited in increasing order. Each
                    // output receives exactly its original vertical taps, including
                    // separate contributions from duplicated clamped edge samples.
                    var coordinate = first[0];
                    var lastSourceRow = Math.Clamp(last[height - 1], 0, source.Height - 1);
                    while (coordinate <= last[height - 1])
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        var firstSourceRow = Math.Clamp(coordinate, 0, source.Height - 1);
                        var rowCount = Math.Min(SourceRowsPerBlock, lastSourceRow - firstSourceRow + 1);
                        var rows = horizontal[..rowCount];
                        rows.Clear();
                        FilterRows(input, stride, firstSourceRow, rows, offsets, pairWeights,
                            firstX, countX, centerX, scaleX, source.Width, context);
                        var lastRow = firstSourceRow + rowCount - 1;
                        while (coordinate <= last[height - 1]
                            && Math.Clamp(coordinate, 0, source.Height - 1) <= lastRow)
                        {
                            context.Cancellation.ThrowIfCancellationRequested();
                            var sum = rows[Math.Clamp(coordinate, 0, source.Height - 1) - firstSourceRow];
                            for (var y = 0; y < height; y++)
                            {
                                if (coordinate < first[y] || coordinate > last[y]) continue;
                                var w = ImageTransforms.Kernel((coordinate - centers[y]) / scaleY, ResizeFilter.Bicubic);
                                normalizers[y] += w; verticalMagnitudes[y] += Math.Abs(w);
                                totals[y] = Avx.Add(totals[y], Avx.Multiply(sum, Vector256.Create(w)));
                            }
                            coordinate++;
                        }
                    }
                    for (var y = 0; y < height; y++)
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        var normalizer = normalizerX * normalizers[y];
                        // At least eight epsilons per admitted original tap,
                        // scaled by L1 magnitudes, covers both evaluation orders.
                        var unitError = (double)tapsX * tapsY / (1L << 40)
                            * Math.Max(1, horizontalMagnitude * verticalMagnitudes[y] / 1024);
                        if (!TryWrite(totals[y], normalizer, unitError, result.Pixels.Slice((blockStart + y) * result.Stride + x * 4, 4)))
                        {
                            if (!(normalizer >= .5)
                                || !OriginalPixel(source, result, x, blockStart + y, centerX, firstX, countX, scaleX,
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

    private static void FilterRows(ReadOnlySpan<byte> input, int stride, int firstRow,
        Span<Vector256<double>> sums, Span<int> offsets, Span<Vector512<double>> pairWeights,
        int firstX, int countX,
        double centerX, double scaleX, int sourceWidth, OperationContext context)
    {
        for (var t = 0; t < countX; t += 8)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var weights = BicubicMath.Coefficients(firstX + t, centerX, scaleX);
            var count = Math.Min(8, countX - t);
            for (var i = 0; i < count; i++)
                offsets[i] = Math.Clamp(firstX + t + i, 0, sourceWidth - 1) * 4;
            var lane = 0;
            for (; lane + 1 < count; lane += 2)
            {
                var w0 = weights.GetElement(lane); var w1 = weights.GetElement(lane + 1);
                pairWeights[lane / 2] = Vector512.Create(w0, w0, w0, w0, w1, w1, w1, w1);
            }
            for (var r = 0; r < sums.Length; r++)
            {
                var row = input.Slice((firstRow + r) * stride, stride);
                var sum = sums[r];
                for (var pair = 0; pair < lane; pair += 2)
                {
                    var p0 = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(offsets[pair], 4));
                    var p1 = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(offsets[pair + 1], 4));
                    var samples = Avx2.ConvertToVector256Int32(Vector128.CreateScalar((ulong)p0 | ((ulong)p1 << 32)).AsByte());
                    var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                    var values = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors));
                    var products = Avx512F.Multiply(values, pairWeights[pair / 2]);
                    // Preserve the original per-row lower-then-upper tap order.
                    sum = Avx.Add(sum, products.GetLower()); sum = Avx.Add(sum, products.GetUpper());
                }
                if (lane < count)
                {
                    var packed = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(offsets[lane], 4));
                    var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsByte());
                    var alpha = (int)(packed >> 24);
                    var products = Sse41.MultiplyLow(samples, Vector128.Create(alpha, alpha, alpha, 1));
                    var values = Avx.ConvertToVector256Double(products);
                    sum = Avx.Add(sum, Avx.Multiply(values, Vector256.Create(weights.GetElement(lane))));
                }
                sums[r] = sum;
            }
        }
    }

    private static bool TryWrite(Vector256<double> sum, double normalizer, double unitError, Span<byte> output)
    {
        const double maximum = 255d;
        var alphaError = unitError * maximum;
        var alpha = sum.GetElement(3);
        if (!(normalizer >= .5 && normalizer > unitError && alpha > alphaError)) return false;
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
        var words = Sse2.PackSignedSaturate(ints, Vector128<int>.Zero);
        var packed = Sse2.PackUnsignedSaturate(words, Vector128<short>.Zero).AsUInt32().GetElement(0);
        BinaryPrimitives.WriteUInt32LittleEndian(output, packed);
        return true;
    }

    private static bool OriginalPixel(ImageBuffer source, ImageBuffer result, int x, int y,
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
