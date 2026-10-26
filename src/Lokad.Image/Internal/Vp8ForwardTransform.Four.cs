using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Four independent prediction candidates share the exact integer forward kernel.
// Each 128-bit lane holds one 4x4 block; RFC6386 section14 supplies geometry.
internal static partial class Vp8ForwardTransform
{
    internal static bool SupportsFour => Avx512F.IsSupported && Avx512BW.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Transpose(ref Vector512<int> r0, ref Vector512<int> r1, ref Vector512<int> r2, ref Vector512<int> r3)
    {
        var a = Avx512F.UnpackLow(r0, r1); var b = Avx512F.UnpackHigh(r0, r1);
        var c = Avx512F.UnpackLow(r2, r3); var d = Avx512F.UnpackHigh(r2, r3);
        r0 = Avx512F.UnpackLow(a.AsInt64(), c.AsInt64()).AsInt32();
        r1 = Avx512F.UnpackHigh(a.AsInt64(), c.AsInt64()).AsInt32();
        r2 = Avx512F.UnpackLow(b.AsInt64(), d.AsInt64()).AsInt32();
        r3 = Avx512F.UnpackHigh(b.AsInt64(), d.AsInt64()).AsInt32();
    }

    static void Transform(Vector512<int> r0, Vector512<int> r1, Vector512<int> r2, Vector512<int> r3, Span<short> output)
    {
        Transpose(ref r0, ref r1, ref r2, ref r3);
        var a = r0 + r3; var b = r1 + r2; var c = r0 - r3; var d = r1 - r2;
        var sine = Vector512.Create(Sine); var cosine = Vector512.Create(Cosine);
        r0 = Avx512F.ShiftLeftLogical(a + b, 3);
        r1 = Avx512F.ShiftRightArithmetic(Avx512F.MultiplyLow(c, cosine) + Avx512F.MultiplyLow(d, sine) + Vector512.Create(1812), 9);
        r2 = Avx512F.ShiftLeftLogical(a - b, 3);
        r3 = Avx512F.ShiftRightArithmetic(Avx512F.MultiplyLow(c, sine) - Avx512F.MultiplyLow(d, cosine) + Vector512.Create(937), 9);
        Transpose(ref r0, ref r1, ref r2, ref r3);
        a = r0 + r3; b = r1 + r2; c = r0 - r3; d = r1 - r2;
        r0 = Avx512F.ShiftRightArithmetic(a + b + Vector512.Create(7), 4);
        var correction = Avx512F.AndNot(Avx512F.CompareEqual(c, Vector512<int>.Zero), Vector512.Create(1));
        r1 = Avx512F.ShiftRightArithmetic(Avx512F.MultiplyLow(c, cosine) + Avx512F.MultiplyLow(d, sine) + Vector512.Create(12000), 16) + correction;
        r2 = Avx512F.ShiftRightArithmetic(a - b + Vector512.Create(7), 4);
        r3 = Avx512F.ShiftRightArithmetic(Avx512F.MultiplyLow(c, sine) - Avx512F.MultiplyLow(d, cosine) + Vector512.Create(51000), 16);
        var first = Avx512BW.PackSignedSaturate(r0, r1).AsInt64();
        var second = Avx512BW.PackSignedSaturate(r2, r3).AsInt64();
        Avx512F.PermuteVar8x64x2(first, Vector512.Create(0L, 1, 8, 9, 2, 3, 10, 11), second).AsInt16().CopyTo(output);
        Avx512F.PermuteVar8x64x2(first, Vector512.Create(4L, 5, 12, 13, 6, 7, 14, 15), second).AsInt16().CopyTo(output[32..]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector512<int> ResidualRow(ReadOnlySpan<byte> source, ReadOnlySpan<byte> predictions, int row)
    {
        var word = BinaryPrimitives.ReadUInt32LittleEndian(source);
        var four = Vector128.Create(BinaryPrimitives.ReadUInt32LittleEndian(predictions[row..]),
            BinaryPrimitives.ReadUInt32LittleEndian(predictions[(16 + row)..]),
            BinaryPrimitives.ReadUInt32LittleEndian(predictions[(32 + row)..]),
            BinaryPrimitives.ReadUInt32LittleEndian(predictions[(48 + row)..]));
        return Avx512F.ConvertToVector512Int32(Vector128.Create(word).AsByte()) - Avx512F.ConvertToVector512Int32(four.AsByte());
    }

    // Borrow four compact predictions and a strided 4x4 source; publish four
    // compact coefficient blocks after all input reads. No heap storage or extra
    // boundary: the enclosing macroblock already admits its fixed stack space.
    internal static void DctFourPredictions(ReadOnlySpan<byte> source, int stride, ReadOnlySpan<byte> predictions, Span<short> output)
    {
        if (stride < 4 || (long)stride * 3 + 4 > source.Length || predictions.Length < 64 || output.Length < 64) throw new ArgumentException();
        if (!SupportsFour)
        {
            Span<short> residual = stackalloc short[64];
            for (var p = 0; p < 4; p++) for (var y = 0; y < 4; y++) for (var x = 0; x < 4; x++)
                residual[p * 16 + y * 4 + x] = (short)(source[y * stride + x] - predictions[p * 16 + y * 4 + x]);
            for (var p = 0; p < 4; p++) Dct(residual.Slice(p * 16, 16), output.Slice(p * 16, 16));
            return;
        }
        var r0 = ResidualRow(source, predictions, 0); var r1 = ResidualRow(source[stride..], predictions, 4);
        var r2 = ResidualRow(source[(stride * 2)..], predictions, 8); var r3 = ResidualRow(source[(stride * 3)..], predictions, 12);
        Transform(r0, r1, r2, r3, output);
    }
}
