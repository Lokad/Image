using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private void WriteRgb420Samples(Span<byte> destination, int stride)
    {
        Span<byte> scratch = stackalloc byte[24];
        var chroma = components[1]; var chromaStride = chroma.BlockColumns * 8;
        for (var y = 0; y < info.Height; y++)
        {
            // Preserve the ordinary caller-sample work/cancellation boundary.
            context.Work(info.Width * 32L);
            var row = destination.Slice(y * stride, info.Width * 3);
            var luma = components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width);
            var iy = (y - 1) >> 1;
            var y0 = Math.Clamp(iy, 0, chroma.SampleHeight - 1);
            var y1 = Math.Clamp(iy + 1, 0, chroma.SampleHeight - 1);
            var topWeight = (y & 1) == 0 ? 1 : 3;
            var x = 0;
            for (; x < 2; x++) WriteRgb420Edge(row.Slice(x * 3, 3), luma[x], x, y);
            for (; x <= info.Width - 16; x += 16)
            {
                var column = x / 2 - 1;
                if (column > chromaStride - 16 || column > chroma.SampleWidth - 10) break;
                var cbTop = chroma.Plane.AsSpan(y0 * chromaStride + column, 16);
                var cbBottom = chroma.Plane.AsSpan(y1 * chromaStride + column, 16);
                var crTop = components[2].Plane.AsSpan(y0 * chromaStride + column, 16);
                var crBottom = components[2].Plane.AsSpan(y1 * chromaStride + column, 16);
                var output = row.Slice(x * 3, 48);
                if (!TryRenderYcbcr420Rgb16(luma.Slice(x, 16), cbTop, cbBottom, crTop, crBottom, topWeight, output))
                {
                    RenderYcbcr420Rgb8(luma.Slice(x, 8), cbTop[..8], cbBottom[..8], crTop[..8], crBottom[..8], topWeight, output[..24], scratch);
                    RenderYcbcr420Rgb8(luma.Slice(x + 8, 8), cbTop.Slice(4, 8), cbBottom.Slice(4, 8),
                        crTop.Slice(4, 8), crBottom.Slice(4, 8), topWeight, output.Slice(24, 24), scratch);
                }
            }
            for (; x <= info.Width - 8; x += 8)
            {
                var column = x / 2 - 1;
                if (column > chromaStride - 8 || column > chroma.SampleWidth - 6) break;
                RenderYcbcr420Rgb8(luma.Slice(x, 8), chroma.Plane.AsSpan(y0 * chromaStride + column, 8),
                    chroma.Plane.AsSpan(y1 * chromaStride + column, 8), components[2].Plane.AsSpan(y0 * chromaStride + column, 8),
                    components[2].Plane.AsSpan(y1 * chromaStride + column, 8), topWeight, row.Slice(x * 3, 24), scratch);
            }
            for (; x < info.Width; x++) WriteRgb420Edge(row.Slice(x * 3, 3), luma[x], x, y);
        }
    }

    private void WriteRgb420Edge(Span<byte> output, byte first, int x, int y)
    {
        var cb = Interpolate(components[1], x, y) - 128; var cr = Interpolate(components[2], x, y) - 128;
        output[0] = Round(first + 1.402 * cr);
        output[1] = Round(first - 0.344136286 * cb - 0.714136286 * cr);
        output[2] = Round(first + 1.772 * cb);
    }

    internal static bool TryRenderYcbcr420Rgb16(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> cbTop, ReadOnlySpan<byte> cbBottom,
        ReadOnlySpan<byte> crTop, ReadOnlySpan<byte> crBottom, int topWeight, Span<byte> output)
    {
        if (luma.Length != 16 || cbTop.Length != 16 || cbBottom.Length != 16 || crTop.Length != 16 || crBottom.Length != 16
            || topWeight is not (1 or 3) || output.Length != 48) throw new ArgumentException("JPEG RGB batch requires sixteen samples, quarter weights and 48 output bytes.");
        if (!(Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported && Ssse3.IsSupported)) return false;
        var first = Avx512F.ConvertToVector512Single(Avx512F.ConvertToVector512Int32(Vector128.Create(luma)));
        var cb = Interpolate420Row16(cbTop, cbBottom, topWeight); var cr = Interpolate420Row16(crTop, crBottom, topWeight);
        var red = Avx512F.Add(first, Avx512F.Multiply(Vector512.Create(1.402f), cr));
        var green = Avx512F.Subtract(Avx512F.Subtract(first, Avx512F.Multiply(Vector512.Create(0.344136286f), cb)),
            Avx512F.Multiply(Vector512.Create(0.714136286f), cr));
        var blue = Avx512F.Add(first, Avx512F.Multiply(Vector512.Create(1.772f), cb));
        // The existing error certificate excludes every near-half result;
        // those batches use the original binary64 arithmetic below.
        if (!TryRoundColor16(red, out var r) || !TryRoundColor16(green, out var g) || !TryRoundColor16(blue, out var b)) return false;
        var packed = Avx512F.Or(Avx512F.Or(r.AsUInt32(), Avx512F.ShiftLeftLogical(g.AsUInt32(), 8)), Avx512F.ShiftLeftLogical(b.AsUInt32(), 16));
        PackRgb8(packed.GetLower(), output[..24]); PackRgb8(packed.GetUpper(), output.Slice(24, 24)); return true;
    }

    internal static void RenderYcbcr420Rgb8(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> cbTop, ReadOnlySpan<byte> cbBottom,
        ReadOnlySpan<byte> crTop, ReadOnlySpan<byte> crBottom, int topWeight, Span<byte> output, Span<byte> scratch)
    {
        if (luma.Length != 8 || cbTop.Length != 8 || cbBottom.Length != 8 || crTop.Length != 8 || crBottom.Length != 8
            || topWeight is not (1 or 3) || output.Length != 24 || scratch.Length != 24)
            throw new ArgumentException("JPEG RGB batch requires eight samples, quarter weights and 24 output/scratch bytes.");
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Avx2.IsSupported && Ssse3.IsSupported)
        {
            var first = Avx512F.ConvertToVector512Double(WidenSamples8(luma));
            var cb = Interpolate420Row8(cbTop, cbBottom, topWeight); var cr = Interpolate420Row8(crTop, crBottom, topWeight);
            JpegTransform.RoundSamples(Avx512F.Add(first, Avx512F.Multiply(Vector512.Create(1.402), cr)), scratch[..8]);
            JpegTransform.RoundSamples(Avx512F.Subtract(Avx512F.Subtract(first, Avx512F.Multiply(Vector512.Create(0.344136286), cb)),
                Avx512F.Multiply(Vector512.Create(0.714136286), cr)), scratch.Slice(8, 8));
            JpegTransform.RoundSamples(Avx512F.Add(first, Avx512F.Multiply(Vector512.Create(1.772), cb)), scratch.Slice(16, 8));
            var packed = Avx2.Or(Avx2.Or(WidenSamples8(scratch[..8]).AsUInt32(), Avx2.ShiftLeftLogical(WidenSamples8(scratch.Slice(8, 8)).AsUInt32(), 8)),
                Avx2.ShiftLeftLogical(WidenSamples8(scratch.Slice(16, 8)).AsUInt32(), 16));
            PackRgb8(packed, output); return;
        }
        for (var x = 0; x < 8; x++)
        {
            var left = x / 2 + (x & 1); var right = left + 1; var fx = (x & 1) == 0 ? .75 : .25;
            var top = topWeight / 4d; var bottom = (4 - topWeight) / 4d;
            var cb = (cbTop[left] * (1 - fx) + cbTop[right] * fx) * top + (cbBottom[left] * (1 - fx) + cbBottom[right] * fx) * bottom - 128;
            var cr = (crTop[left] * (1 - fx) + crTop[right] * fx) * top + (crBottom[left] * (1 - fx) + crBottom[right] * fx) * bottom - 128;
            output[x * 3] = Round(luma[x] + 1.402 * cr); output[x * 3 + 1] = Round(luma[x] - 0.344136286 * cb - 0.714136286 * cr);
            output[x * 3 + 2] = Round(luma[x] + 1.772 * cb);
        }
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
