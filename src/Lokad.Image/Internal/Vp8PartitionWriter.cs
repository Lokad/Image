namespace Lokad.Image.Internal;

// Owned RFC6386 section7.2 interval stream. Normalization is expressed directly
// in bits; canonical termination is pinned by original native symbol controls.
// No implementation tables, native calls or buffers from the caller are retained.
internal sealed class Vp8PartitionWriter : IDisposable
{
    private readonly OperationContext context;
    private readonly int maximum;
    private byte[]? buffer;
    private int position, used, bottom, range = 255;
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
            bottom += boundary; range -= boundary;
            if (bottom >= 256)
            {
                bottom -= 256; var at = position - 1; var increment = used == 0 ? 1 : 1 << (8 - used);
                while (at >= 0)
                {
                    context.Cancellation.ThrowIfCancellationRequested(); var value = buffer![at] + increment; buffer[at] = unchecked((byte)value);
                    if (value < 256) break;
                    at--; increment = 1;
                }
                if (at < 0) throw new InvalidOperationException("Arithmetic interval escaped its initial range.");
            }
        }
        while (range < 128) { Append(bottom >> 7); bottom = bottom * 2 & 255; range *= 2; }
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
        // No emitted bits after an actual interval update is distinct from the
        // pristine empty stream. Do not blindly remove one lookahead byte.
        var target = position + 2 + (position == 0 && range != 255 ? 1 : 0);
        for (var p = 7; p >= 0; p--) Append(bottom >> p & 1);
        while (used != 0 || position < target) Append(0);
        finished = true;
    }

    private void Append(int bit)
    {
        if (used == 0)
        {
            if (position >= maximum || position == Array.MaxLength)
                throw new ImageLimitException(position >= maximum ? ImageLimitKind.OutputBytes : ImageLimitKind.BufferLength, "VP8 partition exceeds its output limit.");
            if (buffer is null || position == buffer.Length) Grow();
            buffer![position++] = 0;
        }
        buffer![position - 1] |= (byte)(bit << (7 - used)); used = (used + 1) & 7;
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
