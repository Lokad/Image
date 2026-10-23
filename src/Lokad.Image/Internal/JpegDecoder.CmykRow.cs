using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    internal static void RenderCmykRow(ReadOnlySpan<byte> cyan, ReadOnlySpan<byte> magenta, ReadOnlySpan<byte> yellow,
        ReadOnlySpan<byte> black, bool inverted, Span<byte> output)
    {
        if (magenta.Length != cyan.Length || yellow.Length != cyan.Length || black.Length != cyan.Length || output.Length != cyan.Length * 4L)
            throw new ArgumentException("Full-resolution CMYK rows require four equal planes and four output bytes per pixel.");
        var x = 0;
        if (Avx2.IsSupported)
        {
            for (; x <= cyan.Length - 16; x += 16)
            {
                var c = Avx2.ConvertToVector256Int16(Vector128.Create(cyan.Slice(x, 16))).AsUInt16();
                var m = Avx2.ConvertToVector256Int16(Vector128.Create(magenta.Slice(x, 16))).AsUInt16();
                var y = Avx2.ConvertToVector256Int16(Vector128.Create(yellow.Slice(x, 16))).AsUInt16();
                var k = Avx2.ConvertToVector256Int16(Vector128.Create(black.Slice(x, 16))).AsUInt16();
                if (!inverted)
                {
                    var full = Vector256.Create((ushort)255);
                    c = Avx2.Subtract(full, c); m = Avx2.Subtract(full, m); y = Avx2.Subtract(full, y); k = Avx2.Subtract(full, k);
                }
                var r = Product(Avx2.MultiplyLow(c, k)); var g = Product(Avx2.MultiplyLow(m, k)); var b = Product(Avx2.MultiplyLow(y, k));
                Pack(r.GetLower(), g.GetLower(), b.GetLower(), output.Slice(x * 4, 32));
                Pack(r.GetUpper(), g.GetUpper(), b.GetUpper(), output.Slice(x * 4 + 32, 32));
            }
        }
        for (; x < cyan.Length; x++)
        {
            var k = inverted ? black[x] : 255 - black[x];
            output[x * 4] = (byte)(JpegBlackColor.Product(inverted ? cyan[x] : 255 - cyan[x], k));
            output[x * 4 + 1] = (byte)(JpegBlackColor.Product(inverted ? magenta[x] : 255 - magenta[x], k));
            output[x * 4 + 2] = (byte)(JpegBlackColor.Product(inverted ? yellow[x] : 255 - yellow[x], k)); output[x * 4 + 3] = 255;
        }

        static Vector256<ushort> Product(Vector256<ushort> product)
            => Avx2.MultiplyHigh(product, Vector256.Create(JpegBlackColor.ProductScale));
        static void Pack(Vector128<ushort> red, Vector128<ushort> green, Vector128<ushort> blue, Span<byte> target)
        {
            var r = Avx2.ConvertToVector256Int32(red.AsInt16()).AsUInt32();
            var g = Avx2.ShiftLeftLogical(Avx2.ConvertToVector256Int32(green.AsInt16()).AsUInt32(), 8);
            var b = Avx2.ShiftLeftLogical(Avx2.ConvertToVector256Int32(blue.AsInt16()).AsUInt32(), 16);
            Avx2.Or(Avx2.Or(r, g), Avx2.Or(b, Vector256.Create(0xff000000u))).AsByte().CopyTo(target);
        }
    }
}
