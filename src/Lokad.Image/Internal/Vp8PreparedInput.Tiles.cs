using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal sealed partial class Vp8PreparedInput
{
    // A detached eight-byte value. Prepare the four samples and the EXACT
    // truncated reciprocal once per2x2 cell, then reuse across RGB channels.
    internal readonly struct AlphaWeights
    {
        private readonly uint packed;
        private readonly int reciprocal;

        internal AlphaWeights(byte a, byte b, byte c, byte d)
        {
            packed = a | (uint)b << 8 | (uint)c << 16 | (uint)d << 24;
            var sum = a + b + c + d;
            reciprocal = sum is 0 or 1020 ? 0 : (1 << 19) / sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int Combine(int a, int b, int c, int d)
        {
            // Fully invisible cells intentionally keep the unweighted source
            // color, just like opaque cells. Cleanup is a later, separate step.
            if (reciprocal == 0) return a + b + c + d;
            var sum = (long)a * (packed & 255) + (long)b * (packed >> 8 & 255)
                + (long)c * (packed >> 16 & 255) + (long)d * (packed >> 24);
            return (int)((sum * reciprocal) >> 17);
        }
    }

    // Borrows validated source rows and the caller's admitted128-byte workspace.
    // It owns no storage, scope or reservation and cannot escape synchronous use.
    // The same scheduling/normalization serves opaque and weighted execution.
    private ref struct InputTiles
    {
        private readonly ReadOnlySpan<byte> bytes;
        private readonly ReadOnlySpan<ushort> words;
        private readonly Span<byte> normalized;
        private readonly bool eightBit;
        private readonly int width, height;
        internal int PreparedColumns { get; }
        internal int X { get; private set; }
        internal int Y { get; private set; }
        internal int BottomY { get; private set; }
        internal ReadOnlySpan<byte> Top { get; private set; }
        internal ReadOnlySpan<byte> Bottom { get; private set; }

        internal InputTiles(ReadOnlySpan<byte> bytes, ReadOnlySpan<ushort> words, bool eightBit,
            int width, int height, Span<byte> normalized)
        {
            if (normalized.Length < 128) throw new ArgumentException("Tile workspace is short.", nameof(normalized));
            this.bytes = bytes; this.words = words; this.eightBit = eightBit;
            this.width = width; this.height = height; this.normalized = normalized;
            PreparedColumns = width / 16 * 16; X = -16; Y = BottomY = 0; Top = Bottom = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool MoveNext(OperationContext context)
        {
            X += 16;
            if (X >= PreparedColumns) { X = 0; Y += 2; }
            if (Y >= height || PreparedColumns == 0) return false;
            if (X == 0) context.Cancellation.ThrowIfCancellationRequested();
            BottomY = Math.Min(Y + 1, height - 1);
            var topAt = (Y * width + X) * 4; var bottomAt = (BottomY * width + X) * 4;
            if (eightBit) { Top = bytes.Slice(topAt, 64); Bottom = bytes.Slice(bottomAt, 64); }
            else
            {
                Normalize16(words.Slice(topAt, 64), normalized[..64]);
                if (BottomY != Y) Normalize16(words.Slice(bottomAt, 64), normalized[64..]);
                Top = normalized[..64]; Bottom = BottomY == Y ? Top : normalized[64..];
            }
            return true;
        }
    }

    private static int PrepareAlphaTiles(ReadOnlySpan<byte> bytes, ReadOnlySpan<ushort> words, bool eightBit,
        int width, int height, Span<byte> yPlane, Span<byte> uPlane, Span<byte> vPlane, Span<byte> alphaPlane,
        int stride, ReadOnlySpan<ushort> linear, ReadOnlySpan<int> knots, OperationContext context)
    {
        Span<byte> normalized = stackalloc byte[128];
        var tiles = new InputTiles(bytes, words, eightBit, width, height, normalized);
        while (tiles.MoveNext(context))
        {
            var top = tiles.Top; var bottom = tiles.Bottom; var x = tiles.X; var y = tiles.Y;
            PrepareLuma8(top, yPlane.Slice(y * stride + x, 16));
            if (tiles.BottomY != y) PrepareLuma8(bottom, yPlane.Slice(tiles.BottomY * stride + x, 16));
            for (var i = 0; i < 16; i++)
            {
                alphaPlane[y * width + x + i] = top[i * 4 + 3];
                if (tiles.BottomY != y) alphaPlane[tiles.BottomY * width + x + i] = bottom[i * 4 + 3];
            }
            for (var i = 0; i < 8; i++)
            {
                var a = i * 8; var b = a + 4;
                var weights = new AlphaWeights(top[a + 3], top[b + 3], bottom[a + 3], bottom[b + 3]);
                var r = ReduceTile(top, bottom, a, b, 0, weights, linear, knots);
                var g = ReduceTile(top, bottom, a, b, 1, weights, linear, knots);
                var colorB = ReduceTile(top, bottom, a, b, 2, weights, linear, knots);
                var at = y / 2 * (stride / 2) + x / 2 + i;
                uPlane[at] = (byte)Math.Clamp((Ur * r + Ug * g + (ChromaScale / 2) * colorB + (128 << 18) + (1 << 17)) >> 18, 0, 255);
                vPlane[at] = (byte)Math.Clamp((Vr * r + Vg * g + Vb * colorB + (128 << 18) + (1 << 17)) >> 18, 0, 255);
            }
        }
        return tiles.PreparedColumns;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ReduceTile(ReadOnlySpan<byte> top, ReadOnlySpan<byte> bottom, int a, int b, int channel,
        AlphaWeights weights, ReadOnlySpan<ushort> linear, ReadOnlySpan<int> knots)
        => Compress(weights.Combine(linear[top[a + channel]], linear[top[b + channel]],
            linear[bottom[a + channel]], linear[bottom[b + channel]]), knots);
}
