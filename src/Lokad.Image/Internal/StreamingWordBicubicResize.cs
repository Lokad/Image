using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Streaming source rows feed a bounded set of output accumulators.</summary>
internal static class StreamingWordBicubicResize
{
    private const int Capacity = 304;
    private const int OutputRowsPerBlock = 16;
    private const int WorkspaceBytes = 4096;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, long rowWork, OperationContext context)
    {
        if (source.BitDepth != 16 || result.Width >= source.Width || result.Height >= source.Height
            || tapsX > Capacity || tapsY > Capacity
            || tapsX <= 72 && tapsY <= 72
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // Fixed arrays occupy 3,936 bytes. Only the last horizontal result
            // is retained, including repeated clamped edge rows.
            Span<double> weights = stackalloc double[Capacity];
            Span<ushort> offsets = stackalloc ushort[Capacity];
            Span<Vector256<double>> totals = stackalloc Vector256<double>[OutputRowsPerBlock];
            Span<double> centers = stackalloc double[OutputRowsPerBlock];
            Span<int> first = stackalloc int[OutputRowsPerBlock];
            Span<int> last = stackalloc int[OutputRowsPerBlock];
            Span<double> normalizers = stackalloc double[OutputRowsPerBlock];
            var input = source.Pixels;
            var stride = source.Stride;
            for (var x = 0; x < result.Width; x++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var centerX = (x + .5) * source.Width / result.Width - .5;
                var firstX = (int)Math.Ceiling(centerX - 2 * scaleX);
                var countX = (int)Math.Floor(centerX + 2 * scaleX) - firstX + 1;
                var sourceFirst = Math.Clamp(firstX, 0, source.Width - 1);
                double normalizerX = 0;
                for (var t = 0; t < countX; t++)
                {
                    var w = ImageTransforms.Kernel((firstX + t - centerX) / scaleX, ResizeFilter.Bicubic);
                    weights[t] = w; normalizerX += w;
                    var relative = Math.Clamp(firstX + t, 0, source.Width - 1) - sourceFirst;
                    if ((uint)relative >= Capacity) return false;
                    offsets[t] = (ushort)relative;
                }
                var sourceCount = offsets[countX - 1] + 1;
                // Reuse the same bounded arrays across output-row blocks.
                // Each output keeps original global coordinates and full tap order.
                for (var blockStart = 0; blockStart < result.Height; blockStart += OutputRowsPerBlock)
                {
                    var height = Math.Min(OutputRowsPerBlock, result.Height - blockStart);
                    for (var y = 0; y < height; y++)
                    {
                        centers[y] = (blockStart + y + .5) * source.Height / result.Height - .5;
                        first[y] = (int)Math.Ceiling(centers[y] - 2 * scaleY);
                        last[y] = (int)Math.Floor(centers[y] + 2 * scaleY);
                    }
                    totals.Clear(); normalizers.Clear();
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
                            var row = input.Slice(sy * stride + sourceFirst * 8, sourceCount * 8);
                            horizontal = FilterRow(row, weights[..countX], offsets[..countX]);
                        }
                        for (var y = 0; y < height; y++)
                        {
                            if (coordinate < first[y] || coordinate > last[y]) continue;
                            var w = ImageTransforms.Kernel((coordinate - centers[y]) / scaleY, ResizeFilter.Bicubic);
                            normalizers[y] += w;
                            totals[y] = Avx.Add(totals[y], Avx.Multiply(horizontal, Vector256.Create(w)));
                        }
                    }
                    for (var y = 0; y < height; y++)
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        var normalizer = normalizerX * normalizers[y];
                        if (!TryWrite(totals[y], normalizer, result.Pixels.Slice((blockStart + y) * result.Stride + x * 8, 8)))
                        {
                            if (!(normalizer >= .5 && totals[y].GetElement(3) / normalizer >= 2)
                                || !OriginalWord(source, result, x, blockStart + y, weights[..countX], offsets[..countX],
                                    sourceFirst, centers[y], first[y], last[y], scaleY, context)) return false;
                        }
                    }
                }
            }
            context.Work(rowWork * result.Height);
            return true;
        }
        finally { context.Release(WorkspaceBytes); }
    }

    private static Vector256<double> FilterRow(ReadOnlySpan<byte> row,
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
        var aOut = RoundWord(normalized);
        var color = aOut > 0 && alphaSum > 0;
        var destination = result.GetRow16(y).Slice(x * 4, 4);
        destination[0] = RoundWord(color ? red / alphaSum : 0);
        destination[1] = RoundWord(color ? green / alphaSum : 0);
        destination[2] = RoundWord(color ? blue / alphaSum : 0);
        destination[3] = aOut;
        return true;
    }

    private static ushort RoundWord(double value)
        => (ushort)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 65535);
}
