using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

// Pinned full-range BT.601 conversion: decimal matrix -> limited Q6 -> full Q7
// -> byte, preserving each integer rounding stage. No frame or row storage.
internal static partial class JpegReferenceColor
{
    // Same independently derived rounded coefficients, evaluated at compile time.
    private const int Yr = (int)((299L * 219 * 32768 + 127500) / 255000);
    private const int Yg = (int)((587L * 219 * 32768 + 127500) / 255000);
    private const int Yb = (int)((114L * 219 * 32768 + 127500) / 255000);
    private const int Ur = -(int)((169L * 224 * 32768 + 127500) / 255000);
    private const int Ug = -(int)((331L * 224 * 32768 + 127500) / 255000);
    private const int Ub = (int)((500L * 224 * 32768 + 127500) / 255000);
    private const int Vr = (int)((500L * 224 * 32768 + 127500) / 255000);
    private const int Vg = -(int)((419L * 224 * 32768 + 127500) / 255000);
    private const int Vb = -(int)((81L * 224 * 32768 + 127500) / 255000);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte Luma(int r, int g, int b)
        => FullRange((Yr * r + Yg * g + Yb * b + (16 << 15) + 256) >> 9, 219);

    // An even-width source combines RGB pairs before matrix rounding. Odd
    // widths instead filter full-resolution Q6 matrix samples horizontally.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
