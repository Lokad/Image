using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Independent matrix evaluation of T.81 A.3.3 at the pinned reference's
// fixed-point boundaries, used by every JPEG inverse block.
// The kernel is generated from the cosine definition, without upstream tables.
internal static class JpegInverseTransform
{
    private static readonly int[] Kernel = BuildKernel();
    private static readonly Vector256<short> Row01 = BuildRowPair(0);
    private static readonly Vector256<short> Row23 = BuildRowPair(2);
    private static readonly Vector256<short> Row45 = BuildRowPair(4);
    private static readonly Vector256<short> Row67 = BuildRowPair(6);
    private static readonly Vector512<short> Column01 = BuildColumnPair(0);
    private static readonly Vector512<short> Column23 = BuildColumnPair(2);
    private static readonly Vector512<short> Column45 = BuildColumnPair(4);
    private static readonly Vector512<short> Column67 = BuildColumnPair(6);

    private static Vector512<short> BuildColumnPair(int row)
    {
        // Match the existing reflected column evaluation, including its
        // explicitly negated odd-frequency weight in the lower half.
        int Odd(int y) => y < 4 ? Kernel[8 + y] : -Kernel[8 + 7 - y];
        return Vector512.Create(PairWeights(Kernel[0], Odd(row)), PairWeights(Kernel[0], Odd(row + 1)));
    }

    private static Vector256<short> BuildRowPair(int frequency)
    {
        Span<short> weights = stackalloc short[16];
        for (var x = 0; x < 8; x++)
        {
            weights[x * 2] = checked((short)Kernel[frequency * 8 + x]);
            weights[x * 2 + 1] = checked((short)Kernel[(frequency + 1) * 8 + x]);
        }
        return MemoryMarshal.Read<Vector256<short>>(MemoryMarshal.AsBytes(weights));
    }

    private static int[] BuildKernel()
    {
        var result = new int[64];
        for (var x = 0; x < 8; x++)
        for (var frequency = 0; frequency < 8; frequency++)
        {
            var value = frequency == 0 ? 1 : Math.Sqrt(2) * Math.Cos((2 * x + 1) * frequency * Math.PI / 16);
            // The pinned 8-bit policy represents the unit rotation one Q14
            // step below unity, including the DC and quarter-turn columns.
            // One frequency's eight output weights are contiguous for SIMD.
            result[frequency * 8 + x] = frequency is 0 or 4
                ? Math.Sign(value) * ((1 << 14) - 1)
                : (int)Math.Round(value * (1 << 14), MidpointRounding.AwayFromZero);
        }
        return result;
    }

    internal static void Inverse(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization,
        Span<byte> pixels, OperationContext context, bool scanOrder = false)
    {
        if (coefficients.Length != 64 || quantization.Length != 64 || pixels.Length != 64)
            throw new ArgumentException("JPEG transforms operate on complete 8x8 blocks.");
        context.Cancellation.ThrowIfCancellationRequested();
        Span<short> input = stackalloc short[64];
        Span<short> rows = stackalloc short[64];
        var support = scanOrder ? DequantizeScanOrder(coefficients, quantization, input)
            : Dequantize(coefficients, quantization, input);
        if (Avx2.IsSupported ? (support & ~1UL) == 0 : !input[1..].ContainsAnyExcept((short)0))
        {
            // Both matrix passes are constant. Preserve the row word wrap
            // and the column's word bias before its widened multiplication.
            var row = unchecked((short)(input[0] * 8));
            var biased = unchecked((short)(row + 32));
            pixels.Fill((byte)Math.Clamp((biased * Kernel[0]) >> 20, 0, 255));
            return;
        }
        if (Avx2.IsSupported)
        {
            InverseAvx2(input, rows, pixels, support);
            return;
        }
        for (var y = 0; y < 8; y++)
        {
            var row = input.Slice(y * 8, 8);
            if (!row[1..].ContainsAnyExcept((short)0))
            {
                rows.Slice(y * 8, 8).Fill(unchecked((short)(row[0] * 8)));
                continue;
            }
            for (var x = 0; x < 8; x++)
            {
                var sum = 1 << 10;
                for (var u = 0; u < 8; u++)
                    sum = unchecked(sum + row[u] * Kernel[u * 8 + x]);
                rows[y * 8 + x] = (short)Math.Clamp(sum >> 11, short.MinValue, short.MaxValue);
            }
        }
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            // The reference SIMD path adds the column bias in signed words
            // before multiplication. Widening that addition changes extremes.
            var biased = unchecked((short)(rows[x] + 32));
            var sum = biased * Kernel[y];
            for (var v = 1; v < 8; v++)
                sum = unchecked(sum + rows[v * 8 + x] * Kernel[v * 8 + y]);
            pixels[y * 8 + x] = (byte)Math.Clamp(sum >> 20, 0, 255);
        }
    }

    // The caller validates complete 64-sample blocks. AC only retains the low
    // word, so truncating operands before word multiplication is exact modulo
    // 65536 for the entire Int32 coefficient/UInt16 quantizer domain.
    internal static ulong Dequantize(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization, Span<short> input)
    {
        if (Avx512BW.IsSupported)
        {
            var low = Vector512.Narrow(Vector512.Create(coefficients), Vector512.Create(coefficients[16..]));
            var high = Vector512.Narrow(Vector512.Create(coefficients[32..]), Vector512.Create(coefficients[48..]));
            low = Avx512BW.MultiplyLow(low, Vector512.Create(quantization).AsInt16());
            high = Avx512BW.MultiplyLow(high, Vector512.Create(quantization[32..]).AsInt16());
            return StoreDequantized(low, high, coefficients[0], quantization[0], input);
        }
        if (Avx2.IsSupported)
        {
            for (var i = 0; i < 64; i += 16)
            {
                var first = MemoryMarshal.Read<Vector256<int>>(MemoryMarshal.AsBytes(coefficients.Slice(i, 8)));
                var second = MemoryMarshal.Read<Vector256<int>>(MemoryMarshal.AsBytes(coefficients.Slice(i + 8, 8)));
                var words = Vector256.Narrow(first, second);
                var scales = MemoryMarshal.Read<Vector256<short>>(MemoryMarshal.AsBytes(quantization.Slice(i, 16)));
                Avx2.MultiplyLow(words, scales).CopyTo(input.Slice(i, 16));
            }
        }
        else
            for (var i = 0; i < 64; i++)
                input[i] = unchecked((short)((long)coefficients[i] * quantization[i]));
        // DC saturates after the original full-width product and level shift;
        // a previously narrowed product cannot replace this calculation.
        input[0] = (short)Math.Clamp((long)coefficients[0] * quantization[0] + 1024, short.MinValue, short.MaxValue);
        return Avx2.IsSupported ? JpegNonzeroMask.Natural(input) : 0;
    }

    private static ulong StoreDequantized(Vector512<short> low, Vector512<short> high,
        int dc, ushort quantizer, Span<short> input)
    {
        low.CopyTo(input); high.CopyTo(input[32..]);
        // Support describes actual modular AC words, not coefficient support.
        // DC instead saturates its full-width product after the level shift.
        var shifted = (short)Math.Clamp((long)dc * quantizer + 1024, short.MinValue, short.MaxValue);
        input[0] = shifted;
        var first = ~Vector512.ExtractMostSignificantBits(Vector512.Equals(low, Vector512<short>.Zero)) & uint.MaxValue;
        var last = ~Vector512.ExtractMostSignificantBits(Vector512.Equals(high, Vector512<short>.Zero)) & uint.MaxValue;
        return (first & ~1UL) | (last << 32) | (shifted != 0 ? 1UL : 0);
    }

    // Inverse of the T.81 diagonal geometry generated in JpegTransform.
    // Compiled format data, without a heap table or image-specific state.
    private static ReadOnlySpan<short> NaturalToScan => [
        0, 1, 5, 6, 14, 15, 27, 28, 2, 4, 7, 13, 16, 26, 29, 42,
        3, 8, 12, 17, 25, 30, 41, 43, 9, 11, 18, 24, 31, 40, 44, 53,
        10, 19, 23, 32, 39, 45, 52, 54, 20, 22, 33, 38, 46, 51, 55, 60,
        21, 34, 37, 47, 50, 56, 59, 61, 35, 36, 48, 49, 57, 58, 62, 63];

    internal static ulong DequantizeScanOrder(ReadOnlySpan<int> coefficients,
        ReadOnlySpan<ushort> quantization, Span<short> input)
    {
        if (Avx512BW.IsSupported)
        {
            // AC retains only the product's low word. Narrowing commutes with
            // permutation and multiplication, so two word permutations replace
            // a natural-order Int32 copy followed by another full-block read.
            var low = Vector512.Narrow(Vector512.Create(coefficients), Vector512.Create(coefficients[16..]));
            var high = Vector512.Narrow(Vector512.Create(coefficients[32..]), Vector512.Create(coefficients[48..]));
            var first = Avx512BW.PermuteVar32x16x2(low, Vector512.Create(NaturalToScan), high);
            var last = Avx512BW.PermuteVar32x16x2(low, Vector512.Create(NaturalToScan[32..]), high);
            first = Avx512BW.MultiplyLow(first, Vector512.Create(quantization).AsInt16());
            last = Avx512BW.MultiplyLow(last, Vector512.Create(quantization[32..]).AsInt16());
            return StoreDequantized(first, last, coefficients[0], quantization[0], input);
        }
        else
            for (var k = 0; k < 64; k++)
            {
                var at = JpegTransform.Zigzag[k];
                input[at] = unchecked((short)((long)coefficients[k] * quantization[at]));
            }
        // DC uses the original full-width coefficient and unsigned quantizer;
        // it saturates after the level shift, independently of AC narrowing.
        input[0] = (short)Math.Clamp((long)coefficients[0] * quantization[0] + 1024, short.MinValue, short.MaxValue);
        return Avx2.IsSupported ? JpegNonzeroMask.Natural(input) : 0;
    }

    private static void InverseAvx2(ReadOnlySpan<short> input, Span<short> rows, Span<byte> pixels, ulong support)
    {
        var twoLowRows = (support >> 16) == 0;
        var horizontal = (support >> 8) == 0;
        // With only horizontal support, every later row transforms to zero.
        // Do not materialize/read an all-zero second row just to rediscover it.
        var rowCount = horizontal ? 1 : twoLowRows ? 2 : 8;
        for (var y = 0; y < rowCount; y++)
        {
            var row = input.Slice(y * 8, 8);
            if (((support >> (y * 8)) & 0xfe) == 0)
            {
                rows.Slice(y * 8, 8).Fill(unchecked((short)(row[0] * 8)));
                continue;
            }
            // Signed-word dot pairs retain the original modulo-Int32 sums.
            var sum = Avx2.Add(Vector256.Create(1 << 10), RowProduct(row, 0, Row01));
            sum = Avx2.Add(sum, RowProduct(row, 2, Row23));
            sum = Avx2.Add(sum, RowProduct(row, 4, Row45));
            sum = Avx2.Add(sum, RowProduct(row, 6, Row67));
            sum = Avx2.ShiftRightArithmetic(sum, 11);
            var packed = Sse2.PackSignedSaturate(sum.GetLower(), sum.GetUpper());
            MemoryMarshal.Write(MemoryMarshal.AsBytes(rows.Slice(y * 8, 8)), in packed);
        }

        // Bias in signed words before widening, preserving the scalar wrap.
        var first = MemoryMarshal.Read<Vector128<short>>(MemoryMarshal.AsBytes(rows[..8]));
        if (twoLowRows)
        {
            // Every omitted row transforms to zero. Keep all sixteen lower
            // frequencies, the original word bias and the same column sums.
            var second = horizontal ? Vector128<short>.Zero
                : MemoryMarshal.Read<Vector128<short>>(MemoryMarshal.AsBytes(rows.Slice(8, 8)));
            if (Vector128.EqualsAll(second, Vector128<short>.Zero))
            {
                // Every odd term is exactly zero. Preserve the existing word
                // bias, modulo-Int32 product and clipping once, then reuse the
                // completed bytes for all eight identical output rows.
                var constant = Avx2.ConvertToVector256Int32(Sse2.Add(first, Vector128.Create((short)32)));
                var even = Avx2.MultiplyLow(constant, Vector256.Create(Kernel[0]));
                StoreRow(even, pixels[..8]);
                var packed = BinaryPrimitives.ReadUInt64LittleEndian(pixels[..8]);
                Vector512.Create(packed).AsByte().CopyTo(pixels);
                return;
            }
            if (Avx512BW.IsSupported)
            {
                StoreTwoRowColumnsAvx512(first, second, pixels);
                return;
            }
            var low0 = Avx2.ConvertToVector256Int32(Sse2.Add(first, Vector128.Create((short)32)));
            var evenLow = Avx2.MultiplyLow(low0, Vector256.Create(Kernel[0]));
            var r1 = Avx2.ConvertToVector256Int32(second);
            for (var y = 0; y < 4; y++)
            {
                var odd = Avx2.MultiplyLow(r1, Vector256.Create(Kernel[8 + y]));
                StoreRow(Avx2.Add(evenLow, odd), pixels.Slice(y * 8, 8));
                StoreRow(Avx2.Subtract(evenLow, odd), pixels.Slice((7 - y) * 8, 8));
            }
            return;
        }
        var r0 = Avx2.ConvertToVector256Int32(Sse2.Add(first, Vector128.Create((short)32)));
        var r4 = WidenRow(rows, 4);
        var p26 = PairRows(rows, 2, 6);
        var p13 = PairRows(rows, 1, 3);
        var p57 = PairRows(rows, 5, 7);
        // T.81 A.3.3: reflected outputs share even frequencies and negate odd
        // frequencies. DC/quarter-turn weights also share the same magnitude.
        // All operations remain modulo Int32, so regrouping preserves even
        // overflowing sums. Keep the signed-word DC bias above this pairing.
        var unit = Vector256.Create(Kernel[0]);
        var plus = Avx2.MultiplyLow(Avx2.Add(r0, r4), unit);
        var minus = Avx2.MultiplyLow(Avx2.Subtract(r0, r4), unit);
        for (var y = 0; y < 4; y++)
        {
            var even = y is 0 or 3 ? plus : minus;
            even = Avx2.Add(even, Avx2.MultiplyAddAdjacent(p26, PairWeights(Kernel[16 + y], Kernel[48 + y])));
            var odd = Avx2.Add(
                Avx2.MultiplyAddAdjacent(p13, PairWeights(Kernel[8 + y], Kernel[24 + y])),
                Avx2.MultiplyAddAdjacent(p57, PairWeights(Kernel[40 + y], Kernel[56 + y])));
            StoreRow(Avx2.Add(even, odd), pixels.Slice(y * 8, 8));
            StoreRow(Avx2.Subtract(even, odd), pixels.Slice((7 - y) * 8, 8));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void StoreTwoRowColumnsAvx512(Vector128<short> first, Vector128<short> second, Span<byte> pixels)
    {
        // The caller validates the complete block. Bias wraps in signed words
        // before the pair products, just as in the AVX2/scalar column paths.
        first = Sse2.Add(first, Vector128.Create((short)32));
        var pair = Vector256.Create(Sse2.UnpackLow(first, second), Sse2.UnpackHigh(first, second));
        var pairs = Vector512.Create(pair, pair);
        var a = Avx512F.ShiftRightArithmetic(Avx512BW.MultiplyAddAdjacent(pairs, Column01), 20);
        var b = Avx512F.ShiftRightArithmetic(Avx512BW.MultiplyAddAdjacent(pairs, Column23), 20);
        var c = Avx512F.ShiftRightArithmetic(Avx512BW.MultiplyAddAdjacent(pairs, Column45), 20);
        var d = Avx512F.ShiftRightArithmetic(Avx512BW.MultiplyAddAdjacent(pairs, Column67), 20);
        // Shifted Int32 values are within [-2048,2047], so narrowing to words
        // is exact. The unsigned saturating pack performs final byte clipping.
        var top = Vector512.Narrow(a, b);
        var bottom = Vector512.Narrow(c, d);
        var packed = Avx512BW.PackUnsignedSaturate(top, bottom).AsUInt64();
        var ordered = Avx512F.PermuteVar8x64(packed, Vector512.Create(0ul, 2ul, 4ul, 6ul, 1ul, 3ul, 5ul, 7ul));
        MemoryMarshal.Write(pixels, in ordered);
    }

    private static Vector256<int> RowProduct(ReadOnlySpan<short> row, int first, Vector256<short> weights)
    {
        var pair = MemoryMarshal.Read<int>(MemoryMarshal.AsBytes(row.Slice(first, 2)));
        return Avx2.MultiplyAddAdjacent(Vector256.Create(pair).AsInt16(), weights);
    }

    private static Vector256<short> PairRows(ReadOnlySpan<short> rows, int first, int second)
    {
        var a = MemoryMarshal.Read<Vector128<short>>(MemoryMarshal.AsBytes(rows.Slice(first * 8, 8)));
        var b = MemoryMarshal.Read<Vector128<short>>(MemoryMarshal.AsBytes(rows.Slice(second * 8, 8)));
        return Vector256.Create(Sse2.UnpackLow(a, b), Sse2.UnpackHigh(a, b));
    }

    private static Vector256<short> PairWeights(int first, int second)
        => Vector256.Create(unchecked((int)((uint)(ushort)first | ((uint)(ushort)second << 16)))).AsInt16();

    private static void StoreRow(Vector256<int> sum, Span<byte> pixels)
    {
        sum = Avx2.ShiftRightArithmetic(sum, 20);
        var words = Sse2.PackSignedSaturate(sum.GetLower(), sum.GetUpper());
        var bytes = Sse2.PackUnsignedSaturate(words, words);
        BinaryPrimitives.WriteUInt64LittleEndian(pixels, bytes.AsUInt64().ToScalar());
    }

    private static Vector256<int> WidenRow(ReadOnlySpan<short> rows, int y)
        => Avx2.ConvertToVector256Int32(MemoryMarshal.Read<Vector128<short>>(MemoryMarshal.AsBytes(rows.Slice(y * 8, 8))));
}
