using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Eight-column strips reuse filtered rows while reducing width and enlarging height.</summary>
internal static class CrossWordColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, long rowWork, OperationContext context)
    {
        if (source.BitDepth != 16 || result.Width >= source.Width || result.Height <= source.Height
            || tapsX > 10 || tapsY > 5
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // Fixed arrays occupy 3,440 bytes within the admitted charge. This
            // frame unwinds before direct replay can need tiny-alpha scratch.
            Span<Vector256<double>> cache = stackalloc Vector256<double>[5 * 8];
            Span<int> rowIds = stackalloc int[5];
            Span<Vector256<double>> decoded = stackalloc Vector256<double>[32];
            Span<double> weights = stackalloc double[8 * 10];
            Span<int> offsets = stackalloc int[8 * 10];
            Span<int> counts = stackalloc int[8];
            Span<double> normalizers = stackalloc double[8];
            Span<double> vertical = stackalloc double[5];
            Span<int> rows = stackalloc int[5];
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
                rowIds.Fill(-1);
                for (var y = 0; y < result.Height; y++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var center = (y + .5) * source.Height / result.Height - .5;
                    var first = (int)Math.Ceiling(center - 2 * scaleY);
                    var count = (int)Math.Floor(center + 2 * scaleY) - first + 1;
                    double normalizerY = 0;
                    for (var t = 0; t < count; t++)
                    {
                        var wy = ImageTransforms.Kernel((first + t - center) / scaleY, ResizeFilter.Bicubic);
                        vertical[t] = wy; normalizerY += wy;
                        var sy = Math.Clamp(first + t, 0, source.Height - 1);
                        var slot = sy % 5; rows[t] = slot * 8;
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
                        if (!TryWritePair(sum, n0, n1, destination.Slice((tile + outputX) * 8, 16)))
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
                        if (!TryWriteSingle(sum, normalizer, destination.Slice((tile + x) * 8, 8)))
                        {
                            if (!(normalizer >= .5)
                                || !OriginalWord(source, result, tile + x, y, weights.Slice(x * 10, counts[x]),
                                    offsets.Slice(x * 10, counts[x]), sourceFirst, vertical[..count], first, context)) return false;
                        }
                    }
                }
            }
            // Admission bounds complete original tap cost. Failed speculative
            // strips leave the counter untouched for original-order replay.
            context.Work(rowWork * result.Height);
            return true;
        }
        finally { context.Release(WorkspaceBytes); }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
        const double maximum = 65535d;
        const double alphaError = unitError * maximum;
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 > alphaError && a1 > alphaError)) return false;
        var divisors = Vector512.Create(a0, a0, a0, n0, a1, a1, a1, n1);
        var values = Avx512F.Divide(sum, divisors);
        // Established 2^-40*16^2 word bounds cover every in-range
        // rounding threshold: alpha margin 1e-4, color below 2.0001/alpha.
        // Alpha's lower power of two gives a conservative reciprocal bound.
        var exponent0 = (int)((BitConverter.DoubleToUInt64Bits(a0) >> 52) & 2047);
        var exponent1 = (int)((BitConverter.DoubleToUInt64Bits(a1) >> 52) & 2047);
        var margin0 = 2.0001 * BitConverter.UInt64BitsToDouble((ulong)(2046 - exponent0) << 52);
        var margin1 = 2.0001 * BitConverter.UInt64BitsToDouble((ulong)(2046 - exponent1) << 52);
        if (margin0 >= .25 || margin1 >= .25
            || !(values.GetElement(3) >= 1 + 1e-4 && values.GetElement(7) >= 1 + 1e-4)) return false;
        var margins = Vector512.Create(margin0, margin0, margin0, 1e-4, margin1, margin1, margin1, 1e-4);
        var fractional = Avx512F.Subtract(values, Avx512F.RoundScale(values, 1));
        var distance = Avx512F.AndNot(Vector512.Create(long.MinValue),
            Avx512F.Subtract(fractional, Vector512.Create(.5)).AsInt64()).AsDouble();
        var inRange = Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values, Vector512<double>.Zero).AsInt64(),
            Avx512F.CompareLessThanOrEqual(values, Vector512.Create(maximum)).AsInt64());
        var ambiguous = Avx512F.And(inRange, Avx512F.CompareLessThanOrEqual(distance, margins).AsInt64());
        if (Vector512.ExtractMostSignificantBits(ambiguous) != 0) return false;
        var clipped = Avx512F.Min(Vector512.Create(maximum), Avx512F.Max(Vector512<double>.Zero, values));
        var ints = Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped, 0));
        var words = Sse41.PackUnsignedSaturate(ints.GetLower(), ints.GetUpper());
        MemoryMarshal.Write(output, in words);
        return true;
    }

    private static bool TryWriteSingle(Vector256<double> sum, double normalizer, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
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
