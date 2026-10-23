namespace Lokad.Image.Internal;

// Auxiliary-image parsing without a color cache. All candidates borrow the same
// pixels/matches; only the chosen owned token buffer escapes, with capacity live.
internal static class Vp8lReferenceLz
{
    private const int GreenSize = 280;
    private const int Red = 280, Blue = 536, Alpha = 792, Distance = 1048, HistogramSize = 1088;
    private const long Scale = Vp8lReferenceCost.Scale;

    internal static Vp8lTokenBuffer Greedy(ReadOnlySpan<uint> pixels, ReadOnlySpan<uint> matches, OperationContext context)
    {
        if (pixels.IsEmpty || pixels.Length != matches.Length) throw new ArgumentException("Expected matching source/index lengths.");
        var result = new Vp8lTokenBuffer(context, pixels.Length);
        try
        {
            var at = 0;
            while (at < pixels.Length)
            {
                context.Work(1);
                var length = (int)(matches[at] & 4095); var distance = (int)(matches[at] >> 12);
                if (length >= 4)
                {
                    var farthest = 0; var stop = Math.Min(at + length, pixels.Length - 1);
                    for (var next = at + 1; next <= stop; next++)
                    {
                        context.Work(1);
                        var span = (int)(matches[next] & 4095); var reach = next + (span >= 4 ? span : 1);
                        if (reach > farthest) { length = next - at; farthest = reach; if (reach >= pixels.Length) break; }
                    }
                }
                else length = 1;
                result.Add(length == 1 ? Vp8lToken.Literal(pixels[at]) : Vp8lToken.Copy(length, distance)); at += length;
            }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    private static int Match(ReadOnlySpan<uint> pixels, int at, int distance, OperationContext context)
    {
        var length = 0; var maximum = Math.Min(4095, pixels.Length - at);
        while (length < maximum)
        {
            context.Work(1);
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

    private static void Histogram(ReadOnlySpan<Vp8lToken> tokens, int width, bool plane, Span<int> counts, OperationContext context)
    {
        foreach (var token in tokens)
        {
            context.Work(4);
            if (token.Code > 0)
            {
                counts[256 + Vp8lSymbols.Prefix(token.Code).Code]++;
                counts[Distance + Vp8lSymbols.Prefix(plane ? Vp8lSymbols.Plane((int)token.Value, width) : (int)token.Value).Code]++;
            }
            else if (token.Code == 0)
            {
                var pixel = token.Value;
                counts[(int)(pixel >> 8 & 255)]++; counts[Red + (int)(pixel >> 16 & 255)]++;
                counts[Blue + (int)(pixel & 255)]++; counts[Alpha + (int)(pixel >> 24)]++;
            }
            else throw new InvalidOperationException("Uncached histogram received a cache token.");
        }
    }

    internal static long Estimate(ReadOnlySpan<Vp8lToken> tokens, int width, OperationContext context)
    {
        context.Work(0); context.Reserve(HistogramSize * 4);
        try
        {
            context.Work(0); var counts = new int[HistogramSize];
            Histogram(tokens, width, false, counts, context);
            var cost = Vp8lReferenceEntropy.Population(counts.AsSpan(0, GreenSize), context)
                + Vp8lReferenceEntropy.Population(counts.AsSpan(Red, 256), context)
                + Vp8lReferenceEntropy.Population(counts.AsSpan(Blue, 256), context)
                + Vp8lReferenceEntropy.Population(counts.AsSpan(Alpha, 256), context)
                + Vp8lReferenceEntropy.Population(counts.AsSpan(Distance, 40), context);
            context.Work(64);
            for (var code = 0; code < 24; code++) cost += counts[256 + code] * (long)Vp8lSymbols.Extra(code) * Scale;
            for (var code = 0; code < 40; code++) cost += counts[Distance + code] * (long)Vp8lSymbols.Extra(code) * Scale;
            return cost;
        }
        finally { context.Release(HistogramSize * 4); }
    }

    private sealed class TraceState : IDisposable
    {
        private readonly OperationContext context;
        private readonly long reserved;
        private int[]? histogram;
        private long[]? prices, lengthPrices, costs, pending;
        private ushort[]? path;
        private int[]? pendingSource;

        internal TraceState(int length, ReadOnlySpan<Vp8lToken> seed, int width, OperationContext context)
        {
            this.context = context;
            var lengths = Math.Min(4095, length);
            reserved = HistogramSize * 12L + lengths * 8L + length * 22L + 256;
            context.Work(0); context.Reserve(reserved);
            try
            {
                context.Work(0);
                histogram = new int[HistogramSize]; prices = new long[HistogramSize]; lengthPrices = new long[lengths];
                costs = new long[length]; pending = new long[length]; path = new ushort[length]; pendingSource = new int[length];
                for (var at = 0; at < length; at += 4096)
                {
                    var count = Math.Min(4096, length - at); context.Work(count * 2L);
                    costs.AsSpan(at, count).Fill(long.MaxValue); pending.AsSpan(at, count).Fill(long.MaxValue);
                }
                Histogram(seed, width, true, histogram, context);
                PricePlane(0, GreenSize); PricePlane(Red, 256); PricePlane(Blue, 256); PricePlane(Alpha, 256); PricePlane(Distance, 40);
                context.Work(lengths);
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
            context.Work(length * 2L); var sum = 0; var nonzero = 0;
            for (var i = start; i < start + length; i++) { sum += histogram![i]; if (histogram[i] != 0) nonzero++; }
            if (nonzero <= 1) return;
            var total = Vp8lReferenceEntropy.Log(sum);
            for (var i = start; i < start + length; i++) prices![i] = total - Vp8lReferenceEntropy.Log(histogram![i]);
        }

        internal void Resolve(int at)
        {
            context.Work(1);
            if (pending![at] < costs![at]) { costs[at] = pending[at]; path![at] = (ushort)(at - pendingSource![at] + 1); }
        }

        internal long CostAt(int at) => costs![at];
        internal int PathAt(int at) => path![at];
        internal long DistancePrice(int distance, int width)
        {
            var prefix = Vp8lSymbols.Prefix(Vp8lSymbols.Plane(distance, width));
            return prices![Distance + prefix.Code] + prefix.Extra * Scale;
        }

        internal void Literal(ReadOnlySpan<uint> pixels, int at, long prior)
        {
            context.Work(4);
            var pixel = pixels[at];
            var price = prices![(int)(pixel >> 8 & 255)] + prices[Red + (int)(pixel >> 16 & 255)]
                + prices[Blue + (int)(pixel & 255)] + prices[Alpha + (int)(pixel >> 24)];
            var value = prior + (price * 82 + 50) / 100;
            if (value < costs![at]) { costs[at] = value; path![at] = 1; }
        }

        internal void Push(int at, int length, long prior)
        {
            for (var i = 0; i < length; i++)
            {
                context.Work(1);
                var value = prior + lengthPrices![i]; var end = at + i;
                if (length < 10)
                {
                    if (value < costs![end]) { costs[end] = value; path![end] = (ushort)(i + 1); }
                }
                else if (value < pending![end]) { pending[end] = value; pendingSource![end] = at; }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref histogram, null) is null) return;
            prices = lengthPrices = costs = pending = null; path = null; pendingSource = null;
            context.Release(reserved);
        }
    }

    internal static Vp8lTokenBuffer Trace(ReadOnlySpan<uint> pixels, ReadOnlySpan<uint> matches, int width,
        ReadOnlySpan<Vp8lToken> seed, OperationContext context)
    {
        Validate(pixels.Length, width);
        if (pixels.Length != matches.Length) throw new ArgumentException("Expected matching source/index lengths.");
        var result = new Vp8lTokenBuffer(context, pixels.Length);
        try
        {
            using var state = new TraceState(pixels.Length, seed, width, context);
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
                                context.Work(1); nextLength = (int)(matches[next + 1] & 4095);
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
                context.Work(1); var length = state.PathAt(end);
                if (length < 1) throw new InvalidOperationException("Incomplete VP8L parse path.");
                var start = end - length + 1;
                result.Add(length == 1 ? Vp8lToken.Literal(pixels[start]) : Vp8lToken.Copy(length, (int)(matches[start] >> 12)));
                end -= length;
            }
            result.Reverse(); return result;
        }
        catch { result.Dispose(); throw; }
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
            repeated.Dispose(); repeated = null;
            if (traceSearch)
            {
                trace = Trace(pixels, matches, width, standard.Tokens, context);
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
