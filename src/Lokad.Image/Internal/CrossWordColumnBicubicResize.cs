using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Eight-column strips reuse filtered rows while reducing width and preserving or enlarging height.</summary>
internal static class CrossWordColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    // Up to this bound (y + .5) * height is an exact half-integer below
    // 2^52. Division and subtraction recover y exactly, so the five original
    // vertical weights are +0,+0,1,+0,+0, with normalizer one.
    internal static bool IsIdentityHeight(int sourceHeight, int resultHeight) =>
        sourceHeight > 0 && sourceHeight <= 1 << 26 && sourceHeight == resultHeight;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth != 16 || result.Width >= source.Width || result.Height < source.Height
            || tapsX > 10 || tapsY > 5
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        // Resize owns this unpublished output independently of the source. Its
        // charged suffix can hold workspace until final tail rows overwrite it.
        var cacheRows = IsIdentityHeight(source.Height, result.Height) ? 1 : 5;
        var workspaceBytes = (132L + 32L * cacheRows) * result.Width + 32L * source.Width;
        var tailRows = (workspaceBytes + result.Stride - 1) / result.Stride;
        var prefixRows = tailRows <= result.Height / 2 ? result.Height - (int)tailRows : 0;
        context.Reserve(WorkspaceBytes);
        try
        {
            if (prefixRows > 0 && !ApplyRows(source, result, prefixRows,
                result.Pixels.Slice(prefixRows * result.Stride, (int)workspaceBytes), scaleX, scaleY, context))
                return false;
            return ApplyStrips(source, result, prefixRows, scaleX, scaleY, context);
        }
        finally { context.Release(WorkspaceBytes); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ApplyStrips(ImageBuffer source, ImageBuffer result, int startY,
        double scaleX, double scaleY, OperationContext context)
    {
        var identity = IsIdentityHeight(source.Height, result.Height);
        var cacheRows = identity ? 1 : 5;
        // Fixed arrays occupy at most 3,440 bytes within the admitted charge. This
        // frame unwinds before direct replay can need tiny-alpha scratch.
        Span<Vector256<double>> cache = stackalloc Vector256<double>[cacheRows * 8];
        Span<int> rowIds = stackalloc int[cacheRows];
        Span<Vector256<double>> decoded = stackalloc Vector256<double>[32];
        Span<double> weights = stackalloc double[8 * 10];
        Span<int> offsets = stackalloc int[8 * 10];
        Span<int> counts = stackalloc int[8];
        Span<double> normalizers = stackalloc double[8];
        Span<double> vertical = stackalloc double[cacheRows];
        Span<int> rows = stackalloc int[cacheRows];
        var input = source.Pixels;
        var stride = source.Stride;
        for (var tile = 0; tile < result.Width; tile += 8)
        {
            var length = Math.Min(8, result.Width - tile);
            for (var x = 0; x < length; x++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var center = (tile + x + .5) * source.Width / result.Width - .5;
                var first = (int)Math.Ceiling(center - 2 * scaleX);
                var count = (int)Math.Floor(center + 2 * scaleX) - first + 1;
                counts[x] = count;
                double normalizer = 0;
                for (var t = 0; t < count; t++)
                {
                    var w = ImageTransforms.Kernel((first + t - center) / scaleX, ResizeFilter.Bicubic);
                    weights[x * 10 + t] = w; normalizer += w;
                    offsets[x * 10 + t] = Math.Clamp(first + t, 0, source.Width - 1);
                }
                normalizers[x] = normalizer;
            }
            var sourceFirst = offsets[0];
            var sourceLast = offsets[(length - 1) * 10 + counts[length - 1] - 1];
            var sourceCount = sourceLast - sourceFirst + 1;
            if (sourceCount > decoded.Length) return false;
            for (var x = 0; x < length; x++)
                for (var t = 0; t < counts[x]; t++) offsets[x * 10 + t] -= sourceFirst;
            BicubicMath.PrepareOverlappingPairs(offsets, counts, length, sourceCount, 10, 8, 2, context.Cancellation);
            rowIds.Fill(-1);
            for (var y = startY; y < result.Height; y++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var center = identity ? 0 : (y + .5) * source.Height / result.Height - .5;
                var first = identity ? y : (int)Math.Ceiling(center - 2 * scaleY);
                var count = identity ? 1 : (int)Math.Floor(center + 2 * scaleY) - first + 1;
                double normalizerY = 0;
                for (var t = 0; t < count; t++)
                {
                    var wy = identity ? 1 : ImageTransforms.Kernel((first + t - center) / scaleY, ResizeFilter.Bicubic);
                    vertical[t] = wy; normalizerY += wy;
                    var sy = Math.Clamp(first + t, 0, source.Height - 1);
                    var slot = identity ? 0 : sy % 5; rows[t] = slot * 8;
                    if (rowIds[slot] == sy) continue;
                    rowIds[slot] = sy;
                    var row = input.Slice(sy * stride + sourceFirst * 8, sourceCount * 8);
                    var decodedBytes = MemoryMarshal.AsBytes(decoded[..sourceCount]);
                    var px = 0;
                    for (; px + 1 < sourceCount; px += 2)
                    {
                        var samples = Avx2.ConvertToVector256Int32(MemoryMarshal.Read<Vector128<ushort>>(row.Slice(px * 8, 16)));
                        var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                        var products = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors).AsUInt32());
                        MemoryMarshal.Write(decodedBytes.Slice(px * 32, 64), in products);
                    }
                    if (px < sourceCount)
                    {
                        var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(px * 8, 8));
                        var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsUInt16());
                        var a = (int)(packed >> 48);
                        var products = Avx512F.ConvertToVector512Double(Vector256.Create(
                            Sse41.MultiplyLow(samples, Vector128.Create(a, a, a, 1)).AsUInt32(), Vector128<uint>.Zero)).GetLower();
                        MemoryMarshal.Write(decodedBytes.Slice(px * 32, 32), in products);
                    }
                    for (var x = 0; x < length; x++)
                    {
                        if (x + 1 < length && counts[x] == 8 && counts[x + 1] == 8 && offsets[x * 10 + 9] != 0)
                        {
                            var pair = BicubicMath.FilterOverlappingHorizontalPair(decoded.Slice(offsets[x * 10 + 9] - 1, 10),
                                weights.Slice(x * 10, 8), weights.Slice((x + 1) * 10, 8), 8, 2);
                            cache[slot * 8 + x] = pair.GetLower(); cache[slot * 8 + x + 1] = pair.GetUpper();
                            x++;
                            continue;
                        }
                        var tapWeights = weights.Slice(x * 10, counts[x]);
                        var tapOffsets = offsets.Slice(x * 10, counts[x]);
                        var sum = Vector256<double>.Zero;
                        for (var tx = 0; tx < tapWeights.Length; tx++)
                            sum = Avx.Add(sum, Avx.Multiply(decoded[tapOffsets[tx]], Vector256.Create(tapWeights[tx])));
                        cache[slot * 8 + x] = sum;
                    }
                }
                var destination = result.Pixels.Slice(y * result.Stride, result.Stride);
                var outputX = 0;
                for (; outputX + 1 < length; outputX += 2)
                {
                    var sum = Vector512<double>.Zero;
                    for (var t = 0; t < count; t++)
                    {
                        var pair = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cache.Slice(rows[t] + outputX, 2)));
                        sum = Avx512F.Add(sum, Avx512F.Multiply(pair, Vector512.Create(vertical[t])));
                    }
                    var n0 = normalizers[outputX] * normalizerY;
                    var n1 = normalizers[outputX + 1] * normalizerY;
                    if (!BicubicMath.TryWriteWordPair(sum, n0, n1, destination.Slice((tile + outputX) * 8, 16)))
                    {
                        if (!(n0 >= .5 && n1 >= .5)
                            || !OriginalWord(source, result, tile + outputX, y, weights.Slice(outputX * 10, counts[outputX]),
                                offsets.Slice(outputX * 10, counts[outputX]), sourceFirst, vertical[..count], first, context)
                            || !OriginalWord(source, result, tile + outputX + 1, y, weights.Slice((outputX + 1) * 10, counts[outputX + 1]),
                                offsets.Slice((outputX + 1) * 10, counts[outputX + 1]), sourceFirst, vertical[..count], first, context)) return false;
                    }
                }
                for (var x = outputX; x < length; x++)
                {
                    var sum = Vector256<double>.Zero;
                    for (var t = 0; t < count; t++)
                        sum = Avx.Add(sum, Avx.Multiply(cache[rows[t] + x], Vector256.Create(vertical[t])));
                    var normalizer = normalizers[x] * normalizerY;
                    if (!BicubicMath.TryWriteWord(sum, normalizer, destination.Slice((tile + x) * 8, 8)))
                    {
                        if (!(normalizer >= .5)
                            || !OriginalWord(source, result, tile + x, y, weights.Slice(x * 10, counts[x]),
                                offsets.Slice(x * 10, counts[x]), sourceFirst, vertical[..count], first, context)) return false;
                    }
                }
            }
        }
        context.Cancellation.ThrowIfCancellationRequested();
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ApplyRows(ImageBuffer source, ImageBuffer result, int height, Span<byte> workspace,
        double scaleX, double scaleY, OperationContext context)
    {
        var identity = IsIdentityHeight(source.Height, result.Height);
        var cacheRows = identity ? 1 : 5;
        var width = result.Width;
        var cacheElements = cacheRows * 4 * width;
        var doubleCount = cacheElements + 11 * width + 4 * source.Width;
        var storage = MemoryMarshal.Cast<byte, double>(workspace[..(doubleCount * sizeof(double))]);
        var geometry = MemoryMarshal.Cast<byte, int>(workspace[(doubleCount * sizeof(double))..]);
        var cache = MemoryMarshal.Cast<double, Vector256<double>>(storage[..cacheElements]);
        var decoded = MemoryMarshal.Cast<double, Vector256<double>>(storage.Slice(cacheElements, source.Width * 4));
        var weights = storage.Slice(cacheElements + source.Width * 4, width * 10);
        var normalizers = storage.Slice(cacheElements + source.Width * 4 + width * 10, width);
        var offsets = geometry[..(width * 10)];
        var counts = geometry.Slice(width * 10, width);
        for (var x = 0; x < width; x++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var center = (x + .5) * source.Width / width - .5;
            var first = (int)Math.Ceiling(center - 2 * scaleX);
            var count = (int)Math.Floor(center + 2 * scaleX) - first + 1;
            counts[x] = count;
            double normalizer = 0;
            for (var t = 0; t < count; t++)
            {
                var w = ImageTransforms.Kernel((first + t - center) / scaleX, ResizeFilter.Bicubic);
                weights[x * 10 + t] = w; normalizer += w;
                offsets[x * 10 + t] = Math.Clamp(first + t, 0, source.Width - 1);
            }
            normalizers[x] = normalizer;
        }
        BicubicMath.PrepareOverlappingPairs(offsets, counts, width, source.Width, 10, 8, 2, context.Cancellation);
        Span<int> rowIds = stackalloc int[cacheRows]; rowIds.Fill(-1);
        Span<int> rows = stackalloc int[cacheRows];
        Span<double> vertical = stackalloc double[cacheRows];
        for (var y = 0; y < height; y++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var center = identity ? 0 : (y + .5) * source.Height / result.Height - .5;
            var first = identity ? y : (int)Math.Ceiling(center - 2 * scaleY);
            var count = identity ? 1 : (int)Math.Floor(center + 2 * scaleY) - first + 1;
            double normalizerY = 0;
            for (var t = 0; t < count; t++)
            {
                var wy = identity ? 1 : ImageTransforms.Kernel((first + t - center) / scaleY, ResizeFilter.Bicubic);
                vertical[t] = wy; normalizerY += wy;
                var sy = Math.Clamp(first + t, 0, source.Height - 1);
                var slot = identity ? 0 : sy % 5; rows[t] = slot * width;
                if (rowIds[slot] == sy) continue;
                rowIds[slot] = sy;
                var row = source.Pixels.Slice(sy * source.Stride, source.Stride);
                var decodedBytes = MemoryMarshal.AsBytes(decoded);
                var px = 0;
                for (; px + 1 < source.Width; px += 2)
                {
                    if ((px & 127) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    var samples = Avx2.ConvertToVector256Int32(MemoryMarshal.Read<Vector128<ushort>>(row.Slice(px * 8, 16)));
                    var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                    var products = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors).AsUInt32());
                    MemoryMarshal.Write(decodedBytes.Slice(px * 32, 64), in products);
                }
                if (px < source.Width)
                {
                    var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(px * 8, 8));
                    var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsUInt16());
                    var a = (int)(packed >> 48);
                    var products = Avx512F.ConvertToVector512Double(Vector256.Create(
                        Sse41.MultiplyLow(samples, Vector128.Create(a, a, a, 1)).AsUInt32(), Vector128<uint>.Zero)).GetLower();
                    MemoryMarshal.Write(decodedBytes.Slice(px * 32, 32), in products);
                }
                var filtered = cache.Slice(rows[t], width);
                for (var x = 0; x < width; x++)
                {
                    if ((x & 127) == 0) context.Cancellation.ThrowIfCancellationRequested();
                    if (x + 1 < width && counts[x] == 8 && counts[x + 1] == 8 && offsets[x * 10 + 9] != 0)
                    {
                        var pair = BicubicMath.FilterOverlappingHorizontalPair(decoded.Slice(offsets[x * 10 + 9] - 1, 10),
                            weights.Slice(x * 10, 8), weights.Slice((x + 1) * 10, 8), 8, 2);
                        filtered[x] = pair.GetLower(); filtered[x + 1] = pair.GetUpper();
                        x++;
                        continue;
                    }
                    var tapWeights = weights.Slice(x * 10, counts[x]);
                    var tapOffsets = offsets.Slice(x * 10, counts[x]);
                    var sum = Vector256<double>.Zero;
                    for (var tx = 0; tx < tapWeights.Length; tx++)
                        sum = Avx.Add(sum, Avx.Multiply(decoded[tapOffsets[tx]], Vector256.Create(tapWeights[tx])));
                    filtered[x] = sum;
                }
            }
            var destination = result.Pixels.Slice(y * result.Stride, result.Stride);
            var outputX = 0;
            for (; outputX + 1 < width; outputX += 2)
            {
                if ((outputX & 127) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var sum = Vector512<double>.Zero;
                for (var t = 0; t < count; t++)
                {
                    var pair = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cache.Slice(rows[t] + outputX, 2)));
                    sum = Avx512F.Add(sum, Avx512F.Multiply(pair, Vector512.Create(vertical[t])));
                }
                var n0 = normalizers[outputX] * normalizerY;
                var n1 = normalizers[outputX + 1] * normalizerY;
                if (!BicubicMath.TryWriteWordPair(sum, n0, n1, destination.Slice(outputX * 8, 16)))
                {
                    if (!(n0 >= .5 && n1 >= .5)
                        || !OriginalWord(source, result, outputX, y, weights.Slice(outputX * 10, counts[outputX]),
                            offsets.Slice(outputX * 10, counts[outputX]), 0, vertical[..count], first, context)
                        || !OriginalWord(source, result, outputX + 1, y, weights.Slice((outputX + 1) * 10, counts[outputX + 1]),
                            offsets.Slice((outputX + 1) * 10, counts[outputX + 1]), 0, vertical[..count], first, context)) return false;
                }
            }
            if (outputX < width)
            {
                var sum = Vector256<double>.Zero;
                for (var t = 0; t < count; t++)
                    sum = Avx.Add(sum, Avx.Multiply(cache[rows[t] + outputX], Vector256.Create(vertical[t])));
                var normalizer = normalizers[outputX] * normalizerY;
                if (!BicubicMath.TryWriteWord(sum, normalizer, destination.Slice(outputX * 8, 8))
                    && (!(normalizer >= .5)
                        || !OriginalWord(source, result, outputX, y, weights.Slice(outputX * 10, counts[outputX]),
                            offsets.Slice(outputX * 10, counts[outputX]), 0, vertical[..count], first, context))) return false;
            }
        }
        return true;
    }

    private static bool OriginalWord(ImageBuffer source, ImageBuffer result, int x, int y,
        ReadOnlySpan<double> weights, ReadOnlySpan<int> offsets, int sourceFirst,
        ReadOnlySpan<double> vertical, int firstY, OperationContext context)
    {
        double red = 0, green = 0, blue = 0, alphaSum = 0, normalizer = 0;
        for (var ty = 0; ty < vertical.Length; ty++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var wy = vertical[ty]; if (wy == 0) continue;
            var row = source.GetRow16(Math.Clamp(firstY + ty, 0, source.Height - 1));
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
        // Exact tiny-alpha arithmetic needs the complete 4-KiB allowance.
        // Decline while this frame is live; the caller replays after it unwinds.
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
