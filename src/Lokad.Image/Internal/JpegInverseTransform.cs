using System.Buffers.Binary;
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
        Span<byte> pixels, OperationContext context)
    {
        if (coefficients.Length != 64 || quantization.Length != 64 || pixels.Length != 64)
            throw new ArgumentException("JPEG transforms operate on complete 8x8 blocks.");
        context.Cancellation.ThrowIfCancellationRequested();
        Span<short> input = stackalloc short[64];
        Span<short> rows = stackalloc short[64];
        Dequantize(coefficients, quantization, input);
        if (!input[1..].ContainsAnyExcept((short)0))
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
            InverseAvx2(input, rows, pixels);
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
    internal static void Dequantize(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization, Span<short> input)
    {
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
    }

    private static void InverseAvx2(ReadOnlySpan<short> input, Span<short> rows, Span<byte> pixels)
    {
        var twoLowRows = !input[16..].ContainsAnyExcept((short)0);
        var rowCount = twoLowRows ? 2 : 8;
        for (var y = 0; y < rowCount; y++)
        {
            var row = input.Slice(y * 8, 8);
            if (!row[1..].ContainsAnyExcept((short)0))
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
        var r0 = Avx2.ConvertToVector256Int32(Sse2.Add(first, Vector128.Create((short)32)));
        if (twoLowRows)
        {
            // Every omitted row transforms to zero. Keep all sixteen lower
            // frequencies, the original word bias and the same column sums.
            var even = Avx2.MultiplyLow(r0, Vector256.Create(Kernel[0]));
            var r1 = WidenRow(rows, 1);
            if (Vector256.EqualsAll(r1, Vector256<int>.Zero))
            {
                // Every odd term is exactly zero. Preserve the existing word
                // bias, modulo-Int32 product and clipping once, then reuse the
                // completed bytes for all eight identical output rows.
                StoreRow(even, pixels[..8]);
                var packed = BinaryPrimitives.ReadUInt64LittleEndian(pixels[..8]);
                for (var y = 1; y < 8; y++)
                    BinaryPrimitives.WriteUInt64LittleEndian(pixels.Slice(y * 8, 8), packed);
                return;
            }
            for (var y = 0; y < 4; y++)
            {
                var odd = Avx2.MultiplyLow(r1, Vector256.Create(Kernel[8 + y]));
                StoreRow(Avx2.Add(even, odd), pixels.Slice(y * 8, 8));
                StoreRow(Avx2.Subtract(even, odd), pixels.Slice((7 - y) * 8, 8));
            }
            return;
        }
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
