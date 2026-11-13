using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Shared exact rounding and guarded SIMD publication. Strip/cache kernels keep
// their own geometry, tap order and portable fallback selection.
internal static class BicubicMath
{
    // Both depths accumulate four adjacent pixels in the same tap order.
    // Publication and exact repair remain depth-specific.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void FilterVerticalQuartet(ReadOnlySpan<Vector256<double>> cache,
        ReadOnlySpan<double> vertical, ReadOnlySpan<int> rows, int x,
        out Vector512<double> first, out Vector512<double> second)
    {
        var firstSum = Vector512<double>.Zero; var secondSum = Vector512<double>.Zero;
        for (var t = 0; t < vertical.Length; t++)
        {
            var weight = Vector512.Create(vertical[t]);
            var row = MemoryMarshal.AsBytes(cache.Slice(rows[t] + x, 4));
            firstSum = Avx512F.Add(firstSum, Avx512F.Multiply(MemoryMarshal.Read<Vector512<double>>(row), weight));
            secondSum = Avx512F.Add(secondSum, Avx512F.Multiply(MemoryMarshal.Read<Vector512<double>>(row[64..]), weight));
        }
        first = firstSum; second = secondSum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryWriteOrdinaryBytePair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 >= 16 && a1 >= 16)) return false;
        var divisors = Vector512.Create(a0, a0, a0, n0, a1, a1, a1, n1);
        // VRCP14PD has relative error <2^-14 (Intel ISA reference). One fused
        // Newton correction and the quotient multiply give error <2^-27.
        // For alpha sums >=16 and normalizers >=.5, the wider existing
        // 400/2^40 accumulation bound gives color/alpha error <7.852e-6 /
        // 2.660e-6 including reciprocal error near quantization thresholds.
        // The unchanged 2^-16 margin covers both narrow and wide byte paths.
        // Clipped endpoints are stable under the same affine error bound.
        var reciprocal = Avx512F.Reciprocal14(divisors);
        reciprocal = Avx512F.Multiply(reciprocal,
            Avx512F.FusedMultiplyAddNegated(divisors, reciprocal, Vector512.Create(2d)));
        var values = Avx512F.Multiply(sum, reciprocal);
        const double margin = 1.52587890625e-5;
        var margins = Vector512.Create(margin);
        if (!(values.GetElement(3) >= 1 + margin && values.GetElement(7) >= 1 + margin)) return false;
        if (!TryRoundBytePair(values, margins, out var ints)) return false;
        var shorts = Sse2.PackSignedSaturate(ints.GetLower(), ints.GetUpper());
        var packed = Sse2.PackUnsignedSaturate(shorts, Vector128<short>.Zero).AsUInt64().GetElement(0);
        BinaryPrimitives.WriteUInt64LittleEndian(output, packed);
        return true;
    }

    // Callers supply finite values and nonnegative margins below .25.
    // After clipping, .5 - abs(v - round(v)) is the same distance to a
    // half-integer used by the certificate. Near that boundary both distances
    // are exact (Sterbenz); clipped integer endpoints are never ambiguous.
    // Reuse this nearest-even result for output instead of rounding twice.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryRoundBytePair(Vector512<double> values, Vector512<double> margins,
        out Vector256<int> integers)
    {
        var clipped = Avx512F.Min(Vector512.Create(255d), Avx512F.Max(Vector512<double>.Zero, values));
        var rounded = Avx512F.RoundScale(clipped, 0);
        var residual = Avx512F.AndNot(Vector512.Create(long.MinValue),
            Avx512F.Subtract(clipped, rounded).AsInt64()).AsDouble();
        var distance = Avx512F.Subtract(Vector512.Create(.5), residual);
        if (Vector512.ExtractMostSignificantBits(Avx512F.CompareLessThanOrEqual(distance, margins)) != 0)
        {
            integers = default;
            return false;
        }
        integers = Avx512F.ConvertToVector256Int32WithTruncation(rounded);
        return true;
    }

    // Proven contiguous windows need no per-tap offset loads or source checks.
    // Each output keeps its original t0..tn multiply/add sequence.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void FilterContiguousHorizontalPair(
        ReadOnlySpan<Vector256<double>> decoded, ReadOnlySpan<double> firstWeights,
        ReadOnlySpan<double> secondWeights, int stride, out Vector256<double> firstSum, out Vector256<double> secondSum)
    {
        var first = decoded[..firstWeights.Length];
        var second = decoded.Slice(stride, firstWeights.Length);
        var rightWeights = secondWeights[..firstWeights.Length];
        var left = Vector256<double>.Zero; var right = Vector256<double>.Zero;
        for (var t = 0; t < firstWeights.Length; t++)
        {
            left = Avx.Add(left, Avx.Multiply(first[t], Vector256.Create(firstWeights[t])));
            right = Avx.Add(right, Avx.Multiply(second[t], Vector256.Create(rightWeights[t])));
        }
        firstSum = left; secondSum = right;
    }

    // Callers select fixed four/eight-tap/+2 or sixteen-tap/+4 geometry. Reuse
    // overlapping source windows without changing either lane's tap order.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector512<double> FilterOverlappingHorizontalPair(
        ReadOnlySpan<Vector256<double>> decoded, ReadOnlySpan<double> firstWeights,
        ReadOnlySpan<double> secondWeights, int taps, int stride)
    {
        var window = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded[..2]));
        var delayed = stride == 4
            ? MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(2, 2))) : default;
        var carry = stride == 4
            ? MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(4, 2))) : default;
        var sum = Vector512<double>.Zero;
        for (var tap = 0; tap < taps; tap += 2)
        {
            var next = stride == 4 ? carry
                : MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(tap + 2, 2)));
            var even = Avx512F.Shuffle4x128(window, next, 0x44);
            var odd = Avx512F.Shuffle4x128(window, next, 0xee);
            var evenWeights = Avx512F.InsertVector256(Vector512.Create(firstWeights[tap]), Vector256.Create(secondWeights[tap]), 1);
            var oddWeights = Avx512F.InsertVector256(Vector512.Create(firstWeights[tap + 1]), Vector256.Create(secondWeights[tap + 1]), 1);
            sum = Avx512F.Add(sum, Avx512F.Multiply(even, evenWeights));
            sum = Avx512F.Add(sum, Avx512F.Multiply(odd, oddWeights));
            if (stride == 4)
            {
                window = delayed; delayed = next;
                if (tap + 2 < taps)
                    carry = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(tap + 6, 2)));
            }
            else window = next;
        }
        return sum;
    }

    // Four sixteen-tap outputs start four pixels apart. Keep two ordered
    // pair sums while recycling fourteen two-pixel loads across all outputs.
    // The cache is operation-private. Check cancellation before bounded work;
    // no caller observes or resumes a failed partial cache.
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void FilterOverlappingHorizontalQuartet(ReadOnlySpan<Vector256<double>> decoded,
        ReadOnlySpan<double> weights0, ReadOnlySpan<double> weights1,
        ReadOnlySpan<double> weights2, ReadOnlySpan<double> weights3,
        Span<Vector256<double>> cached, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        decoded = decoded[..28];
        weights0 = weights0[..16]; weights1 = weights1[..16];
        weights2 = weights2[..16]; weights3 = weights3[..16];
        cached = cached[..4];
        var p0 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded[..2]));
        var p1 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(2, 2)));
        var p2 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(4, 2)));
        var p3 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(6, 2)));
        var p4 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(8, 2)));
        var p5 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(10, 2)));
        var p6 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(12, 2)));
        var p7 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(14, 2)));
        var sum0 = Vector512<double>.Zero; var sum1 = Vector512<double>.Zero;
        for (var tap = 0; tap < 16; tap += 4)
        {
            var a = Avx512F.Shuffle4x128(p0, p2, 0x44);
            var b = Avx512F.Shuffle4x128(p4, p6, 0x44);
            var wa = Avx512F.InsertVector256(Vector512.Create(weights0[tap]), Vector256.Create(weights1[tap]), 1);
            var wb = Avx512F.InsertVector256(Vector512.Create(weights2[tap]), Vector256.Create(weights3[tap]), 1);
            sum0 = Avx512F.Add(sum0, Avx512F.Multiply(a, wa));
            sum1 = Avx512F.Add(sum1, Avx512F.Multiply(b, wb));
            a = Avx512F.Shuffle4x128(p0, p2, 0xee);
            b = Avx512F.Shuffle4x128(p4, p6, 0xee);
            wa = Avx512F.InsertVector256(Vector512.Create(weights0[tap + 1]), Vector256.Create(weights1[tap + 1]), 1);
            wb = Avx512F.InsertVector256(Vector512.Create(weights2[tap + 1]), Vector256.Create(weights3[tap + 1]), 1);
            sum0 = Avx512F.Add(sum0, Avx512F.Multiply(a, wa));
            sum1 = Avx512F.Add(sum1, Avx512F.Multiply(b, wb));
            a = Avx512F.Shuffle4x128(p1, p3, 0x44);
            b = Avx512F.Shuffle4x128(p5, p7, 0x44);
            wa = Avx512F.InsertVector256(Vector512.Create(weights0[tap + 2]), Vector256.Create(weights1[tap + 2]), 1);
            wb = Avx512F.InsertVector256(Vector512.Create(weights2[tap + 2]), Vector256.Create(weights3[tap + 2]), 1);
            sum0 = Avx512F.Add(sum0, Avx512F.Multiply(a, wa));
            sum1 = Avx512F.Add(sum1, Avx512F.Multiply(b, wb));
            a = Avx512F.Shuffle4x128(p1, p3, 0xee);
            b = Avx512F.Shuffle4x128(p5, p7, 0xee);
            wa = Avx512F.InsertVector256(Vector512.Create(weights0[tap + 3]), Vector256.Create(weights1[tap + 3]), 1);
            wb = Avx512F.InsertVector256(Vector512.Create(weights2[tap + 3]), Vector256.Create(weights3[tap + 3]), 1);
            sum0 = Avx512F.Add(sum0, Avx512F.Multiply(a, wa));
            sum1 = Avx512F.Add(sum1, Avx512F.Multiply(b, wb));
            if (tap + 4 < 16)
            {
                p0 = p2; p1 = p3; p2 = p4; p3 = p5; p4 = p6; p5 = p7;
                p6 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(tap + 16, 2)));
                p7 = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(tap + 18, 2)));
            }
        }
        MemoryMarshal.Write(MemoryMarshal.AsBytes(cached[..2]), in sum0);
        MemoryMarshal.Write(MemoryMarshal.AsBytes(cached.Slice(2, 2)), in sum1);
    }

    // Four taps in a common window or windows shifted by one pixel. Every
    // half keeps its original t0,t1,t2,t3 multiply/add sequence.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector512<double> FilterAdjacentHorizontalPair(
        ReadOnlySpan<Vector256<double>> decoded, ReadOnlySpan<double> firstWeights,
        ReadOnlySpan<double> secondWeights, bool shifted)
    {
        var early = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded[..2]));
        var late = MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(2, 2)));
        var t0 = shifted ? early : Avx512F.Shuffle4x128(early, early, 0x44);
        var t1 = shifted ? Avx512F.Shuffle4x128(early, late, 0x4e) : Avx512F.Shuffle4x128(early, early, 0xee);
        var t2 = shifted ? late : Avx512F.Shuffle4x128(late, late, 0x44);
        var t3 = shifted ? MemoryMarshal.Read<Vector512<double>>(MemoryMarshal.AsBytes(decoded.Slice(3, 2)))
            : Avx512F.Shuffle4x128(late, late, 0xee);
        var w0 = Avx512F.InsertVector256(Vector512.Create(firstWeights[0]), Vector256.Create(secondWeights[0]), 1);
        var w1 = Avx512F.InsertVector256(Vector512.Create(firstWeights[1]), Vector256.Create(secondWeights[1]), 1);
        var w2 = Avx512F.InsertVector256(Vector512.Create(firstWeights[2]), Vector256.Create(secondWeights[2]), 1);
        var w3 = Avx512F.InsertVector256(Vector512.Create(firstWeights[3]), Vector256.Create(secondWeights[3]), 1);
        var sum = Avx512F.Add(Vector512<double>.Zero, Avx512F.Multiply(t0, w0));
        sum = Avx512F.Add(sum, Avx512F.Multiply(t1, w1));
        sum = Avx512F.Add(sum, Avx512F.Multiply(t2, w2));
        return Avx512F.Add(sum, Avx512F.Multiply(t3, w3));
    }

    internal static bool PrepareAdjacentFourTapPairs(Span<int> offsets, ReadOnlySpan<int> counts,
        int sourceCount, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        for (var x = 0; x < counts.Length; x++)
        {
            if ((x & 255) == 0) cancellation.ThrowIfCancellationRequested();
            if (counts[x] < 5) offsets[x * 5 + 4] = 0;
        }
        var paired = false;
        for (var x = 0; x + 1 < counts.Length; x += 2)
        {
            if ((x & 255) == 0) cancellation.ThrowIfCancellationRequested();
            if (counts[x] != 4 || counts[x + 1] != 4) continue;
            var at = x * 5; var first = offsets[at]; var second = offsets[at + 5];
            if (first < 0 || second < first || second - first > 1) continue;
            var shift = second - first;
            if (sourceCount < 4 + shift || first > sourceCount - 4 - shift) continue;
            var contiguous = true;
            for (var t = 0; t < 4; t++)
                contiguous &= offsets[at + t] == first + t && offsets[at + 5 + t] == second + t;
            if (!contiguous) continue;
            // Only dead padding carries the proof: -1 common, -2 adjacent.
            offsets[at + 4] = -1 - shift;
            paired = true;
        }
        return paired;
    }

    // Each vector already contains a source-column pair. Delay two decoded
    // rows so both output rows reuse ten loads and retain their own tap order.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void FilterOverlappingVerticalPair(ReadOnlySpan<Vector512<double>> decoded,
        ReadOnlySpan<double> firstWeights, ReadOnlySpan<double> secondWeights,
        out Vector512<double> firstSum, out Vector512<double> secondSum)
    {
        var early0 = decoded[0]; var early1 = decoded[1];
        var sum0 = Vector512<double>.Zero; var sum1 = Vector512<double>.Zero;
        for (var tap = 0; tap < 8; tap += 2)
        {
            var late0 = decoded[tap + 2]; var late1 = decoded[tap + 3];
            sum0 = Avx512F.Add(sum0, Avx512F.Multiply(early0, Vector512.Create(firstWeights[tap])));
            sum1 = Avx512F.Add(sum1, Avx512F.Multiply(late0, Vector512.Create(secondWeights[tap])));
            sum0 = Avx512F.Add(sum0, Avx512F.Multiply(early1, Vector512.Create(firstWeights[tap + 1])));
            sum1 = Avx512F.Add(sum1, Avx512F.Multiply(late1, Vector512.Create(secondWeights[tap + 1])));
            early0 = late0; early1 = late1;
        }
        firstSum = sum0; secondSum = sum1;
    }

    internal static void PrepareOverlappingPairs(Span<byte> offsets, ReadOnlySpan<int> counts,
        int columns, int sourceCount, int capacity, int taps, int stride, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        if (taps >= capacity) return;
        // The proof occupies only dead padding, reset on every strip. A full
        // column retains its last slot as an actual source offset.
        for (var x = 0; x < columns; x++)
            if (counts[x] < capacity) offsets[x * capacity + capacity - 1] = 0;
        for (var x = 0; x + 1 < columns; x += 2)
        {
            if (counts[x] != taps || counts[x + 1] != taps) continue;
            var base0 = x * capacity; var base1 = base0 + capacity;
            var first = offsets[base0]; var sourcePixels = taps + stride;
            if (sourceCount < sourcePixels || first > sourceCount - sourcePixels || offsets[base1] != first + stride) continue;
            var contiguous = true;
            for (var tap = 0; tap < taps; tap++)
                contiguous &= offsets[base0 + tap] == first + tap && offsets[base1 + tap] == first + tap + stride;
            if (contiguous) offsets[base0 + capacity - 1] = (byte)(first + 1);
        }
    }

    internal static void PrepareOverlappingPairs(Span<int> offsets, ReadOnlySpan<int> counts,
        int columns, int sourceCount, int capacity, int taps, int stride, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        if (taps >= capacity) return;
        // Full-row word preparation needs int indexes. Keep the same dead-slot
        // proof as the byte-index strips; never overwrite a live final tap.
        for (var x = 0; x < columns; x++)
        {
            if ((x & 127) == 0) cancellation.ThrowIfCancellationRequested();
            if (counts[x] < capacity) offsets[x * capacity + capacity - 1] = 0;
        }
        for (var x = 0; x + 1 < columns; x += 2)
        {
            if ((x & 127) == 0) cancellation.ThrowIfCancellationRequested();
            if (counts[x] != taps || counts[x + 1] != taps) continue;
            var base0 = x * capacity; var base1 = base0 + capacity;
            var first = offsets[base0]; var sourcePixels = taps + stride;
            if (first < 0 || sourceCount < sourcePixels || first > sourceCount - sourcePixels
                || offsets[base1] != first + stride) continue;
            var contiguous = true;
            for (var tap = 0; tap < taps; tap++)
                contiguous &= offsets[base0 + tap] == first + tap && offsets[base1 + tap] == first + tap + stride;
            if (contiguous) offsets[base0 + capacity - 1] = first + 1;
        }
    }

    internal const int StreamingWorkspaceBytes = 4096;
    internal const int StreamingTapCapacity = 304;
    internal const int PackedStreamingTapCapacity = 544;
    // One vector total, center/normalizer and first/last coordinate per row.
    private const int StreamingRowBytes = 6 * sizeof(double) + 2 * sizeof(int);
    internal const int StreamingRows = (StreamingWorkspaceBytes
        - StreamingTapCapacity * (sizeof(double) + sizeof(ushort))) / StreamingRowBytes;
    internal const int PackedStreamingRows = (StreamingWorkspaceBytes
        - PackedStreamingTapCapacity * (sizeof(uint) + sizeof(ushort))) / (StreamingRowBytes + sizeof(double));

    // Geometry is bounded by the caller's admitted block, without pixel dispatch.
    internal static int PrepareStreamingRows(int sourceHeight, int targetHeight, int blockStart,
        double scale, Span<double> centers, Span<int> first, Span<int> last)
    {
        var height = Math.Min(centers.Length, targetHeight - blockStart);
        for (var y = 0; y < height; y++)
        {
            centers[y] = (blockStart + y + .5) * sourceHeight / targetHeight - .5;
            first[y] = (int)Math.Ceiling(centers[y] - 2 * scale);
            last[y] = (int)Math.Floor(centers[y] + 2 * scale);
        }
        return height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryWriteBytePair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
        const double maximum = 255d;
        const double alphaError = unitError * maximum;
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 > alphaError && a1 > alphaError)) return false;
        var divisors = Vector512.Create(a0, a0, a0, n0, a1, a1, a1, n1);
        var values = Avx512F.Divide(sum, divisors);
        // With 2^-40*16^2 and maximum 255, quotient bounds at every
        // in-range threshold are below 1e-6 for alpha and 1e-4/alpha
        // for color. The quarter-margin guard protects clipping thresholds.
        // Alpha's lower power of two gives a conservative reciprocal bound.
        var exponent0 = (int)((BitConverter.DoubleToUInt64Bits(a0) >> 52) & 2047);
        var exponent1 = (int)((BitConverter.DoubleToUInt64Bits(a1) >> 52) & 2047);
        var margin0 = 1e-4 * BitConverter.UInt64BitsToDouble((ulong)(2046 - exponent0) << 52);
        var margin1 = 1e-4 * BitConverter.UInt64BitsToDouble((ulong)(2046 - exponent1) << 52);
        if (margin0 >= .25 || margin1 >= .25
            || !(values.GetElement(3) >= 1 + 1e-6 && values.GetElement(7) >= 1 + 1e-6)) return false;
        var margins = Vector512.Create(margin0, margin0, margin0, 1e-6, margin1, margin1, margin1, 1e-6);
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

    internal static bool TryWriteByte(Vector256<double> sum, double normalizer, Span<byte> output)
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryWriteWordPair(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        return PrepareWordPair(sum, n0, n1, out var values, out var margins)
            && WritePreparedWordPair(values, margins, output);
    }

    // Prepare two independent quotients before either certificate. Each pair
    // retains its own acceptance bit and leaves refused output untouched.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int WriteWordQuartet(Vector512<double> first, Vector512<double> second,
        double n0, double n1, double n2, double n3, Span<byte> output)
    {
        var validFirst = PrepareWordPair(first, n0, n1, out var firstValues, out var firstMargins);
        var validSecond = PrepareWordPair(second, n2, n3, out var secondValues, out var secondMargins);
        var acceptedFirst = validFirst && WritePreparedWordPair(firstValues, firstMargins, output[..16]);
        var acceptedSecond = validSecond && WritePreparedWordPair(secondValues, secondMargins, output.Slice(16, 16));
        return (acceptedFirst ? 1 : 0) | (acceptedSecond ? 2 : 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool PrepareWordPair(Vector512<double> sum, double n0, double n1,
        out Vector512<double> values, out Vector512<double> margins)
    {
        const double alphaError = 2.3283064365386963e-10 * 65535d;
        values = margins = default;
        if (!(n0 >= .5 && n1 >= .5 && sum.GetElement(3) > alphaError && sum.GetElement(7) > alphaError)) return false;
        var alpha = Avx512F.PermuteVar8x64(sum, Vector512.Create(3L, 3, 3, 3, 7, 7, 7, 7));
        var alphaLanes = Vector512.Create(0L, 0, 0, -1L, 0, 0, 0, -1L).AsDouble();
        var normalizers = Vector512.Create(Vector256.Create(n0), Vector256.Create(n1));
        values = Avx512F.Divide(sum, Vector512.ConditionalSelect(alphaLanes, normalizers, alpha));
        // Established 2^-40*16^2 word bounds cover every in-range
        // rounding threshold. The integer shifts reproduce the scalar exponent
        // construction bit for bit; arithmetic and division precision stay intact.
        var exponents = Avx512F.And(Avx512F.ShiftRightLogical(alpha.AsUInt64(), 52), Vector512.Create(2047UL));
        var powers = Avx512F.ShiftLeftLogical(Avx512F.Subtract(Vector512.Create(2046UL), exponents), 52).AsDouble();
        margins = Vector512.ConditionalSelect(alphaLanes, Vector512.Create(1e-4), Avx512F.Multiply(powers, Vector512.Create(2.0001)));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool WritePreparedWordPair(Vector512<double> values, Vector512<double> margins, Span<byte> output)
    {
        const double maximum = 65535d;
        if (Vector512.ExtractMostSignificantBits(Avx512F.CompareGreaterThanOrEqual(margins, Vector512.Create(.25))) != 0
            || !(values.GetElement(3) >= 1 + 1e-4 && values.GetElement(7) >= 1 + 1e-4)) return false;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryWriteWordPairWide(Vector512<double> sum, double n0, double n1, Span<byte> output)
    {
        const double unitError = 2.3283064365386963e-10;
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

    internal static bool TryWriteWord(Vector256<double> sum, double normalizer, Span<byte> output)
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

    internal static bool TryWriteOriginalByte(ImageBuffer source, ImageBuffer result, int x, int y,
        ReadOnlySpan<double> weights, ReadOnlySpan<byte> offsets, int sourceFirst,
        ReadOnlySpan<double> vertical, int firstY, OperationContext context)
    {
        double red = 0, green = 0, blue = 0, alpha = 0, normalizer = 0;
        for (var ty = 0; ty < vertical.Length; ty++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var wy = vertical[ty]; if (wy == 0) continue;
            var row = source.GetRow8(Math.Clamp(firstY + ty, 0, source.Height - 1));
            for (var tx = 0; tx < weights.Length; tx++)
            {
                var weight = wy * weights[tx]; if (weight == 0) continue;
                var offset = (sourceFirst + offsets[tx]) * 4;
                var a = row[offset + 3];
                red += weight * a * row[offset];
                green += weight * a * row[offset + 1];
                blue += weight * a * row[offset + 2];
                alpha += weight * a; normalizer += weight;
            }
        }
        var normalized = alpha / normalizer;
        // Exact tiny-alpha arithmetic needs this same 4-KiB allowance after unwind.
        if (normalized > 0 && normalized < 1) return false;
        var aOut = BicubicMath.RoundByte(normalized);
        var color = aOut > 0 && alpha > 0;
        var output = result.GetRow8(y).Slice(x * 4, 4);
        output[0] = BicubicMath.RoundByte(color ? red / alpha : 0);
        output[1] = BicubicMath.RoundByte(color ? green / alpha : 0);
        output[2] = BicubicMath.RoundByte(color ? blue / alpha : 0);
        output[3] = aOut;
        return true;
    }

    internal static byte RoundByte(double value)
        => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);

    internal static ushort RoundWord(double value)
        => (ushort)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 65535);
    internal static int Round(double value,int maximum)=>(int)Math.Clamp(Math.Round(value,MidpointRounding.AwayFromZero),0,maximum);

    internal static Vector512<double> Coefficients(int first, double center, double scale)
    {
        var positions = Vector512.Create((double)first, first + 1d, first + 2d, first + 3d,
            first + 4d, first + 5d, first + 6d, first + 7d);
        var distance = Avx512F.Divide(Avx512F.Subtract(positions, Vector512.Create(center)), Vector512.Create(scale));
        var x = Avx512F.AndNot(Vector512.Create(long.MinValue), distance.AsInt64()).AsDouble();
        var squareTerm = Avx512F.Multiply(Avx512F.Multiply(Vector512.Create(2.5), x), x);
        var nearCube = Avx512F.Multiply(Avx512F.Multiply(Avx512F.Multiply(Vector512.Create(1.5), x), x), x);
        var near = Avx512F.Add(Avx512F.Subtract(nearCube, squareTerm), Vector512.Create(1d));
        var farCube = Avx512F.Multiply(Avx512F.Multiply(Avx512F.Multiply(Vector512.Create(-.5), x), x), x);
        var far = Avx512F.Add(Avx512F.Subtract(Avx512F.Add(farCube, squareTerm),
            Avx512F.Multiply(Vector512.Create(4d), x)), Vector512.Create(2d));
        var outside = Vector512.ConditionalSelect(Avx512F.CompareLessThan(x, Vector512.Create(2d)), far, Vector512<double>.Zero);
        return Vector512.ConditionalSelect(Avx512F.CompareLessThan(x, Vector512.Create(1d)), near, outside);
    }

    internal static double Coefficient(uint low, ushort high)
        => BitConverter.UInt64BitsToDouble(((ulong)high << 48) | ((ulong)low << 16));

    // Four independently owned pixels; each original pair keeps its acceptance
    // bit and leaves refused staging bytes untouched. Existing pair users retain
    // their direct path. Prepare both quotients before either certificate.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int WriteByteQuartet(Vector512<double> first, Vector512<double> second,
        double n0, double n1, double n2, double n3, Span<byte> output)
    {
        var validFirst = PrepareByteQuartetPair(first, n0, n1, out var firstValues, out var firstMargins);
        var validSecond = PrepareByteQuartetPair(second, n2, n3, out var secondValues, out var secondMargins);
        var acceptedFirst = validFirst && CertifyByteQuartetPair(firstValues, firstMargins, output[..8]);
        var acceptedSecond = validSecond && CertifyByteQuartetPair(secondValues, secondMargins, output.Slice(8, 8));
        return (acceptedFirst ? 1 : 0) | (acceptedSecond ? 2 : 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PrepareByteQuartetPair(Vector512<double> sum, double n0, double n1,
        out Vector512<double> values, out Vector512<double> margins)
    {
        const double unitError = 2.3283064365386963e-10;
        const double maximum = 255d;
        const double alphaError = unitError * maximum;
        values = margins = default;
        var a0 = sum.GetElement(3); var a1 = sum.GetElement(7);
        if (!(n0 >= .5 && n1 >= .5 && a0 > alphaError && a1 > alphaError)) return false;
        var alpha = Avx512F.PermuteVar8x64(sum, Vector512.Create(3L, 3, 3, 3, 7, 7, 7, 7));
        var alphaLanes = Vector512.Create(0L, 0, 0, -1L, 0, 0, 0, -1L).AsDouble();
        var normalizers = Vector512.Create(Vector256.Create(n0), Vector256.Create(n1));
        values = Avx512F.Divide(sum, Vector512.ConditionalSelect(alphaLanes, normalizers, alpha));
        // With 2^-40*16^2 and maximum 255, quotient bounds at every
        // in-range threshold are below 1e-6 for alpha and 1e-4/alpha
        // for color. The quarter-margin guard protects clipping thresholds.
        // Alpha's lower power of two gives a conservative reciprocal bound.
        var exponents = Avx512F.And(Avx512F.ShiftRightLogical(alpha.AsUInt64(), 52), Vector512.Create(2047UL));
        var powers = Avx512F.ShiftLeftLogical(Avx512F.Subtract(Vector512.Create(2046UL), exponents), 52).AsDouble();
        margins = Vector512.ConditionalSelect(alphaLanes, Vector512.Create(1e-6), Avx512F.Multiply(powers, Vector512.Create(1e-4)));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CertifyByteQuartetPair(Vector512<double> values, Vector512<double> margins, Span<byte> output)
    {
        const double maximum = 255d;
        if (Vector512.ExtractMostSignificantBits(Avx512F.CompareGreaterThanOrEqual(margins, Vector512.Create(.25))) != 0
            || !(values.GetElement(3) >= 1 + 1e-6 && values.GetElement(7) >= 1 + 1e-6)) return false;
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
}
