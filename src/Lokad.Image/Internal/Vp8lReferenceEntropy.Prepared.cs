namespace Lokad.Image.Internal;

internal static partial class Vp8lReferenceEntropy
{
    internal partial struct RunCost
    {
        // Conservative retained-capacity accounting, including struct padding.
        internal const int AdmittedBytes = 64;

        internal void Subtract(int count, int length)
        {
            // Prepared sums only add nonnegative counts. Their final maximum
            // cannot fall below the original maximum; retain that lower bound.
            if (count != 0)
            {
                sum -= count * length; nonzero -= length;
                entropy += Vp8lReferenceCost.SLog(count) * length;
                if (length > 3) { nonzeroRuns--; longNonzero -= length; }
                else shortNonzero -= length;
            }
            else if (length > 3) { zeroRuns--; longZero -= length; }
            else shortZero -= length;
        }
    }

    internal static RunCost PrepareRuns(ReadOnlySpan<int> counts, Span<uint> runs, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested(); var cost = new RunCost();
        for (var at = 0; at < counts.Length;)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var end = at + 1; while (end < counts.Length && counts[end] == counts[at]) end++;
            cost.Add(counts[at], end - at);
            // Physical bank size <=1304: local start/end each fit twelve bits.
            runs.Slice(at, end - at).Fill((uint)(at | end << 12)); at = end;
        }
        return cost;
    }

    private static int RepairStart(ReadOnlySpan<uint> runs, int symbol)
    {
        var start = (int)(runs[symbol] & 4095);
        return start == 0 ? 0 : (int)(runs[start - 1] & 4095);
    }

    private static int RepairEnd(ReadOnlySpan<uint> runs, int symbol)
    {
        var end = (int)(runs[symbol] >> 12);
        return end == runs.Length ? end : (int)(runs[end] >> 12);
    }

    // Entries are the sorted immutable B leaf restricted to this logical bank.
    // Repair complete original runs and their immediate neighbors. Thus every
    // outside boundary retains its original unequal, unmodified counts, even
    // when an edited count joins either neighbor. Overlapping repairs unite.
    internal static long PreparedPopulationSum(ReadOnlySpan<int> counts, ReadOnlySpan<uint> runs,
        RunCost basis, ReadOnlySpan<uint> entries, int bankStart, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested(); var cost = basis;
        for (var first = 0; first < entries.Length;)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var symbol = (int)(entries[first] & 4095) - bankStart;
            var start = RepairStart(runs, symbol); var end = RepairEnd(runs, symbol); var last = first + 1;
            while (last < entries.Length)
            {
                symbol = (int)(entries[last] & 4095) - bankStart;
                if (RepairStart(runs, symbol) > end) break;
                end = Math.Max(end, RepairEnd(runs, symbol)); last++;
            }
            for (var at = start; at < end;)
            {
                var stop = (int)(runs[at] >> 12);
                cost.Subtract(counts[at], stop - at); at = stop;
            }
            var position = start; var next = first; var previous = -1; var length = 0;
            while (position < end)
            {
                int value, segment;
                if (next < last && position == (int)(entries[next] & 4095) - bankStart)
                { value = counts[position] + (int)(entries[next] >> 12); segment = 1; next++; }
                else
                {
                    value = counts[position]; var stop = Math.Min(end, (int)(runs[position] >> 12));
                    if (next < last) stop = Math.Min(stop, (int)(entries[next] & 4095) - bankStart);
                    segment = stop - position;
                }
                if (value == previous) length += segment;
                else
                { if (length != 0) cost.Add(previous, length); previous = value; length = segment; }
                position += segment;
            }
            if (length != 0) cost.Add(previous, length); first = last;
        }
        return cost.Finish();
    }
}
