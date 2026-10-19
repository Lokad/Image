using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

internal sealed class OperationContext : IDisposable
{
    public ImageLimits Limits { get; }
    public CancellationToken Cancellation { get; }
    private long liveBytes;
    private long work;
    private IMemoryScope? memoryScope;
    private IMemoryReservation? memory;
    internal bool HasMemoryBudget => Limits.MemoryBudget is not null;

    public long RemainingWork => Limits.MaxWork - work;
    public long RemainingWorkingBytes => Limits.MaxWorkingBytes - liveBytes;
    internal long ConsumedWork => work;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Work(long units)
    {
        Cancellation.ThrowIfCancellationRequested();
        if (units < 0 || units > Limits.MaxWork - work)
            throw new ImageLimitException(ImageLimitKind.Work, "Computational work limit exceeded.");
        work += units;
    }

    // Publish charges already admitted by an exclusive loop against its saved
    // remaining budget. Each charge must check cancellation before consumption.
    // Cleanup must also publish completed work after cancellation, without a
    // new cancellation check discarding charges or replacing the original error.
    public void CommitAdmittedWork(long units)
    {
        if (units < 0 || units > Limits.MaxWork - work)
            throw new ImageLimitException(ImageLimitKind.Work, "Computational work limit exceeded.");
        work += units;
    }

    // An entropy code may admit its full logical bit cost together. When the
    // budget is too tight, its caller retains the original one-bit sequence so
    // partial consumption and format/limit failure precedence stay unchanged.
    public bool TryWork(long units)
    {
        Cancellation.ThrowIfCancellationRequested();
        if (units < 0 || units > Limits.MaxWork - work) return false;
        work += units; return true;
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
