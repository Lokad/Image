using System.Runtime.Intrinsics;

namespace Lokad.Image.Internal;

// Independent spatial population clustering. Tile syntax is RFC 9649 section
// 3.7.2.2; deterministic merge decisions use the admitted integer entropy model.
internal static class Vp8lReferenceClustering
{
    // Both callers add distinct, complete populations. The address mapping of
    // compact storage is resolved before this loop; logical merge order stays put.
    internal static void AddPopulation(Span<int> target, ReadOnlySpan<int> source, OperationContext context)
    {
        if (target.Length != source.Length) throw new ArgumentException("Population lengths differ.");
        context.Cancellation.ThrowIfCancellationRequested(); var i = 0;
        if (Vector512.IsHardwareAccelerated)
            for (; i <= target.Length - Vector512<int>.Count; i += Vector512<int>.Count)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                Vector512.Add(Vector512.Create<int>(target.Slice(i)), Vector512.Create<int>(source.Slice(i))).CopyTo(target.Slice(i));
            }
        if (Vector256.IsHardwareAccelerated)
            for (; i <= target.Length - Vector256<int>.Count; i += Vector256<int>.Count)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                Vector256.Add(Vector256.Create<int>(target.Slice(i)), Vector256.Create<int>(source.Slice(i))).CopyTo(target.Slice(i));
            }
        for (; i < target.Length; i++)
        {
            if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
            target[i] = unchecked(target[i] + source[i]);
        }
    }

    // A single nonzero logical 256-symbol bank has zero refined entropy.
    // Its run cost depends on the symbol position, but never its positive count.
    // The caller supplies unused, zeroed admitted storage and gets it back zeroed.
    internal static long ConstantPopulationCost(Span<int> zeroCounts, int symbol, OperationContext context)
    {
        if (zeroCounts.Length != 256 || (uint)symbol > 255) throw new ArgumentOutOfRangeException(nameof(symbol));
        zeroCounts[symbol] = 1;
        try { return Vp8lReferenceEntropy.Population(zeroCounts, context); }
        finally { zeroCounts[symbol] = 0; }
    }

    private const int SamplingPairs = 9;

    private static int SupportWords(Vp8lHistogramLayout layout, int tiles)
        => tiles < 64 ? 0 : (layout.RedSize == 256 ? 4 : 0)
            + (layout.BlueSize == 256 ? 4 : 0) + (layout.AlphaSize == 256 ? 4 : 0);

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
        private readonly uint literal;
        private readonly long emptyCost, redCost, blueCost, alphaCost;
        private readonly int[] slots;
        private readonly Vp8lPopulationWorkspace populations;
        private readonly int supportWords, blueSupport, alphaSupport;
        private readonly Stats[] originals, items;
        private readonly Pair[] pool;
        private int count, pairs;
        internal State(int tiles, Vp8lPopulationWorkspace populations,
            Vp8lHistogramLayout layout, uint literal, int supportWords, OperationContext context, int quality, int threshold)
        {
            this.quality = quality; this.threshold = threshold; this.tiles = tiles; this.layout = layout; this.context = context;
            this.populations = populations; this.literal = literal; this.supportWords = supportWords;
            blueSupport = layout.RedSize == 256 ? 4 : 0;
            alphaSupport = blueSupport + (layout.BlueSize == 256 ? 4 : 0);
            if (layout.ConstantBanks != 0)
            {
                // Logical constant-bank prices use bounded temporary stack storage.
                Span<int> scratch = stackalloc int[256]; scratch.Clear();
                emptyCost = Vp8lReferenceEntropy.Population(scratch, context);
                if (layout.RedSize == 1) redCost = ConstantPopulationCost(scratch, (int)(literal >> 16 & 255), context);
                if (layout.BlueSize == 1) blueCost = ConstantPopulationCost(scratch, (int)(literal & 255), context);
                if (layout.AlphaSize == 1) alphaCost = ConstantPopulationCost(scratch, (int)(literal >> 24), context);
            }
            slots = new int[tiles]; originals = new Stats[tiles]; items = new Stats[tiles];
            pool = new Pair[PairCapacity(tiles, threshold)];
        }

        private Span<int> Population(int slot, int side = 0, bool original = false)
            => populations.Read(slot, side, original);
        private Span<ulong> Support(int slot, int side = 0, bool original = false)
            => populations.Support(slot, side, original);
        private int SupportStart(int bank) => bank == 1 ? 0 : bank == 2 ? blueSupport : alphaSupport;

        private int Start(int bank) => bank switch { 0 => 0, 1 => layout.Red, 2 => layout.Blue, 3 => layout.Alpha, _ => layout.Distance };
        private int Size(int bank) => bank switch { 0 => layout.GreenSize, 1 => layout.RedSize, 2 => layout.BlueSize, 3 => layout.AlphaSize, _ => 40 };
        private long ConstantCost(int bank) => bank == 1 ? redCost : bank == 2 ? blueCost : alphaCost;
        private int ConstantSymbol(int bank) => bank == 1 ? (int)(literal >> 16 & 255) : bank == 2 ? (int)(literal & 255) : (int)(literal >> 24);
        private static long Limit(long baseline, long change) => change > long.MaxValue - baseline ? long.MaxValue : baseline + change;

        private Stats Initial(int slot)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (!populations.IsMerged(slot)) return InitialLeaf(populations.Leaf(slot));
            var counts = Population(slot); var words = Support(slot); var result = new Stats();
            for (var bank = 0; bank < 5; bank++)
            {
                var samples = counts.Slice(Start(bank), Size(bank)); var nonzero = 0; var last = -1;
                context.Cancellation.ThrowIfCancellationRequested();
                if (samples.Length == 1)
                {
                    var used = samples[0] != 0; var logicalCost = used ? ConstantCost(bank) : emptyCost;
                    if (used) result.Used |= 1 << bank;
                    result.Total += logicalCost; result.Set(bank, logicalCost, used ? ConstantSymbol(bank) : -1);
                    continue;
                }
                if (samples.Length == 256 && supportWords != 0)
                    (nonzero, last) = Vp8lReferenceEntropy.PrepareColorSupport(samples, words.Slice(SupportStart(bank), 4), context);
                else
                    for (var symbol = 0; symbol < samples.Length; symbol++) if (samples[symbol] != 0) { nonzero++; last = symbol; }
                if (nonzero != 0) result.Used |= 1 << bank;
                var cost = Vp8lReferenceEntropy.Population(samples, context); result.Total += cost;
                result.Set(bank, cost, nonzero == 1 ? last : -1);
            }
            return result;
        }

        private Stats InitialLeaf(ReadOnlySpan<uint> entries)
        {
            var result = new Stats(); var next = 0;
            for (var bank = 0; bank < 5; bank++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var start = Start(bank); var size = Size(bank); var first = next;
                while (next < entries.Length && (entries[next] & 4095) < start + size) next++;
                var nonzero = next - first; var used = nonzero != 0; long cost; int last;
                if (size == 1)
                { cost = used ? ConstantCost(bank) : emptyCost; last = used ? ConstantSymbol(bank) : -1; }
                else
                {
                    cost = Vp8lReferenceEntropy.SparsePopulation(entries.Slice(first, nonzero), start, size, context);
                    last = nonzero == 1 ? (int)(entries[first] & 4095) - start : -1;
                }
                if (used) result.Used |= 1 << bank;
                result.Total += cost; result.Set(bank, cost, last);
            }
            return result;
        }

        internal void Fill()
        {
            for (var tile = 0; tile < tiles; tile++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                if (populations.IsEmpty(tile)) continue;
                var stats = Initial(tile); originals[tile] = items[tile] = stats;
                if (stats.Used != 0) slots[count++] = tile;
            }
        }

        private bool Evaluate(int aSlot, int bSlot, bool fromOriginal, long maximum, out Pair pair)
        {
            context.Cancellation.ThrowIfCancellationRequested(); pair = default; if (maximum <= 0) return false;
            var a = items[aSlot]; var b = fromOriginal ? originals[bSlot] : items[bSlot];
            var aCounts = Population(aSlot); var bCounts = Population(bSlot, 1, fromOriginal);
            var aSupport = Support(aSlot); var bSupport = Support(bSlot, 1, fromOriginal);
            // Preserve the measured support traversal for 256-symbol colors.
            // The larger green/cache alphabet reuses unchanged A run models.
            var leaf = fromOriginal || !populations.IsMerged(bSlot);
            var entries = leaf ? populations.Leaf(bSlot) : default; var greenEnd = 0;
            if (leaf) while (greenEnd < entries.Length && (entries[greenEnd] & 4095) < layout.GreenSize) greenEnd++;
            for (var bank = 0; bank < 5; bank++)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var flag = 1 << bank; long cost;
                if ((a.Used & flag) == 0 || (b.Used & flag) == 0 || a.Trivial(bank) >= 0 && a.Trivial(bank) == b.Trivial(bank))
                    cost = (a.Used & flag) != 0 ? a.Cost(bank) : b.Cost(bank);
                else
                {
                    var start = Start(bank); var size = Size(bank);
                    // A prior merge with an empty bank can erase the trivial
                    // flag. Preserve that flag's decision semantics, but score
                    // the still-constant logical bank rather than one word.
                    cost = size == 1 ? ConstantCost(bank)
                        : bank == 0 && leaf ? populations.SumGreenLeaf(aSlot, entries[..greenEnd])
                        : size == 256 && supportWords != 0
                            ? Vp8lReferenceEntropy.PopulationSumColor(aCounts.Slice(start, size), bCounts.Slice(start, size),
                                aSupport.Slice(SupportStart(bank), 4), bSupport.Slice(SupportStart(bank), 4), context)
                            : Vp8lReferenceEntropy.PopulationSum(aCounts.Slice(start, size), bCounts.Slice(start, size), context);
                }
                pair.Set(bank, cost); pair.Total += cost; if (pair.Total >= maximum) return false;
            }
            return true;
        }

        private void Combine(int aSlot, int bSlot, Pair costs)
        {
            var a = items[aSlot]; var b = items[bSlot]; context.Cancellation.ThrowIfCancellationRequested();
            populations.Merge(aSlot, bSlot);
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
            for (var i = 0; i < count; i++) items[slots[i]] = Initial(slots[i]);
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
            var full = new Vp8lHistogramLayout(layout.CacheBits);
            var result = new Vp8lHistogramPlan(context, full, count, tiles, columns, bits);
            try
            {
                var map = result.WritableMap; var output = result.WritablePopulations;
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
                    var start = map[tile] * full.Total;
                    var counts = Population(tile, 1, true);
                    layout.AddToFull(output.Slice(start, full.Total), counts, literal, context);
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
        uint literal = 0, varying = 0; var hasLiteral = false;
        if (tiles >= 64)
        {
            // Keep the existing logical constant-bank decision and token order.
            var position = 0;
            foreach (var token in tokens)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                if (position >= (long)width * height || token.Length > (long)width * height - position)
                    throw new ArgumentException("Tokens exceed their image.", nameof(tokens));
                if (token.Code == 0)
                {
                    if (!hasLiteral) { literal = token.Value; hasLiteral = true; }
                    else varying |= literal ^ token.Value;
                }
                position += token.Length;
            }
            if (position != width * height) throw new ArgumentException("Tokens do not cover their image.", nameof(tokens));
        }
        if (hasLiteral)
        {
            var constantBanks = ((varying & 0x00ff0000u) == 0 ? 2 : 0)
                | ((varying & 0x000000ffu) == 0 ? 4 : 0) | ((varying & 0xff000000u) == 0 ? 8 : 0);
            layout = new Vp8lHistogramLayout(cacheBits, constantBanks);
        }
        var supportWords = SupportWords(layout, tiles);
        using var populations = Vp8lPopulationWorkspace.Create(tokens, width, height, tileBits, layout, supportWords, context);
        var reserved = tiles * 192L + pairCapacity * 64L + 512;
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(reserved);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var state = new State(tiles, populations, layout, literal, supportWords, context, quality, threshold);
            state.Fill(); return state.Finish(lowEffort, columns, tileBits);
        }
        finally { context.Release(reserved); }
    }
}
