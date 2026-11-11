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

    internal readonly struct DecodedPrefix { }
    internal readonly struct PackedSuffix { }
    internal readonly struct ByteSamples { }
    internal readonly struct WordSamples { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth is not (8 or 16) || result.Width < 2 || result.Width >= source.Width || result.Height >= source.Height
            || tapsX > (source.BitDepth == 16 ? 34 : 36) || tapsY > (source.BitDepth == 16 ? 34 : 36) || tapsX <= 20 && tapsY <= 20
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            return source.BitDepth == 16
                ? Apply<WordSamples>(source, result, scaleX, scaleY, context)
                : Apply<ByteSamples>(source, result, scaleX, scaleY, context);
        }
        finally { context.Release(WorkspaceBytes); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Apply<TSamples>(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, OperationContext context) where TSamples : struct
    {
        var firstColumn = 0;
        if (TryLocateResultCache(result, scaleX, scaleY, out var start, out var prefixColumns))
        {
            var loan = MemoryMarshal.Cast<byte, Vector256<double>>(result.Pixels[start..]);
            if (!ApplyRange<DecodedPrefix, TSamples>(source, result, scaleX, scaleY, loan,
                result.Stride / 32, 0, prefixColumns, context)) return false;
            firstColumn = prefixColumns;
        }
        // All prefix/cache users have returned before suffix publication.
        return ApplyRange<PackedSuffix, TSamples>(source, result, scaleX, scaleY, default,
            0, firstColumn, result.Width, context);
    }

    internal static bool TryLocateResultCache(ImageBuffer result, double scaleX, double scaleY,
        out int start, out int prefixColumns)
    {
        start = prefixColumns = 0;
        if (result.BitDepth is not (8 or 16) || result.Stride % 32 != 0 || result.Height < 32
            || scaleX > 8.125 || scaleY > 8.125) return false;
        // Four vectors in each of32 unpublished row tails. At scale<=8.125,
        // support length<=32.5 contains at most33 source samples; one cache
        // slot remains on stack. Four adjacent centers cover<=56.875 source
        // intervals, so58 decoded samples suffice. No per-image plans escape.
        var candidate = result.Stride - 128;
        var prefix = candidate / (result.BitDepth / 2) / 4 * 4;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector256<ushort> PrepareFourBytePixels(Vector128<byte> pixels)
    {
        var samples = Avx2.ConvertToVector256Int16(pixels).AsUInt16();
        var alpha = Avx2.ShuffleHigh(Avx2.ShuffleLow(samples, 255), 255);
        var factors = Avx2.Blend(alpha, Vector256.Create((ushort)1), 136);
        // Byte color products fit UInt16; the alpha lane keeps its sample.
        return Avx2.MultiplyLow(samples, factors);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ApplyRange<TMode, TSamples>(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, scoped Span<Vector256<double>> loan,
        int loanStride, int firstColumn, int endColumn, OperationContext context) where TMode : struct where TSamples : struct
    {
        // Prefix and suffix frames are physically separate and retire
        // before precision replay. The prefix borrows32 four-vector slots
        // from unpublished result tails; one slot remains in this frame.
        var prepared = typeof(TMode) == typeof(DecodedPrefix);
        var word = typeof(TSamples) == typeof(WordSamples);
        var pixelBytes = word ? 8 : 4;
        Span<byte> workspace = stackalloc byte[prepared ? 3880 : word ? 4044 : 3992];
        var columns = prepared ? 4 : 2;
        var taps = prepared ? 33 : word ? 34 : 36;
        var sourceCapacity = prepared ? 58 : word ? 43 : 46;
        var cache = Take<Vector256<double>>(ref workspace, prepared ? 4 : taps * 2);
        var decoded = Take<byte>(ref workspace, sourceCapacity * (prepared ? 32 : word ? 16 : 8));
        var weights = Take<double>(ref workspace, columns * taps);
        var offsets = Take<ushort>(ref workspace, columns * taps);
        var counts = Take<int>(ref workspace, columns);
        var normalizers = Take<double>(ref workspace, columns);
        var vertical = Take<double>(ref workspace, taps);
        var rows = Take<int>(ref workspace, word && !prepared ? 0 : taps);
        var wordRows = Take<ushort>(ref workspace, word && !prepared ? taps : 0);
        // Keep the odd-sized33-entry integer arrays after all double storage.
        var rowIds = Take<int>(ref workspace, taps);
        ReadOnlySpan<double> horizontalWeights = weights;
        ReadOnlySpan<ushort> horizontalOffsets = offsets;
        ReadOnlySpan<byte> input = source.Pixels;
        ReadOnlySpan<Vector256<double>> cacheRead = cache;
        ReadOnlySpan<Vector256<double>> loanRead = loan;
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
            var contiguousPairs = PrepareContiguousPairs(counts[..length], horizontalOffsets, taps);
            var contiguousQuartet = prepared && IsContiguousQuartet(counts[..length], horizontalOffsets, taps, sourceCount, contiguousPairs);
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
                    if (word && !prepared) wordRows[t] = (ushort)(slot * columns);
                    else rows[t] = prepared ? slot < 32 ? slot * loanStride : -1 - (slot - 32) * columns : slot * columns;
                    if (rowIds[slot] == sy) continue;
                    rowIds[slot] = sy;
                    var row = input.Slice(sy * stride + sourceFirst * pixelBytes, sourceCount * pixelBytes);
                    var decodedBytes = decoded[..(sourceCount * (prepared ? 32 : word ? 16 : 8))];
                    scoped Span<Vector256<double>> selectedCache = cache;
                    var cacheAt = slot * columns;
                    if (prepared)
                    {
                        if (slot < 32) { selectedCache = loan; cacheAt = slot * loanStride; }
                        else cacheAt = (slot - 32) * columns;
                    }
                    var px = 0;
                    if (!word)
                    {
                        for (; px + 3 < sourceCount; px += 4)
                        {
                            var products = PrepareFourBytePixels(MemoryMarshal.Read<Vector128<byte>>(row.Slice(px * 4, 16)));
                            if (prepared)
                            {
                                var firstPixels = Avx512F.ConvertToVector512Double(Avx2.ConvertToVector256Int32(products.GetLower()).AsUInt32());
                                var secondPixels = Avx512F.ConvertToVector512Double(Avx2.ConvertToVector256Int32(products.GetUpper()).AsUInt32());
                                MemoryMarshal.Write(decodedBytes.Slice(px * 32, 64), in firstPixels);
                                MemoryMarshal.Write(decodedBytes.Slice((px + 2) * 32, 64), in secondPixels);
                            }
                            else MemoryMarshal.Write(decodedBytes.Slice(px * 8, 32), in products);
                        }
                    }
                    for (; px + 1 < sourceCount; px += 2)
                    {
                        var samples = word
                            ? Avx2.ConvertToVector256Int32(MemoryMarshal.Read<Vector128<ushort>>(row.Slice(px * 8, 16)))
                            : Avx2.ConvertToVector256Int32(Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(px * 4, 8))).AsByte());
                        var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                        // UInt16 products fit UInt32, including values above
                        // Int32.MaxValue. Both depths convert exactly to Double.
                        var products = Avx2.MultiplyLow(samples, factors);
                        if (prepared)
                        {
                            var values = Avx512F.ConvertToVector512Double(products.AsUInt32());
                            MemoryMarshal.Write(decodedBytes.Slice(px * 32, 64), in values);
                        }
                        else if (word) MemoryMarshal.Write(decodedBytes.Slice(px * 16, 32), in products);
                        else
                        {
                            var packedProducts = Sse41.PackUnsignedSaturate(products.GetLower(), products.GetUpper());
                            MemoryMarshal.Write(decodedBytes.Slice(px * 8, 16), in packedProducts);
                        }
                    }
                    if (px < sourceCount)
                    {
                        if (word)
                        {
                            var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(px * 8, 8));
                            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsUInt16());
                            var alpha = (int)(packed >> 48);
                            var products = Sse41.MultiplyLow(samples, Vector128.Create(alpha, alpha, alpha, 1)).AsUInt32();
                            if (prepared)
                            {
                                var values = Avx512F.ConvertToVector512Double(Vector256.Create(products, Vector128<uint>.Zero)).GetLower();
                                MemoryMarshal.Write(decodedBytes.Slice(px * 32, 32), in values);
                            }
                            else MemoryMarshal.Write(decodedBytes.Slice(px * 16, 16), in products);
                        }
                        else
                        {
                            var packed = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(px * 4, 4));
                            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsByte());
                            var alpha = (int)(packed >> 24);
                            var products = Sse41.MultiplyLow(samples, Vector128.Create(alpha, alpha, alpha, 1));
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
                    }
                    ReadOnlySpan<byte> decodedRead = decodedBytes;
                    var column = 0;
                    if (contiguousQuartet)
                    {
                        FilterContiguousQuartet(MemoryMarshal.Cast<byte, Vector256<double>>(decodedRead),
                            horizontalWeights, selectedCache.Slice(cacheAt, 4), context);
                        column = 4;
                    }
                    for (; column + 1 < length; column += 2)
                    {
                        var count0 = counts[column]; var count1 = counts[column + 1];
                        var weights0 = horizontalWeights.Slice(column * taps, count0); var weights1 = horizontalWeights.Slice((column + 1) * taps, count1);
                        var offsets0 = horizontalOffsets.Slice(column * taps, count0); var offsets1 = horizontalOffsets.Slice((column + 1) * taps, count1);
                        var sum = Vector512<double>.Zero;
                        var shared = Math.Min(count0, count1);
                        if ((contiguousPairs & (1 << column)) != 0)
                            sum = FilterContiguousPair<TMode, TSamples>(decodedRead, offsets0[0], offsets1[0], weights0, weights1, shared);
                        else
                            sum = FilterIndexedPair<TMode, TSamples>(decodedRead, offsets0, offsets1, weights0, weights1, shared);
                        var sum0 = sum.GetLower(); var sum1 = sum.GetUpper();
                        for (var tx = shared; tx < count0; tx++)
                            sum0 = Avx.Add(sum0, Avx.Multiply(ReadDecoded<TMode, TSamples>(decodedRead, offsets0[tx]), Vector256.Create(weights0[tx])));
                        for (var tx = shared; tx < count1; tx++)
                            sum1 = Avx.Add(sum1, Avx.Multiply(ReadDecoded<TMode, TSamples>(decodedRead, offsets1[tx]), Vector256.Create(weights1[tx])));
                        selectedCache[cacheAt + column] = sum0; selectedCache[cacheAt + column + 1] = sum1;
                    }
                    if ((length & 1) != 0)
                    {
                        column = length - 1;
                        var tapWeights = weights.Slice(column * taps, counts[column]);
                        var tapOffsets = offsets.Slice(column * taps, counts[column]);
                        var sum = Vector256<double>.Zero;
                        for (var tx = 0; tx < tapWeights.Length; tx++)
                            sum = Avx.Add(sum, Avx.Multiply(ReadDecoded<TMode, TSamples>(decodedRead, tapOffsets[tx]), Vector256.Create(tapWeights[tx])));
                        selectedCache[cacheAt + column] = sum;
                    }
                }
                var destination = result.Pixels.Slice(y * result.Stride, result.Stride);
                var outputX = 0;
                for (; outputX + 1 < length; outputX += 2)
                {
                    var sum = Vector512<double>.Zero;
                    for (var t = 0; t < count; t++)
                    {
                        var pair = ReadPair<TMode>(cacheRead, loanRead, word && !prepared ? wordRows[t] : rows[t], outputX);
                        sum = Avx512F.Add(sum, Avx512F.Multiply(pair, Vector512.Create(vertical[t])));
                    }
                    if (word)
                    {
                        var n0 = normalizers[outputX] * normalizerY; var n1 = normalizers[outputX + 1] * normalizerY;
                        if (!TryWriteWordPair(sum, n0, n1, destination.Slice((tile + outputX) * 8, 16)))
                        {
                            if (!(n0 >= .5 && n1 >= .5 && sum.GetElement(3) / n0 >= 2 && sum.GetElement(7) / n1 >= 2)
                                || !OriginalWord(source, result, tile + outputX, y, weights.Slice(outputX * taps, counts[outputX]),
                                    offsets.Slice(outputX * taps, counts[outputX]), sourceFirst, vertical[..count], first, context)
                                || !OriginalWord(source, result, tile + outputX + 1, y, weights.Slice((outputX + 1) * taps, counts[outputX + 1]),
                                    offsets.Slice((outputX + 1) * taps, counts[outputX + 1]), sourceFirst, vertical[..count], first, context)) return false;
                        }
                    }
                    else if (!TryWritePair(sum, normalizers[outputX] * normalizerY, normalizers[outputX + 1] * normalizerY,
                        destination.Slice((tile + outputX) * 4, 8))) return false;
                }
                for (var x = outputX; x < length; x++)
                {
                    var sum = Vector256<double>.Zero;
                    for (var t = 0; t < count; t++)
                        sum = Avx.Add(sum, Avx.Multiply(ReadCache<TMode>(cacheRead, loanRead, word && !prepared ? wordRows[t] : rows[t], x), Vector256.Create(vertical[t])));
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
    // Prove once per strip. The original indexed path keeps repeated/clipped
    // samples and unequal counts; no coefficients or ownership are changed.
    internal static int PrepareContiguousPairs(ReadOnlySpan<int> counts,
        ReadOnlySpan<ushort> offsets, int taps)
    {
        var mask = 0;
        for (var x = 0; x + 1 < counts.Length; x += 2)
        {
            var count = counts[x];
            if (count <= 0 || counts[x + 1] != count) continue;
            var first = x * taps; var second = first + taps;
            var start0 = offsets[first]; var start1 = offsets[second];
            var contiguous = true;
            for (var t = 1; t < count; t++)
                contiguous &= offsets[first + t] == start0 + t && offsets[second + t] == start1 + t;
            if (contiguous) mask |= 1 << x;
        }
        return mask;
    }

    internal static bool IsContiguousQuartet(ReadOnlySpan<int> counts, ReadOnlySpan<ushort> offsets,
        int taps, int sourceCount, int pairs)
        => taps == 33 && counts.Length == 4 && sourceCount >= 56 && (pairs & 5) == 5
            && counts[0] == 32 && counts[1] == 32 && counts[2] == 32 && counts[3] == 32
            && offsets[0] == 0 && offsets[33] == 8 && offsets[66] == 16 && offsets[99] == 24;

    // Visit the union of four overlapping windows in seven fixed bands.
    // Each column still consumes its original t0..t31 Add(Multiply) order,
    // with the original column-major coefficients and exact-result repair.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void FilterContiguousQuartet(ReadOnlySpan<Vector256<double>> decoded,
        ReadOnlySpan<double> weights, Span<Vector256<double>> cache, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        decoded = decoded[..56]; weights = weights[..132]; cache = cache[..4];
        var sum0 = Vector256<double>.Zero; var sum1 = Vector256<double>.Zero;
        var sum2 = Vector256<double>.Zero; var sum3 = Vector256<double>.Zero;
        for (var p = 0; p < 8; p++)
            sum0 = Avx.Add(sum0, Avx.Multiply(decoded[p], Vector256.Create(weights[p])));
        for (var p = 8; p < 16; p++)
        {
            var pixel = decoded[p];
            sum0 = Avx.Add(sum0, Avx.Multiply(pixel, Vector256.Create(weights[p])));
            sum1 = Avx.Add(sum1, Avx.Multiply(pixel, Vector256.Create(weights[p + 25])));
        }
        for (var p = 16; p < 24; p++)
        {
            var pixel = decoded[p];
            sum0 = Avx.Add(sum0, Avx.Multiply(pixel, Vector256.Create(weights[p])));
            sum1 = Avx.Add(sum1, Avx.Multiply(pixel, Vector256.Create(weights[p + 25])));
            sum2 = Avx.Add(sum2, Avx.Multiply(pixel, Vector256.Create(weights[p + 50])));
        }
        for (var p = 24; p < 32; p++)
        {
            var pixel = decoded[p];
            sum0 = Avx.Add(sum0, Avx.Multiply(pixel, Vector256.Create(weights[p])));
            sum1 = Avx.Add(sum1, Avx.Multiply(pixel, Vector256.Create(weights[p + 25])));
            sum2 = Avx.Add(sum2, Avx.Multiply(pixel, Vector256.Create(weights[p + 50])));
            sum3 = Avx.Add(sum3, Avx.Multiply(pixel, Vector256.Create(weights[p + 75])));
        }
        for (var p = 32; p < 40; p++)
        {
            var pixel = decoded[p];
            sum1 = Avx.Add(sum1, Avx.Multiply(pixel, Vector256.Create(weights[p + 25])));
            sum2 = Avx.Add(sum2, Avx.Multiply(pixel, Vector256.Create(weights[p + 50])));
            sum3 = Avx.Add(sum3, Avx.Multiply(pixel, Vector256.Create(weights[p + 75])));
        }
        for (var p = 40; p < 48; p++)
        {
            var pixel = decoded[p];
            sum2 = Avx.Add(sum2, Avx.Multiply(pixel, Vector256.Create(weights[p + 50])));
            sum3 = Avx.Add(sum3, Avx.Multiply(pixel, Vector256.Create(weights[p + 75])));
        }
        for (var p = 48; p < 56; p++)
            sum3 = Avx.Add(sum3, Avx.Multiply(decoded[p], Vector256.Create(weights[p + 75])));
        cache[0] = sum0; cache[1] = sum1;
        context.Cancellation.ThrowIfCancellationRequested();
        cache[2] = sum2; cache[3] = sum3;
    }

    // Isolate the indexed reduction so publishing its halves cannot make the
    // accumulator address-exposed throughout the tap loop. Every view borrows
    // the current row; clipped/repeated offsets keep their original order.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static Vector512<double> FilterIndexedPair<TMode, TSamples>(ReadOnlySpan<byte> decoded,
        ReadOnlySpan<ushort> firstOffsets, ReadOnlySpan<ushort> secondOffsets,
        ReadOnlySpan<double> firstWeights, ReadOnlySpan<double> secondWeights, int count)
        where TMode : struct where TSamples : struct
    {
        var prepared = typeof(TMode) == typeof(DecodedPrefix);
        var word = typeof(TSamples) == typeof(WordSamples);
        var decodedValues = prepared ? MemoryMarshal.Cast<byte, Vector256<double>>(decoded) : default;
        var wordValues = !prepared && word ? MemoryMarshal.Cast<byte, Vector128<uint>>(decoded) : default;
        var byteValues = !prepared && !word ? MemoryMarshal.Cast<byte, ulong>(decoded) : default;
        firstOffsets = firstOffsets[..count]; secondOffsets = secondOffsets[..count];
        firstWeights = firstWeights[..count]; secondWeights = secondWeights[..count];
        var sum = Vector512<double>.Zero;
        for (var t = 0; t < firstWeights.Length; t++)
        {
            var first = firstOffsets[t]; var second = secondOffsets[t];
            Vector512<double> values;
            if (prepared)
                values = Avx512F.InsertVector256(decodedValues[first].ToVector512Unsafe(), decodedValues[second], 1);
            else if (word)
                values = Avx512F.ConvertToVector512Double(Vector256.Create(wordValues[first], wordValues[second]));
            else
            {
                var samples = Avx2.ConvertToVector256Int32(Vector128.Create(byteValues[first], byteValues[second]).AsUInt16());
                values = Avx512F.ConvertToVector512Double(samples);
            }
            var coefficients = Avx512F.InsertVector256(Vector512.Create(firstWeights[t]), Vector256.Create(secondWeights[t]), 1);
            sum = Avx512F.Add(sum, Avx512F.Multiply(values, coefficients));
        }
        return sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vector512<double> FilterContiguousPair<TMode, TSamples>(ReadOnlySpan<byte> decoded,
        int firstOffset, int secondOffset, ReadOnlySpan<double> firstWeights, ReadOnlySpan<double> secondWeights,
        int count) where TMode : struct where TSamples : struct
    {
        var prepared = typeof(TMode) == typeof(DecodedPrefix);
        var word = typeof(TSamples) == typeof(WordSamples);
        // All views borrow the current decoded row. Constant modes erase the
        // unused representations. Count-sized views expose each loop bound.
        var firstDecoded = prepared ? MemoryMarshal.Cast<byte, Vector256<double>>(decoded).Slice(firstOffset, count) : default;
        var secondDecoded = prepared ? MemoryMarshal.Cast<byte, Vector256<double>>(decoded).Slice(secondOffset, count) : default;
        var firstWord = !prepared && word ? MemoryMarshal.Cast<byte, Vector128<uint>>(decoded).Slice(firstOffset, count) : default;
        var secondWord = !prepared && word ? MemoryMarshal.Cast<byte, Vector128<uint>>(decoded).Slice(secondOffset, count) : default;
        var firstByte = !prepared && !word ? MemoryMarshal.Cast<byte, ulong>(decoded).Slice(firstOffset, count) : default;
        var secondByte = !prepared && !word ? MemoryMarshal.Cast<byte, ulong>(decoded).Slice(secondOffset, count) : default;
        firstWeights = firstWeights[..count]; secondWeights = secondWeights[..count];
        var sum = Vector512<double>.Zero;
        for (var t = 0; t < firstWeights.Length; t++)
        {
            Vector512<double> values;
            if (prepared)
                values = Avx512F.InsertVector256(firstDecoded[t].ToVector512Unsafe(), secondDecoded[t], 1);
            else if (word)
                values = Avx512F.ConvertToVector512Double(Vector256.Create(firstWord[t], secondWord[t]));
            else
            {
                var samples = Avx2.ConvertToVector256Int32(Vector128.Create(firstByte[t], secondByte[t]).AsUInt16());
                values = Avx512F.ConvertToVector512Double(samples);
            }
            var coefficients = Avx512F.InsertVector256(Vector512.Create(firstWeights[t]), Vector256.Create(secondWeights[t]), 1);
            sum = Avx512F.Add(sum, Avx512F.Multiply(values, coefficients));
        }
        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<double> ReadDecoded<TMode, TSamples>(ReadOnlySpan<byte> products, int offset) where TMode : struct where TSamples : struct
    {
        if (typeof(TMode) == typeof(DecodedPrefix))
            return MemoryMarshal.Read<Vector256<double>>(products.Slice(offset * 32, 32));
        if (typeof(TSamples) == typeof(WordSamples))
        {
            var words = MemoryMarshal.Read<Vector128<uint>>(products.Slice(offset * 16, 16));
            return Avx512F.ConvertToVector512Double(Vector256.Create(words, Vector128<uint>.Zero)).GetLower();
        }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWriteWordPair(Vector512<double> sum, double n0, double n1, Span<byte> output)
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

    private static bool TryWriteWordSingle(Vector256<double> sum, double normalizer, Span<byte> output)
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
