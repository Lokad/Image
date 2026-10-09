namespace Lokad.Image.Internal;

/// <summary>VP8L least-significant-bit-first fields, bounded by the chunk payload.</summary>
internal ref struct WebpBitReader
{
    private readonly ReadOnlySpan<byte> data;
    private readonly OperationContext context;
    private int position, available;
    private ulong bits;

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
}
