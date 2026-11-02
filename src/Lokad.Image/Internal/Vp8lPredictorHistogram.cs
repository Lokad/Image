using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

// Full-color leaf histograms for RFC 9649 section 3.5.1. Share the neighbours
// and floor averages across all fourteen modes; the selector still owns their
// histories and decisions. The caller clears and admits the borrowed histogram.
internal static class Vp8lPredictorHistogram
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Average(uint first, uint second)
        => (first & second) + (((first ^ second) & 0xfefefefeu) >> 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Add(Span<int> histogram, int mode, uint actual, uint prediction)
    {
        var offset = mode * 1024;
        histogram[offset + (int)(unchecked((actual >> 24) - (prediction >> 24)) & 255)]++;
        histogram[offset + 256 + (int)(unchecked((actual >> 16) - (prediction >> 16)) & 255)]++;
        histogram[offset + 512 + (int)(unchecked((actual >> 8) - (prediction >> 8)) & 255)]++;
        histogram[offset + 768 + (int)(unchecked(actual - prediction) & 255)]++;
    }

    internal static void AccumulateInterior(Span<int> histogram, uint actual,
        uint left, uint top, uint corner, uint right)
    {
        var leftCorner = Average(left, corner); var leftTop = Average(left, top);
        var topRight = Average(top, right);
        Add(histogram, 0, actual, 0xff000000u);
        Add(histogram, 1, actual, left);
        Add(histogram, 2, actual, top);
        Add(histogram, 3, actual, right);
        Add(histogram, 4, actual, corner);
        Add(histogram, 5, actual, Average(Average(left, right), top));
        Add(histogram, 6, actual, leftCorner);
        Add(histogram, 7, actual, leftTop);
        Add(histogram, 8, actual, Average(corner, top));
        Add(histogram, 9, actual, topRight);
        Add(histogram, 10, actual, Average(leftCorner, topRight));
        uint full = 0, half = 0; var leftError = 0; var topError = 0;
        for (var shift = 0; shift < 32; shift += 8)
        {
            var l = (int)(left >> shift & 255); var t = (int)(top >> shift & 255);
            var c = (int)(corner >> shift & 255); var average = (int)(leftTop >> shift & 255);
            leftError += Math.Abs(t - c); topError += Math.Abs(l - c);
            full |= (uint)Math.Clamp(l + t - c, 0, 255) << shift;
            // Division truncates towards zero, including negative differences.
            half |= (uint)Math.Clamp(average + (average - c) / 2, 0, 255) << shift;
        }
        Add(histogram, 11, actual, leftError < topError ? left : top);
        Add(histogram, 12, actual, full);
        Add(histogram, 13, actual, half);
    }

    internal static void Build(ReadOnlySpan<uint> pixels, int width, int height,
        int x0, int y0, int bits, Span<int> histogram, OperationContext context)
    {
        var endX = Math.Min(x0 + (1 << bits), width); var endY = Math.Min(y0 + (1 << bits), height);
        for (var y = y0; y < endY; y++)
        for (var x = x0; x < endX; x++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var at = y * width + x; var actual = pixels[at];
            if (y == 0 || x == 0)
            {
                var prediction = y == 0 ? x == 0 ? 0xff000000u : pixels[at - 1] : pixels[at - width];
                for (var mode = 0; mode < 14; mode++) Add(histogram, mode, actual, prediction);
            }
            else AccumulateInterior(histogram, actual, pixels[at - 1], pixels[at - width],
                pixels[at - width - 1], pixels[x == width - 1 ? y * width : at - width + 1]);
        }
    }
}
