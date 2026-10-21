using System.Numerics;

namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    // RFC9649 section3.6.2: Distance is the encoded plane value for a copy,
    // zero for a literal, or -1-cacheIndex. Position/Length retain token boundaries.
    private readonly record struct ReferenceToken(int Position, int Length, int Distance, uint Pixel);
    private sealed record ReferencePlan(ReferenceToken[] Tokens, int Count, bool Alpha)
    {
        internal long MemoryBytes => Tokens.Length * 16L + 64;
    }

    private static CachedPlan SelectReferencePlan(ImageBuffer image, int effort, uint[] cache,
        CachedPlan baseline, OperationContext context)
    {
        var count = image.Width * image.Height;
        // The demonstrated opportunity is repeated raw/cache patterns. Predicted
        // residual plans keep their independently qualified coding strategy.
        if (effort < 4 || baseline.Predict || !baseline.UseCache || baseline.References is not null
            || baseline.Groups is not null || count < 4) return baseline;
        var history = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Min(count, 262144));
        var heads = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Min(count, 65536));
        var fixedBytes = (history + heads) * 4L + 224;
        if (fixedBytes + count * 40L + CachedWorkingBytes > context.RemainingWorkingBytes
            || count * 2048L + 1_000_000 > context.RemainingWork) return baseline;
        var scan = WalkCachedPixels(image, effort, cache, false, null, null, null, context,
            null, null, null, null, true, null);
        var scratch = fixedBytes + scan.Tokens * 40L + 8;
        context.Reserve(scratch);
        var retained = 0L;
        try
        {
            context.Work(0);
            var original = new ReferenceToken[scan.Tokens];
            var result = new ReferenceToken[scan.Tokens];
            var prefix = new long[scan.Tokens + 1];
            var head = new int[heads]; var previous = new int[history];
            context.Work(heads + (long)history);
            WalkCachedPixels(image, effort, cache, false, null, null, null, context,
                null, null, null, null, true, original);
            for (var i = 0; i < original.Length; i++)
                prefix[i + 1] = prefix[i] + ReferencePrice(original[i], baseline.Tables, context);
            var cursor = 0; var used = 0; long saved = 0;
            for (var index = 0; index < original.Length;)
            {
                context.Work(1);
                var at = original[index].Position; var end = index + 1;
                var chosen = original[index]; long gain = 0;
                if (at <= count - 4)
                {
                    var key = ReferenceKey(image, at, context);
                    var candidate = head[ReferenceHash(key) & (heads - 1)] - 1;
                    var matching = 0;
                    // Bound unrelated hash collisions separately from the sixteen
                    // matching four-pixel keys in the frozen-cost recipe.
                    for (var visited = 0; visited < 256 && candidate >= 0 && at - candidate <= 262144; visited++)
                    {
                        if (ReferenceKey(image, candidate, context) == key)
                        {
                            var length = 4; var maximum = Math.Min(1024, count - at);
                            while (length < maximum && GreenPixel(image, at + length, context)
                                == GreenPixel(image, candidate + length, context)) length++;
                            var endpoint = ReferenceEndpoint(original, count, at + length, context);
                            var span = (endpoint == original.Length ? count : original[endpoint].Position) - at;
                            if (endpoint > index && span >= 4)
                            {
                                var token = new ReferenceToken(at, span,
                                    ReferenceDistance(at - candidate, image.Width, context), 0);
                                var saving = prefix[endpoint] - prefix[index]
                                    - ReferencePrice(token, baseline.Tables, context);
                                if (saving > gain) { chosen = token; gain = saving; end = endpoint; }
                            }
                            if (++matching == 16) break;
                        }
                        candidate = previous[candidate & (history - 1)] - 1;
                    }
                }
                result[used++] = chosen; saved += gain;
                var until = end == original.Length ? count : original[end].Position;
                while (cursor < until)
                {
                    if (cursor <= count - 4)
                    {
                        var hash = ReferenceHash(ReferenceKey(image, cursor, context)) & (heads - 1);
                        previous[cursor & (history - 1)] = head[hash]; head[hash] = cursor + 1;
                    }
                    cursor++;
                }
                index = end;
            }
            if (saved == 0) return baseline;
            var references = new ReferencePlan(result, used, baseline.Alpha);
            var candidatePlan = PreparePlanTables(image, effort, cache, false, context,
                null, null, null, true, references);
            if (PayloadUpper(image, candidatePlan, context) + 2 < PayloadUpper(image, baseline, context))
            {
                ReleaseCodingPlan(baseline, context); retained = references.MemoryBytes; return candidatePlan;
            }
            ReleaseCodingPlan(candidatePlan, context); return baseline;
        }
        finally { context.Release(scratch - retained); }
    }

    private static (uint, uint, uint, uint) ReferenceKey(ImageBuffer image, int at, OperationContext context)
        => (GreenPixel(image, at, context), GreenPixel(image, at + 1, context),
            GreenPixel(image, at + 2, context), GreenPixel(image, at + 3, context));

    private static int ReferenceHash((uint A, uint B, uint C, uint D) key)
        => unchecked((int)(((((2166136261u ^ key.A) * 16777619 ^ key.B) * 16777619 ^ key.C)
            * 16777619 ^ key.D) * 16777619));

    private static int ReferenceEndpoint(ReferenceToken[] tokens, int pixels, int limit, OperationContext context)
    {
        var low = 0; var high = tokens.Length + 1;
        while (low < high)
        {
            context.Work(1); var middle = low + (high - low) / 2;
            if ((middle == tokens.Length ? pixels : tokens[middle].Position) <= limit) low = middle + 1;
            else high = middle;
        }
        return low - 1;
    }

    private static int ReferenceDistance(int distance, int width, OperationContext context)
    {
        // Reuse the specification-derived geometry, preferring the first plane
        // value when narrow widths make several neighbors resolve identically.
        context.Work(120);
        for (var value = 1; value <= 120; value++)
            if (WebpDistances.Resolve(value, width) == distance) return value;
        return distance + 120;
    }

    private static void ReferencePrefix(int value, out int code, out int extra, out int bits)
    {
        if (value <= 4) { code = value - 1; extra = bits = 0; return; }
        var log = BitOperations.Log2((uint)(value - 1)); bits = log - 1;
        code = log * 2 + (((value - 1) >> bits) & 1);
        extra = value - (((2 + (code & 1)) << bits) + 1);
    }

    private static long ReferencePrice(ReferenceToken token, WebpPrefixCode[] tables,
        OperationContext context)
    {
        context.Work(8);
        static int Cost(WebpPrefixCode table, int symbol) => table.Length(symbol) == 0 ? 15 : table.BitLength(symbol);
        if (token.Length > 1)
        {
            Prefix(token.Length, out var code, out _, out var bits);
            ReferencePrefix(token.Distance, out var distance, out _, out var distanceBits);
            return Cost(tables[0], 256 + code) + bits + Cost(tables[4], distance) + distanceBits;
        }
        if (token.Distance < 0) return Cost(tables[0], 279 - token.Distance);
        var p = token.Pixel;
        return Cost(tables[0], (int)(p >> 8 & 255)) + Cost(tables[1], (int)(p >> 16 & 255))
            + Cost(tables[2], (int)(p & 255)) + Cost(tables[3], (int)(p >> 24));
    }

    private static CachedScan ReferencePixels(ImageBuffer image, ReferencePlan references,
        WebpBitWriter? writer, WebpPrefixCode[]? tables, int[][]? frequencies, OperationContext context,
        TileHistograms? histograms = null, EntropyGroups? groups = null)
    {
        var literals = 0; long extraBits = 0;
        for (var i = 0; i < references.Count; i++)
        {
            var token = references.Tokens[i]; context.Work(token.Length);
            var tile = (token.Position / image.Width >> 3) * ((image.Width + 7) >> 3) + (token.Position % image.Width >> 3);
            var offset = groups is null ? 0 : groups.Map[tile] * 5;
            if (token.Length > 1)
            {
                Prefix(token.Length, out var code, out var extra, out var bits);
                CachedSymbol(0, 256 + code, writer, tables, frequencies, context, offset, histograms, tile);
                if (writer is not null) writer.Write(extra, bits); else context.Work(1);
                extraBits += bits;
                ReferencePrefix(token.Distance, out code, out extra, out bits);
                CachedSymbol(4, code, writer, tables, frequencies, context, offset, histograms, tile);
                if (writer is not null) writer.Write(extra, bits); else context.Work(1);
                extraBits += bits;
            }
            else if (token.Distance < 0)
                CachedSymbol(0, 279 - token.Distance, writer, tables, frequencies, context, offset, histograms, tile);
            else
            {
                var p = token.Pixel; literals++;
                CachedSymbol(0, (int)(p >> 8 & 255), writer, tables, frequencies, context, offset, histograms, tile);
                CachedSymbol(1, (int)(p >> 16 & 255), writer, tables, frequencies, context, offset, histograms, tile);
                CachedSymbol(2, (int)(p & 255), writer, tables, frequencies, context, offset, histograms, tile);
                CachedSymbol(3, (int)(p >> 24), writer, tables, frequencies, context, offset, histograms, tile);
            }
        }
        return new(references.Alpha, literals, extraBits, references.Count);
    }
}
