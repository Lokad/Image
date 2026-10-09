namespace Lokad.Image.Internal;

/// <summary>Owned bit-at-a-time interval encoder derived from RFC 6386 section 7.2.</summary>
internal sealed class Vp8BooleanWriter(OperationContext context, int maximum) : IDisposable
{
    private byte[] buffer = [];
    private int position, used, bottom, range = 255;
    private bool finished;

    public ReadOnlySpan<byte> Bytes => finished ? buffer.AsSpan(0, position) : throw new InvalidOperationException();

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
        while (range < 128)
        {
            Append(bottom >> 7); bottom = bottom * 2 & 255; range *= 2;
        }
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
            if (position == maximum || position == Array.MaxLength) throw new ImageLimitException("VP8 arithmetic partition exceeds its output limit.");
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
