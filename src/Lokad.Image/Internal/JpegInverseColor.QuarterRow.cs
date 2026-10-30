using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegInverseColor
{
    internal static void WriteFilteredQuarterRow(ReadOnlySpan<byte> luma, ReadOnlySpan<ushort> cbTop, ReadOnlySpan<ushort> cbBottom,
        ReadOnlySpan<ushort> crTop, ReadOnlySpan<ushort> crBottom, int bottomWeight, Span<byte> output)
    {
        if ((uint)bottomWeight > 4096) throw new ArgumentOutOfRangeException(nameof(bottomWeight));
        if (cbTop.Length != luma.Length || cbBottom.Length != luma.Length || crTop.Length != luma.Length
            || crBottom.Length != luma.Length || output.Length != luma.Length * 4L)
            throw new ArgumentException("Quarter-chroma rows require equal Q7 planes and four output bytes per pixel.");
        var x = 0;
        if (Avx2.IsSupported)
        {
            var top = Vector256.Create(4096 - bottomWeight); var bottom = Vector256.Create(bottomWeight);
            var center = Vector256.Create(128 << 9); var bias = Vector256.Create(1 << 21);
            var red = Vector256.Create(Red); var blue = Vector256.Create(Blue);
            var greenU = Vector256.Create(GreenU); var greenV = Vector256.Create(GreenV);
            for (; x <= luma.Length - 8; x += 8)
            {
                var u = Avx2.Subtract(Avx2.ShiftRightArithmetic(Avx2.Add(
                    Avx2.MultiplyLow(Avx2.ConvertToVector256Int32(Vector128.Create(cbTop.Slice(x, 8))), top),
                    Avx2.MultiplyLow(Avx2.ConvertToVector256Int32(Vector128.Create(cbBottom.Slice(x, 8))), bottom)), 10), center);
                var v = Avx2.Subtract(Avx2.ShiftRightArithmetic(Avx2.Add(
                    Avx2.MultiplyLow(Avx2.ConvertToVector256Int32(Vector128.Create(crTop.Slice(x, 8))), top),
                    Avx2.MultiplyLow(Avx2.ConvertToVector256Int32(Vector128.Create(crBottom.Slice(x, 8))), bottom)), 10), center);
                var y = Avx2.Add(Avx2.ShiftLeftLogical(WidenEight(luma, x), 22), bias);
                var r = Clip(Avx2.ShiftRightArithmetic(Avx2.Add(y, Avx2.MultiplyLow(v, red)), 22));
                var g = Clip(Avx2.ShiftRightArithmetic(Avx2.Add(Avx2.Add(y, Avx2.MultiplyLow(u, greenU)), Avx2.MultiplyLow(v, greenV)), 22));
                var b = Clip(Avx2.ShiftRightArithmetic(Avx2.Add(y, Avx2.MultiplyLow(u, blue)), 22));
                Avx2.Or(Avx2.Or(r.AsUInt32(), Avx2.ShiftLeftLogical(g.AsUInt32(), 8)),
                    Avx2.Or(Avx2.ShiftLeftLogical(b.AsUInt32(), 16), Vector256.Create(0xFF000000u))).AsByte().CopyTo(output.Slice(x * 4, 32));
            }
        }
        for (; x < luma.Length; x++)
        {
            var u = (cbTop[x] * (4096 - bottomWeight) + cbBottom[x] * bottomWeight - (128 << 19)) >> 10;
            var v = (crTop[x] * (4096 - bottomWeight) + crBottom[x] * bottomWeight - (128 << 19)) >> 10;
            WriteFilteredPixel(luma[x], u, v, output.Slice(x * 4, 4), 4);
        }
    }
}
