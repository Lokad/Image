using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Fourteen-column strips reuse rows for one-axis or two-axis enlargement within an admitted workspace.</summary>
internal static class EnlargingWordColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    // A completed resize clears RGB under zero alpha, so this temporary marker
    // cannot collide with a completed pixel. The result is private until return.
    private const ulong DeferredPixel = 0x0000FFFFFFFFFFFF;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (!TryApplyCore(source, result, scaleX, scaleY, tapsX, tapsY, context, out var deferred)) return false;
        if (deferred) ReplayTiny(source, result, scaleX, scaleY, context);
        return true;
    }

    // Exact tiny-alpha scratch is acquired only after this stack frame returns.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryApplyCore(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context, out bool deferred)
    {
        deferred = false;
        if (source.BitDepth != 16 || result.Width < source.Width || result.Height < source.Height
            || result.Width == source.Width && result.Height == source.Height
            || tapsX > 5 || tapsY > 5
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // Fixed arrays occupy 4,000 bytes within the admitted charge. This
            // frame unwinds before direct replay can need tiny-alpha scratch.
            Span<Vector256<double>> cache = stackalloc Vector256<double>[5 * 14];
            Span<int> rowIds = stackalloc int[5];
            Span<Vector256<double>> decoded = stackalloc Vector256<double>[21];
            Span<double> weights = stackalloc double[14 * 5];
            Span<int> offsets = stackalloc int[14 * 5];
            Span<int> counts = stackalloc int[14];
            Span<double> normalizers = stackalloc double[14];
            Span<double> vertical = stackalloc double[5];
            Span<int> rows = stackalloc int[5];
            var input = source.Pixels;
            var stride = source.Stride;
            var identityHeight = result.Height == source.Height;
            for (var tile = 0; tile < result.Width; tile += 14)
            {
                var length = Math.Min(14, result.Width - tile);
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
                        weights[x * 5 + t] = w; normalizer += w;
                        offsets[x * 5 + t] = Math.Clamp(first + t, 0, source.Width - 1);
                    }
                    normalizers[x] = normalizer;
                }
                var sourceFirst = offsets[0];
                var sourceLast = offsets[(length - 1) * 5 + counts[length - 1] - 1];
                var sourceCount = sourceLast - sourceFirst + 1;
                if (sourceCount > decoded.Length) return false;
                for (var x = 0; x < length; x++)
                    for (var t = 0; t < counts[x]; t++) offsets[x * 5 + t] -= sourceFirst;
                rowIds.Fill(-1);
                for (var y = 0; y < result.Height; y++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    // Integer centers on an unchanged axis have the exact
                    // cardinal kernel [0, 0, 1, 0, 0]. Omit zero-weight rows;
                    // workspace admission still covers the original support.
                    var center = (y + .5) * source.Height / result.Height - .5;
                    // Retain original coordinate rounding even if callers raise
                    // dimension limits beyond exact binary64 products.
                    var identityRow = identityHeight && center == y;
                    var first = identityRow ? y : (int)Math.Ceiling(center - 2 * scaleY);
                    var count = identityRow ? 1 : (int)Math.Floor(center + 2 * scaleY) - first + 1;
                    double normalizerY = 0;
                    for (var t = 0; t < count; t++)
                    {
                        var wy = identityRow ? 1 : ImageTransforms.Kernel((first + t - center) / scaleY, ResizeFilter.Bicubic);
                        vertical[t] = wy; normalizerY += wy;
                        var sy = Math.Clamp(first + t, 0, source.Height - 1);
                        var slot = sy % 5; rows[t] = slot * 14;
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
                            var tapWeights = weights.Slice(x * 5, counts[x]);
                            var tapOffsets = offsets.Slice(x * 5, counts[x]);
                            var sum = Vector256<double>.Zero;
                            for (var tx = 0; tx < tapWeights.Length; tx++)
                                sum = Avx.Add(sum, Avx.Multiply(decoded[tapOffsets[tx]], Vector256.Create(tapWeights[tx])));
                            cache[slot * 14 + x] = sum;
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
                        if (!BicubicMath.TryWriteWordPairWide(sum, n0, n1, destination.Slice((tile + outputX) * 8, 16)))
                        {
                            if (!(n0 >= .5 && n1 >= .5)
                                || !OriginalWord(source, result, tile + outputX, y, weights.Slice(outputX * 5, counts[outputX]),
                                    offsets.Slice(outputX * 5, counts[outputX]), sourceFirst, vertical[..count], first, context, ref deferred)
                                || !OriginalWord(source, result, tile + outputX + 1, y, weights.Slice((outputX + 1) * 5, counts[outputX + 1]),
                                    offsets.Slice((outputX + 1) * 5, counts[outputX + 1]), sourceFirst, vertical[..count], first, context, ref deferred)) return false;
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
                                || !OriginalWord(source, result, tile + x, y, weights.Slice(x * 5, counts[x]),
                                    offsets.Slice(x * 5, counts[x]), sourceFirst, vertical[..count], first, context, ref deferred)) return false;
                        }
                    }
                }
            }
            // Admission bounds complete original tap cost. Failed speculative
            // strips leave the counter untouched for original-order replay.
            context.Cancellation.ThrowIfCancellationRequested();
            return true;
        }
        finally { context.Release(WorkspaceBytes); }
    }

    private static bool OriginalWord(ImageBuffer source, ImageBuffer result, int x, int y,
        ReadOnlySpan<double> weights, ReadOnlySpan<int> offsets, int sourceFirst,
        ReadOnlySpan<double> vertical, int firstY, OperationContext context, ref bool deferred)
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
        if (normalized > 0 && normalized < 1)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(result.Pixels.Slice(y * result.Stride + x * 8, 8), DeferredPixel);
            deferred = true;
            return true;
        }
        var aOut = BicubicMath.RoundWord(normalized);
        var color = aOut > 0 && alphaSum > 0;
        var destination = result.GetRow16(y).Slice(x * 4, 4);
        destination[0] = BicubicMath.RoundWord(color ? red / alphaSum : 0);
        destination[1] = BicubicMath.RoundWord(color ? green / alphaSum : 0);
        destination[2] = BicubicMath.RoundWord(color ? blue / alphaSum : 0);
        destination[3] = aOut;
        return true;
    }

    private static void ReplayTiny(ImageBuffer source, ImageBuffer result, double scaleX, double scaleY, OperationContext context)
    {
        for (var y = 0; y < result.Height; y++)
        {
            var destination = result.GetRow16(y);
            for (var x = 0; x < result.Width; x++)
            {
                if ((x & 255) == 0) context.Cancellation.ThrowIfCancellationRequested();
                if (BinaryPrimitives.ReadUInt64LittleEndian(MemoryMarshal.AsBytes(destination.Slice(x * 4, 4))) != DeferredPixel) continue;
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
                    var row = source.GetRow16(Math.Clamp(sy, 0, source.Height - 1));
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
                pixel[0] = BicubicMath.RoundWord(color ? red / alphaSum : 0);
                pixel[1] = BicubicMath.RoundWord(color ? green / alphaSum : 0);
                pixel[2] = BicubicMath.RoundWord(color ? blue / alphaSum : 0);
                pixel[3] = (ushort)rounded;
            }
        }
    }

}
