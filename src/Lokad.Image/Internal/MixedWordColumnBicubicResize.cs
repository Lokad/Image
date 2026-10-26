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
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth != 16 || result.Width < source.Width || result.Height >= source.Height
            || tapsX > 5 || tapsY > 10
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        // Preserve the faster existing vertical row cache when it fits. For an
        // unchanged width it needs 85 doubles and 17 ints per column, 128 bytes
        // of array allowance, and the separate exact-alpha scratch reservation.
        if (result.Width == source.Width && source.Width >= 2 && Avx512DQ.IsSupported
            && 85L * source.Width <= Array.MaxLength
            && context.RemainingWorkingBytes >= 85L * source.Width * sizeof(double)
                + 17L * source.Width * sizeof(int) + 128 + ResizeAlpha.ScratchBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // These fixed arrays occupy 3,972 bytes; row offsets are bounded below 24.
            // Intermediate values remain
            // unrounded; the frame returns before exact tiny-alpha scratch.
            Span<Vector256<double>> cache = stackalloc Vector256<double>[6 * 8];
            Span<int> columnIds = stackalloc int[6];
            Span<Vector512<double>> decoded = stackalloc Vector512<double>[24];
            Span<double> vertical = stackalloc double[8 * 10];
            Span<byte> rowOffsets = stackalloc byte[8 * 10];
            Span<int> counts = stackalloc int[8];
            Span<double> normalizers = stackalloc double[8];
            Span<double> horizontal = stackalloc double[5];
            Span<int> columns = stackalloc int[5];
            var input = source.Pixels;
            var inputStride = source.Stride;
            var output = result.Pixels;
            var outputStride = result.Stride;
            // At eight rows, support spans at most 24 source rows in this range.
            // Larger vertical scales retain four-row bands and the same dispatch.
            var bandRows = scaleY <= 2.125 ? 8 : 4;
            for (var tileY = 0; tileY < result.Height; tileY += bandRows)
            {
                var length = Math.Min(bandRows, result.Height - tileY);
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
                        rowOffsets[y * 10 + t] = (byte)relative;
                    }
                    normalizers[y] = normalizer;
                }
                var sourceCount = rowOffsets[(length - 1) * 10 + counts[length - 1] - 1] + 1;
                columnIds.Fill(-1);
                for (var x = 0; x < result.Width; x++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var center = (x + .5) * source.Width / result.Width - .5;
                    // A preserved or enlarged width fixes scaleX at one, keeping the
                    // original binary64 coefficient values without a division.
                    var first = (int)Math.Ceiling(center - 2);
                    var count = (int)Math.Floor(center + 2) - first + 1;
                    double normalizerX = 0;
                    for (var t = 0; t < count; t++)
                    {
                        var w = ImageTransforms.Kernel(first + t - center, ResizeFilter.Bicubic);
                        horizontal[t] = w; normalizerX += w;
                        var sx = Math.Clamp(first + t, 0, source.Width - 1);
                        var slot = sx % 6; columns[t] = slot * 8;
                        if (columnIds[slot] == sx) continue;
                        columnIds[slot] = sx;
                        // Decode adjacent columns together. Each source row supplies
                        // eight word channels in one load; one wide accumulation
                        // filters both columns in the original vertical tap order.
                        // Six slots cover five active taps plus one prefetched column.
                        var paired = sx + 1 < source.Width;
                        var nextSlot = (slot + 1) % 6;
                        if (paired) columnIds[nextSlot] = sx + 1;
                        for (var sy = 0; sy < sourceCount; sy++)
                        {
                            var offset = (sourceFirst + sy) * inputStride + sx * 8;
                            var packed = paired
                                ? MemoryMarshal.Read<Vector128<ushort>>(input.Slice(offset, 16))
                                : Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(offset, 8))).AsUInt16();
                            var samples = Avx2.ConvertToVector256Int32(packed);
                            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                            decoded[sy] = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors).AsUInt32());
                        }
                        for (var y = 0; y < length; y++)
                        {
                            var weights = vertical.Slice(y * 10, counts[y]);
                            var offsets = rowOffsets.Slice(y * 10, counts[y]);
                            var sum = Vector512<double>.Zero;
                            for (var ty = 0; ty < weights.Length; ty++)
                                sum = Avx512F.Add(sum, Avx512F.Multiply(decoded[offsets[ty]], Vector512.Create(weights[ty])));
                            cache[slot * 8 + y] = sum.GetLower();
                            if (paired) cache[nextSlot * 8 + y] = sum.GetUpper();
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
                        if (BicubicMath.TryWriteWordPair(sum, n0, n1, packed))
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
                        if (!BicubicMath.TryWriteWord(sum, normalizer, output.Slice((tileY + y) * outputStride + x * 8, 8))
                            && !OriginalWord(source, result, x, tileY + y, center, first,
                                vertical.Slice(y * 10, counts[y]), rowOffsets.Slice(y * 10, counts[y]), sourceFirst, context)) return false;
                    }
                }
            }
            context.Cancellation.ThrowIfCancellationRequested();
            return true;
        }
        finally { context.Release(WorkspaceBytes); }
    }

    private static bool OriginalWord(ImageBuffer source, ImageBuffer result, int x, int y,
        double centerX, int firstX, ReadOnlySpan<double> vertical, ReadOnlySpan<byte> rows,
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
        var alphaOut = BicubicMath.RoundWord(normalized);
        var color = alphaOut > 0 && alphaSum > 0;
        var destination = result.GetRow16(y).Slice(x * 4, 4);
        destination[0] = BicubicMath.RoundWord(color ? red / alphaSum : 0);
        destination[1] = BicubicMath.RoundWord(color ? green / alphaSum : 0);
        destination[2] = BicubicMath.RoundWord(color ? blue / alphaSum : 0);
        destination[3] = alphaOut;
        return true;
    }
}
