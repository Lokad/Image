namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // Covers at most256 luma conversions,128 chroma samples with3x3 taps and
    // paired RGB input, area/range rounding, eight transforms and entropy events.
    private const int ReferenceSubsampledMcuWork = 262144;

    private static void ReferenceSubsampledPass(ImageBuffer? image, JpegSampleInput source, int width, int height,
        uint? background, JpegSampling sampling, ushort[] quantization, int[][] histograms,
        JpegEncodingTable[]? tables, JpegEntropyWriter? writer, OperationContext context, int restartInterval)
    {
        // All per-MCU and nested tap storage fits the admitted8-KiB fixed owner.
        // No row/filter plan, frame copy or coefficient cache escapes this call.
        Span<byte> mcu = stackalloc byte[512];
        Span<short> transformed = stackalloc short[64], coefficients = stackalloc short[64];
        Span<int> previous = stackalloc int[3]; previous.Clear();
        var chromaWidth = (width + 1) / 2;
        var chromaRows = sampling == JpegSampling.YCbCr420 ? 8 : 16;
        var chromaHeight = sampling == JpegSampling.YCbCr420 ? (height + 1) / 2 : height;
        var mcuIndex = 0;
        for (var top = 0; top < height; top += 16)
        for (var left = 0; left < width; left += 16)
        {
            ReferenceRestart(mcuIndex++, restartInterval, previous, writer, context);
            context.Work(ReferenceSubsampledMcuWork);
            for (var y = 0; y < 16; y++)
            for (var x = 0; x < 16; x++)
            {
                ReferenceRgb(image, source, Math.Min(left + x, width - 1), Math.Min(top + y, height - 1),
                    background, out var r, out var g, out var b);
                mcu[(y / 8 * 2 + x / 8) * 64 + y % 8 * 8 + x % 8] = JpegReferenceColor.Luma(r, g, b);
            }
            for (var y = 0; y < chromaRows; y++)
            for (var x = 0; x < 8; x++)
            {
                var cx = Math.Min(left / 2 + x, chromaWidth - 1);
                var cy = Math.Min((sampling == JpegSampling.YCbCr420 ? top / 2 : top) + y, chromaHeight - 1);
                ReferenceChroma(image, source, width, height, chromaWidth, chromaHeight, cx, cy, background,
                    out var cb, out var cr);
                var position = y / 8 * 64 + y % 8 * 8 + x;
                mcu[256 + position] = cb; mcu[256 + chromaRows * 8 + position] = cr;
            }
            for (var component = 0; component < 3; component++)
            for (var vertical = 0; vertical < (component == 0 ? 2 : chromaRows / 8); vertical++)
            for (var horizontal = 0; horizontal < (component == 0 ? 2 : 1); horizontal++)
            {
                var offset = component == 0 ? (vertical * 2 + horizontal) * 64
                    : 256 + (component - 1) * chromaRows * 8 + vertical * 64;
                JpegReferenceTransform.Forward(mcu.Slice(offset, 64), transformed);
                JpegReferenceTransform.Quantize(transformed, quantization, coefficients);
                ReferenceBlock(coefficients, component, previous, histograms, tables, writer);
            }
        }
    }

    private static void ReferenceChroma(ImageBuffer? image, JpegSampleInput source, int width, int height,
        int chromaWidth, int chromaHeight, int x, int y, uint? background, out byte cb, out byte cr)
    {
        var pair = (width & 1) == 0;
        var horizontal = JpegReferenceArea.Create(pair ? width / 2 : width, chromaWidth, x, 14);
        var vertical = JpegReferenceArea.Create(height, chromaHeight, y, 12);
        var sumU = 1 << 18; var sumV = 1 << 18;
        for (var row = 0; row < vertical.Count; row++)
        {
            var horizontalU = 0; var horizontalV = 0;
            for (var column = 0; column < horizontal.Count; column++)
            {
                var sx = horizontal.Source(column) * (pair ? 2 : 1); var sy = vertical.Source(row);
                ReferenceRgb(image, source, sx, sy, background, out var r, out var g, out var b);
                if (pair)
                {
                    ReferenceRgb(image, source, sx + 1, sy, background, out var r1, out var g1, out var b1);
                    r += r1; g += g1; b += b1;
                }
                JpegReferenceColor.LimitedChroma(r, g, b, pair, out var u, out var v);
                horizontalU += u * horizontal.Weight(column); horizontalV += v * horizontal.Weight(column);
            }
            sumU += JpegReferenceColor.FullChromaQ7(Math.Min(32767, horizontalU >> 13)) * vertical.Weight(row);
            sumV += JpegReferenceColor.FullChromaQ7(Math.Min(32767, horizontalV >> 13)) * vertical.Weight(row);
        }
        cb = (byte)Math.Clamp(sumU >> 19, 0, 255); cr = (byte)Math.Clamp(sumV >> 19, 0, 255);
    }

    private static void ReferenceRgb(ImageBuffer? image, JpegSampleInput source, int x, int y, uint? background,
        out int r, out int g, out int b)
    {
        if (image is not null) { ReadRgb(image, x, y, background, out r, out g, out b); return; }
        Span<byte> rgb = stackalloc byte[3]; source.CopyRgbRow(x, y, rgb);
        r = rgb[0]; g = rgb[1]; b = rgb[2];
    }
}
