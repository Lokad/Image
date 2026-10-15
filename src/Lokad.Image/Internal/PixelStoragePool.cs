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
}
