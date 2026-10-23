namespace Lokad.Image.Internal;

// An operation-owned alphabet. Dispose before its borrowed context; the entire
// capacity remains charged, including unused symbols and single-symbol trees.
internal sealed class Vp8lHuffmanLengths : IDisposable
{
    private readonly OperationContext context;
    private byte[]? lengths;
    internal ReadOnlySpan<byte> Values => lengths ?? throw new ObjectDisposedException(nameof(Vp8lHuffmanLengths));
    private Span<byte> Writable => lengths ?? throw new ObjectDisposedException(nameof(Vp8lHuffmanLengths));

    private Vp8lHuffmanLengths(int count, OperationContext context)
    {
        this.context = context; context.Work(0); context.Reserve(count);
        try { context.Work(0); lengths = new byte[count]; }
        catch { context.Release(count); throw; }
    }

    // RFC 9649 section 5.2 defines the canonical length representation. The
    // independent encoder policy smooths short population runs, then builds a
    // deterministic tree. Raising the positive-weight floor bounds its depth.
    internal static Vp8lHuffmanLengths Build(ReadOnlySpan<int> populations, OperationContext context, int maximum = 15)
    {
        if (populations.Length > 1304) throw new ArgumentOutOfRangeException(nameof(populations));
        if (maximum is < 1 or > 15) throw new ArgumentOutOfRangeException(nameof(maximum));
        var result = new Vp8lHuffmanLengths(populations.Length, context);
        long reserved = 0;
        try
        {
            var count = populations.Length;
            // Five int arrays, one bool array, and a presized BCL priority queue
            // (conservatively 32 bytes/entry), including object/header allowance.
            var scratch = count * 53L + 512;
            context.Reserve(scratch); reserved = scratch; context.Work(count);
            var weights = populations.ToArray(); var protectedRun = new bool[count];
            var first = new int[count]; var second = new int[count];
            var nodes = new int[count]; var depths = new int[count];
            var queue = new PriorityQueue<int, (long Weight, int Kind, int Rank)>(count);
            foreach (var weight in weights)
                if (weight < 0) throw new ArgumentOutOfRangeException(nameof(populations));
            Smooth(weights, protectedRun, context);
            var leaves = 0;
            foreach (var weight in weights) if (weight != 0) leaves++;
            if (leaves == 0) return result;
            if (leaves > 1 << maximum) throw new ArgumentException("Alphabet cannot fit the requested maximum depth.", nameof(maximum));
            for (long floor = 1; ; floor *= 2)
            {
                queue.Clear(); context.Work(count); result.Writable.Clear();
                for (var symbol = 0; symbol < count; symbol++)
                    if (weights[symbol] != 0) queue.Enqueue(symbol, (Math.Max(weights[symbol], floor), 0, -symbol));
                if (leaves == 1) { result.Writable[queue.Dequeue()] = 1; return result; }
                var serial = 0;
                while (queue.Count > 1)
                {
                    context.Work(1);
                    queue.TryDequeue(out var left, out var leftPriority);
                    queue.TryDequeue(out var right, out var rightPriority);
                    first[serial] = left; second[serial] = right;
                    queue.Enqueue(count + serial, (leftPriority.Weight + rightPriority.Weight, 1, serial)); serial++;
                }
                var used = 1; nodes[0] = queue.Dequeue(); depths[0] = 0; var deepest = 0;
                while (used != 0)
                {
                    context.Work(1); used--; var node = nodes[used]; var depth = depths[used];
                    if (node < count)
                    {
                        deepest = Math.Max(deepest, depth);
                        if (depth <= maximum) result.Writable[node] = (byte)depth;
                    }
                    else
                    {
                        var parent = node - count;
                        nodes[used] = first[parent]; depths[used++] = depth + 1;
                        nodes[used] = second[parent]; depths[used++] = depth + 1;
                    }
                }
                if (deepest <= maximum) return result;
            }
        }
        catch { result.Dispose(); throw; }
        finally { if (reserved != 0) context.Release(reserved); }
    }

    private static void Smooth(Span<int> weights, Span<bool> protectedRun, OperationContext context)
    {
        var end = weights.Length;
        while (end != 0 && weights[end - 1] == 0) { context.Work(1); end--; }
        if (end == 0) return;
        for (var start = 0; start < end;)
        {
            var stop = start + 1;
            while (stop < end && weights[stop] == weights[start]) { context.Work(1); stop++; }
            context.Work(1);
            if (stop - start >= (weights[start] == 0 ? 5 : 7)) protectedRun[start..stop].Fill(true);
            start = stop;
        }
        var span = 0; long sum = 0; var center = weights[0];
        for (var at = 0; at <= end; at++)
        {
            context.Work(1);
            var boundary = at == end || protectedRun[at] || at > 0 && protectedRun[at - 1]
                || Math.Abs((long)weights[at] - center) >= 4;
            if (boundary)
            {
                if (span >= 4 || span >= 3 && sum == 0)
                {
                    var replacement = sum == 0 ? 0 : Math.Max(1, (int)((sum + span / 2) / span));
                    weights.Slice(at - span, span).Fill(replacement);
                }
                span = 0; sum = 0;
                center = at < end - 3 ? (int)(((long)weights[at] + weights[at + 1] + weights[at + 2] + weights[at + 3] + 2) / 4)
                    : at < end ? weights[at] : 0;
            }
            span++;
            if (at < end) { sum += weights[at]; if (span >= 4) center = (int)((sum + span / 2) / span); }
        }
    }

    public void Dispose()
    {
        var owned = Interlocked.Exchange(ref lengths, null);
        if (owned is not null) context.Release(owned.Length);
    }
}
