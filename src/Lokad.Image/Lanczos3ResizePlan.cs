using Lokad.Image.Internal;

namespace Lokad.Image;

/// <summary>Caller-owned Lanczos3 geometry and reusable working rows for one source shape, destination shape and pixel depth.</summary>
/// <remarks>Results have independent ownership. The creation limits and borrowed
/// memory root apply to every execution. No input image is retained. Concurrent
/// or reentrant executions, and disposal during execution, are rejected with
/// InvalidOperationException. Calls after disposal starts throw ObjectDisposedException.
/// Dispose the plan after its active execution has finished.</remarks>
public sealed class Lanczos3ResizePlan : IDisposable
{
    private readonly OperationContext owner;
    private readonly ImageLimits limits;
    private LanczosPlan? horizontal, vertical;
    private LanczosWorkspace? workspace;
    private long storageBytes;
    private int state; // 0 idle, 1 executing, 2 disposing, 3 disposed.

    public int SourceWidth { get; }
    public int SourceHeight { get; }
    public int Width { get; }
    public int Height { get; }
    public int BitDepth { get; }
    /// <summary>Coarse owned geometry/workspace capacity, charged until disposal. Does not include inputs or results.</summary>
    public long StorageBytes => Volatile.Read(ref storageBytes);

    private Lanczos3ResizePlan(int sourceWidth, int sourceHeight, int width, int height,
        int bitDepth, ImageLimits limits, CancellationToken cancellationToken)
    {
        SourceWidth = sourceWidth; SourceHeight = sourceHeight;
        Width = width; Height = height; BitDepth = bitDepth; this.limits = limits;
        owner = new OperationContext(limits, cancellationToken);
    }

    /// <summary>Prepare admitted axes and working rows. The creation token governs preparation only.</summary>
    public static Lanczos3ResizePlan Create(int sourceWidth, int sourceHeight, int width, int height,
        int bitDepth, ImageLimits? limits = null, CancellationToken cancellationToken = default)
    {
        var plan = new Lanczos3ResizePlan(sourceWidth, sourceHeight, width, height,
            bitDepth, limits ?? ImageLimits.Default, cancellationToken);
        try
        {
            var context = plan.owner;
            var inputBytes = context.PixelBytes(sourceWidth, sourceHeight, bitDepth);
            var outputBytes = context.PixelBytes(width, height, bitDepth);
            context.ReserveBorrowed(inputBytes);
            if (sourceWidth != width || sourceHeight != height)
            {
                plan.horizontal = LanczosPlan.Create(sourceWidth, width, context);
                plan.vertical = LanczosPlan.Create(sourceHeight, height, context);
                var fast = LanczosFmaResize.CanUse(bitDepth, width, height, plan.horizontal, plan.vertical, context);
                context.ReserveBorrowed(outputBytes);
                plan.workspace = new LanczosWorkspace(plan.horizontal, plan.vertical, bitDepth, fast, context);
                plan.storageBytes = plan.horizontal.StorageBytes + plan.vertical.StorageBytes + plan.workspace.Bytes;
            }
            else context.ReserveBorrowed(outputBytes);
            context.ReleaseBorrowed(inputBytes + (long)outputBytes);
            return plan;
        }
        catch { plan.Dispose(); throw; }
    }

    /// <summary>Resize a borrowed matching source into an independently owned image, using the creation limits/root.</summary>
    public ImageBuffer Resize(ImageBuffer source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        var previous = Interlocked.CompareExchange(ref state, 1, 0);
        if (previous >= 2) throw new ObjectDisposedException(nameof(Lanczos3ResizePlan));
        if (previous != 0) throw new InvalidOperationException("The resize plan is already executing.");
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((source.Width, source.Height, source.BitDepth) != (SourceWidth, SourceHeight, BitDepth))
                throw new ArgumentException("The source shape and pixel depth must match the resize plan.", nameof(source));
            if (workspace is null) return source.Clone(limits, cancellationToken);
            using var context = new OperationContext(limits, cancellationToken);
            context.ReserveBorrowed(source.ByteLength);
            context.ReserveBorrowed(storageBytes);
            return LanczosResize.Execute(source, Width, Height, horizontal!, vertical!, workspace, context);
        }
        finally { Volatile.Write(ref state, 0); }
    }

    /// <summary>Release retained geometry and workspace. Surviving results remain valid and admitted.</summary>
    public void Dispose()
    {
        var previous = Interlocked.CompareExchange(ref state, 2, 0);
        if (previous == 3) return;
        if (previous != 0) throw new InvalidOperationException("The resize plan is executing or being disposed.");
        try
        {
            workspace?.Dispose(); workspace = null;
            vertical?.Dispose(); vertical = null; horizontal?.Dispose(); horizontal = null;
        }
        finally { owner.Dispose(); Volatile.Write(ref storageBytes, 0); Volatile.Write(ref state, 3); }
    }
}
