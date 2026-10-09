using System.IO.Compression;

namespace Lokad.Image;

public enum PngColorType { Grayscale = 0, Rgb = 2, GrayscaleAlpha = 4, Rgba = 6 }

public sealed record PngEncodeOptions
{
    public ImageLimits Limits { get; init; } = ImageLimits.Default;
    public PngColorType ColorType { get; init; } = PngColorType.Rgba;
    public CompressionLevel Compression { get; init; } = CompressionLevel.Optimal;
}
