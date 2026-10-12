namespace Lokad.Image.Internal;

/// <summary>One bounded idle byte array; live owners never share a lease.</summary>
internal sealed class PixelStoragePool
{
    public const int MaxRetainedBytes = 4 * 1024 * 1024;
    public static PixelStoragePool Shared { get; } = new();
    private byte[]? available;
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
            var buffer = Interlocked.Exchange(ref available, null);
            if (buffer?.Length == length) return buffer;
        }
        return GC.AllocateUninitializedArray<byte>(length);
    }

    public void Return(byte[] buffer)
    {
        if (buffer.Length <= maxRetainedBytes)
            Interlocked.CompareExchange(ref available, buffer, null);
    }
}
