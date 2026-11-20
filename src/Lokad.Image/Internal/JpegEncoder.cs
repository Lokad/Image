namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    public static void Encode(ImageBuffer image, Stream output, JpegEncodeOptions options, OperationContext context)
        => EncodeReference(image, default, output, options, context);

    private static void ReadRgb(ImageBuffer image, int x, int y, uint? background, out int r, out int g, out int b)
    {
        var color = background ?? 0;
        // Acquire/validate the row once, then bound all four channel reads to
        // one complete pixel. No row or converted-pixel cache is retained.
        if (image.BitDepth == 8)
        {
            var pixel = image.GetRow8(y).Slice(x * 4, 4); var alpha = pixel[3];
            if (alpha != 255 && background is null) throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", nameof(image));
            r = Flatten8(pixel[0], alpha, (int)((color >> 16) & 255));
            g = Flatten8(pixel[1], alpha, (int)((color >> 8) & 255));
            b = Flatten8(pixel[2], alpha, (int)(color & 255));
            return;
        }
        var sample = image.GetRow16(y).Slice(x * 4, 4); var opacity = sample[3];
        if (opacity != 65535 && background is null) throw new ArgumentException("JPEG requires opaque pixels or an explicit RGB background.", nameof(image));
        r = Flatten(sample[0], opacity, (int)((color >> 16) & 255), 65535);
        g = Flatten(sample[1], opacity, (int)((color >> 8) & 255), 65535);
        b = Flatten(sample[2], opacity, (int)(color & 255), 65535);
    }

    // The specified encoded-space blend at maximum 255 cancels one factor
    // 255. Its odd denominator has no half ties; nearest rounding is +127.
    // Byte inputs bound the numerator to 65025, so int arithmetic is exact.
    internal static int Flatten8(int value, int alpha, int background)
        => (value * alpha + background * (255 - alpha) + 127) / 255;

    private static int Flatten(int value, int alpha, int background, int maximum)
    {
        var numerator = value * (long)alpha * 255 + background * (long)(maximum - alpha) * maximum;
        var denominator = (long)maximum * maximum;
        return (int)((numerator + denominator / 2) / denominator);
    }
}
