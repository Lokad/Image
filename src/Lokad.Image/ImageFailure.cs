namespace Lokad.Image;

/// <summary>The owner of an exhausted resource allowance.</summary>
public enum ImageLimitScope { Operation, SharedBudget }

/// <summary>Admission categories reported by <see cref="ImageLimitException.Limit"/>.</summary>
public enum ImageLimitKind
{
    Unspecified,
    Dimension,
    PixelCount,
    InputBytes,
    OutputBytes,
    WorkingBytes,
    MetadataBytes,
    ContainerItems,
    Work,
    /// <summary>A required buffer exceeds the managed array length limit.</summary>
    BufferLength
}

/// <summary>Feature categories reported by <see cref="UnsupportedImageException.Feature"/>.</summary>
public enum UnsupportedImageFeature
{
    Unspecified,
    Format,
    PixelDecoding,
    Animation,
    CodingProfile,
    BitDepth,
    ComponentLayout,
    ColorTransform,
    ControlMarker,
    SamplingLayout,
    CriticalChunk,
    Version,
    ColorSpace,
    AlphaCoding
}
