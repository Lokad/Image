namespace Lokad.Image.Internal;

/// <summary>Owned edge-by-edge formulation of RFC 6386 section 15.</summary>
internal static class Vp8LoopFilter
{
    public static void Macroblock(byte[] plane, int stride, int x, int y, int size, int level,
        int sharpness, bool simple, bool skipInterior, OperationContext context)
    {
        if (level == 0) return;
        var interior = level;
        if (sharpness != 0) interior = Math.Min(interior >> (sharpness > 4 ? 2 : 1), 9 - sharpness);
        interior = Math.Max(1, interior);
        var variance = level >= 40 ? 2 : level >= 15 ? 1 : 0;
        var outer = 2 * (level + 2) + interior; var inner = 2 * level + interior;
        if (x != 0) Line(plane, y * stride + x, 1, stride, size, outer, interior, variance, true, simple, context);
        if (!skipInterior)
            for (var column = 4; column < size; column += 4)
                Line(plane, y * stride + x + column, 1, stride, size, inner, interior, variance, false, simple, context);
        if (y != 0) Line(plane, y * stride + x, stride, 1, size, outer, interior, variance, true, simple, context);
        if (!skipInterior)
            for (var row = 4; row < size; row += 4)
                Line(plane, (y + row) * stride + x, stride, 1, size, inner, interior, variance, false, simple, context);
    }

    private static void Line(byte[] plane, int start, int step, int along, int count, int edge, int interior,
        int variance, bool macroblock, bool simple, OperationContext context)
    {
        context.Work(count * 64L);
        Span<int> samples = stackalloc int[8];
        for (var pixel = 0; pixel < count; pixel++)
        {
            var location = start + along * pixel;
            for (var i = 0; i < 8; i++) samples[i] = plane[location + (i - 4) * step];
            Segment(samples, edge, interior, variance, macroblock, simple);
            for (var i = 1; i < 7; i++) plane[location + (i - 4) * step] = (byte)samples[i];
        }
    }

    internal static void Segment(Span<int> samples, int edge, int interior, int variance, bool macroblock, bool simple)
    {
        var p0 = samples[3]; var q0 = samples[4]; var p1 = samples[2]; var q1 = samples[5];
        if (2 * Math.Abs(p0 - q0) + Math.Abs(p1 - q1) / 2 > edge) return;
        if (!simple)
            for (var i = 1; i < 8; i++)
                if (i != 4 && Math.Abs(samples[i] - samples[i - 1]) > interior) return;
        var highVariance = Math.Abs(p1 - p0) > variance || Math.Abs(q1 - q0) > variance;
        var adjustment = SignedClamp((simple || macroblock || highVariance ? SignedClamp(p1 - q1) : 0) + 3 * (q0 - p0));
        if (macroblock && !simple && !highVariance)
        {
            for (var pair = 0; pair < 3; pair++)
            {
                var change = ((27 - 9 * pair) * adjustment + 63) >> 7;
                samples[3 - pair] = Math.Clamp(samples[3 - pair] + change, 0, 255);
                samples[4 + pair] = Math.Clamp(samples[4 + pair] - change, 0, 255);
            }
        }
        else
        {
            var rightChange = SignedClamp(adjustment + 4) >> 3;
            var leftChange = SignedClamp(adjustment + 3) >> 3;
            samples[3] = Math.Clamp(p0 + leftChange, 0, 255); samples[4] = Math.Clamp(q0 - rightChange, 0, 255);
            if (!simple && !highVariance)
            {
                var change = (rightChange + 1) >> 1;
                samples[2] = Math.Clamp(p1 + change, 0, 255); samples[5] = Math.Clamp(q1 - change, 0, 255);
            }
        }
    }

    private static int SignedClamp(int value) => Math.Clamp(value, -128, 127);
}
