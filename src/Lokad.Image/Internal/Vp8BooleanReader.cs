using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

/// <summary>Owned bit-at-a-time interval decoder, derived from RFC 6386 section 7.2.</summary>
internal partial struct Vp8BooleanReader
{
    private int position, end, shift, range, value;

    public Vp8BooleanReader(ReadOnlySpan<byte> data, int offset, int length)
    {
        if (offset < 0 || length < 1 || offset > data.Length - length)
            throw new ImageDataException("Truncated VP8 arithmetic partition.");
        value = data[offset]; range = 255; position = offset + 1; end = offset + length; shift = 7;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> data, int probability, OperationContext context)
    {
        context.Work(1);
        if ((uint)probability > 255) throw new ArgumentOutOfRangeException(nameof(probability));
        if (value >= range) throw new ImageDataException("Invalid VP8 arithmetic interval.");
        var boundary = 1 + ((range - 1) * probability >> 8);
        var result = value >= boundary ? 1 : 0;
        // The result is 0/1, so its negation selects either interval without a
        // data-dependent branch: left (boundary, value), right (range-boundary,
        // value-boundary). All valid interval arithmetic stays within +/-508.
        var mask = -result;
        value -= boundary & mask;
        range = boundary + ((range - 2 * boundary) & mask);
        while (range < 128)
        {
            if (position == end) throw new ImageDataException("Truncated VP8 arithmetic bits.");
            range *= 2; value = value * 2 + (data[position] >> shift & 1);
            if (--shift < 0) { shift = 7; position++; }
        }
        return result;
    }

    public int Literal(ReadOnlySpan<byte> data, int bits, OperationContext context)
    {
        if ((uint)bits > 24) throw new ArgumentOutOfRangeException(nameof(bits));
        var result = 0;
        for (var bit = 0; bit < bits; bit++) result = result * 2 + Read(data, 128, context);
        return result;
    }

    public int Tree(ReadOnlySpan<byte> data, ReadOnlySpan<sbyte> tree, ReadOnlySpan<byte> probabilities,
        OperationContext context, int root = 0)
    {
        var node = root;
        do { node = tree[node + Read(data, probabilities[node / 2], context)]; } while (node >= 0);
        return -node - 1;
    }

    /// <summary>Fixed coefficient-token decisions from RFC 6386 section 13.2.</summary>
    public int CoefficientToken(ReadOnlySpan<byte> data, ReadOnlySpan<byte> probabilities,
        OperationContext context, bool previousZero)
    {
        // A zero coefficient suppresses only the EOB decision. Every remaining
        // decision keeps the original probability access and arithmetic read order.
        if (!previousZero && Read(data, probabilities[0], context) == 0) return 11;
        if (Read(data, probabilities[1], context) == 0) return 0;
        if (Read(data, probabilities[2], context) == 0) return 1;
        if (Read(data, probabilities[3], context) == 0)
            return Read(data, probabilities[4], context) == 0 ? 2 : 3 + Read(data, probabilities[5], context);
        if (Read(data, probabilities[6], context) == 0) return 5 + Read(data, probabilities[7], context);
        var upper = Read(data, probabilities[8], context);
        return 7 + 2 * upper + Read(data, probabilities[9 + upper], context);
    }
}
