using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

/// <summary>VP8L least-significant-bit-first fields, bounded by the chunk payload.</summary>
internal ref struct WebpBitReader
{
    private readonly ReadOnlySpan<byte> data;
    private readonly OperationContext context;
    private int position, available;
    private ulong bits;
    internal int Position => position;

    public WebpBitReader(ReadOnlySpan<byte> data, OperationContext context)
    {
        this.data = data; this.context = context; position = 0; available = 0; bits = 0;
    }

    public int Read(int count)
    {
        if ((uint)count > 24) throw new ArgumentOutOfRangeException(nameof(count));
        context.Work(1);
        while (available < count)
        {
            if (position == data.Length) throw new ImageDataException("Truncated VP8L bitstream.");
            bits |= (ulong)data[position++] << available; available += 8;
        }
        var value = (int)(bits & ((1ul << count) - 1));
        bits >>= count; available -= count; return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryReadHuffman(ReadOnlySpan<int> codes, out int symbol)
    {
        symbol = 0;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        var mask = codes.Length - 1;
        while (true)
        {
            var packed = codes[(int)candidateBits & mask]; var count = packed & 15;
            if (count != 0 && count <= candidateAvailable)
            {
                // Charge the same logical one-bit reads. A tight budget falls
                // back without mutation, retaining partial consumption and
                // limit-before-truncation precedence. Read no unnecessary byte.
                if (!context.TryWork(count)) return false;
                bits = candidateBits >> count; available = candidateAvailable - count; position = candidatePosition;
                symbol = packed >> 4; return true;
            }
            if (candidateAvailable >= 8 || (1 << candidateAvailable) >= codes.Length || candidatePosition == data.Length) return false;
            candidateBits |= (ulong)data[candidatePosition++] << candidateAvailable; candidateAvailable += 8;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryReadCanonical(ReadOnlySpan<int> firstCode, ReadOnlySpan<int> counts,
        ReadOnlySpan<int> firstSymbol, ReadOnlySpan<ushort> symbols, out int symbol)
    {
        symbol = 0;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        var code = 0;
        // RFC 9649 section 3.7: walk the canonical prefix using local bits.
        // No extra table, allocation or byte beyond the actual code is needed.
        for (var length = 1; length <= 15; length++)
        {
            if (candidateAvailable < length)
            {
                if (candidatePosition == data.Length) return false;
                candidateBits |= (ulong)data[candidatePosition++] << candidateAvailable; candidateAvailable += 8;
            }
            code = code * 2 + (int)(candidateBits >> (length - 1) & 1);
            var index = code - firstCode[length];
            if ((uint)index >= (uint)counts[length]) continue;
            // A partial budget or missing byte keeps the original per-bit path,
            // including consumed prefixes and limit-before-truncation order.
            if (!context.TryWork(length)) return false;
            bits = candidateBits >> length; available = candidateAvailable - length; position = candidatePosition;
            symbol = symbols[firstSymbol[length] + index]; return true;
        }
        return false;
    }
}
