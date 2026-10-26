namespace Lokad.Image.Internal;

// Independent spatial population clustering. Tile syntax is RFC 9649 section
// 5.2.3; deterministic merge decisions use the admitted integer entropy model.
internal static class Vp8lReferenceClustering
{
    private const int SamplingPairs = 9;

    private static int PairCapacity(int tiles, int threshold)
    {
        var limit = Math.Min(tiles, threshold);
        // Sampling can enqueue repeated pairs independently of the subsequent
        // greedy threshold. Its loop runs only with at least four live tiles.
        return Math.Max(limit * limit, tiles >= 4 ? SamplingPairs : 1);
    }

    private struct Stats
    {
        internal long C0, C1, C2, C3, C4, Total;
        internal int T0, T1, T2, T3, T4, Used, Bin;
        internal readonly long Cost(int bank) => bank switch { 0 => C0, 1 => C1, 2 => C2, 3 => C3, _ => C4 };
        internal readonly int Trivial(int bank) => bank switch { 0 => T0, 1 => T1, 2 => T2, 3 => T3, _ => T4 };
        internal readonly bool Nontrivial => T1 < 0 || T2 < 0 || T3 < 0;
        internal void Set(int bank, long cost, int trivial)
        {
            switch (bank)
            {
                case 0: C0 = cost; T0 = trivial; break;
                case 1: C1 = cost; T1 = trivial; break;
                case 2: C2 = cost; T2 = trivial; break;
                case 3: C3 = cost; T3 = trivial; break;
                default: C4 = cost; T4 = trivial; break;
            }
        }
    }

    private struct Pair
    {
        internal int A, B;
        internal long C0, C1, C2, C3, C4, Total, Delta;
        internal readonly long Cost(int bank) => bank switch { 0 => C0, 1 => C1, 2 => C2, 3 => C3, _ => C4 };
        internal void Set(int bank, long cost)
        {
            switch (bank) { case 0: C0 = cost; break; case 1: C1 = cost; break; case 2: C2 = cost; break; case 3: C3 = cost; break; default: C4 = cost; break; }
        }
    }

    private sealed class State
    {
        private readonly OperationContext context;
        private readonly Vp8lHistogramLayout layout;
        private readonly int tiles, quality, threshold;
        private readonly int[] original, working, slots;
        private readonly Stats[] originals, items;
        private readonly Pair[] pool;
        private int count, pairs;
        internal State(int tiles, Vp8lHistogramLayout layout, OperationContext context, int quality, int threshold)
        {
            this.quality = quality; this.threshold = threshold; this.tiles = tiles; this.layout = layout; this.context = context;
            original = new int[tiles * layout.Total]; working = new int[original.Length];
            slots = new int[tiles]; originals = new Stats[tiles]; items = new Stats[tiles];
            pool = new Pair[PairCapacity(tiles, threshold)];
        }

        private int Start(int bank) => bank switch { 0 => 0, 1 => layout.Red, 2 => layout.Blue, 3 => layout.Alpha, _ => layout.Distance };
        private int Size(int bank) => bank == 0 ? layout.GreenSize : bank == 4 ? 40 : 256;
        private static long Limit(long baseline, long change) => change > long.MaxValue - baseline ? long.MaxValue : baseline + change;

        private Stats Initial(ReadOnlySpan<int> counts)
        {
            var result = new Stats();
            for (var bank = 0; bank < 5; bank++)
            {
                var samples = counts.Slice(Start(bank), Size(bank)); var nonzero = 0; var last = -1;
                context.Cancellation.ThrowIfCancellationRequested();
                for (var symbol = 0; symbol < samples.Length; symbol++) if (samples[symbol] != 0) { nonzero++; last = symbol; }
                if (nonzero != 0) result.Used |= 1 << bank;
                var cost = Vp8lReferenceEntropy.Population(samples, context); result.Total += cost;
                result.Set(bank, cost, nonzero == 1 ? last : -1);
            }
            return result;
        }

        internal void Fill(ReadOnlySpan<Vp8lToken> tokens, int width, int height, int bits)
        {
            var columns = (width + (1 << bits) - 1) >> bits; var position = 0;
            for (var i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].Length > (long)width * height - position) throw new ArgumentException("Tokens exceed their image.", nameof(tokens));
                var tile = (position / width >> bits) * columns + (position % width >> bits);
                layout.Fill(tokens.Slice(i, 1), width, true, original.AsSpan(tile * layout.Total, layout.Total), context);
                // Every token contributes a green symbol to its start tile.
                // Reuse the pending stats' Used field to avoid scanning empty tiles.
                originals[tile].Used = 1;
                position += tokens[i].Length;
            }
            if (position != width * height) throw new ArgumentException("Tokens do not cover their image.", nameof(tokens));
            context.Cancellation.ThrowIfCancellationRequested(); original.CopyTo(working, 0);
            for (var tile = 0; tile < tiles; tile++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                if (originals[tile].Used == 0) continue;
                var stats = Initial(original.AsSpan(tile * layout.Total, layout.Total)); originals[tile] = items[tile] = stats;
                if (stats.Used != 0) slots[count++] = tile;
            }
        }

        private bool Evaluate(int aSlot, int bSlot, bool fromOriginal, long maximum, out Pair pair)
        {
            context.Cancellation.ThrowIfCancellationRequested(); pair = default; if (maximum <= 0) return false;
            var a = items[aSlot]; var b = fromOriginal ? originals[bSlot] : items[bSlot];
            var bCounts = fromOriginal ? original : working;
            for (var bank = 0; bank < 5; bank++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var flag = 1 << bank; long cost;
                if ((a.Used & flag) == 0 || (b.Used & flag) == 0 || a.Trivial(bank) >= 0 && a.Trivial(bank) == b.Trivial(bank))
                    cost = (a.Used & flag) != 0 ? a.Cost(bank) : b.Cost(bank);
                else
                {
                    var start = Start(bank); var size = Size(bank);
                    cost = Vp8lReferenceEntropy.PopulationSum(working.AsSpan(aSlot * layout.Total + start, size),
                        bCounts.AsSpan(bSlot * layout.Total + start, size), context);
                }
                pair.Set(bank, cost); pair.Total += cost; if (pair.Total >= maximum) return false;
            }
            return true;
        }

        private void Combine(int aSlot, int bSlot, Pair costs)
        {
            var a = items[aSlot]; var b = items[bSlot]; context.Cancellation.ThrowIfCancellationRequested();
            for (var i = 0; i < layout.Total; i++) working[aSlot * layout.Total + i] += working[bSlot * layout.Total + i];
            for (var bank = 0; bank < 5; bank++) a.Set(bank, costs.Cost(bank), a.Trivial(bank) == b.Trivial(bank) ? a.Trivial(bank) : -1);
            a.Total = costs.Total; a.Used |= b.Used; items[aSlot] = a;
        }

        private void Remove(int at) => slots[at] = slots[--count];
        private void RemovePair(int at) => pool[at] = pool[--pairs];
        private void Promote(int at) { if (pool[at].Delta < pool[0].Delta) (pool[at], pool[0]) = (pool[0], pool[at]); }
        private static void Rename(ref Pair pair, int oldIndex, int newIndex)
        {
            if (pair.A == oldIndex) pair.A = newIndex; if (pair.B == oldIndex) pair.B = newIndex;
            if (pair.A > pair.B) (pair.A, pair.B) = (pair.B, pair.A);
        }
        private bool Refresh(ref Pair pair, long threshold)
        {
            var sum = items[slots[pair.A]].Total + items[slots[pair.B]].Total;
            if (!Evaluate(slots[pair.A], slots[pair.B], false, Limit(sum, threshold), out var costs)) return false;
            costs.A = pair.A; costs.B = pair.B; costs.Delta = costs.Total - sum; pair = costs; return true;
        }
        private long Push(int a, int b, long threshold, int capacity)
        {
            context.Cancellation.ThrowIfCancellationRequested(); if (pairs == capacity) return 0;
            var pair = new Pair { A = Math.Min(a, b), B = Math.Max(a, b) };
            if (!Refresh(ref pair, threshold)) return 0;
            pool[pairs++] = pair; Promote(pairs - 1); return pair.Delta;
        }

        private void MergeLowBins()
        {
            var minimum = long.MaxValue; var maximum = long.MinValue;
            for (var i = 0; i < count; i++) { context.Cancellation.ThrowIfCancellationRequested(); var cost = items[slots[i]].C0; minimum = Math.Min(minimum, cost); maximum = Math.Max(maximum, cost); }
            var range = maximum - minimum;
            for (var i = 0; i < count; i++)
            { context.Cancellation.ThrowIfCancellationRequested(); items[slots[i]].Bin = range == 0 ? 0 : (int)((4 - 1e-6) * (items[slots[i]].C0 - minimum) / range); }
            Span<int> first = stackalloc int[4]; first.Fill(-1);
            for (var at = 0; at < count;)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var bin = items[slots[at]].Bin;
                if (first[bin] < 0) { first[bin] = at++; continue; }
                Combine(slots[first[bin]], slots[at], default); Remove(at);
            }
            for (var i = 0; i < count; i++) items[slots[i]] = Initial(working.AsSpan(slots[i] * layout.Total, layout.Total));
        }

        private void MergeBins()
        {
            Span<long> minimum = stackalloc long[3]; minimum.Fill(long.MaxValue);
            Span<long> maximum = stackalloc long[3]; maximum.Fill(long.MinValue);
            for (var i = 0; i < count; i++)
            for (var c = 0; c < 3; c++)
            { context.Cancellation.ThrowIfCancellationRequested(); var cost = items[slots[i]].Cost(c); minimum[c] = Math.Min(minimum[c], cost); maximum[c] = Math.Max(maximum[c], cost); }
            for (var i = 0; i < count; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); ref var item = ref items[slots[i]]; item.Bin = 0;
                for (var c = 0; c < 3; c++) { var range = maximum[c] - minimum[c]; item.Bin = item.Bin * 4 + (range == 0 ? 0 : (int)((4 - 1e-6) * (item.Cost(c) - minimum[c]) / range)); }
            }
            Span<int> first = stackalloc int[64]; first.Fill(-1);
            Span<int> failures = stackalloc int[64]; failures.Clear();
            var factor = 16;
            if (quality < 90)
            {
                if (tiles > 256) factor /= 2;
                if (tiles > 512) factor /= 2;
                if (tiles > 1024) factor /= 2;
                if (quality <= 50) factor /= 2;
            }
            for (var at = 0; at < count;)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var candidate = items[slots[at]]; var bin = candidate.Bin;
                if (first[bin] < 0) { first[bin] = at++; continue; }
                var aSlot = slots[first[bin]]; var a = items[aSlot]; var change = -((candidate.Total * factor + 50) / 100);
                if (!Evaluate(aSlot, slots[at], false, a.Total + candidate.Total + change, out var costs)) { at++; continue; }
                var nontrivial = false;
                for (var bank = 1; bank < 4; bank++) nontrivial |= a.Trivial(bank) != candidate.Trivial(bank) || a.Trivial(bank) < 0;
                if (!nontrivial || candidate.Nontrivial && a.Nontrivial || failures[bin] >= 32)
                { Combine(aSlot, slots[at], costs); Remove(at); }
                else { failures[bin]++; at++; }
            }
        }

        private bool Sample()
        {
            const int capacity = SamplingPairs;
            if (count < threshold) return true;
            var outer = count; var misses = 0; var limit = outer / 2; uint seed = 1;
            for (var iteration = 0; iteration < outer && count >= threshold && ++misses < limit; iteration++)
            {
                var best = pairs == 0 ? 0 : pool[0].Delta; var range = (uint)((count - 1) * count); var tries = count / 2;
                for (var trial = 0; count >= 2 && trial < tries; trial++)
                {
                    context.Cancellation.ThrowIfCancellationRequested(); seed = (uint)((ulong)seed * 48271 % 2147483647); var value = seed % range;
                    var a = (int)(value / (count - 1)); var b = (int)(value % (count - 1)); if (b >= a) b++;
                    var delta = Push(a, b, best, capacity); if (delta < 0) { best = delta; if (pairs == capacity) break; }
                }
                if (pairs == 0) continue;
                var chosen = pool[0]; var first = chosen.A; var second = chosen.B;
                Combine(slots[first], slots[second], chosen); Remove(second);
                for (var at = 0; at < pairs;)
                {
                    context.Cancellation.ThrowIfCancellationRequested(); var pair = pool[at]; var a = pair.A == first || pair.A == second; var b = pair.B == first || pair.B == second;
                    if (a && b) { RemovePair(at); continue; }
                    if (a || b) { Rename(ref pair, second, first); if (!Refresh(ref pair, 0)) { RemovePair(at); continue; } }
                    Rename(ref pair, count, second); pool[at] = pair; Promote(at); at++;
                }
                misses = 0;
            }
            return count <= threshold;
        }

        private void Greedy()
        {
            // Sampling and greedy selection have separate candidate sets. Reuse
            // the allocation, not candidates left over by the sampling phase.
            pairs = 0; var capacity = count * count;
            for (var a = 0; a < count; a++) for (var b = a + 1; b < count; b++) Push(a, b, 0, capacity);
            while (pairs != 0)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var chosen = pool[0]; var a = chosen.A; var b = chosen.B;
                Combine(slots[a], slots[b], chosen); Remove(b);
                for (var at = 0; at < pairs;)
                {
                    context.Cancellation.ThrowIfCancellationRequested(); var pair = pool[at];
                    if (pair.A == a || pair.A == b || pair.B == a || pair.B == b) { RemovePair(at); continue; }
                    Rename(ref pair, count, b); pool[at] = pair; Promote(at); at++;
                }
                for (var other = 0; other < count; other++) if (other != a) Push(a, other, 0, capacity);
            }
        }

        internal Vp8lHistogramPlan Finish(bool lowEffort, int columns, int bits)
        {
            var useBins = count > (lowEffort ? 8 : 128) && quality < 100;
            if (useBins) { if (lowEffort) MergeLowBins(); else MergeBins(); }
            if (!(lowEffort && useBins) && Sample()) Greedy();
            var result = new Vp8lHistogramPlan(context, layout, count, tiles, columns, bits);
            try
            {
                var map = result.WritableMap;
                for (var tile = 0; tile < tiles; tile++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    if (originals[tile].Used == 0) { map[tile] = tile == 0 ? 0 : map[tile - 1]; continue; }
                    long best = long.MaxValue;
                    for (var group = 0; group < count; group++)
                    {
                        var slot = slots[group];
                        if (Evaluate(slot, tile, true, Limit(items[slot].Total, best), out var costs))
                        { best = costs.Total - items[slot].Total; map[tile] = group; }
                    }
                }
                var output = result.WritablePopulations;
                for (var tile = 0; tile < tiles; tile++)
                {
                    context.Cancellation.ThrowIfCancellationRequested();
                    if (originals[tile].Used == 0) continue;
                    var start = map[tile] * layout.Total;
                    for (var i = 0; i < layout.Total; i++) output[start + i] += original[tile * layout.Total + i];
                }
                return result;
            }
            catch { result.Dispose(); throw; }
        }
    }

    internal static Vp8lHistogramPlan Build(ReadOnlySpan<Vp8lToken> tokens, int width, int height,
        int tileBits, int cacheBits, bool lowEffort, OperationContext context, int quality = 75)
    {
        if (width is < 1 or > 16384 || height is < 1 or > 16384 || tileBits is < 2 or > 9)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (quality is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(quality));
        var threshold = 1 + (quality * quality * quality * 99 + 500_000) / 1_000_000;
        var columns = (width + (1 << tileBits) - 1) >> tileBits;
        var tiles = columns * ((height + (1 << tileBits) - 1) >> tileBits);
        // The preceding analysis selects tile bits to keep this finite bound.
        if (tiles > 2600) throw new ArgumentException("Histogram tiling exceeds the analyzed bound.", nameof(tileBits));
        var layout = new Vp8lHistogramLayout(cacheBits); var pairCapacity = PairCapacity(tiles, threshold);
        // Two flat count slabs, two conservative metadata allowances, index
        // slots and a fixed pair pool. Paired costs need no sum scratch.
        var reserved = tiles * (layout.Total * 8L + 192) + pairCapacity * 64L + 512;
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(reserved);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested(); var state = new State(tiles, layout, context, quality, threshold);
            state.Fill(tokens, width, height, tileBits); return state.Finish(lowEffort, columns, tileBits);
        }
        finally { context.Release(reserved); }
    }
}
