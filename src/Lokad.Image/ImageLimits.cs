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
    /// <summary>Optional borrowed admission root, shared with concurrent and nested work.</summary>
    /// <remarks>Does not replace local memory and container limits. Retained results
    /// own admission until disposal; keep the supplied owner alive through all
    /// operations and results. Caller-owned inputs are not charged again.</remarks>
    public IMemoryBudget? MemoryBudget { get; init; }

    internal void Validate()
    {
        if (MaxDimension < 1 || MaxPixels < 1 || MaxInputBytes < 1 || MaxOutputBytes < 1
            || MaxWorkingBytes < 1 || MaxMetadataBytes < 1 || MaxContainerItems < 1)
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
    /// <summary>The exhausted limit. The message-only constructor uses Unspecified.</summary>
    public ImageLimitKind Limit { get; }
    /// <summary>Whether the exhausted allowance belongs to one operation or a shared budget.</summary>
    public ImageLimitScope Scope { get; }

    public ImageLimitException(string message) : this(ImageLimitKind.Unspecified, message) { }
    public ImageLimitException(ImageLimitKind limit, string message) : this(limit, message, ImageLimitScope.Operation) { }
    public ImageLimitException(ImageLimitKind limit, string message, ImageLimitScope scope)
        : base(message) { Limit = limit; Scope = scope; }
}

public sealed class UnsupportedImageException : NotSupportedException
{
    /// <summary>The unsupported feature, without requiring message parsing.</summary>
    public UnsupportedImageFeature Feature { get; }
    /// <summary>The recognized format, or null when the format is unrecognized or unspecified.</summary>
    public ImageFormat? Format { get; }

    public UnsupportedImageException(string message) : this(UnsupportedImageFeature.Unspecified, message) { }
    public UnsupportedImageException(UnsupportedImageFeature feature, string message, ImageFormat? format = null)
        : base(message) { Feature = feature; Format = format; }
}
