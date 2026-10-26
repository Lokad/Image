namespace Lokad.Image.Internal;

internal struct ShapingGlyph
{
    internal int Glyph { get; set; }
    internal int Scalar { get; set; }
    internal int SourceOffset { get; set; }
    internal int SourceLength { get; set; }
    internal int Cluster { get; set; }
    internal int Ligature { get; set; }
    internal int Component { get; set; }
    internal int ComponentCount { get; set; }
    internal uint Features { get; set; }
    internal byte Level { get; set; }
    internal byte Kind { get; set; }
    internal double Advance { get; set; }
    internal double OffsetX { get; set; }
    internal double OffsetY { get; set; }
    internal double OriginX { get; set; }
    internal int AttachTo { get; set; } // Logical glyph index + 1; zero means unattached.
    internal double AttachX { get; set; }
    internal double AttachY { get; set; }
}

// One coarse temporary owner for a run. Retained layout storage is allocated
// separately before publication; the caller's font and source text remain borrowed.
internal sealed class ShapingBuffer : IDisposable
{
    private const int GlyphBytes = 96, Overhead = 64;
    private readonly OperationContext context;
    private ShapingGlyph[] glyphs;
    private bool disposed;
    internal OperationContext Context => context;
    internal int Count { get; private set; }
    internal int NextLigature { get; set; }
    internal Span<ShapingGlyph> Span { get { Check(); return glyphs.AsSpan(0, Count); } }
    internal ref ShapingGlyph this[int index]
    {
        get { Check(); if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index)); return ref glyphs[index]; }
    }
    internal ShapingBuffer(int capacity, OperationContext context)
    {
        this.context = context;
        CheckCapacity(capacity); var bytes = Storage(capacity); context.Reserve(bytes);
        try { glyphs = capacity == 0 ? [] : new ShapingGlyph[capacity]; }
        catch { context.Release(bytes); throw; }
    }
    private static long Storage(int capacity) => Overhead + capacity * (long)GlyphBytes;
    private void Check() => ObjectDisposedException.ThrowIf(disposed, this);
    private void CheckCapacity(int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (count > context.Limits.MaxContainerItems)
            throw new ImageLimitException(ImageLimitKind.ContainerItems, "Shaping glyph limit exceeded.");
        if (count > Array.MaxLength / GlyphBytes)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "Shaping storage exceeds the managed limit.");
    }
    internal void Add(ShapingGlyph glyph)
    {
        Check(); CheckCapacity(checked(Count + 1));
        if (Count == glyphs.Length)
        {
            var capacity = (int)Math.Min(context.Limits.MaxContainerItems, Math.Max(Count + 1L, Math.Max(4, glyphs.Length * 2L)));
            CheckCapacity(capacity); var bytes = Storage(capacity); context.Reserve(bytes);
            ShapingGlyph[] next;
            try { context.Cancellation.ThrowIfCancellationRequested(); next = new ShapingGlyph[capacity]; glyphs.AsSpan(0, Count).CopyTo(next); }
            catch { context.Release(bytes); throw; }
            context.Release(Storage(glyphs.Length)); glyphs = next;
        }
        context.Cancellation.ThrowIfCancellationRequested(); glyphs[Count++] = glyph;
    }
    internal void RemoveAt(int index)
    {
        Check(); if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
        context.Cancellation.ThrowIfCancellationRequested();
        glyphs.AsSpan(index + 1, Count - index - 1).CopyTo(glyphs.AsSpan(index));
        glyphs[--Count] = default;
    }
    internal void RemoveKind(byte kind)
    {
        Check(); var write = 0;
        for (var read = 0; read < Count; read++)
        {
            context.Cancellation.ThrowIfCancellationRequested(); if (glyphs[read].Kind != kind) glyphs[write++] = glyphs[read];
        }
        glyphs.AsSpan(write, Count - write).Clear(); Count = write;
    }
    internal void RemoveComponents(ReadOnlySpan<int> indices)
    {
        Check();
        var write = indices[0] + 1; var next = 1;
        for (var read = write; read < Count; read++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (next < indices.Length && read == indices[next]) { next++; continue; }
            glyphs[write++] = glyphs[read];
        }
        glyphs.AsSpan(write, Count - write).Clear(); Count = write;
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true; var bytes = Storage(glyphs.Length); glyphs = []; Count = 0; context.Release(bytes);
    }
}
