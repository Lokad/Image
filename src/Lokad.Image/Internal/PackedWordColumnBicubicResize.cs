using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Two-column strips reuse source rows within an admitted stack workspace.</summary>
internal static class PackedWordColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, long rowWork, OperationContext context)
    {
        if (source.BitDepth != 16 || result.Width < 2 || result.Width >= source.Width || result.Height >= source.Height
            || tapsX > 34 || tapsY > 34 || tapsX <= 20 && tapsY <= 20
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // Fixed arrays occupy 4,044 bytes within the admitted charge. This
            // frame unwinds before direct replay can need tiny-alpha scratch.
            Span<Vector256<double>> cache = stackalloc Vector256<double>[34 * 2];
            Span<int> rowIds = stackalloc int[34];
            Span<uint> decoded = stackalloc uint[43 * 4];
            Span<double> weights = stackalloc double[2 * 34];
            Span<ushort> offsets = stackalloc ushort[2 * 34];
            Span<int> counts = stackalloc int[2];
            Span<double> normalizers = stackalloc double[2];
            Span<double> vertical = stackalloc double[34];
            Span<ushort> rows = stackalloc ushort[34];
            var input = source.Pixels;
            var stride = source.Stride;
            for (var tile = 0; tile < result.Width; tile += 2)
            {
                var length = Math.Min(2, result.Width - tile);
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
                        weights[x * 34 + t] = w; normalizer += w;
                        var relative = Math.Clamp(first + t, 0, source.Width - 1) - sourceFirst;
                        if ((uint)relative >= 43) return false;
                        offsets[x * 34 + t] = (ushort)relative;
                    }
                    normalizers[x] = normalizer;
                }
                var sourceCount = offsets[(length - 1) * 34 + counts[length - 1] - 1] + 1;
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
                        var slot = sy % 34; rows[t] = (ushort)(slot * 2);
                        if (rowIds[slot] == sy) continue;
                        rowIds[slot] = sy;
                        var row = input.Slice(sy * stride + sourceFirst * 8, sourceCount * 8);
                        var decodedBytes = MemoryMarshal.AsBytes(decoded[..(sourceCount * 4)]);
                        var px = 0;
                        for (; px + 1 < sourceCount; px += 2)
                        {
                            var samples = Avx2.ConvertToVector256Int32(MemoryMarshal.Read<Vector128<ushort>>(row.Slice(px * 8, 16)));
                            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                            // Full-range word products require unsigned UInt32;
                            // their exact maximum is 65535 * 65535.
                            var products = Avx2.MultiplyLow(samples, factors).AsUInt32();
                            MemoryMarshal.Write(decodedBytes.Slice(px * 16, 32), in products);
                        }
                        if (px < sourceCount)
                        {
                            var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(px * 8, 8));
                            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsUInt16());
                            var a = (int)(packed >> 48);
                            var products = Sse41.MultiplyLow(samples, Vector128.Create(a, a, a, 1)).AsUInt32();
                            MemoryMarshal.Write(decodedBytes.Slice(px * 16, 16), in products);
                        }
                        if (length == 2)
                        {
                            var count0 = counts[0]; var count1 = counts[1];
                            var weights0 = weights[..count0]; var weights1 = weights.Slice(34, count1);
                            var offsets0 = offsets[..count0]; var offsets1 = offsets.Slice(34, count1);
                            var sum = Vector512<double>.Zero;
                            var shared = Math.Min(count0, count1);
                            for (var tx = 0; tx < shared; tx++)
                            {
                                var p0 = MemoryMarshal.Read<Vector128<uint>>(decodedBytes.Slice(offsets0[tx] * 16, 16));
                                var p1 = MemoryMarshal.Read<Vector128<uint>>(decodedBytes.Slice(offsets1[tx] * 16, 16));
                                var values = Avx512F.ConvertToVector512Double(Vector256.Create(p0, p1));
                                var w0 = weights0[tx]; var w1 = weights1[tx];
                                var coefficients = Vector512.Create(w0, w0, w0, w0, w1, w1, w1, w1);
                                sum = Avx512F.Add(sum, Avx512F.Multiply(values, coefficients));
                            }
                            var sum0 = sum.GetLower(); var sum1 = sum.GetUpper();
                            for (var tx = shared; tx < count0; tx++)
                                sum0 = Avx.Add(sum0, Avx.Multiply(ReadDecoded(decodedBytes, offsets0[tx]), Vector256.Create(weights0[tx])));
                            for (var tx = shared; tx < count1; tx++)
                                sum1 = Avx.Add(sum1, Avx.Multiply(ReadDecoded(decodedBytes, offsets1[tx]), Vector256.Create(weights1[tx])));
                            cache[slot * 2] = sum0; cache[slot * 2 + 1] = sum1;
                        }
                        else
                        {
                            var tapWeights = weights[..counts[0]]; var tapOffsets = offsets[..counts[0]];
                            var sum = Vector256<double>.Zero;
                            for (var tx = 0; tx < tapWeights.Length; tx++)
                                sum = Avx.Add(sum, Avx.Multiply(ReadDecoded(decodedBytes, tapOffsets[tx]), Vector256.Create(tapWeights[tx])));
                            cache[slot * 2] = sum;
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
                            if (!(n0 >= .5 && n1 >= .5 && sum.GetElement(3) / n0 >= 2 && sum.GetElement(7) / n1 >= 2)
                                || !OriginalWord(source, result, tile + outputX, y, weights.Slice(outputX * 34, counts[outputX]),
                                    offsets.Slice(outputX * 34, counts[outputX]), sourceFirst, vertical[..count], first, context)
                                || !OriginalWord(source, result, tile + outputX + 1, y, weights.Slice((outputX + 1) * 34, counts[outputX + 1]),
                                    offsets.Slice((outputX + 1) * 34, counts[outputX + 1]), sourceFirst, vertical[..count], first, context)) return false;
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
                            if (!(normalizer >= .5 && sum.GetElement(3) / normalizer >= 2)
                                || !OriginalWord(source, result, tile + x, y, weights.Slice(x * 34, counts[x]),
                                    offsets.Slice(x * 34, counts[x]), sourceFirst, vertical[..count], first, context)) return false;
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
    private static Vector256<double> ReadDecoded(ReadOnlySpan<byte> products, int offset)
    {
        var packed = MemoryMarshal.Read<Vector128<uint>>(products.Slice(offset * 16, 16));
        return Avx512F.ConvertToVector512Double(Vector256.Create(packed, Vector128<uint>.Zero)).GetLower();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double unitError = 1.051375875249505e-9;
        const double maximum = 65535d;
        const double alphaError = unitError * maximum;
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 > alphaError && a1 > alphaError)) return false;
        var divisors = Vector512.Create(a0, a0, a0, n0, a1, a1, a1, n1);
        var values = Avx512F.Divide(sum, divisors);
        var magnitudes = Avx512F.AndNot(Vector512.Create(long.MinValue), values.AsInt64()).AsDouble();
        // Per-channel quotient bounds avoid scalar maximum/division chains.
        // Doubling covers rounding of the positive error-bound calculation.
        var errors = Vector512.Create(alphaError, alphaError, alphaError, unitError,
            alphaError, alphaError, alphaError, unitError);
        var numerators = Avx512F.Add(Vector512.Create(alphaError * maximum, alphaError * maximum, alphaError * maximum, alphaError,
            alphaError * maximum, alphaError * maximum, alphaError * maximum, alphaError), Avx512F.Multiply(magnitudes, errors));
        var margins = Avx512F.Divide(Avx512F.Multiply(numerators, Vector512.Create(2d)), Avx512F.Subtract(divisors, errors));
        if (Vector512.ExtractMostSignificantBits(Avx512F.CompareGreaterThanOrEqual(margins, Vector512.Create(.25))) != 0
            || !(values.GetElement(3) >= 1 + margins.GetElement(3) && values.GetElement(7) >= 1 + margins.GetElement(7))) return false;
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
        const double unitError = 1.051375875249505e-9;
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
