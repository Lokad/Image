using System.Numerics;
using System.Runtime.CompilerServices;

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

    internal void WriteFixedAc(ReadOnlySpan<int> quantized, ulong occupied, uint[] words)
    {
        // The caller has constructed the original scan mask after writing DC.
        // Keep AC state local, but retain each field's work/cancellation check
        // and every byte callback, including the exact state on a failed write.
        var cache = bits; var pending = available;
        var destination = output; var limited = destination as LimitedWriteStream;
        var work = context;
        try
        {
            var previous = 0;
            while (occupied != 0)
            {
                var index = BitOperations.TrailingZeroCount(occupied);
                var zeros = index - previous - 1;
                while (zeros >= 16)
                {
                    var zeroRun = words[0xF0];
                    WriteFixedField(destination, limited, work, ref cache, ref pending, zeroRun & 65535, (int)(zeroRun >> 16));
                    zeros -= 16;
                }
                var value = quantized[JpegTransform.Zigzag[index]];
                var category = value == 0 ? 0 : 32 - BitOperations.LeadingZeroCount((uint)Math.Abs(value));
                if (category > 10) throw new ImageDataException("JPEG AC transform exceeds its coding range.");
                var word = words[zeros << 4 | category];
                WriteFixedField(destination, limited, work, ref cache, ref pending, word & 65535, (int)(word >> 16));
                // Category was derived from this same value and admitted above:
                // its signed amplitude necessarily fits the original 0..16-bit
                // range check. General Write/Signed entry points keep that check.
                var amplitude = value < 0 ? value + ((1 << category) - 1) : value;
                WriteFixedField(destination, limited, work, ref cache, ref pending, (uint)amplitude, category);
                previous = index; occupied &= occupied - 1;
            }
            if (previous != 63)
            {
                var end = words[0];
                WriteFixedField(destination, limited, work, ref cache, ref pending, end & 65535, (int)(end >> 16));
            }
        }
        finally { bits = cache; available = pending; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteFixedField(Stream destination, LimitedWriteStream? limited, OperationContext context,
        ref uint cache, ref int pending, uint value, int count)
    {
        context.Work(1);
        cache = cache << count | value; pending += count;
        while (pending >= 8)
        {
            pending -= 8;
            var next = (byte)(cache >> pending);
            if (limited is not null) limited.WriteByte(next); else destination.WriteByte(next);
            if (next == 255)
            {
                if (limited is not null) limited.WriteByte(0); else destination.WriteByte(0);
            }
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
