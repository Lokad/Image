namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    private const int EntropyAlphabet = 2112;
    private const int MaximumGroups = 9;
    private const int MaximumTileSymbols = 320;
    private sealed record EntropyGroups(byte[] Map, WebpPrefixCode MapTable);
    private static ReadOnlySpan<int> ChannelOffsets => [0,1304,1560,1816,2072,2112];

    // RFC9649 section3.7.2.2: one subresolution image selects among independent
    // prefix-code groups. The token parse and transformed source stay unchanged.
    private static CachedPlan SelectEntropyPlan(ImageBuffer image, int effort, uint[] cache, CachedPlan baseline, OperationContext context)
    {
        var columns = (image.Width + 7) >> 3; var rows = (image.Height + 7) >> 3;
        var count = columns * rows;
        var scratch = TileHistograms.StorageBytes(count) + count * 9L + 112
            + MaximumGroups * EntropyAlphabet * 4L + 200_000;
        var work = (long)image.Width * image.Height * 4096 + count * 32768L + 1_000_000;
        if (effort < 4 || count < MaximumGroups || scratch > context.RemainingWorkingBytes || work > context.RemainingWork)
            return baseline;
        var histogram = new TileHistograms(count, context);
        try
        {
            var scan = CachedPixels(image, effort, cache, baseline.Predict, null, null, null, context, histogram);
            context.Reserve(count * 9L + 112); context.Work(0);
            var map = new byte[count]; var ranking = new long[count];
            LearnEntropyMap(histogram, baseline.Tables, map, ranking, context);
            context.Release(count * 8L + 32);
            var groups = CompactGroups(map, context);
            var tables = GroupTables(histogram, map, groups, context);
            long bodyBits = scan.ExtraBits;
            for (var tile = 0; tile < count; tile++)
            {
                context.Work(histogram.Used[tile]);
                bodyBits += histogram.Cost(tile, tables, map[tile], missing: false);
            }
            context.Reserve(280 * 4 + 32); WebpPrefixCode mapTable;
            try
            {
                var frequencies = new int[280]; context.Work(count);
                foreach (var group in map) frequencies[group]++;
                mapTable = new(frequencies, context);
            }
            finally { context.Release(280 * 4 + 32); }
            var candidate = new CachedPlan(tables, bodyBits, baseline.Alpha, baseline.Predict, scan.Literals, new(map, mapTable));
            if (PayloadUpper(image, candidate, context) + 2 < PayloadUpper(image, baseline, context))
            {
                ReleasePlan(baseline, context); return candidate;
            }
            ReleasePlan(candidate, context); return baseline;
        }
        finally { context.Release(TileHistograms.StorageBytes(count)); }
    }

    internal sealed class TileHistograms
    {
        internal readonly byte[] Frequencies;
        internal readonly ushort[] Symbols;
        internal readonly ushort[] Used;
        internal int Count => Used.Length;
        internal static long StorageBytes(int count) => count * (EntropyAlphabet + MaximumTileSymbols * 2L + 2) + 160;
        internal TileHistograms(int count, OperationContext context)
        {
            context.Reserve(StorageBytes(count)); context.Work(0);
            Frequencies = new byte[checked(count * EntropyAlphabet)];
            Symbols = new ushort[checked(count * MaximumTileSymbols)]; Used = new ushort[count];
            context.Work((long)Frequencies.Length + Symbols.Length);
        }
        internal void Add(int tile, int channel, int symbol)
        {
            var key = ChannelOffsets[channel] + symbol; var at = tile * EntropyAlphabet + key;
            if (Frequencies[at] == 0) Symbols[tile * MaximumTileSymbols + Used[tile]++] = (ushort)key;
            Frequencies[at]++;
        }
        internal long Cost(int tile, WebpPrefixCode[] tables, int group, bool missing)
        {
            long result = 0;
            for (var n = 0; n < Used[tile]; n++)
            {
                var key = Symbols[tile * MaximumTileSymbols + n]; var channel = 0;
                while (key >= ChannelOffsets[channel + 1]) channel++;
                var symbol = key - ChannelOffsets[channel]; var table = tables[group * 5 + channel];
                result += (long)Frequencies[tile * EntropyAlphabet + key]
                    * (missing && table.Length(symbol) == 0 ? 15 : table.BitLength(symbol));
            }
            return result;
        }
    }

    private static WebpPrefixCode[] GroupTables(TileHistograms histogram, ReadOnlySpan<byte> map, int groups, OperationContext context)
    {
        var bytes = groups * EntropyAlphabet * 4L + 32; context.Reserve(bytes);
        try
        {
            context.Work(0); var frequencies = new int[groups * EntropyAlphabet];
            context.Work(frequencies.Length);
            for (var tile = 0; tile < map.Length; tile++)
            {
                context.Work(histogram.Used[tile]);
                for (var n = 0; n < histogram.Used[tile]; n++)
                {
                    var key = histogram.Symbols[tile * MaximumTileSymbols + n];
                    frequencies[map[tile] * EntropyAlphabet + key] += histogram.Frequencies[tile * EntropyAlphabet + key];
                }
            }
            context.Reserve(groups * 5 * 8L + 32); context.Work(0);
            var tables = new WebpPrefixCode[groups * 5];
            for (var group = 0; group < groups; group++)
            for (var channel = 0; channel < 5; channel++)
                tables[group * 5 + channel] = new(frequencies.AsSpan(group * EntropyAlphabet + ChannelOffsets[channel],
                    ChannelOffsets[channel + 1] - ChannelOffsets[channel]), context);
            return tables;
        }
        finally { context.Release(bytes); }
    }

    internal static void LearnEntropyMap(TileHistograms histogram, WebpPrefixCode[] baseline,
        Span<byte> map, Span<long> ranking, OperationContext context)
    {
        if (map.Length != histogram.Count || ranking.Length != histogram.Count) throw new ArgumentException("Entropy map dimensions differ.");
        var used = 0;
        for (var tile = 0; tile < histogram.Count; tile++)
        {
            context.Work(histogram.Used[tile]);
            if (histogram.Used[tile] != 0) ranking[used++] = (histogram.Cost(tile, baseline, 0, missing: false) << 32) | (uint)tile;
        }
        context.Work((long)used * 32); ranking[..used].Sort();
        for (var rank = 0; rank < used; rank++) map[(int)(ranking[rank] & uint.MaxValue)] = (byte)(rank * 9L / used);
        // Exactly four coding-cost assignments, with deterministic lower-id ties.
        // No data/quality/effort search or independent allowance from root capacity.
        for (var pass = 0; pass < 4; pass++)
        {
            var tables = GroupTables(histogram, map, MaximumGroups, context);
            try
            {
                for (var tile = 0; tile < histogram.Count; tile++)
                {
                    context.Work(histogram.Used[tile] * 9L);
                    if (histogram.Used[tile] == 0)
                    {
                        // Empty tiles have no token start and cannot affect
                        // pixels. Keeping the previous id needs no width state.
                        continue;
                    }
                    long best = long.MaxValue; var selected = 0;
                    for (var group = 0; group < MaximumGroups; group++)
                    {
                        var cost = histogram.Cost(tile, tables, group, missing: true);
                        if (cost < best) { best = cost; selected = group; }
                    }
                    map[tile] = (byte)selected;
                }
            }
            finally { ReleaseTables(tables, context); }
        }
    }

    private static void ReleaseTables(WebpPrefixCode[] tables, OperationContext context)
    {
        long bytes = tables.Length * 8L + 32;
        foreach (var table in tables) bytes += table.MemoryBytes;
        context.Release(bytes);
    }

    private static int CompactGroups(Span<byte> map, OperationContext context)
    {
        Span<byte> ids = stackalloc byte[MaximumGroups]; ids.Fill(255); var groups = 0;
        context.Work(map.Length * 2L);
        foreach (var id in map) ids[id] = 0;
        for (var id = 0; id < ids.Length; id++) if (ids[id] == 0) ids[id] = (byte)groups++;
        for (var tile = 0; tile < map.Length; tile++) map[tile] = ids[map[tile]];
        return groups;
    }

    private static void EntropyHeader(EntropyGroups groups, WebpBitWriter writer, OperationContext context)
    {
        writer.Write(1, 1); writer.Write(1, 3); // Spatial groups,8x8 tiles.
        writer.Write(0, 1); // Auxiliary image: no cache and no spatial-group flag.
        groups.MapTable.Header(writer, context); SingleHeader(writer, 0); SingleHeader(writer, 0);
        SingleHeader(writer, 255); SingleHeader(writer, 0);
        foreach (var group in groups.Map) groups.MapTable.Write(writer, group);
    }
}
