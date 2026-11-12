using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Bounded column strips reuse source rows within an admitted stack workspace.</summary>
internal static partial class WideColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    private readonly struct FourColumns { }
    private readonly struct ThreeColumns { }
    private readonly struct RowLoanColumns { }
    private readonly struct TailColumns { }
    private readonly struct ByteSamples { }
    private readonly struct WordSamples { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth is not (8 or 16) || result.Width < 2 || result.Width >= source.Width || result.Height >= source.Height
            || tapsX > 20 || tapsY > 20 || tapsX <= 10 && tapsY <= 10
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            var four = scaleX <= 4.125 && scaleY <= 4.125;
            if (source.BitDepth == 16)
                return four
                    ? ApplyRange<FourColumns, WordSamples>(source, result, scaleX, scaleY, context)
                    : ApplyRange<ThreeColumns, WordSamples>(source, result, scaleX, scaleY, context);
            if (four && TryLocateRowLoan(result, out _, out _))
            {
                if (!ApplyRange<RowLoanColumns, ByteSamples>(source, result, scaleX, scaleY, context)) return false;
                // The cache/weight frame has returned before metadata restore
                // or publication can reuse any of its borrowed result rows.
                return ApplyRange<TailColumns, ByteSamples>(source, result, scaleX, scaleY, context);
            }
            return four
                ? ApplyRange<FourColumns, ByteSamples>(source, result, scaleX, scaleY, context)
                : ApplyRange<ThreeColumns, ByteSamples>(source, result, scaleX, scaleY, context);
        }
        finally { context.Release(WorkspaceBytes); }
    }

    internal static bool TryLocateRowLoan(ImageBuffer result, out int firstTailRow, out int start)
    {
        firstTailRow = start = 0;
        if (result.BitDepth != 8 || result.Width < 16) return false;
        const int bytes = 17 * 16 * 32 + 16 * 17 * sizeof(double);
        var rows = (int)((bytes + (long)result.Stride - 1) / result.Stride);
        var first = result.Height - rows;
        if (first < 17) return false;
        firstTailRow = first; start = checked(first * result.Stride);
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ApplyRange<TWidth, TSamples>(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, OperationContext context) where TWidth : struct where TSamples : struct
    {
        // Closed modes share one schedule. The row loan retains3,296B of
        // explicit stack payload; original modes retain4,068/3,776B.
        var columns = typeof(TWidth) == typeof(RowLoanColumns) ? 16 : typeof(TWidth) == typeof(ThreeColumns) ? 3 : 4;
        var taps = typeof(TWidth) == typeof(ThreeColumns) ? 20 : 17;
        var firstTailRow = 0; var loanStart = 0;
        if (typeof(TWidth) == typeof(RowLoanColumns) || typeof(TWidth) == typeof(TailColumns))
            if (!TryLocateRowLoan(result, out firstTailRow, out loanStart)) return false;
        var word = typeof(TSamples) == typeof(WordSamples);
        var pixelBytes = word ? 8 : 4;
        Span<Vector256<double>> cache = typeof(TWidth) == typeof(RowLoanColumns)
            ? MemoryMarshal.Cast<byte, Vector256<double>>(result.Pixels.Slice(loanStart, 8704))
            : stackalloc Vector256<double>[taps * columns];
        Span<int> rowIds = stackalloc int[taps];
        Span<Vector256<double>> decoded = stackalloc Vector256<double>[typeof(TWidth) == typeof(RowLoanColumns) ? 80 : 30];
        Span<double> weights = typeof(TWidth) == typeof(RowLoanColumns)
            ? MemoryMarshal.Cast<byte, double>(result.Pixels.Slice(loanStart + 8704, 2176))
            : stackalloc double[columns * taps];
        Span<byte> offsets = stackalloc byte[columns * taps];
        Span<int> counts = stackalloc int[columns];
        Span<double> normalizers = stackalloc double[columns];
        Span<double> vertical = stackalloc double[taps];
        Span<int> rows = stackalloc int[taps];
        var input = source.Pixels;
        var stride = source.Stride;
        ColumnBicubicGeometry geometry;
        if (typeof(TWidth) == typeof(TailColumns))
            geometry = ColumnBicubicGeometry.PrepareTail(result, source.Height, scaleY, taps, 4, firstTailRow, context.Cancellation);
        else
            geometry = ColumnBicubicGeometry.Prepare(result, source.Height, scaleY, taps,
                typeof(TWidth) == typeof(RowLoanColumns) ? 4 : columns, context.Cancellation);
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
                    var w = ImageTransforms.BicubicWeight(Math.Abs((first + t - center) / scaleX));
                    weights[x * taps + t] = w; normalizer += w;
                    var relative = Math.Clamp(first + t, 0, source.Width - 1) - sourceFirst;
                    if ((uint)relative >= (uint)decoded.Length) return false;
                    offsets[x * taps + t] = (byte)relative;
                }
                normalizers[x] = normalizer;
            }
            var sourceCount = offsets[(length - 1) * taps + counts[length - 1] - 1] + 1;
            if (taps == 17)
                BicubicMath.PrepareOverlappingPairs(offsets, counts, length, sourceCount, 17, 16, 4, context.Cancellation);
            rowIds.Fill(-1);
            for (var y = typeof(TWidth) == typeof(TailColumns) ? firstTailRow : 0;
                y < (typeof(TWidth) == typeof(RowLoanColumns) ? firstTailRow : result.Height); y++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                geometry.Read(y, tile < geometry.PrefixColumns, vertical,
                    out var first, out var count, out var normalizerY);
                if ((uint)(count - 1) >= (uint)taps) return false;
                for (var t = 0; t < count; t++)
                {
                    var sy = Math.Clamp(first + t, 0, source.Height - 1);
                    var slot = sy % taps; rows[t] = slot * columns;
                    if (rowIds[slot] == sy) continue;
                    rowIds[slot] = sy;
                    var row = input.Slice(sy * stride + sourceFirst * pixelBytes, sourceCount * pixelBytes);
                    var decodedBytes = MemoryMarshal.AsBytes(decoded[..sourceCount]);
                    if (word)
                    {
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
                    }
                    else
                    {
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
                    }
                    for (var x = 0; x + 1 < length; x += 2)
                    {
                        var count0 = counts[x]; var count1 = counts[x + 1];
                        var weights0 = weights.Slice(x * taps, count0); var weights1 = weights.Slice((x + 1) * taps, count1);
                        var offsets0 = offsets.Slice(x * taps, count0); var offsets1 = offsets.Slice((x + 1) * taps, count1);
                        if (typeof(TWidth) == typeof(RowLoanColumns) && count0 == 16 && count1 == 16 && offsets[x * taps + 16] != 0)
                        {
                            var firstPixel = offsets[x * taps + 16] - 1;
                            BicubicMath.FilterContiguousHorizontalPair(
                                decoded.Slice(firstPixel, 20), weights0, weights1, 4, out var firstSum, out var secondSum);
                            cache[slot * columns + x] = firstSum;
                            cache[slot * columns + x + 1] = secondSum;
                            continue;
                        }
                        if (columns == 4 && count0 == 16 && count1 == 16 && offsets[x * taps + 16] != 0)
                        {
                            var firstPixel = offsets[x * taps + 16] - 1;
                            var pair = BicubicMath.FilterOverlappingHorizontalPair(
                                decoded.Slice(firstPixel, 20), weights0, weights1, 16, 4);
                            cache[slot * columns + x] = pair.GetLower();
                            cache[slot * columns + x + 1] = pair.GetUpper();
                            continue;
                        }
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
                var destination = word ? result.Pixels.Slice(y * result.Stride, result.Stride) : result.GetRow8(y);
                var outputX = 0;
                // Share four-pixel traversal, then publish each pair in its original order.
                var pending = Vector512<double>.Zero;
                for (; outputX + 1 < length; outputX += 2)
                {
                    Vector512<double> sum;
                    if ((outputX & 2) != 0)
                        sum = pending;
                    else if (outputX + 3 < length)
                        BicubicMath.FilterVerticalQuartet(cache, vertical[..count], rows, outputX, out sum, out pending);
                    else
                    {
                        sum = Vector512<double>.Zero;
                        for (var t = 0; t < count; t++)
                        {
                            var pair = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cache.Slice(rows[t] + outputX, 2)));
                            sum = Avx512F.Add(sum, Avx512F.Multiply(pair, Vector512.Create(vertical[t])));
                        }
                    }
                    if (word)
                    {
                        var n0 = normalizers[outputX] * normalizerY;
                        var n1 = normalizers[outputX + 1] * normalizerY;
                        if (!TryWriteWordPair(sum, n0, n1, destination.Slice((tile + outputX) * 8, 16)))
                        {
                            if (!(n0 >= .5 && n1 >= .5 && sum.GetElement(3) / n0 >= 2 && sum.GetElement(7) / n1 >= 2)
                                || !OriginalWord(source, result, tile + outputX, y, weights.Slice(outputX * taps, counts[outputX]),
                                    offsets.Slice(outputX * taps, counts[outputX]), sourceFirst, vertical[..count], first, context)
                                || !OriginalWord(source, result, tile + outputX + 1, y, weights.Slice((outputX + 1) * taps, counts[outputX + 1]),
                                    offsets.Slice((outputX + 1) * taps, counts[outputX + 1]), sourceFirst, vertical[..count], first, context)) return false;
                        }
                    }
                    else if (typeof(TSamples) == typeof(ByteSamples) && !TryWritePair(sum, normalizers[outputX] * normalizerY, normalizers[outputX + 1] * normalizerY,
                        destination.Slice((tile + outputX) * 4, 8)))
                    {
                        for (var repair = outputX; repair < outputX + 2; repair++)
                            if (!BicubicMath.TryWriteOriginalByte(source, result, tile + repair, y,
                                weights.Slice(repair * taps, counts[repair]), offsets.Slice(repair * taps, counts[repair]),
                                sourceFirst, vertical[..count], first, context)) return false;
                    }
                }
                for (var x = outputX; x < length; x++)
                {
                    var sum = Vector256<double>.Zero;
                    for (var t = 0; t < count; t++)
                        sum = Avx.Add(sum, Avx.Multiply(cache[rows[t] + x], Vector256.Create(vertical[t])));
                    var normalizer = normalizers[x] * normalizerY;
                    if (word)
                    {
                        if (!TryWriteWordSingle(sum, normalizer, destination.Slice((tile + x) * 8, 8)))
                        {
                            if (!(normalizer >= .5 && sum.GetElement(3) / normalizer >= 2)
                                || !OriginalWord(source, result, tile + x, y, weights.Slice(x * taps, counts[x]),
                                    offsets.Slice(x * taps, counts[x]), sourceFirst, vertical[..count], first, context)) return false;
                        }
                        continue;
                    }
                    if (!TryWriteSingle(sum, normalizer, destination.Slice((tile + x) * 4, 4))
                        && !BicubicMath.TryWriteOriginalByte(source, result, tile + x, y,
                            weights.Slice(x * taps, counts[x]), offsets.Slice(x * taps, counts[x]),
                            sourceFirst, vertical[..count], first, context)) return false;
                }
            }
        }
        context.Cancellation.ThrowIfCancellationRequested();
        return true;
    }

    private static bool TryWriteSingle(Vector256<double> sum, double normalizer, Span<byte> output)
    {
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
        BinaryPrimitives.WriteUInt32LittleEndian(output, pixel);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        // Ordinary alpha shares the bounded reciprocal certificate. Keep the
        // dynamic division path for tiny alpha and uncertain rounding cases.
        if (BicubicMath.TryWriteOrdinaryBytePair(sum, n0, n1, output)) return true;
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
        if (!BicubicMath.TryRoundBytePair(values, margins, out var ints)) return false;
        var shorts = Sse2.PackSignedSaturate(ints.GetLower(), ints.GetUpper());
        var packed = Sse2.PackUnsignedSaturate(shorts, Vector128<short>.Zero).AsUInt64().GetElement(0);
        BinaryPrimitives.WriteUInt64LittleEndian(output, packed);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWriteWordPair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double unitError = 3.637978807091713e-10;
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

    private static bool TryWriteWordSingle(Vector256<double> sum, double normalizer, Span<byte> output)
    {
        const double unitError = 3.637978807091713e-10;
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
