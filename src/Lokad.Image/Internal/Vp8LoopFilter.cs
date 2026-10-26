using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
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
        context.Cancellation.ThrowIfCancellationRequested();
        if (Vector128.IsHardwareAccelerated && along == 1 && count is 8 or 16 && step >= count
            && start >= 4L * step && (long)start + 3L * step + count <= plane.Length)
        {
            for (var pixel = 0; pixel < count; pixel += 8)
                Horizontal8(plane, start + pixel, step, edge, interior, variance, macroblock, simple);
            return;
        }
        if (Sse2.IsSupported && step == 1 && count is 8 or 16 && along >= 8
            && start >= 4 && start % along >= 4 && start % along <= along - 4
            && (long)start + (count - 1L) * along + 3 < plane.Length)
        {
            for (var pixel = 0; pixel < count; pixel += 8)
                Vertical8(plane, start + pixel * along, along, edge, interior, variance, macroblock, simple);
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

    // RFC 6386 section 15: transpose eight independent row neighborhoods into
    // the same signed-word arithmetic as horizontal edges, then transpose back.
    // The caller bounds every eight-byte row and charges the original full line.
    private static void Vertical8(byte[] plane, int start, int stride, int edge, int interior,
        int variance, bool macroblock, bool simple)
    {
        Transpose8(Row8(plane, start - 4), Row8(plane, start + stride - 4),
            Row8(plane, start + 2 * stride - 4), Row8(plane, start + 3 * stride - 4),
            Row8(plane, start + 4 * stride - 4), Row8(plane, start + 5 * stride - 4),
            Row8(plane, start + 6 * stride - 4), Row8(plane, start + 7 * stride - 4),
            out var columns01, out var columns23, out var columns45, out var columns67);
        var zero = Vector128<byte>.Zero;
        var p3 = Sse2.UnpackLow(columns01, zero).AsInt16(); var p2 = Sse2.UnpackHigh(columns01, zero).AsInt16();
        var p1 = Sse2.UnpackLow(columns23, zero).AsInt16(); var p0 = Sse2.UnpackHigh(columns23, zero).AsInt16();
        var q0 = Sse2.UnpackLow(columns45, zero).AsInt16(); var q1 = Sse2.UnpackHigh(columns45, zero).AsInt16();
        var q2 = Sse2.UnpackLow(columns67, zero).AsInt16(); var q3 = Sse2.UnpackHigh(columns67, zero).AsInt16();
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
        columns01 = PackRows(p3, p2 + third); columns23 = PackRows(p1 + second, p0 + left);
        columns45 = PackRows(q0 - right, q1 - second); columns67 = PackRows(q2 - third, q3);
        Transpose8(columns01, Sse2.ShiftRightLogical128BitLane(columns01, 8),
            columns23, Sse2.ShiftRightLogical128BitLane(columns23, 8),
            columns45, Sse2.ShiftRightLogical128BitLane(columns45, 8),
            columns67, Sse2.ShiftRightLogical128BitLane(columns67, 8),
            out var rows01, out var rows23, out var rows45, out var rows67);
        StoreRows(plane, start - 4, stride, rows01); StoreRows(plane, start + 2 * stride - 4, stride, rows23);
        StoreRows(plane, start + 4 * stride - 4, stride, rows45); StoreRows(plane, start + 6 * stride - 4, stride, rows67);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose8(Vector128<byte> a, Vector128<byte> b, Vector128<byte> c, Vector128<byte> d,
        Vector128<byte> e, Vector128<byte> f, Vector128<byte> g, Vector128<byte> h,
        out Vector128<byte> first, out Vector128<byte> second, out Vector128<byte> third, out Vector128<byte> fourth)
    {
        var ab = Sse2.UnpackLow(a, b); var cd = Sse2.UnpackLow(c, d);
        var ef = Sse2.UnpackLow(e, f); var gh = Sse2.UnpackLow(g, h);
        var abcd0 = Sse2.UnpackLow(ab.AsUInt16(), cd.AsUInt16()).AsUInt32();
        var abcd1 = Sse2.UnpackHigh(ab.AsUInt16(), cd.AsUInt16()).AsUInt32();
        var efgh0 = Sse2.UnpackLow(ef.AsUInt16(), gh.AsUInt16()).AsUInt32();
        var efgh1 = Sse2.UnpackHigh(ef.AsUInt16(), gh.AsUInt16()).AsUInt32();
        first = Sse2.UnpackLow(abcd0, efgh0).AsByte(); second = Sse2.UnpackHigh(abcd0, efgh0).AsByte();
        third = Sse2.UnpackLow(abcd1, efgh1).AsByte(); fourth = Sse2.UnpackHigh(abcd1, efgh1).AsByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Row8(byte[] plane, int offset)
        => Vector128.Create(MemoryMarshal.Read<Vector64<byte>>(plane.AsSpan(offset, 8)), Vector64<byte>.Zero);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> PackRows(Vector128<short> first, Vector128<short> second)
    {
        var zero = Vector128<short>.Zero; var maximum = Vector128.Create((short)255);
        first = Vector128.Min(Vector128.Max(first, zero), maximum);
        second = Vector128.Min(Vector128.Max(second, zero), maximum);
        return Vector128.Narrow(first.AsUInt16(), second.AsUInt16());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StoreRows(byte[] plane, int offset, int stride, Vector128<byte> rows)
    {
        var first = rows.GetLower(); var second = rows.GetUpper();
        MemoryMarshal.Write(plane.AsSpan(offset, 8), in first);
        MemoryMarshal.Write(plane.AsSpan(offset + stride, 8), in second);
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
