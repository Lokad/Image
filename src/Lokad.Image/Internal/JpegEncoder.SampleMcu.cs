using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // T.81 component sampling, preserving the owned binary64 matrix and rounding.
    // Four Y blocks followed by Cb/Cr; no frame storage or second RGB traversal.
    internal static void SampleCaller420Mcu(JpegSampleInput source, int left, int top, Span<byte> blocks)
    {
        Span<byte> row = stackalloc byte[48];
        var even = Vector512.Create(0L, 2, 4, 6, 8, 10, 12, 14);
        var odd = Vector512.Create(1L, 3, 5, 7, 9, 11, 13, 15);
        var cbSum = Vector512<double>.Zero; var crSum = Vector512<double>.Zero;
        for (var y = 0; y < 16; y++)
        {
            source.CopyRgbRow(left, top + y, row);
            CallerRgbComponents8(row[..24], out var yl, out var cbl, out var crl);
            CallerRgbComponents8(row[24..], out var yr, out var cbr, out var crr);
            var offset = y / 8 * 128 + y % 8 * 8;
            JpegTransform.RoundSamples(yl, blocks.Slice(offset, 8));
            JpegTransform.RoundSamples(yr, blocks.Slice(offset + 64, 8));
            var cbEven = Avx512F.PermuteVar8x64x2(cbl.AsInt64(), even, cbr.AsInt64()).AsDouble();
            var cbOdd = Avx512F.PermuteVar8x64x2(cbl.AsInt64(), odd, cbr.AsInt64()).AsDouble();
            var crEven = Avx512F.PermuteVar8x64x2(crl.AsInt64(), even, crr.AsInt64()).AsDouble();
            var crOdd = Avx512F.PermuteVar8x64x2(crl.AsInt64(), odd, crr.AsInt64()).AsDouble();
            if ((y & 1) == 0)
            {
                cbSum = Avx512F.Add(Avx512F.Add(Vector512<double>.Zero, cbEven), cbOdd);
                crSum = Avx512F.Add(Avx512F.Add(Vector512<double>.Zero, crEven), crOdd);
            }
            else
            {
                // Top even/odd then bottom even/odd, exactly the scalar dy/dx order.
                cbSum = Avx512F.Add(Avx512F.Add(cbSum, cbEven), cbOdd);
                crSum = Avx512F.Add(Avx512F.Add(crSum, crEven), crOdd);
                JpegTransform.RoundSamples(Avx512F.Divide(cbSum, Vector512.Create(4d)), blocks.Slice(256 + y / 2 * 8, 8));
                JpegTransform.RoundSamples(Avx512F.Divide(crSum, Vector512.Create(4d)), blocks.Slice(320 + y / 2 * 8, 8));
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CallerRgbComponents8(ReadOnlySpan<byte> rgb,
        out Vector512<double> y, out Vector512<double> cb, out Vector512<double> cr)
    {
        var first = Vector128.Create(rgb[..16]); var last = Vector128.Create(rgb.Slice(8, 16));
        var r = PackedChannel8(first, last, 0); var g = PackedChannel8(first, last, 1); var b = PackedChannel8(first, last, 2);
        y = Avx512F.Add(Avx512F.Add(Avx512F.Multiply(Vector512.Create(0.299), r), Avx512F.Multiply(Vector512.Create(0.587), g)), Avx512F.Multiply(Vector512.Create(0.114), b));
        cb = Avx512F.Add(Avx512F.Subtract(Avx512F.Subtract(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.168735892), r)), Avx512F.Multiply(Vector512.Create(0.331264108), g)), Avx512F.Multiply(Vector512.Create(0.5), b));
        cr = Avx512F.Subtract(Avx512F.Subtract(Avx512F.Add(Vector512.Create(128d), Avx512F.Multiply(Vector512.Create(0.5), r)), Avx512F.Multiply(Vector512.Create(0.418687589), g)), Avx512F.Multiply(Vector512.Create(0.081312411), b));
    }
}
