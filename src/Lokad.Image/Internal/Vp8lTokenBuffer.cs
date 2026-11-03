using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

// Eight bytes per token. Positions are derived cumulatively, not retained.
// Code zero is a literal, positive is a copy length, negative is ~cacheIndex.
internal readonly record struct Vp8lToken(uint Value, int Code)
{
    internal int Length => Code > 0 ? Code : 1;
    internal static Vp8lToken Literal(uint pixel) => new(pixel, 0);
    internal static Vp8lToken Copy(int length, int distance) => new((uint)distance, length);
    internal static Vp8lToken Cached(int index) => new(0, ~index);
}

// Fixed input-derived capacity; no resize or available-memory strategy changes.
// This operation-owned buffer must be disposed before its context.
internal sealed class Vp8lTokenBuffer : IDisposable
{
    private readonly OperationContext context;
    private Vp8lToken[]? tokens;
    internal int Count { get; private set; }
    internal int CacheBits { get; set; }
    internal ReadOnlySpan<Vp8lToken> Tokens => (tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer))).AsSpan(0, Count);
    internal Span<Vp8lToken> Writable => (tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer))).AsSpan(0, Count);
    internal long CapacityBytes => (tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer))).Length * 8L;

    // Transfer a rejected parsing candidate's existing capacity to the next
    // candidate. Only Count entries are live; the previous values need no clear.
    // Admission remains charged throughout, without a pool or another allowance.
    internal void ResetForReuse(OperationContext owner, int capacity)
    {
        var storage = tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer));
        if (!ReferenceEquals(context, owner) || storage.Length != capacity)
            throw new ArgumentException("Token scratch must belong to the same operation and input capacity.");
        Count = 0; CacheBits = 0;
    }

    // Matching/forward tracing finish before output tokens are written. Borrow
    // only unused, already admitted capacity; no selected candidate may alias it.
    internal Span<int> BorrowScratchWords(OperationContext owner, int words)
    {
        var storage = tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer));
        if (!ReferenceEquals(context, owner) || Count != 0 || CacheBits != 0)
            throw new ArgumentException("Scratch must be an empty token owner in the same operation.");
        ArgumentOutOfRangeException.ThrowIfNegative(words);
        if (words > storage.Length * 2) throw new ArgumentException("Token capacity does not cover scratch.", nameof(words));
        return MemoryMarshal.Cast<Vp8lToken, int>(storage.AsSpan())[..words];
    }

    // History reads the live prefix without writing tokens. Its temporary users
    // finish before cache application or further Add calls. Keep all capacity
    // charged to this owner; insufficient capacity is decided before admission.
    internal Span<byte> BorrowTailBytes(OperationContext owner, int bytes)
    {
        var storage = tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer));
        if (!ReferenceEquals(context, owner) || CacheBits != 0)
            throw new ArgumentException("History scratch must belong to the uncached candidate's operation.");
        ArgumentOutOfRangeException.ThrowIfNegative(bytes);
        var tail = MemoryMarshal.AsBytes(storage.AsSpan(Count));
        return bytes <= tail.Length ? tail[..bytes] : default;
    }

    // Aliasing must include idle capacity before it is reused as workspace.
    internal bool OverlapsStorage<T>(ReadOnlySpan<T> input) where T : unmanaged
    {
        var storage = tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer));
        return MemoryMarshal.AsBytes(input).Overlaps(MemoryMarshal.AsBytes(storage.AsSpan()));
    }

    // Backtracking filled this tail in forward token order, after every path
    // read completed. Move toward the front in bounded, overlap-safe chunks.
    internal void CommitTail(OperationContext owner, int first)
    {
        var storage = tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer));
        if (!ReferenceEquals(context, owner) || Count != 0 || CacheBits != 0)
            throw new ArgumentException("Trace tail must belong to the empty output owner.");
        if ((uint)first > (uint)storage.Length) throw new ArgumentOutOfRangeException(nameof(first));
        var count = storage.Length - first;
        context.Cancellation.ThrowIfCancellationRequested();
        if (first != 0) for (var at = 0; at < count; at += 1024)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var length = Math.Min(1024, count - at);
            storage.AsSpan(first + at, length).CopyTo(storage.AsSpan(at, length));
        }
        Count = count;
    }

    internal Vp8lTokenBuffer(OperationContext context, int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity > Array.MaxLength / 8) throw new ImageLimitException(ImageLimitKind.BufferLength, "Token buffer exceeds the supported length.");
        this.context = context; context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(capacity * 8L);
        try { context.Cancellation.ThrowIfCancellationRequested(); tokens = new Vp8lToken[capacity]; }
        catch { context.Release(capacity * 8L); throw; }
    }

    internal void Add(Vp8lToken token)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        var storage = tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer));
        if (Count == storage.Length) throw new InvalidOperationException("Token capacity exceeded.");
        storage[Count++] = token;
    }

    internal void Reverse()
    {
        context.Cancellation.ThrowIfCancellationRequested();
        var storage = tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer));
        var pairs = Count / 2;
        for (var i = 0; i < pairs; i++)
        {
            if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
            (storage[i], storage[Count - 1 - i]) = (storage[Count - 1 - i], storage[i]);
        }
    }

    public void Dispose()
    {
        var owned = Interlocked.Exchange(ref tokens, null);
        if (owned is not null) context.Release(owned.Length * 8L);
    }
}
