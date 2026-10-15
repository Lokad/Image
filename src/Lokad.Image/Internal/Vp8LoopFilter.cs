using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

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
        if (Vector128.IsHardwareAccelerated && along == 1 && count is 8 or 16 && step >= count
            && start >= 4L * step && (long)start + 3L * step + count <= plane.Length)
        {
            for (var pixel = 0; pixel < count; pixel += 8)
                Horizontal8(plane, start + pixel, step, edge, interior, variance, macroblock, simple);
            return;
        }
        Span<int> samples = stackalloc int[8];
        for (var pixel = 0; pixel < count; pixel++)
        {
            var location = start + along * pixel;
            for (var i = 0; i < 8; i++) samples[i] = plane[location + (i - 4) * step];
            Segment(samples, edge, interior, variance, macroblock, simple);
            for (var i = 1; i < 7; i++) plane[location + (i - 4) * step] = (byte)samples[i];
        }
    }

    // RFC 6386 section 15, eight independent segments in signed word lanes.
    // Differences are at most255, the unclamped adjustment at most893, and
    // weighted adjustments at most3456: every intermediate fits a signed word.
    private static void Horizontal8(byte[] plane, int start, int step, int edge, int interior,
        int variance, bool macroblock, bool simple)
    {
        var p3 = Load8(plane, start - 4 * step); var p2 = Load8(plane, start - 3 * step);
        var p1 = Load8(plane, start - 2 * step); var p0 = Load8(plane, start - step);
        var q0 = Load8(plane, start); var q1 = Load8(plane, start + step);
        var q2 = Load8(plane, start + 2 * step); var q3 = Load8(plane, start + 3 * step);
        var enabled = Vector128.LessThanOrEqual(Vector128.Abs(p0 - q0) * Vector128.Create((short)2)
            + (Vector128.Abs(p1 - q1) >> 1), Vector128.Create((short)edge));
        if (!simple)
        {
            var limit = Vector128.Create((short)interior);
            enabled &= Vector128.LessThanOrEqual(Vector128.Abs(p2 - p3), limit)
                & Vector128.LessThanOrEqual(Vector128.Abs(p1 - p2), limit)
                & Vector128.LessThanOrEqual(Vector128.Abs(p0 - p1), limit)
                & Vector128.LessThanOrEqual(Vector128.Abs(q1 - q0), limit)
                & Vector128.LessThanOrEqual(Vector128.Abs(q2 - q1), limit)
                & Vector128.LessThanOrEqual(Vector128.Abs(q3 - q2), limit);
        }
        if (Vector128.EqualsAll(enabled, Vector128<short>.Zero)) return;
        var threshold = Vector128.Create((short)variance);
        var highVariance = Vector128.GreaterThan(Vector128.Abs(p1 - p0), threshold)
            | Vector128.GreaterThan(Vector128.Abs(q1 - q0), threshold);
        var difference = SignedClamp(p1 - q1);
        if (!simple && !macroblock) difference &= highVariance;
        var adjustment = SignedClamp(difference + (q0 - p0) * Vector128.Create((short)3));
        var right = SignedClamp(adjustment + Vector128.Create((short)4)) >> 3;
        var left = SignedClamp(adjustment + Vector128.Create((short)3)) >> 3;
        var second = Vector128<short>.Zero; var third = Vector128<short>.Zero;
        if (macroblock && !simple)
        {
            var rounding = Vector128.Create((short)63);
            var first = (adjustment * Vector128.Create((short)27) + rounding) >> 7;
            left = Vector128.ConditionalSelect(highVariance, left, first);
            right = Vector128.ConditionalSelect(highVariance, right, first);
            second = ~highVariance & ((adjustment * Vector128.Create((short)18) + rounding) >> 7);
            third = ~highVariance & ((adjustment * Vector128.Create((short)9) + rounding) >> 7);
        }
        else if (!simple)
            second = ~highVariance & ((right + Vector128.Create((short)1)) >> 1);
        left &= enabled; right &= enabled; second &= enabled; third &= enabled;
        Store8(plane, start - 3 * step, p2 + third); Store8(plane, start + 2 * step, q2 - third);
        Store8(plane, start - 2 * step, p1 + second); Store8(plane, start + step, q1 - second);
        Store8(plane, start - step, p0 + left); Store8(plane, start, q0 - right);
    }

    private static Vector128<short> Load8(byte[] plane, int offset)
        => Vector128.WidenLower(Vector128.Create(MemoryMarshal.Read<Vector64<byte>>(plane.AsSpan(offset, 8)),
            Vector64<byte>.Zero)).AsInt16();

    private static void Store8(byte[] plane, int offset, Vector128<short> value)
    {
        value = Vector128.Min(Vector128.Max(value, Vector128<short>.Zero), Vector128.Create((short)255));
        var packed = Vector128.Narrow(value.AsUInt16(), Vector128<ushort>.Zero).GetLower();
        MemoryMarshal.Write(plane.AsSpan(offset, 8), in packed);
    }

    private static Vector128<short> SignedClamp(Vector128<short> value)
        => Vector128.Min(Vector128.Max(value, Vector128.Create((short)-128)), Vector128.Create((short)127));

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
