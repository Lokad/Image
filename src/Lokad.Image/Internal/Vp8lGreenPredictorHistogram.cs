namespace Lokad.Image.Internal;

// ALPH preparation supplies pixels with A/R/B zero and filtered alpha in green.
// The caller admits and clears the complete histogram; all storage stays borrowed.
internal static class Vp8lGreenPredictorHistogram
{
    internal static int Predict(int mode, int left, int top, int corner, int right)
    {
        static int Average(int a, int b) => (a + b) / 2;
        static int Half(int a, int c) => Math.Clamp(a + (a - c) / 2, 0, 255);
        return mode switch {
            0 => 0, 1 => left, 2 => top, 3 => right, 4 => corner,
            5 => Average(Average(left, right), top), 6 => Average(left, corner),
            7 => Average(left, top), 8 => Average(corner, top), 9 => Average(top, right),
            10 => Average(Average(left, corner), Average(top, right)),
            11 => Math.Abs(top - corner) < Math.Abs(left - corner) ? left : top,
            12 => Math.Clamp(left + top - corner, 0, 255),
            13 => Half(Average(left, top), corner), _ => throw new ArgumentOutOfRangeException(nameof(mode)) };
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
            for (var mode = 0; mode < 14; mode++)
            {
                var prediction = y == 0 ? x == 0 ? 0 : (int)(pixels[at - 1] >> 8)
                    : x == 0 ? (int)(pixels[at - width] >> 8)
                    : Predict(mode, (int)(pixels[at - 1] >> 8), (int)(pixels[at - width] >> 8),
                        (int)(pixels[at - width - 1] >> 8), (int)(pixels[x == width - 1 ? y * width : at - width + 1] >> 8));
                histogram[mode * 1024 + 512 + ((actual - prediction) & 255)]++;
            }
        }
    }
}
