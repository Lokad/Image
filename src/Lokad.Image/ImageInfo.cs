namespace Lokad.Image;

public enum ImageFormat { Png, Jpeg, WebP }

/// <summary>The encoded source color model, before RGBA normalization or caller color overrides.</summary>
public enum SourceColorModel : byte { Unknown, Grayscale, Rgb, Indexed, YCbCr, Cmyk, Ycck }

/// <summary>How the source encodes alpha; this does not establish that any pixel is transparent.</summary>
public enum SourceAlphaRepresentation : byte { Unknown, None, Channel, PaletteTransparency, ColorKeyTransparency }

/// <summary>EXIF orientation values, with coordinates relative to the coded raster.</summary>
public enum ImageOrientation
{
    Normal = 1,
    MirrorHorizontal = 2,
    Rotate180 = 3,
    MirrorVertical = 4,
    Transpose = 5,
    Rotate90 = 6,
    Transverse = 7,
    Rotate270 = 8
}

/// <summary>Header geometry, source sample/index precision and encoded representation.</summary>
/// <remarks>BitDepth describes encoded samples or PNG indices, independently of decoded RGBA.
/// HasAlpha keeps its format-header semantics, including WebP's nonbinding alpha hint.
/// A manually constructed six-argument record has unknown source descriptors until initialized.</remarks>
public sealed record ImageInfo(ImageFormat Format, int Width, int Height, int BitDepth,
    bool HasAlpha, ImageOrientation Orientation)
{
    public SourceColorModel SourceColor { get; init; }
    public SourceAlphaRepresentation SourceAlpha { get; init; }
    /// <summary>Coded components per pixel; PNG palette indices count as one. Zero means unknown.</summary>
    public byte SourceComponentCount { get; init; }
    public int OrientedWidth => (int)Orientation >= 5 ? Height : Width;
    public int OrientedHeight => (int)Orientation >= 5 ? Width : Height;
}

public sealed record DecodeOptions
{
    public ImageLimits Limits { get; init; } = ImageLimits.Default;
    public bool ApplyOrientation { get; init; }
}
