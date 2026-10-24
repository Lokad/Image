namespace Lokad.Image.Internal;

/// <summary>Centered byte chroma and explicitly normalized limited-range BT.601.</summary>
internal static class Vp8Color
{
    // Derive coefficients from BT.601 Kr=299/1000, Kb=114/1000 and the
    // studio luma/chroma ranges219/224. Preserve the Q16-to-Q13 rounding.
    private const int Luma = ((65536 * 255 / 219) + 4) >> 3;
    private static readonly int Red = Inverse(1402, 1000, false);
    private static readonly int Blue = Inverse(1772, 1000, false);
    private static readonly int GreenU = Inverse(2L * 114 * 886, 1000L * 587, true);
    private static readonly int GreenV = Inverse(2L * 299 * 701, 1000L * 587, true);
    private static readonly int Yr = Forward(299, 219), Yg = Forward(587, 219), Yb = Forward(114, 219);
    private static readonly int Ur = -Forward(169, 224), Ug = -Forward(331, 224), Ub = Forward(500, 224);
    private static readonly int Vr = Forward(500, 224), Vg = -Forward(419, 224), Vb = -Forward(81, 224);

    private static int Inverse(long numerator, long denominator, bool negative)
    {
        var q16 = (numerator * 255 * 65536 + denominator * 112) / (denominator * 224);
        return (int)(((negative ? -q16 : q16) + 4) >> 3);
    }

    private static int Forward(int weight, int range)
        => (int)((weight * (long)range * 32768 + 127500) / 255000);

    public static ImageBuffer Render(Vp8Planes planes, byte[]? alpha, OperationContext context)
    {
        var result = ImageBuffer.Allocate(planes.Width, planes.Height, 8, context);
        try
        {
            for (var y = 0; y < planes.Height; y++)
            {
                context.Work(planes.Width * 64L);
                var row = result.GetRow8(y);
                for (var x = 0; x < planes.Width; x++)
                {
                    var u = Chroma(planes.U, planes.Stride / 2, (planes.Width + 1) / 2, (planes.Height + 1) / 2, x, y) - 128;
                    var v = Chroma(planes.V, planes.Stride / 2, (planes.Width + 1) / 2, (planes.Height + 1) / 2, x, y) - 128;
                    WritePixel(planes.Y[y * planes.Stride + x], u, v, row.Slice(x * 4, 3));
                    row[x * 4 + 3] = alpha is null ? (byte)255 : alpha[y * planes.Width + x];
                }
            }
            context.Release(planes.MemoryBytes + (alpha is null ? 0 : alpha.Length + 32L));
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static void WritePixel(byte luma, int u, int v, Span<byte> output)
    {
        // The approved saturated16-bit RGB normalization is followed by a Q15
        // studio-YUV round trip before reduction to bytes. Keeping those
        // intermediate precision steps avoids a different half-sample policy.
        // All intermediates are scalars; no converted frame or row is stored.
        var y = (luma - 16) * Luma;
        var red = Math.Clamp((y + v * Red + 16) >> 5, 0, 65535);
        var green = Math.Clamp((y + u * GreenU + v * GreenV + 16) >> 5, 0, 65535);
        var blue = Math.Clamp((y + u * Blue + 16) >> 5, 0, 65535);
        var studioY = (Yr * (long)red + Yg * (long)green + Yb * (long)blue + (8193L << 14)) >> 15;
        var studioU = (Ur * (long)red + Ug * (long)green + Ub * (long)blue + (65537L << 14)) >> 15;
        var studioV = (Vr * (long)red + Vg * (long)green + Vb * (long)blue + (65537L << 14)) >> 15;
        // Truncate16-bit samples to15bits before the final Q13 conversion.
        y = (int)((studioY >> 1) - 2048) * Luma;
        u = (int)(studioU >> 1) - 16384;
        v = (int)(studioV >> 1) - 16384;
        output[0] = Sample(y + v * Red);
        output[1] = Sample(y + u * GreenU + v * GreenV);
        output[2] = Sample(y + u * Blue);
    }

    private static byte Sample(int value) => (byte)Math.Clamp((value + (1 << 19)) >> 20, 0, 255);

    private static int Chroma(byte[] plane, int stride, int width, int height, int x, int y)
    {
        // Render supplies nonnegative coordinates within the doubled chroma
        // extent. Centered phases are 1/4 on odd pixels and 3/4 on even pixels;
        // the first pixel clamps to phase zero. Integer division maps -1/2 to
        // zero here, while duplicated last samples provide the other edge.
        var left = (x - 1) / 2; var top = (y - 1) / 2;
        var right = Math.Min(left + 1, width - 1); var bottom = Math.Min(top + 1, height - 1);
        var dx = x == 0 ? 0 : (x & 1) == 0 ? 3 : 1;
        var dy = y == 0 ? 0 : (y & 1) == 0 ? 3 : 1;
        var first = plane[top * stride + left] * (4 - dx) + plane[top * stride + right] * dx;
        var second = plane[bottom * stride + left] * (4 - dx) + plane[bottom * stride + right] * dx;
        // The weighted sum is at most 255*16. Round the exact /16 expansion
        // before the normalized integer color conversion, with half-up rounding.
        return (first * (4 - dy) + second * dy + 8) >> 4;
    }

}
