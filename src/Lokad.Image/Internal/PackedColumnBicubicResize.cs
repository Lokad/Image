using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Column strips reuse decoded source rows within an admitted stack workspace.</summary>
internal static class PackedColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    private readonly struct DecodedPrefix { }
    private readonly struct PackedSuffix { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth != 8 || result.Width < 2 || result.Width >= source.Width || result.Height >= source.Height
            || tapsX > 36 || tapsY > 36 || tapsX <= 20 && tapsY <= 20
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            var firstColumn = 0;
            if (TryLocateResultCache(result, scaleX, scaleY, out var start, out var prefixColumns))
            {
                var loan = MemoryMarshal.Cast<byte, Vector256<double>>(result.Pixels[start..]);
                if (!ApplyRange<DecodedPrefix>(source, result, scaleX, scaleY, loan,
                    result.Stride / 32, 0, prefixColumns, context)) return false;
                firstColumn = prefixColumns;
            }
            // All prefix/cache users have returned before suffix publication.
            return ApplyRange<PackedSuffix>(source, result, scaleX, scaleY, default,
                0, firstColumn, result.Width, context);
        }
        finally { context.Release(WorkspaceBytes); }
    }

    internal static bool TryLocateResultCache(ImageBuffer result, double scaleX, double scaleY,
        out int start, out int prefixColumns)
    {
        start = prefixColumns = 0;
        if (result.BitDepth != 8 || result.Stride % 32 != 0 || result.Height < 32
            || scaleX > 8.125 || scaleY > 8.125) return false;
        // Four vectors in each of32 unpublished row tails. At scale<=8.125,
        // support length<=32.5 contains at most33 source samples; one cache
        // slot remains on stack. Four adjacent centers cover<=56.875 source
        // intervals, so58 decoded samples suffice. No per-image plans escape.
        var candidate = result.Stride - 128;
        var prefix = candidate / 4 / 4 * 4;
        if (candidate < 0 || prefix < 4) return false;
        start = candidate; prefixColumns = prefix;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Span<T> Take<T>(ref Span<byte> workspace, int count) where T : unmanaged
    {
        var bytes = count * Unsafe.SizeOf<T>();
        var result = MemoryMarshal.Cast<byte, T>(workspace[..bytes]);
        workspace = workspace[bytes..];
        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ApplyRange<TMode>(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, scoped Span<Vector256<double>> loan,
        int loanStride, int firstColumn, int endColumn, OperationContext context) where TMode : struct
    {
        // Prefix and suffix frames are physically separate and retire
        // before precision replay. The prefix borrows32 four-vector slots
        // from unpublished result tails; one slot remains in this frame.
        var prepared = typeof(TMode) == typeof(DecodedPrefix);
        Span<byte> workspace = stackalloc byte[prepared ? 3880 : 3992];
        var columns = prepared ? 4 : 2;
        var taps = prepared ? 33 : 36;
        var sourceCapacity = prepared ? 58 : 46;
        var cache = Take<Vector256<double>>(ref workspace, prepared ? 4 : 36 * 2);
        var decoded = Take<byte>(ref workspace, sourceCapacity * (prepared ? 32 : 8));
        var weights = Take<double>(ref workspace, columns * taps);
        var offsets = Take<ushort>(ref workspace, columns * taps);
        var counts = Take<int>(ref workspace, columns);
        var normalizers = Take<double>(ref workspace, columns);
        var vertical = Take<double>(ref workspace, taps);
        var rows = Take<int>(ref workspace, taps);
        // Keep the odd-sized33-entry integer arrays after all double storage.
        var rowIds = Take<int>(ref workspace, taps);
        var input = source.Pixels;
        var stride = source.Stride;
        for (var tile = firstColumn; tile < endColumn; tile += columns)
        {
            var length = Math.Min(columns, endColumn - tile);
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
                    weights[x * taps + t] = w; normalizer += w;
                    var relative = Math.Clamp(first + t, 0, source.Width - 1) - sourceFirst;
                    if ((uint)relative >= (uint)sourceCapacity) return false;
                    offsets[x * taps + t] = (ushort)relative;
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
                double normalizerY = 0;
                for (var t = 0; t < count; t++)
                {
                    var wy = ImageTransforms.Kernel((first + t - center) / scaleY, ResizeFilter.Bicubic);
                    vertical[t] = wy; normalizerY += wy;
                    var sy = Math.Clamp(first + t, 0, source.Height - 1);
                    var slot = sy % taps;
                    rows[t] = prepared ? slot < 32 ? slot * loanStride : -1 - (slot - 32) * columns : slot * columns;
                    if (rowIds[slot] == sy) continue;
                    rowIds[slot] = sy;
                    var row = input.Slice(sy * stride + sourceFirst * 4, sourceCount * 4);
                    var decodedBytes = decoded[..(sourceCount * (prepared ? 32 : 8))];
                    scoped Span<Vector256<double>> selectedCache = cache;
                    var cacheAt = slot * columns;
                    if (prepared)
                    {
                        if (slot < 32) { selectedCache = loan; cacheAt = slot * loanStride; }
                        else cacheAt = (slot - 32) * columns;
                    }
                    var px = 0;
                    for (; px + 1 < sourceCount; px += 2)
                    {
                        var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(px * 4, 8));
                        var samples = Avx2.ConvertToVector256Int32(Vector128.CreateScalar(packed).AsByte());
                        var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                        // Products<=65025 fit UInt16 and Double exactly;
                        // alpha remains the fourth sample.
                        var products = Avx2.MultiplyLow(samples, factors);
                        if (prepared)
                        {
                            var values = Avx512F.ConvertToVector512Double(products.AsUInt32());
                            MemoryMarshal.Write(decodedBytes.Slice(px * 32, 64), in values);
                        }
                        else
                        {
                            var packedProducts = Sse41.PackUnsignedSaturate(products.GetLower(), products.GetUpper());
                            MemoryMarshal.Write(decodedBytes.Slice(px * 8, 16), in packedProducts);
                        }
                    }
                    if (px < sourceCount)
                    {
                        var packed = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(px * 4, 4));
                        var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsByte());
                        var factors = Vector128.Create((int)(packed >> 24), (int)(packed >> 24), (int)(packed >> 24), 1);
                        var products = Sse41.MultiplyLow(samples, factors);
                        if (prepared)
                        {
                            var values = Avx.ConvertToVector256Double(products);
                            MemoryMarshal.Write(decodedBytes.Slice(px * 32, 32), in values);
                        }
                        else
                        {
                            var packedProducts = Sse41.PackUnsignedSaturate(products, Vector128<int>.Zero).AsUInt64().GetElement(0);
                            BinaryPrimitives.WriteUInt64LittleEndian(decodedBytes.Slice(px * 8, 8), packedProducts);
                        }
                    }
                    for (var column = 0; column + 1 < length; column += 2)
                    {
                        var count0 = counts[column]; var count1 = counts[column + 1];
                        var weights0 = weights.Slice(column * taps, count0); var weights1 = weights.Slice((column + 1) * taps, count1);
                        var offsets0 = offsets.Slice(column * taps, count0); var offsets1 = offsets.Slice((column + 1) * taps, count1);
                        var sum = Vector512<double>.Zero;
                        var shared = Math.Min(count0, count1);
                        for (var tx = 0; tx < shared; tx++)
                        {
                            Vector512<double> values;
                                if (prepared)
                                    // Fill the upper half immediately. The explicit
                                    // insert avoids a temporary512bit construction
                                    // with a loop-carried spill/reload dependency.
                                    values = Avx512F.InsertVector256(ReadDecoded<TMode>(decodedBytes, offsets0[tx]).ToVector512Unsafe(),
                                        ReadDecoded<TMode>(decodedBytes, offsets1[tx]), 1);
                            else
                            {
                                var p0 = BinaryPrimitives.ReadUInt64LittleEndian(decodedBytes.Slice(offsets0[tx] * 8, 8));
                                var p1 = BinaryPrimitives.ReadUInt64LittleEndian(decodedBytes.Slice(offsets1[tx] * 8, 8));
                                var samples = Avx2.ConvertToVector256Int32(Vector128.Create(p0, p1).AsUInt16());
                                values = Avx512F.ConvertToVector512Double(samples);
                            }
                            var w0 = weights0[tx]; var w1 = weights1[tx];
                            var coefficients = Avx512F.InsertVector256(Vector512.Create(w0), Vector256.Create(w1), 1);
                            sum = Avx512F.Add(sum, Avx512F.Multiply(values, coefficients));
                        }
                        var sum0 = sum.GetLower(); var sum1 = sum.GetUpper();
                        for (var tx = shared; tx < count0; tx++)
                            sum0 = Avx.Add(sum0, Avx.Multiply(ReadDecoded<TMode>(decodedBytes, offsets0[tx]), Vector256.Create(weights0[tx])));
                        for (var tx = shared; tx < count1; tx++)
                            sum1 = Avx.Add(sum1, Avx.Multiply(ReadDecoded<TMode>(decodedBytes, offsets1[tx]), Vector256.Create(weights1[tx])));
                        selectedCache[cacheAt + column] = sum0; selectedCache[cacheAt + column + 1] = sum1;
                    }
                    if ((length & 1) != 0)
                    {
                        var column = length - 1;
                        var tapWeights = weights.Slice(column * taps, counts[column]);
                        var tapOffsets = offsets.Slice(column * taps, counts[column]);
                        var sum = Vector256<double>.Zero;
                        for (var tx = 0; tx < tapWeights.Length; tx++)
                            sum = Avx.Add(sum, Avx.Multiply(ReadDecoded<TMode>(decodedBytes, tapOffsets[tx]), Vector256.Create(tapWeights[tx])));
                        selectedCache[cacheAt + column] = sum;
                    }
                }
                var destination = result.GetRow8(y);
                var outputX = 0;
                for (; outputX + 1 < length; outputX += 2)
                {
                    var sum = Vector512<double>.Zero;
                    for (var t = 0; t < count; t++)
                    {
                        var pair = ReadPair<TMode>(cache, loan, rows[t], outputX);
                        sum = Avx512F.Add(sum, Avx512F.Multiply(pair, Vector512.Create(vertical[t])));
                    }
                    if (!TryWritePair(sum, normalizers[outputX] * normalizerY, normalizers[outputX + 1] * normalizerY,
                        destination.Slice((tile + outputX) * 4, 8))) return false;
                }
                for (var x = outputX; x < length; x++)
                {
                    var sum = Vector256<double>.Zero;
                    for (var t = 0; t < count; t++)
                        sum = Avx.Add(sum, Avx.Multiply(ReadCache<TMode>(cache, loan, rows[t], x), Vector256.Create(vertical[t])));
                    var normalizer = normalizers[x] * normalizerY;
                    var alpha = sum.GetElement(3);
                    // Each coefficient is bounded by one, with at most 1296
                    // two-axis terms. The existing 2^-40 * 36^2 absolute-error
                    // budget covers both orders and normalization sums.
                    const double unitError = 1.178705133497715e-9;
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
        context.Cancellation.ThrowIfCancellationRequested();
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<double> ReadPair<TMode>(ReadOnlySpan<Vector256<double>> cache,
        ReadOnlySpan<Vector256<double>> loan, int index, int channel) where TMode : struct
    {
        if (typeof(TMode) == typeof(DecodedPrefix))
        {
            if (index >= 0)
                return MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(loan.Slice(index + channel, 2)));
            index = -index - 1;
        }
        return MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(cache.Slice(index + channel, 2)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<double> ReadCache<TMode>(ReadOnlySpan<Vector256<double>> cache,
        ReadOnlySpan<Vector256<double>> loan, int index, int channel) where TMode : struct
    {
        if (typeof(TMode) == typeof(DecodedPrefix))
        {
            if (index >= 0) return loan[index + channel];
            index = -index - 1;
        }
        return cache[index + channel];
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<double> ReadDecoded<TMode>(ReadOnlySpan<byte> products, int offset) where TMode : struct
    {
        if (typeof(TMode) == typeof(DecodedPrefix))
            return MemoryMarshal.Read<Vector256<double>>(products.Slice(offset * 32, 32));
        var packed = BinaryPrimitives.ReadUInt64LittleEndian(products.Slice(offset * 8, 8));
        return Avx.ConvertToVector256Double(Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsUInt16()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double unitError = 1.178705133497715e-9;
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
