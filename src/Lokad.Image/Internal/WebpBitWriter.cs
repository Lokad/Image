namespace Lokad.Image.Internal;

internal sealed class WebpBitWriter(Stream output, OperationContext context)
{
    private uint bits;
    private int available;
    internal int PendingBits => available;

    public void Write(int value, int count)
    {
        if ((uint)count > 24 || value < 0 || (uint)value >= 1u << count) throw new ArgumentOutOfRangeException(nameof(value));
        context.Cancellation.ThrowIfCancellationRequested(); bits |= (uint)value << available; available += count;
        while (available >= 8) { output.WriteByte((byte)bits); bits >>= 8; available -= 8; }
    }

    public void Finish()
    {
        if (available != 0) output.WriteByte((byte)bits);
        bits = 0; available = 0;
    }

    // Only the sealed discard sink can substitute a prepared bit count.
    // Real emission always visits the original tokens and writes their bits.
    internal bool TryCountBits(long count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (output is not WebpLosslessEncoder.CountStream counter) return false;
        context.Cancellation.ThrowIfCancellationRequested();
        var pending = available + (int)(count & 7);
        counter.Advance(count / 8 + (pending >> 3));
        available = pending & 7; bits = 0;
        return true;
    }
}
