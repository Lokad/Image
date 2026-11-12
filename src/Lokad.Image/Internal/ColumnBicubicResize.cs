using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

/// <summary>Bounded column strips reuse source rows within an admitted stack workspace.</summary>
internal static partial class ColumnBicubicResize
{
    private const int WorkspaceBytes = 4096;

    private readonly struct SmallStrip { }
    private readonly struct LargeStrip { }
    private readonly struct RowLoanStrip { }
    private readonly struct RowTailStrip { }
    private readonly struct ByteSamples { }
    private readonly struct WordSamples { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool TryApply(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, long tapsX, long tapsY, OperationContext context)
    {
        if (source.BitDepth is not (8 or 16) || result.Width < 2 || result.Width >= source.Width || result.Height >= source.Height
            || tapsX > 10 || tapsY > 10
            || !Avx512F.IsSupported || !Avx2.IsSupported || !Avx.IsSupported || !Sse41.IsSupported
            || context.RemainingWorkingBytes < WorkspaceBytes) return false;
        context.Reserve(WorkspaceBytes);
        try
        {
            return source.BitDepth == 16
                ? ApplyPrepared<WordSamples>(source, result, scaleX, scaleY, context)
                : ApplyPrepared<ByteSamples>(source, result, scaleX, scaleY, context);
        }
        finally { context.Release(WorkspaceBytes); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ApplyPrepared<TSamples>(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, OperationContext context) where TSamples : struct
    {
        var word = typeof(TSamples) == typeof(WordSamples);
        var rowSlots = scaleY < 2.25 && (!word || scaleX < 2.25) ? 9 : 10;
        var stripColumns = rowSlots == 9 ? 8 : 6;
        var geometry = ColumnBicubicGeometry.Prepare(result, source.Height, scaleY, 10,
            stripColumns, context.Cancellation);
        if (typeof(TSamples) == typeof(ByteSamples)
            && TryLocateRowLoan(result, rowSlots, out var firstTailRow, out _))
        {
            if (!ApplyRange<RowLoanStrip, ByteSamples>(source, result, scaleX, scaleY, rowSlots,
                geometry, default, 32, 0, result.Width, context)) return false;
            // Cache and coefficient users have returned before their rows are
            // reused for tail metadata and final pixels.
            geometry = ColumnBicubicGeometry.PrepareTail(result, source.Height, scaleY, 10,
                stripColumns, firstTailRow, context.Cancellation);
            return ApplyRange<RowTailStrip, ByteSamples>(source, result, scaleX, scaleY, rowSlots,
                geometry, default, 0, 0, result.Width, context);
        }
        var firstColumn = 0;
        if (geometry.PrefixColumns != 0
            && TryLocateResultCache(result, rowSlots, out var start, out var prefixColumns))
        {
            var cache = MemoryMarshal.Cast<byte, Vector256<double>>(result.Pixels[start..]);
            if (!ApplyRange<LargeStrip, TSamples>(source, result, scaleX, scaleY, rowSlots,
                geometry, cache, result.Stride / 32, 0, prefixColumns, context)) return false;
            firstColumn = prefixColumns;
        }
        return ApplyRange<SmallStrip, TSamples>(source, result, scaleX, scaleY, rowSlots,
            geometry, default, 0, firstColumn, result.Width, context);
    }

    internal static bool TryLocateRowLoan(ImageBuffer result, int rowSlots, out int firstTailRow, out int start)
    {
        firstTailRow = start = 0;
        if (result.BitDepth != 8 || result.Width < 32 || (uint)(rowSlots - 9) > 1) return false;
        var bytes = rowSlots * 32 * 32 + 32 * 10 * sizeof(double);
        var rows = (int)((bytes + (long)result.Stride - 1) / result.Stride);
        var first = result.Height - rows;
        if (first < rowSlots) return false;
        firstTailRow = first; start = checked(first * result.Stride);
        return true;
    }

    internal static bool TryLocateResultCache(ImageBuffer result, int rowSlots,
        out int start, out int prefixColumns)
    {
        start = prefixColumns = 0;
        if (result.BitDepth is not (8 or 16) || result.Stride % 32 != 0 || result.Height < rowSlots) return false;
        // Existing geometry occupies96B at the end of every visible row.
        // Each cache slot occupies512B in a distinct, still-unwritten row tail.
        var pixelBytes = result.BitDepth / 8 * 4;
        var candidate = (result.Width * pixelBytes - 96 - 16 * 32) & ~31;
        if (candidate < 0) return false;
        var prefix = candidate / pixelBytes / 16 * 16;
        if (prefix < 32) return false;
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
    private static bool ApplyRange<TStrip, TSamples>(ImageBuffer source, ImageBuffer result,
        double scaleX, double scaleY, int rowSlots, ColumnBicubicGeometry geometry,
        scoped Span<Vector256<double>> cache, int cacheStride, int firstColumn, int endColumn,
        OperationContext context) where TStrip : struct where TSamples : struct
    {
        // Frames retire before their unpublished result storage is reused.
        // Payloads: old small/large4,080/3,232B; row loan3,424B. No new capacity.
        var rowLoan = typeof(TStrip) == typeof(RowLoanStrip);
        var rowTail = typeof(TStrip) == typeof(RowTailStrip);
        var firstTailRow = 0; var loanStart = 0;
        if (rowLoan || rowTail)
            if (!TryLocateRowLoan(result, rowSlots, out firstTailRow, out loanStart)) return false;
        var large = typeof(TStrip) == typeof(LargeStrip) || rowLoan;
        var word = typeof(TSamples) == typeof(WordSamples);
        var pixelBytes = word ? 8 : 4;
        Span<byte> workspace = stackalloc byte[rowLoan ? 3424 : large ? 3232 : 4080];
        var capacity = rowLoan ? 32 : large ? 16 : 8;
        var sourceCapacity = rowLoan ? 80 : large ? 45 : 25;
        if (rowLoan)
        {
            cache = MemoryMarshal.Cast<byte, Vector256<double>>(result.Pixels.Slice(loanStart, rowSlots * 32 * 32));
            cacheStride = 32;
        }
        else if (!large) { cache = Take<Vector256<double>>(ref workspace, 9 * 8); cacheStride = rowSlots == 9 ? 8 : 6; }
        var rowIds = Take<int>(ref workspace, 10);
        var decoded = Take<Vector256<double>>(ref workspace, sourceCapacity);
        scoped Span<double> weights;
        if (rowLoan)
            weights = MemoryMarshal.Cast<byte, double>(result.Pixels.Slice(loanStart + rowSlots * 32 * 32, 32 * 10 * sizeof(double)));
        else
            weights = Take<double>(ref workspace, capacity * 10);
        var offsets = Take<byte>(ref workspace, capacity * 10);
        var counts = Take<int>(ref workspace, capacity);
        var normalizers = Take<double>(ref workspace, capacity);
        var vertical = Take<double>(ref workspace, 10);
        var rows = Take<int>(ref workspace, 10);
        var input = source.Pixels;
        var stripColumns = rowLoan ? 32 : large ? 16 : rowSlots == 9 ? 8 : 6;
        for (var tile = firstColumn; tile < endColumn; tile += stripColumns)
        {
            var length = Math.Min(stripColumns, endColumn - tile);
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
                    var w = ImageTransforms.BicubicWeight(Math.Abs((first + t - center) / scaleX));
                    weights[x * 10 + t] = w; normalizer += w;
                    var relative = Math.Clamp(first + t, 0, source.Width - 1) - sourceFirst;
                    if ((uint)relative >= (uint)sourceCapacity) return false;
                    offsets[x * 10 + t] = (byte)relative;
                }
                normalizers[x] = normalizer;
            }
            var sourceCount = offsets[(length - 1) * 10 + counts[length - 1] - 1] + 1;
            BicubicMath.PrepareOverlappingPairs(offsets, counts, length, sourceCount, 10, 8, 2, context.Cancellation);
            if (typeof(TSamples) == typeof(ByteSamples))
                PreparePairedByteWeights(weights, offsets, counts[..length],
                    MemoryMarshal.Cast<Vector256<double>, double>(decoded[..4]));
            rowIds.Fill(-1);
            for (var y = rowTail ? firstTailRow : 0; y < (rowLoan ? firstTailRow : result.Height); y++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                geometry.Read(y, tile < geometry.PrefixColumns, vertical,
                    out var first, out var count, out var normalizerY);
                if (count > rowSlots) return false;
                for (var t = 0; t < count; t++)
                {
                    var sy = Math.Clamp(first + t, 0, source.Height - 1);
                    var slot = sy % rowSlots; rows[t] = slot * cacheStride;
                    if (rowIds[slot] == sy) continue;
                    rowIds[slot] = sy;
                    var row = input.Slice(sy * source.Stride + sourceFirst * pixelBytes, sourceCount * pixelBytes);
                    var px = 0;
                    if (word)
                    {
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
                    }
                    else
                    {
                        for (; px + 1 < sourceCount; px += 2)
                        {
                            var packed = BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(px * 4, 8));
                            var samples = Avx2.ConvertToVector256Int32(Vector128.CreateScalar(packed).AsByte());
                            var factors = Avx2.Blend(Avx2.Shuffle(samples, 255), Vector256.Create(1), 136);
                            var products = Avx512F.ConvertToVector512Double(Avx2.MultiplyLow(samples, factors).AsUInt32());
                            decoded[px] = products.GetLower(); decoded[px + 1] = products.GetUpper();
                        }
                        if (px < sourceCount)
                        {
                            var packed = BinaryPrimitives.ReadUInt32LittleEndian(row.Slice(px * 4, 4));
                            var samples = Sse41.ConvertToVector128Int32(Vector128.CreateScalar(packed).AsByte());
                            var factors = Vector128.Create((int)(packed >> 24), (int)(packed >> 24), (int)(packed >> 24), 1);
                            decoded[px] = Avx.ConvertToVector256Double(Sse41.MultiplyLow(samples, factors));
                        }
                    }
                    var x = 0;
                    for (; x + 1 < length; x += 2)
                    {
                        var count0 = counts[x]; var count1 = counts[x + 1];
                        var offsets0 = offsets.Slice(x * 10, count0);
                        var offsets1 = offsets.Slice((x + 1) * 10, count1);
                        var weights0 = weights.Slice(x * 10, count0);
                        var weights1 = weights.Slice((x + 1) * 10, count1);
                        if (count0 == 8 && count1 == 8 && offsets[x * 10 + 9] != 0)
                        {
                            var firstPixel = offsets[x * 10 + 9] - 1;
                            var pair = typeof(TSamples) == typeof(ByteSamples)
                                ? FilterPreparedBytePair(decoded.Slice(firstPixel, 10), weights.Slice(x * 10, 16))
                                : BicubicMath.FilterOverlappingHorizontalPair(
                                    decoded.Slice(firstPixel, 10), weights0, weights1, 8, 2);
                            cache[slot * cacheStride + x] = pair.GetLower();
                            cache[slot * cacheStride + x + 1] = pair.GetUpper();
                            continue;
                        }
                        var shared = Math.Min(count0, count1);
                        var sum = Vector512<double>.Zero;
                        for (var tx = 0; tx < shared; tx++)
                        {
                            // Each half retains its column's original tap order.
                            var samples = Avx512F.InsertVector256(decoded[offsets0[tx]].ToVector512Unsafe(), decoded[offsets1[tx]], 1);
                            var coefficients = Avx512F.InsertVector256(Vector512.Create(weights0[tx]), Vector256.Create(weights1[tx]), 1);
                            sum = Avx512F.Add(sum, Avx512F.Multiply(samples, coefficients));
                        }
                        var sum0 = sum.GetLower(); var sum1 = sum.GetUpper();
                        for (var tx = shared; tx < count0; tx++)
                            sum0 = Avx.Add(sum0, Avx.Multiply(decoded[offsets0[tx]], Vector256.Create(weights0[tx])));
                        for (var tx = shared; tx < count1; tx++)
                            sum1 = Avx.Add(sum1, Avx.Multiply(decoded[offsets1[tx]], Vector256.Create(weights1[tx])));
                        cache[slot * cacheStride + x] = sum0; cache[slot * cacheStride + x + 1] = sum1;
                    }
                    if (x < length)
                    {
                        var sum = Vector256<double>.Zero;
                        for (var tx = 0; tx < counts[x]; tx++)
                            sum = Avx.Add(sum, Avx.Multiply(decoded[offsets[x * 10 + tx]], Vector256.Create(weights[x * 10 + tx])));
                        cache[slot * cacheStride + x] = sum;
                    }
                }
                var destination = word ? result.Pixels.Slice(y * result.Stride, result.Stride) : result.GetRow8(y);
                var outputX = 0;
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
                        if (!BicubicMath.TryWriteWordPairWide(sum, n0, n1, destination.Slice((tile + outputX) * 8, 16)))
                        {
                            // Preserve the word certificate and original-order
                            // ordinary-alpha repair. Tiny alpha unwinds first.
                            if (!(n0 >= .5 && n1 >= .5 && sum.GetElement(3) / n0 >= 2 && sum.GetElement(7) / n1 >= 2)
                                || !OriginalWord(source, result, tile + outputX, y, weights.Slice(outputX * 10, counts[outputX]),
                                    offsets.Slice(outputX * 10, counts[outputX]), sourceFirst, vertical[..count], first, context)
                                || !OriginalWord(source, result, tile + outputX + 1, y, weights.Slice((outputX + 1) * 10, counts[outputX + 1]),
                                    offsets.Slice((outputX + 1) * 10, counts[outputX + 1]), sourceFirst, vertical[..count], first, context)) return false;
                        }
                    }
                    else if (typeof(TSamples) == typeof(ByteSamples) && !BicubicMath.TryWriteOrdinaryBytePair(sum, normalizers[outputX] * normalizerY, normalizers[outputX + 1] * normalizerY,
                        destination.Slice((tile + outputX) * 4, 8)))
                    {
                        // Horizontal users have finished. Borrow decoded storage for
                        // original coefficients only during this uncertain repair.
                        if (!TryRepairBytePair(source, result, tile + outputX, y,
                            weights.Slice(outputX * 10, 20), offsets.Slice(outputX * 10, 20), counts.Slice(outputX, 2),
                            sourceFirst, vertical[..count], first,
                            MemoryMarshal.Cast<Vector256<double>, double>(decoded[..2]), context)) return false;
                    }
                }
                for (var x = outputX; x < length; x++)
                {
                    if (word)
                    {
                        if (!OriginalWord(source, result, tile + x, y, weights.Slice(x * 10, counts[x]),
                            offsets.Slice(x * 10, counts[x]), sourceFirst, vertical[..count], first, context)) return false;
                        continue;
                    }
                    var sum = Vector256<double>.Zero;
                    for (var t = 0; t < count; t++)
                        sum = Avx.Add(sum, Avx.Multiply(cache[rows[t] + x], Vector256.Create(vertical[t])));
                    var normalizer = normalizers[x] * normalizerY;
                    var alpha = sum.GetElement(3);
                    // Each coefficient is bounded by one, with at most 256
                    // two-axis terms. The existing 2^-40 * 16^2 absolute-error
                    // budget covers both orders and normalization sums.
                    const double unitError = 2.3283064365386963e-10;
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
    internal static void PreparePairedByteWeights(Span<double> weights, ReadOnlySpan<byte> offsets,
        ReadOnlySpan<int> counts, Span<double> scratch)
    {
        scratch = scratch[..16];
        for (var x = 0; x + 1 < counts.Length; x += 2)
        {
            if (counts[x] != 8 || counts[x + 1] != 8 || offsets[x * 10 + 9] == 0) continue;
            // Two taps from each column share one four-double load.
            for (var tap = 0; tap < 8; tap += 2)
            {
                scratch[tap * 2] = weights[x * 10 + tap];
                scratch[tap * 2 + 1] = weights[x * 10 + tap + 1];
                scratch[tap * 2 + 2] = weights[(x + 1) * 10 + tap];
                scratch[tap * 2 + 3] = weights[(x + 1) * 10 + tap + 1];
            }
            scratch.CopyTo(weights.Slice(x * 10, 16));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector512<double> FilterPreparedBytePair(ReadOnlySpan<Vector256<double>> decoded,
        ReadOnlySpan<double> weights)
    {
        var pairs = MemoryMarshal.Cast<Vector256<double>, Vector512<double>>(decoded[..10]);
        var groups = MemoryMarshal.Cast<double, Vector512<double>>(weights[..16]);
        var window = pairs[0];
        var sum = Vector512<double>.Zero;
        for (var group = 0; group < 2; group++)
        {
            // The existing packed load contains two taps from both columns,
            // followed by their next two taps. Select each coefficient directly.
            var coefficients = groups[group];
            var next = pairs[group * 2 + 1];
            var evenWeights = Avx512F.PermuteVar8x64(coefficients, Vector512.Create(0L, 0, 0, 0, 2, 2, 2, 2));
            var oddWeights = Avx512F.PermuteVar8x64(coefficients, Vector512.Create(1L, 1, 1, 1, 3, 3, 3, 3));
            var even = Avx512F.Shuffle4x128(window, next, 0x44);
            var odd = Avx512F.Shuffle4x128(window, next, 0xee);
            sum = Avx512F.Add(sum, Avx512F.Multiply(even, evenWeights));
            sum = Avx512F.Add(sum, Avx512F.Multiply(odd, oddWeights));
            window = next;
            next = pairs[group * 2 + 2];
            evenWeights = Avx512F.PermuteVar8x64(coefficients, Vector512.Create(4L, 4, 4, 4, 6, 6, 6, 6));
            oddWeights = Avx512F.PermuteVar8x64(coefficients, Vector512.Create(5L, 5, 5, 5, 7, 7, 7, 7));
            even = Avx512F.Shuffle4x128(window, next, 0x44);
            odd = Avx512F.Shuffle4x128(window, next, 0xee);
            sum = Avx512F.Add(sum, Avx512F.Multiply(even, evenWeights));
            sum = Avx512F.Add(sum, Avx512F.Multiply(odd, oddWeights));
            window = next;
        }
        return sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool TryRepairBytePair(ImageBuffer source, ImageBuffer result, int x, int y,
        ReadOnlySpan<double> weights, ReadOnlySpan<byte> offsets, ReadOnlySpan<int> counts,
        int sourceFirst, ReadOnlySpan<double> vertical, int firstY, Span<double> replay, OperationContext context)
    {
        var prepared = counts[0] == 8 && counts[1] == 8 && offsets[9] != 0;
        for (var lane = 0; lane < 2; lane++)
        {
            var count = counts[lane];
            ReadOnlySpan<double> original = weights.Slice(lane * 10, count);
            if (prepared)
            {
                for (var tap = 0; tap < 8; tap++) replay[tap] = weights[(tap & ~1) * 2 + lane * 2 + (tap & 1)];
                original = replay[..8];
            }
            if (!BicubicMath.TryWriteOriginalByte(source, result, x + lane, y,
                original, offsets.Slice(lane * 10, count), sourceFirst, vertical, firstY, context)) return false;
        }
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
