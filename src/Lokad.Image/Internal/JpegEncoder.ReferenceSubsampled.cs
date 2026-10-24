using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    // Covers at most256 luma conversions,128 chroma samples with3x3 taps and
    // paired RGB input, area/range rounding, eight transforms and entropy events.
    private const int ReferenceSubsampledMcuWork = 262144;

    private static void ReferenceSubsampledPass(ImageBuffer? image, JpegSampleInput source, int width, int height,
        uint? background, JpegSampling sampling, ReadOnlySpan<ushort> quantizationPlan, int[][] histograms,
        JpegEncodingTable[]? tables, JpegEntropyWriter? writer, OperationContext context, int restartInterval,
        JpegEntropyRecords? entropyCache)
    {
        // All per-MCU and nested tap storage fits the admitted8-KiB fixed owner.
        // Row/filter plans stay here; optional entropy records have an outer owner.
        Span<byte> mcu = stackalloc byte[512];
        Span<short> transformed = stackalloc short[64], coefficients = stackalloc short[64];
        Span<int> previous = stackalloc int[3]; previous.Clear();
        // Each axis plan depends only on its output coordinate. Reuse the
        // vertical plans across the MCU row and the horizontal plans across
        // its chroma rows. These672 stack bytes fit the existing8-KiB owner.
        Span<JpegReferenceArea> horizontalPlans = stackalloc JpegReferenceArea[8];
        Span<JpegReferenceArea> verticalPlans = stackalloc JpegReferenceArea[16];
        // The Q16 area phase and three taps extend an MCU to at most18 samples
        // per axis. Packed RGB is reused for two Q6 chroma words after luma.
        // This1296-byte tile,54-byte caller row and32-byte horizontal result
        // fit the same8-KiB owner. A fixed pitch also covers tiny edge MCUs.
        Span<int> pixels = stackalloc int[18 * 18];
        Span<byte> callerRow = stackalloc byte[18 * 3];
        Span<short> horizontalChroma = stackalloc short[16];
        const int stride = 18;
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
            var columns = lastX - firstX + 1;
            for (var sy = firstY; sy <= lastY; sy++)
            {
                var row = pixels.Slice((sy - firstY) * stride, columns);
                if (image is null)
                {
                    var bytes = callerRow[..(columns * 3)]; source.CopyRgbRow(firstX, sy, bytes);
                    for (var x = 0; x < columns; x++) row[x] = bytes[x * 3] | bytes[x * 3 + 1] << 8 | bytes[x * 3 + 2] << 16;
                }
                else if (image.BitDepth == 8)
                    JpegReferenceColor.NormalizeRgba8(image.GetRow8(sy).Slice(firstX * 4, columns * 4), row, background);
                else
                    JpegReferenceColor.NormalizeRgba16(image.GetRow16(sy).Slice(firstX * 4, columns * 4), row, background);
            }
            for (var y = 0; y < 16; y++)
            for (var x = 0; x < 16; x++)
            {
                var at = (Math.Min(top + y, height - 1) - firstY) * stride
                    + Math.Min(left + x, width - 1) - firstX;
                var rgb = pixels[at];
                mcu[(y / 8 * 2 + x / 8) * 64 + y % 8 * 8 + x % 8] =
                    JpegReferenceColor.Luma(rgb & 255, rgb >> 8 & 255, rgb >> 16 & 255);
            }
            // Every luma sample is now independent of this tile. Convert each
            // distinct chroma input once, pairing RGB before matrix rounding.
            // The affine Q6 channels stay in1024..15360 and fit two16-bit fields.
            for (var row = 0; row <= lastY - firstY; row++)
            {
                var rowPixels = pixels.Slice(row * stride, stride);
                for (var x = 0; x < columns; x += pair ? 2 : 1)
                {
                    var rgb = rowPixels[x];
                    var r = rgb & 255; var g = rgb >> 8 & 255; var b = rgb >> 16 & 255;
                    if (pair)
                    {
                        var next = rowPixels[x + 1];
                        r += next & 255; g += next >> 8 & 255; b += next >> 16 & 255;
                    }
                    JpegReferenceColor.LimitedChroma(r, g, b, pair, out var u, out var v);
                    rowPixels[x] = u | v << 16;
                }
                for (var x = 0; x < 8; x++)
                {
                    var plan = horizontalPlans[x]; var sumU = 0; var sumV = 0;
                    for (var tap = 0; tap < plan.Count; tap++)
                    {
                        var packed = rowPixels[plan.Source(tap) * (pair ? 2 : 1) - firstX];
                        sumU += (packed & 65535) * plan.Weight(tap);
                        sumV += (packed >> 16) * plan.Weight(tap);
                    }
                    // Preserve the signed full-range guard sample (it can be
                    // -1), before vertical filtering and final byte clamping.
                    horizontalChroma[x * 2] = checked((short)JpegReferenceColor.FullChromaQ7(Math.Min(32767, sumU >> 13)));
                    horizontalChroma[x * 2 + 1] = checked((short)JpegReferenceColor.FullChromaQ7(Math.Min(32767, sumV >> 13)));
                }
                // Read all horizontal taps before overwriting their Q6 inputs.
                horizontalChroma.CopyTo(MemoryMarshal.Cast<int, short>(rowPixels));
            }
            var chroma = MemoryMarshal.Cast<int, short>(pixels);
            for (var y = 0; y < chromaRows; y++)
            for (var x = 0; x < 8; x++)
            {
                ReferenceChroma(chroma, stride * 2, firstY, x, verticalPlans[y],
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
                if (entropyCache is null)
                    ReferenceBlock(coefficients, component, previous, histograms, tables, writer);
                else
                {
                    var target = entropyCache.BeginBlock();
                    var count = JpegEntropyRecords.RecordBlock(coefficients, component, previous, histograms, target);
                    entropyCache.Commit(count);
                }
            }
            entropyCache?.MarkMcuEnd();
        }
    }

    private static void ReferenceChroma(ReadOnlySpan<short> chroma, int stride, int firstY, int x,
        JpegReferenceArea vertical, out byte cb, out byte cr)
    {
        var sumU = 1 << 18; var sumV = 1 << 18;
        for (var row = 0; row < vertical.Count; row++)
        {
            var at = (vertical.Source(row) - firstY) * stride + x * 2;
            sumU += chroma[at] * vertical.Weight(row);
            sumV += chroma[at + 1] * vertical.Weight(row);
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
