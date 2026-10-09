namespace Lokad.Image;

/// <summary>Finite limits for one operation, including supplied and returned pixel buffers.</summary>
public sealed record ImageLimits
{
    public static ImageLimits Default { get; } = new();
    public int MaxDimension { get; init; } = 32768;
    public long MaxPixels { get; init; } = 32_000_000;
    public int MaxInputBytes { get; init; } = 64 * 1024 * 1024;
    public int MaxOutputBytes { get; init; } = 64 * 1024 * 1024;
    public long MaxWorkingBytes { get; init; } = 512L * 1024 * 1024;
    public int MaxMetadataBytes { get; init; } = 4 * 1024 * 1024;
    public int MaxContainerItems { get; init; } = 100_000;
    public long MaxWork { get; init; } = 2_000_000_000;

    internal void Validate()
    {
        if (MaxDimension < 1 || MaxPixels < 1 || MaxInputBytes < 1 || MaxOutputBytes < 1
            || MaxWorkingBytes < 1 || MaxMetadataBytes < 1 || MaxContainerItems < 1 || MaxWork < 1)
            throw new ArgumentOutOfRangeException(nameof(ImageLimits), "All limits must be positive.");
    }
}

public sealed class ImageDataException : IOException
{
    public ImageDataException(string message) : base(message) { }
    public ImageDataException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class ImageLimitException : IOException
{
    public ImageLimitException(string message) : base(message) { }
}

public sealed class UnsupportedImageException : NotSupportedException
{
    public UnsupportedImageException(string message) : base(message) { }
}
