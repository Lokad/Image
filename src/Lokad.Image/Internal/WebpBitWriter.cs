namespace Lokad.Image.Internal;

internal sealed class WebpBitWriter(Stream output, OperationContext context)
{
    private uint bits;
    private int available;

    public void Write(int value, int count)
    {
        if ((uint)count > 24 || value < 0 || (uint)value >= 1u << count) throw new ArgumentOutOfRangeException(nameof(value));
        context.Work(1); bits |= (uint)value << available; available += count;
        while (available >= 8) { output.WriteByte((byte)bits); bits >>= 8; available -= 8; }
    }

    public void Finish()
    {
        if (available != 0) output.WriteByte((byte)bits);
        bits = 0; available = 0;
    }
}
