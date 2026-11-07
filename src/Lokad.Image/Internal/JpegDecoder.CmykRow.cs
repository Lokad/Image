using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    // The private full-resolution result holds four raw components until EOI.
    // Read every lane before replacing K with opaque alpha. Keep the pinned
    // Q16 reciprocal, including its deliberately non-rounded white endpoint.
    internal static void RenderCmykInPlace(Span<byte> pixels, bool inverted)
    {
        if (pixels.Length % 4 != 0) throw new ArgumentException("CMYK pixels require four bytes per pixel.");
        var offset = 0;
        if (Avx2.IsSupported)
        {
            var mask = Vector256.Create(255u);
            var scale = Vector256.Create((uint)JpegBlackColor.ProductScale);
            for (; offset <= pixels.Length - 32; offset += 32)
            {
                var raw = Vector256.Create(pixels.Slice(offset, 32)).AsUInt32();
                if (!inverted) raw = ~raw;
                var black = Avx2.ShiftRightLogical(raw, 24);
                var red = Product(raw & mask, black, scale);
                var green = Avx2.ShiftLeftLogical(Product(Avx2.ShiftRightLogical(raw, 8) & mask, black, scale), 8);
                var blue = Avx2.ShiftLeftLogical(Product(Avx2.ShiftRightLogical(raw, 16) & mask, black, scale), 16);
                (red | green | blue | Vector256.Create(0xff000000u)).AsByte().CopyTo(pixels.Slice(offset, 32));
            }
        }
        for (; offset < pixels.Length; offset += 4)
        {
            var pixel = pixels.Slice(offset, 4);
            JpegBlackColor.WriteCmyk(pixel[0], pixel[1], pixel[2], pixel[3], inverted, pixel, 4);
        }

        static Vector256<uint> Product(Vector256<uint> value, Vector256<uint> black, Vector256<uint> scale)
            => Avx2.ShiftRightLogical(Avx2.MultiplyLow(Avx2.MultiplyLow(value, black), scale), 16);
    }

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
