namespace Lokad.Image.Internal;

// Prepared entropy state borrows no pixels and owns all tokens, maps and codes.
// It can be counted and emitted repeatedly without analysis/parsing or a packet
// copy. Dispose before its borrowed operation context.
internal sealed class Vp8lImageCode : IDisposable
{
    private readonly OperationContext context;
    private Vp8lTokenBuffer? tokens;
    private Vp8lHistogramPlan? histograms;
    private Vp8lHuffmanGroup?[]? groups;
    private Vp8lImageCode? groupMap;
    private long groupReservation;
    private readonly bool main;
    internal int Width { get; }
    internal int Height { get; }
    internal int StoredTileBits { get; private set; }
    private Vp8lImageCode(Vp8lTokenBuffer tokens, int width, int height, bool main, OperationContext context)
    { this.tokens = tokens; Width = width; Height = height; this.main = main; this.context = context; }

    private void BuildGroups(ReadOnlySpan<int> counts, Vp8lHistogramLayout layout, int count)
    {
        // Group references and a coarse allowance for code-owner/array headers.
        // Substantial symbol storage is charged by each canonical-code owner.
        var reservation = count * 1024L + 64; context.Reserve(reservation);
        try { context.Cancellation.ThrowIfCancellationRequested(); groups = new Vp8lHuffmanGroup?[count]; groupReservation = reservation; }
        catch { context.Release(reservation); throw; }
        for (var i = 0; i < count; i++) groups[i] = Vp8lHuffmanGroup.Build(counts.Slice(i * layout.Total, layout.Total), layout, context);
    }

    internal static Vp8lImageCode Auxiliary(ReadOnlySpan<uint> pixels, int width, int quality, bool lowEffort, OperationContext context)
    {
        if (width < 1 || pixels.Length == 0 || pixels.Length % width != 0) throw new ArgumentException("Expected complete auxiliary rows.");
        using var matches = Vp8lReferenceMatcher.Build(pixels, width, quality, lowEffort, context);
        var tokens = Vp8lReferenceLz.Encode(pixels, matches.Values, width, quality >= 25, context);
        var result = new Vp8lImageCode(tokens, width, pixels.Length / width, false, context);
        try
        {
            var layout = new Vp8lHistogramLayout(0); var reserved = layout.Total * 4L; context.Reserve(reserved);
            try
            {
                context.Cancellation.ThrowIfCancellationRequested(); var counts = new int[layout.Total]; layout.Fill(tokens.Tokens, width, true, counts, context);
                result.BuildGroups(counts, layout, 1);
            }
            finally { context.Release(reserved); }
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    // Ownership of tokens transfers on entry, including failure. The caller
    // must not dispose or modify them after passing them here.
    internal static Vp8lImageCode TakeMain(Vp8lTokenBuffer tokens, int width, int height,
        int tileBits, bool lowEffort, OperationContext context, int quality = 75)
    {
        var result = new Vp8lImageCode(tokens, width, height, true, context);
        try
        {
            var plan = result.histograms = Vp8lReferenceClustering.Build(tokens.Tokens, width, height, tileBits, tokens.CacheBits, lowEffort, context, quality);
            result.StoredTileBits = tileBits;
            if (plan.GroupCount > 1)
            {
                var map = plan.Map; var columns = plan.Columns; var rows = map.Length / columns;
                for (var candidate = tileBits + 1; candidate <= 9; candidate++)
                {
                    var step = 1 << (candidate - tileBits); var same = true;
                    for (var y = 0; same && y < rows; y++)
                    for (var x = 0; x < columns; x++)
                    {
                        context.Cancellation.ThrowIfCancellationRequested();
                        if (map[y * columns + x] != map[(y / step * step) * columns + x / step * step]) { same = false; break; }
                    }
                    if (!same) break; result.StoredTileBits = candidate;
                }
                var stride = 1 << (result.StoredTileBits - tileBits);
                var codedColumns = (columns + stride - 1) / stride; var codedRows = (rows + stride - 1) / stride;
                var reserved = codedColumns * codedRows * 4L; context.Reserve(reserved);
                try
                {
                    context.Cancellation.ThrowIfCancellationRequested(); var pixels = new uint[codedColumns * codedRows];
                    for (var y = 0; y < codedRows; y++)
                    for (var x = 0; x < codedColumns; x++)
                    { context.Cancellation.ThrowIfCancellationRequested(); pixels[y * codedColumns + x] = (uint)map[y * stride * columns + x * stride] << 8; }
                    result.groupMap = Auxiliary(pixels, codedColumns, quality, lowEffort, context);
                }
                finally { context.Release(reserved); }
            }
            result.BuildGroups(plan.Populations, plan.Layout, plan.GroupCount);
            plan.DisposePopulations(); // Only original assignments are needed to emit tokens.
            return result;
        }
        catch { result.Dispose(); throw; }
    }

    internal void Write(WebpBitWriter writer)
    {
        var ownedTokens = tokens ?? throw new ObjectDisposedException(nameof(Vp8lImageCode));
        var codes = groups ?? throw new ObjectDisposedException(nameof(Vp8lImageCode));
        if (main) writer.Write(0, 1); // End the transform list.
        writer.Write(ownedTokens.CacheBits == 0 ? 0 : 1, 1);
        if (ownedTokens.CacheBits != 0) writer.Write(ownedTokens.CacheBits, 4);
        if (main)
        {
            writer.Write(groupMap is null ? 0 : 1, 1);
            if (groupMap is not null) { writer.Write(StoredTileBits - 2, 3); groupMap.Write(writer); }
        }
        foreach (var group in codes) group!.WriteTrees(writer);
        var position = 0;
        foreach (var token in ownedTokens.Tokens)
        {
            var group = 0;
            if (histograms is { } plan)
                group = plan.Map[(position / Width >> plan.TileBits) * plan.Columns + (position % Width >> plan.TileBits)];
            codes[group]!.WriteToken(writer, token, Width); position += token.Length;
        }
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref tokens, null)?.Dispose(); Interlocked.Exchange(ref histograms, null)?.Dispose();
        Interlocked.Exchange(ref groupMap, null)?.Dispose();
        var ownedGroups = Interlocked.Exchange(ref groups, null);
        if (ownedGroups is not null) foreach (var group in ownedGroups) group?.Dispose();
        var reserved = Interlocked.Exchange(ref groupReservation, 0); if (reserved != 0) context.Release(reserved);
    }
}
