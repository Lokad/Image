namespace Lokad.Image;

/// <summary>Validation of optional EXIF content, independent of container and pixel validity.</summary>
public enum ImageMetadataPolicy
{
    /// <summary>Reject unusable IFD0 headers/orientation and duplicate or misplaced EXIF. This is the default.</summary>
    Strict,
    /// <summary>Ignore unusable EXIF; the last usable orientation wins. Bounds, checksums
    /// and required container/coding structures still apply. Unused tags are never expanded.</summary>
    IgnoreInvalid
}
