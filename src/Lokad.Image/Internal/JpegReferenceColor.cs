namespace Lokad.Image.Internal;

// Pinned full-range BT.601 conversion: decimal matrix -> limited Q6 -> full Q7
// -> byte, preserving each integer rounding stage. No frame or row storage.
internal static partial class JpegReferenceColor
{
    private static readonly int Yr = Coefficient(299, 219), Yg = Coefficient(587, 219), Yb = Coefficient(114, 219);
    private static readonly int Ur = Coefficient(-169, 224), Ug = Coefficient(-331, 224), Ub = Coefficient(500, 224);
    private static readonly int Vr = Coefficient(500, 224), Vg = Coefficient(-419, 224), Vb = Coefficient(-81, 224);

    private static int Coefficient(int value, int range)
    {
        var magnitude = (int)((Math.Abs((long)value) * range * 32768 + 127500) / 255000);
        return value < 0 ? -magnitude : magnitude;
    }

    private static byte FullRange(int limitedQ6, int range)
    {
        var scale = (255 * 16384 + range - 1) / range;
        var offset = (255 << 21) - ((16 + range) << 7) * scale + 8192;
        var fullQ7 = Math.Min(32767, (limitedQ6 * 2 * scale + offset) >> 14);
        return (byte)Math.Clamp((fullQ7 + 64) >> 7, 0, 255);
    }

    internal static void Convert(int r, int g, int b, out byte y, out byte cb, out byte cr)
    {
        y = FullRange((Yr * r + Yg * g + Yb * b + (16 << 15) + 256) >> 9, 219);
        cb = FullRange((Ur * r + Ug * g + Ub * b + (128 << 15) + 256) >> 9, 224);
        cr = FullRange((Vr * r + Vg * g + Vb * b + (128 << 15) + 256) >> 9, 224);
    }

    internal static byte Luma(int r, int g, int b)
        => FullRange((Yr * r + Yg * g + Yb * b + (16 << 15) + 256) >> 9, 219);

    // An even-width source combines RGB pairs before matrix rounding. Odd
    // widths instead filter full-resolution Q6 matrix samples horizontally.
    internal static void LimitedChroma(int r, int g, int b, bool pair, out int cb, out int cr)
    {
        var shift = pair ? 10 : 9;
        var offset = pair ? (128 << 16) + 512 : (128 << 15) + 256;
        cb = (Ur * r + Ug * g + Ub * b + offset) >> shift;
        cr = (Vr * r + Vg * g + Vb * b + offset) >> shift;
    }

    internal static int FullChromaQ7(int limitedQ7)
    {
        const int range = 224;
        var scale = (255 * 16384 + range - 1) / range;
        var offset = (255 << 21) - ((16 + range) << 7) * scale + 8192;
        return Math.Min(32767, (limitedQ7 * scale + offset) >> 14);
    }
}
