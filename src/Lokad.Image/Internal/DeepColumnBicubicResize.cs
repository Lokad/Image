using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>A single output column reuses wider source support within 4 KiB.</summary>
internal static class DeepColumnBicubicResize
{
    private const int Capacity = 72;
    private const int WorkspaceBytes = 4096;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth != 8 || result.Width >= source.Width || result.Height >= source.Height
            || tapsX > Capacity || tapsY > Capacity || tapsX <= 36 && tapsY <= 36
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // These six fixed arrays occupy 4,032 bytes. No decoded-source table
            // is needed: each selected source row is filtered once per column.
            Span<Vector256<double>> cache = stackalloc Vector256<double>[Capacity];
            Span<int> rowIds = stackalloc int[Capacity];
            Span<double> weights = stackalloc double[Capacity];
            Span<ushort> offsets = stackalloc ushort[Capacity];
            Span<double> vertical = stackalloc double[Capacity];
            Span<ushort> rows = stackalloc ushort[Capacity];
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
                rowIds.Fill(-1);
                for (var y = 0; y < result.Height; y++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var centerY = (y + .5) * source.Height / result.Height - .5;
                    var firstY = (int)Math.Ceiling(centerY - 2 * scaleY);
                    var countY = (int)Math.Floor(centerY + 2 * scaleY) - firstY + 1;
                    double normalizerY = 0;
                    for (var t = 0; t < countY; t++)
                    {
                        var w = ImageTransforms.Kernel((firstY + t - centerY) / scaleY, ResizeFilter.Bicubic);
                        vertical[t] = w; normalizerY += w;
                        var sy = Math.Clamp(firstY + t, 0, source.Height - 1);
                        var slot = sy % Capacity; rows[t] = (ushort)slot;
                        if (rowIds[slot] == sy) continue;
                        rowIds[slot] = sy;
                        var row = input.Slice(sy * stride + sourceFirst * 4, sourceCount * 4);
                        var partial = Vector512<double>.Zero;
                        var tx = 0;
                        for (; tx + 1 < countX; tx += 2)
                        {
                            var p0 = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(offsets[tx] * 4, 4));
                            var p1 = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(offsets[tx + 1] * 4, 4));
                            var samples = Avx2.ConvertToVector256Int32(Vector128.CreateScalar((ulong)p0 | (ulong)p1 << 32).AsByte());
                            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                            var values = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors).AsUInt32());
                            var w0 = weights[tx]; var w1 = weights[tx + 1];
                            var products = Avx512F.Multiply(values, Avx512F.InsertVector256(Vector512.Create(w0), Vector256.Create(w1), 1));
                            // Two independent tap streams shorten the dependency chain.
                            // At most 37 dependent additions including the final fold
                            // remain within the original 72-term rounding certificate.
                            partial = Avx512F.Add(partial, products);
                        }
                        var sum = Avx.Add(partial.GetLower(), partial.GetUpper());
                        if (tx < countX)
                        {
                            var packed = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(offsets[tx] * 4, 4));
                            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsByte());
                            var alpha = (int)(packed >> 24);
                            var products = Sse41.MultiplyLow(samples, Vector128.Create(alpha, alpha, alpha, 1)).AsUInt32();
                            var values = Avx512F.ConvertToVector512Double(Vector256.Create(products, Vector128<uint>.Zero)).GetLower();
                            sum = Avx.Add(sum, Avx.Multiply(values, Vector256.Create(weights[tx])));
                        }
                        cache[slot] = sum;
                    }
                    var total = Vector256<double>.Zero;
                    for (var t = 0; t < countY; t++)
                        total = Avx.Add(total, Avx.Multiply(cache[rows[t]], Vector256.Create(vertical[t])));
                    var normalizer = normalizerX * normalizerY;
                    if (!TryWrite(total, normalizer, result.Pixels.Slice(y * result.Stride + x * 4, 4)))
                    {
                        if (!(normalizer >= .5 && total.GetElement(3) / normalizer >= 2)
                            || !OriginalByte(source, result, x, y, weights[..countX], offsets[..countX],
                                sourceFirst, vertical[..countY], firstY, context)) return false;
                    }
                }
            }
            // Speculation preserves the original tap order and pixel rounding.
            context.Cancellation.ThrowIfCancellationRequested();
            return true;
        }
        finally { context.Release(WorkspaceBytes); }
    }

    private static bool TryWrite(Vector256<double> sum, double normalizer, Span<byte> output)
    {
        // 2^-40 * 72^2 covers both accumulation orders and normalization.
        const double unitError = 72d * 72 / (1L << 40);
        const double maximum = 255d;
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
        var shorts = Sse2.PackSignedSaturate(ints, Vector128<int>.Zero);
        var pixel = Sse2.PackUnsignedSaturate(shorts, Vector128<short>.Zero).AsUInt32().GetElement(0);
        BinaryPrimitives.WriteUInt32LittleEndian(output, pixel);
        return true;
    }

    private static bool OriginalByte(ImageBuffer source, ImageBuffer result, int x, int y,
        ReadOnlySpan<double> weights, ReadOnlySpan<ushort> offsets, int sourceFirst,
        ReadOnlySpan<double> vertical, int firstY, OperationContext context)
    {
        double red = 0, green = 0, blue = 0, alphaSum = 0, normalizer = 0;
        for (var ty = 0; ty < vertical.Length; ty++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var wy = vertical[ty]; if (wy == 0) continue;
            var row = source.GetRow8(Math.Clamp(firstY + ty, 0, source.Height - 1));
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
