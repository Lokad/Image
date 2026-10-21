using System.Numerics;

namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    private readonly record struct SeededMatch(ReferenceToken Token, int End, int Distance);

    // RFC9649 section3.6.2: fit copy probabilities from possible references
    // before pricing them. Previously unused distance codes need an actual model.
    private static CachedPlan SelectSeededPlan(ImageBuffer image, int effort, uint[] cache,
        CachedPlan baseline, OperationContext context)
    {
        if (effort < 4 || !baseline.Predict || !baseline.UseCache || baseline.Modes is not null
            || baseline.Color is not null || baseline.Groups is null || baseline.References is not null)
            return baseline;
        var count = image.Width * image.Height;
        if (count < 3) return baseline;
        var history = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Min(count, 262144));
        var heads = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Min(count, 65536));
        var window = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Min(count, 262144 + 1024 + 3));
        var groups = baseline.Tables.Length / 5;
        var fixedBytes = (history + heads + (long)window) * 4 + groups * (1344L * 4 + 5 * 8) + 2048;
        var tiles = baseline.Groups.Map.Length;
        var fitBytes = TileHistograms.StorageBytes(tiles) + tiles * 9L + 112
            + MaximumGroups * EntropyAlphabet * 4L + 200_000;
        if (fixedBytes + count * 40L + 8 + fitBytes + CachedWorkingBytes > context.RemainingWorkingBytes
            || count * 8192L + tiles * 32768L + 1_000_000 > context.RemainingWork) return baseline;
        var references = BuildSeededReferences(image, effort, cache, baseline, history, heads, window, fixedBytes, context);
        if (references is null) return baseline;
        var retained = false;
        try
        {
            var candidate = PreparePlanTables(image, effort, cache, true, context,
                null, null, null, true, references) with { Alpha = baseline.Alpha };
            candidate = SelectEntropyPlan(image, effort, cache, candidate, context);
            if (PayloadUpper(image, candidate, context) + 2 < PayloadUpper(image, baseline, context))
            {
                ReleaseCodingPlan(baseline, context); retained = true; return candidate;
            }
            ReleaseCodingPlan(candidate, context); return baseline;
        }
        finally { if (!retained) context.Release(references.MemoryBytes); }
    }

    private static ReferencePlan? BuildSeededReferences(ImageBuffer image, int effort, uint[] cache,
        CachedPlan baseline, int history, int heads, int windowSize, long fixedBytes, OperationContext context)
    {
        var scan = WalkCachedPixels(image, effort, cache, true, null, null, null, context,
            null, null, null, null, true, null);
        var scratch = fixedBytes + scan.Tokens * 40L + 8;
        context.Reserve(scratch);
        WebpPrefixCode[]? models = null; long retained = 0;
        try
        {
            context.Work(0);
            var original = new ReferenceToken[scan.Tokens]; var result = new ReferenceToken[scan.Tokens];
            var prefix = new long[scan.Tokens + 1]; var head = new int[heads]; var previous = new int[history];
            var window = new SeededWindow(image, windowSize, context);
            var groups = baseline.Tables.Length / 5; var frequencies = new int[groups * 1344];
            models = new WebpPrefixCode[groups * 5];
            WalkCachedPixels(image, effort, cache, true, null, null, null, context,
                null, null, null, null, true, original);
            foreach (var token in original)
                AddSeedFrequency(token, SeededGroup(image.Width, baseline.Groups!, token.Position), frequencies, context);
            Span<SeededMatch> matches = stackalloc SeededMatch[16];
            var used = 0; var target = 0; var changed = false;
            for (var pass = 0; pass < 2; pass++)
            {
                context.Work(head.Length + (long)previous.Length); head.AsSpan().Clear(); previous.AsSpan().Clear();
                window.Rewind(); var cursor = 0;
                for (var index = 0; index < original.Length; index++)
                {
                    var at = original[index].Position;
                    if (pass == 0 || index >= target)
                    {
                        var n = SeededMatches(window, image.Width, original, index, head, previous, matches, context);
                        if (pass == 0 && n != 0)
                        {
                            var best = matches[0];
                            for (var i = 1; i < n; i++)
                                if (matches[i].Token.Length > best.Token.Length
                                    || matches[i].Token.Length == best.Token.Length && matches[i].Distance < best.Distance)
                                    best = matches[i];
                            // Original copies carry a captured pixel, new copies do
                            // not. Encoded length/distance alone identify equality.
                            if (best.Token.Length != original[index].Length || best.Token.Distance != original[index].Distance)
                                AddSeedFrequency(best.Token, SeededGroup(image.Width, baseline.Groups!, at), frequencies, context);
                        }
                        else if (pass == 1)
                        {
                            var token = original[index]; target = index + 1; long gain = 0;
                            var offset = SeededGroup(image.Width, baseline.Groups!, at) * 5;
                            for (var i = 0; i < n; i++)
                            {
                                var saving = prefix[matches[i].End] - prefix[index]
                                    - SeededPrice(matches[i].Token, models, offset, context);
                                if (saving > gain) { gain = saving; token = matches[i].Token; target = matches[i].End; }
                            }
                            result[used++] = token; changed |= gain != 0;
                        }
                    }
                    var until = index + 1 == original.Length ? window.Count : original[index + 1].Position;
                    while (cursor < until)
                    {
                        context.Work(1);
                        if (cursor <= window.Count - 3)
                        {
                            var hash = SeededHash(window, cursor) & (head.Length - 1);
                            previous[cursor & (previous.Length - 1)] = head[hash]; head[hash] = cursor + 1;
                        }
                        cursor++;
                    }
                }
                if (pass == 0)
                {
                    for (var g = 0; g < groups; g++)
                    {
                        models[g * 5] = new(frequencies.AsSpan(g * 1344, 1304), context);
                        Array.Copy(baseline.Tables, g * 5 + 1, models, g * 5 + 1, 3); // BORROWED channels.
                        models[g * 5 + 4] = new(frequencies.AsSpan(g * 1344 + 1304, 40), context);
                    }
                    for (var i = 0; i < original.Length; i++)
                        prefix[i + 1] = prefix[i] + SeededPrice(original[i], models,
                            SeededGroup(image.Width, baseline.Groups!, original[i].Position) * 5, context);
                }
            }
            if (!changed) return null;
            var references = new ReferencePlan(result, used, baseline.Alpha);
            retained = references.MemoryBytes; return references;
        }
        finally
        {
            // Release ONLY the new G/distance trees; R/B/A still belong to the
            // live baseline. Search/seed scratch ends before subsequent groupfit.
            if (models is not null)
                for (var g = 0; g < models.Length; g += 5)
                {
                    if (models[g] is { } green) context.Release(green.MemoryBytes);
                    if (models[g + 4] is { } distance) context.Release(distance.MemoryBytes);
                }
            context.Release(scratch - retained);
        }
    }

    private static int SeededGroup(int width, EntropyGroups groups, int at)
        => groups.Map[(at / width >> 3) * ((width + 7) >> 3) + (at % width >> 3)];

    private static void AddSeedFrequency(ReferenceToken token, int group, int[] frequencies, OperationContext context)
    {
        context.Work(8); var at = group * 1344;
        if (token.Length > 1)
        {
            Prefix(token.Length, out var code, out _, out _); frequencies[at + 256 + code]++;
            ReferencePrefix(token.Distance, out code, out _, out _); frequencies[at + 1304 + code]++;
        }
        else frequencies[at + (token.Distance < 0 ? 279 - token.Distance : (int)(token.Pixel >> 8 & 255))]++;
    }

    private static long SeededPrice(ReferenceToken token, WebpPrefixCode[] tables, int offset, OperationContext context)
    {
        context.Work(8);
        static int Cost(WebpPrefixCode table, int symbol) => table.Length(symbol) == 0 ? 15 : table.BitLength(symbol);
        if (token.Length > 1)
        {
            Prefix(token.Length, out var code, out _, out var bits);
            ReferencePrefix(token.Distance, out var distance, out _, out var distanceBits);
            return Cost(tables[offset], 256 + code) + bits + Cost(tables[offset + 4], distance) + distanceBits;
        }
        if (token.Distance < 0) return Cost(tables[offset], 279 - token.Distance);
        var p = token.Pixel;
        return Cost(tables[offset], (int)(p >> 8 & 255)) + Cost(tables[offset + 1], (int)(p >> 16 & 255))
            + Cost(tables[offset + 2], (int)(p & 255)) + Cost(tables[offset + 3], (int)(p >> 24));
    }

    private static int SeededMatches(SeededWindow window, int width, ReferenceToken[] original, int index,
        int[] head, int[] previous, Span<SeededMatch> matches, OperationContext context)
    {
        var at = original[index].Position; var used = 0; var matching = 0;
        if (at > window.Count - 3) return 0;
        var candidate = head[SeededHash(window, at) & (head.Length - 1)] - 1;
        for (var visited = 0; visited < 256 && candidate >= 0 && at - candidate <= 262144; visited++)
        {
            if (window.Get(at) == window.Get(candidate) && window.Get(at + 1) == window.Get(candidate + 1)
                && window.Get(at + 2) == window.Get(candidate + 2))
            {
                var length = 3; var maximum = Math.Min(1024, window.Count - at);
                while (length < maximum && window.Get(at + length) == window.Get(candidate + length)) length++;
                var end = ReferenceEndpoint(original, window.Count, at + length, context);
                var span = (end == original.Length ? window.Count : original[end].Position) - at;
                if (end > index && span >= 3)
                    matches[used++] = new(new(at, span, ReferenceDistance(at - candidate, width, context), 0), end, at - candidate);
                if (++matching == 16) break;
            }
            candidate = previous[candidate & (previous.Length - 1)] - 1;
        }
        return used;
    }

    private static int SeededHash(SeededWindow window, int at)
        => unchecked((int)((((2166136261u ^ window.Get(at)) * 16777619 ^ window.Get(at + 1))
            * 16777619 ^ window.Get(at + 2)) * 16777619));

    private sealed class SeededWindow
    {
        private readonly ImageBuffer image;
        private readonly OperationContext context;
        private readonly uint[] pixels;
        private int filled;
        internal int Count => image.Width * image.Height;
        internal SeededWindow(ImageBuffer image, int capacity, OperationContext context)
        { this.image = image; this.context = context; pixels = new uint[capacity]; }
        internal void Rewind() => filled = 0;
        internal uint Get(int at)
        {
            context.Work(1);
            if ((uint)at >= Count || at < filled - pixels.Length)
                throw new InvalidOperationException("Reference lookahead/history bounds differ.");
            while (filled <= at)
            {
                pixels[filled & (pixels.Length - 1)] = LeftResidual(image, filled, context); filled++;
            }
            return pixels[at & (pixels.Length - 1)];
        }
    }
}
