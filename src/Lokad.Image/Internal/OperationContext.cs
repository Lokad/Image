using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal sealed class OperationContext : IDisposable
{
    public ImageLimits Limits { get; }
    public CancellationToken Cancellation { get; }
    private long liveBytes;
    private IMemoryScope? memoryScope;
    private IMemoryReservation? memory;
    internal bool HasMemoryBudget => Limits.MemoryBudget is not null;
    public long RemainingWorkingBytes => Limits.MaxWorkingBytes - liveBytes;

    public OperationContext(ImageLimits limits, CancellationToken cancellation)
    {
        ArgumentNullException.ThrowIfNull(limits);
        limits.Validate();
        cancellation.ThrowIfCancellationRequested();
        Limits = limits;
        Cancellation = cancellation;
    }

    public void Reserve(long bytes)
    {
        if (bytes < 0 || bytes > Limits.MaxWorkingBytes - liveBytes)
            throw new ImageLimitException(ImageLimitKind.WorkingBytes, "Working memory limit exceeded.");
        if (Limits.MemoryBudget is { } budget)
        {
            memoryScope ??= budget.CreateScope();
            if (memory is null)
            {
                if (!memoryScope.TryReserve(bytes, out memory)) throw MemoryAdmission.Refused();
            }
            else if (!memory.TryResize(checked(memory.Bytes + bytes))) throw MemoryAdmission.Refused();
        }
        liveBytes += bytes;
    }

    // Borrowed input still counts toward the existing local limit, but its owner
    // already accounts for storage at the shared root. Copies use Reserve instead.
    internal void ReserveBorrowed(long bytes)
    {
        if (bytes < 0 || bytes > Limits.MaxWorkingBytes - liveBytes)
            throw new ImageLimitException(ImageLimitKind.WorkingBytes, "Working memory limit exceeded.");
        liveBytes += bytes;
    }

    internal void ReleaseBorrowed(long bytes) => liveBytes -= bytes;

    public void Release(long bytes)
    {
        if (memory is not null && !memory.TryResize(memory.Bytes - bytes))
            throw new InvalidOperationException("A shrinking memory reservation was refused.");
        liveBytes -= bytes;
    }

    public void Dispose()
    {
        // The execution boundary has awaited every user before reaching here.
        Interlocked.Exchange(ref memoryScope, null)?.Dispose();
        memory = null;
    }

    internal byte[] RentPixels(PixelStoragePool pool, int length) => HasMemoryBudget
        ? GC.AllocateUninitializedArray<byte>(length) : pool.Rent(length);
    internal void ReturnPixels(PixelStoragePool pool, byte[] bytes)
    {
        if (!HasMemoryBudget) pool.Return(bytes);
    }
    internal int[] RentCoefficients(int length) => HasMemoryBudget
        ? new int[length] : JpegCoefficientPool.Shared.Rent(length);
    internal void ReturnCoefficients(int[] bytes)
    {
        if (!HasMemoryBudget) JpegCoefficientPool.Shared.Return(bytes);
    }

    public int PixelBytes(int width, int height, int bitDepth)
    {
        if (width < 1 || height < 1)
            throw new ArgumentOutOfRangeException(nameof(width), "Dimensions must be positive.");
        if (bitDepth is not (8 or 16))
            throw new ArgumentOutOfRangeException(nameof(bitDepth), "Pixel depth must be 8 or 16.");
        var pixels = (long)width * height;
        if (width > Limits.MaxDimension || height > Limits.MaxDimension || pixels > Limits.MaxPixels
            || pixels > Array.MaxLength / ((bitDepth / 8) * 4))
            throw new ImageLimitException(width > Limits.MaxDimension || height > Limits.MaxDimension
                ? ImageLimitKind.Dimension : pixels > Limits.MaxPixels
                ? ImageLimitKind.PixelCount : ImageLimitKind.BufferLength,
                "Image dimensions or pixel count exceed the configured limits.");
        return (int)(pixels * (bitDepth / 8) * 4);
    }
}
