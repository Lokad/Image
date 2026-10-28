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
        for (var i = 0; i < 64; i++)
            input[i] = unchecked((short)((long)coefficients[i] * quantization[i]));
        // Level shift belongs to the dequantized DC input. Narrow AC modulo
        // 16 bits, but saturate DC after adding the eight-bit sample midpoint.
        input[0] = (short)Math.Clamp((long)coefficients[0] * quantization[0] + 1024, short.MinValue, short.MaxValue);
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

    private static void InverseAvx2(ReadOnlySpan<short> input, Span<short> rows, Span<byte> pixels)
    {
        for (var y = 0; y < 8; y++)
        {
            var row = input.Slice(y * 8, 8);
            if (!row[1..].ContainsAnyExcept((short)0))
            {
                rows.Slice(y * 8, 8).Fill(unchecked((short)(row[0] * 8)));
                continue;
            }
            var sum = Vector256.Create(1 << 10);
            for (var u = 0; u < 8; u++)
            {
                var weights = MemoryMarshal.Read<Vector256<int>>(MemoryMarshal.AsBytes(Kernel.AsSpan(u * 8, 8)));
                sum = Avx2.Add(sum, Avx2.MultiplyLow(Vector256.Create((int)row[u]), weights));
            }
            sum = Avx2.ShiftRightArithmetic(sum, 11);
            var packed = Sse2.PackSignedSaturate(sum.GetLower(), sum.GetUpper());
            MemoryMarshal.Write(MemoryMarshal.AsBytes(rows.Slice(y * 8, 8)), in packed);
        }

        // Bias in signed words before widening, preserving the scalar wrap.
        var first = MemoryMarshal.Read<Vector128<short>>(MemoryMarshal.AsBytes(rows[..8]));
        var r0 = Avx2.ConvertToVector256Int32(Sse2.Add(first, Vector128.Create((short)32)));
        var r1 = WidenRow(rows, 1); var r2 = WidenRow(rows, 2); var r3 = WidenRow(rows, 3);
        var r4 = WidenRow(rows, 4); var r5 = WidenRow(rows, 5); var r6 = WidenRow(rows, 6); var r7 = WidenRow(rows, 7);
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
            even = Avx2.Add(even, Avx2.MultiplyLow(r2, Vector256.Create(Kernel[16 + y])));
            even = Avx2.Add(even, Avx2.MultiplyLow(r6, Vector256.Create(Kernel[48 + y])));
            var odd = Avx2.MultiplyLow(r1, Vector256.Create(Kernel[8 + y]));
            odd = Avx2.Add(odd, Avx2.MultiplyLow(r3, Vector256.Create(Kernel[24 + y])));
            odd = Avx2.Add(odd, Avx2.MultiplyLow(r5, Vector256.Create(Kernel[40 + y])));
            odd = Avx2.Add(odd, Avx2.MultiplyLow(r7, Vector256.Create(Kernel[56 + y])));
            StoreRow(Avx2.Add(even, odd), pixels.Slice(y * 8, 8));
            StoreRow(Avx2.Subtract(even, odd), pixels.Slice((7 - y) * 8, 8));
        }
    }

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
