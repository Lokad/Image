using System.Buffers.Binary;
using System.Runtime.CompilerServices;

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

    // RFC 6386 section 12: encoder candidates overwrite only this block;
    // every top/left/corner sample lies outside it and is immutable throughout
    // their ordered search. Caller retains each original work/cancel checkpoint.
    internal static int PrepareSmall(byte[] plane, int stride, int x, int y, int macroX, int macroY,
        Span<int> prepared)
    {
        if (prepared.Length < 13) throw new ArgumentException("Small prediction needs thirteen samples.", nameof(prepared));
        for (var i = 0; i < 8; i++)
            prepared[i] = i >= 4 && x == macroX + 12
                ? Sample(plane, stride, macroX + 12 + i, macroY - 1) : Sample(plane, stride, x + i, y - 1);
        for (var i = 0; i < 4; i++) prepared[8 + i] = Sample(plane, stride, x - 1, y + i);
        prepared[12] = Sample(plane, stride, x - 1, y - 1);
        return (prepared[0] + prepared[1] + prepared[2] + prepared[3]
            + prepared[8] + prepared[9] + prepared[10] + prepared[11] + 4) >> 3;
    }

    // Whole-block expansion of the original geometric SmallPixel formulas.
    // Averaged samples are bytes; only true motion needs an explicit clamp.
    internal static void SmallPrepared(byte[] plane, int stride, int x, int y, int mode,
        ReadOnlySpan<int> prepared, int dc)
    {
        if (prepared.Length < 13) throw new ArgumentException("Small prediction needs thirteen samples.", nameof(prepared));
        uint row0, row1, row2, row3;
        switch (mode)
        {
            case 0:
            {
                var v0 = dc;
                row0 = (uint)v0 * 0x01010101u;
                row1 = (uint)v0 * 0x01010101u;
                row2 = (uint)v0 * 0x01010101u;
                row3 = (uint)v0 * 0x01010101u;
                break;
            }
            case 1:
            {
                var v0 = Math.Clamp(prepared[0] + prepared[8] - prepared[12], 0, 255);
                var v1 = Math.Clamp(prepared[1] + prepared[8] - prepared[12], 0, 255);
                var v2 = Math.Clamp(prepared[2] + prepared[8] - prepared[12], 0, 255);
                var v3 = Math.Clamp(prepared[3] + prepared[8] - prepared[12], 0, 255);
                var v4 = Math.Clamp(prepared[0] + prepared[9] - prepared[12], 0, 255);
                var v5 = Math.Clamp(prepared[1] + prepared[9] - prepared[12], 0, 255);
                var v6 = Math.Clamp(prepared[2] + prepared[9] - prepared[12], 0, 255);
                var v7 = Math.Clamp(prepared[3] + prepared[9] - prepared[12], 0, 255);
                var v8 = Math.Clamp(prepared[0] + prepared[10] - prepared[12], 0, 255);
                var v9 = Math.Clamp(prepared[1] + prepared[10] - prepared[12], 0, 255);
                var v10 = Math.Clamp(prepared[2] + prepared[10] - prepared[12], 0, 255);
                var v11 = Math.Clamp(prepared[3] + prepared[10] - prepared[12], 0, 255);
                var v12 = Math.Clamp(prepared[0] + prepared[11] - prepared[12], 0, 255);
                var v13 = Math.Clamp(prepared[1] + prepared[11] - prepared[12], 0, 255);
                var v14 = Math.Clamp(prepared[2] + prepared[11] - prepared[12], 0, 255);
                var v15 = Math.Clamp(prepared[3] + prepared[11] - prepared[12], 0, 255);
                row0 = PackRow(v0, v1, v2, v3);
                row1 = PackRow(v4, v5, v6, v7);
                row2 = PackRow(v8, v9, v10, v11);
                row3 = PackRow(v12, v13, v14, v15);
                break;
            }
            case 2:
            {
                var v0 = Average3(prepared[12], prepared[0], prepared[1]);
                var v1 = Average3(prepared[0], prepared[1], prepared[2]);
                var v2 = Average3(prepared[1], prepared[2], prepared[3]);
                var v3 = Average3(prepared[2], prepared[3], prepared[4]);
                row0 = PackRow(v0, v1, v2, v3);
                row1 = PackRow(v0, v1, v2, v3);
                row2 = PackRow(v0, v1, v2, v3);
                row3 = PackRow(v0, v1, v2, v3);
                break;
            }
            case 3:
            {
                var v0 = Average3(prepared[12], prepared[8], prepared[9]);
                var v1 = Average3(prepared[8], prepared[9], prepared[10]);
                var v2 = Average3(prepared[9], prepared[10], prepared[11]);
                var v3 = Average3(prepared[10], prepared[11], prepared[11]);
                row0 = (uint)v0 * 0x01010101u;
                row1 = (uint)v1 * 0x01010101u;
                row2 = (uint)v2 * 0x01010101u;
                row3 = (uint)v3 * 0x01010101u;
                break;
            }
            case 4:
            {
                var v0 = Average3(prepared[0], prepared[1], prepared[2]);
                var v1 = Average3(prepared[1], prepared[2], prepared[3]);
                var v2 = Average3(prepared[2], prepared[3], prepared[4]);
                var v3 = Average3(prepared[3], prepared[4], prepared[5]);
                var v4 = Average3(prepared[4], prepared[5], prepared[6]);
                var v5 = Average3(prepared[5], prepared[6], prepared[7]);
                var v6 = Average3(prepared[6], prepared[7], prepared[7]);
                row0 = PackRow(v0, v1, v2, v3);
                row1 = PackRow(v1, v2, v3, v4);
                row2 = PackRow(v2, v3, v4, v5);
                row3 = PackRow(v3, v4, v5, v6);
                break;
            }
            case 5:
            {
                var v0 = Average3(prepared[11], prepared[10], prepared[9]);
                var v1 = Average3(prepared[10], prepared[9], prepared[8]);
                var v2 = Average3(prepared[9], prepared[8], prepared[12]);
                var v3 = Average3(prepared[8], prepared[12], prepared[0]);
                var v4 = Average3(prepared[12], prepared[0], prepared[1]);
                var v5 = Average3(prepared[0], prepared[1], prepared[2]);
                var v6 = Average3(prepared[1], prepared[2], prepared[3]);
                row0 = PackRow(v3, v4, v5, v6);
                row1 = PackRow(v2, v3, v4, v5);
                row2 = PackRow(v1, v2, v3, v4);
                row3 = PackRow(v0, v1, v2, v3);
                break;
            }
            case 6:
            {
                var v0 = Average3(prepared[10], prepared[9], prepared[8]);
                var v1 = Average3(prepared[9], prepared[8], prepared[12]);
                var v2 = Average3(prepared[8], prepared[12], prepared[0]);
                var v3 = Average2(prepared[12], prepared[0]);
                var v4 = Average3(prepared[12], prepared[0], prepared[1]);
                var v5 = Average2(prepared[0], prepared[1]);
                var v6 = Average3(prepared[0], prepared[1], prepared[2]);
                var v7 = Average2(prepared[1], prepared[2]);
                var v8 = Average3(prepared[1], prepared[2], prepared[3]);
                var v9 = Average2(prepared[2], prepared[3]);
                row0 = PackRow(v3, v5, v7, v9);
                row1 = PackRow(v2, v4, v6, v8);
                row2 = PackRow(v1, v3, v5, v7);
                row3 = PackRow(v0, v2, v4, v6);
                break;
            }
            case 7:
            {
                var v0 = Average2(prepared[0], prepared[1]);
                var v1 = Average3(prepared[0], prepared[1], prepared[2]);
                var v2 = Average2(prepared[1], prepared[2]);
                var v3 = Average3(prepared[1], prepared[2], prepared[3]);
                var v4 = Average2(prepared[2], prepared[3]);
                var v5 = Average3(prepared[2], prepared[3], prepared[4]);
                var v6 = Average2(prepared[3], prepared[4]);
                var v7 = Average3(prepared[3], prepared[4], prepared[5]);
                var v8 = Average3(prepared[4], prepared[5], prepared[6]);
                var v9 = Average3(prepared[5], prepared[6], prepared[7]);
                row0 = PackRow(v0, v2, v4, v6);
                row1 = PackRow(v1, v3, v5, v7);
                row2 = PackRow(v2, v4, v6, v8);
                row3 = PackRow(v3, v5, v7, v9);
                break;
            }
            case 8:
            {
                var v0 = Average3(prepared[0], prepared[1], prepared[2]);
                var v1 = Average3(prepared[12], prepared[0], prepared[1]);
                var v2 = Average3(prepared[8], prepared[12], prepared[0]);
                var v3 = Average2(prepared[8], prepared[12]);
                var v4 = Average3(prepared[9], prepared[8], prepared[12]);
                var v5 = Average2(prepared[9], prepared[8]);
                var v6 = Average3(prepared[10], prepared[9], prepared[8]);
                var v7 = Average2(prepared[10], prepared[9]);
                var v8 = Average3(prepared[11], prepared[10], prepared[9]);
                var v9 = Average2(prepared[11], prepared[10]);
                row0 = PackRow(v3, v2, v1, v0);
                row1 = PackRow(v5, v4, v3, v2);
                row2 = PackRow(v7, v6, v5, v4);
                row3 = PackRow(v9, v8, v7, v6);
                break;
            }
            case 9:
            {
                var v0 = Average2(prepared[8], prepared[9]);
                var v1 = Average3(prepared[8], prepared[9], prepared[10]);
                var v2 = Average2(prepared[9], prepared[10]);
                var v3 = Average3(prepared[9], prepared[10], prepared[11]);
                var v4 = Average2(prepared[10], prepared[11]);
                var v5 = Average3(prepared[10], prepared[11], prepared[11]);
                var v6 = prepared[11];
                row0 = PackRow(v0, v1, v2, v3);
                row1 = PackRow(v2, v3, v4, v5);
                row2 = PackRow(v4, v5, v6, v6);
                row3 = (uint)v6 * 0x01010101u;
                break;
            }
            default: throw new ImageDataException("Invalid VP8 subblock prediction.");
        }
        var offset = y * stride + x;
        BinaryPrimitives.WriteUInt32LittleEndian(plane.AsSpan(offset, 4), row0);
        BinaryPrimitives.WriteUInt32LittleEndian(plane.AsSpan(offset + stride, 4), row1);
        BinaryPrimitives.WriteUInt32LittleEndian(plane.AsSpan(offset + 2 * stride, 4), row2);
        BinaryPrimitives.WriteUInt32LittleEndian(plane.AsSpan(offset + 3 * stride, 4), row3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint PackRow(int a, int b, int c, int d)
        => (uint)a | (uint)b << 8 | (uint)c << 16 | (uint)d << 24;

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
