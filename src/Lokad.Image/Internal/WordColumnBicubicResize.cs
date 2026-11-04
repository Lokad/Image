using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Bounded column strips reuse source rows within an admitted stack workspace.</summary>
internal static class WordColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth != 16 || result.Width < 2 || result.Width >= source.Width || result.Height >= source.Height
            || tapsX > 10 || tapsY > 10
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        const int sampleBytes = 2;
        context.Reserve(WorkspaceBytes);
        try
        {
            // Fixed arrays occupy 4,080 bytes within the admitted charge. This
            // frame unwinds before direct replay can need tiny-alpha scratch.
            Span<Vector256<double>> cache = stackalloc Vector256<double>[9 * 8];
            Span<int> rowIds = stackalloc int[10];
            Span<Vector256<double>> decoded = stackalloc Vector256<double>[25];
            Span<double> weights = stackalloc double[8 * 10];
            Span<byte> offsets = stackalloc byte[8 * 10];
            Span<int> counts = stackalloc int[8];
            Span<double> normalizers = stackalloc double[8];
            Span<double> vertical = stackalloc double[10];
            Span<int> rows = stackalloc int[10];
            var input = source.Pixels;
            var rowSlots = scaleX < 2.25 && scaleY < 2.25 ? 9 : 10;
            var stripColumns = rowSlots == 9 ? 8 : 6;
            var geometry = ColumnBicubicGeometry.Prepare(result, source.Height, scaleY, 10,
                stripColumns, context.Cancellation);
            for (var tile = 0; tile < result.Width; tile += stripColumns)
            {
                var length = Math.Min(stripColumns, result.Width - tile);
                var firstCenter = (tile + .5) * source.Width / result.Width - .5;
                var sourceFirst = Math.Clamp((int)Math.Ceiling(firstCenter - 2 * scaleX), 0, source.Width - 1);
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
                        var relative = Math.Clamp(first + t, 0, source.Width - 1) - sourceFirst;
                        if ((uint)relative >= 25) return false;
                        offsets[x * 10 + t] = (byte)relative;
                    }
                    normalizers[x] = normalizer;
                }
                var sourceCount = offsets[(length - 1) * 10 + counts[length - 1] - 1] + 1;
                rowIds.Fill(-1);
                for (var y = 0; y < result.Height; y++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    geometry.Read(y, tile < geometry.PrefixColumns, vertical,
                        out var first, out var count, out var normalizerY);
                    if (count > rowSlots) return false;
                    for (var t = 0; t < count; t++)
                    {
                        var sy = Math.Clamp(first + t, 0, source.Height - 1);
                        var slot = sy % rowSlots; rows[t] = slot * stripColumns;
                        if (rowIds[slot] == sy) continue;
                        rowIds[slot] = sy;
                        var row = input.Slice(sy * source.Stride + sourceFirst * 4 * sampleBytes, sourceCount * 4 * sampleBytes);
                        var px = 0;
                        for (; px + 1 < sourceCount; px += 2)
                        {
                            var samples = Avx2.ConvertToVector256Int32(MemoryMarshal.Read<Vector128<ushort>>(row.Slice(px * 8, 16)));
                            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                            var products = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors).AsUInt32());
                            decoded[px] = products.GetLower(); decoded[px + 1] = products.GetUpper();
                        }
                        if (px < sourceCount)
                        {
                            var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(px * 8, 8));
                            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsUInt16());
                            var alpha = (int)(packed >> 48);
                            var products = Sse41.MultiplyLow(samples, Vector128.Create(alpha, alpha, alpha, 1));
                            decoded[px] = Avx512F.ConvertToVector512Double(Vector256.Create(products.AsUInt32(), Vector128<uint>.Zero)).GetLower();
                        }
                        var x = 0;
                        for (; x + 1 < length; x += 2)
                        {
                            var sum0 = Vector256<double>.Zero;
                            var sum1 = Vector256<double>.Zero;
                            var count0 = counts[x]; var count1 = counts[x + 1];
                            for (var tx = 0; tx < Math.Max(count0, count1); tx++)
                            {
                                if (tx < count0)
                                    sum0 = Avx.Add(sum0, Avx.Multiply(decoded[offsets[x * 10 + tx]], Vector256.Create(weights[x * 10 + tx])));
                                if (tx < count1)
                                    sum1 = Avx.Add(sum1, Avx.Multiply(decoded[offsets[(x + 1) * 10 + tx]], Vector256.Create(weights[(x + 1) * 10 + tx])));
                            }
                            cache[slot * stripColumns + x] = sum0; cache[slot * stripColumns + x + 1] = sum1;
                        }
                        if (x < length)
                        {
                            var sum = Vector256<double>.Zero;
                            for (var tx = 0; tx < counts[x]; tx++)
                                sum = Avx.Add(sum, Avx.Multiply(decoded[offsets[x * 10 + tx]], Vector256.Create(weights[x * 10 + tx])));
                            cache[slot * stripColumns + x] = sum;
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
                        if (!BicubicMath.TryWriteWordPairWide(sum, n0, n1, destination.Slice((tile + outputX) * 4 * sampleBytes, 8 * sampleBytes)))
                        {
                            // Word color bounds admit fewer pixels. Replay only
                            // uncertain ordinary-alpha pixels in original order;
                            // tiny alpha still unwinds this frame before scratch.
                            if (!(n0 >= .5 && n1 >= .5 && sum.GetElement(3) / n0 >= 2 && sum.GetElement(7) / n1 >= 2)
                                || !OriginalWord(source, result, tile + outputX, y, weights.Slice(outputX * 10, counts[outputX]),
                                    offsets.Slice(outputX * 10, counts[outputX]), sourceFirst, vertical[..count], first, context)
                                || !OriginalWord(source, result, tile + outputX + 1, y, weights.Slice((outputX + 1) * 10, counts[outputX + 1]),
                                    offsets.Slice((outputX + 1) * 10, counts[outputX + 1]), sourceFirst, vertical[..count], first, context)) return false;
                        }
                    }
                    for (var x = outputX; x < length; x++)
                        if (!OriginalWord(source, result, tile + x, y, weights.Slice(x * 10, counts[x]),
                            offsets.Slice(x * 10, counts[x]), sourceFirst, vertical[..count], first, context)) return false;
                }
            }
            context.Cancellation.ThrowIfCancellationRequested();
            return true;
        }
        finally { context.Release(WorkspaceBytes); }
    }

    private static bool OriginalWord(ImageBuffer source, ImageBuffer result, int x, int y,
        ReadOnlySpan<double> weights, ReadOnlySpan<byte> offsets, int sourceFirst,
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
