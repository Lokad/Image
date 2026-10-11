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
}
