namespace Lokad.Image.Internal;

// Callers reserve owned storage before allocation and retain their array locals.
// Equal dimensions and non-null arrays remain visible to their execution loops.
internal static class BilinearVerticalGeometry
{
    internal const int Taps = 6; // Inclusive triangle support at scale <= 2.5.

    internal static long StorageBytes(int height)
        => (long)height * Taps * sizeof(double)
            + (long)height * (2 * sizeof(int) + sizeof(double)) + 4 * 64;

    internal static void Prepare(int sourceHeight, int height, double scaleY,
        double[] weights, int[] firstRows, int[] counts, double[] normalizers, CancellationToken cancellation)
    {
        for (var y = 0; y < height; y++)
        {
            cancellation.ThrowIfCancellationRequested();
            var center = (y + .5) * sourceHeight / height - .5;
            var first = (int)Math.Ceiling(center - scaleY);
            var count = (int)Math.Floor(center + scaleY) - first + 1;
            firstRows[y] = first; counts[y] = count;
            double normalizer = 0;
            for (var t = 0; t < count; t++)
            {
                var wy = ImageTransforms.Kernel((first + t - center) / scaleY, ResizeFilter.Bilinear);
                weights[y * Taps + t] = wy; normalizer += wy;
            }
            normalizers[y] = normalizer;
        }
    }
}
