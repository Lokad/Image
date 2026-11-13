namespace Lokad.Image.Internal;

/// <summary>Two idle exact-size arrays within one byte ceiling; live owners never share a lease.</summary>
internal sealed class PixelStoragePool
{
    public const int MaxRetainedBytes = 4 * 1024 * 1024;
    public static PixelStoragePool Shared { get; } = new();
    private readonly object gate = new();
    private byte[]? first, second;
    private readonly int maxRetainedBytes;

    public PixelStoragePool(int maxRetainedBytes = MaxRetainedBytes)
    {
        if (maxRetainedBytes < 0) throw new ArgumentOutOfRangeException(nameof(maxRetainedBytes));
        this.maxRetainedBytes = maxRetainedBytes;
    }

    // This storage is private until every result sample has been overwritten.
    // Exact lengths keep backing capacity equal to the admitted pixel bytes.
    public byte[] Rent(int length)
    {
        if (length <= maxRetainedBytes)
        {
            lock (gate)
            {
                if (first?.Length == length)
                {
                    var buffer = first; first = second; second = null; return buffer;
                }
                if (second?.Length == length)
                {
                    var buffer = second; second = null; return buffer;
                }
                // Preserve the original discard-on-size-miss policy.
                first = second = null;
            }
        }
        return GC.AllocateUninitializedArray<byte>(length);
    }

    public void Return(byte[] buffer)
    {
        if (buffer.Length <= maxRetainedBytes)
        {
            lock (gate)
            {
                if (ReferenceEquals(first, buffer) || ReferenceEquals(second, buffer)) return;
                if (first is null) first = buffer;
                else if (second is null && first.Length <= maxRetainedBytes - buffer.Length) second = buffer;
            }
        }
    }

    // Image pipelines can keep several differently sized owners alive at once.
    // Retain the larger released images when they fit in the same two slots;
    // a later size miss still leaves room for that request's ordinary return.
    public void ReturnLargest(byte[] buffer)
    {
        if (buffer.Length <= maxRetainedBytes)
        {
            lock (gate)
            {
                if (ReferenceEquals(first, buffer) || ReferenceEquals(second, buffer)) return;
                if (first is null) first = buffer;
                else if (second is null)
                {
                    if (first.Length <= maxRetainedBytes - buffer.Length) second = buffer;
                }
                else if (first.Length <= second.Length)
                {
                    if (buffer.Length > first.Length && second.Length <= maxRetainedBytes - buffer.Length) first = buffer;
                }
                else if (buffer.Length > second.Length && first.Length <= maxRetainedBytes - buffer.Length) second = buffer;
            }
        }
    }

    // Font loads may interleave with differently sized images. A miss must not
    // discard those idle image arrays, and a newly released font must be eligible
    // for reuse even when both slots previously held other sizes.
    public byte[] RentPreservingIdle(int length)
    {
        if (length <= maxRetainedBytes)
        {
            lock (gate)
            {
                if (first?.Length == length)
                {
                    var buffer = first; first = second; second = null; return buffer;
                }
                if (second?.Length == length)
                {
                    var buffer = second; second = null; return buffer;
                }
            }
        }
        return GC.AllocateUninitializedArray<byte>(length);
    }

    public void ReturnLatest(byte[] buffer)
    {
        if (buffer.Length <= maxRetainedBytes)
        {
            lock (gate)
            {
                if (ReferenceEquals(first, buffer) || ReferenceEquals(second, buffer)) return;
                second = first is not null && first.Length <= maxRetainedBytes - buffer.Length ? first : null;
                first = buffer;
            }
        }
    }

    // Cleared canvases and private full-overwrite images share storage, not
    // parsed/rendered state. On a miss keep one size only when it leaves both
    // capacity and a free slot for the requested image's return.
    public byte[] RentImage(int length, CancellationToken cancellation, bool clear)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        cancellation.ThrowIfCancellationRequested();
        byte[]? buffer = null;
        if (length <= maxRetainedBytes)
        {
            lock (gate)
            {
                if (first?.Length == length)
                {
                    buffer = first; first = second; second = null;
                }
                else if (second?.Length == length)
                {
                    buffer = second; second = null;
                }
                else
                {
                    second = null;
                    if (first is not null && first.Length > maxRetainedBytes - length) first = null;
                }
            }
        }
        // Cleared cold misses keep CLR initialization, including large buffers;
        // complete-overwrite owners can skip it without publishing stale samples.
        if (buffer is null) return clear ? new byte[length] : GC.AllocateUninitializedArray<byte>(length);
        if (!clear) return buffer;
        try
        {
            if (!cancellation.CanBeCanceled) buffer.AsSpan().Clear();
            else
            {
                for (var offset = 0; offset < buffer.Length;)
                {
                    cancellation.ThrowIfCancellationRequested();
                    var count = Math.Min(16384, buffer.Length - offset);
                    buffer.AsSpan(offset, count).Clear(); offset += count;
                }
                cancellation.ThrowIfCancellationRequested();
            }
            return buffer;
        }
        catch { Return(buffer); throw; }
    }
}
