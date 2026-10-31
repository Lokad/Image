using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

// A temporary view over an unpublished result suffix, already admitted by the
// result owner. A band may read it only while every output byte precedes it.
internal readonly ref struct MixedBicubicGeometry
{
    private readonly Span<double> values;
    private readonly Span<int> indices;
    internal readonly int SafeRows;

    private MixedBicubicGeometry(Span<double> values, Span<int> indices, int safeRows)
    {
        this.values = values; this.indices = indices; SafeRows = safeRows;
    }

    internal static MixedBicubicGeometry Prepare(ImageBuffer source, ImageBuffer result,
        int bandRows, CancellationToken cancellation)
    {
        // Seven double/int fields per column: center, normalizer, five weights;
        // first, count, five clamped source indices. Unused tap fields stay private.
        var bytes = (long)result.Width * 7 * (sizeof(double) + sizeof(int));
        if (result.Width <= source.Width || bytes >= result.ByteLength) return default;
        var start = (int)(result.ByteLength - bytes);
        var safeRows = start / result.Stride;
        if (safeRows < 2 * bandRows) return default;
        var suffix = result.Pixels[start..];
        var doubleBytes = result.Width * 7 * sizeof(double);
        var values = MemoryMarshal.Cast<byte, double>(suffix[..doubleBytes]);
        var indices = MemoryMarshal.Cast<byte, int>(suffix[doubleBytes..]);
        for (var x = 0; x < result.Width; x++)
        {
            cancellation.ThrowIfCancellationRequested();
            var center = (x + .5) * source.Width / result.Width - .5;
            var first = (int)Math.Ceiling(center - 2);
            var count = (int)Math.Floor(center + 2) - first + 1;
            var at = x * 7;
            values[at] = center; indices[at] = first; indices[at + 1] = count;
            double normalizer = 0;
            for (var t = 0; t < count; t++)
            {
                var w = ImageTransforms.Kernel(first + t - center, ResizeFilter.Bicubic);
                values[at + 2 + t] = w; normalizer += w;
                indices[at + 2 + t] = Math.Clamp(first + t, 0, source.Width - 1);
            }
            values[at + 1] = normalizer;
        }
        return new(values, indices, safeRows);
    }

    internal void Read(int x, Span<double> weights, Span<int> samples,
        out double center, out int first, out int count, out double normalizer)
    {
        var v = values.Slice(x * 7, 7); var i = indices.Slice(x * 7, 7);
        center = v[0]; first = i[0]; count = i[1]; normalizer = v[1];
        v.Slice(2, count).CopyTo(weights); i.Slice(2, count).CopyTo(samples);
    }
}
