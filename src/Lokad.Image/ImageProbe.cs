namespace Lokad.Image;

/// <summary>Facts to establish without validating the complete image.</summary>
[Flags]
public enum ImageProbeFacts
{
    None = 0,
    Dimensions = 1,
    /// <summary>Source color, sample precision and coded component count. Alpha is a separate fact.</summary>
    SourceRepresentation = 2,
    Transparency = 4,
    Orientation = 8,
    Animation = 16,
    All = Dimensions | SourceRepresentation | Transparency | Orientation | Animation
}

/// <summary>Bounded prefix acquisition. Complete container validation is a separate operation.</summary>
public sealed record ImageProbeOptions
{
    public ImageLimits Limits { get; init; } = ImageLimits.Default;
    public ImageProbeFacts Facts { get; init; } = ImageProbeFacts.Dimensions;
    public ImageMetadataPolicy MetadataPolicy { get; init; }
    /// <summary>Maximum bytes actually read, excluding seeks; refusal does not read past this bound.</summary>
    public long MaxAcquiredBytes { get; init; } = 1024 * 1024;
}

/// <summary>Established header facts. Null fields are unexamined, not evidence of absence.</summary>
/// <remarks>Orientation Normal includes established absence of EXIF orientation. HasAlpha
/// describes source header semantics, not observed transparent pixels. Values apply to the
/// examined prefix; later corruption or conflicting metadata may invalidate the image.</remarks>
public sealed record ImageProbeResult
{
    public ImageFormat Format { get; init; }
    public ImageProbeFacts EstablishedFacts { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public int? BitDepth { get; init; }
    public SourceColorModel? SourceColor { get; init; }
    public byte? SourceComponentCount { get; init; }
    public SourceAlphaRepresentation? SourceAlpha { get; init; }
    public bool? HasAlpha { get; init; }
    public ImageOrientation? Orientation { get; init; }
    public bool? IsAnimated { get; init; }
    /// <summary>Bytes actually acquired from the stream, excluding seeks.</summary>
    public long BytesRead { get; init; }
    /// <summary>Distance from the starting stream position, including seeks and any read-ahead.</summary>
    public long BytesTraversed { get; init; }
}
