using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Bounded column strips reuse source rows within an admitted stack workspace.</summary>
internal static class WideColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, long rowWork, OperationContext context)
    {
        if (source.BitDepth != 8 || result.Width < 2 || result.Width >= source.Width || result.Height >= source.Height
            || tapsX > 20 || tapsY > 20 || tapsX <= 10 && tapsY <= 10
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            // At scale <=4.125, support length <=16.5 contains at most17 samples.
            // Four adjacent columns cover <=28.875 source intervals. Relative
            // byte offsets let four columns fit in4,068 of the admitted4,096B.
            // Larger supports retain three columns. Actual bounds are checked
            // before writes; this frame unwinds before original-order replay.
            var columns = scaleX <= 4.125 && scaleY <= 4.125 ? 4 : 3;
            var taps = columns == 4 ? 17 : 20;
            Span<Vector256<double>> cache = stackalloc Vector256<double>[taps * columns];
            Span<int> rowIds = stackalloc int[taps];
            Span<Vector256<double>> decoded = stackalloc Vector256<double>[30];
            Span<double> weights = stackalloc double[columns * taps];
            Span<byte> offsets = stackalloc byte[columns * taps];
            Span<int> counts = stackalloc int[columns];
            Span<double> normalizers = stackalloc double[columns];
            Span<double> vertical = stackalloc double[taps];
            Span<int> rows = stackalloc int[taps];
            var input = source.Pixels;
            var stride = source.Stride;
            for (var tile = 0; tile < result.Width; tile += columns)
            {
                var length = Math.Min(columns, result.Width - tile);
                var firstCenter = (tile + .5) * source.Width / result.Width - .5;
                var sourceFirst = Math.Clamp((int)Math.Ceiling(firstCenter - 2 * scaleX), 0, source.Width - 1);
                for (var x = 0; x < length; x++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var center = (tile + x + .5) * source.Width / result.Width - .5;
                    var first = (int)Math.Ceiling(center - 2 * scaleX);
                    var count = (int)Math.Floor(center + 2 * scaleX) - first + 1;
                    if ((uint)(count - 1) >= (uint)taps) return false;
                    counts[x] = count;
                    double normalizer = 0;
                    for (var t = 0; t < count; t++)
                    {
                        var w = ImageTransforms.Kernel((first + t - center) / scaleX, ResizeFilter.Bicubic);
                        weights[x * taps + t] = w; normalizer += w;
                        var relative = Math.Clamp(first + t, 0, source.Width - 1) - sourceFirst;
                        if ((uint)relative >= (uint)decoded.Length) return false;
                        offsets[x * taps + t] = (byte)relative;
                    }
                    normalizers[x] = normalizer;
                }
                var sourceCount = offsets[(length - 1) * taps + counts[length - 1] - 1] + 1;
                rowIds.Fill(-1);
                for (var y = 0; y < result.Height; y++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var center = (y + .5) * source.Height / result.Height - .5;
                    var first = (int)Math.Ceiling(center - 2 * scaleY);
                    var count = (int)Math.Floor(center + 2 * scaleY) - first + 1;
                    if ((uint)(count - 1) >= (uint)taps) return false;
                    double normalizerY = 0;
                    for (var t = 0; t < count; t++)
                    {
                        var wy = ImageTransforms.Kernel((first + t - center) / scaleY, ResizeFilter.Bicubic);
                        vertical[t] = wy; normalizerY += wy;
                        var sy = Math.Clamp(first + t, 0, source.Height - 1);
                        var slot = sy % taps; rows[t] = slot * columns;
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
                            var products = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors).AsUInt32());
                            MemoryMarshal.Write(decodedBytes.Slice(px * 32, 64), in products);
                        }
                        if (px < sourceCount)
                        {
                            var packed = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(px * 4, 4));
                            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsByte());
                            var factors = Vector128.Create((int)(packed >> 24), (int)(packed >> 24), (int)(packed >> 24), 1);
                            var products = Avx.ConvertToVector256Double(Sse41.MultiplyLow(samples, factors));
                            MemoryMarshal.Write(decodedBytes.Slice(px * 32, 32), in products);
                        }
                        for (var x = 0; x + 1 < length; x += 2)
                        {
                            var count0 = counts[x]; var count1 = counts[x + 1];
                            var weights0 = weights.Slice(x * taps, count0); var weights1 = weights.Slice((x + 1) * taps, count1);
                            var offsets0 = offsets.Slice(x * taps, count0); var offsets1 = offsets.Slice((x + 1) * taps, count1);
                            var sum0 = Vector256<double>.Zero; var sum1 = Vector256<double>.Zero;
                            var shared = Math.Min(count0, count1);
                            for (var tx = 0; tx < shared; tx++)
                            {
                                sum0 = Avx.Add(sum0, Avx.Multiply(decoded[offsets0[tx]], Vector256.Create(weights0[tx])));
                                sum1 = Avx.Add(sum1, Avx.Multiply(decoded[offsets1[tx]], Vector256.Create(weights1[tx])));
                            }
                            for (var tx = shared; tx < count0; tx++)
                                sum0 = Avx.Add(sum0, Avx.Multiply(decoded[offsets0[tx]], Vector256.Create(weights0[tx])));
                            for (var tx = shared; tx < count1; tx++)
                                sum1 = Avx.Add(sum1, Avx.Multiply(decoded[offsets1[tx]], Vector256.Create(weights1[tx])));
                            cache[slot * columns + x] = sum0; cache[slot * columns + x + 1] = sum1;
                        }
                        if ((length & 1) != 0)
                        {
                            var x = length - 1;
                            var tapWeights = weights.Slice(x * taps, counts[x]);
                            var tapOffsets = offsets.Slice(x * taps, counts[x]);
                            var sum = Vector256<double>.Zero;
                            for (var tx = 0; tx < tapWeights.Length; tx++)
                                sum = Avx.Add(sum, Avx.Multiply(decoded[tapOffsets[tx]], Vector256.Create(tapWeights[tx])));
                            cache[slot * columns + x] = sum;
                        }
                    }
                    var destination = result.GetRow8(y);
                    var outputX = 0;
                    for (; outputX + 1 < length; outputX += 2)
                    {
                        var sum = Vector512<double>.Zero;
                        for (var t = 0; t < count; t++)
                        {
                            var pair = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cache.Slice(rows[t] + outputX, 2)));
                            sum = Avx512F.Add(sum, Avx512F.Multiply(pair, Vector512.Create(vertical[t])));
                        }
                        if (!TryWritePair(sum, normalizers[outputX] * normalizerY, normalizers[outputX + 1] * normalizerY,
                            destination.Slice((tile + outputX) * 4, 8))) return false;
                    }
                    for (var x = outputX; x < length; x++)
                    {
                        var sum = Vector256<double>.Zero;
                        for (var t = 0; t < count; t++)
                            sum = Avx.Add(sum, Avx.Multiply(cache[rows[t] + x], Vector256.Create(vertical[t])));
                        var normalizer = normalizers[x] * normalizerY;
                        var alpha = sum.GetElement(3);
                        // Each coefficient is bounded by one, with at most 400
                        // two-axis terms. The existing 2^-40 * 20^2 absolute-error
                        // budget covers both orders and normalization sums.
                        const double unitError = 3.637978807091713e-10;
                        const double alphaError = unitError * 255;
                        if (!(normalizer >= .5 && alpha > alphaError)) return false;
                        var values = Avx.Divide(sum, Vector256.Create(alpha, alpha, alpha, normalizer));
                        var magnitude = Math.Max(Math.Abs(values.GetElement(0)),
                            Math.Max(Math.Abs(values.GetElement(1)), Math.Abs(values.GetElement(2))));
                        var colorMargin = 2 * (alphaError * 255 + magnitude * alphaError) / (alpha - alphaError);
                        var alphaMargin = 2 * (alphaError + Math.Abs(values.GetElement(3)) * unitError) / (normalizer - unitError);
                        if (colorMargin >= .25 || !(values.GetElement(3) >= 1 + alphaMargin)) return false;
                        for (var c = 0; c < 4; c++)
                        {
                            var value = values.GetElement(c);
                            if (value >= 0 && value <= 255
                                && Math.Abs(value - Math.Floor(value) - .5) <= (c == 3 ? alphaMargin : colorMargin)) return false;
                        }
                        var clipped = Avx.Min(Vector256.Create(255d), Avx.Max(Vector256<double>.Zero, values));
                        var ints = Avx.ConvertToVector128Int32WithTruncation(Avx.RoundToNearestInteger(clipped));
                        var shorts = Sse2.PackSignedSaturate(ints, Vector128<int>.Zero);
                        var pixel = Sse2.PackUnsignedSaturate(shorts, Vector128<short>.Zero).AsUInt32().GetElement(0);
                        BinaryPrimitives.WriteUInt32LittleEndian(destination.Slice((tile + x) * 4, 4), pixel);
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
        const double unitError = 3.637978807091713e-10;
        const double alphaError = unitError * 255;
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 > alphaError && a1 > alphaError)) return false;
        var divisors = Vector512.Create(a0, a0, a0, n0, a1, a1, a1, n1);
        var values = Avx512F.Divide(sum, divisors);
        var magnitudes = Avx512F.AndNot(Vector512.Create(long.MinValue), values.AsInt64()).AsDouble();
        // Per-channel quotient bounds avoid scalar maximum/division chains.
        // Doubling covers rounding of the positive error-bound calculation.
        var errors = Vector512.Create(alphaError, alphaError, alphaError, unitError,
            alphaError, alphaError, alphaError, unitError);
        var numerators = Avx512F.Add(Vector512.Create(alphaError * 255, alphaError * 255, alphaError * 255, alphaError,
            alphaError * 255, alphaError * 255, alphaError * 255, alphaError), Avx512F.Multiply(magnitudes, errors));
        var margins = Avx512F.Divide(Avx512F.Multiply(numerators, Vector512.Create(2d)), Avx512F.Subtract(divisors, errors));
        if (Vector512.ExtractMostSignificantBits(Avx512F.CompareGreaterThanOrEqual(margins, Vector512.Create(.25))) != 0
            || !(values.GetElement(3) >= 1 + margins.GetElement(3) && values.GetElement(7) >= 1 + margins.GetElement(7))) return false;
        var fractional = Avx512F.Subtract(values, Avx512F.RoundScale(values, 1));
        var distance = Avx512F.AndNot(Vector512.Create(long.MinValue),
            Avx512F.Subtract(fractional, Vector512.Create(.5)).AsInt64()).AsDouble();
        var inRange = Avx512F.And(Avx512F.CompareGreaterThanOrEqual(values, Vector512<double>.Zero).AsInt64(),
            Avx512F.CompareLessThanOrEqual(values, Vector512.Create(255d)).AsInt64());
        var ambiguous = Avx512F.And(inRange, Avx512F.CompareLessThanOrEqual(distance, margins).AsInt64());
        if (Vector512.ExtractMostSignificantBits(ambiguous) != 0) return false;
        var clipped = Avx512F.Min(Vector512.Create(255d), Avx512F.Max(Vector512<double>.Zero, values));
        var ints = Avx512F.ConvertToVector256Int32WithTruncation(Avx512F.RoundScale(clipped, 0));
        var shorts = Sse2.PackSignedSaturate(ints.GetLower(), ints.GetUpper());
        var packed = Sse2.PackUnsignedSaturate(shorts, Vector128<short>.Zero).AsUInt64().GetElement(0);
        BinaryPrimitives.WriteUInt64LittleEndian(output, packed);
        return true;
    }

}
