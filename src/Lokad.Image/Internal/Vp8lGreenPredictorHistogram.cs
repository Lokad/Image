using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

// ALPH preparation supplies pixels with A/R/B zero and filtered alpha in green.
// The caller admits and clears the complete histogram; all storage stays borrowed.
internal static class Vp8lGreenPredictorHistogram
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Add(Span<int> histogram, int mode, int residual)
        => histogram[mode * 1024 + 512 + (residual & 255)]++;

    // Every mode consumes the same neighbours. Share their loads and repeated
    // averages, preserving the individual predictors' rounding and tie rules.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AccumulateInterior(Span<int> histogram, int actual,
        int left, int top, int corner, int right)
    {
        var leftCorner = (left + corner) / 2;
        var leftTop = (left + top) / 2;
        var topRight = (top + right) / 2;
        Add(histogram, 0, actual);
        Add(histogram, 1, actual - left);
        Add(histogram, 2, actual - top);
        Add(histogram, 3, actual - right);
        Add(histogram, 4, actual - corner);
        Add(histogram, 5, actual - ((left + right) / 2 + top) / 2);
        Add(histogram, 6, actual - leftCorner);
        Add(histogram, 7, actual - leftTop);
        Add(histogram, 8, actual - (corner + top) / 2);
        Add(histogram, 9, actual - topRight);
        Add(histogram, 10, actual - (leftCorner + topRight) / 2);
        Add(histogram, 11, actual - (Math.Abs(top - corner) < Math.Abs(left - corner) ? left : top));
        Add(histogram, 12, actual - Math.Clamp(left + top - corner, 0, 255));
        Add(histogram, 13, actual - Math.Clamp(leftTop + (leftTop - corner) / 2, 0, 255));
    }

    internal static void Build(ReadOnlySpan<uint> pixels, int width, int height,
        int x0, int y0, int bits, Span<int> histogram, OperationContext context)
    {
        var endX = Math.Min(x0 + (1 << bits), width); var endY = Math.Min(y0 + (1 << bits), height);
        var count = (endX - x0) * (endY - y0); var first = x0 == 0 && y0 == 0 ? 1 : 0;
        var interior = (endX - Math.Max(x0, 1)) * (endY - Math.Max(y0, 1));
        for (var mode = 0; mode < 14; mode++)
        {
            // Input A/R/B are zero. Mode0 predicts black A255 in the
            // interior; every mode predicts that black at the global origin.
            var ones = mode == 0 ? interior + first : first;
            histogram[mode * 1024] = count - ones;
            histogram[mode * 1024 + 1] = ones;
            histogram[mode * 1024 + 256] = count;
            histogram[mode * 1024 + 768] = count;
        }
        for (var y = y0; y < endY; y++)
        for (var x = x0; x < endX; x++)
        {
            context.Work(56);
            var at = y * width + x; var actual = (int)(pixels[at] >> 8);
            if (y == 0 || x == 0)
            {
                var prediction = y == 0 ? x == 0 ? 0 : (int)(pixels[at - 1] >> 8) : (int)(pixels[at - width] >> 8);
                for (var mode = 0; mode < 14; mode++) Add(histogram, mode, actual - prediction);
            }
            else AccumulateInterior(histogram, actual,
                (int)(pixels[at - 1] >> 8), (int)(pixels[at - width] >> 8),
                (int)(pixels[at - width - 1] >> 8), (int)(pixels[x == width - 1 ? y * width : at - width + 1] >> 8));
        }
    }
}
