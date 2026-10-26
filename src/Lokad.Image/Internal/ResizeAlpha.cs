using System.Numerics;

namespace Lokad.Image.Internal;

/// <summary>Resolve the zero/nonzero alpha boundary without binary64 half-tie ambiguity.</summary>
internal static class ResizeAlpha
{
    // With Int32 dimensions, cubic numerators need at most 100 bits. Two axes,
    // UInt16 alpha and all admitted taps keep sums below 300 bits. This charge
    // conservatively covers the live BigInteger operands and arithmetic scratch;
    // there are no coefficient arrays or source-sized temporary buffers.
    internal const int ScratchBytes = 4096;

    public static int Round(ImageBuffer source, int x, int y, int width, int height,
        ResizeFilter filter, OperationContext context)
    {
        context.Reserve(ScratchBytes);
        try
        {
            var radius = filter == ResizeFilter.Bicubic ? 2 : 1;
            var dx = 2L * Math.Max(source.Width, width); var dy = 2L * Math.Max(source.Height, height);
            var cx = ((Int128)2 * x + 1) * source.Width - width;
            var cy = ((Int128)2 * y + 1) * source.Height - height;
            var firstX = Floor(cx - radius * dx, 2L * width);
            var lastX = Floor(cx + radius * dx, 2L * width) + 1;
            var firstY = Floor(cy - radius * dy, 2L * height);
            var lastY = Floor(cy + radius * dy, 2L * height) + 1;
            BigInteger alpha = 0, total = 0;
            for (var sy = firstY; sy <= lastY; sy++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var wy = Weight(Int128.Abs((Int128)2 * height * sy - cy), dy, filter);
                var iy = (int)Math.Clamp(sy, 0, source.Height - 1);
                for (var sx = firstX; sx <= lastX; sx++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    if (wy == 0) continue;
                    var wx = Weight(Int128.Abs((Int128)2 * width * sx - cx), dx, filter);
                    if (wx == 0) continue;
                    var weight = (BigInteger)wx * (BigInteger)wy;
                    total += weight;
                    alpha += weight * source.Sample((int)Math.Clamp(sx, 0, source.Width - 1), iy, 3);
                }
            }
            // Common axis denominators cancel. Round a positive rational half
            // away from zero, then clamp, independently of the floating path.
            if (total <= 0) throw new ImageDataException("Resize kernel has no positive total weight.");
            if (alpha <= 0) return 0;
            var rounded = (alpha * 2 + total) / (total * 2);
            var maximum = source.BitDepth == 8 ? 255 : 65535;
            return rounded >= maximum ? maximum : (int)rounded;
        }
        finally { context.Release(ScratchBytes); }
    }

    private static long Floor(Int128 numerator, long denominator)
        => (long)(numerator >= 0 ? numerator / denominator : -((-numerator + denominator - 1) / denominator));

    private static Int128 Weight(Int128 n, Int128 d, ResizeFilter filter)
    {
        if (filter == ResizeFilter.Bilinear) return n < d ? d - n : 0;
        // Catmull-Rom B=0,C=1/2, multiplied by the common 2*d^3 denominator.
        if (n < d) return 3 * n * n * n - 5 * n * n * d + 2 * d * d * d;
        if (n < 2 * d) return -n * n * n + 5 * n * n * d - 8 * n * d * d + 4 * d * d * d;
        return 0;
    }
}
