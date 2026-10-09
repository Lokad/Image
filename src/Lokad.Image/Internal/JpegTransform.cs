namespace Lokad.Image.Internal;

internal static class JpegTransform
{
    // Natural positions for JPEG's diagonal scan, generated from the geometry
    // defined in T.81 rather than copied from an implementation's lookup table.
    public static readonly int[] Zigzag = BuildZigzag();
    // Original binary64 evaluation of T.81 A.3.3: C(u) cos((2x+1)u*pi/16),
    // C(0)=1/sqrt(2), C(u>0)=1. These round-trip values pin the first owned
    // implementation's numerical policy. Platform Math.Cos implementations can
    // differ by one ULP and change quantization or sample rounding at half ties.
    // This is a mathematically generated basis, not an upstream codec table.
    // Row-major storage avoids rectangular-array addressing in the inner loops.
    private static readonly double[] Cosines =
    [
        0.7071067811865475, 0.9807852804032304, 0.9238795325112867, 0.8314696123025452, 0.7071067811865476, 0.5555702330196023, 0.38268343236508984, 0.19509032201612833,
        0.7071067811865475, 0.8314696123025452, 0.38268343236508984, -0.1950903220161282, -0.7071067811865475, -0.9807852804032304, -0.9238795325112868, -0.5555702330196022,
        0.7071067811865475, 0.5555702330196023, -0.3826834323650897, -0.9807852804032304, -0.7071067811865477, 0.1950903220161283, 0.9238795325112865, 0.8314696123025456,
        0.7071067811865475, 0.19509032201612833, -0.9238795325112867, -0.5555702330196022, 0.7071067811865474, 0.8314696123025456, -0.3826834323650899, -0.9807852804032307,
        0.7071067811865475, -0.1950903220161282, -0.9238795325112868, 0.5555702330196018, 0.7071067811865477, -0.8314696123025451, -0.38268343236509056, 0.9807852804032305,
        0.7071067811865475, -0.555570233019602, -0.38268343236509034, 0.9807852804032304, -0.7071067811865467, -0.19509032201612803, 0.9238795325112867, -0.831469612302545,
        0.7071067811865475, -0.8314696123025453, 0.38268343236509, 0.19509032201612878, -0.7071067811865471, 0.9807852804032307, -0.9238795325112864, 0.5555702330196015,
        0.7071067811865475, -0.9807852804032304, 0.9238795325112865, -0.8314696123025451, 0.7071067811865466, -0.5555702330196015, 0.38268343236508956, -0.19509032201612858
    ];

    private static int[] BuildZigzag()
    {
        var result = new int[64]; var index = 0;
        for (var diagonal = 0; diagonal <= 14; diagonal++)
        {
            var first = Math.Max(0, diagonal - 7); var last = Math.Min(7, diagonal);
            for (var position = first; position <= last; position++)
            {
                var row = (diagonal & 1) == 0 ? last - (position - first) : position;
                var column = diagonal - row;
                result[index++] = row * 8 + column;
            }
        }
        return result;
    }

    public static void Inverse(ReadOnlySpan<int> coefficients, ReadOnlySpan<ushort> quantization,
        Span<byte> pixels, OperationContext context)
    {
        if (coefficients.Length != 64 || quantization.Length != 64 || pixels.Length != 64)
            throw new ArgumentException("JPEG transforms operate on complete 8x8 blocks.");
        context.Work(4096);
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            double sum = 0;
            for (var v = 0; v < 8; v++)
            {
                var vertical = Cosines[y * 8 + v];
                for (var u = 0; u < 8; u++)
                    sum += (double)coefficients[v * 8 + u] * quantization[v * 8 + u] * Cosines[x * 8 + u] * vertical;
            }
            pixels[y * 8 + x] = (byte)Math.Clamp(Math.Round(sum / 4 + 128, MidpointRounding.AwayFromZero), 0, 255);
        }
    }

    public static void Forward(ReadOnlySpan<byte> pixels, Span<double> coefficients, OperationContext context)
    {
        if (pixels.Length != 64 || coefficients.Length != 64)
            throw new ArgumentException("JPEG transforms operate on complete 8x8 blocks.");
        context.Work(4096);
        for (var v = 0; v < 8; v++)
        for (var u = 0; u < 8; u++)
        {
            double sum = 0;
            for (var y = 0; y < 8; y++)
            {
                var vertical = Cosines[y * 8 + v];
                for (var x = 0; x < 8; x++) sum += (pixels[y * 8 + x] - 128) * Cosines[x * 8 + u] * vertical;
            }
            coefficients[v * 8 + u] = sum / 4;
        }
    }
}
