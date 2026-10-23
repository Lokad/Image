namespace Lokad.Image.Internal;

// Device-color conversion at the pinned reference precision. The reciprocal is
// derived from the eight-bit range, and luma coefficients from rational BT.601.
// Parents own storage and admit row work before invoking these allocation-free leaves.
internal static class JpegBlackColor
{
    internal const ushort ProductScale = (1 << 16) / byte.MaxValue;
    private const int RedLuma = (int)((299L * 219 * 32768 + 127500) / 255000);
    private const int GreenLuma = (int)((587L * 219 * 32768 + 127500) / 255000);
    private const int BlueLuma = (int)((114L * 219 * 32768 + 127500) / 255000);
    private const int RangeScale = (255 * 16384 + 218) / 219;
    private const int RangeOffset = (255 << 21) - (235 << 7) * RangeScale + (1 << 13);

    internal static int Product(int value, int black) => value * black * ProductScale >> 16;

    internal static byte Gray(int red, int green, int blue)
    {
        // Round studio luma at six fractional bits, retain seven through the
        // endpoint-aligned full-range conversion, then round to an output byte.
        var studio = (RedLuma * red + GreenLuma * green + BlueLuma * blue + (16 << 15) + 256) >> 9;
        var full = Math.Min(32767, (studio * 2 * RangeScale + RangeOffset) >> 14);
        return (byte)Math.Clamp((full + 64) >> 7, 0, 255);
    }

    internal static void WriteCmyk(byte cyan, byte magenta, byte yellow, byte black, bool inverted,
        Span<byte> destination, int channels)
    {
        var k = inverted ? black : 255 - black;
        var red = Product(inverted ? cyan : 255 - cyan, k);
        var green = Product(inverted ? magenta : 255 - magenta, k);
        var blue = Product(inverted ? yellow : 255 - yellow, k);
        if (channels == 1) { destination[0] = Gray(red, green, blue); return; }
        destination[0] = (byte)red; destination[1] = (byte)green; destination[2] = (byte)blue;
        if (channels == 4) destination[3] = 255;
    }

    internal static void WriteYcck(byte luma, byte cb, byte cr, byte black, bool inverted,
        Span<byte> destination, int channels)
    {
        var k = inverted ? black : 255 - black;
        var y = (byte)Product(inverted ? 255 - luma : luma, k);
        if (channels == 1) { destination[0] = y; return; }
        Span<byte> adjusted = stackalloc byte[3];
        adjusted[0] = y;
        adjusted[1] = (byte)(Product(inverted ? 128 - cb : cb - 128, k) + 128);
        adjusted[2] = (byte)(Product(inverted ? 128 - cr : cr - 128, k) + 128);
        JpegInverseColor.WriteRow(adjusted[..1], adjusted.Slice(1, 1), adjusted.Slice(2, 1), destination, channels);
    }
}
