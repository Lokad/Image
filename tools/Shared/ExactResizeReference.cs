using System.Buffers.Binary;
using System.Numerics;

namespace Lokad.Image.Tooling;

/// <summary>Development-only rational reference; no production resize helpers or floating-point arithmetic.</summary>
internal static class ExactResizeReference
{
    public static byte[] Resize(byte[] source, int sourceWidth, int sourceHeight, int depth,
        int width, int height, ResizeFilter filter)
    {
        var targetMaximum = filter == ResizeFilter.Nearest ? 8192 : 4096;
        if (sourceWidth is < 1 or > 4096 || sourceHeight is < 1 or > 4096
            || width < 1 || width > targetMaximum || height < 1 || height > targetMaximum || depth is not (8 or 16)
            || !Enum.IsDefined(filter)) throw new ArgumentException("Unsupported development reference geometry.");
        var sampleBytes = depth / 8; var pixelBytes = 4 * sampleBytes;
        if (source.Length != checked(sourceWidth * sourceHeight * pixelBytes)) throw new ArgumentException("Source raster length differs.");
        if (width == sourceWidth && height == sourceHeight) return (byte[])source.Clone();
        var output = new byte[checked(width * height * pixelBytes)];
        if (filter == ResizeFilter.Nearest)
        {
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var sx = (int)(((2L * x + 1) * sourceWidth) / (2L * width));
                var sy = (int)(((2L * y + 1) * sourceHeight) / (2L * height));
                source.AsSpan((sy * sourceWidth + sx) * pixelBytes, pixelBytes)
                    .CopyTo(output.AsSpan((y * width + x) * pixelBytes, pixelBytes));
            }
            return output;
        }

        var horizontal = Weights(sourceWidth, width, filter);
        var vertical = Weights(sourceHeight, height, filter);
        // Separable exact accumulation retains all intermediate precision. The
        // production implementation accumulates two-dimensional binary64 taps.
        var rows = new BigInteger[sourceHeight, width, 4];
        for (var y = 0; y < sourceHeight; y++)
        for (var x = 0; x < width; x++)
        foreach (var (sx, weight) in horizontal[x].Taps)
        {
            var offset = (y * sourceWidth + sx) * pixelBytes;
            var alpha = Read(source, offset + 3 * sampleBytes, depth);
            rows[y, x, 3] += (BigInteger)weight * alpha;
            for (var channel = 0; channel < 3; channel++)
                rows[y, x, channel] += (BigInteger)weight * alpha * Read(source, offset + channel * sampleBytes, depth);
        }
        var maximum = depth == 8 ? 255 : 65535;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var values = new BigInteger[4];
            foreach (var (sy, weight) in vertical[y].Taps)
                for (var channel = 0; channel < 4; channel++) values[channel] += weight * rows[sy, x, channel];
            var denominator = (BigInteger)horizontal[x].Sum * vertical[y].Sum;
            var alpha = Round(values[3], denominator, maximum);
            var offset = (y * width + x) * pixelBytes;
            for (var channel = 0; channel < 3; channel++)
                Write(output, offset + channel * sampleBytes, depth,
                    alpha > 0 && values[3] > 0 ? Round(values[channel], values[3], maximum) : 0);
            Write(output, offset + 3 * sampleBytes, depth, alpha);
        }
        return output;
    }

    /// <summary>Certify an exact alpha half without using production arithmetic.</summary>
    public static bool AlphaIsHalf(byte[] source, int sourceWidth, int sourceHeight, int depth,
        int width, int height, int x, int y, ResizeFilter filter)
    {
        var horizontal=Weights(sourceWidth,width,filter)[x];
        var vertical=Weights(sourceHeight,height,filter)[y];
        var sampleBytes=depth/8;BigInteger numerator=0;
        foreach(var (sx,wx) in horizontal.Taps)
        foreach(var (sy,wy) in vertical.Taps)
            numerator+=(BigInteger)wx*wy*Read(source,((sy*sourceWidth+sx)*4+3)*sampleBytes,depth);
        var denominator=(BigInteger)horizontal.Sum*vertical.Sum;
        return numerator>0 && denominator>0 && 2*(numerator%denominator)==denominator;
    }

    private sealed record Axis((int Index, long Weight)[] Taps, long Sum);

    private static Axis[] Weights(int source, int target, ResizeFilter filter)
    {
        var result = new Axis[target]; var denominator = 2L * Math.Max(source, target);
        var radius = filter == ResizeFilter.Bicubic ? 2 : 1;
        for (var destination = 0; destination < target; destination++)
        {
            var centerNumerator = (2L * destination + 1) * source - target;
            var edge = radius * denominator;
            // Broad integer bounds, then exact support membership. Boundary taps
            // have exactly zero weight, including non-binary rational centers.
            var first = Floor(centerNumerator - edge, 2L * target);
            var last = Floor(centerNumerator + edge, 2L * target) + 1;
            var merged = new SortedDictionary<int, long>();
            for (var position = first; position <= last; position++)
            {
                var n = Math.Abs(2L * target * position - centerNumerator);
                long weight;
                if (filter == ResizeFilter.Bilinear) weight = Math.Max(0, denominator - n);
                else if (n < denominator)
                    weight = checked((denominator - n) * (2 * denominator * denominator + 2 * denominator * n - 3 * n * n));
                else if (n < 2 * denominator)
                    weight = checked(-(n - denominator) * (2 * denominator - n) * (2 * denominator - n));
                else weight = 0;
                if (weight == 0) continue;
                var index = (int)Math.Clamp(position, 0, source - 1);
                merged[index] = checked(merged.GetValueOrDefault(index) + weight);
            }
            var taps = merged.Where(item => item.Value != 0).Select(item => (item.Key, item.Value)).ToArray();
            result[destination] = new Axis(taps, taps.Sum(tap => tap.Value));
            if (result[destination].Sum <= 0) throw new InvalidOperationException("Reference kernel has no positive total weight.");
        }
        return result;
    }

    private static long Floor(long numerator, long denominator)
        => numerator >= 0 ? numerator / denominator : -((-numerator + denominator - 1) / denominator);

    private static int Round(BigInteger numerator, BigInteger denominator, int maximum)
    {
        if (denominator <= 0) throw new InvalidOperationException("Nonpositive reference denominator.");
        if (numerator <= 0) return 0;
        var value = (2 * numerator + denominator) / (2 * denominator);
        return value >= maximum ? maximum : (int)value;
    }

    private static int Read(byte[] bytes, int offset, int depth)
        => depth == 8 ? bytes[offset] : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));

    private static void Write(byte[] bytes, int offset, int depth, int value)
    {
        if (depth == 8) bytes[offset] = (byte)value;
        else BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset, 2), (ushort)value);
    }
}
