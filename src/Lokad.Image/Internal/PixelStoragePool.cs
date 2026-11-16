namespace Lokad.Image.Internal;

/// <summary>Bounded idle exact-size arrays within one byte ceiling; live owners never share storage.</summary>
internal sealed class PixelStoragePool
{
    public const int MaxRetainedBytes = 4 * 1024 * 1024;
    public static PixelStoragePool Shared { get; } = new(retainedArrays: 4);
    private readonly object gate = new();
    private readonly byte[]?[] idle;
    private readonly int maxRetainedBytes;

    public PixelStoragePool(int maxRetainedBytes = MaxRetainedBytes, int retainedArrays = 2)
    {
        if (maxRetainedBytes < 0) throw new ArgumentOutOfRangeException(nameof(maxRetainedBytes));
        if (retainedArrays is not (2 or 4)) throw new ArgumentOutOfRangeException(nameof(retainedArrays));
        this.maxRetainedBytes = maxRetainedBytes;
        // Codec-local pools keep their original two-slot policy. Shared image/font
        // storage can retain four simultaneous pipeline sizes under the same cap.
        idle = new byte[retainedArrays][];
    }

    // Called under gate only. Removing a slot transfers its unique live owner.
    private byte[]? Take(int length)
    {
        for (var i = 0; i < idle.Length; i++)
        {
            if (idle[i]?.Length != length) continue;
            var result = idle[i];
            for (var j = i; j + 1 < idle.Length; j++) idle[j] = idle[j + 1];
            idle[^1] = null;
            return result;
        }
        return null;
    }

    public byte[] Rent(int length)
    {
        if (length <= maxRetainedBytes)
        {
            lock (gate)
            {
                var buffer = Take(length); if (buffer is not null) return buffer;
                // Preserve discard-on-size-miss behavior for this caller policy.
                Array.Clear(idle);
            }
        }
        return GC.AllocateUninitializedArray<byte>(length);
    }

    public void Return(byte[] buffer) => Retain(buffer, preferLargest: false);
    public void ReturnLargest(byte[] buffer) => Retain(buffer, preferLargest: true);

    private void Retain(byte[] buffer, bool preferLargest)
    {
        if (buffer.Length > maxRetainedBytes) return;
        lock (gate)
        {
            var retained = 0; var empty = -1; var smallest = -1;
            for (var i = 0; i < idle.Length; i++)
            {
                var item = idle[i];
                if (ReferenceEquals(item, buffer)) return;
                if (item is null) { if (empty < 0) empty = i; continue; }
                retained += item.Length;
                if (smallest < 0 || item.Length < idle[smallest]!.Length) smallest = i;
            }
            if (empty >= 0)
            {
                if (retained <= maxRetainedBytes - buffer.Length) idle[empty] = buffer;
            }
            else if (preferLargest && smallest >= 0 && buffer.Length > idle[smallest]!.Length
                && retained - idle[smallest]!.Length <= maxRetainedBytes - buffer.Length)
                idle[smallest] = buffer;
        }
    }

    public byte[] RentPreservingIdle(int length)
    {
        if (length <= maxRetainedBytes)
        {
            lock (gate)
            {
                var buffer = Take(length); if (buffer is not null) return buffer;
            }
        }
        return GC.AllocateUninitializedArray<byte>(length);
    }

    public void ReturnLatest(byte[] buffer)
    {
        if (buffer.Length > maxRetainedBytes) return;
        lock (gate)
        {
            for (var i = 0; i < idle.Length; i++) if (ReferenceEquals(idle[i], buffer)) return;
            var available = maxRetainedBytes - buffer.Length;
            for (var i = idle.Length - 1; i > 0; i--) idle[i] = idle[i - 1];
            idle[0] = buffer;
            for (var i = 1; i < idle.Length; i++)
            {
                if (idle[i] is not { } item) continue;
                if (item.Length > available) idle[i] = null;
                else available -= item.Length;
            }
        }
    }

    public byte[] RentImage(int length, CancellationToken cancellation, bool clear)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        cancellation.ThrowIfCancellationRequested();
        byte[]? buffer = null;
        if (length <= maxRetainedBytes)
        {
            lock (gate)
            {
                buffer = Take(length);
                if (buffer is null)
                {
                    // Preserve one compatible idle size on a miss, leaving slots
                    // and byte capacity for the requested owner's eventual return.
                    for (var i = 1; i < idle.Length; i++) idle[i] = null;
                    if (idle[0] is { } item && item.Length > maxRetainedBytes - length) idle[0] = null;
                }
            }
        }
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
