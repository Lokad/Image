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
        // T.81 F.2.2.1: a missing leading one subtracts 2^count - 1.
        // ReadBits admits only 1..16 here, so the difference fits Int32;
        // its sign gives an exact mask without a data-dependent branch.
        var threshold = 1 << (count - 1);
        return value + (((value - threshold) >> 31) & (1 - (threshold << 1)));
    }

    internal bool TryReadHuffman(ReadOnlySpan<ushort> shortCodes, out int symbol)
    {
        symbol = 0;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        // Complete cached short codes need no additional input inspection.
        var prefix = (int)((bits & ((1u << available) - 1)) << (8 - available));
        var packed = shortCodes[prefix]; var count = packed & 15;
        if (count == 0 || count > available)
        {
            // Inspect at most one unstuffed byte without changing reader state
            // or raising format errors. Markers/truncation use the scalar path.
            if (candidatePosition >= data.Length) return false;
            var next = data[candidatePosition++];
            if (next == 255 && (candidatePosition >= data.Length || data[candidatePosition++] != 0)) return false;
            candidateBits = candidateBits << 8 | next; candidateAvailable += 8;
            prefix = (int)((candidateBits >> (candidateAvailable - 8)) & 255);
            packed = shortCodes[prefix]; count = packed & 15;
            if (count == 0) return false;
        }
        // Preserve exact logical work. Near the limit, fall back without any
        // mutation to retain the original per-bit failure position/precedence.
        if (!context.TryWork(count)) return false;
        bits = candidateBits; available = candidateAvailable - count; position = candidatePosition;
        symbol = packed >> 4; return true;
    }

    internal bool TryReadLongHuffman(ReadOnlySpan<ushort> longCodes, out int symbol)
    {
        symbol = 0;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        while (true)
        {
            var prefix = candidateAvailable >= 10 ? (int)((candidateBits >> (candidateAvailable - 10)) & 1023)
                : (int)((candidateBits & ((1u << candidateAvailable) - 1)) << (10 - candidateAvailable));
            var packed = longCodes[prefix]; var count = packed & 15;
            if (count != 0 && count <= candidateAvailable)
            {
                if (!context.TryWork(count)) return false;
                bits = candidateBits; available = candidateAvailable - count; position = candidatePosition;
                symbol = packed >> 4; return true;
            }
            if (candidateAvailable >= 10 || candidatePosition >= data.Length) return false;
            // Inspect only while the complete code is unavailable. Never
            // commit bytes beyond its last required byte, even at padding.
            var next = data[candidatePosition++];
            if (next == 255 && (candidatePosition >= data.Length || data[candidatePosition++] != 0)) return false;
            candidateBits = candidateBits << 8 | next; candidateAvailable += 8;
        }
    }

    internal bool TryReadCanonicalPrefix(ReadOnlySpan<ushort> shortCodes, out int prefix)
    {
        prefix = 0;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        if (candidateAvailable < 8)
        {
            if (candidatePosition >= data.Length) return false;
            var next = data[candidatePosition++];
            if (next == 255 && (candidatePosition >= data.Length || data[candidatePosition++] != 0)) return false;
            candidateBits = candidateBits << 8 | next; candidateAvailable += 8;
        }
        var value = (int)((candidateBits >> (candidateAvailable - 8)) & 255);
        // T.81 C.2: a complete prefix with no assigned code of length <=8
        // can start canonical traversal at length nine. Admit the same eight
        // logical bits together; tight work budgets, markers and truncation
        // retain the original one-bit traversal without any state mutation.
        if (shortCodes[value] != 0 || !context.TryWork(8)) return false;
        bits = candidateBits; available = candidateAvailable - 8; position = candidatePosition;
        prefix = value; return true;
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
