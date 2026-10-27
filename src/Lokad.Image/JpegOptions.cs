namespace Lokad.Image;

/// <summary>JPEG encoder color and chroma-sampling profiles.</summary>
public enum JpegSampling { Grayscale, YCbCr444, YCbCr422, YCbCr420 }

/// <summary>Interleaved, 8-bit caller sample layouts.</summary>
public enum JpegSampleFormat { Grayscale = 1, Rgb = 3, Cmyk = 4 }

/// <summary>Interpretation of encoded JPEG components before conversion to caller samples.</summary>
public enum JpegColorSpace { Auto, Grayscale, Rgb, YCbCr, Cmyk, Ycck }

/// <summary>Inversion after interpreting four-component CMYK or YCCK samples.</summary>
public enum JpegCmykInversion { Auto, None, Invert }

public sealed record JpegDecodeOptions
{
    public ImageLimits Limits { get; init; } = ImageLimits.Default;
    public ImageMetadataPolicy MetadataPolicy { get; init; }
    /// <summary>Auto uses component count, Adobe declarations and RGB component IDs.</summary>
    public JpegColorSpace SourceColorSpace { get; init; }
    /// <summary>Auto inverts Adobe APP14 transform 0/2 CMYK values; untagged data is not inverted.</summary>
    public JpegCmykInversion CmykInversion { get; init; }
    /// <summary>Opt in to EXIF reorientation while writing caller samples.</summary>
    public bool ApplyOrientation { get; init; }
}

public sealed record JpegEncodeOptions
{
    public ImageLimits Limits { get; init; } = ImageLimits.Default;
    /// <summary>Quality from 1 to 100, default 85. Color maps to qscale 1 + (100 - quality) * 30 / 99; grayscale uses its owned quantizers.</summary>
    public int Quality { get; init; } = 85;
    public JpegSampling Sampling { get; init; } = JpegSampling.YCbCr420;
    /// <summary>RGB background for alpha flattening, packed as 0xRRGGBB. Null requires opaque input.</summary>
    public uint? BackgroundRgb { get; init; }
    /// <summary>MCUs per restart interval, from 0 (disabled) to 65535.</summary>
    public int RestartInterval { get; init; }
}
