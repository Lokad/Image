namespace Lokad.Image.Internal;

internal sealed class JpegEntropyWriter(Stream output, OperationContext context, long admittedPacketBytes = 0)
{
    private uint bits;
    private int available;
    private readonly LimitedWriteStream? admitted = output is LimitedWriteStream limited
        && limited.SupportsAdmittedJpegBytes(admittedPacketBytes) ? limited : null;

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

    // Only immutable encoder tables supply these prevalidated code/length
    // words. Dynamic amplitudes retain Write's argument checks. Keep each
    // logical work charge and byte callback at its original position.
    internal void WriteCodeword(uint word)
    {
        context.Work(1);
        var count = (int)(word >> 16);
        bits = bits << count | (word & 65535); available += count;
        while (available >= 8)
        {
            available -= 8;
            var next = (byte)(bits >> available);
            output.WriteByte(next);
            if (next == 255) output.WriteByte(0);
        }
    }

    public void Align()
    {
        if (available != 0) Write((1 << (8 - available)) - 1, 8 - available);
        bits = 0; available = 0;
    }

    public void Restart(int index)
    {
        Align(); output.WriteByte(255); output.WriteByte((byte)(0xD0 + (index & 7)));
    }

    internal void WriteAdmitted(int value, int count)
    {
        if (admitted is null) { Write(value, count); return; }
        if (count is < 0 or > 16 || value < 0 || value >= 1 << count) throw new ArgumentOutOfRangeException(nameof(value));
        context.Work(1);
        bits = bits << count | (uint)value; available += count;
        while (available >= 8)
        {
            available -= 8;
            var next = (byte)(bits >> available);
            admitted.WriteAdmittedJpegByte(next);
            if (next == 255) admitted.WriteAdmittedJpegByte(0);
        }
    }

    internal void AlignAdmitted()
    {
        if (available != 0) WriteAdmitted((1 << (8 - available)) - 1, 8 - available);
        bits = 0; available = 0;
    }

    internal void RestartAdmitted(int index)
    {
        if (admitted is null) { Restart(index); return; }
        AlignAdmitted(); admitted.WriteAdmittedJpegByte(255); admitted.WriteAdmittedJpegByte((byte)(0xD0 + (index & 7)));
    }
}
