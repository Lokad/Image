using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Caller-owned exact text coverage for a fixed canvas shape, depth and text origin.</summary>
/// <remarks>The layout and font are borrowed during creation only and may then be disposed.
/// Every draw reads and updates the supplied canvas, compositing glyphs in layout order
/// with the same exact coverage and color arithmetic as ImageDrawing.DrawText.
/// Creation limits and their borrowed memory root govern the retained capacity and drawing.
/// Concurrent or reentrant drawing, and disposal during drawing, throw InvalidOperationException.
/// Calls after disposal throw ObjectDisposedException. Synchronize canvas access separately.</remarks>
public sealed class TextRenderPlan : IDisposable
{
    private readonly OperationContext owner;
    private readonly int left, top, right, bottom;
    private CoverageRun[] runs = [];
    private ushort[] coverage = [];
    private int runCount, coverageCount;
    private long runBytes, coverageBytes;
    private long storageBytes;
    private int state; // 0 idle, 1 drawing, 2 disposing, 3 disposed.

    private readonly record struct CoverageRun(int X, int Y, int First, int Count);

    public int Width { get; }
    public int Height { get; }
    public int BitDepth { get; }
    /// <summary>Coarse owned coverage capacity charged until disposal, excluding borrowed inputs and canvases.</summary>
    public long StorageBytes => Volatile.Read(ref storageBytes);

    private TextRenderPlan(TextLayout layout, int width, int height, int bitDepth,
        double x, double y, OperationContext owner)
    {
        Width = width; Height = height; BitDepth = bitDepth;
        this.owner = owner;
        (left, top, right, bottom) = TextRasterizer.Clip(layout.InkBounds, x, y, width, height);
    }

    /// <summary>Prepare admitted exact glyph coverage. The token governs preparation only.</summary>
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
                using var raster = new TextRasterizer(layout, context, plan.right - plan.left);
                raster.PrepareCoverage(plan, x, y, plan.left, plan.top, plan.right, plan.bottom, cancellationToken);
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

    internal void CaptureCoverageRow(ReadOnlySpan<double> row, int left, int y, CancellationToken cancellation)
    {
        var first = 0;
        while (first < row.Length)
        {
            if ((first & 127) == 0) cancellation.ThrowIfCancellationRequested();
            if (RoundedCoverage(row[first]) != 0) break;
            first++;
        }
        if (first == row.Length) return;
        var last = row.Length - 1;
        while (last > first)
        {
            if ((last & 127) == 0) cancellation.ThrowIfCancellationRequested();
            if (RoundedCoverage(row[last]) != 0) break;
            last--;
        }
        var count = last - first + 1;
        Grow(ref runs, runCount, (long)runCount + 1, 16, ref runBytes, cancellation);
        Grow(ref coverage, coverageCount, (long)coverageCount + count, 2, ref coverageBytes, cancellation);
        for (var i = 0; i < count; i++)
        {
            if ((i & 127) == 0) cancellation.ThrowIfCancellationRequested();
            coverage[coverageCount + i] = RoundedCoverage(row[first + i]);
        }
        runs[runCount++] = new(left + first, y, coverageCount, count);
        coverageCount += count;
    }

    // This is the immediate rasterizer's original final rounding. A stored word
    // preserves every possible coverage sample, independently of drawing color.
    private static ushort RoundedCoverage(double value)
        => (ushort)(uint)Math.Clamp(Math.Floor(value * 65535 + .5), 0, 65535);

    private void Grow<T>(ref T[] array, int used, long needed, int itemBytes, ref long admitted,
        CancellationToken cancellation)
        where T : unmanaged
    {
        if (needed <= array.Length) return;
        var maximum = (Array.MaxLength - 32) / itemBytes;
        if (needed > maximum)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "Text coverage exceeds the managed buffer limit.");
        var capacity = (int)Math.Min(maximum, Math.Max(needed, Math.Max(8L, array.Length * 2L)));
        var nextBytes = 32 + capacity * (long)itemBytes;
        owner.Reserve(nextBytes);
        T[] next;
        try
        {
            cancellation.ThrowIfCancellationRequested();
            next = new T[capacity];
            for (var offset = 0; offset < used;)
            {
                var count = Math.Min(4096, used - offset); cancellation.ThrowIfCancellationRequested();
                array.AsSpan(offset, count).CopyTo(next.AsSpan(offset)); offset += count;
            }
        }
        catch { owner.Release(nextBytes); throw; }
        owner.Release(admitted);
        storageBytes += nextBytes - admitted; admitted = nextBytes; array = next;
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
            if (color.A == 0) return;
            // Runs retain glyph order as well as row order. Overlapping glyphs
            // must compose separately, with the same rounding on fresh pixels.
            for (var runIndex = 0; runIndex < runCount; runIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested(); var run = runs[runIndex];
                for (var i = 0; i < run.Count; i++)
                {
                    if ((i & 127) == 0) cancellationToken.ThrowIfCancellationRequested();
                    var value = coverage[run.First + i];
                    if (value != 0) TextRasterizer.Blend(canvas, run.X + i, run.Y, color, value);
                }
            }
        }
        finally { Volatile.Write(ref state, 0); }
    }

    /// <summary>Release retained capacity. Borrowed canvases and the memory root remain usable.</summary>
    public void Dispose()
    {
        var previous = Interlocked.CompareExchange(ref state, 2, 0);
        if (previous == 3) return;
        if (previous != 0) throw new InvalidOperationException("The text render plan is drawing or being disposed.");
        try { runs = []; coverage = []; runCount = coverageCount = 0; }
        finally { owner.Dispose(); Volatile.Write(ref storageBytes, 0); Volatile.Write(ref state, 3); }
    }
}
