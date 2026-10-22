namespace Lokad.Image;

/// <summary>JPEG encoder color and chroma-sampling profiles.</summary>
public enum JpegSampling { Grayscale, YCbCr444, YCbCr422, YCbCr420 }

/// <summary>Encoder-specific quantization formulas. Numbering does not match other encoders.</summary>
public enum JpegQuantizationPolicy
{
    /// <summary>The original frequency-weighted formula, including grayscale.</summary>
    Original,
    /// <summary>Add a quality-scaled precision floor for color JPEGs; grayscale keeps Original.</summary>
    CompactColor,
    /// <summary>Managed reference-compatible baseline grayscale/4:4:4/4:2:2/4:2:0, without restart markers.
    /// Color quality maps to qscale 1 + (100 - quality) * 30 / 99; default85 maps to5.
    /// Grayscale uses the owned mathematical quality formula, matching the pinned grayscale reference at69.</summary>
    Reference
}

/// <summary>Interleaved, 8-bit caller sample layouts.</summary>
public enum JpegSampleFormat { Grayscale = 1, Rgb = 3, Cmyk = 4 }

/// <summary>Interpretation of encoded JPEG components before conversion to caller samples.</summary>
public enum JpegColorSpace { Auto, Grayscale, Rgb, YCbCr, Cmyk, Ycck }

/// <summary>Inversion after interpreting four-component CMYK or YCCK samples.</summary>
public enum JpegCmykInversion { Auto, None, Invert }

public sealed record JpegDecodeOptions
{
    public ImageLimits Limits { get; init; } = ImageLimits.Default;
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
    /// <summary>Quality from 1 to 100. Defaults to 85; numbering and mapping are profile-specific.</summary>
    public int Quality { get; init; } = 85;
    /// <summary>Original by default. CompactColor adds a precision floor; Reference selects the pinned compatible baseline profile.</summary>
    public JpegQuantizationPolicy QuantizationPolicy { get; init; }
    public JpegSampling Sampling { get; init; } = JpegSampling.YCbCr420;
    /// <summary>Require baseline JPEG syntax. Otherwise admitted Original 4:4:4 encoding may use progressive scans. Reference always uses baseline.</summary>
    public bool BaselineOnly { get; init; }
    /// <summary>RGB background for alpha flattening, packed as 0xRRGGBB. Null requires opaque input.</summary>
    public uint? BackgroundRgb { get; init; }
    /// <summary>MCUs per restart interval, from 0 (disabled) to 65535.</summary>
    public int RestartInterval { get; init; }
}
