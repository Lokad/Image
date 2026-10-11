using Lokad.Image.Internal;

namespace Lokad.Image;

public enum ResizeFilter { Nearest, Bilinear, Bicubic }
public enum QuarterTurn { Clockwise90, Half, CounterClockwise90 }

/// <summary>Transforms return independently owned buffers and retain the source precision.</summary>
public static class ImageTransforms
{
    public static ImageBuffer Crop(ImageBuffer source, int x, int y, int width, int height,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (x < 0 || y < 0 || width < 1 || height < 1 || (long)x + width > source.Width
            || (long)y + height > source.Height)
            throw new ArgumentOutOfRangeException(nameof(width), "Crop rectangle must fit the source.");
        return Map(source, width, height, (dx, dy) => (dx + x, dy + y), limits, cancellationToken);
    }

    public static ImageBuffer CropCentered(ImageBuffer source, int width, int height,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        return Crop(source, (source.Width - width) / 2, (source.Height - height) / 2, width, height, limits, cancellationToken);
    }

    public static ImageBuffer CropClamped(ImageBuffer source, int x, int y, int width, int height,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (width < 1 || height < 1 || width > source.Width || height > source.Height)
            throw new ArgumentOutOfRangeException(nameof(width));
        return Crop(source, Math.Clamp(x, 0, source.Width - width), Math.Clamp(y, 0, source.Height - height),
            width, height, limits, cancellationToken);
    }

    public static ImageBuffer FlipHorizontal(ImageBuffer source, ImageLimits? limits = null,
        CancellationToken cancellationToken = default)
        => Orient(source, ImageOrientation.MirrorHorizontal, limits, cancellationToken);

    public static ImageBuffer FlipVertical(ImageBuffer source, ImageLimits? limits = null,
        CancellationToken cancellationToken = default)
        => Orient(source, ImageOrientation.MirrorVertical, limits, cancellationToken);

    public static ImageBuffer Rotate(ImageBuffer source, QuarterTurn turn, ImageLimits? limits = null,
        CancellationToken cancellationToken = default)
        => Orient(source, turn switch
        {
            QuarterTurn.Clockwise90 => ImageOrientation.Rotate90,
            QuarterTurn.Half => ImageOrientation.Rotate180,
            QuarterTurn.CounterClockwise90 => ImageOrientation.Rotate270,
            _ => throw new ArgumentOutOfRangeException(nameof(turn))
        }, limits, cancellationToken);

    public static ImageBuffer Transpose(ImageBuffer source, bool mirrored = false, ImageLimits? limits = null,
        CancellationToken cancellationToken = default)
        => Orient(source, mirrored ? ImageOrientation.Transverse : ImageOrientation.Transpose, limits, cancellationToken);

    public static ImageBuffer AutoOrient(ImageBuffer source, ImageLimits? limits = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        var result = Orient(source, source.Orientation, limits, cancellationToken);
        result.Orientation = ImageOrientation.Normal;
        return result;
    }

    private static ImageBuffer Orient(ImageBuffer source, ImageOrientation orientation, ImageLimits? limits,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        var w = source.Width;
        var h = source.Height;
        if ((int)orientation is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(orientation));
        if (orientation == ImageOrientation.MirrorHorizontal)
            return HorizontalFlip.Apply(source, limits, cancellationToken);
        return Map(source, (int)orientation >= 5 ? h : w, (int)orientation >= 5 ? w : h, (x, y) => orientation switch
        {
            ImageOrientation.Normal => (x, y),
            ImageOrientation.MirrorHorizontal => (w - 1 - x, y),
            ImageOrientation.Rotate180 => (w - 1 - x, h - 1 - y),
            ImageOrientation.MirrorVertical => (x, h - 1 - y),
            ImageOrientation.Transpose => (y, x),
            ImageOrientation.Rotate90 => (y, h - 1 - x),
            ImageOrientation.Transverse => (w - 1 - y, h - 1 - x),
            ImageOrientation.Rotate270 => (w - 1 - y, x),
            _ => throw new ArgumentOutOfRangeException(nameof(orientation))
        }, limits, cancellationToken);
    }

    private static ImageBuffer Map(ImageBuffer source, int width, int height, Func<int, int, (int X, int Y)> map,
        ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.Reserve(source.ByteLength);
        var result = ImageBuffer.Allocate(width, height, source.BitDepth, context);
        var bytesPerPixel = source.BitDepth / 8 * 4;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                // A caller may raise MaxDimension beyond the default. Keep work
                // admission and cancellation bounded within a long row as well.
                if ((x & 1023) == 0) context.Work(Math.Min(1024, width - x) * 4L);
                var (sx, sy) = map(x, y);
                source.Pixels.Slice(sy * source.Stride + sx * bytesPerPixel, bytesPerPixel)
                    .CopyTo(result.Pixels.Slice(y * result.Stride + x * bytesPerPixel, bytesPerPixel));
            }
        }
        result.Orientation = source.Orientation;
        return result;
    }

    public static ImageBuffer ResizeToFit(ImageBuffer source, int maxWidth, int maxHeight, bool allowUpscaling,
        ResizeFilter filter = ResizeFilter.Bicubic, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (maxWidth < 1 || maxHeight < 1) throw new ArgumentOutOfRangeException(nameof(maxWidth));
        var ratio = Math.Min((double)maxWidth / source.Width, (double)maxHeight / source.Height);
        if (!allowUpscaling) ratio = Math.Min(1, ratio);
        var width = Math.Max(1, (int)Math.Round(source.Width * ratio, MidpointRounding.AwayFromZero));
        var height = Math.Max(1, (int)Math.Round(source.Height * ratio, MidpointRounding.AwayFromZero));
        return Resize(source, width, height, filter, limits, cancellationToken);
    }

    public static ImageBuffer Resize(ImageBuffer source, int width, int height, ResizeFilter filter = ResizeFilter.Bicubic,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!Enum.IsDefined(filter)) throw new ArgumentOutOfRangeException(nameof(filter));
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.Reserve(source.ByteLength);
        context.PixelBytes(width, height, source.BitDepth);
        if (width == source.Width && height == source.Height) return source.Clone(limits, cancellationToken);
        if (filter == ResizeFilter.Nearest)
            return Map(source, width, height, (x, y) =>
                (ResizeSampling.Nearest(x, source.Width, width),
                 ResizeSampling.Nearest(y, source.Height, height)), limits, cancellationToken);

        var scaleX = Math.Max(1, (double)source.Width / width);
        var scaleY = Math.Max(1, (double)source.Height / height);
        var radius = filter == ResizeFilter.Bicubic ? 2 : 1;
        var tapsX = (long)Math.Ceiling(2 * radius * scaleX) + 1;
        var tapsY = (long)Math.Ceiling(2 * radius * scaleY) + 1;
        var units = (double)width * height * tapsX * tapsY * 4;
        if (units > context.Limits.MaxWork) throw new ImageLimitException("Resize work limit exceeded.");
        var result = ImageBuffer.Allocate(width, height, source.BitDepth, context);
        var maximum = source.BitDepth == 8 ? 255 : 65535;
        for (var y = 0; y < height; y++)
        {
            context.Work(width * tapsX * tapsY * 4);
            Span<byte> destination8 = default;
            Span<ushort> destination16 = default;
            if (source.BitDepth == 8) destination8 = result.GetRow8(y);
            else destination16 = result.GetRow16(y);
            var cy = (y + 0.5) * source.Height / height - 0.5;
            for (var x = 0; x < width; x++)
            {
                var cx = (x + 0.5) * source.Width / width - 0.5;
                var firstX = (int)Math.Ceiling(cx - radius * scaleX);
                var lastX = (int)Math.Floor(cx + radius * scaleX);
                // Reuse short horizontal support across vertical taps without
                // a heap workspace. Wider support retains direct evaluation.
                var cached = lastX - firstX < 8;
                var w0 = cached ? Kernel((firstX - cx) / scaleX, filter) : 0;
                var w1 = cached && lastX - firstX >= 1 ? Kernel((firstX + 1 - cx) / scaleX, filter) : 0;
                var w2 = cached && lastX - firstX >= 2 ? Kernel((firstX + 2 - cx) / scaleX, filter) : 0;
                var w3 = cached && lastX - firstX >= 3 ? Kernel((firstX + 3 - cx) / scaleX, filter) : 0;
                var w4 = cached && lastX - firstX >= 4 ? Kernel((firstX + 4 - cx) / scaleX, filter) : 0;
                var w5 = cached && lastX - firstX >= 5 ? Kernel((firstX + 5 - cx) / scaleX, filter) : 0;
                var w6 = cached && lastX - firstX >= 6 ? Kernel((firstX + 6 - cx) / scaleX, filter) : 0;
                var w7 = cached && lastX - firstX >= 7 ? Kernel((firstX + 7 - cx) / scaleX, filter) : 0;
                double r = 0, g = 0, b = 0, alpha = 0, weightSum = 0;
                for (var sy = (int)Math.Ceiling(cy - radius * scaleY); sy <= Math.Floor(cy + radius * scaleY); sy++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    var wy = Kernel((sy - cy) / scaleY, filter);
                    if (wy == 0) continue;
                    var iy = Math.Clamp(sy, 0, source.Height - 1);
                    ReadOnlySpan<byte> row8 = default;
                    ReadOnlySpan<ushort> row16 = default;
                    if (source.BitDepth == 8) row8 = source.GetRow8(iy);
                    else row16 = source.GetRow16(iy);
                    for (var sx = firstX; sx <= lastX; sx++)
                    {
                        var wx = cached ? (sx - firstX) switch
                        {
                            0 => w0, 1 => w1, 2 => w2, 3 => w3,
                            4 => w4, 5 => w5, 6 => w6, _ => w7
                        } : Kernel((sx - cx) / scaleX, filter);
                        var weight = wy * wx;
                        if (weight == 0) continue;
                        var ix = Math.Clamp(sx, 0, source.Width - 1);
                        var offset = ix * 4;
                        int a, red, green, blue;
                        if (source.BitDepth == 8)
                        {
                            a = row8[offset + 3];
                            red = row8[offset];
                            green = row8[offset + 1];
                            blue = row8[offset + 2];
                        }
                        else
                        {
                            a = row16[offset + 3];
                            red = row16[offset];
                            green = row16[offset + 1];
                            blue = row16[offset + 2];
                        }
                        r += weight * a * red;
                        g += weight * a * green;
                        b += weight * a * blue;
                        alpha += weight * a;
                        weightSum += weight;
                    }
                }
                var normalizedAlpha = alpha / weightSum;
                var roundedAlpha = normalizedAlpha > 0 && normalizedAlpha < 1
                    ? ResizeAlpha.Round(source, x, y, width, height, filter, context)
                    : Round(normalizedAlpha, maximum);
                // Zero-alpha filtered output has no visible color. This also
                // prevents tiny positive weight residue from exposing hidden RGB.
                var hasColor = roundedAlpha > 0 && alpha > 0;
                var destinationOffset = x * 4;
                if (source.BitDepth == 8)
                {
                    destination8[destinationOffset] = (byte)Round(hasColor ? r / alpha : 0, maximum);
                    destination8[destinationOffset + 1] = (byte)Round(hasColor ? g / alpha : 0, maximum);
                    destination8[destinationOffset + 2] = (byte)Round(hasColor ? b / alpha : 0, maximum);
                    destination8[destinationOffset + 3] = (byte)roundedAlpha;
                }
                else
                {
                    destination16[destinationOffset] = (ushort)Round(hasColor ? r / alpha : 0, maximum);
                    destination16[destinationOffset + 1] = (ushort)Round(hasColor ? g / alpha : 0, maximum);
                    destination16[destinationOffset + 2] = (ushort)Round(hasColor ? b / alpha : 0, maximum);
                    destination16[destinationOffset + 3] = (ushort)roundedAlpha;
                }
            }
        }
        result.Orientation = source.Orientation;
        return result;
    }

    private static int Round(double value, int maximum)
        => (int)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, maximum);

    private static double Kernel(double distance, ResizeFilter filter)
    {
        var x = Math.Abs(distance);
        if (filter == ResizeFilter.Bilinear) return Math.Max(0, 1 - x);
        // Catmull-Rom: Mitchell-Netravali B=0, C=1/2. These coefficients are derived
        // from the piecewise cubic interpolation conditions, not an upstream table.
        return x < 1 ? 1.5 * x * x * x - 2.5 * x * x + 1
            : x < 2 ? -0.5 * x * x * x + 2.5 * x * x - 4 * x + 2 : 0;
    }
}
