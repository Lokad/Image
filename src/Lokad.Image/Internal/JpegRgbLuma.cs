namespace Lokad.Image.Internal;

// Full-range grayscale from normalized eight-bit RGB. Generate the BT.601
// coefficients from rational weights, preserving the pinned precision steps.
internal static class JpegRgbLuma
{
    private const int RedLuma = (int)((299L * 219 * 32768 + 127500) / 255000);
    private const int GreenLuma = (int)((587L * 219 * 32768 + 127500) / 255000);
    private const int BlueLuma = (int)((114L * 219 * 32768 + 127500) / 255000);
    private const int RangeScale = (255 * 16384 + 218) / 219;
    private const int RangeOffset = (255 << 21) - (235 << 7) * RangeScale + (1 << 13);

    internal static byte FromRgb(byte red, byte green, byte blue)
    {
        // Round studio luma at six fractional bits, retain seven through the
        // endpoint-aligned full-range conversion, then round to an output byte.
        var studio = (RedLuma * red + GreenLuma * green + BlueLuma * blue + (16 << 15) + 256) >> 9;
        var full = Math.Min(32767, (studio * 2 * RangeScale + RangeOffset) >> 14);
        return (byte)Math.Clamp((full + 64) >> 7, 0, 255);
    }

}
