using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.Internal;

internal ref partial struct JpegDecoder
{
    private bool TryWriteYcck420Rgb(Span<byte> destination, int stride, bool invert)
    {
        if (!invert || info.Width < 16 || maxH != 2 || maxV != 2 || components.Length != 4
            || components[0].H != 2 || components[0].V != 2 || components[1].H != 1 || components[1].V != 1
            || components[2].H != 1 || components[2].V != 1 || components[3].H != 2 || components[3].V != 2
            || !Avx512F.IsSupported || !Avx512F.VL.IsSupported || !Avx2.IsSupported || !Ssse3.IsSupported) return false;
        Span<byte> scratch = stackalloc byte[24]; var chroma = components[1]; var chromaStride = chroma.BlockColumns * 8;
        for (var y = 0; y < info.Height; y++)
        {
            context.Work(info.Width * 32L);
            var row = destination.Slice(y * stride, info.Width * 3);
            var luma = components[0].Plane.AsSpan(y * components[0].BlockColumns * 8, info.Width);
            var black = components[3].Plane.AsSpan(y * components[3].BlockColumns * 8, info.Width);
            var iy = (y - 1) >> 1; var y0 = Math.Clamp(iy, 0, chroma.SampleHeight - 1); var y1 = Math.Clamp(iy + 1, 0, chroma.SampleHeight - 1);
            var topWeight = (y & 1) == 0 ? 1 : 3; var x = 0;
            for (; x < 2; x++) WriteYcckRgbEdge(row.Slice(x * 3, 3), x, y);
            for (; x <= info.Width - 8; x += 8)
            {
                var column = x / 2 - 1;
                if (column > chromaStride - 8 || column > chroma.SampleWidth - 6) break;
                RenderYcck420Rgb8(luma.Slice(x, 8), black.Slice(x, 8), chroma.Plane.AsSpan(y0 * chromaStride + column, 8),
                    chroma.Plane.AsSpan(y1 * chromaStride + column, 8), components[2].Plane.AsSpan(y0 * chromaStride + column, 8),
                    components[2].Plane.AsSpan(y1 * chromaStride + column, 8), topWeight, row.Slice(x * 3, 24), scratch);
            }
            for (; x < info.Width; x++) WriteYcckRgbEdge(row.Slice(x * 3, 3), x, y);
        }
        return true;
    }

    private void WriteYcckRgbEdge(Span<byte> output, int x, int y)
    {
        CmykValues(x, y, JpegColorSpace.Ycck, true, out var c, out var m, out var yellow, out var k);
        output[0] = Round((255 - c) * (255 - k) / 255); output[1] = Round((255 - m) * (255 - k) / 255);
        output[2] = Round((255 - yellow) * (255 - k) / 255);
    }
}
