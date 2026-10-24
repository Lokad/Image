namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // Covers at most256 luma conversions,128 chroma samples with3x3 taps and
    // paired RGB input, area/range rounding, eight transforms and entropy events.
    private const int ReferenceSubsampledMcuWork = 262144;

    private static void ReferenceSubsampledPass(ImageBuffer? image, JpegSampleInput source, int width, int height,
        uint? background, JpegSampling sampling, ReadOnlySpan<int> quantizationPlan, int[][] histograms,
        JpegEncodingTable[]? tables, JpegEntropyWriter? writer, OperationContext context, int restartInterval)
    {
        // All per-MCU and nested tap storage fits the admitted8-KiB fixed owner.
        // No row/filter plan, frame copy or coefficient cache escapes this call.
        Span<byte> mcu = stackalloc byte[512];
        Span<short> transformed = stackalloc short[64], coefficients = stackalloc short[64];
        Span<int> previous = stackalloc int[3]; previous.Clear();
        // Each axis plan depends only on its output coordinate. Reuse the
        // vertical plans across the MCU row and the horizontal plans across
        // its chroma rows. These672 stack bytes fit the existing8-KiB owner.
        Span<JpegReferenceArea> horizontalPlans = stackalloc JpegReferenceArea[8];
        Span<JpegReferenceArea> verticalPlans = stackalloc JpegReferenceArea[16];
        // A 16-pixel MCU and the area filter's one-pixel halo need at most
        // 18 samples per axis. Reserve a bounded20x20 RGB tile inside the
        // same8-KiB owner and normalize each distinct source pixel once.
        Span<byte> rgb = stackalloc byte[20 * 20 * 3];
        var pair = (width & 1) == 0;
        var chromaWidth = (width + 1) / 2;
        var chromaRows = sampling == JpegSampling.YCbCr420 ? 8 : 16;
        var chromaHeight = sampling == JpegSampling.YCbCr420 ? (height + 1) / 2 : height;
        var mcuIndex = 0;
        for (var top = 0; top < height; top += 16)
        for (var left = 0; left < width; left += 16)
        {
            ReferenceRestart(mcuIndex++, restartInterval, previous, writer, context);
            context.Work(ReferenceSubsampledMcuWork);
            for (var x = 0; x < 8; x++)
                horizontalPlans[x] = JpegReferenceArea.Create(pair ? width / 2 : width, chromaWidth,
                    Math.Min(left / 2 + x, chromaWidth - 1), 14);
            if (left == 0)
                for (var y = 0; y < chromaRows; y++)
                    verticalPlans[y] = JpegReferenceArea.Create(height, chromaHeight,
                        Math.Min((sampling == JpegSampling.YCbCr420 ? top / 2 : top) + y, chromaHeight - 1), 12);
            var firstX = left; var lastX = Math.Min(left + 15, width - 1);
            var firstY = top; var lastY = Math.Min(top + 15, height - 1);
            foreach (var plan in horizontalPlans)
            {
                firstX = Math.Min(firstX, plan.Source(0) * (pair ? 2 : 1));
                lastX = Math.Max(lastX, plan.Source(plan.Count - 1) * (pair ? 2 : 1) + (pair ? 1 : 0));
            }
            foreach (var plan in verticalPlans[..chromaRows])
            {
                firstY = Math.Min(firstY, plan.Source(0));
                lastY = Math.Max(lastY, plan.Source(plan.Count - 1));
            }
            var stride = (lastX - firstX + 1) * 3;
            for (var sy = firstY; sy <= lastY; sy++)
            {
                var row = rgb.Slice((sy - firstY) * stride, stride);
                if (image is null) source.CopyRgbRow(firstX, sy, row);
                else if (image.BitDepth == 8)
                    JpegReferenceColor.NormalizeRgba8(image.GetRow8(sy).Slice(firstX * 4, stride / 3 * 4), row, background);
                else
                    for (var sx = firstX; sx <= lastX; sx++)
                    {
                        ReadRgb(image, sx, sy, background, out var r, out var g, out var b);
                        var at = (sx - firstX) * 3;
                        row[at] = (byte)r; row[at + 1] = (byte)g; row[at + 2] = (byte)b;
                    }
            }
            for (var y = 0; y < 16; y++)
            for (var x = 0; x < 16; x++)
            {
                var at = (Math.Min(top + y, height - 1) - firstY) * stride
                    + (Math.Min(left + x, width - 1) - firstX) * 3;
                mcu[(y / 8 * 2 + x / 8) * 64 + y % 8 * 8 + x % 8] =
                    JpegReferenceColor.Luma(rgb[at], rgb[at + 1], rgb[at + 2]);
            }
            for (var y = 0; y < chromaRows; y++)
            for (var x = 0; x < 8; x++)
            {
                ReferenceChroma(rgb, stride, firstX, firstY, pair, horizontalPlans[x], verticalPlans[y],
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
                JpegReferenceTransform.Quantize(transformed, quantizationPlan, coefficients);
                ReferenceBlock(coefficients, component, previous, histograms, tables, writer);
            }
        }
    }

    private static void ReferenceChroma(ReadOnlySpan<byte> rgb, int stride, int firstX, int firstY, bool pair,
        JpegReferenceArea horizontal, JpegReferenceArea vertical, out byte cb, out byte cr)
    {
        var sumU = 1 << 18; var sumV = 1 << 18;
        for (var row = 0; row < vertical.Count; row++)
        {
            var horizontalU = 0; var horizontalV = 0;
            for (var column = 0; column < horizontal.Count; column++)
            {
                var sx = horizontal.Source(column) * (pair ? 2 : 1); var sy = vertical.Source(row);
                var at = (sy - firstY) * stride + (sx - firstX) * 3;
                int r = rgb[at], g = rgb[at + 1], b = rgb[at + 2];
                if (pair)
                {
                    r += rgb[at + 3]; g += rgb[at + 4]; b += rgb[at + 5];
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
