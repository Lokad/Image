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

    internal Vp8lTokenBuffer(OperationContext context, int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        if (capacity > Array.MaxLength / 8) throw new ImageLimitException(ImageLimitKind.BufferLength, "Token buffer exceeds the supported length.");
        this.context = context; context.Work(0); context.Reserve(capacity * 8L);
        try { context.Work(0); tokens = new Vp8lToken[capacity]; }
        catch { context.Release(capacity * 8L); throw; }
    }

    internal void Add(Vp8lToken token)
    {
        context.Work(1);
        var storage = tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer));
        if (Count == storage.Length) throw new InvalidOperationException("Token capacity exceeded.");
        storage[Count++] = token;
    }

    internal void Reverse()
    {
        context.Work(0);
        var storage = tokens ?? throw new ObjectDisposedException(nameof(Vp8lTokenBuffer));
        var pairs = Count / 2;
        for (var i = 0; i < pairs; i++)
        {
            if ((i & 1023) == 0) context.Work(Math.Min(1024, pairs - i));
            (storage[i], storage[Count - 1 - i]) = (storage[Count - 1 - i], storage[i]);
        }
    }

    public void Dispose()
    {
        var owned = Interlocked.Exchange(ref tokens, null);
        if (owned is not null) context.Release(owned.Length * 8L);
    }
}
