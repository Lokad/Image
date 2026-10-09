namespace Lokad.Image.Internal;

/// <summary>Owned bit-at-a-time interval decoder, derived from RFC 6386 section 7.2.</summary>
internal struct Vp8BooleanReader
{
    private int position, end, shift, range, value;

    public Vp8BooleanReader(ReadOnlySpan<byte> data, int offset, int length)
    {
        if (offset < 0 || length < 1 || offset > data.Length - length)
            throw new ImageDataException("Truncated VP8 arithmetic partition.");
        value = data[offset]; range = 255; position = offset + 1; end = offset + length; shift = 7;
    }

    public int Read(ReadOnlySpan<byte> data, int probability, OperationContext context)
    {
        context.Work(1);
        if ((uint)probability > 255) throw new ArgumentOutOfRangeException(nameof(probability));
        if (value >= range) throw new ImageDataException("Invalid VP8 arithmetic interval.");
        var boundary = 1 + ((range - 1) * probability >> 8);
        var result = value >= boundary ? 1 : 0;
        if (result == 0) range = boundary;
        else { value -= boundary; range -= boundary; }
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
}
