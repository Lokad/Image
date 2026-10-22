// Independently written scalar stage for the pending exact JPEG encoder.
// BT.601 coefficients are generated from decimal matrix definitions, not copied
// implementation tables. The pinned oracle's staged fixed-point rounding and
// range conversion are intentional; a single floating-point matrix is different.
// This development probe is not yet a production encoding path.
using System;

public static class JpegColorParity
{
    public static bool Equal(byte[] actual, byte[] expected) => actual.AsSpan().SequenceEqual(expected);

    private static int Coefficient(int value, int range)
    {
        int magnitude = (int)((Math.Abs((long)value) * range * 32768 + 127500) / 255000);
        return value < 0 ? -magnitude : magnitude;
    }

    private static int FullRange(int limitedQ6, int range)
    {
        int scale = (255 * 16384 + range - 1) / range;
        int offset = (255 << 21) - ((16 + range) << 7) * scale + 8192;
        int fullQ7 = Math.Min(32767, (limitedQ6 * 2 * scale + offset) >> 14);
        return Math.Clamp((fullQ7 + 64) >> 7, 0, 255);
    }

    public static byte[] Convert(byte[] source, int depth)
    {
        if ((depth != 8 && depth != 16) || source.Length % (depth / 2) != 0)
            throw new ArgumentException("Complete RGBA8 or little-endian RGBA16 required.");
        int pixels = source.Length / (depth / 2);
        var result = new byte[checked(pixels * 3)];
        int yr = Coefficient(299, 219), yg = Coefficient(587, 219), yb = Coefficient(114, 219);
        int ur = Coefficient(-169, 224), ug = Coefficient(-331, 224), ub = Coefficient(500, 224);
        int vr = Coefficient(500, 224), vg = Coefficient(-419, 224), vb = Coefficient(-81, 224);
        for (int i = 0; i < pixels; i++)
        {
            int r, g, b;
            if (depth == 8)
            {
                int alpha = source[i * 4 + 3];
                r = (source[i * 4] * alpha + 32 * (255 - alpha) + 127) / 255;
                g = (source[i * 4 + 1] * alpha + 64 * (255 - alpha) + 127) / 255;
                b = (source[i * 4 + 2] * alpha + 96 * (255 - alpha) + 127) / 255;
            }
            else
            {
                int offset = i * 8;
                long alpha = source[offset + 6] | source[offset + 7] << 8;
                r = Flatten16(source[offset] | source[offset + 1] << 8, alpha, 32);
                g = Flatten16(source[offset + 2] | source[offset + 3] << 8, alpha, 64);
                b = Flatten16(source[offset + 4] | source[offset + 5] << 8, alpha, 96);
            }
            int y = (yr * r + yg * g + yb * b + (16 << 15) + 256) >> 9;
            int u = (ur * r + ug * g + ub * b + (128 << 15) + 256) >> 9;
            int v = (vr * r + vg * g + vb * b + (128 << 15) + 256) >> 9;
            result[i] = (byte)FullRange(y, 219);
            result[pixels + i] = (byte)FullRange(u, 224);
            result[pixels * 2 + i] = (byte)FullRange(v, 224);
        }
        return result;
    }

    private static int Flatten16(long value, long alpha, int background)
    {
        const long maximum = 65535, denominator = maximum * maximum;
        return (int)((value * alpha * 255 + background * (maximum - alpha) * maximum + denominator / 2) / denominator);
    }
}
