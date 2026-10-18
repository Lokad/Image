using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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
        // The encoder's admitted8-KiB fixed workspace also covers these256
        // stack bytes. Prepare only integer fields; defer every range/overflow
        // error until its original scan position after preceding zero runs.
        Span<uint> fields = stackalloc uint[64];
        var prepared = Avx512F.IsSupported && Avx512CD.IsSupported && quantized.Length >= 64;
        if (prepared)
            for (var chunk = 0; chunk < 4; chunk++)
            {
                var values = Vector512.Create(quantized.Slice(chunk * 16, 16));
                var categories = Vector512.Subtract(Vector512.Create(32u),
                    Avx512CD.LeadingZeroCount(Vector512.Abs(values).AsUInt32()));
                var complements = Vector512.Subtract(Avx512F.ShiftLeftLogicalVariable(Vector512.Create(1u), categories), Vector512.Create(1u));
                var amplitudes = Vector512.Add(values.AsUInt32(), Vector512.BitwiseAnd(complements,
                    Vector512.LessThan(values, Vector512<int>.Zero).AsUInt32()));
                Vector512.BitwiseOr(Vector512.ShiftLeft(categories, 26),
                    Vector512.BitwiseAnd(amplitudes, Vector512.Create(65535u))).CopyTo(fields.Slice(chunk * 16, 16));
            }
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
                var natural = JpegTransform.Zigzag[index];
                var value = quantized[natural];
                var field = prepared ? fields[natural] : 0;
                var category = prepared ? (int)(field >> 26)
                    : value == 0 ? 0 : 32 - BitOperations.LeadingZeroCount((uint)Math.Abs(value));
                if (category > 10)
                {
                    _ = Math.Abs(value); // Preserve Int32.MinValue's original overflow.
                    throw new ImageDataException("JPEG AC transform exceeds its coding range.");
                }
                var word = words[zeros << 4 | category];
                WriteFixedField(destination, limited, work, ref cache, ref pending, word & 65535, (int)(word >> 16));
                // Category was derived from this same value and admitted above:
                // its signed amplitude necessarily fits the original 0..16-bit
                // range check. General Write/Signed entry points keep that check.
                var amplitude = prepared ? (int)(field & 65535) : value < 0 ? value + ((1 << category) - 1) : value;
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
