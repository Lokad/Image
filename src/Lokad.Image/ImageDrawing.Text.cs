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
        var bounds = layout.InkBounds.Translate(x, y);
        if (!double.IsFinite(bounds.Right) || !double.IsFinite(bounds.Bottom)) throw new ArgumentOutOfRangeException(nameof(x));
        var left = Math.Max(0, Math.Floor(bounds.X)); var top = Math.Max(0, Math.Floor(bounds.Y));
        var right = Math.Min(canvas.Width, Math.Ceiling(bounds.Right)); var bottom = Math.Min(canvas.Height, Math.Ceiling(bounds.Bottom));
        if (left >= right || top >= bottom || color.A == 0) return;
        context.PixelBytes((int)(right - left), (int)(bottom - top), canvas.BitDepth);
        using var raster = new TextRasterizer(layout, context, (int)(right - left));
        raster.Draw(canvas, layout, x, y, color, (int)left, (int)top, (int)right, (int)bottom);
    }
}
