using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static class JpegGrayQuantization
{
    internal static void Prepare(ReadOnlySpan<ushort> table, Span<ushort> reciprocals)
    {
        if (table.Length != 64 || reciprocals.Length != 64)
            throw new ArgumentException("Complete quantization blocks required.");
        for (var k = 0; k < 64; k++)
        {
            if (table[k] is 0 or > 255) throw new ArgumentException("8-bit quantization required.");
            var divisor = table[k] * 8;
            reciprocals[k] = (ushort)((65536 + divisor - 1) / divisor);
        }
    }

    internal static void Quantize(ReadOnlySpan<short> source, ReadOnlySpan<ushort> table,
        ReadOnlySpan<ushort> reciprocals, Span<short> output)
    {
        if (source.Length != 64 || table.Length != 64 || reciprocals.Length != 64 || output.Length != 64)
            throw new ArgumentException("Complete blocks and prepared quantization required.");
        if (Avx2.IsSupported)
        {
            for (var at = 0; at < 64; at += 16)
            {
                var value = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(source), (nuint)at);
                var sign = Avx2.ShiftRightArithmetic(value, 15);
                var magnitude = Avx2.Subtract(Avx2.Xor(value, sign), sign).AsUInt16();
                var divisor = Avx2.ShiftLeftLogical(Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(table), (nuint)at), 3);
                var numerator = Avx2.Add(magnitude, Avx2.ShiftRightLogical(divisor, 1));
                var candidate = Avx2.MultiplyHigh(numerator, Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(reciprocals), (nuint)at));
                // n<=33788<65536 and r=ceil(65536/d): the candidate is exact
                // or one high. candidate*d<65536; its positive excess<2040
                // fits signed16. Correct once before restoring the sign.
                var excess = Avx2.SubtractSaturate(Avx2.MultiplyLow(candidate, divisor), numerator);
                var correction = Avx2.And(Avx2.CompareGreaterThan(excess.AsInt16(), Vector256<short>.Zero).AsUInt16(), Vector256.Create((ushort)1));
                var quantized = Avx2.Subtract(candidate, correction).AsInt16();
                Avx2.Subtract(Avx2.Xor(quantized, sign), sign).StoreUnsafe(ref MemoryMarshal.GetReference(output), (nuint)at);
            }
            return;
        }
        for (var k = 0; k < 64; k++)
        {
            int value = source[k], divisor = table[k] * 8;
            var magnitude = (Math.Abs(value) + divisor / 2) / divisor;
            output[k] = (short)(value < 0 ? -magnitude : magnitude);
        }
    }
}
