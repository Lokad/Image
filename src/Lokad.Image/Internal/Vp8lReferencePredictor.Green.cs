using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

internal static partial class Vp8lReferencePredictor
{
    private ref partial struct Analysis
    {
        // A view of this owner's admitted storage, not another owner. Sixteen
        // padded green banks leave room for 256 temporary long history products.
        // Those products are prepared only after all children finish merging.
        private const int GreenHistogramSize = 16 * 256;
        private const int GreenCounts = 14 * 256;
        private const int GreenLevelSize = GreenHistogramSize + 258 + 14;
        private Span<int> GreenHistogram(int level) => data.Slice(level * GreenLevelSize, GreenHistogramSize);
        private Span<int> GreenHistory(int level) => data.Slice(level * GreenLevelSize + GreenHistogramSize, 258);
        private Span<int> GreenFrequency(int level) => data.Slice(level * GreenLevelSize + GreenHistogramSize + 258, 14);

        private void VisitGreen(ReadOnlySpan<uint> pixels, int level, int x0, int y0)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var bits = minBits + level; var histogram = GreenHistogram(level);
            histogram[..GreenCounts].Clear();
            if (level == 0)
                Vp8lCompactGreenHistogram.Build(pixels, width, height, x0, y0, bits, histogram, context);
            else
            {
                var half = 1 << (bits - 1);
                for (var child = 0; child < 4; child++)
                {
                    var childX = x0 + (child & 1) * half; var childY = y0 + (child >> 1) * half;
                    if (childX >= width || childY >= height) continue;
                    VisitGreen(pixels, level - 1, childX, childY);
                    var lower = GreenHistogram(level - 1);
                    context.Cancellation.ThrowIfCancellationRequested();
                    for (var i = 0; i < GreenCounts; i++) histogram[i] += lower[i];
                }
            }
            var endX = Math.Min(x0 + (1 << bits), width); var endY = Math.Min(y0 + (1 << bits), height);
            var samples = (endX - x0) * (endY - y0);
            var origin = x0 == 0 && y0 == 0 ? 1 : 0;
            var black = (endX - Math.Max(x0, 1)) * (endY - Math.Max(y0, 1)) + origin;
            var history = GreenHistory(level); var modes = Map(level);
            var tx = x0 >> bits; var ty = y0 >> bits; var tile = ty * columns[level] + tx;
            var products = MemoryMarshal.Cast<int, long>(histogram[GreenCounts..]);
            var costs = Vp8lReferenceCost.PrepareGreen(history, samples, black, origin, products, context);
            var chosen = 0; var best = long.MaxValue;
            for (var mode = 0; mode < 14; mode++)
            {
                var cost = Vp8lReferenceCost.PredictionGreen(histogram.Slice(mode * 256, 256),
                    history[..256], products, mode == 0 ? costs.Black : costs.Other, context);
                if (tx != 0 && modes[tile - 1] == mode) cost -= 15 * Vp8lReferenceCost.Scale;
                if (ty != 0 && modes[tile - columns[level]] == mode) cost -= 15 * Vp8lReferenceCost.Scale;
                if (cost < best) { best = cost; chosen = mode; }
            }
            modes[tile] = (byte)chosen; GreenFrequency(level)[chosen]++;
            context.Cancellation.ThrowIfCancellationRequested();
            for (var i = 0; i < 256; i++) history[i] += histogram[chosen * 256 + i];
            var ones = chosen == 0 ? black : origin;
            history[256] += samples - ones; history[257] += ones;
        }

        internal Vp8lPredictionPlan SelectGreen(ReadOnlySpan<uint> pixels)
        {
            for (var y = 0; y < height; y += 1 << maxBits)
            for (var x = 0; x < width; x += 1 << maxBits) VisitGreen(pixels, levels - 1, x, y);
            var winner = 0; var best = long.MaxValue;
            for (var level = 0; level < levels; level++)
            {
                var history = GreenHistory(level);
                var cost = Vp8lReferenceCost.Shannon(GreenFrequency(level), context)
                    + Vp8lReferenceCost.Shannon(history[..256], context)
                    + Vp8lReferenceCost.Shannon(history[256..], context);
                if (cost < best) { best = cost; winner = level; }
            }
            return FinishSelection(winner, best);
        }
    }
}
