using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private bool TryRenderYcck420(ImageBuffer result, JpegColorSpace source, bool invert)
    {
        if (source != JpegColorSpace.Ycck || !invert || info.Width < 16 || maxH != 2 || maxV != 2
            || components.Length != 4 || components[0].H != 2 || components[0].V != 2
            || components[1].H != 1 || components[1].V != 1 || components[2].H != 1 || components[2].V != 1
            || components[3].H != 2 || components[3].V != 2
            || !Avx512F.IsSupported || !Avx512F.VL.IsSupported || !Avx2.IsSupported) return false;
        Span<byte> scratch = stackalloc byte[24];
        var chroma = components[1]; var chromaStride = chroma.BlockColumns * 8;
        for (var y = 0; y < info.Height; y++)
        {
            // Retain the original per-row admission/cancellation boundary.
            context.Work(info.Width * 32L);
            var row = result.GetRow8(y);
            var luma = components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width);
            var black = components[3].Plane.AsSpan(y * components[3].BlockColumns * 8, info.Width);
            var iy = (y - 1) >> 1;
            var y0 = Math.Clamp(iy, 0, chroma.SampleHeight - 1); var y1 = Math.Clamp(iy + 1, 0, chroma.SampleHeight - 1);
            var topWeight = (y & 1) == 0 ? 1 : 3; var x = 0;
            for (; x < 2; x++) WriteYcckEdge(row.Slice(x * 4, 4), x, y);
            for (; x <= info.Width - 8; x += 8)
            {
                var column = x / 2 - 1;
                if (column > chromaStride - 8 || column > chroma.SampleWidth - 6) break;
                RenderYcck420Row8(luma.Slice(x, 8), black.Slice(x, 8), chroma.Plane.AsSpan(y0 * chromaStride + column, 8),
                    chroma.Plane.AsSpan(y1 * chromaStride + column, 8), components[2].Plane.AsSpan(y0 * chromaStride + column, 8),
                    components[2].Plane.AsSpan(y1 * chromaStride + column, 8), topWeight, row.Slice(x * 4, 32), scratch);
            }
            for (; x < info.Width; x++) WriteYcckEdge(row.Slice(x * 4, 4), x, y);
        }
        return true;
    }

    private void WriteYcckEdge(Span<byte> output, int x, int y)
    {
        CmykValues(x, y, JpegColorSpace.Ycck, true, out var c, out var m, out var yellow, out var k);
        output[0] = Round((255 - c) * (255 - k) / 255); output[1] = Round((255 - m) * (255 - k) / 255);
        output[2] = Round((255 - yellow) * (255 - k) / 255); output[3] = 255;
    }

    internal static void RenderYcck420Row8(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> black,
        ReadOnlySpan<byte> cbTop, ReadOnlySpan<byte> cbBottom, ReadOnlySpan<byte> crTop, ReadOnlySpan<byte> crBottom,
        int topWeight, Span<byte> rgba, Span<byte> scratch)
    {
        if (luma.Length != 8 || black.Length != 8 || cbTop.Length != 8 || cbBottom.Length != 8 || crTop.Length != 8 || crBottom.Length != 8
            || topWeight is not (1 or 3) || rgba.Length != 32 || scratch.Length != 24)
            throw new ArgumentException("JPEG YCCK batch requires eight samples, quarter weights, 32 output and 24 scratch bytes.");
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported)
        {
            var packed = RenderYcck420Packed8(luma, black, cbTop, cbBottom, crTop, crBottom, topWeight, scratch);
            Avx2.Or(packed, Vector256.Create(0xff000000u)).AsByte().CopyTo(rgba); return;
        }
        for (var x = 0; x < 8; x++)
        {
            var left = x / 2 + (x & 1); var right = left + 1; var fx = (x & 1) == 0 ? .75 : .25;
            var upper = topWeight / 4d; var lower = (4 - topWeight) / 4d;
            var cb = (cbTop[left] * (1 - fx) + cbTop[right] * fx) * upper + (cbBottom[left] * (1 - fx) + cbBottom[right] * fx) * lower - 128;
            var cr = (crTop[left] * (1 - fx) + crTop[right] * fx) * upper + (crBottom[left] * (1 - fx) + crBottom[right] * fx) * lower - 128;
            var c = 255 - (luma[x] + 1.402 * cr); var m = 255 - (luma[x] - .344136286 * cb - .714136286 * cr);
            var yellow = 255 - (luma[x] + 1.772 * cb); double k = black[x];
            c = 255 - c; m = 255 - m; yellow = 255 - yellow; k = 255 - k;
            rgba[x * 4] = Round((255 - c) * (255 - k) / 255); rgba[x * 4 + 1] = Round((255 - m) * (255 - k) / 255);
            rgba[x * 4 + 2] = Round((255 - yellow) * (255 - k) / 255); rgba[x * 4 + 3] = 255;
        }
    }
    internal static void RenderYcck420Rgb8(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> black,
        ReadOnlySpan<byte> cbTop, ReadOnlySpan<byte> cbBottom, ReadOnlySpan<byte> crTop, ReadOnlySpan<byte> crBottom,
        int topWeight, Span<byte> output, Span<byte> scratch)
    {
        if (luma.Length != 8 || black.Length != 8 || cbTop.Length != 8 || cbBottom.Length != 8 || crTop.Length != 8 || crBottom.Length != 8
            || topWeight is not (1 or 3) || output.Length != 24 || scratch.Length != 24)
            throw new ArgumentException("JPEG YCCK batch requires eight samples, quarter weights, 24 output and 24 scratch bytes.");
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported && Ssse3.IsSupported)
        {
            PackRgb8(RenderYcck420Packed8(luma, black, cbTop, cbBottom, crTop, crBottom, topWeight, scratch), output); return;
        }
        for (var x = 0; x < 8; x++)
        {
            var left = x / 2 + (x & 1); var right = left + 1; var fx = (x & 1) == 0 ? .75 : .25;
            var upper = topWeight / 4d; var lower = (4 - topWeight) / 4d;
            var cb = (cbTop[left] * (1 - fx) + cbTop[right] * fx) * upper + (cbBottom[left] * (1 - fx) + cbBottom[right] * fx) * lower - 128;
            var cr = (crTop[left] * (1 - fx) + crTop[right] * fx) * upper + (crBottom[left] * (1 - fx) + crBottom[right] * fx) * lower - 128;
            var c = 255 - (luma[x] + 1.402 * cr); var m = 255 - (luma[x] - .344136286 * cb - .714136286 * cr);
            var yellow = 255 - (luma[x] + 1.772 * cb); double k = black[x];
            c = 255 - c; m = 255 - m; yellow = 255 - yellow; k = 255 - k;
            output[x * 3] = Round((255 - c) * (255 - k) / 255); output[x * 3 + 1] = Round((255 - m) * (255 - k) / 255);
            output[x * 3 + 2] = Round((255 - yellow) * (255 - k) / 255);
        }
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> RenderYcck420Packed8(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> black,
        ReadOnlySpan<byte> cbTop, ReadOnlySpan<byte> cbBottom, ReadOnlySpan<byte> crTop, ReadOnlySpan<byte> crBottom,
        int topWeight, Span<byte> scratch)
    {
    var full = Vector512.Create(255d); var first = Avx512F.ConvertToVector512Double(WidenSamples8(luma));
    var cb = Interpolate420Row8(cbTop, cbBottom, topWeight); var cr = Interpolate420Row8(crTop, crBottom, topWeight);
    // Preserve every original binary64 matrix/complement operation.
    // Do not quantize canonical CMYK, cancel complements, fuse matrix
    // operations or replace division by reciprocal multiplication.
    var c = Avx512F.Subtract(full, Avx512F.Add(first, Avx512F.Multiply(Vector512.Create(1.402), cr)));
    var m = Avx512F.Subtract(full, Avx512F.Subtract(Avx512F.Subtract(first, Avx512F.Multiply(Vector512.Create(.344136286), cb)),
        Avx512F.Multiply(Vector512.Create(.714136286), cr)));
    var yellow = Avx512F.Subtract(full, Avx512F.Add(first, Avx512F.Multiply(Vector512.Create(1.772), cb)));
    var k = Avx512F.ConvertToVector512Double(WidenSamples8(black));
    c = Avx512F.Subtract(full, c); m = Avx512F.Subtract(full, m);
    yellow = Avx512F.Subtract(full, yellow); k = Avx512F.Subtract(full, k);
    var factor = Avx512F.Subtract(full, k);
    JpegTransform.RoundSamples(Avx512F.Divide(Avx512F.Multiply(Avx512F.Subtract(full, c), factor), full), scratch[..8]);
    JpegTransform.RoundSamples(Avx512F.Divide(Avx512F.Multiply(Avx512F.Subtract(full, m), factor), full), scratch.Slice(8, 8));
    JpegTransform.RoundSamples(Avx512F.Divide(Avx512F.Multiply(Avx512F.Subtract(full, yellow), factor), full), scratch.Slice(16, 8));
    var r = WidenSamples8(scratch[..8]).AsUInt32(); var g = Avx2.ShiftLeftLogical(WidenSamples8(scratch.Slice(8, 8)).AsUInt32(), 8);
    var b = Avx2.ShiftLeftLogical(WidenSamples8(scratch.Slice(16, 8)).AsUInt32(), 16);
        return Avx2.Or(Avx2.Or(r, g), b);
    }

    private static Vector512<double> Interpolate420Row8(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom, int topWeight)
    {
        // T.81 sampling maps these centered pixels to exact quarter fractions.
        // Integer numerators reproduce the binary64 horizontal/vertical sums
        // exactly: their maximum is 16*255. No chroma rounding is introduced.
        var left = Vector256.Create(0, 1, 1, 2, 2, 3, 3, 4);
        var right = Vector256.Create(1, 2, 2, 3, 3, 4, 4, 5);
        var a = Vector256.Create(1, 3, 1, 3, 1, 3, 1, 3);
        var b = Vector256.Create(3, 1, 3, 1, 3, 1, 3, 1);
        var upper = WidenSamples8(top); var lower = WidenSamples8(bottom);
        upper = Avx2.Add(Avx2.MultiplyLow(Avx2.PermuteVar8x32(upper, left), a), Avx2.MultiplyLow(Avx2.PermuteVar8x32(upper, right), b));
        lower = Avx2.Add(Avx2.MultiplyLow(Avx2.PermuteVar8x32(lower, left), a), Avx2.MultiplyLow(Avx2.PermuteVar8x32(lower, right), b));
        var sum = Avx2.Add(Avx2.MultiplyLow(upper, Vector256.Create(topWeight)), Avx2.MultiplyLow(lower, Vector256.Create(4 - topWeight)));
        return Avx512F.Multiply(Avx512F.ConvertToVector512Double(Avx2.Subtract(sum, Vector256.Create(128 * 16))), Vector512.Create(1d / 16));
    }

    private static void PackRgb8(Vector256<uint> packed, Span<byte> output)
    {
        // Four 32-bit channel lanes become twelve bytes. No alpha raster or
        // padding write: the final store is exactly four bytes, even at tails.
        var mask = Vector128.Create((byte)0, 1, 2, 4, 5, 6, 8, 9, 10, 12, 13, 14, 128, 128, 128, 128);
        var lower = Ssse3.Shuffle(packed.GetLower().AsByte(), mask); var upper = Ssse3.Shuffle(packed.GetUpper().AsByte(), mask);
        BinaryPrimitives.WriteUInt64LittleEndian(output, lower.AsUInt64().GetElement(0));
        BinaryPrimitives.WriteUInt32LittleEndian(output.Slice(8), lower.AsUInt32().GetElement(2));
        BinaryPrimitives.WriteUInt64LittleEndian(output.Slice(12), upper.AsUInt64().GetElement(0));
        BinaryPrimitives.WriteUInt32LittleEndian(output.Slice(20), upper.AsUInt32().GetElement(2));
    }
}
