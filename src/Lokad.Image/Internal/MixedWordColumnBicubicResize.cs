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
    private readonly struct EightRowBand { }
    private readonly struct SixteenRowBand { }
    private readonly struct VerticalEightRowBand { }
    private readonly struct VerticalSixteenRowBand { }

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
        // Select by the local ceiling before shared admission. A refused root
        // reservation is terminal; never retry with a smaller band afterward.
        var largerBand = result.Height >= 16 && scaleY <= 2.125 && context.RemainingWorkingBytes >= 8192;
        // Up to this width, (x+.5)*width is an exact half-integer below 2^52.
        // The original center is exactly x; its five weights are 0,0,1,0,0.
        // Retain the original geometry for larger caller-permitted dimensions.
        if (source.Width == result.Width && source.Width <= 1 << 26)
            return largerBand
                ? Apply<VerticalSixteenRowBand>(source, result, scaleY, context)
                : Apply<VerticalEightRowBand>(source, result, scaleY, context);
        return largerBand
            ? Apply<SixteenRowBand>(source, result, scaleY, context)
            : Apply<EightRowBand>(source, result, scaleY, context);
    }

    private static bool Apply<TBand>(ImageBuffer source, ImageBuffer result,
        double scaleY, OperationContext context) where TBand : struct
    {
        // Value-type specialization makes these capacities and cache strides
        // constants in both kernels, while keeping one execution source body.
        var identity = typeof(TBand) == typeof(VerticalEightRowBand) || typeof(TBand) == typeof(VerticalSixteenRowBand);
        var rowCapacity = typeof(TBand) == typeof(SixteenRowBand) || typeof(TBand) == typeof(VerticalSixteenRowBand) ? 16 : 8;
        var sourceCapacity = rowCapacity == 16 ? 42 : 24;
        var workspaceBytes = rowCapacity == 16 ? 8192 : WorkspaceBytes;
        context.Reserve(workspaceBytes);
        try
        {
            // Explicit arrays occupy 3,992 or 7,596 bytes. Six columns cover
            // five active taps and the paired-column prefetch in either band.
            // Intermediate values remain
            // unrounded; the frame returns before exact tiny-alpha scratch.
            Span<Vector256<double>> cache = stackalloc Vector256<double>[6 * rowCapacity];
            Span<int> columnIds = stackalloc int[6];
            Span<Vector512<double>> decoded = stackalloc Vector512<double>[sourceCapacity];
            Span<double> vertical = stackalloc double[rowCapacity * 10];
            Span<byte> rowOffsets = stackalloc byte[rowCapacity * 10];
            Span<int> counts = stackalloc int[rowCapacity];
            Span<double> normalizers = stackalloc double[rowCapacity];
            Span<double> horizontal = stackalloc double[5];
            Span<int> columns = stackalloc int[5];
            Span<int> samplesX = stackalloc int[5];
            var input = source.Pixels;
            var inputStride = source.Stride;
            var output = result.Pixels;
            var outputStride = result.Stride;
            // Support spans at most floor((rows+3)*scaleY)+1 source rows:
            // <=41 for16rows and <=24 for8rows at scaleY<=2.125.
            // Larger scales retain the existing four-row band.
            var bandRows = scaleY <= 2.125 ? rowCapacity : 4;
            var geometry = identity ? default : MixedBicubicGeometry.Prepare(source, result, bandRows, context.Cancellation);
            for (var tileY = 0; tileY < result.Height; tileY += bandRows)
            {
                var length = Math.Min(bandRows, result.Height - tileY);
                var planned = tileY + length <= geometry.SafeRows;
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
                        if ((uint)relative >= (uint)sourceCapacity) return false;
                        rowOffsets[y * 10 + t] = (byte)relative;
                    }
                    normalizers[y] = normalizer;
                }
                var sourceCount = rowOffsets[(length - 1) * 10 + counts[length - 1] - 1] + 1;
                columnIds.Fill(-1);
                for (var x = 0; x < result.Width; x++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    double center, normalizerX;
                    int first, count;
                    if (identity)
                    {
                        center = x; first = x - 2; count = 1; normalizerX = 1;
                        horizontal[0] = 1; samplesX[0] = x;
                    }
                    else if (planned)
                        geometry.Read(x, horizontal, samplesX, out center, out first, out count, out normalizerX);
                    else
                    {
                        center = (x + .5) * source.Width / result.Width - .5;
                        first = (int)Math.Ceiling(center - 2);
                        count = (int)Math.Floor(center + 2) - first + 1;
                        normalizerX = 0;
                        for (var t = 0; t < count; t++)
                        {
                            var w = ImageTransforms.Kernel(first + t - center, ResizeFilter.Bicubic);
                            horizontal[t] = w; normalizerX += w;
                            samplesX[t] = Math.Clamp(first + t, 0, source.Width - 1);
                        }
                    }
                    for (var t = 0; t < count; t++)
                    {
                        var sx = samplesX[t];
                        var slot = sx % 6; columns[t] = slot * rowCapacity;
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
                            cache[slot * rowCapacity + y] = sum.GetLower();
                            if (paired) cache[nextSlot * rowCapacity + y] = sum.GetUpper();
                        }
                    }
                    var outputY = 0;
                    for (; outputY + 3 < length; outputY += 4)
                    {
                        // Share the horizontal taps across two independent row
                        // pairs, then prepare both quotients before certification.
                        var firstSum = Vector512<double>.Zero;
                        var secondSum = Vector512<double>.Zero;
                        for (var t = 0; t < count; t++)
                        {
                            var at = columns[t] + outputY;
                            var firstPair = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cache.Slice(at, 2)));
                            var secondPair = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cache.Slice(at + 2, 2)));
                            var weight = Vector512.Create(horizontal[t]);
                            firstSum = Avx512F.Add(firstSum, Avx512F.Multiply(firstPair, weight));
                            secondSum = Avx512F.Add(secondSum, Avx512F.Multiply(secondPair, weight));
                        }
                        // Decoded source values have retired into the column cache.
                        // Reuse their storage for four independently checked rows.
                        var packed = MemoryMarshal.AsBytes(decoded)[..32];
                        var accepted = BicubicMath.WriteWordQuartet(firstSum, secondSum,
                            normalizerX * normalizers[outputY], normalizerX * normalizers[outputY + 1],
                            normalizerX * normalizers[outputY + 2], normalizerX * normalizers[outputY + 3], packed);
                        for (var pair = 0; pair < 2; pair++)
                        {
                            var y = outputY + 2 * pair;
                            if ((accepted & (1 << pair)) != 0)
                            {
                                BinaryPrimitives.WriteUInt64LittleEndian(output.Slice((tileY + y) * outputStride + x * 8, 8),
                                    BinaryPrimitives.ReadUInt64LittleEndian(packed.Slice(pair * 16, 8)));
                                BinaryPrimitives.WriteUInt64LittleEndian(output.Slice((tileY + y + 1) * outputStride + x * 8, 8),
                                    BinaryPrimitives.ReadUInt64LittleEndian(packed.Slice(pair * 16 + 8, 8)));
                            }
                            else if (!OriginalWord(source, result, x, tileY + y, center, first,
                                vertical.Slice(y * 10, counts[y]), rowOffsets.Slice(y * 10, counts[y]), sourceFirst, context)
                                || !OriginalWord(source, result, x, tileY + y + 1, center, first,
                                    vertical.Slice((y + 1) * 10, counts[y + 1]), rowOffsets.Slice((y + 1) * 10, counts[y + 1]), sourceFirst, context)) return false;
                        }
                    }
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
        finally { context.Release(workspaceBytes); }
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
