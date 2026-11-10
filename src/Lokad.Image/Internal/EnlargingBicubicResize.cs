using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Shared enlargement preparation and two output-workspace schedules. Pixel
// decoding, certified publication and repair retain their depth-specific kernels.
internal static class EnlargingBicubicResize
{
    private const int WorkspaceBytes = 4096;
    private readonly struct ByteSamples { }
    private readonly struct WordSamples { }

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth is not (8 or 16) || result.Width < source.Width || result.Height < source.Height
            || result.Width == source.Width && result.Height == source.Height
            || tapsX > 5 || tapsY > 5
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        var deferred = false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // Both schedules borrow this one admitted scratch lifetime. Prefix
            // workspace views unwind before strips overwrite the output tail.
            if (source.BitDepth == 8)
            {
                if (!TryApplyPrefix<ByteSamples>(source, result, scaleX, scaleY, context, out var firstRow, ref deferred)
                    || !TryApplyStrips<ByteSamples>(source, result, scaleX, scaleY, firstRow, context, ref deferred)) return false;
            }
            else
            {
                if (!TryApplyPrefix<WordSamples>(source, result, scaleX, scaleY, context, out var firstRow, ref deferred)
                    || !TryApplyStrips<WordSamples>(source, result, scaleX, scaleY, firstRow, context, ref deferred)) return false;
            }
        }
        finally { context.Release(WorkspaceBytes); }
        // Exact tiny-alpha scratch is acquired after both stack frames unwind.
        if (deferred)
        {
            if (source.BitDepth == 8) EnlargingColumnBicubicResize.ReplayTiny(source, result, scaleX, scaleY, context);
            else EnlargingWordColumnBicubicResize.ReplayTiny(source, result, scaleX, scaleY, context);
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryApplyPrefix<TSamples>(ImageBuffer source, ImageBuffer result, double scaleX, double scaleY,
        OperationContext context, out int firstRow, ref bool deferred)
    {
        firstRow = 0;
        var width = result.Width;
        // Owned result capacity is charged before entry. No completed pixels
        // overlap these views, and nothing borrows them after this call returns.
        var bytes = (long)width * 232 + (long)source.Width * 32;
        var output = result.Pixels;
        var end = output.Length & ~7;
        if (bytes > end) return true;
        var start = end - (int)bytes;
        var prefixRows = start / result.Stride;
        if (prefixRows == 0) return true;
        var storage = output.Slice(start, (int)bytes);
        var cache = MemoryMarshal.Cast<byte, Vector256<double>>(storage[..(width * 160)]);
        var weights = MemoryMarshal.Cast<byte, double>(storage.Slice(width * 160, width * 40));
        var offsets = MemoryMarshal.Cast<byte, int>(storage.Slice(width * 200, width * 20));
        var counts = MemoryMarshal.Cast<byte, int>(storage.Slice(width * 220, width * 4));
        var normalizers = MemoryMarshal.Cast<byte, double>(storage.Slice(width * 224, width * 8));
        var decoded = MemoryMarshal.Cast<byte, Vector256<double>>(storage[(width * 232)..]);
        PrepareHorizontal(0, width, source.Width, width, scaleX, weights, offsets, counts, normalizers, context);
        Span<int> rowIds = stackalloc int[5]; rowIds.Fill(-1);
        Span<double> vertical = stackalloc double[5]; Span<int> rows = stackalloc int[5];
        for (var y = 0; y < prefixRows; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var normalizerY = PrepareVertical(y, source.Height, result.Height, scaleY, vertical, rows, out var first, out var count);
            for (var t = 0; t < count; t++)
            {
                var sy = rows[t]; var slot = sy % 5; rows[t] = slot * width;
                if (rowIds[slot] == sy) continue;
                rowIds[slot] = sy;
                DecodeRow<TSamples>(source, sy, 0, source.Width, decoded, context);
                FilterHorizontal(decoded, weights, offsets, counts, cache.Slice(slot * width, width), context);
            }
            if (!WriteRow<TSamples>(source, result, 0, y, width, cache, normalizers, weights, offsets, counts,
                0, vertical[..count], rows, first, normalizerY, context, ref deferred)) return false;
        }
        firstRow = prefixRows;
        return true;
    }

    // Fixed arrays occupy 4,000 bytes. The prefix's output-backed views are
    // dead before this frame materializes every byte of the remaining tail.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryApplyStrips<TSamples>(ImageBuffer source, ImageBuffer result, double scaleX, double scaleY,
        int firstRow, OperationContext context, ref bool deferred)
    {
        Span<Vector256<double>> cache = stackalloc Vector256<double>[5 * 14];
        Span<int> rowIds = stackalloc int[5];
        Span<Vector256<double>> decoded = stackalloc Vector256<double>[21];
        Span<double> weights = stackalloc double[14 * 5];
        Span<int> offsets = stackalloc int[14 * 5];
        Span<int> counts = stackalloc int[14];
        Span<double> normalizers = stackalloc double[14];
        Span<double> vertical = stackalloc double[5]; Span<int> rows = stackalloc int[5];
        for (var tile = 0; tile < result.Width; tile += 14)
        {
            var length = Math.Min(14, result.Width - tile);
            PrepareHorizontal(tile, length, source.Width, result.Width, scaleX, weights, offsets, counts, normalizers, context);
            var sourceFirst = offsets[0];
            var sourceLast = offsets[(length - 1) * 5 + counts[length - 1] - 1];
            var sourceCount = sourceLast - sourceFirst + 1;
            if (sourceCount > decoded.Length) return false;
            for (var x = 0; x < length; x++)
                for (var t = 0; t < counts[x]; t++) offsets[x * 5 + t] -= sourceFirst;
            rowIds.Fill(-1);
            for (var y = firstRow; y < result.Height; y++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var normalizerY = PrepareVertical(y, source.Height, result.Height, scaleY, vertical, rows, out var first, out var count);
                for (var t = 0; t < count; t++)
                {
                    var sy = rows[t]; var slot = sy % 5; rows[t] = slot * 14;
                    if (rowIds[slot] == sy) continue;
                    rowIds[slot] = sy;
                    DecodeRow<TSamples>(source, sy, sourceFirst, sourceCount, decoded[..sourceCount], context);
                    FilterHorizontal(decoded, weights, offsets, counts[..length], cache.Slice(slot * 14, length), context);
                }
                if (!WriteRow<TSamples>(source, result, tile, y, length, cache, normalizers, weights, offsets, counts,
                    sourceFirst, vertical[..count], rows, first, normalizerY, context, ref deferred)) return false;
            }
        }
        context.Cancellation.ThrowIfCancellationRequested();
        return true;
    }

    private static void PrepareHorizontal(int tile, int length, int sourceWidth, int width, double scaleX,
        Span<double> weights, Span<int> offsets, Span<int> counts, Span<double> normalizers, OperationContext context)
    {
        for (var x = 0; x < length; x++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var center = (tile + x + .5) * sourceWidth / width - .5;
            var first = (int)Math.Ceiling(center - 2 * scaleX);
            var count = (int)Math.Floor(center + 2 * scaleX) - first + 1;
            counts[x] = count; double normalizer = 0;
            for (var t = 0; t < count; t++)
            {
                var w = ImageTransforms.Kernel((first + t - center) / scaleX, ResizeFilter.Bicubic);
                weights[x * 5 + t] = w; normalizer += w;
                offsets[x * 5 + t] = Math.Clamp(first + t, 0, sourceWidth - 1);
            }
            normalizers[x] = normalizer;
        }
    }

    private static double PrepareVertical(int y, int sourceHeight, int height, double scaleY,
        Span<double> vertical, Span<int> rows, out int first, out int count)
    {
        var center = (y + .5) * sourceHeight / height - .5;
        // Preserve original cardinal and binary64 coordinate behavior.
        var identityRow = height == sourceHeight && center == y;
        first = identityRow ? y : (int)Math.Ceiling(center - 2 * scaleY);
        count = identityRow ? 1 : (int)Math.Floor(center + 2 * scaleY) - first + 1;
        double normalizer = 0;
        for (var t = 0; t < count; t++)
        {
            var w = identityRow ? 1 : ImageTransforms.Kernel((first + t - center) / scaleY, ResizeFilter.Bicubic);
            vertical[t] = w; normalizer += w; rows[t] = Math.Clamp(first + t, 0, sourceHeight - 1);
        }
        return normalizer;
    }

    // Specialization is at source-row and output-row boundaries, outside the
    // typed sample loops. These marker types carry no state or ownership.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DecodeRow<TSamples>(ImageBuffer source, int y, int first, int count,
        Span<Vector256<double>> decoded, OperationContext context)
    {
        if (typeof(TSamples) == typeof(WordSamples))
            EnlargingWordColumnBicubicResize.DecodeRow(MemoryMarshal.AsBytes(source.GetRow16(y)).Slice(first * 8, count * 8), decoded, context);
        else
            EnlargingColumnBicubicResize.DecodeRow(source.GetRow8(y).Slice(first * 4, count * 4), decoded, context);
    }

    private static void FilterHorizontal(ReadOnlySpan<Vector256<double>> decoded, ReadOnlySpan<double> weights,
        ReadOnlySpan<int> offsets, ReadOnlySpan<int> counts, Span<Vector256<double>> cache, OperationContext context)
    {
        for (var x = 0; x < cache.Length; x++)
        {
            if ((x & 255) == 0) context.Cancellation.ThrowIfCancellationRequested();
            var tapWeights = weights.Slice(x * 5, counts[x]);
            var tapOffsets = offsets.Slice(x * 5, counts[x]);
            var sum = Vector256<double>.Zero;
            for (var tx = 0; tx < tapWeights.Length; tx++)
                sum = Avx.Add(sum, Avx.Multiply(decoded[tapOffsets[tx]], Vector256.Create(tapWeights[tx])));
            cache[x] = sum;
        }
    }

    // Both depths accumulate four adjacent pixels in the same tap order.
    // Publication and exact repair remain depth-specific.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void FilterVerticalQuartet(ReadOnlySpan<Vector256<double>> cache,
        ReadOnlySpan<double> vertical, ReadOnlySpan<int> rows, int x,
        out Vector512<double> first, out Vector512<double> second)
    {
        var firstSum = Vector512<double>.Zero; var secondSum = Vector512<double>.Zero;
        for (var t = 0; t < vertical.Length; t++)
        {
            var weight = Vector512.Create(vertical[t]);
            var row = MemoryMarshal.AsBytes(cache.Slice(rows[t] + x, 4));
            firstSum = Avx512F.Add(firstSum, Avx512F.Multiply(MemoryMarshal.Read<Vector512<double>>(row), weight));
            secondSum = Avx512F.Add(secondSum, Avx512F.Multiply(MemoryMarshal.Read<Vector512<double>>(row[64..]), weight));
        }
        first = firstSum; second = secondSum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool WriteRow<TSamples>(ImageBuffer source, ImageBuffer result, int tile, int y, int length,
        ReadOnlySpan<Vector256<double>> cache, ReadOnlySpan<double> normalizers, ReadOnlySpan<double> weights,
        ReadOnlySpan<int> offsets, ReadOnlySpan<int> counts, int sourceFirst, ReadOnlySpan<double> vertical,
        ReadOnlySpan<int> rows, int first, double normalizerY, OperationContext context, ref bool deferred)
    {
        return typeof(TSamples) == typeof(WordSamples)
            ? EnlargingWordColumnBicubicResize.WriteRow(source, result, tile, y, length, cache, normalizers, weights,
                offsets, counts, sourceFirst, vertical, rows, first, normalizerY, context, ref deferred)
            : EnlargingColumnBicubicResize.WriteRow(source, result, tile, y, length, cache, normalizers, weights,
                offsets, counts, sourceFirst, vertical, rows, first, normalizerY, context, ref deferred);
    }
}
