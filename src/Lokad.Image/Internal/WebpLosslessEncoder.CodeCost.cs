namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    // RFC9649 sections3.5.1/3.7: refine one uncached predictor map against the
    // selected actual channel codes, rather than a separate tile-local entropy.
    private static CachedPlan SelectCodeCostPlan(ImageBuffer image, int effort, uint[] cache,
        CachedPlan baseline, OperationContext context)
    {
        if (effort < 4 || !baseline.Predict || baseline.UseCache || baseline.Modes is not { } oldModes
            || baseline.ModeTable is not { } oldModeTable || baseline.Groups is null
            || baseline.Color is null || baseline.References is not null) return baseline;
        var tiles = oldModes.Length; var modeBytes = tiles + 32L;
        var scratch = TileHistograms.StorageBytes(tiles) + tiles * 9L + 112
            + MaximumGroups * EntropyAlphabet * 4L + 200_000;
        var work = (long)image.Width * image.Height * 4096 + tiles * 32768L + 1_000_000;
        if (modeBytes + 1024 + CachedWorkingBytes + scratch > context.RemainingWorkingBytes
            || work > context.RemainingWork) return baseline;
        context.Reserve(modeBytes);
        WebpPrefixCode? modeTable = null; var retained = false;
        try
        {
            context.Work(0); var modes = new byte[tiles];
            SelectCodeCostModes(image, modes, baseline, context);
            context.Reserve(280 * 4 + 32);
            try
            {
                context.Work(0); var frequencies = new int[280]; context.Work(tiles);
                foreach (var mode in modes) frequencies[mode]++;
                modeTable = new(frequencies, context);
            }
            finally { context.Release(280 * 4 + 32); }
            var candidate = PrepareCachedTables(image, effort, cache, true, context,
                modes, modeTable, baseline.Color, useCache: false) with { Alpha = baseline.Alpha };
            candidate = SelectEntropyPlan(image, effort, cache, candidate, context);
            // The coefficient owner is borrowed by BOTH plans; only the losing
            // coding and predictor owners end here. Its capacity stays charged.
            if (PayloadUpper(image, candidate, context) + 2 < PayloadUpper(image, baseline, context))
            {
                ReleaseCodingPlan(baseline, context);
                context.Release(oldModes.Length + 32L + oldModeTable.MemoryBytes);
                retained = true; return candidate;
            }
            ReleaseCodingPlan(candidate, context); return baseline;
        }
        finally
        {
            if (!retained) context.Release(modeBytes + (modeTable?.MemoryBytes ?? 0));
        }
    }

    private static void SelectCodeCostModes(ImageBuffer image, Span<byte> modes,
        CachedPlan baseline, OperationContext context)
    {
        var columns = (image.Width + 7) >> 3; var rows = (image.Height + 7) >> 3;
        context.Reserve(1024);
        try
        {
            context.Work(0);
            Span<uint> pixels = stackalloc uint[90]; Span<uint> first = stackalloc uint[8];
            for (var ty = 0; ty < rows; ty++)
            for (var tx = 0; tx < columns; tx++)
            {
                var x0 = tx * 8; var y0 = ty * 8;
                var width = Math.Min(8, image.Width - x0); var height = Math.Min(8, image.Height - y0);
                for (var dy = -1; dy < 8; dy++)
                for (var dx = -1; dx <= 8; dx++)
                {
                    var x = x0 + dx; var y = y0 + dy;
                    pixels[(dy + 1) * 10 + dx + 1] = (uint)x < image.Width && (uint)y < image.Height
                        ? GreenPixel(image, y * image.Width + x, context) : 0;
                }
                for (var dy = 0; dy < height; dy++) first[dy] = GreenPixel(image, (y0 + dy) * image.Width, context);
                var tile = ty * columns + tx; var coefficients = baseline.Color!.Coefficients.AsSpan(tile * 3, 3);
                var tables = baseline.Tables.AsSpan(baseline.Groups!.Map[tile] * 5, 5);
                var best = baseline.Modes![tile];
                var bestScore = CodeCostScore(image, x0, y0, width, height, best, pixels, first,
                    tables, baseline.ModeTable!, coefficients, context);
                for (var mode = 0; mode < 14; mode++)
                {
                    if (mode == baseline.Modes[tile]) continue;
                    var score = CodeCostScore(image, x0, y0, width, height, mode, pixels, first,
                        tables, baseline.ModeTable!, coefficients, context);
                    if (score < bestScore) { bestScore = score; best = (byte)mode; }
                }
                modes[tile] = best;
            }
        }
        finally { context.Release(1024); }
    }

    private static long CodeCostScore(ImageBuffer image, int x0, int y0, int width, int height, int mode,
        ReadOnlySpan<uint> pixels, ReadOnlySpan<uint> first, ReadOnlySpan<WebpPrefixCode> tables,
        WebpPrefixCode modeTable, ReadOnlySpan<byte> coefficients, OperationContext context)
    {
        static int Cost(WebpPrefixCode table, int symbol) => table.Length(symbol) == 0 ? 15 : table.BitLength(symbol);
        long score = Cost(modeTable, mode);
        for (var dy = 0; dy < height; dy++)
        for (var dx = 0; dx < width; dx++)
        {
            context.Work(64);
            var x = x0 + dx; var y = y0 + dy; var at = (dy + 1) * 10 + dx + 1;
            var prediction = y == 0 ? x == 0 ? 0xff000000u : pixels[at - 1]
                : x == 0 ? pixels[at - 10]
                : WebpLosslessDecoder.Predict(mode, pixels[at - 1], pixels[at - 10], pixels[at - 11],
                    x == image.Width - 1 ? first[dy] : pixels[at - 9]);
            var residual = ColorResidual(SubtractChannels(pixels[at], prediction),
                unchecked((sbyte)coefficients[0]), unchecked((sbyte)coefficients[1]), unchecked((sbyte)coefficients[2]));
            score += Cost(tables[0], (int)(residual >> 8 & 255)) + Cost(tables[1], (int)(residual >> 16 & 255))
                + Cost(tables[2], (int)(residual & 255)) + Cost(tables[3], (int)(residual >> 24));
        }
        return score;
    }
}
