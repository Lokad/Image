using System.Buffers.Binary;
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

    // Trusted complete eight-bit channel trees. On refusal the caller retains
    // the original three reads, including every partial bit/work/EOF failure.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryReadByteChannels(ReadOnlySpan<int> red,ReadOnlySpan<int> blue,
        ReadOnlySpan<int> alpha,out uint channels)
    {
        channels=0;
        if(context.Cancellation.CanBeCanceled)return false;
        var candidateBits=bits;var candidateAvailable=available;var candidatePosition=position;
        if(candidateAvailable<=7 && data.Length-candidatePosition>=3)
        {
            // Exactly the three bytes required by these codes; never read a
            // fourth byte for a wider load near the end of the chunk.
            candidateBits|=(ulong)BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(candidatePosition,2))<<candidateAvailable;
            candidateBits|=(ulong)data[candidatePosition+2]<<(candidateAvailable+16);
            candidatePosition+=3;candidateAvailable+=24;
        }
        else while(candidateAvailable<24)
        {
            if(candidatePosition==data.Length)return false;
            candidateBits|=(ulong)data[candidatePosition++]<<candidateAvailable;candidateAvailable+=8;
        }
        if(!context.TryWork(24))return false;
        var r=red[(int)candidateBits&255]>>4;
        var b=blue[(int)(candidateBits>>8)&255]>>4;
        var a=alpha[(int)(candidateBits>>16)&255]>>4;
        channels=(uint)(a<<24|r<<16|b);
        bits=candidateBits>>24;available=candidateAvailable-24;position=candidatePosition;
        return true;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryReadWideGreen(ReadOnlySpan<uint> codes, out int symbol)
    {
        symbol = 0;
        var candidateBits = bits; var candidateAvailable = available; var candidatePosition = position;
        while (true)
        {
            var packed = codes[(int)candidateBits & 4095]; var count = (int)(packed & 15);
            if (count != 0 && count <= candidateAvailable)
            {
                if (!context.TryWork(count)) return false;
                bits = candidateBits >> count; available = candidateAvailable - count; position = candidatePosition;
                symbol = (int)(packed >> 4); return true;
            }
            if (candidateAvailable >= 12 || candidatePosition == data.Length) return false;
            candidateBits |= (ulong)data[candidatePosition++] << candidateAvailable; candidateAvailable += 8;
        }
    }
}
