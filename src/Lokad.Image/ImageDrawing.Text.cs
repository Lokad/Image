using Lokad.Image.Internal;

namespace Lokad.Image;

public static partial class ImageDrawing
{
    /// <summary>Render a prepared layout in the supplied color at a top-left text origin, in coded canvas coordinates.</summary>
    /// <remarks>Unhinted nonzero-winding coverage; glyphs composite in layout order.
    /// Color/alpha retain sixteen-bit precision until final canvas samples. Destinations
    /// clip to the canvas. No full-frame temporary, implicit orientation or font lookup.
    /// Workspace admission precedes mutation; cancellation can leave completed chunks.</remarks>
    public static void DrawText(ImageBuffer canvas, TextLayout layout, double x, double y, RgbaColor color,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(canvas); ArgumentNullException.ThrowIfNull(layout);
        layout.EnsureAlive();
        if (!double.IsFinite(x) || !double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(x));
        using var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        context.ReserveBorrowed(canvas.ByteLength); context.ReserveBorrowed(layout.StorageBytes);
        var (left, top, right, bottom) = TextRasterizer.Clip(layout.InkBounds, x, y, canvas.Width, canvas.Height);
        if (left >= right || top >= bottom || color.A == 0) return;
        context.PixelBytes(right - left, bottom - top, canvas.BitDepth);
        using var raster = new TextRasterizer(layout, context, right - left);
        raster.Draw(canvas, x, y, color, left, top, right, bottom, context.Cancellation);
    }
}
