namespace Lokad.Image.Internal;

/// <summary>Explicit centered chroma expansion and limited-range BT.601 conversion.</summary>
internal static class Vp8Color
{
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
                    var luma = (planes.Y[y * planes.Stride + x] - 16) * (255.0 / 219);
                    var u = Chroma(planes.U, planes.Stride / 2, (planes.Width + 1) / 2, (planes.Height + 1) / 2, x, y) - 128;
                    var v = Chroma(planes.V, planes.Stride / 2, (planes.Width + 1) / 2, (planes.Height + 1) / 2, x, y) - 128;
                    row[x * 4] = Sample(luma + 1.596026785714286 * v);
                    row[x * 4 + 1] = Sample(luma - 0.391762290094914 * u - 0.812967647237771 * v);
                    row[x * 4 + 2] = Sample(luma + 2.017232142857143 * u);
                    row[x * 4 + 3] = alpha is null ? (byte)255 : alpha[y * planes.Width + x];
                }
            }
            context.Release(planes.MemoryBytes + (alpha is null ? 0 : alpha.Length + 32L));
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static double Chroma(byte[] plane, int stride, int width, int height, int x, int y)
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
        // before the unchanged color matrix, with the original half-up rule.
        return (first * (4 - dy) + second * dy + 8) >> 4;
    }

    private static byte Sample(double value) => (byte)Math.Clamp((int)Math.Floor(value + 0.5), 0, 255);
}
