using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

// A temporary view over an unpublished result suffix, already admitted by the
// result owner. A band may read it only while every output byte precedes it.
internal readonly ref struct MixedBicubicGeometry
{
    private readonly Span<double> values;
    private readonly Span<int> indices;
    private readonly int sourceWidth;
    internal readonly int SafeRows;

    private MixedBicubicGeometry(Span<double> values, Span<int> indices, int sourceWidth, int safeRows)
    {
        this.values = values; this.indices = indices; this.sourceWidth = sourceWidth; SafeRows = safeRows;
    }

    internal static long StorageBytes(int width) => (long)width * (4 * sizeof(double) + sizeof(int));

    internal static MixedBicubicGeometry Prepare(ImageBuffer source, ImageBuffer result,
        int bandRows, CancellationToken cancellation)
    {
        // Four weights and packed first/count. A fifth tap is retained only when
        // its original weight is exactly +0. Centers, ordered normalizers and
        // clamped indices reconstruct exactly without stored copies.
        var bytes = StorageBytes(result.Width);
        if (result.Width <= source.Width || bytes >= result.ByteLength) return default;
        var start = (int)(result.ByteLength - bytes);
        var safeRows = start / result.Stride;
        if (safeRows < 2 * bandRows) return default;
        var suffix = result.Pixels[start..];
        var doubleBytes = result.Width * 4 * sizeof(double);
        var values = MemoryMarshal.Cast<byte, double>(suffix[..doubleBytes]);
        var indices = MemoryMarshal.Cast<byte, int>(suffix[doubleBytes..]);
        for (var x = 0; x < result.Width; x++)
        {
            cancellation.ThrowIfCancellationRequested();
            var center = (x + .5) * source.Width / result.Width - .5;
            var first = (int)Math.Ceiling(center - 2);
            var count = (int)Math.Floor(center + 2) - first + 1;
            if (count is < 4 or > 5 || first < int.MinValue / 2 || first > int.MaxValue / 2) return default;
            indices[x] = (first << 1) | (count - 4);
            for (var t = 0; t < count; t++)
            {
                var w = ImageTransforms.Kernel(first + t - center, ResizeFilter.Bicubic);
                if (t < 4) values[x * 4 + t] = w;
                else if (BitConverter.DoubleToInt64Bits(w) != 0) return default;
            }
        }
        return new(values, indices, source.Width, safeRows);
    }

    internal void Read(int x, Span<double> weights, Span<int> samples,
        out double center, out int first, out int count, out double normalizer)
    {
        center = (x + .5) * sourceWidth / indices.Length - .5;
        var packed = indices[x]; first = packed >> 1; count = 4 + (packed & 1);
        values.Slice(x * 4, 4).CopyTo(weights);
        if (count == 5) weights[4] = 0;
        normalizer = 0;
        for (var t = 0; t < count; t++)
        {
            normalizer += weights[t];
            samples[t] = Math.Clamp(first + t, 0, sourceWidth - 1);
        }
    }
}
