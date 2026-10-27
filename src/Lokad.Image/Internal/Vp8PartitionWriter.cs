using System.Buffers.Binary;
using System.Numerics;

namespace Lokad.Image.Internal;

// Owned RFC6386 section7.2 interval stream. A bounded word retains pending
// arithmetic bits; canonical termination is pinned by original native controls.
// No implementation tables, native calls or buffers from the caller are retained.
internal sealed class Vp8PartitionWriter : IDisposable
{
    private readonly OperationContext context;
    private readonly int maximum;
    private byte[]? buffer;
    private int position, pending, range = 255;
    private ulong value;
    private long capacity;
    private bool finished, disposed;
    internal long CapacityBytes => capacity;
    internal ReadOnlyMemory<byte> Memory => !disposed && finished ? buffer!.AsMemory(0, position) : throw new InvalidOperationException("Partition is not available.");
    internal ReadOnlySpan<byte> Bytes => Memory.Span;

    internal Vp8PartitionWriter(OperationContext context, int maximum)
    {
        if (maximum < 0) throw new ArgumentOutOfRangeException(nameof(maximum));
        this.context = context; this.maximum = maximum;
    }

    internal void Write(int bit, int probability = 128)
    {
        if (finished || disposed) throw new InvalidOperationException("Partition is closed.");
        if ((uint)bit > 1 || (uint)probability > 255) throw new ArgumentOutOfRangeException(nameof(bit));
        context.Cancellation.ThrowIfCancellationRequested(); var boundary = 1 + ((range - 1) * probability >> 8);
        if (bit == 0) range = boundary;
        else
        {
            value += (uint)boundary; range -= boundary;
        }
        if (range < 128)
        {
            var shift = BitOperations.LeadingZeroCount((uint)range) - 24;
            value <<= shift; range <<= shift; pending += shift;
            // Admit capacity at the original logical byte boundary, even when
            // its bits are still held in the word. Replacements keep overlap.
            Ensure(position + ((pending + 7) >> 3));
            if (pending >= 32)
            {
                Carry();
                BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(position), (uint)(value >> (pending - 24)));
                position += 4; pending -= 32;
                value &= (1UL << (pending + 8)) - 1;
            }
        }
    }

    internal void Literal(int value, int count)
    {
        if ((uint)count > 31 || value < 0 || (uint)value >= 1u << count) throw new ArgumentOutOfRangeException(nameof(value));
        for (var p = count - 1; p >= 0; p--) Write(value >> p & 1);
    }

    internal void Finish()
    {
        if (finished || disposed) throw new InvalidOperationException("Partition is closed.");
        context.Cancellation.ThrowIfCancellationRequested();
        var logical = position + ((pending + 7) >> 3);
        var target = (long)logical + 2 + (logical == 0 && range != 255 ? 1 : 0);
        Carry();
        // Publish the pending prefix, its eight residual bits, then the same
        // zero padding/lookahead as the bit-by-bit canonical termination.
        var count = pending + 8; var padding = -count & 7;
        value <<= padding; count += padding;
        while (count != 0)
        {
            Ensure((long)position + 1); count -= 8;
            buffer![position++] = unchecked((byte)(value >> count));
        }
        while (position < target) { Ensure((long)position + 1); buffer![position++] = 0; }
        pending = 0;
        finished = true;
    }

    private void Carry()
    {
        // At most one carry can escape a pending prefix: the selected interval
        // stays within its previous interval. With pending<=38, the word needs
        // at most47bits, including that carry and the eight residual bits.
        var width = pending + 8;
        if ((value >> width) == 0) return;
        var at = position - 1;
        while (at >= 0)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var incremented = buffer![at] + 1; buffer[at] = unchecked((byte)incremented);
            if (incremented < 256) break;
            at--;
        }
        if (at < 0) throw new InvalidOperationException("Arithmetic interval escaped its initial range.");
        value &= (1UL << width) - 1;
    }

    private void Ensure(long required)
    {
        if (required > maximum || required > Array.MaxLength)
            throw new ImageLimitException(required > maximum ? ImageLimitKind.OutputBytes : ImageLimitKind.BufferLength, "VP8 partition exceeds its output limit.");
        if (buffer is null || required > buffer.Length) Grow();
    }

    private void Grow()
    {
        var size = (int)Math.Min(Math.Min(maximum, Array.MaxLength), Math.Max(64L, (buffer?.Length ?? 0) * 2L));
        var nextCapacity = size + 128L; context.Reserve(nextCapacity); byte[] next;
        try { context.Cancellation.ThrowIfCancellationRequested(); next = new byte[size]; context.Cancellation.ThrowIfCancellationRequested(); buffer.AsSpan(0, position).CopyTo(next); }
        catch { context.Release(nextCapacity); throw; }
        var previous = capacity; buffer = next; capacity = nextCapacity;
        if (previous != 0) context.Release(previous);
    }

    public void Dispose()
    {
        disposed = true; Interlocked.Exchange(ref buffer, null);
        var bytes = Interlocked.Exchange(ref capacity, 0); if (bytes != 0) context.Release(bytes);
    }
}
