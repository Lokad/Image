using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

/// <summary>Owned interval encoder derived from RFC 6386 section 7.2.</summary>
internal sealed class Vp8BooleanWriter(OperationContext context, int maximum) : IDisposable
{
    private byte[] buffer = [];
    private int position, used, bottom, range = 255;
    private bool finished;

    public ReadOnlySpan<byte> Bytes => finished ? buffer.AsSpan(0, position) : throw new InvalidOperationException();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int bit, int probability = 128)
    {
        if (finished) throw new InvalidOperationException("Arithmetic partition is finished.");
        if ((uint)bit > 1 || (uint)probability > 255) throw new ArgumentOutOfRangeException(nameof(bit));
        context.Work(1);
        var boundary = 1 + ((range - 1) * probability >> 8);
        if (bit == 0) range = boundary;
        else
        {
            bottom += boundary; range -= boundary;
            if (bottom >= 256) { bottom -= 256; Carry(); }
        }
        var shift = BitOperations.LeadingZeroCount((uint)range) - 24;
        if (shift == 0) return;
        var nextUsed = used + shift;
        if (used != 0 && nextUsed <= 8)
        {
            // RFC 6386 7.2 normalization emits at most seven bits. Pack them
            // together when no allocation/admission checkpoint is crossed.
            buffer[position - 1] |= (byte)((bottom >> (8 - shift)) << (8 - nextUsed));
            used = nextUsed & 7;
        }
        else if (position < buffer.Length)
        {
            var bits = bottom >> (8 - shift);
            if (used == 0) { buffer[position++] = (byte)(bits << (8 - shift)); used = shift; }
            else
            {
                var remaining = nextUsed - 8;
                buffer[position - 1] |= (byte)(bits >> remaining);
                buffer[position++] = (byte)(bits << (8 - remaining)); used = remaining;
            }
        }
        else
        {
            // Growth/output/work failures retain the original bit sequence,
            // including every partially written byte and interval update.
            while (range < 128)
            {
                Append(bottom >> 7); bottom = bottom * 2 & 255; range *= 2;
            }
            return;
        }
        bottom = bottom << shift & 255; range <<= shift;
    }

    public void Literal(int value, int count)
    {
        if ((uint)count > 24 || value < 0 || (uint)value >= 1u << count) throw new ArgumentOutOfRangeException(nameof(value));
        for (var bit = count - 1; bit >= 0; bit--) Write(value >> bit & 1);
    }

    public void Tree(ReadOnlySpan<sbyte> tree, ReadOnlySpan<byte> probabilities, int token, int root = 0)
    {
        // Small bounded format trees: discover the leaf path without an implementation code table.
        var path = 0; var depth = 0;
        if (!Find(tree, root, token, 0, 0, ref path, ref depth)) throw new ArgumentOutOfRangeException(nameof(token));
        var node = root;
        for (var bit = depth - 1; bit >= 0; bit--)
        {
            var branch = path >> bit & 1; Write(branch, probabilities[node / 2]); node = tree[node + branch];
        }
    }

    internal void Codeword(ulong path, ReadOnlySpan<byte> probabilities)
    {
        // Five bits per step: even tree-node index plus branch. The terminal
        // one is outside those steps; paths are derived from our format trees.
        while (path != 1)
        {
            var step = (int)(path & 31);
            Write(step & 1, probabilities[step >> 1]); path >>= 5;
        }
    }

    internal static ulong[] CompileTree(ReadOnlySpan<sbyte> tree, int leaves)
    {
        var paths = new ulong[leaves]; Compile(tree, paths, 0, 0, 0); return paths;
    }

    private static void Compile(ReadOnlySpan<sbyte> tree, Span<ulong> paths, int node, ulong path, int depth)
    {
        if (node < 0) { paths[-node - 1] = path | 1ul << (5 * depth); return; }
        if (node > 30 || depth >= 12) throw new InvalidOperationException("VP8 encoder tree exceeds its bounded codeword.");
        Compile(tree, paths, tree[node], path | (ulong)node << (5 * depth), depth + 1);
        Compile(tree, paths, tree[node + 1], path | (ulong)(node + 1) << (5 * depth), depth + 1);
    }

    private static bool Find(ReadOnlySpan<sbyte> tree, int node, int token, int path, int depth, ref int result, ref int count)
    {
        if (node < 0)
        {
            if (-node - 1 != token) return false;
            result = path; count = depth; return true;
        }
        return Find(tree, tree[node], token, path * 2, depth + 1, ref result, ref count)
            || Find(tree, tree[node + 1], token, path * 2 + 1, depth + 1, ref result, ref count);
    }

    public void Finish()
    {
        if (finished) throw new InvalidOperationException("Arithmetic partition is finished.");
        context.Cancellation.ThrowIfCancellationRequested();
        for (var bit = 7; bit >= 0; bit--) Append(bottom >> bit & 1);
        while (used != 0) Append(0);
        // Zero lookahead for decoders with a wider internal arithmetic window.
        for (var bit = 0; bit < 16; bit++) Append(0);
        finished = true;
    }

    private void Append(int bit)
    {
        if (used == 0)
        {
            if (position == maximum || position == Array.MaxLength) throw new ImageLimitException(position == maximum ? ImageLimitKind.OutputBytes : ImageLimitKind.BufferLength, "VP8 arithmetic partition exceeds its output limit.");
            if (position == buffer.Length) Grow();
            buffer[position++] = 0;
        }
        buffer[position - 1] |= (byte)(bit << (7 - used)); used = (used + 1) & 7;
    }

    private void Carry()
    {
        var index = position - 1; var increment = used == 0 ? 1 : 1 << (8 - used);
        while (index >= 0)
        {
            context.Work(1);
            var sum = buffer[index] + increment; buffer[index] = unchecked((byte)sum);
            if (sum < 256) return;
            index--; increment = 1;
        }
        throw new InvalidOperationException("Arithmetic interval escaped the initial range.");
    }

    private void Grow()
    {
        var size = (int)Math.Min(Math.Min(maximum, Array.MaxLength), Math.Max(64L, buffer.Length * 2L));
        context.Reserve(size + 32L);
        var next = new byte[size]; context.Work(position); buffer.AsSpan(0, position).CopyTo(next);
        if (buffer.Length != 0) context.Release(buffer.Length + 32L);
        buffer = next;
    }

    public void Dispose()
    {
        if (buffer.Length != 0) context.Release(buffer.Length + 32L);
        buffer = []; position = 0; finished = true;
    }
}
