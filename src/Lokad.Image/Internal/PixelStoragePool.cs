namespace Lokad.Image.Internal;

/// <summary>One bounded idle raster; live owners never share a lease.</summary>
internal sealed class PixelStoragePool
{
    public const int MaxRetainedBytes = 4 * 1024 * 1024;
    public static PixelStoragePool Shared { get; } = new();
    private byte[]? available;

    // This storage is private until every result sample has been overwritten.
    // Exact lengths keep backing capacity equal to the admitted pixel bytes.
    public byte[] Rent(int length)
    {
        if (length <= MaxRetainedBytes)
        {
            var buffer = Interlocked.Exchange(ref available, null);
            if (buffer?.Length == length) return buffer;
        }
        return GC.AllocateUninitializedArray<byte>(length);
    }

    public void Return(byte[] buffer)
    {
        if (buffer.Length <= MaxRetainedBytes)
            Interlocked.CompareExchange(ref available, buffer, null);
    }
}
