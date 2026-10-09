namespace Lokad.Image.Internal;

/// <summary>MSB-first JPEG entropy bits with marker-aware byte unstuffing.</summary>
internal ref struct JpegEntropyReader
{
    private readonly ReadOnlySpan<byte> data;
    private readonly OperationContext context;
    private int position;
    private uint bits;
    private int available;
    public int Position => position;

    public JpegEntropyReader(ReadOnlySpan<byte> data, int start, OperationContext context)
    {
        if ((uint)start > (uint)data.Length) throw new ArgumentOutOfRangeException(nameof(start));
        this.data = data; this.context = context; position = start; bits = 0; available = 0;
    }

    public int ReadBits(int count)
    {
        if ((uint)count > 16) throw new ArgumentOutOfRangeException(nameof(count));
        context.Work(1);
        while (available < count)
        {
            if (position >= data.Length) throw new ImageDataException("Truncated JPEG entropy data.");
            var next = data[position++];
            if (next == 255)
            {
                if (position >= data.Length || data[position++] != 0)
                    throw new ImageDataException("Unexpected marker in JPEG entropy data.");
            }
            bits = bits << 8 | next;
            available += 8;
        }
        available -= count;
        return (int)((bits >> available) & ((1u << count) - 1));
    }

    public int ReadSigned(int count)
    {
        if (count == 0) return 0;
        var value = ReadBits(count);
        return value < 1 << (count - 1) ? value - ((1 << count) - 1) : value;
    }

    public void Restart(int expected)
    {
        if ((uint)expected > 7) throw new ArgumentOutOfRangeException(nameof(expected));
        Align();
        if (ReadMarker() != 0xD0 + expected) throw new ImageDataException("Unexpected JPEG restart marker sequence.");
    }

    public void Align()
    {
        // T.81 pads entropy segments with 1 bits. ReadBits never speculates past
        // the next required byte, so all remaining bits belong to that byte.
        if ((bits & ((1u << available) - 1)) != (1u << available) - 1)
            throw new ImageDataException("Invalid JPEG entropy padding.");
        bits = 0; available = 0;
    }

    private int ReadMarker()
    {
        if (position >= data.Length || data[position++] != 255) throw new ImageDataException("Missing JPEG restart marker.");
        while (position < data.Length && data[position] == 255) { context.Work(1); position++; }
        if (position >= data.Length) throw new ImageDataException("Truncated JPEG restart marker.");
        return data[position++];
    }
}
