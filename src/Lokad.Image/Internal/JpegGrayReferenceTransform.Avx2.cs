using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegGrayReferenceTransform
{
    private static Vector256<int> MultiplyAvx2(Vector256<int> x, int c) => Avx2.MultiplyLow(x, Vector256.Create(c));
    private static Vector256<int> RoundAvx2(Vector256<int> x, bool row) => row
        ? Avx2.ShiftRightArithmetic(x + Vector256.Create(1024), 11)
        : Avx2.ShiftRightArithmetic(x + Vector256.Create(16384), 15);

    private static void TransformAvx2(Span<Vector256<int>> v, bool row)
    {
        var a0 = v[0] + v[7]; var a1 = v[1] + v[6]; var a2 = v[2] + v[5]; var a3 = v[3] + v[4];
        var t0 = a0 + a3; var t1 = a1 + a2; var t2 = a1 - a2; var t3 = a0 - a3;
        var rotation = MultiplyAvx2(t2 + t3, Rotation);
        var d0 = v[3] - v[4]; var d1 = v[2] - v[5]; var d2 = v[1] - v[6]; var d3 = v[0] - v[7];
        var common = MultiplyAvx2(d0 + d1 + d2 + d3, OddBase);
        var p = MultiplyAvx2(d0 + d3, Odd03); var q = MultiplyAvx2(d1 + d2, Odd12);
        var r = MultiplyAvx2(d0 + d2, Odd02); var s = MultiplyAvx2(d1 + d3, Odd13);
        v[0] = RoundAvx2(Avx2.ShiftLeftLogical(t0 + t1, 13), row);
        v[1] = RoundAvx2(common + p + s + MultiplyAvx2(d3, Extra0), row);
        v[2] = RoundAvx2(rotation + MultiplyAvx2(t3, Near), row);
        v[3] = RoundAvx2(common + q + r + MultiplyAvx2(d2, Extra1), row);
        v[4] = RoundAvx2(Avx2.ShiftLeftLogical(t0 - t1, 13), row);
        v[5] = RoundAvx2(common + q + s + MultiplyAvx2(d1, Extra2), row);
        v[6] = RoundAvx2(rotation - MultiplyAvx2(t2, Far), row);
        v[7] = RoundAvx2(common + p + r + MultiplyAvx2(d0, Extra3), row);
    }

    private static void TransposeAvx2(Span<Vector256<int>> v)
    {
        var a0 = Avx2.UnpackLow(v[0], v[1]); var a1 = Avx2.UnpackHigh(v[0], v[1]);
        var a2 = Avx2.UnpackLow(v[2], v[3]); var a3 = Avx2.UnpackHigh(v[2], v[3]);
        var a4 = Avx2.UnpackLow(v[4], v[5]); var a5 = Avx2.UnpackHigh(v[4], v[5]);
        var a6 = Avx2.UnpackLow(v[6], v[7]); var a7 = Avx2.UnpackHigh(v[6], v[7]);
        var b0 = Avx2.UnpackLow(a0.AsInt64(), a2.AsInt64()).AsInt32();
        var b1 = Avx2.UnpackHigh(a0.AsInt64(), a2.AsInt64()).AsInt32();
        var b2 = Avx2.UnpackLow(a1.AsInt64(), a3.AsInt64()).AsInt32();
        var b3 = Avx2.UnpackHigh(a1.AsInt64(), a3.AsInt64()).AsInt32();
        var b4 = Avx2.UnpackLow(a4.AsInt64(), a6.AsInt64()).AsInt32();
        var b5 = Avx2.UnpackHigh(a4.AsInt64(), a6.AsInt64()).AsInt32();
        var b6 = Avx2.UnpackLow(a5.AsInt64(), a7.AsInt64()).AsInt32();
        var b7 = Avx2.UnpackHigh(a5.AsInt64(), a7.AsInt64()).AsInt32();
        v[0] = Avx2.Permute2x128(b0, b4, 0x20); v[1] = Avx2.Permute2x128(b1, b5, 0x20);
        v[2] = Avx2.Permute2x128(b2, b6, 0x20); v[3] = Avx2.Permute2x128(b3, b7, 0x20);
        v[4] = Avx2.Permute2x128(b0, b4, 0x31); v[5] = Avx2.Permute2x128(b1, b5, 0x31);
        v[6] = Avx2.Permute2x128(b2, b6, 0x31); v[7] = Avx2.Permute2x128(b3, b7, 0x31);
    }

    private static void ForwardAvx2(ReadOnlySpan<byte> samples, Span<short> output)
    {
        Span<Vector256<int>> rows = stackalloc Vector256<int>[8];
        for (var y = 0; y < 8; y++)
            rows[y] = Avx2.ConvertToVector256Int32(Vector128.CreateScalar(MemoryMarshal.Read<ulong>(samples[(y * 8)..])).AsByte()) - Vector256.Create(128);
        TransposeAvx2(rows); TransformAvx2(rows, true); TransposeAvx2(rows); TransformAvx2(rows, false);
        // The qualified full-byte-domain bound is8195, so signed saturation
        // publishes exactly the existing signed16 cast. All input is snapshotted.
        for (var y = 0; y < 8; y++) Sse2.PackSignedSaturate(rows[y].GetLower(), rows[y].GetUpper()).CopyTo(output[(y * 8)..]);
    }
}
