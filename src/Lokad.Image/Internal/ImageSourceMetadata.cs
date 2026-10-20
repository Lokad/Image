namespace Lokad.Image.Internal;

internal static class ImageSourceMetadata
{
    internal static ImageInfo Png(int width, int height, int depth, int color, bool transparency, ImageOrientation orientation)
        => new(ImageFormat.Png, width, height, depth, color is 4 or 6 || transparency, orientation)
        {
            SourceColor = color switch { 0 or 4 => SourceColorModel.Grayscale, 3 => SourceColorModel.Indexed, _ => SourceColorModel.Rgb },
            SourceAlpha = color is 4 or 6 ? SourceAlphaRepresentation.Channel
                : !transparency ? SourceAlphaRepresentation.None
                : color == 3 ? SourceAlphaRepresentation.PaletteTransparency : SourceAlphaRepresentation.ColorKeyTransparency,
            SourceComponentCount = color switch { 0 or 3 => 1, 2 => 3, 4 => 2, _ => 4 }
        };

    internal static ImageInfo Webp(int width, int height, bool alpha, bool lossless, bool alphaChunk, ImageOrientation orientation)
        => new(ImageFormat.WebP, width, height, 8, alpha, orientation)
        {
            SourceColor = lossless ? SourceColorModel.Rgb : SourceColorModel.YCbCr,
            // VP8L logically codes four components even when its alpha hint is unset.
            SourceAlpha = lossless || alphaChunk ? SourceAlphaRepresentation.Channel : SourceAlphaRepresentation.None,
            SourceComponentCount = lossless || alphaChunk ? (byte)4 : (byte)3
        };
}

// T.81 frame identifiers and JFIF/Adobe APP declarations identify some source
// models. Do not pretend to know an unlabelled three/four-component color model.
internal struct JpegSourceMetadata
{
    private int components, adobe;
    private uint identifiers;
    private bool jfif;
    public JpegSourceMetadata() { adobe = -1; }

    internal void Frame(int count, ReadOnlySpan<byte> descriptors)
    {
        components = count; identifiers = 0;
        for (var i = 0; i < count; i++) identifiers |= (uint)descriptors[i * 3] << (i * 8);
    }

    internal void Application(int marker, ReadOnlySpan<byte> payload)
    {
        if (marker == 0xE0 && payload.StartsWith("JFIF\0"u8)) jfif = true;
        if (marker == 0xEE && payload.Length >= 12 && payload.StartsWith("Adobe"u8))
            adobe = adobe == -1 || adobe == payload[11] ? payload[11] : -2;
    }

    internal ImageInfo Info(int width, int height, ImageOrientation orientation)
        => new(ImageFormat.Jpeg, width, height, 8, false, orientation)
        {
            SourceColor = Color,
            SourceAlpha = SourceAlphaRepresentation.None,
            SourceComponentCount = (byte)components
        };

    private readonly SourceColorModel Color => components switch
    {
        1 => SourceColorModel.Grayscale,
        3 when adobe == 0 => SourceColorModel.Rgb,
        3 when adobe == 1 => SourceColorModel.YCbCr,
        4 when adobe == 0 => SourceColorModel.Cmyk,
        4 when adobe == 2 => SourceColorModel.Ycck,
        3 when adobe == -1 && jfif => SourceColorModel.YCbCr,
        3 when adobe == -1 && identifiers == 0x00424752 => SourceColorModel.Rgb, // R,G,B
        4 when adobe == -1 && identifiers == 0x4B594D43 => SourceColorModel.Cmyk, // C,M,Y,K
        _ => SourceColorModel.Unknown
    };
}
