namespace Lokad.Image.Internal;

// Synchronous tree construction borrows this operation-owned storage. Canonical
// lengths and words never refer to it and may survive every subsequent reuse.
internal sealed class Vp8lHuffmanWorkspace : IDisposable
{
    private readonly OperationContext context;
    private readonly int capacity;
    private int[]? integers;
    private bool[]? protectedRun;
    private PriorityQueue<int, (long Weight, int Kind, int Rank)>? queue;
    private long reserved;

    internal Vp8lHuffmanWorkspace(int capacity, OperationContext context)
    {
        if (capacity is < 0 or > 1304) throw new ArgumentOutOfRangeException(nameof(capacity));
        this.context = context; this.capacity = capacity;
        // Five int regions, one bool region and a presized BCL priority queue
        // (conservatively 32 bytes/entry), including object/header allowance.
        var bytes = capacity * 53L + 512;
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(bytes);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested();
            integers = new int[capacity * 5]; protectedRun = new bool[capacity];
            queue = new(capacity); reserved = bytes;
        }
        catch { context.Release(bytes); throw; }
    }

    internal void Validate(int count, OperationContext borrower)
    {
        ObjectDisposedException.ThrowIf(integers is null, this);
        if (!ReferenceEquals(context, borrower) || count > capacity)
            throw new ArgumentException("Huffman workspace must cover the alphabet and share its operation.");
    }

    internal Span<int> Region(int region, int count) => integers.AsSpan(region * capacity, count);
    internal Span<bool> ProtectedRun(int count) => protectedRun.AsSpan(0, count);
    internal PriorityQueue<int, (long Weight, int Kind, int Rank)> Queue => queue!;

    public void Dispose()
    {
        var bytes = Interlocked.Exchange(ref reserved, 0);
        integers = null; protectedRun = null; queue = null;
        if (bytes != 0) context.Release(bytes);
    }
}
