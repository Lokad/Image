namespace Lokad.Image.Internal;

internal enum Vp8lTransformKind { Direct, Prediction, SubtractGreen, PredictionSubtractGreen, Palette, PalettePrediction }

internal readonly record struct Vp8lEncodingAnalysis(Vp8lTransformKind Transform, bool RedBlueZero,
    bool HasNonOpaqueAlpha, int DistinctColorsCapped, int HistogramBits, int TransformBits)
{
    private const int HistogramLength = 13 * 256;
    private const int ColorSlots = 512;
    private const long ScratchBytes = HistogramLength * 4L + ColorSlots * 5L + 256;

    private static int Tiles(int width, int height, int bits)
        => ((width + (1 << bits) - 1) >> bits) * ((height + (1 << bits) - 1) >> bits);

    private static long Entropy(ReadOnlySpan<int> histogram, OperationContext context)
    {
        context.Work(256);
        var total = 0; var nonzero = 0; var maximum = 0; long entropy = 0;
        foreach (var count in histogram)
        {
            total += count; if (count != 0) nonzero++; maximum = Math.Max(maximum, count);
            entropy -= Vp8lReferenceCost.SLog(count);
        }
        if (nonzero <= 1) return 0;
        entropy += Vp8lReferenceCost.SLog(total);
        if (nonzero == 2) return (99L * total * Vp8lReferenceCost.Scale + entropy + 50) / 100;
        var mix = nonzero == 3 ? 950 : nonzero == 4 ? 700 : 627;
        // Wide intermediates avoid overflow when a caller admits very large
        // images. The final fixed-point cost is well within Int64 capacity.
        var minimum = ((Int128)mix * (2L * total - maximum) * Vp8lReferenceCost.Scale
            + (Int128)(1000 - mix) * entropy + 500) / 1000;
        return Math.Max(entropy, (long)minimum);
    }

    internal static Vp8lEncodingAnalysis Select(ReadOnlySpan<uint> pixels, int width, int effort, OperationContext context)
    {
        if (width is < 1 or > 16384 || pixels.Length == 0 || pixels.Length % width != 0
            || pixels.Length / width > 16384) throw new ArgumentException("Expected complete VP8L image rows.");
        if (effort is < 0 or > 6) throw new ArgumentOutOfRangeException(nameof(effort));
        var height = pixels.Length / width;
        context.Work(0); context.Reserve(ScratchBytes);
        try
        {
            context.Work(0);
            var histograms = new int[HistogramLength];
            // Count at most 257 distinct colors. Fixed half-full storage needs
            // no resize, and every collision probe checks work/cancellation.
            var colors = new uint[ColorSlots]; var occupied = new byte[ColorSlots];
            var distinct = 0; var alpha = false; var previous = pixels[0];
            for (var at = 0; at < pixels.Length; at++)
            {
                context.Work(16);
                var pixel = pixels[at]; alpha |= pixel >> 24 != 255;
                if (distinct <= 256)
                {
                    // Only cardinality matters here; this table order never
                    // becomes palette order or encoded data.
                    var slot = (int)(unchecked(pixel * 0x9e3779b9u) >> 23);
                    context.Work(1);
                    while (occupied[slot] != 0 && colors[slot] != pixel)
                    { context.Work(1); slot = (slot + 1) & (ColorSlots - 1); }
                    if (occupied[slot] == 0) { occupied[slot] = 1; colors[slot] = pixel; distinct++; }
                }
                var prior = previous; previous = pixel;
                if (pixel == prior || at >= width && pixel == pixels[at - width]) continue;
                var a = (int)(pixel >> 24); var r = (int)(pixel >> 16 & 255);
                var g = (int)(pixel >> 8 & 255); var b = (int)(pixel & 255);
                var da = (a - (int)(prior >> 24)) & 255; var dr = (r - (int)(prior >> 16 & 255)) & 255;
                var dg = (g - (int)(prior >> 8 & 255)) & 255; var db = (b - (int)(prior & 255)) & 255;
                histograms[a]++; histograms[256 + r]++; histograms[512 + g]++; histograms[768 + b]++;
                histograms[1024 + da]++; histograms[1280 + dr]++; histograms[1536 + dg]++; histograms[1792 + db]++;
                histograms[2048 + ((r - g) & 255)]++; histograms[2304 + ((b - g) & 255)]++;
                histograms[2560 + ((dr - dg) & 255)]++; histograms[2816 + ((db - dg) & 255)]++;
                // The pinned multiplicative sketch selects palette entropy;
                // its parameter is not a copied implementation table.
                var bucket = (int)(unchecked((pixel + (pixel >> 19)) * 0x39c5fba7u) >> 24);
                histograms[3072 + bucket]++;
            }
            var palettePossible = distinct <= 256;
            var histogramBits = Math.Clamp((palettePossible ? 9 : 7) - effort, 2, 9);
            while (histogramBits < 9 && Tiles(width, height, histogramBits) > 2600) histogramBits++;
            while (histogramBits > 2 && Tiles(width, height, histogramBits - 1) == 1) histogramBits--;
            var transformBits = Math.Min(effort < 4 ? 6 : effort > 4 ? 4 : 5, histogramBits);
            if (effort == 0)
                return new(palettePossible ? Vp8lTransformKind.Palette : Vp8lTransformKind.PredictionSubtractGreen,
                    palettePossible, alpha, distinct, histogramBits, transformBits);
            if (palettePossible && distinct <= 16)
                return new(Vp8lTransformKind.Palette, true, alpha, distinct, histogramBits, transformBits);
            for (var plane = 4; plane < 8; plane++) histograms[plane * 256]++;
            histograms[10 * 256]++; histograms[11 * 256]++;
            Span<long> entropies = stackalloc long[13];
            for (var plane = 0; plane < 13; plane++) entropies[plane] = Entropy(histograms.AsSpan(plane * 256, 256), context);
            var tiles = Tiles(width, height, transformBits);
            Span<long> costs = stackalloc long[5];
            costs[0] = entropies[0] + entropies[1] + entropies[2] + entropies[3];
            costs[1] = entropies[4] + entropies[5] + entropies[6] + entropies[7]
                + tiles * (long)Math.Round(Vp8lReferenceCost.Scale * Math.Log2(14));
            costs[2] = entropies[0] + entropies[2] + entropies[8] + entropies[9];
            costs[3] = entropies[4] + entropies[6] + entropies[10] + entropies[11]
                + tiles * (long)Math.Round(Vp8lReferenceCost.Scale * Math.Log2(24));
            costs[4] = entropies[12] + distinct * 8L * Vp8lReferenceCost.Scale;
            var mode = 0;
            for (var candidate = 1; candidate < (palettePossible ? 5 : 4); candidate++)
                if (costs[candidate] < costs[mode]) mode = candidate;
            var redPlane = mode is 0 or 4 ? 1 : mode == 1 ? 5 : mode == 2 ? 8 : 10;
            var bluePlane = mode is 0 or 4 ? 3 : mode == 1 ? 7 : mode == 2 ? 9 : 11;
            context.Work(510); var redBlueZero = true;
            for (var i = 1; i < 256; i++)
                redBlueZero &= histograms[redPlane * 256 + i] == 0 && histograms[bluePlane * 256 + i] == 0;
            return new((Vp8lTransformKind)mode, redBlueZero, alpha, distinct, histogramBits, transformBits);
        }
        finally { context.Release(ScratchBytes); }
    }
}
