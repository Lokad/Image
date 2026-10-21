namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    private const int PredictorBits = 3;

    private static CachedPlan SelectAdaptivePlan(ImageBuffer image, int effort, uint[] cache, CachedPlan baseline, OperationContext context)
    {
        var columns = (image.Width + 7) >> PredictorBits; var rows = (image.Height + 7) >> PredictorBits;
        var modeBytes = columns * (long)rows + 32;
        // Local admission is chosen before shared reservation. Includes the
        // staged baseline, map construction and subsequent complete traversals.
        var work = (long)image.Width * image.Height * 4096 + columns * (long)rows * 32768 + 1_000_000;
        if (effort < 4 || modeBytes + 8192 + CachedWorkingBytes > context.RemainingWorkingBytes || work > context.RemainingWork)
            return baseline;
        context.Reserve(modeBytes); context.Work(0);
        var modes = new byte[columns * rows];
        SelectPredictorModes(image, modes, context);
        context.Reserve(280 * 4 + 32); WebpPrefixCode modeTable;
        try
        {
            var frequencies = new int[280]; context.Work(modes.Length);
            foreach (var mode in modes) frequencies[mode]++;
            modeTable = new WebpPrefixCode(frequencies, context);
        }
        finally { context.Release(280 * 4 + 32); }
        var candidate = PrepareCachedTables(image, effort, cache, true, context, modes, modeTable) with { Alpha = baseline.Alpha };
        candidate = SelectEntropyPlan(image, effort, cache, candidate, context);
        if (PayloadUpper(image, candidate, context) + 2 < PayloadUpper(image, baseline, context))
        {
            ReleasePlan(baseline, context); return candidate;
        }
        ReleasePlan(candidate, context); return baseline;
    }

    // RFC 9649 section 3.5.1. A tile-local Shannon estimate ranks all fourteen
    // normative predictors. Only one map is built; full header/body costs decide
    // whether it replaces LEFT. Source pixels and every residual remain exact.
    internal static void SelectPredictorModes(ImageBuffer image, Span<byte> modes, OperationContext context)
    {
        var columns = (image.Width + 7) >> PredictorBits; var rows = (image.Height + 7) >> PredictorBits;
        if (modes.Length != columns * rows) throw new ArgumentException("Predictor map dimensions differ.", nameof(modes));
        context.Reserve(8192);
        try
        {
            context.Work(0);
            Span<int> frequencies = stackalloc int[1024];
            Span<uint> pixels = stackalloc uint[90]; Span<uint> first = stackalloc uint[8];
            Span<double> entropy = stackalloc double[65]; entropy[0] = 0;
            for (var n = 1; n <= 64; n++) entropy[n] = n * Math.Log2(n);
            ReadOnlySpan<byte> order = [1,0,2,3,4,5,6,7,8,9,10,11,12,13];
            for (var ty = 0; ty < rows; ty++)
            for (var tx = 0; tx < columns; tx++)
            {
                var x0 = tx * 8; var y0 = ty * 8; var width = Math.Min(8, image.Width - x0); var height = Math.Min(8, image.Height - y0);
                for (var dy = -1; dy < 8; dy++)
                for (var dx = -1; dx <= 8; dx++)
                {
                    var x = x0 + dx; var y = y0 + dy;
                    pixels[(dy + 1) * 10 + dx + 1] = (uint)x < image.Width && (uint)y < image.Height
                        ? GreenPixel(image, y * image.Width + x, context) : 0;
                }
                for (var dy = 0; dy < height; dy++) first[dy] = GreenPixel(image, (y0 + dy) * image.Width, context);
                var best = 1; var bestScore = double.PositiveInfinity;
                foreach (var mode in order)
                {
                    context.Work(1024); frequencies.Clear();
                    for (var dy = 0; dy < height; dy++)
                    for (var dx = 0; dx < width; dx++)
                    {
                        context.Work(48);
                        var x = x0 + dx; var y = y0 + dy; var at = (dy + 1) * 10 + dx + 1;
                        var prediction = y == 0 ? x == 0 ? 0xff000000u : pixels[at - 1]
                            : x == 0 ? pixels[at - 10]
                            : WebpLosslessDecoder.Predict(mode, pixels[at - 1], pixels[at - 10], pixels[at - 11],
                                x == image.Width - 1 ? first[dy] : pixels[at - 9]);
                        var residual = SubtractChannels(pixels[at], prediction);
                        for (var shift = 0; shift < 32; shift += 8) frequencies[shift * 32 + (int)(residual >> shift & 255)]++;
                    }
                    context.Work(1024); var score = 0d;
                    foreach (var frequency in frequencies) score -= entropy[frequency];
                    // n*log2(n) is identical for all candidates in this tile.
                    if (score < bestScore) { bestScore = score; best = mode; }
                }
                modes[ty * columns + tx] = (byte)best;
            }
        }
        finally { context.Release(8192); }
    }

    internal static uint AdaptiveResidual(ImageBuffer image, int position, ReadOnlySpan<byte> modes, OperationContext context)
    {
        var x = position % image.Width; var y = position / image.Width;
        var pixel = GreenPixel(image, position, context); uint prediction;
        context.Work(32);
        if (y == 0) prediction = x == 0 ? 0xff000000u : GreenPixel(image, position - 1, context);
        else if (x == 0) prediction = GreenPixel(image, position - image.Width, context);
        else
        {
            var columns = (image.Width + 7) >> PredictorBits;
            var mode = modes[(y >> PredictorBits) * columns + (x >> PredictorBits)];
            prediction = WebpLosslessDecoder.Predict(mode,
                GreenPixel(image, position - 1, context), GreenPixel(image, position - image.Width, context),
                GreenPixel(image, position - image.Width - 1, context),
                GreenPixel(image, x == image.Width - 1 ? y * image.Width : position - image.Width + 1, context));
        }
        return SubtractChannels(pixel, prediction);
    }

    private static uint SubtractChannels(uint pixel, uint prediction)
        => unchecked(((pixel - prediction) & 255)
            | (((pixel >> 8) - (prediction >> 8)) & 255) << 8
            | (((pixel >> 16) - (prediction >> 16)) & 255) << 16
            | (((pixel >> 24) - (prediction >> 24)) & 255) << 24);

    private static void AdaptivePredictorHeader(byte[] modes, WebpPrefixCode modeTable, WebpBitWriter writer, OperationContext context)
    {
        writer.Write(1, 1); writer.Write(0, 2); writer.Write(PredictorBits - 2, 3);
        writer.Write(0, 1); // Auxiliary image: no cache; no recursive spatial-group flag.
        modeTable.Header(writer, context); SingleHeader(writer, 0); SingleHeader(writer, 0);
        SingleHeader(writer, 255); SingleHeader(writer, 0);
        foreach (var mode in modes) modeTable.Write(writer, mode);
    }
}
