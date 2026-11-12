using System.Buffers.Binary;
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

    internal static bool WriteRow<TSamples>(ImageBuffer source, ImageBuffer result, int tile, int y, int length,
        ReadOnlySpan<Vector256<double>> cache, ReadOnlySpan<double> normalizers, ReadOnlySpan<double> weights,
        ReadOnlySpan<int> offsets, ReadOnlySpan<int> counts, int sourceFirst, ReadOnlySpan<double> vertical,
        ReadOnlySpan<int> rows, int first, double normalizerY, OperationContext context, ref bool deferred)
    {
        var tapCapacity = typeof(TSamples) == typeof(EnlargingBicubicResize.CrossByteSamples) ? 10 : 5;
        var destination = result.GetRow8(y); var outputX = 0;
        for (; outputX + 3 < length; outputX += 4)
        {
            if ((outputX & 255) == 0) context.Cancellation.ThrowIfCancellationRequested();
            BicubicMath.FilterVerticalQuartet(cache, vertical, rows, outputX, out var firstSum, out var secondSum);
            var n0 = normalizers[outputX] * normalizerY;
            var n1 = normalizers[outputX + 1] * normalizerY;
            var n2 = normalizers[outputX + 2] * normalizerY;
            var n3 = normalizers[outputX + 3] * normalizerY;
            // Enlargement's <=5 taps fit the shared <=16-tap certificate.
            // Each pair keeps its own acceptance and exact repair path.
            var accepted = BicubicMath.WriteByteQuartet(firstSum, secondSum, n0, n1, n2, n3,
                destination.Slice((tile + outputX) * 4, 16));
            if ((accepted & 1) == 0 && (!(n0 >= .5 && n1 >= .5)
                || !RepairPair<TSamples>(source, result, tile, y, outputX, weights, offsets, counts, sourceFirst,
                    vertical, first, context, ref deferred))) return false;
            if ((accepted & 2) == 0 && (!(n2 >= .5 && n3 >= .5)
                || !RepairPair<TSamples>(source, result, tile, y, outputX + 2, weights, offsets, counts, sourceFirst,
                    vertical, first, context, ref deferred))) return false;
        }
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
            if (!BicubicMath.TryWriteBytePair(sum, n0, n1, destination.Slice((tile + outputX) * 4, 8)))
            {
                if (!(n0 >= .5 && n1 >= .5)
                    || !RepairPair<TSamples>(source, result, tile, y, outputX, weights, offsets, counts, sourceFirst,
                        vertical, first, context, ref deferred)) return false;
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
                    || !OriginalByte(source, result, tile + x, y, weights.Slice(x * tapCapacity, counts[x]),
                        offsets.Slice(x * tapCapacity, counts[x]), sourceFirst, vertical, first, context, ref deferred)) return false;
            }
        }
        return true;
    }
    private static bool RepairPair<TSamples>(ImageBuffer source, ImageBuffer result, int tile, int y, int x,
        ReadOnlySpan<double> weights, ReadOnlySpan<int> offsets, ReadOnlySpan<int> counts, int sourceFirst,
        ReadOnlySpan<double> vertical, int first, OperationContext context, ref bool deferred)
    {
        var tapCapacity = typeof(TSamples) == typeof(EnlargingBicubicResize.CrossByteSamples) ? 10 : 5;
        return OriginalByte(source, result, tile + x, y, weights.Slice(x * tapCapacity, counts[x]),
                offsets.Slice(x * tapCapacity, counts[x]), sourceFirst, vertical, first, context, ref deferred)
            && OriginalByte(source, result, tile + x + 1, y, weights.Slice((x + 1) * tapCapacity, counts[x + 1]),
                offsets.Slice((x + 1) * tapCapacity, counts[x + 1]), sourceFirst, vertical, first, context, ref deferred);
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
