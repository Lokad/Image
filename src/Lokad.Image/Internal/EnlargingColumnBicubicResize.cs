using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Byte decoding and certified publication for the shared enlargement schedules.
internal static class EnlargingColumnBicubicResize
{
    // A completed resize clears RGB under zero alpha, so this temporary marker
    // cannot collide with a completed pixel. The result is private until return.
    private const uint DeferredPixel = 0x00FFFFFF;

    internal static void DecodeRow(ReadOnlySpan<byte> row, Span<Vector256<double>> decoded, OperationContext context)
    {
        var bytes = MemoryMarshal.AsBytes(decoded); var px = 0;
        for (; px + 1 < decoded.Length; px += 2)
        {
            if ((px & 255) == 0) context.Cancellation.ThrowIfCancellationRequested();
            var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(px * 4, 8));
            var samples = Avx2.ConvertToVector256Int32(Vector128.CreateScalar(packed).AsByte());
            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
            var products = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors));
            MemoryMarshal.Write(bytes.Slice(px * 32, 64), in products);
        }
        if (px < decoded.Length)
        {
            var packed = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(px * 4, 4));
            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsByte());
            var a = (int)(packed >> 24);
            var products = Avx.ConvertToVector256Double(Sse41.MultiplyLow(samples, Vector128.Create(a, a, a, 1)));
            MemoryMarshal.Write(bytes.Slice(px * 32, 32), in products);
        }
    }

    internal static bool WriteRow(ImageBuffer source, ImageBuffer result, int tile, int y, int length,
        ReadOnlySpan<Vector256<double>> cache, ReadOnlySpan<double> normalizers, ReadOnlySpan<double> weights,
        ReadOnlySpan<int> offsets, ReadOnlySpan<int> counts, int sourceFirst, ReadOnlySpan<double> vertical,
        ReadOnlySpan<int> rows, int first, double normalizerY, OperationContext context, ref bool deferred)
    {
        var destination = result.GetRow8(y); var outputX = 0;
        for (; outputX + 1 < length; outputX += 2)
        {
            if ((outputX & 255) == 0) context.Cancellation.ThrowIfCancellationRequested();
            var sum = Vector512<double>.Zero;
            for (var t = 0; t < vertical.Length; t++)
            {
                var pair = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cache.Slice(rows[t] + outputX, 2)));
                sum = Avx512F.Add(sum, Avx512F.Multiply(pair, Vector512.Create(vertical[t])));
            }
            var n0 = normalizers[outputX] * normalizerY; var n1 = normalizers[outputX + 1] * normalizerY;
            if (!TryWritePair(sum, n0, n1, destination.Slice((tile + outputX) * 4, 8)))
            {
                if (!(n0 >= .5 && n1 >= .5)
                    || !OriginalByte(source, result, tile + outputX, y, weights.Slice(outputX * 5, counts[outputX]),
                        offsets.Slice(outputX * 5, counts[outputX]), sourceFirst, vertical, first, context, ref deferred)
                    || !OriginalByte(source, result, tile + outputX + 1, y, weights.Slice((outputX + 1) * 5, counts[outputX + 1]),
                        offsets.Slice((outputX + 1) * 5, counts[outputX + 1]), sourceFirst, vertical, first, context, ref deferred)) return false;
            }
        }
        for (var x = outputX; x < length; x++)
        {
            var sum = Vector256<double>.Zero;
            for (var t = 0; t < vertical.Length; t++)
                sum = Avx.Add(sum, Avx.Multiply(cache[rows[t] + x], Vector256.Create(vertical[t])));
            var normalizer = normalizers[x] * normalizerY;
            if (!BicubicMath.TryWriteByte(sum, normalizer, destination.Slice((tile + x) * 4, 4)))
            {
                if (!(normalizer >= .5)
                    || !OriginalByte(source, result, tile + x, y, weights.Slice(x * 5, counts[x]),
                        offsets.Slice(x * 5, counts[x]), sourceFirst, vertical, first, context, ref deferred)) return false;
            }
        }
        return true;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double maximum = 255d;
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 >= 1 && a1 >= 1)) return false;
        var divisors = Vector512.Create(a0, a0, a0, n0, a1, a1, a1, n1);
        var values = Avx512F.Divide(sum, divisors);
        // For clamp(N/D,0,255), the perturbation is bounded by
        // (Nerr + 255*Derr)/(D-Derr), including across saturation boundaries.
        // Existing byte arithmetic errors are (65025u,255u) for RGB and
        // (255u,u) for alpha, u=2^-32. At D>=1 and D>=1/2 respectively,
        // these fixed margins exceed twice that bound plus 2^-28 for rounding.
        const double rgbMargin = 0.00006103515625; // 2^-14
        const double alphaMargin = 0.00000095367431640625; // 2^-20
        var margins = Vector512.Create(rgbMargin, rgbMargin, rgbMargin, alphaMargin,
            rgbMargin, rgbMargin, rgbMargin, alphaMargin);
        if (!(values.GetElement(3) >= 1 + alphaMargin && values.GetElement(7) >= 1 + alphaMargin)) return false;
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
        var aOut = BicubicMath.RoundByte(normalized);
        var color = aOut > 0 && alphaSum > 0;
        var destination = result.GetRow8(y).Slice(x * 4, 4);
        destination[0] = BicubicMath.RoundByte(color ? red / alphaSum : 0);
        destination[1] = BicubicMath.RoundByte(color ? green / alphaSum : 0);
        destination[2] = BicubicMath.RoundByte(color ? blue / alphaSum : 0);
        destination[3] = aOut;
        return true;
    }

    internal static void ReplayTiny(ImageBuffer source, ImageBuffer result, double scaleX, double scaleY, OperationContext context)
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
