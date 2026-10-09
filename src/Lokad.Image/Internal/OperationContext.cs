namespace Lokad.Image.Internal;

internal sealed class OperationContext
{
    public ImageLimits Limits { get; }
    public CancellationToken Cancellation { get; }
    private long liveBytes;
    private long work;

    public OperationContext(ImageLimits limits, CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(limits);
        limits.Validate();
        cancellation.ThrowIfCancellationRequested();
        Limits = limits;
        Cancellation = cancellation;
    }

    public void Reserve(long bytes)
    {
        if (bytes < 0 || bytes > Limits.MaxWorkingBytes - liveBytes)
            throw new ImageLimitException("Working memory limit exceeded.");
        liveBytes += bytes;
    }

    public void Release(long bytes) => liveBytes -= bytes;

    public void Work(long units)
    {
        Cancellation.ThrowIfCancellationRequested();
        if (units < 0 || units > Limits.MaxWork - work)
            throw new ImageLimitException("Computational work limit exceeded.");
        work += units;
    }

    public int PixelBytes(int width, int height, int bitDepth)
    {
        if (width < 1 || height < 1)
            throw new ArgumentOutOfRangeException(nameof(width), "Dimensions must be positive.");
        if (bitDepth is not (8 or 16))
            throw new ArgumentOutOfRangeException(nameof(bitDepth), "Pixel depth must be 8 or 16.");
        var pixels = (long)width * height;
        if (width > Limits.MaxDimension || height > Limits.MaxDimension || pixels > Limits.MaxPixels
            || pixels > Array.MaxLength / ((bitDepth / 8) * 4))
            throw new ImageLimitException("Image dimensions or pixel count exceed the configured limits.");
        return (int)(pixels * (bitDepth / 8) * 4);
    }
}
