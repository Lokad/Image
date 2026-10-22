namespace Lokad.Image.Internal;

internal sealed class JpegEntropyWriter(Stream output, OperationContext context)
{
    private uint bits;
    private int available;

    public void Write(int value, int count)
    {
        if (count is < 0 or > 16 || value < 0 || value >= 1 << count) throw new ArgumentOutOfRangeException(nameof(value));
        context.Work(1);
        bits = bits << count | (uint)value; available += count;
        while (available >= 8)
        {
            available -= 8;
            var next = (byte)(bits >> available);
            output.WriteByte(next);
            if (next == 255) output.WriteByte(0);
        }
    }

    public void Signed(int value, int count)
        => Write(value < 0 ? value + ((1 << count) - 1) : value, count);

    public void Align()
    {
        if (available != 0) Write((1 << (8 - available)) - 1, 8 - available);
        bits = 0; available = 0;
    }

    public void Restart(int index)
    {
        Align(); output.WriteByte(255); output.WriteByte((byte)(0xD0 + (index & 7)));
    }

}
