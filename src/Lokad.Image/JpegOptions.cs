namespace Lokad.Image;

/// <summary>JPEG encoder color and chroma-sampling profiles.</summary>
public enum JpegSampling { Grayscale, YCbCr444, YCbCr422, YCbCr420 }

public sealed record JpegEncodeOptions
{
    public ImageLimits Limits { get; init; } = ImageLimits.Default;
    /// <summary>Original quantization policy, from 1 to 100. Defaults to 85; numbering is encoder-specific.</summary>
    public int Quality { get; init; } = 85;
    public JpegSampling Sampling { get; init; } = JpegSampling.YCbCr420;
    /// <summary>RGB background for alpha flattening, packed as 0xRRGGBB. Null requires opaque input.</summary>
    public uint? BackgroundRgb { get; init; }
    /// <summary>MCUs per restart interval, from 0 (disabled) to 65535.</summary>
    public int RestartInterval { get; init; }
}
