using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

// Encoder search/bit-cost order, distinct from the shared geometric formula
// indices for the three diagonal modes. Keep search ties in this explicit order.
internal enum Vp8SmallIntraMode : byte
{
    Dc, TrueMotion, Vertical, Horizontal, DownRight, VerticalRight, DownLeft,
    VerticalLeft, HorizontalDown, HorizontalUp
}

/// <summary>Owned geometric formulation of RFC 6386 section 12 intra prediction.</summary>
internal static class Vp8Prediction
{
    // Pure borrowed4x4 entry for the prepared encoder. Caller supplies eight top
    // and four left samples (including defaults/edge replication). Snapshot all
    // neighbors before writes so the output can alias either border. The outer
    // encoder admits block cancellation and coarse workspace; no heap/cache/scope.
    internal static void SmallBlock(Vp8SmallIntraMode mode, ReadOnlySpan<byte> topSamples,
        ReadOnlySpan<byte> leftSamples, byte corner, Span<byte> destination)
    {
        if ((uint)mode > 9) throw new ArgumentOutOfRangeException(nameof(mode));
        if (topSamples.Length < 8 || leftSamples.Length < 4 || destination.Length < 16)
            throw new ArgumentException("Small prediction requires eight top samples, four left samples and sixteen output pixels.");
        Span<int> neighbors = stackalloc int[13];
        for (var p = 0; p < 8; p++) neighbors[p] = topSamples[p];
        for (var p = 0; p < 4; p++) neighbors[8 + p] = leftSamples[p];
        neighbors[12] = corner;
        var dc = (neighbors[0] + neighbors[1] + neighbors[2] + neighbors[3]
            + neighbors[8] + neighbors[9] + neighbors[10] + neighbors[11] + 4) >> 3;
        // Shared geometry indexes LD/RD/VR as4/5/6; encoder visits RD/VR/LD.
        var geometry = mode switch
        {
            Vp8SmallIntraMode.DownRight => 5,
            Vp8SmallIntraMode.VerticalRight => 6,
            Vp8SmallIntraMode.DownLeft => 4,
            _ => (int)mode
        };
        SmallPrepared(destination, 4, 0, 0, geometry, neighbors, dc);
    }

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

    // RFC 6386 section 12: encoder candidates overwrite only this block;
    // every top/left/corner sample lies outside it and is immutable throughout
    // their ordered search. Caller retains each original cancellation checkpoint.
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

    // Shared whole-row expansion of RFC6386 section12 geometric prediction.
    // Averaged samples are bytes; only true motion needs an explicit clamp.
    internal static void SmallPrepared(Span<byte> plane, int stride, int x, int y, int mode,
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
        BinaryPrimitives.WriteUInt32LittleEndian(plane.Slice(offset, 4), row0);
        BinaryPrimitives.WriteUInt32LittleEndian(plane.Slice(offset + stride, 4), row1);
        BinaryPrimitives.WriteUInt32LittleEndian(plane.Slice(offset + 2 * stride, 4), row2);
        BinaryPrimitives.WriteUInt32LittleEndian(plane.Slice(offset + 3 * stride, 4), row3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint PackRow(int a, int b, int c, int d)
        => (uint)a | (uint)b << 8 | (uint)c << 16 | (uint)d << 24;

    private static int Average2(int a, int b) => (a + b + 1) >> 1;
    private static int Average3(int a, int b, int c) => (a + 2 * b + c + 2) >> 2;
}
