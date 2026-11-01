using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

// Eight independent integer matrix lanes preserve every limited/full-range
// rounding stage. Rows borrow the admitted MCU tile; no extra storage is owned.
internal static partial class JpegReferenceColor
{
    internal static Vector256<int> MatrixLuma8(Vector256<int> red, Vector256<int> green, Vector256<int> blue)
    {
        var limited = Avx2.ShiftRightArithmetic(Avx2.MultiplyLow(red, Vector256.Create(Yr))
            + Avx2.MultiplyLow(green, Vector256.Create(Yg)) + Avx2.MultiplyLow(blue, Vector256.Create(Yb))
            + Vector256.Create((16 << 15) + 256), 9);
        const int scale = (255 * 16384 + 218) / 219;
        const int offset = (255 << 21) - ((16 + 219) << 7) * scale + 8192;
        var full = Avx2.ShiftRightArithmetic(Avx2.MultiplyLow(limited, Vector256.Create(2 * scale)) + Vector256.Create(offset), 14);
        full = Avx2.ShiftRightArithmetic(Avx2.Min(full, Vector256.Create(32767)) + Vector256.Create(64), 7);
        return Avx2.Min(Avx2.Max(full, Vector256<int>.Zero), Vector256.Create(255));
    }

    internal static void MatrixChroma8(Vector256<int> red, Vector256<int> green, Vector256<int> blue,
        bool pair, out Vector256<int> cb, out Vector256<int> cr)
    {
        var offset = Vector256.Create(pair ? (128 << 16) + 512 : (128 << 15) + 256);
        cb = Avx2.MultiplyLow(red, Vector256.Create(Ur)) + Avx2.MultiplyLow(green, Vector256.Create(Ug))
            + Avx2.MultiplyLow(blue, Vector256.Create(Ub)) + offset;
        cr = Avx2.MultiplyLow(red, Vector256.Create(Vr)) + Avx2.MultiplyLow(green, Vector256.Create(Vg))
            + Avx2.MultiplyLow(blue, Vector256.Create(Vb)) + offset;
        if (pair) { cb = Avx2.ShiftRightArithmetic(cb, 10); cr = Avx2.ShiftRightArithmetic(cr, 10); }
        else { cb = Avx2.ShiftRightArithmetic(cb, 9); cr = Avx2.ShiftRightArithmetic(cr, 9); }
    }
    private static void Channels8(ReadOnlySpan<int> rgb, out Vector256<int> red, out Vector256<int> green, out Vector256<int> blue)
    {
        var packed = MemoryMarshal.Read<Vector256<int>>(MemoryMarshal.AsBytes(rgb));
        var mask = Vector256.Create(255);
        red = packed & mask; green = Avx2.ShiftRightLogical(packed, 8) & mask; blue = Avx2.ShiftRightLogical(packed, 16) & mask;
    }

    private static void StoreLuma8(Vector256<int> luma, Span<byte> target)
    {
        var words = Sse2.PackSignedSaturate(luma.GetLower(), luma.GetUpper());
        var packed = Sse2.PackUnsignedSaturate(words, Vector128<short>.Zero).AsUInt64().GetElement(0);
        MemoryMarshal.Write(target, in packed);
    }

    internal static void MatrixLumaRow(ReadOnlySpan<int> rgb, int firstX, int left, int width, Span<byte> first, Span<byte> second)
    {
        if (Avx2.IsSupported && BitConverter.IsLittleEndian && left + 16 <= width)
        {
            var source = rgb[(left - firstX)..];
            Channels8(source, out var r0, out var g0, out var b0);
            Channels8(source[8..], out var r1, out var g1, out var b1);
            StoreLuma8(MatrixLuma8(r0, g0, b0), first); StoreLuma8(MatrixLuma8(r1, g1, b1), second);
            return;
        }
        for (var x = 0; x < 16; x++)
        {
            var packed = rgb[Math.Min(left + x, width - 1) - firstX];
            var value = Luma(packed & 255, packed >> 8 & 255, packed >> 16 & 255);
            if (x < 8) first[x] = value; else second[x - 8] = value;
        }
    }

    private static Vector256<int> PairChannels(Vector256<int> first, Vector256<int> second)
        => Avx2.PermuteVar8x32(Avx2.HorizontalAdd(first, second), Vector256.Create(0, 1, 4, 5, 2, 3, 6, 7));

    internal static void MatrixChromaRow(Span<int> rgb, int columns, bool pair)
    {
        var x = 0;
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            if (pair)
            {
                for (; x + 16 <= columns; x += 16)
                {
                    Channels8(rgb[x..], out var r0, out var g0, out var b0);
                    Channels8(rgb[(x + 8)..], out var r1, out var g1, out var b1);
                    MatrixChroma8(PairChannels(r0, r1), PairChannels(g0, g1), PairChannels(b0, b1), true, out var u, out var v);
                    var packed = u | Avx2.ShiftLeftLogical(v, 16);
                    // Gather all sixteen inputs before replacing any even lane.
                    for (var lane = 0; lane < 8; lane++) rgb[x + lane * 2] = packed.GetElement(lane);
                }
            }
            else
            {
                for (; x + 8 <= columns; x += 8)
                {
                    Channels8(rgb[x..], out var r, out var g, out var b);
                    MatrixChroma8(r, g, b, false, out var u, out var v);
                    var packed = u | Avx2.ShiftLeftLogical(v, 16);
                    packed.CopyTo(rgb[x..]);
                }
            }
        }
        for (; x < columns; x += pair ? 2 : 1)
        {
            var packed = rgb[x]; var r = packed & 255; var g = packed >> 8 & 255; var b = packed >> 16 & 255;
            if (pair) { var next = rgb[x + 1]; r += next & 255; g += next >> 8 & 255; b += next >> 16 & 255; }
            LimitedChroma(r, g, b, pair, out var u, out var v); rgb[x] = u | v << 16;
        }
    }

    internal static void VerticalChromaRow(ReadOnlySpan<short> chroma, int stride, int firstY,
        JpegReferenceArea vertical, Span<byte> cb, Span<byte> cr)
    {
        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            var low = Vector256.Create(1 << 18); var high = low;
            for (var row = 0; row < vertical.Count; row++)
            {
                var at = (vertical.Source(row) - firstY) * stride;
                // Eight interleaved signed Q7 pairs, borrowed from an already
                // initialized MCU row. Q12 weights sum to4096, so Int32 suffices.
                var pairs = MemoryMarshal.Read<Vector256<short>>(MemoryMarshal.AsBytes(chroma[at..]));
                var weight = Vector256.Create(vertical.Weight(row));
                low += Avx2.MultiplyLow(Avx2.ConvertToVector256Int32(pairs.GetLower()), weight);
                high += Avx2.MultiplyLow(Avx2.ConvertToVector256Int32(pairs.GetUpper()), weight);
            }
            low = Avx2.ShiftRightArithmetic(low, 19); high = Avx2.ShiftRightArithmetic(high, 19);
            var first = Sse2.PackSignedSaturate(low.GetLower(), low.GetUpper());
            var second = Sse2.PackSignedSaturate(high.GetLower(), high.GetUpper());
            var interleaved = Sse2.PackUnsignedSaturate(first, second).AsUInt16();
            var u = (interleaved & Vector128.Create((ushort)255)).AsInt16();
            var v = Sse2.ShiftRightLogical(interleaved, 8).AsInt16();
            var result = Sse2.PackUnsignedSaturate(u, v).AsUInt64();
            var uBytes = result.GetElement(0); var vBytes = result.GetElement(1);
            MemoryMarshal.Write(cb, in uBytes); MemoryMarshal.Write(cr, in vBytes);
            return;
        }
        for (var x = 0; x < 8; x++)
        {
            var sumU = 1 << 18; var sumV = 1 << 18;
            for (var row = 0; row < vertical.Count; row++)
            {
                var at = (vertical.Source(row) - firstY) * stride + x * 2;
                sumU += chroma[at] * vertical.Weight(row);
                sumV += chroma[at + 1] * vertical.Weight(row);
            }
            cb[x] = (byte)Math.Clamp(sumU >> 19, 0, 255);
            cr[x] = (byte)Math.Clamp(sumV >> 19, 0, 255);
        }
    }
}
