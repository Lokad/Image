namespace Lokad.Image.Internal;

// Final spatial assignments and populations, independent of clustering scratch.
// Both arrays retain their full capacity charge until this owner is disposed.
internal sealed class Vp8lHistogramPlan : IDisposable
{
    private readonly OperationContext context;
    private int[]? counts;
    private int[]? map;
    internal Vp8lHistogramLayout Layout { get; }
    internal int GroupCount { get; }
    internal int Columns { get; }
    internal int TileBits { get; }
    internal ReadOnlySpan<int> Map => map ?? throw new ObjectDisposedException(nameof(Vp8lHistogramPlan));
    internal ReadOnlySpan<int> Populations => counts ?? throw new ObjectDisposedException(nameof(Vp8lHistogramPlan));
    internal Span<int> WritableMap => map ?? throw new ObjectDisposedException(nameof(Vp8lHistogramPlan));
    internal Span<int> WritablePopulations => counts ?? throw new ObjectDisposedException(nameof(Vp8lHistogramPlan));

    internal Vp8lHistogramPlan(OperationContext context, Vp8lHistogramLayout layout, int groups, int tiles, int columns, int bits)
    {
        this.context = context; Layout = layout; GroupCount = groups; Columns = columns; TileBits = bits;
        var capacity = checked(groups * layout.Total); var reserved = ((long)capacity + tiles) * 4;
        context.Work(0); context.Reserve(reserved);
        try { context.Work(0); counts = new int[capacity]; map = new int[tiles]; }
        catch { counts = null; context.Release(reserved); throw; }
    }

    internal void DisposePopulations()
    {
        var ownedCounts = Interlocked.Exchange(ref counts, null);
        if (ownedCounts is not null) context.Release(ownedCounts.Length * 4L);
    }

    public void Dispose()
    {
        DisposePopulations();
        var ownedMap = Interlocked.Exchange(ref map, null);
        if (ownedMap is not null) context.Release(ownedMap.Length * 4L);
    }
}
