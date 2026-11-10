using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Caller-owned text geometry and workspace for a fixed canvas shape, depth and text origin.</summary>
/// <remarks>The layout and font are borrowed during creation only and may then be disposed.
/// Every draw reads and updates the supplied canvas, compositing glyphs in layout order
/// with the same exact coverage and color arithmetic as ImageDrawing.DrawText.
/// Creation limits and their borrowed memory root govern the retained capacity and drawing.
/// Concurrent or reentrant drawing, and disposal during drawing, throw InvalidOperationException.
/// Calls after disposal throw ObjectDisposedException. Synchronize canvas access separately.</remarks>
public sealed class TextRenderPlan : IDisposable
{
    private readonly OperationContext owner;
    private readonly double x, y;
    private readonly int left, top, right, bottom;
    private TextRasterizer? raster;
    private long storageBytes;
    private int state; // 0 idle, 1 drawing, 2 disposing, 3 disposed.

    public int Width { get; }
    public int Height { get; }
    public int BitDepth { get; }
    /// <summary>Coarse owned geometry/workspace capacity charged until disposal, excluding borrowed inputs and canvases.</summary>
    public long StorageBytes => Volatile.Read(ref storageBytes);

    private TextRenderPlan(TextLayout layout, int width, int height, int bitDepth,
        double x, double y, OperationContext owner)
    {
        Width = width; Height = height; BitDepth = bitDepth;
        this.x = x; this.y = y; this.owner = owner;
        (left, top, right, bottom) = TextRasterizer.Clip(layout.InkBounds, x, y, width, height);
    }

    /// <summary>Copy and prepare admitted geometry. The token governs preparation only.</summary>
    /// <remarks>Local working limits include the borrowed layout and a matching canvas during preparation.
    /// No canvas is retained. Preparation completes before any canvas can be mutated.</remarks>
    public static TextRenderPlan Create(TextLayout layout, int width, int height, int bitDepth,
        double x, double y, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(layout); layout.EnsureAlive();
        if (!double.IsFinite(x) || !double.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(x));
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        TextRenderPlan? plan = null;
        try
        {
            var canvasBytes = context.PixelBytes(width, height, bitDepth);
            context.ReserveBorrowed(canvasBytes + layout.StorageBytes);
            plan = new(layout, width, height, bitDepth, x, y, context);
            if (plan.left < plan.right && plan.top < plan.bottom)
            {
                plan.raster = new(layout, context, plan.right - plan.left);
                plan.storageBytes = plan.raster.StorageBytes;
            }
            context.ReleaseBorrowed(canvasBytes + layout.StorageBytes);
            return plan;
        }
        catch
        {
            if (plan is null) context.Dispose(); else plan.Dispose();
            throw;
        }
    }

    /// <summary>Composite onto a borrowed matching canvas. Cancellation may leave completed chunks.</summary>
    public void Draw(ImageBuffer canvas, RgbaColor color, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        var previous = Interlocked.CompareExchange(ref state, 1, 0);
        if (previous >= 2) throw new ObjectDisposedException(nameof(TextRenderPlan));
        if (previous != 0) throw new InvalidOperationException("The text render plan is already drawing.");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((canvas.Width, canvas.Height, canvas.BitDepth) != (Width, Height, BitDepth))
                throw new ArgumentException("The canvas shape and pixel depth must match the text render plan.", nameof(canvas));
            using var context = new OperationContext(owner.Limits, cancellationToken);
            context.ReserveBorrowed(canvas.ByteLength); context.ReserveBorrowed(storageBytes);
            if (raster is not null && color.A != 0)
                raster.Draw(canvas, x, y, color, left, top, right, bottom, cancellationToken);
        }
        finally { Volatile.Write(ref state, 0); }
    }

    /// <summary>Release retained capacity. Borrowed canvases and the memory root remain usable.</summary>
    public void Dispose()
    {
        var previous = Interlocked.CompareExchange(ref state, 2, 0);
        if (previous == 3) return;
        if (previous != 0) throw new InvalidOperationException("The text render plan is drawing or being disposed.");
        try { raster?.Dispose(); raster = null; }
        finally { owner.Dispose(); Volatile.Write(ref storageBytes, 0); Volatile.Write(ref state, 3); }
    }
}
