namespace Lokad.Image.Internal;

// Full-range BT.601 conversion at the pinned reference's integer precision.
// Derive coefficients from Kr=299/1000 and Kb=114/1000, rather than incorporating
// a native conversion table. Callers admit row work and own all supplied spans.
internal static class JpegInverseColor
{
    private static readonly int Red = Coefficient(2 * (1000 - 299), 1000, false);
    private static readonly int Blue = Coefficient(2 * (1000 - 114), 1000, false);
    private static readonly int GreenU = Coefficient(2L * 114 * (1000 - 114), 1000L * (1000 - 299 - 114), true);
    private static readonly int GreenV = Coefficient(2L * 299 * (1000 - 299), 1000L * (1000 - 299 - 114), true);

    private static int Coefficient(long numerator, long denominator, bool negative)
    {
        // The pinned conversion first rounds a studio-chroma Q16 coefficient,
        // then removes that range expansion with integer division, then rounds
        // to Q13. Preserve each precision boundary, including signed rounding.
        var expanded = (numerator * 255 * 65536 + denominator * 112) / (denominator * 224);
        var fullRange = (negative ? -expanded : expanded) * 224 / 255;
        return (int)((fullRange + 4) >> 3);
    }

    internal static void WriteRow(ReadOnlySpan<byte> luma, ReadOnlySpan<byte> cb, ReadOnlySpan<byte> cr,
        Span<byte> destination, int channels)
    {
        if (channels is not (3 or 4)) throw new ArgumentOutOfRangeException(nameof(channels));
        var required = (long)luma.Length * channels;
        if (cb.Length != luma.Length || cr.Length != luma.Length || required > destination.Length)
            throw new ArgumentException("Color planes and destination must cover the complete row.");
        var target = destination[..(int)required];
        if (luma.Overlaps(target) || cb.Overlaps(target) || cr.Overlaps(target))
            throw new ArgumentException("Color inputs and destination must not overlap.");
        for (var x = 0; x < luma.Length; x++)
        {
            var y = (luma[x] << 13) + (1 << 12);
            var u = cb[x] - 128; var v = cr[x] - 128;
            target[x * channels] = (byte)Math.Clamp((y + v * Red) >> 13, 0, 255);
            target[x * channels + 1] = (byte)Math.Clamp((y + u * GreenU + v * GreenV) >> 13, 0, 255);
            target[x * channels + 2] = (byte)Math.Clamp((y + u * Blue) >> 13, 0, 255);
            if (channels == 4) target[x * channels + 3] = 255;
        }
    }
}
