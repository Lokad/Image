namespace Lokad.Image.Internal;

/// <summary>Owned geometric formulation of RFC 6386 section 12 intra prediction.</summary>
internal static class Vp8Prediction
{
    public static int Sample(byte[] plane, int stride, int x, int y)
        => y < 0 ? 127 : x < 0 ? 129 : plane[y * stride + Math.Min(x, stride - 1)];

    public static void Large(byte[] plane, int stride, int x, int y, int size, int mode)
    {
        var dc = 128;
        if (mode == 0 && (x != 0 || y != 0))
        {
            var sum = 0; var count = 0;
            for (var i = 0; i < size; i++)
            {
                if (x != 0) { sum += Sample(plane, stride, x - 1, y + i); count++; }
                if (y != 0) { sum += Sample(plane, stride, x + i, y - 1); count++; }
            }
            dc = (sum + count / 2) / count;
        }
        var corner = Sample(plane, stride, x - 1, y - 1);
        for (var row = 0; row < size; row++)
            for (var column = 0; column < size; column++)
            {
                var value = mode switch
                {
                    0 => dc,
                    1 => Sample(plane, stride, x + column, y - 1),
                    2 => Sample(plane, stride, x - 1, y + row),
                    3 => Sample(plane, stride, x + column, y - 1) + Sample(plane, stride, x - 1, y + row) - corner,
                    _ => throw new ImageDataException("Invalid VP8 large-block prediction.")
                };
                plane[(y + row) * stride + x + column] = (byte)Math.Clamp(value, 0, 255);
            }
    }

    public static void Small(byte[] plane, int stride, int x, int y, int macroX, int macroY, int mode)
    {
        Span<int> top = stackalloc int[8]; Span<int> left = stackalloc int[4]; Span<int> edge = stackalloc int[9];
        for (var i = 0; i < 8; i++)
            top[i] = i >= 4 && x == macroX + 12
                ? Sample(plane, stride, macroX + 12 + i, macroY - 1) : Sample(plane, stride, x + i, y - 1);
        for (var i = 0; i < 4; i++) { left[i] = Sample(plane, stride, x - 1, y + i); edge[3 - i] = left[i]; edge[5 + i] = top[i]; }
        var corner = edge[4] = Sample(plane, stride, x - 1, y - 1);
        var dc = (top[0] + top[1] + top[2] + top[3] + left[0] + left[1] + left[2] + left[3] + 4) >> 3;
        for (var row = 0; row < 4; row++)
            for (var column = 0; column < 4; column++)
                plane[(y + row) * stride + x + column] = (byte)Math.Clamp(SmallPixel(mode, column, row, top, left, edge, corner, dc), 0, 255);
    }

    private static int SmallPixel(int mode, int x, int y, ReadOnlySpan<int> top, ReadOnlySpan<int> left,
        ReadOnlySpan<int> edge, int corner, int dc)
    {
        switch (mode)
        {
            case 0: return dc;
            case 1: return left[y] + top[x] - corner;
            case 2: return Average3(x == 0 ? corner : top[x - 1], top[x], top[x + 1]);
            case 3: return Average3(y == 0 ? corner : left[y - 1], left[y], left[Math.Min(y + 1, 3)]);
            case 4: { var z = x + y; return Average3(top[z], top[z + 1], top[Math.Min(z + 2, 7)]); }
            case 5: return Center(edge, 4 + x - y);
            case 6:
                {
                    var z = 2 * x - y;
                    if (z < -1) return Center(edge, 5 + z);
                    return (z & 1) == 0 ? Average2(edge[4 + z / 2], edge[5 + z / 2]) : Center(edge, 4 + (z + 1) / 2);
                }
            case 7:
                {
                    var z = 2 * x + y;
                    if (z >= 8) return Center(top, z - 3);
                    return (z & 1) == 0 ? Average2(top[z / 2], top[z / 2 + 1]) : Center(top, (z + 1) / 2);
                }
            case 8:
                {
                    var z = 2 * y - x;
                    if (z < -1) return Center(edge, 3 - z);
                    return (z & 1) == 0 ? Average2(edge[3 - z / 2], edge[4 - z / 2]) : Center(edge, 3 - (z - 1) / 2);
                }
            case 9:
                {
                    var z = 2 * y + x;
                    if (z >= 6) return left[3];
                    return (z & 1) == 0 ? Average2(left[z / 2], left[z / 2 + 1])
                        : Average3(left[z / 2], left[z / 2 + 1], left[Math.Min(z / 2 + 2, 3)]);
                }
            default: throw new ImageDataException("Invalid VP8 subblock prediction.");
        }
    }

    private static int Average2(int a, int b) => (a + b + 1) >> 1;
    private static int Average3(int a, int b, int c) => (a + 2 * b + c + 2) >> 2;
    private static int Center(ReadOnlySpan<int> values, int index) => Average3(values[index - 1], values[index], values[index + 1]);
}
