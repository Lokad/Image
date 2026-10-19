using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private bool CanRenderYcbcr410()
        => !rgb && components.Length == 3 && maxH == 4 && maxV == 2
            && components[0].H == 4 && components[0].V == 2
            && components[1].H == 1 && components[1].V == 1
            && components[2].H == 1 && components[2].V == 1
            && info.Width >= 16 && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported;

    private ImageBuffer RenderYcbcr410()
    {
        // Preserve the general renderer's component walk, inverse transforms,
        // allocation/admission order, work charges and coefficient release.
        MaterializeSamplePlanes();
        var result = ImageBuffer.Allocate(info.Width, info.Height, 8, context); result.Orientation = info.Orientation;
        try
        {
            for (var y = 0; y < info.Height; y++)
            {
                context.Work(info.Width * 32L);
                var top = (y + 1) / 2 - 1;
                var chroma = components[1]; var stride = chroma.BlockColumns * 8;
                var t = Math.Clamp(top, 0, chroma.SampleHeight - 1); var b = Math.Clamp(top + 1, 0, chroma.SampleHeight - 1);
                RenderYcbcr410Row(components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width),
                    chroma.Plane.AsSpan(t * stride, chroma.SampleWidth), chroma.Plane.AsSpan(b * stride, chroma.SampleWidth),
                    components[2].Plane.AsSpan(t * stride, chroma.SampleWidth), components[2].Plane.AsSpan(b * stride, chroma.SampleWidth),
                    (y & 1) == 0 ? 1 : 3, result.GetRow8(y));
            }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    internal static void RenderYcbcr410Row(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> cbTop, ReadOnlySpan<byte> cbBottom,
        ReadOnlySpan<byte> crTop, ReadOnlySpan<byte> crBottom, int topWeight, Span<byte> rgba)
    {
        if (topWeight is not (1 or 3) || cbTop.Length != (luma.Length + 3L) / 4 || cbBottom.Length != cbTop.Length
            || crTop.Length != cbTop.Length || crBottom.Length != cbTop.Length || rgba.Length != luma.Length * 4L)
            throw new ArgumentException("JPEG410 rows require full Y, quarter-width Cb/Cr rows, centered vertical weights and RGBA output.");
        var x = 0;
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
        {
            Span<byte> scratch = stackalloc byte[24];
            for (; x < Math.Min(4, luma.Length); x++) WriteYcbcr410Pixel(luma, cbTop, cbBottom, crTop, crBottom, topWeight, rgba, x);
            for (; x <= luma.Length - 8; x += 8)
            {
                var column = x / 4 - 1;
                if (column > cbTop.Length - 4) break;
                var first = Avx512F.ConvertToVector512Double(WidenSamples8(luma.Slice(x, 8)));
                var second = Interpolate410Eight(cbTop.Slice(column, 4), cbBottom.Slice(column, 4), topWeight);
                var third = Interpolate410Eight(crTop.Slice(column, 4), crBottom.Slice(column, 4), topWeight);
                // Separate binary64 matrix operations and final-only rounding
                // retain the general pixel contract, including values near ties.
                JpegTransform.RoundSamples(Avx512F.Add(first, Avx512F.Multiply(Vector512.Create(1.402), third)), scratch[..8]);
                JpegTransform.RoundSamples(Avx512F.Subtract(Avx512F.Subtract(first, Avx512F.Multiply(Vector512.Create(.344136286), second)),
                    Avx512F.Multiply(Vector512.Create(.714136286), third)), scratch.Slice(8, 8));
                JpegTransform.RoundSamples(Avx512F.Add(first, Avx512F.Multiply(Vector512.Create(1.772), second)), scratch.Slice(16, 8));
                var r = WidenSamples8(scratch[..8]).AsUInt32();
                var g = Avx2.ShiftLeftLogical(WidenSamples8(scratch.Slice(8, 8)).AsUInt32(), 8);
                var b = Avx2.ShiftLeftLogical(WidenSamples8(scratch.Slice(16, 8)).AsUInt32(), 16);
                Avx2.Or(Avx2.Or(r, g), Avx2.Or(b, Vector256.Create(0xFF000000u))).AsByte().CopyTo(rgba.Slice(x * 4, 32));
            }
        }
        for (; x < luma.Length; x++) WriteYcbcr410Pixel(luma, cbTop, cbBottom, crTop, crBottom, topWeight, rgba, x);
    }

    private static Vector512<double> Interpolate410Eight(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom, int topWeight)
    {
        var a = Avx2.ConvertToVector256Int32(Vector128.Create(BinaryPrimitives.ReadUInt32LittleEndian(top), 0u, 0u, 0u).AsByte());
        var b = Avx2.ConvertToVector256Int32(Vector128.Create(BinaryPrimitives.ReadUInt32LittleEndian(bottom), 0u, 0u, 0u).AsByte());
        var vertical = Avx2.Add(Avx2.MultiplyLow(a, Vector256.Create(topWeight)), Avx2.MultiplyLow(b, Vector256.Create(4 - topWeight)));
        var left = Avx2.PermuteVar8x32(vertical, Vector256.Create(0, 0, 1, 1, 1, 1, 2, 2));
        var right = Avx2.PermuteVar8x32(vertical, Vector256.Create(1, 1, 2, 2, 2, 2, 3, 3));
        var sum = Avx2.Add(Avx2.MultiplyLow(left, Vector256.Create(3, 1, 7, 5, 3, 1, 7, 5)),
            Avx2.MultiplyLow(right, Vector256.Create(5, 7, 1, 3, 5, 7, 1, 3)));
        // Centered H4V2 uses eighth/quarter coordinates. Every integer sum
        // and /32 fraction is exact in binary64, regardless of axis order.
        return Avx512F.Multiply(Avx512F.ConvertToVector512Double(Avx2.Subtract(sum, Vector256.Create(4096))), Vector512.Create(.03125));
    }

    private static void WriteYcbcr410Pixel(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> cbTop, ReadOnlySpan<byte> cbBottom,
        ReadOnlySpan<byte> crTop, ReadOnlySpan<byte> crBottom, int topWeight, Span<byte> rgba, int x)
    {
        var coordinate = 2 * x - 3; var left = coordinate >> 3; var rw = coordinate - 8 * left;
        var a = Math.Clamp(left, 0, cbTop.Length - 1); var b = Math.Clamp(left + 1, 0, cbTop.Length - 1);
        var second = ((cbTop[a] * (8 - rw) + cbTop[b] * rw) * topWeight
            + (cbBottom[a] * (8 - rw) + cbBottom[b] * rw) * (4 - topWeight)) / 32d - 128;
        var third = ((crTop[a] * (8 - rw) + crTop[b] * rw) * topWeight
            + (crBottom[a] * (8 - rw) + crBottom[b] * rw) * (4 - topWeight)) / 32d - 128;
        rgba[x * 4] = Round(luma[x] + 1.402 * third);
        rgba[x * 4 + 1] = Round(luma[x] - .344136286 * second - .714136286 * third);
        rgba[x * 4 + 2] = Round(luma[x] + 1.772 * second); rgba[x * 4 + 3] = 255;
    }
}
