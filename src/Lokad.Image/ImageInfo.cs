namespace Lokad.Image;

public enum ImageFormat { Png, Jpeg, WebP }

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

public sealed record ImageInfo(ImageFormat Format, int Width, int Height, int BitDepth,
    bool HasAlpha, ImageOrientation Orientation)
{
    public int OrientedWidth => (int)Orientation >= 5 ? Height : Width;
    public int OrientedHeight => (int)Orientation >= 5 ? Width : Height;
}

public sealed record DecodeOptions
{
    public ImageLimits Limits { get; init; } = ImageLimits.Default;
    public bool ApplyOrientation { get; init; }
}
