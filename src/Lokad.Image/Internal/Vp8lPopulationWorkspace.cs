namespace Lokad.Image.Internal;

// Operation-owned immutable sparse leaves and reusable dense merged populations.
// Logical tile identities are independent of physical dense slots. Every owned
// capacity remains admitted until disposal, including idle recycled slots.
internal sealed class Vp8lPopulationWorkspace : IDisposable
{
    private readonly OperationContext context;
    private readonly Vp8lHistogramLayout layout;
    private readonly int tiles, supportWords, blueSupport, alphaSupport;
    private int[]? offsets, denseMap, freeSlots, firstScratch, secondScratch;
    private uint[]? entries;
    private int[][]? blocks;
    private ulong[][]? supports;
    private ulong[]? firstSupport, secondSupport;
    private int firstTile = -1, secondTile = -1, allocated, freeCount;
    private long reserved;

    private Vp8lPopulationWorkspace(int tiles, Vp8lHistogramLayout layout, int supportWords, OperationContext context)
    {
        this.tiles = tiles; this.layout = layout; this.supportWords = supportWords; this.context = context;
        blueSupport = layout.RedSize == 256 ? 4 : 0;
        alphaSupport = blueSupport + (layout.BlueSize == 256 ? 4 : 0);
        // Every materialized live cluster contains at least two distinct leaves.
        var capacity = tiles / 2;
        var bytes = (tiles + 1L + tiles + capacity + 2L * layout.Total) * sizeof(int)
            + 2L * capacity * IntPtr.Size + 2L * supportWords * sizeof(ulong) + 512;
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(bytes); reserved = bytes;
        try
        {
            context.Cancellation.ThrowIfCancellationRequested();
            offsets = new int[tiles + 1]; denseMap = new int[tiles]; denseMap.AsSpan().Fill(-1);
            freeSlots = new int[capacity]; blocks = new int[capacity][]; supports = new ulong[capacity][];
            firstScratch = new int[layout.Total]; secondScratch = new int[layout.Total];
            firstSupport = new ulong[supportWords]; secondSupport = new ulong[supportWords];
        }
        catch { Dispose(); throw; }
    }

    internal static Vp8lPopulationWorkspace Create(ReadOnlySpan<Vp8lToken> tokens, int width, int height,
        int bits, Vp8lHistogramLayout layout, int supportWords, OperationContext context)
    {
        var columns = (width + (1 << bits) - 1) >> bits;
        var tiles = columns * ((height + (1 << bits) - 1) >> bits);
        var result = new Vp8lPopulationWorkspace(tiles, layout, supportWords, context);
        try { result.Prepare(tokens, width, height, bits, columns); return result; }
        catch { result.Dispose(); throw; }
    }

    private void Prepare(ReadOnlySpan<Vp8lToken> tokens, int width, int height, int bits, int columns)
    {
        // Row indexes live only through the two preparation passes. One admitted
        // scratch population supplies exact nonzero sizes before CSR allocation.
        var cells = columns * height; var indexBytes = cells * 8L + 64;
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(indexBytes);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var rowTokens = new int[cells]; rowTokens.AsSpan().Fill(-1); var rowPositions = new int[cells];
            var position = 0; var lastCell = -1;
            for (var i = 0; i < tokens.Length; i++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                if (position >= (long)width * height || tokens[i].Length < 1 || tokens[i].Length > (long)width * height - position)
                    throw new ArgumentException("Tokens exceed their image.", nameof(tokens));
                var row = position / width; var column = position % width >> bits;
                var cell = row * columns + column;
                offsets![(row >> bits) * columns + column + 1] = 1;
                if (cell != lastCell) { rowTokens[cell] = i; rowPositions[cell] = position; lastCell = cell; }
                position += tokens[i].Length;
            }
            if (position != width * height) throw new ArgumentException("Tokens do not cover their image.", nameof(tokens));
            var count = 0; var activeCount = 0;
            for (var tile = 0; tile < tiles; tile++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                // The marker for this tile is consumed before its prefix offset
                // replaces it. No later tile marker is overwritten.
                if (offsets![tile + 1] != 0)
                {
                    Replay(tokens, width, height, bits, columns, tile, rowTokens, rowPositions, ref activeCount);
                    count += activeCount;
                }
                offsets[tile + 1] = count;
            }
            var entryBytes = count * 4L + 32;
            context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(entryBytes); reserved += entryBytes;
            context.Cancellation.ThrowIfCancellationRequested(); entries = new uint[count];
            for (var tile = 0; tile < tiles; tile++)
            {
                context.Cancellation.ThrowIfCancellationRequested();
                if (offsets![tile] == offsets[tile + 1]) continue;
                Replay(tokens, width, height, bits, columns, tile, rowTokens, rowPositions, ref activeCount);
                var at = offsets[tile];
                var symbols = secondScratch.AsSpan(0, activeCount); symbols.Sort();
                foreach (var symbol in symbols)
                {
                    var value = firstScratch![symbol];
                    // RFC alphabets use <=2112 words. A leaf spans at most
                    // 512x512 pixels: 12 index bits and 20 count bits suffice.
                    if (value != 0) entries[at++] = (uint)value << 12 | (uint)symbol;
                }
                if (at != offsets[tile + 1]) throw new InvalidOperationException("Leaf population changed during preparation.");
            }
        }
        finally { context.Release(indexBytes); }
    }

    private void Replay(ReadOnlySpan<Vp8lToken> tokens, int width, int height, int bits, int columns, int tile,
        ReadOnlySpan<int> rowTokens, ReadOnlySpan<int> rowPositions, ref int activeCount)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        // The second decode buffer is idle throughout leaf preparation. Use it
        // to clear/count only the symbols touched by the previous leaf.
        for (var i = 0; i < activeCount; i++) firstScratch![secondScratch![i]] = 0;
        activeCount = 0;
        var column = tile % columns; var top = tile / columns << bits;
        var bottom = Math.Min(height, top + (1 << bits)); var right = Math.Min(width, (column + 1) << bits);
        for (var row = top; row < bottom; row++)
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var cell = row * columns + column; var index = rowTokens[cell]; if (index < 0) continue;
            var position = rowPositions[cell]; var end = row * width + right;
            while (index < tokens.Length && position < end)
            {
                context.Cancellation.ThrowIfCancellationRequested(); var token = tokens[index++];
                if (token.Code > 0)
                {
                    Increment(256 + Vp8lSymbols.Prefix(token.Code).Code, ref activeCount);
                    Increment(layout.Distance + Vp8lSymbols.Prefix(Vp8lSymbols.Plane((int)token.Value, width)).Code, ref activeCount);
                }
                else if (token.Code == 0)
                {
                    var pixel = token.Value;
                    Increment((int)(pixel >> 8 & 255), ref activeCount);
                    Increment(layout.Red + (int)(pixel >> 16 & (uint)(layout.RedSize - 1)), ref activeCount);
                    Increment(layout.Blue + (int)(pixel & (uint)(layout.BlueSize - 1)), ref activeCount);
                    Increment(layout.Alpha + (int)(pixel >> 24 & (uint)(layout.AlphaSize - 1)), ref activeCount);
                }
                else
                {
                    var cache = ~token.Code;
                    if ((uint)cache >= (uint)layout.CacheSize) throw new InvalidOperationException("Token cache index exceeds its alphabet.");
                    Increment(280 + cache, ref activeCount);
                }
                position += token.Length;
            }
        }
    }

    private void Increment(int symbol, ref int activeCount)
    {
        if (firstScratch![symbol]++ == 0) secondScratch![activeCount++] = symbol;
    }

    internal bool IsEmpty(int tile) => offsets![tile] == offsets[tile + 1];
    internal bool IsMerged(int tile) => denseMap![tile] >= 0;
    internal ReadOnlySpan<uint> Leaf(int tile) => entries.AsSpan(offsets![tile], offsets[tile + 1] - offsets[tile]);

    internal Span<int> Read(int tile, int side, bool original = false)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        var map = denseMap ?? throw new ObjectDisposedException(nameof(Vp8lPopulationWorkspace));
        if (!original && map[tile] >= 0) return blocks![map[tile]];
        var counts = side == 0 ? firstScratch! : secondScratch!;
        ref var cachedTile = ref (side == 0 ? ref firstTile : ref secondTile);
        if (cachedTile != tile)
        {
            if (cachedTile < 0) counts.AsSpan().Clear();
            else
                for (var i = offsets![cachedTile]; i < offsets[cachedTile + 1]; i++) counts[(int)(entries![i] & 4095)] = 0;
            var words = side == 0 ? firstSupport! : secondSupport!; words.AsSpan().Clear();
            for (var i = offsets![tile]; i < offsets[tile + 1]; i++)
            {
                var value = entries![i]; var symbol = (int)(value & 4095); counts[symbol] = (int)(value >> 12);
                if (supportWords == 0) continue;
                if (symbol >= layout.Red && symbol < layout.Blue && layout.RedSize == 256)
                    words[(symbol - layout.Red) >> 6] |= 1UL << ((symbol - layout.Red) & 63);
                else if (symbol >= layout.Blue && symbol < layout.Alpha && layout.BlueSize == 256)
                    words[blueSupport + ((symbol - layout.Blue) >> 6)] |= 1UL << ((symbol - layout.Blue) & 63);
                else if (symbol >= layout.Alpha && symbol < layout.Distance && layout.AlphaSize == 256)
                    words[alphaSupport + ((symbol - layout.Alpha) >> 6)] |= 1UL << ((symbol - layout.Alpha) & 63);
            }
            cachedTile = tile;
        }
        return counts;
    }

    internal Span<ulong> Support(int tile, int side, bool original = false)
    {
        if (supportWords == 0) return default;
        Read(tile, side, original);
        var slot = denseMap![tile];
        return !original && slot >= 0 ? supports![slot] : side == 0 ? firstSupport : secondSupport;
    }

    internal void Merge(int a, int b)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        if (a == b) throw new ArgumentException("A population cannot merge with itself.");
        var map = denseMap ?? throw new ObjectDisposedException(nameof(Vp8lPopulationWorkspace));
        var first = Read(a, 0); var second = Read(b, 1); var aSlot = map[a]; var bSlot = map[b];
        var firstWords = Support(a, 0); var secondWords = Support(b, 1);
        if (aSlot < 0 && bSlot >= 0)
        {
            // Logical A survives. B's finished storage can become A's target;
            // nonnegative integer population addition is commutative and exact.
            Vp8lReferenceClustering.AddPopulation(second, first, context);
            for (var i = 0; i < supportWords; i++) secondWords[i] |= firstWords[i];
            map[a] = bSlot; map[b] = -1;
            return;
        }
        if (aSlot < 0)
        {
            aSlot = TakeDenseSlot(); first.CopyTo(blocks![aSlot]); firstWords.CopyTo(supports![aSlot]); map[a] = aSlot;
        }
        Vp8lReferenceClustering.AddPopulation(blocks![aSlot], second, context);
        var targetWords = supports![aSlot].AsSpan();
        for (var i = 0; i < supportWords; i++) targetWords[i] |= secondWords[i];
        if (bSlot >= 0) freeSlots![freeCount++] = bSlot;
        map[b] = -1;
    }

    private int TakeDenseSlot()
    {
        if (freeCount != 0) return freeSlots![--freeCount];
        var arrays = blocks!;
        if (allocated >= arrays.Length) throw new InvalidOperationException("Live merged populations exceed their leaf bound.");
        var bytes = layout.Total * 4L + supportWords * 8L + 64;
        context.Cancellation.ThrowIfCancellationRequested(); context.Reserve(bytes);
        try
        {
            context.Cancellation.ThrowIfCancellationRequested();
            var counts = new int[layout.Total]; var words = new ulong[supportWords];
            arrays[allocated] = counts; supports![allocated] = words; reserved += bytes;
            return allocated++;
        }
        catch { context.Release(bytes); throw; }
    }

    public void Dispose()
    {
        var bytes = Interlocked.Exchange(ref reserved, 0);
        offsets = denseMap = freeSlots = firstScratch = secondScratch = null;
        entries = null; blocks = null; supports = null; firstSupport = secondSupport = null;
        if (bytes != 0) context.Release(bytes);
    }
}
