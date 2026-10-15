using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Eight-column strips reuse rows for one-axis or two-axis enlargement within an admitted workspace.</summary>
internal static class EnlargingColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    // A completed resize clears RGB under zero alpha, so this temporary marker
    // cannot collide with a completed pixel. The result is private until return.
    private const uint DeferredPixel = 0x00FFFFFF;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, long rowWork, OperationContext context)
    {
        if (!TryApplyCore(source, result, scaleX, scaleY, tapsX, tapsY, rowWork, context, out var deferred)) return false;
        if (deferred) ReplayTiny(source, result, scaleX, scaleY, context);
        return true;
    }

    // Exact tiny-alpha scratch is acquired only after this stack frame returns.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryApplyCore(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, long rowWork, OperationContext context, out bool deferred)
    {
        deferred = false;
        if (source.BitDepth != 8 || result.Width < source.Width || result.Height < source.Height
            || result.Width == source.Width && result.Height == source.Height
            || tapsX > 5 || tapsY > 5
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // Fixed arrays occupy 2,416 bytes within the admitted charge. This
            // frame unwinds before direct replay can need tiny-alpha scratch.
            Span<Vector256<double>> cache = stackalloc Vector256<double>[5 * 8];
            Span<int> rowIds = stackalloc int[5];
            Span<Vector256<double>> decoded = stackalloc Vector256<double>[15];
            Span<double> weights = stackalloc double[8 * 5];
            Span<int> offsets = stackalloc int[8 * 5];
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
                        var row = input.Slice(sy * stride + sourceFirst * 4, sourceCount * 4);
                        var decodedBytes = MemoryMarshal.AsBytes(decoded[..sourceCount]);
                        var px = 0;
                        for (; px + 1 < sourceCount; px += 2)
                        {
                            var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(px * 4, 8));
                            var samples = Avx2.ConvertToVector256Int32(Vector128.CreateScalar(packed).AsByte());
                            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                            var products = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors));
                            MemoryMarshal.Write(decodedBytes.Slice(px * 32, 64), in products);
                        }
                        if (px < sourceCount)
                        {
                            var packed = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(px * 4, 4));
                            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsByte());
                            var a = (int)(packed >> 24);
                            var products = Avx.ConvertToVector256Double(Sse41.MultiplyLow(samples, Vector128.Create(a, a, a, 1)));
                            MemoryMarshal.Write(decodedBytes.Slice(px * 32, 32), in products);
                        }
                        for (var x = 0; x < length; x++)
                        {
                            var tapWeights = weights.Slice(x * 5, counts[x]);
                            var tapOffsets = offsets.Slice(x * 5, counts[x]);
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
                        if (!TryWritePair(sum, n0, n1, destination.Slice((tile + outputX) * 4, 8)))
                        {
                            if (!(n0 >= .5 && n1 >= .5)
                                || !OriginalByte(source, result, tile + outputX, y, weights.Slice(outputX * 5, counts[outputX]),
                                    offsets.Slice(outputX * 5, counts[outputX]), sourceFirst, vertical[..count], first, context, ref deferred)
                                || !OriginalByte(source, result, tile + outputX + 1, y, weights.Slice((outputX + 1) * 5, counts[outputX + 1]),
                                    offsets.Slice((outputX + 1) * 5, counts[outputX + 1]), sourceFirst, vertical[..count], first, context, ref deferred)) return false;
                        }
                    }
                    for (var x = outputX; x < length; x++)
                    {
                        var sum = Vector256<double>.Zero;
                        for (var t = 0; t < count; t++)
                            sum = Avx.Add(sum, Avx.Multiply(cache[rows[t] + x], Vector256.Create(vertical[t])));
                        var normalizer = normalizers[x] * normalizerY;
                        if (!TryWriteSingle(sum, normalizer, destination.Slice((tile + x) * 4, 4)))
                        {
                            if (!(normalizer >= .5)
                                || !OriginalByte(source, result, tile + x, y, weights.Slice(x * 5, counts[x]),
                                    offsets.Slice(x * 5, counts[x]), sourceFirst, vertical[..count], first, context, ref deferred)) return false;
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
        const double maximum = 255d;
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
        var shorts = Sse2.PackSignedSaturate(ints.GetLower(), ints.GetUpper());
        var packed = Sse2.PackUnsignedSaturate(shorts, Vector128<short>.Zero).AsUInt64().GetElement(0);
        BinaryPrimitives.WriteUInt64LittleEndian(output, packed);
        return true;
    }

    private static bool TryWriteSingle(Vector256<double> sum, double normalizer, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
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
        var packed = Sse2.PackUnsignedSaturate(shorts, Vector128<short>.Zero).AsUInt32().GetElement(0);
        BinaryPrimitives.WriteUInt32LittleEndian(output, packed);
        return true;
    }

    private static bool OriginalByte(ImageBuffer source, ImageBuffer result, int x, int y,
        ReadOnlySpan<double> weights, ReadOnlySpan<int> offsets, int sourceFirst,
        ReadOnlySpan<double> vertical, int firstY, OperationContext context, ref bool deferred)
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
        // Exact tiny-alpha arithmetic needs the complete 4-KiB allowance.
        // Decline while this frame is live; the caller replays after it unwinds.
        if (normalized > 0 && normalized < 1)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(result.GetRow8(y).Slice(x * 4, 4), DeferredPixel);
            deferred = true;
            return true;
        }
        var aOut = RoundByte(normalized);
        var color = aOut > 0 && alphaSum > 0;
        var destination = result.GetRow8(y).Slice(x * 4, 4);
        destination[0] = RoundByte(color ? red / alphaSum : 0);
        destination[1] = RoundByte(color ? green / alphaSum : 0);
        destination[2] = RoundByte(color ? blue / alphaSum : 0);
        destination[3] = aOut;
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
                pixel[0] = RoundByte(color ? red / alphaSum : 0);
                pixel[1] = RoundByte(color ? green / alphaSum : 0);
                pixel[2] = RoundByte(color ? blue / alphaSum : 0);
                pixel[3] = (byte)rounded;
            }
        }
    }

    private static byte RoundByte(double value)
        => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);

}
