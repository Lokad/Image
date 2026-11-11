using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Vertical filtering reuses source columns for a small output-row band.</summary>
internal static class MixedColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;
    private readonly struct EightRowBand { }
    private readonly struct SixtyFourRowBand { }
    private readonly struct VerticalEightRowBand { }

    // A completed resize clears RGB under zero alpha; this temporary marker
    // therefore cannot collide with a completed pixel in the private output.
    private const uint DeferredPixel = 0x00FFFFFF;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (!TryApplyCore(source, result, scaleX, scaleY, tapsX, tapsY, context, out var deferred)) return false;
        if (deferred) ReplayTiny(source, result, scaleX, scaleY, context);
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryApplyCore(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context, out bool deferred)
    {
        deferred = false;
        if (source.BitDepth != 8 || result.Width < source.Width || result.Height >= source.Height
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
        // Width identity has exactly one unit tap under the existing word
        // path's half-integer bound. Keep the faster cached dispatch above.
        return source.Width == result.Width && source.Width <= 1 << 26
            ? Apply<VerticalEightRowBand>(source, result, scaleY, context, out deferred)
            : Apply<EightRowBand>(source, result, scaleY, context, out deferred);
    }

    private static bool Apply<TBand>(ImageBuffer source, ImageBuffer result,
        double scaleY, OperationContext context, out bool deferred) where TBand : struct
    {
        deferred = false;
        var identity = typeof(TBand) == typeof(VerticalEightRowBand);
        context.Reserve(WorkspaceBytes);
        try
        {
            var bandRows = scaleY <= 2.125 ? 8 : 4;
            var geometry = identity ? default : MixedBicubicGeometry.Prepare(source, result, bandRows, context.Cancellation);
            var firstRow = 0;
            if (!identity && scaleY <= 2.125 && geometry.SafeRows != 0
                && TryLocateResultWorkspace(result, out var start, out var safeRows))
            {
                if (!ApplyRange<SixtyFourRowBand>(source, result, scaleY, context,
                    result.Pixels.Slice(start, ResultWorkspaceBytes), geometry, 0, safeRows, ref deferred)) return false;
                firstRow = safeRows;
            }
            // Large views have retired before suffix writes. The enclosing
            // NoInlining core still unwinds before exact tiny-alpha scratch.
            Span<byte> workspace = stackalloc byte[WorkspaceBytes];
            return ApplyRange<TBand>(source, result, scaleY, context,
                workspace, geometry, firstRow, result.Height, ref deferred);
        }
        finally { context.Release(WorkspaceBytes); }
    }

    internal const int ResultWorkspaceBytes = 28136;

    internal static bool TryLocateResultWorkspace(ImageBuffer result, out int start, out int safeRows)
    {
        var geometryStart = result.ByteLength - MixedBicubicGeometry.StorageBytes(result.Width);
        var end = geometryStart & ~63L;
        var candidate = (end - ResultWorkspaceBytes) & ~63L;
        start = candidate >= 0 ? (int)candidate : 0;
        safeRows = start / result.Stride;
        return safeRows >= 64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Span<T> Take<T>(ref Span<byte> workspace, int count) where T : unmanaged
    {
        var bytes = count * Unsafe.SizeOf<T>();
        var values = MemoryMarshal.Cast<byte, T>(workspace[..bytes]);
        workspace = workspace[bytes..];
        return values;
    }

    private static bool ApplyRange<TBand>(ImageBuffer source, ImageBuffer result,
        double scaleY, OperationContext context, Span<byte> workspace, MixedBicubicGeometry geometry,
        int firstRow, int endRow, ref bool deferred) where TBand : struct
    {
        var identity = typeof(TBand) == typeof(VerticalEightRowBand);
        var rowCapacity = typeof(TBand) == typeof(SixtyFourRowBand) ? 64 : 8;
        var sourceCapacity = rowCapacity == 64 ? 144 : 24;
        var decoded = Take<Vector512<double>>(ref workspace, sourceCapacity);
        var cache = Take<Vector256<double>>(ref workspace, 6 * rowCapacity);
        var columnIds = Take<int>(ref workspace, 6);
        var vertical = Take<double>(ref workspace, rowCapacity * 10);
        var rowOffsets = Take<byte>(ref workspace, rowCapacity * 10);
        var counts = Take<int>(ref workspace, rowCapacity);
        var normalizers = Take<double>(ref workspace, rowCapacity);
        var horizontal = Take<double>(ref workspace, 5);
        var columns = Take<int>(ref workspace, 5);
        var samplesX = Take<int>(ref workspace, 5);
        var input = source.Pixels; var inputStride = source.Stride;
        var output = result.Pixels; var outputStride = result.Stride;
        var bandRows = scaleY <= 2.125 ? rowCapacity : 4;
        for (var tileY = firstRow; tileY < endRow; tileY += bandRows)
        {
            var length = Math.Min(bandRows, endRow - tileY);
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
            BicubicMath.PrepareOverlappingPairs(rowOffsets, counts, length, sourceCount, 10, 8, 2, context.Cancellation);
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
                    // Pair adjacent columns, retaining each vertical tap's order.
                    // Six slots hold five active taps and one prefetched column.
                    var paired = sx + 1 < source.Width;
                    var nextSlot = (slot + 1) % 6;
                    if (paired) columnIds[nextSlot] = sx + 1;
                    for (var sy = 0; sy < sourceCount; sy++)
                    {
                        if (typeof(TBand) == typeof(SixtyFourRowBand) && (sy & 31) == 0)
                            context.Cancellation.ThrowIfCancellationRequested();
                        var offset = (sourceFirst + sy) * inputStride + sx * 4;
                        var packed = paired
                            ? BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(offset, 8))
                            : BinaryPrimitives.ReadUInt32LittleEndian(input.Slice(offset, 4));
                        var samples = Avx2.ConvertToVector256Int32(Vector128.CreateScalar(packed).AsByte());
                        var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                        decoded[sy] = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors));
                    }
                    for (var y = 0; y < length; y++)
                    {
                        if (typeof(TBand) == typeof(SixtyFourRowBand) && (y & 7) == 0)
                            context.Cancellation.ThrowIfCancellationRequested();
                        if (counts[y] == 8 && y + 1 < length && counts[y + 1] == 8 && rowOffsets[y * 10 + 9] != 0)
                        {
                            var windowFirst = rowOffsets[y * 10 + 9] - 1;
                            BicubicMath.FilterOverlappingVerticalPair(decoded.Slice(windowFirst, 10),
                                vertical.Slice(y * 10, 8), vertical.Slice((y + 1) * 10, 8), out var sum0, out var sum1);
                            cache[slot * rowCapacity + y] = sum0.GetLower();
                            cache[slot * rowCapacity + y + 1] = sum1.GetLower();
                            if (paired)
                            {
                                cache[nextSlot * rowCapacity + y] = sum0.GetUpper();
                                cache[nextSlot * rowCapacity + y + 1] = sum1.GetUpper();
                            }
                            y++;
                            continue;
                        }
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
                    if (typeof(TBand) == typeof(SixtyFourRowBand) && (outputY & 7) == 0)
                        context.Cancellation.ThrowIfCancellationRequested();
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
                    // All source pairs have retired into the column cache.
                    var packed = MemoryMarshal.AsBytes(decoded)[..16];
                    var accepted = BicubicMath.WriteByteQuartet(firstSum, secondSum,
                        normalizerX * normalizers[outputY], normalizerX * normalizers[outputY + 1],
                        normalizerX * normalizers[outputY + 2], normalizerX * normalizers[outputY + 3], packed);
                    for (var pair = 0; pair < 2; pair++)
                    {
                        var y = outputY + pair * 2;
                        if ((accepted & (1 << pair)) != 0)
                        {
                            BinaryPrimitives.WriteUInt32LittleEndian(output.Slice((tileY + y) * outputStride + x * 4, 4),
                                BinaryPrimitives.ReadUInt32LittleEndian(packed.Slice(pair * 8, 4)));
                            BinaryPrimitives.WriteUInt32LittleEndian(output.Slice((tileY + y + 1) * outputStride + x * 4, 4),
                                BinaryPrimitives.ReadUInt32LittleEndian(packed.Slice(pair * 8 + 4, 4)));
                        }
                        else if (!OriginalByte(source, result, x, tileY + y, center, first,
                            vertical.Slice(y * 10, counts[y]), rowOffsets.Slice(y * 10, counts[y]), sourceFirst, context, ref deferred)
                            || !OriginalByte(source, result, x, tileY + y + 1, center, first,
                            vertical.Slice((y + 1) * 10, counts[y + 1]), rowOffsets.Slice((y + 1) * 10, counts[y + 1]), sourceFirst, context, ref deferred)) return false;
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
                    // its two rows then receive separate checked byte stores.
                    var packed = MemoryMarshal.AsBytes(decoded)[..8];
                    if (BicubicMath.TryWriteBytePair(sum, n0, n1, packed))
                    {
                        BinaryPrimitives.WriteUInt32LittleEndian(output.Slice((tileY + outputY) * outputStride + x * 4, 4),
                            BinaryPrimitives.ReadUInt32LittleEndian(packed[..4]));
                        BinaryPrimitives.WriteUInt32LittleEndian(output.Slice((tileY + outputY + 1) * outputStride + x * 4, 4),
                            BinaryPrimitives.ReadUInt32LittleEndian(packed[4..]));
                    }
                    else if (!OriginalByte(source, result, x, tileY + outputY, center, first,
                        vertical.Slice(outputY * 10, counts[outputY]), rowOffsets.Slice(outputY * 10, counts[outputY]), sourceFirst, context, ref deferred)
                        || !OriginalByte(source, result, x, tileY + outputY + 1, center, first,
                            vertical.Slice((outputY + 1) * 10, counts[outputY + 1]), rowOffsets.Slice((outputY + 1) * 10, counts[outputY + 1]), sourceFirst, context, ref deferred)) return false;
                }
                for (var y = outputY; y < length; y++)
                {
                    var sum = Vector256<double>.Zero;
                    for (var t = 0; t < count; t++)
                        sum = Avx.Add(sum, Avx.Multiply(cache[columns[t] + y], Vector256.Create(horizontal[t])));
                    var normalizer = normalizerX * normalizers[y];
                    if (!BicubicMath.TryWriteByte(sum, normalizer, output.Slice((tileY + y) * outputStride + x * 4, 4))
                        && !OriginalByte(source, result, x, tileY + y, center, first,
                            vertical.Slice(y * 10, counts[y]), rowOffsets.Slice(y * 10, counts[y]), sourceFirst, context, ref deferred)) return false;
                }
            }
        }
        context.Cancellation.ThrowIfCancellationRequested();
        return true;
    }

    private static bool OriginalByte(ImageBuffer source, ImageBuffer result, int x, int y,
        double centerX, int firstX, ReadOnlySpan<double> vertical, ReadOnlySpan<byte> rows,
        int sourceFirst, OperationContext context, ref bool deferred)
    {
        var lastX = (int)Math.Floor(centerX + 2);
        double red = 0, green = 0, blue = 0, alphaSum = 0, normalizer = 0;
        for (var ty = 0; ty < vertical.Length; ty++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var wy = vertical[ty]; if (wy == 0) continue;
            var row = source.GetRow8(sourceFirst + rows[ty]);
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
        if (normalized > 0 && normalized < 1)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(result.GetRow8(y).Slice(x * 4, 4), DeferredPixel);
            deferred = true;
            return true;
        }
        var alphaOut = BicubicMath.RoundByte(normalized);
        var color = alphaOut > 0 && alphaSum > 0;
        var destination = result.GetRow8(y).Slice(x * 4, 4);
        destination[0] = BicubicMath.RoundByte(color ? red / alphaSum : 0);
        destination[1] = BicubicMath.RoundByte(color ? green / alphaSum : 0);
        destination[2] = BicubicMath.RoundByte(color ? blue / alphaSum : 0);
        destination[3] = alphaOut;
        return true;
    }

    private static void ReplayTiny(ImageBuffer source, ImageBuffer result, double scaleX, double scaleY, OperationContext context)
    {
        for (var y = 0; y < result.Height; y++)
        {
            var destination = result.GetRow8(y);
            for (var x = 0; x < result.Width; x++)
            {
                if ((x & 255) == 0) context.Cancellation.ThrowIfCancellationRequested();
                if (BinaryPrimitives.ReadUInt32LittleEndian(destination.Slice(x * 4, 4)) != DeferredPixel) continue;
                var centerX = (x + .5) * source.Width / result.Width - .5;
                var centerY = (y + .5) * source.Height / result.Height - .5;
                var firstX = (int)Math.Ceiling(centerX - 2 * scaleX);
                var lastX = (int)Math.Floor(centerX + 2 * scaleX);
                var firstY = (int)Math.Ceiling(centerY - 2 * scaleY);
                var lastY = (int)Math.Floor(centerY + 2 * scaleY);
                double red = 0, green = 0, blue = 0, alphaSum = 0;
                for (var sy = firstY; sy <= lastY; sy++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var wy = ImageTransforms.Kernel((sy - centerY) / scaleY, ResizeFilter.Bicubic);
                    if (wy == 0) continue;
                    var row = source.GetRow8(Math.Clamp(sy, 0, source.Height - 1));
                    for (var sx = firstX; sx <= lastX; sx++)
                    {
                        var w = wy * ImageTransforms.Kernel((sx - centerX) / scaleX, ResizeFilter.Bicubic);
                        if (w == 0) continue;
                        var offset = Math.Clamp(sx, 0, source.Width - 1) * 4;
                        var a = row[offset + 3];
                        red += w * a * row[offset]; green += w * a * row[offset + 1]; blue += w * a * row[offset + 2];
                        alphaSum += w * a;
                    }
                }
                var rounded = ResizeAlpha.Round(source, x, y, result.Width, result.Height, ResizeFilter.Bicubic, context);
                var color = rounded > 0 && alphaSum > 0;
                var pixel = destination.Slice(x * 4, 4);
                pixel[0] = BicubicMath.RoundByte(color ? red / alphaSum : 0);
                pixel[1] = BicubicMath.RoundByte(color ? green / alphaSum : 0);
                pixel[2] = BicubicMath.RoundByte(color ? blue / alphaSum : 0);
                pixel[3] = (byte)rounded;
            }
        }
    }
}
