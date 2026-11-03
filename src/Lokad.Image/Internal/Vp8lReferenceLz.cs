using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

// Auxiliary and main-image parsing. All candidates borrow the same
// pixels/matches; only the chosen owned token buffer escapes, with capacity live.
internal static class Vp8lReferenceLz
{
    private const long Scale = Vp8lReferenceCost.Scale;

    // Takes ownership of an optional matching-stage token buffer on entry.
    internal static Vp8lTokenBuffer Greedy(ReadOnlySpan<uint> pixels, ReadOnlySpan<uint> matches, OperationContext context,
        Vp8lTokenBuffer? scratch = null)
    {
        var result = scratch;
        try
        {
            if (pixels.IsEmpty || pixels.Length != matches.Length) throw new ArgumentException("Expected matching source/index lengths.");
            if (result is null) result = new Vp8lTokenBuffer(context, pixels.Length);
            else result.ResetForReuse(context, pixels.Length);
            var at = 0;
            while (at < pixels.Length)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var length = (int)(matches[at] & 4095); var distance = (int)(matches[at] >> 12);
                if (length >= 4)
                {
                    var farthest = 0; var stop = Math.Min(at + length, pixels.Length - 1);
                    for (var next = at + 1; next <= stop; next++)
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        var span = (int)(matches[next] & 4095); var reach = next + (span >= 4 ? span : 1);
                        if (reach > farthest) { length = next - at; farthest = reach; if (reach >= pixels.Length) break; }
                    }
                }
                else length = 1;
                result.Add(length == 1 ? Vp8lToken.Literal(pixels[at]) : Vp8lToken.Copy(length, distance)); at += length;
            }
            return result;
        }
        catch { result?.Dispose(); throw; }
    }

    private static int Match(ReadOnlySpan<uint> pixels, int at, int distance, OperationContext context)
    {
        var length = 0; var maximum = Math.Min(4095, pixels.Length - at);
        while (length < maximum)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (pixels[at + length] != pixels[at + length - distance]) break;
            length++;
        }
        return length;
    }

    internal static Vp8lTokenBuffer Repeats(ReadOnlySpan<uint> pixels, int width, OperationContext context)
    {
        Validate(pixels.Length, width);
        var result = new Vp8lTokenBuffer(context, pixels.Length);
        try
        {
            result.Add(Vp8lToken.Literal(pixels[0])); var at = 1;
            while (at < pixels.Length)
            {
                var repeat = Match(pixels, at, 1, context); var above = at >= width ? Match(pixels, at, width, context) : 0;
                var distance = repeat >= above && repeat >= 4 ? 1 : above >= 4 ? width : 0;
                var length = distance == 1 ? repeat : distance == width ? above : 1;
                result.Add(distance == 0 ? Vp8lToken.Literal(pixels[at]) : Vp8lToken.Copy(length, distance)); at += length;
            }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    internal static long Estimate(ReadOnlySpan<Vp8lToken> tokens, int width, OperationContext context, int cacheBits = 0)
    {
        var layout = new Vp8lHistogramLayout(cacheBits);
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(layout.Total * 4L);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested(); var counts = new int[layout.Total];
            layout.Fill(tokens, width, false, counts, context);
            var cost = layout.Population(counts, context);
            context.Cancellation.ThrowIfCancellationRequested();
            for (var code = 0; code < 24; code++) cost += counts[256 + code] * (long)Vp8lSymbols.Extra(code) * Scale;
            for (var code = 0; code < 40; code++) cost += counts[layout.Distance + code] * (long)Vp8lSymbols.Extra(code) * Scale;
            return cost;
        }
        finally { context.Release(layout.Total * 4L); }
    }

    private ref struct TraceState
    {
        // At forward iteration a, lookahead touches only [a-1, a+8188]:
        // extension runs when reach < a+length-1, next <= reach+1, and both
        // lengths are <=4095. Only the backtracking path survives the sweep.
        // Expire a-2 before Literal(a); its future alias a+8190 is still untouched.
        private const int Window = 8192, Mask = Window - 1;
        private readonly OperationContext context;
        private readonly long reserved;
        private readonly Vp8lHistogramLayout layout;
        private Span<int> histogram, pendingSource;
        private Span<long> prices, lengthPrices, costs, pending;
        private Span<ushort> path;
        private Span<uint> cache;

        internal static long ScratchBytes(int length) => ((length * 2L + 7) & ~7L) + Math.Min(Window, length) * 20L;

        internal TraceState(int length, ReadOnlySpan<Vp8lToken> seed, int width, int cacheBits, Span<int> scratch, OperationContext context)
        {
            this = default;
            this.context = context;
            layout = new(cacheBits);
            var lengths = Math.Min(4095, length);
            var slots = Math.Min(Window, length);
            reserved = layout.Total * 12L + lengths * 8L + layout.CacheSize * 4L + 320
                + (scratch.IsEmpty ? length * 2L + slots * 20L : 0);
            context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(reserved);
            try
            {
                context.Cancellation.ThrowIfCancellationRequested();
                histogram = new int[layout.Total]; prices = new long[layout.Total]; lengthPrices = new long[lengths];
                if (scratch.IsEmpty)
                {
                    costs = new long[slots]; pending = new long[slots]; path = new ushort[length]; pendingSource = new int[slots];
                }
                else
                {
                    // All bytes are already charged to the empty output owner.
                    // Forward work dies before token writes; only path remains live.
                    path = MemoryMarshal.Cast<int, ushort>(scratch)[..length];
                    var offset = (length + 3) / 4 * 2; // Align the long windows.
                    costs = MemoryMarshal.Cast<int, long>(scratch.Slice(offset, slots * 2)); offset += slots * 2;
                    pending = MemoryMarshal.Cast<int, long>(scratch.Slice(offset, slots * 2)); offset += slots * 2;
                    pendingSource = scratch.Slice(offset, slots);
                    for (var at = 0; at < path.Length; at += 4096)
                    { context.Cancellation.ThrowIfCancellationRequested(); path.Slice(at, Math.Min(4096, path.Length - at)).Clear(); }
                    for (var at = 0; at < pendingSource.Length; at += 4096)
                    { context.Cancellation.ThrowIfCancellationRequested(); pendingSource.Slice(at, Math.Min(4096, pendingSource.Length - at)).Clear(); }
                }
                cache = new uint[layout.CacheSize];
                for (var at = 0; at < slots; at += 4096)
                {
                    var count = Math.Min(4096, slots - at); context.Cancellation.ThrowIfCancellationRequested();
                    costs.Slice(at, count).Fill(long.MaxValue); pending.Slice(at, count).Fill(long.MaxValue);
                }
                layout.Fill(seed, width, true, histogram, context);
                PricePlane(0, layout.GreenSize); PricePlane(layout.Red, 256); PricePlane(layout.Blue, 256);
                PricePlane(layout.Alpha, 256); PricePlane(layout.Distance, 40);
                context.Cancellation.ThrowIfCancellationRequested();
                for (var i = 0; i < lengths; i++)
                {
                    var prefix = Vp8lSymbols.Prefix(i);
                    lengthPrices[i] = prices[256 + prefix.Code] + prefix.Extra * Scale;
                }
            }
            catch { context.Release(reserved); throw; }
        }

        private void PricePlane(int start, int length)
        {
            context.Cancellation.ThrowIfCancellationRequested(); var sum = 0; var nonzero = 0;
            for (var i = start; i < start + length; i++) { sum += histogram[i]; if (histogram[i] != 0) nonzero++; }
            if (nonzero <= 1) return;
            var total = Vp8lReferenceEntropy.Log(sum);
            for (var i = start; i < start + length; i++) prices[i] = total - Vp8lReferenceEntropy.Log(histogram[i]);
        }

        internal void Resolve(int at)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var slot = at & Mask;
            if (pending[slot] < costs[slot]) { costs[slot] = pending[slot]; path[at] = (ushort)(at - pendingSource[slot] + 1); }
        }

        internal long CostAt(int at) => costs[at & Mask];
        internal int PathAt(int at) => path[at];
        internal long DistancePrice(int distance, int width)
        {
            var prefix = Vp8lSymbols.Prefix(Vp8lSymbols.Plane(distance, width));
            return prices[layout.Distance + prefix.Code] + prefix.Extra * Scale;
        }

        internal void Literal(ReadOnlySpan<uint> pixels, int at, long prior)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            if (at >= 2)
            {
                var expired = (at - 2) & Mask;
                costs[expired] = pending[expired] = long.MaxValue;
            }
            var pixel = pixels[at]; var hit = false; var key = 0;
            if (layout.CacheBits != 0)
            {
                key = Vp8lReferenceCache.Key(pixel, layout.CacheBits); hit = cache[key] == pixel;
                if (!hit) cache[key] = pixel;
            }
            var price = hit ? prices[280 + key]
                : prices[(int)(pixel >> 8 & 255)] + prices[layout.Red + (int)(pixel >> 16 & 255)]
                    + prices[layout.Blue + (int)(pixel & 255)] + prices[layout.Alpha + (int)(pixel >> 24)];
            var value = prior + (price * (hit ? 68 : 82) + 50) / 100;
            var slot = at & Mask;
            if (value < costs[slot]) { costs[slot] = value; path[at] = 1; }
        }

        internal void Push(int at, int length, long prior)
        {
            for (var i = 0; i < length; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                var value = prior + lengthPrices[i]; var end = at + i; var slot = end & Mask;
                if (length < 10)
                {
                    if (value < costs[slot]) { costs[slot] = value; path[end] = (ushort)(i + 1); }
                }
                else if (value < pending[slot]) { pending[slot] = value; pendingSource[slot] = at; }
            }
        }

        public void Dispose()
        {
            if (histogram.IsEmpty) return;
            histogram = pendingSource = default; prices = lengthPrices = costs = pending = default; path = default; cache = default;
            context.Release(reserved);
        }
    }

    internal static Vp8lTokenBuffer Trace(ReadOnlySpan<uint> pixels, ReadOnlySpan<uint> matches, int width,
        ReadOnlySpan<Vp8lToken> seed, OperationContext context, int cacheBits = 0, Vp8lTokenBuffer? scratch = null)
    {
        // Scratch ownership transfers on entry, including failure. The seed is
        // independently owned and must remain unchanged throughout tracing.
        var result = scratch;
        try
        {
            Validate(pixels.Length, width);
            if (pixels.Length != matches.Length) throw new ArgumentException("Expected matching source/index lengths.");
            result ??= new Vp8lTokenBuffer(context, pixels.Length);
            if (result.OverlapsStorage(seed) || result.OverlapsStorage(pixels) || result.OverlapsStorage(matches))
                throw new ArgumentException("Trace inputs must not alias output capacity.", nameof(seed));
            result.ResetForReuse(context, pixels.Length);
            // Choose from actual capacity before admission; refusal never retries
            // the separate-storage path. All selected candidates stay independent.
            var borrow = TraceState.ScratchBytes(pixels.Length) <= result.CapacityBytes;
            var words = borrow ? result.BorrowScratchWords(context, pixels.Length * 2) : default;
            var output = MemoryMarshal.Cast<int, Vp8lToken>(words);
            var tail = output.Length;
            var state = new TraceState(pixels.Length, seed, width, cacheBits, words, context);
            try
            {
                state.Literal(pixels, 0, 0);
                var previousDistance = -1; var previousLength = -1; var firstConstant = false; var reach = 0; long distancePrice = -1;
                for (var at = 1; at < pixels.Length; at++)
                {
                    state.Literal(pixels, at, state.CostAt(at - 1));
                    var distance = (int)(matches[at] >> 12); var length = (int)(matches[at] & 4095);
                    if (length >= 2)
                    {
                        if (distance != previousDistance)
                        {
                            distancePrice = state.DistancePrice(distance, width); firstConstant = true;
                            state.Push(at, length, state.CostAt(at - 1) + distancePrice);
                        }
                        else
                        {
                            if (firstConstant) { reach = at - 1 + previousLength - 1; firstConstant = false; }
                            if (at + length - 1 > reach)
                            {
                                var next = at; var nextLength = 0;
                                for (; next <= reach; next++)
                                {
                                    context.Cancellation.ThrowIfCancellationRequested(); nextLength = (int)(matches[next + 1] & 4095);
                                    if ((int)(matches[next + 1] >> 12) != distance) { nextLength = (int)(matches[next] & 4095); break; }
                                }
                                state.Resolve(next - 1); state.Resolve(next);
                                state.Push(next, nextLength, state.CostAt(next - 1) + distancePrice); reach = next + nextLength - 1;
                            }
                        }
                    }
                    state.Resolve(at); previousDistance = distance; previousLength = length;
                }
                for (var end = pixels.Length - 1; end >= 0;)
                {
                    context.Cancellation.ThrowIfCancellationRequested(); var length = state.PathAt(end);
                    if (length < 1) throw new InvalidOperationException("Incomplete VP8L parse path.");
                    var start = end - length + 1;
                    var token = length == 1 ? Vp8lToken.Literal(pixels[start]) : Vp8lToken.Copy(length, (int)(matches[start] >> 12));
                    // After k+1 tokens consume S>=k+1 pixels, unread path ends at
                    // 2*(N-S), below this tail write at 8*(N-k-1). Copies only help.
                    if (borrow) output[--tail] = token; else result.Add(token);
                    end -= length;
                }
            }
            finally { state.Dispose(); }
            if (borrow) result.CommitTail(context, tail); else result.Reverse();
            if (cacheBits != 0) Vp8lReferenceCache.Apply(pixels, result, cacheBits, context);
            return result;
        }
        catch { result?.Dispose(); throw; }
    }

    internal static Vp8lTokenBuffer Encode(ReadOnlySpan<uint> pixels, ReadOnlySpan<uint> matches, int width,
        bool traceSearch, OperationContext context)
    {
        Validate(pixels.Length, width);
        Vp8lTokenBuffer? standard = null, repeated = null, trace = null;
        try
        {
            standard = Greedy(pixels, matches, context); repeated = Repeats(pixels, width, context);
            var standardCost = Estimate(standard.Tokens, width, context); var repeatCost = Estimate(repeated.Tokens, width, context);
            if (repeatCost < standardCost) { var result = repeated; repeated = null; return result; }
            if (traceSearch)
            {
                var scratch = repeated; repeated = null;
                trace = Trace(pixels, matches, width, standard.Tokens, context, scratch: scratch);
                if (Estimate(trace.Tokens, width, context) < standardCost) { var result = trace; trace = null; return result; }
            }
            var chosen = standard; standard = null; return chosen;
        }
        finally { standard?.Dispose(); repeated?.Dispose(); trace?.Dispose(); }
    }

    private static void Validate(int length, int width)
    {
        if (width is < 1 or > 16384 || length < 1 || length % width != 0 || length / width > 16384)
            throw new ArgumentException("Expected complete VP8L image rows.");
    }
}
