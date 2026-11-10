using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Caller-owned bicubic preparation for a fixed source shape, destination shape and pixel depth.</summary>
/// <remarks>Eligible two-axis reductions retain cached geometry and working rows.
/// Other geometries and portable paths use the ordinary exact resize fallback.
/// Every execution reads the supplied image and returns an independently owned result.
/// The creation limits and borrowed memory root apply to preparation and execution.
/// Concurrent or reentrant execution, and disposal during execution, throw
/// InvalidOperationException. Calls after disposal throw ObjectDisposedException.</remarks>
public sealed class BicubicResizePlan : IDisposable
{
    private readonly OperationContext owner;
    private readonly ImageLimits limits;
    private readonly double scaleY;
    private readonly long tapsX, tapsY;
    private CachedBicubicResize.Workspace workspace;
    private long storageBytes;
    private int state; // 0 idle, 1 executing, 2 disposing, 3 disposed.

    public int SourceWidth { get; }
    public int SourceHeight { get; }
    public int Width { get; }
    public int Height { get; }
    public int BitDepth { get; }
    /// <summary>Coarse owned preparation/workspace capacity charged until disposal, excluding inputs and results.</summary>
    public long StorageBytes => Volatile.Read(ref storageBytes);

    private BicubicResizePlan(int sourceWidth, int sourceHeight, int width, int height, int bitDepth,
        OperationContext owner)
    {
        SourceWidth = sourceWidth; SourceHeight = sourceHeight;
        Width = width; Height = height; BitDepth = bitDepth;
        this.owner = owner; limits = owner.Limits;
        var scaleX = Math.Max(1, (double)sourceWidth / width);
        scaleY = Math.Max(1, (double)sourceHeight / height);
        tapsX = (long)Math.Ceiling(4 * scaleX) + 1;
        tapsY = (long)Math.Ceiling(4 * scaleY) + 1;
    }

    /// <summary>Validate the shape and prepare admitted storage. The token governs preparation only.</summary>
    public static BicubicResizePlan Create(int sourceWidth, int sourceHeight, int width, int height, int bitDepth,
        ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        var context = new OperationContext(limits ?? ImageLimits.Default, cancellationToken);
        BicubicResizePlan? plan = null;
        try
        {
            var inputBytes = context.PixelBytes(sourceWidth, sourceHeight, bitDepth);
            var outputBytes = context.PixelBytes(width, height, bitDepth);
            context.ReserveBorrowed(inputBytes + (long)outputBytes);
            plan = new(sourceWidth, sourceHeight, width, height, bitDepth, context);
            if (width < sourceWidth && height < sourceHeight
                && CachedBicubicResize.TryPrepare(sourceWidth, sourceHeight, bitDepth, width, height,
                    Math.Max(1, (double)sourceWidth / width), plan.tapsX, plan.tapsY, context, out plan.workspace))
                plan.storageBytes = plan.workspace.Bytes;
            context.ReleaseBorrowed(inputBytes + (long)outputBytes);
            return plan;
        }
        catch
        {
            if (plan is null) context.Dispose(); else plan.Dispose();
            throw;
        }
    }

    /// <summary>Resize a borrowed matching source, reading fresh samples and independently owning the result.</summary>
    public ImageBuffer Resize(ImageBuffer source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        var previous = Interlocked.CompareExchange(ref state, 1, 0);
        if (previous >= 2) throw new ObjectDisposedException(nameof(BicubicResizePlan));
        if (previous != 0) throw new InvalidOperationException("The resize plan is already executing.");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((source.Width, source.Height, source.BitDepth) != (SourceWidth, SourceHeight, BitDepth))
                throw new ArgumentException("The source shape and pixel depth must match the resize plan.", nameof(source));
            if (workspace.Bytes == 0)
                return ImageTransforms.Resize(source, Width, Height, ResizeFilter.Bicubic, limits, cancellationToken);
            using var context = new OperationContext(limits, cancellationToken);
            context.ReserveBorrowed(source.ByteLength);
            context.ReserveBorrowed(storageBytes);
            var result = ImageBuffer.AllocateForOverwrite(Width, Height, BitDepth, context);
            try
            {
                CachedBicubicResize.Execute(source, result, scaleY, tapsX, tapsY, workspace, context);
                result.Orientation = source.Orientation;
                return result;
            }
            catch { result.Dispose(); throw; }
        }
        finally { Volatile.Write(ref state, 0); }
    }

    /// <summary>Release retained capacity. Independently owned results remain usable and admitted.</summary>
    public void Dispose()
    {
        var previous = Interlocked.CompareExchange(ref state, 2, 0);
        if (previous == 3) return;
        if (previous != 0) throw new InvalidOperationException("The resize plan is executing or being disposed.");
        try { workspace = default; }
        finally { owner.Dispose(); Volatile.Write(ref storageBytes, 0); Volatile.Write(ref state, 3); }
    }
}
