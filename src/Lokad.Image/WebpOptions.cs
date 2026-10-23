namespace Lokad.Image;

/// <summary>Still WebP encoding options for VP8L lossless or VP8 lossy output.</summary>
public sealed record WebpEncodeOptions
{
    public ImageLimits Limits { get; init; } = ImageLimits.Default;
    public bool Lossless { get; init; } = true;
    /// <summary>Lossy quality from 1 to 100; ignored in lossless mode. Even 100 uses quantization and 4:2:0 chroma.</summary>
    public int Quality { get; init; } = 85;
    /// <summary>Effort from 0 to 6: transform/reference search in lossless mode; more intra prediction candidates in lossy mode.</summary>
    public int Effort { get; init; } = 4;
}
