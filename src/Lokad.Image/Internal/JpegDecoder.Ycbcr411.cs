using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private bool CanRenderYcbcr411()
        => !rgb && components.Length == 3 && maxH == 4 && maxV == 1
            && components[0].H == 4 && components[0].V == 1
            && components[1].H == 1 && components[1].V == 1
            && components[2].H == 1 && components[2].V == 1
            && info.Width >= 16 && Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported;

    private ImageBuffer RenderYcbcr411()
    {
        // This is the same three-component materialization walk used by Render:
        // identical inverse transforms, allocation/work charges and pool release.
        MaterializeSamplePlanes();
        var result = ImageBuffer.Allocate(info.Width, info.Height, 8, context); result.Orientation = info.Orientation;
        try
        {
            for (var y = 0; y < info.Height; y++)
            {
                context.Work(info.Width * 32L);
                RenderYcbcr411Row(components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width),
                    components[1].Plane.AsSpan(y * components[1].BlockColumns * 8, components[1].SampleWidth),
                    components[2].Plane.AsSpan(y * components[2].BlockColumns * 8, components[2].SampleWidth), result.GetRow8(y));
            }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    internal static void RenderYcbcr411Row(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> cb, ReadOnlySpan<byte> cr, Span<byte> rgba)
    {
        if (cb.Length != (luma.Length + 3L) / 4 || cr.Length != cb.Length || rgba.Length != luma.Length * 4L)
            throw new ArgumentException("JPEG411 rows require full Y, quarter-width Cb/Cr and RGBA output.");
        var x = 0;
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
        {
            Span<byte> scratch = stackalloc byte[24];
            for (; x < Math.Min(4, luma.Length); x++) WriteYcbcr411Pixel(luma, cb, cr, rgba, x);
            for (; x <= luma.Length - 8; x += 8)
            {
                var column = x / 4 - 1;
                if (column > cb.Length - 4) break;
                var first = Avx512F.ConvertToVector512Double(WidenSamples8(luma.Slice(x, 8)));
                var second = Interpolate411Eight(cb.Slice(column, 4));
                var third = Interpolate411Eight(cr.Slice(column, 4));
                // Keep each binary64 operation and final-only away-from-zero
                // rounding of the general centered YCbCr pixel contract.
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
        for (; x < luma.Length; x++) WriteYcbcr411Pixel(luma, cb, cr, rgba, x);
    }

    private static Vector512<double> Interpolate411Eight(ReadOnlySpan<byte> samples)
    {
        // T.81 centered H4V1 coordinates are (2*x-3)/8. Starting at x mod4=0,
        // eight outputs need exactly four source samples; do not read padding.
        var four = Vector128.Create(BinaryPrimitives.ReadUInt32LittleEndian(samples), 0u, 0u, 0u).AsByte();
        var widened = Avx2.ConvertToVector256Int32(four);
        var left = Avx2.PermuteVar8x32(widened, Vector256.Create(0, 0, 1, 1, 1, 1, 2, 2));
        var right = Avx2.PermuteVar8x32(widened, Vector256.Create(1, 1, 2, 2, 2, 2, 3, 3));
        var sum = Avx2.Add(Avx2.MultiplyLow(left, Vector256.Create(3, 1, 7, 5, 3, 1, 7, 5)),
            Avx2.MultiplyLow(right, Vector256.Create(5, 7, 1, 3, 5, 7, 1, 3)));
        // All integer sums and dyadic fractions are exact in binary64.
        return Avx512F.Multiply(Avx512F.ConvertToVector512Double(Avx2.Subtract(sum, Vector256.Create(1024))), Vector512.Create(.125));
    }

    private static void WriteYcbcr411Pixel(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> cb, ReadOnlySpan<byte> cr, Span<byte> rgba, int x)
    {
        var coordinate = 2 * x - 3; var left = coordinate >> 3; var rightWeight = coordinate - 8 * left;
        var a = Math.Clamp(left, 0, cb.Length - 1); var b = Math.Clamp(left + 1, 0, cb.Length - 1);
        var second = (cb[a] * (8 - rightWeight) + cb[b] * rightWeight) / 8d - 128;
        var third = (cr[a] * (8 - rightWeight) + cr[b] * rightWeight) / 8d - 128;
        rgba[x * 4] = Round(luma[x] + 1.402 * third);
        rgba[x * 4 + 1] = Round(luma[x] - .344136286 * second - .714136286 * third);
        rgba[x * 4 + 2] = Round(luma[x] + 1.772 * second); rgba[x * 4 + 3] = 255;
    }
}
