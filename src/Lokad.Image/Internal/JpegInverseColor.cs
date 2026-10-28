using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Full-range BT.601 conversion at the pinned reference's integer precision.
// Derive coefficients from Kr=299/1000 and Kb=114/1000, rather than incorporating
// a native conversion table. Callers check row cancellation and own all supplied spans.
internal static class JpegInverseColor
{
    private static readonly int Red = Coefficient(2 * (1000 - 299), 1000, false);
    private static readonly int Blue = Coefficient(2 * (1000 - 114), 1000, false);
    private static readonly int GreenU = Coefficient(2L * 114 * (1000 - 114), 1000L * (1000 - 299 - 114), true);
    private static readonly int GreenV = Coefficient(2L * 299 * (1000 - 299), 1000L * (1000 - 299 - 114), true);

    private static int Coefficient(long numerator, long denominator, bool negative)
    {
        // The pinned conversion first rounds a studio-chroma Q16 coefficient,
        // then removes that range expansion with integer division, then rounds
        // to Q13. Preserve each precision boundary, including signed rounding.
        var expanded = (numerator * 255 * 65536 + denominator * 112) / (denominator * 224);
        var fullRange = (negative ? -expanded : expanded) * 224 / 255;
        return (int)((fullRange + 4) >> 3);
    }

    internal static void WriteRow(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> cb, ReadOnlySpan<byte> cr,
        Span<byte> destination, int channels)
    {
        if (channels is not (3 or 4)) throw new ArgumentOutOfRangeException(nameof(channels));
        var required = (long)luma.Length * channels;
        if (cb.Length != luma.Length || cr.Length != luma.Length || required > destination.Length)
            throw new ArgumentException("Color planes and destination must cover the complete row.");
        var target = destination[..(int)required];
        if (luma.Overlaps(target) || cb.Overlaps(target) || cr.Overlaps(target))
            throw new ArgumentException("Color inputs and destination must not overlap.");
        var x = 0;
        if (luma.Length >= 8 && Avx2.IsSupported && (channels == 4 || Ssse3.IsSupported))
        {
            var red = Vector256.Create(Red); var blue = Vector256.Create(Blue);
            var greenU = Vector256.Create(GreenU); var greenV = Vector256.Create(GreenV);
            var center = Vector256.Create(128); var bias = Vector256.Create(1 << 12);
            var rgb = Vector128.Create((byte)0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 128, 128, 128, 128);
            for (; x + 8 <= luma.Length; x += 8)
            {
                var y = Avx2.Add(Avx2.ShiftLeftLogical(WidenEight(luma, x), 13), bias);
                var u = Avx2.Subtract(WidenEight(cb, x), center);
                var v = Avx2.Subtract(WidenEight(cr, x), center);
                var r = Clip(Avx2.ShiftRightArithmetic(Avx2.Add(y, Avx2.MultiplyLow(v, red)), 13));
                var g = Clip(Avx2.ShiftRightArithmetic(Avx2.Add(Avx2.Add(y, Avx2.MultiplyLow(u, greenU)),
                    Avx2.MultiplyLow(v, greenV)), 13));
                var b = Clip(Avx2.ShiftRightArithmetic(Avx2.Add(y, Avx2.MultiplyLow(u, blue)), 13));
                var packed = Avx2.Or(Avx2.Or(r.AsUInt32(), Avx2.ShiftLeftLogical(g.AsUInt32(), 8)),
                    Avx2.Or(Avx2.ShiftLeftLogical(b.AsUInt32(), 16), Vector256.Create(0xFF000000u))).AsByte();
                if (channels == 4) packed.CopyTo(target.Slice(x * 4, 32));
                else
                {
                    // Each shuffled four-pixel group has twelve RGB bytes.
                    // Store only those bytes, preserving caller row padding.
                    StoreRgb12(target.Slice(x * 3, 12), Ssse3.Shuffle(packed.GetLower(), rgb));
                    StoreRgb12(target.Slice(x * 3 + 12, 12), Ssse3.Shuffle(packed.GetUpper(), rgb));
                }
            }
        }
        for (; x < luma.Length; x++)
        {
            var y = (luma[x] << 13) + (1 << 12);
            var u = cb[x] - 128; var v = cr[x] - 128;
            target[x * channels] = (byte)Math.Clamp((y + v * Red) >> 13, 0, 255);
            target[x * channels + 1] = (byte)Math.Clamp((y + u * GreenU + v * GreenV) >> 13, 0, 255);
            target[x * channels + 2] = (byte)Math.Clamp((y + u * Blue) >> 13, 0, 255);
            if (channels == 4) target[x * channels + 3] = 255;
        }
    }

    private static Vector256<int> WidenEight(ReadOnlySpan<byte> row, int x)
        => Avx2.ConvertToVector256Int32(Vector128.CreateScalar(BinaryPrimitives.ReadUInt64LittleEndian(row.Slice(x, 8))).AsByte());

    private static Vector256<int> Clip(Vector256<int> value)
        => Avx2.Min(Avx2.Max(value, Vector256<int>.Zero), Vector256.Create(255));

    private static void StoreRgb12(Span<byte> target, Vector128<byte> packed)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(target, packed.AsUInt64().ToScalar());
        BinaryPrimitives.WriteUInt32LittleEndian(target[8..], packed.AsUInt32().GetElement(2));
    }

    internal static void WriteFilteredPixel(byte luma, int cbQ9, int crQ9, Span<byte> destination, int channels)
    {
        // Filtered chroma remains centered Q9 through Q13 multiplication.
        // Quantize only the final RGB channels; the byte-plane path is unchanged.
        if (channels is not (3 or 4)) throw new ArgumentOutOfRangeException(nameof(channels));
        if (destination.Length < channels) throw new ArgumentException("Destination must cover one color pixel.");
        var y = (luma << 22) + (1 << 21);
        destination[0] = (byte)Math.Clamp((y + crQ9 * Red) >> 22, 0, 255);
        destination[1] = (byte)Math.Clamp((y + cbQ9 * GreenU + crQ9 * GreenV) >> 22, 0, 255);
        destination[2] = (byte)Math.Clamp((y + cbQ9 * Blue) >> 22, 0, 255);
        if (channels == 4) destination[3] = 255;
    }

}
