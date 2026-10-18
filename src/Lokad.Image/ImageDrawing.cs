using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Straight RGBA color with unsigned 16-bit channels; eight-bit canvases round each channel to 0..255.</summary>
public readonly record struct RgbaColor(ushort R, ushort G, ushort B, ushort A = ushort.MaxValue)
{
    /// <summary>Expand eight-bit samples exactly, multiplying by 257.</summary>
    public static RgbaColor From8(byte red, byte green, byte blue, byte alpha = 255)
        => new((ushort)(red * 257), (ushort)(green * 257), (ushort)(blue * 257), (ushort)(alpha * 257));
}

/// <summary>Mutate a caller-owned canvas in coded coordinates; destination rectangles are clipped.</summary>
/// <remarks>No implicit orientation or color management. Callers synchronize pixel access.
/// Limits/cancellation can leave completed chunks; validation and memory admission precede mutation.
/// Existing buffers stay owned by callers and no full-frame temporary is allocated.</remarks>
public static partial class ImageDrawing
{
    /// <summary>Replace a rectangle with a straight RGBA color, including its alpha and hidden RGB.</summary>
    public static void FillRectangle(ImageBuffer canvas, int x, int y, int width, int height, RgbaColor color,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(canvas); Extent(width, height);
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.Reserve(canvas.ByteLength);
        var region = Clip(canvas, 0, 0, x, y, width, height);
        if (region.Width == 0 || region.Height == 0) return;
        context.PixelBytes(region.Width, region.Height, canvas.BitDepth);
        var r = canvas.BitDepth == 8 ? (color.R + 128) / 257 : color.R;
        var g = canvas.BitDepth == 8 ? (color.G + 128) / 257 : color.G;
        var b = canvas.BitDepth == 8 ? (color.B + 128) / 257 : color.B;
        var a = canvas.BitDepth == 8 ? (color.A + 128) / 257 : color.A;
        for (var row = 0; row < region.Height; row++)
        for (var first = 0; first < region.Width;)
        {
            var count = Math.Min(4096, region.Width - first); context.Work(count * 4L);
            var yRow = region.DestinationY + row;
            if (canvas.BitDepth == 8)
            {
                var samples = canvas.GetRow8(yRow).Slice((region.DestinationX + first) * 4, count * 4);
                for (var i = 0; i < samples.Length; i += 4)
                { samples[i] = (byte)r; samples[i + 1] = (byte)g; samples[i + 2] = (byte)b; samples[i + 3] = (byte)a; }
            }
            else
            {
                var samples = canvas.GetRow16(yRow).Slice((region.DestinationX + first) * 4, count * 4);
                for (var i = 0; i < samples.Length; i += 4)
                { samples[i] = (ushort)r; samples[i + 1] = (ushort)g; samples[i + 2] = (ushort)b; samples[i + 3] = (ushort)a; }
            }
            first += count;
        }
    }

    /// <summary>Copy a source rectangle's exact samples to the canvas. Depths must match; self-overlap has snapshot semantics.</summary>
    public static void CopyRegion(ImageBuffer source, int sourceX, int sourceY, int width, int height,
        ImageBuffer canvas, int x, int y, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(canvas); Extent(width, height);
        if (sourceX < 0 || sourceY < 0 || (long)sourceX + width > source.Width || (long)sourceY + height > source.Height)
            throw new ArgumentOutOfRangeException(nameof(sourceX), "Source rectangle must fit the source.");
        if (source.BitDepth != canvas.BitDepth) throw new ArgumentException("Exact copies require matching pixel depths.", nameof(canvas));
        var context = Admit(canvas, source, limits, cancellationToken);
        var region = Clip(canvas, sourceX, sourceY, x, y, width, height);
        if (region.Width == 0 || region.Height == 0) return;
        context.PixelBytes(region.Width, region.Height, canvas.BitDepth);
        var backwardsY = ReferenceEquals(source, canvas) && region.DestinationY > region.SourceY;
        var backwardsX = ReferenceEquals(source, canvas) && region.DestinationX > region.SourceX;
        for (var row = 0; row < region.Height; row++)
        {
            var offsetY = backwardsY ? region.Height - row - 1 : row;
            for (var first = 0; first < region.Width;)
            {
                var count = Math.Min(4096, region.Width - first); context.Work(count * 4L);
                var offsetX = backwardsX ? region.Width - first - count : first;
                var sx = (region.SourceX + offsetX) * 4; var dx = (region.DestinationX + offsetX) * 4;
                var sy = region.SourceY + offsetY; var dy = region.DestinationY + offsetY;
                if (canvas.BitDepth == 8) source.GetRow8(sy).Slice(sx, count * 4).CopyTo(canvas.GetRow8(dy).Slice(dx, count * 4));
                else source.GetRow16(sy).Slice(sx, count * 4).CopyTo(canvas.GetRow16(dy).Slice(dx, count * 4));
                first += count;
            }
        }
    }

    /// <summary>Composite an image onto the canvas with straight-alpha source-over, supporting either source/canvas depth.</summary>
    /// <remarks>Unrounded coverage determines color; round positive halves upward only at final samples.
    /// Zero source alpha leaves every destination sample unchanged. Self-overlap has snapshot semantics.</remarks>
    public static void Composite(ImageBuffer canvas, ImageBuffer source, int x, int y,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(canvas);
        var context = Admit(canvas, source, limits, cancellationToken);
        var region = Clip(canvas, 0, 0, x, y, source.Width, source.Height);
        if (region.Width == 0 || region.Height == 0) return;
        context.PixelBytes(region.Width, region.Height, canvas.BitDepth);
        var backwardsY = ReferenceEquals(source, canvas) && region.DestinationY > region.SourceY;
        var backwardsX = ReferenceEquals(source, canvas) && region.DestinationX > region.SourceX;
        ulong sMax = source.BitDepth == 8 ? 255u : 65535u, dMax = canvas.BitDepth == 8 ? 255u : 65535u;
        for (var row = 0; row < region.Height; row++)
        {
            var offsetY = backwardsY ? region.Height - row - 1 : row;
            var sy = region.SourceY + offsetY; var dy = region.DestinationY + offsetY;
            for (var first = 0; first < region.Width;)
            {
                var count = Math.Min(256, region.Width - first); context.Work(count * 16L);
                for (var i = first; i < first + count; i++)
                {
                    var offsetX = backwardsX ? region.Width - i - 1 : i;
                    var sx = region.SourceX + offsetX; var dx = region.DestinationX + offsetX;
                    var sa = (ulong)source.Sample(sx, sy, 3); if (sa == 0) continue;
                    var da = (ulong)canvas.Sample(dx, dy, 3);
                    var coverage = sa * dMax + da * (sMax - sa);
                    var denominator = sMax * coverage;
                    for (var channel = 0; channel < 3; channel++)
                    {
                        // W3C Compositing 1, 9.1.4: exact rational source-over.
                        // Max numerator + rounding < 65535^4 + 65535^3/2 < ulong.MaxValue.
                        var numerator = (ulong)source.Sample(sx, sy, channel) * sa * dMax * dMax
                            + (ulong)canvas.Sample(dx, dy, channel) * da * (sMax - sa) * sMax;
                        canvas.SetSample(dx, dy, channel, (int)((numerator + denominator / 2) / denominator));
                    }
                    canvas.SetSample(dx, dy, 3, (int)((coverage + sMax / 2) / sMax));
                }
                first += count;
            }
        }
    }

    private static OperationContext Admit(ImageBuffer canvas, ImageBuffer source, ImageLimits? limits, CancellationToken cancellationToken)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.Reserve(canvas.ByteLength);
        if (!ReferenceEquals(canvas, source)) context.Reserve(source.ByteLength);
        return context;
    }

    private static void Extent(int width, int height)
    {
        if (width < 0 || height < 0) throw new ArgumentOutOfRangeException(nameof(width), "Rectangle extents cannot be negative.");
    }

    private readonly record struct Region(int SourceX, int SourceY, int DestinationX, int DestinationY, int Width, int Height);
    private static Region Clip(ImageBuffer canvas, int sx, int sy, int dx, int dy, int width, int height)
    {
        var left = Math.Max(0L, -(long)dx); var top = Math.Max(0L, -(long)dy);
        var right = Math.Min((long)width, (long)canvas.Width - dx); var bottom = Math.Min((long)height, (long)canvas.Height - dy);
        if (left >= right || top >= bottom) return default;
        return new((int)(sx + left), (int)(sy + top), (int)(dx + left), (int)(dy + top), (int)(right - left), (int)(bottom - top));
    }
}
