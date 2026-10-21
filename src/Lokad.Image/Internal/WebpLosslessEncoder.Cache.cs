using System.Buffers.Binary;

namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    // Bounds simultaneous retained trees, cache, histograms, the largest tree's
    // construction scratch and code-length-header scratch. Actual major owned
    // capacities below are charged separately through the borrowed context.
    private const int CachedWorkingBytes = 65536;
    private const int CacheSize = 1024;
    private readonly record struct CachedScan(bool Alpha, int Literals, long ExtraBits, int Tokens);
    private readonly record struct CachedPlan(WebpPrefixCode[] Tables, long BodyBits, bool Alpha, bool Predict, int Literals,
        EntropyGroups? Groups = null, byte[]? Modes = null, WebpPrefixCode? ModeTable = null, ColorPlan? Color = null,
        bool UseCache = true, ReferencePlan? References = null);

    private static void EncodeCached(ImageBuffer image, Stream output, WebpEncodeOptions options, OperationContext context)
    {
        context.Reserve(CacheSize * 4 + 32 + 128); context.Work(0);
        var cache = new uint[CacheSize];
        var plan = SelectCachedPlan(image, options.Effort, cache, context);
        plan = SelectNoCachePlan(image, options.Effort, cache, plan, context);
        plan = SelectCodeCostPlan(image, options.Effort, cache, plan, context);
        plan = SelectReferencePlan(image, options.Effort, cache, plan, context);
        using var counter = new CountStream(options.Limits.MaxOutputBytes);
        CachedPayload(image, counter, options.Effort, cache, plan, context);
        var payloadLength = counter.Count;
        if (payloadLength + 20L + (payloadLength & 1) > options.Limits.MaxOutputBytes)
            throw new ImageLimitException(ImageLimitKind.OutputBytes, "WebP encoded output limit exceeded.");
        Span<byte> header = stackalloc byte[20];
        "RIFF"u8.CopyTo(header); BinaryPrimitives.WriteInt32LittleEndian(header[4..], payloadLength + 12 + (payloadLength & 1));
        "WEBPVP8L"u8.CopyTo(header[8..]); BinaryPrimitives.WriteInt32LittleEndian(header[16..], payloadLength);
        output.Write(header);
        CachedPayload(image, output, options.Effort, cache, plan, context);
        if ((payloadLength & 1) != 0) output.WriteByte(0);
    }

    private static CachedPlan PrepareCachedTables(ImageBuffer image, int effort, uint[] cache, bool predict, OperationContext context,
        byte[]? modes = null, WebpPrefixCode? modeTable = null, ColorPlan? color = null, bool useCache = true)
        => PreparePlanTables(image, effort, cache, predict, context, modes, modeTable, color, useCache, null);

    private static CachedPlan PreparePlanTables(ImageBuffer image, int effort, uint[] cache, bool predict,
        OperationContext context, byte[]? modes, WebpPrefixCode? modeTable, ColorPlan? color,
        bool useCache, ReferencePlan? references)
    {
        var greenAlphabet = 280 + (useCache ? CacheSize : 0);
        var frequencyBytes = (greenAlphabet + 256 * 3 + 40) * 4 + 256;
        context.Reserve(frequencyBytes);
        try
        {
            context.Work(0);
            int[][] frequencies = [new int[greenAlphabet], new int[256], new int[256], new int[256], new int[40]];
            var scan = references is null
                ? CachedPixels(image, effort, cache, predict, null, null, frequencies, context,
                    modes: modes, color: color, useCache: useCache)
                : ReferencePixels(image, references, null, null, frequencies, context);
            context.Reserve(5 * 8 + 32); context.Work(0);
            var result = new WebpPrefixCode[5];
            for (var channel = 0; channel < result.Length; channel++) result[channel] = new(frequencies[channel], context);
            var bodyBits = scan.ExtraBits;
            for (var channel = 0; channel < result.Length; channel++)
            {
                context.Work(frequencies[channel].Length);
                for (var symbol = 0; symbol < frequencies[channel].Length; symbol++)
                    bodyBits += (long)frequencies[channel][symbol] * result[channel].BitLength(symbol);
            }
            return new(result, bodyBits, scan.Alpha, predict, scan.Literals,
                Modes: modes, ModeTable: modeTable, Color: color, UseCache: useCache, References: references);
        }
        finally { context.Release(frequencyBytes); }
    }

    private static void CachedPayload(ImageBuffer image, Stream output, int effort,
        uint[] cache, CachedPlan plan, OperationContext context)
    {
        var writer = CachedHeader(image, output, plan, context);
        if (plan.References is { } references)
            ReferencePixels(image, references, writer, plan.Tables, null, context, groups: plan.Groups);
        else CachedPixels(image, effort, cache, plan.Predict, writer, plan.Tables, null, context,
            groups: plan.Groups, modes: plan.Modes, color: plan.Color, useCache: plan.UseCache);
        writer.Finish();
    }

    private static WebpBitWriter CachedHeader(ImageBuffer image, Stream output, CachedPlan plan, OperationContext context)
    {
        Span<byte> header = stackalloc byte[5]; header[0] = 0x2f;
        BinaryPrimitives.WriteUInt32LittleEndian(header[1..], (uint)(image.Width - 1) | (uint)(image.Height - 1) << 14 | (plan.Alpha ? 1u << 28 : 0));
        output.Write(header);
        var writer = new WebpBitWriter(output, context);
        // RFC9649 sections3.5.3/3.6.1: green decorrelation and optional ten-bit cache.
        writer.Write(1, 1); writer.Write(2, 2);
        if (plan.Modes is { } modes) AdaptivePredictorHeader(modes, plan.ModeTable!, writer, context);
        else if (plan.Predict) LeftPredictorHeader(image, writer, context);
        if (plan.Color is { } color) ColorHeader(color, writer, context);
        writer.Write(0, 1);
        writer.Write(plan.UseCache ? 1 : 0, 1);
        if (plan.UseCache) writer.Write(10, 4);
        if (plan.Groups is { } groups) EntropyHeader(groups, writer, context);
        else writer.Write(0, 1);
        foreach (var table in plan.Tables) table.Header(writer, context);
        return writer;
    }

    private static CachedScan CachedPixels(ImageBuffer image, int effort, uint[] cache, bool predict,
        WebpBitWriter? writer, WebpPrefixCode[]? tables, int[][]? frequencies, OperationContext context,
        TileHistograms? histograms = null, EntropyGroups? groups = null, byte[]? modes = null,
        ColorPlan? color = null, bool useCache = true)
        => WalkCachedPixels(image, effort, cache, predict, writer, tables, frequencies, context,
            histograms, groups, modes, color, useCache, null);

    private static CachedScan WalkCachedPixels(ImageBuffer image, int effort, uint[] cache, bool predict,
        WebpBitWriter? writer, WebpPrefixCode[]? tables, int[][]? frequencies, OperationContext context,
        TileHistograms? histograms, EntropyGroups? groups, byte[]? modes, ColorPlan? color,
        bool useCache, ReferenceToken[]? capture)
    {
        if (useCache) { context.Work(CacheSize); cache.AsSpan().Clear(); }
        else context.Work(0);
        var count = image.Width * image.Height; var position = 0; uint previous = 0; var alpha = false;
        var literals = 0; var tokens = 0; long extraBits = 0;
        while (position < count)
        {
            var tile = histograms is not null || groups is not null
                ? (position / image.Width >> 3) * ((image.Width + 7) >> 3) + ((position % image.Width) >> 3) : 0;
            var tableOffset = groups is null ? 0 : groups.Map[tile] * 5;
            var pixel = CachedPixel(image, position, predict, context, modes, color);
            // Only the original normalized source determines the header hint.
            // Predictor residual alpha has different semantics.
            if (!predict) alpha |= pixel >> 24 != 255;
            var maximum = Math.Min(64 << effort, count - position); var run = 1; var fromAbove = false;
            if (position != 0 && pixel == previous)
                while (run < maximum && CachedPixel(image, position + run, predict, context, modes, color) == pixel) run++;
            if (position >= image.Width && run < maximum)
            {
                var length = 0;
                while (length < maximum && CachedPixel(image, position + length, predict, context, modes, color)
                    == CachedPixel(image, position - image.Width + length, predict, context, modes, color)) length++;
                if (length > run) { run = length; fromAbove = true; }
            }
            if (run >= 2)
            {
                if (capture is not null) capture[tokens] = new(position, run, fromAbove ? 1 : 2, pixel);
                Prefix(run, out var code, out var extra, out var bits);
                extraBits += bits;
                CachedSymbol(0, 256 + code, writer, tables, frequencies, context, tableOffset, histograms, tile);
                if (writer is not null) writer.Write(extra, bits); else context.Work(1);
                CachedSymbol(4, fromAbove ? 0 : 1, writer, tables, frequencies, context, tableOffset, histograms, tile);
                if (fromAbove)
                {
                    for (var i = 0; i < run; i++)
                    {
                        pixel = CachedPixel(image, position + i, predict, context, modes, color);
                        context.Work(1);
                        if (useCache) cache[CacheIndex(pixel)] = pixel;
                    }
                }
                else
                {
                    // Every copied pixel is identical. One assignment has the
                    // same final cache state; retain the per-pixel work charge.
                    context.Work(run);
                    if (useCache) cache[CacheIndex(pixel)] = pixel;
                }
                position += run;
            }
            else
            {
                var index = useCache ? CacheIndex(pixel) : 0;
                if (capture is not null) capture[tokens] = new(position, 1,
                    useCache && cache[index] == pixel ? -1 - index : 0, pixel);
                if (useCache && cache[index] == pixel) CachedSymbol(0, 280 + index, writer, tables, frequencies, context, tableOffset, histograms, tile);
                else
                {
                    literals++;
                    CachedSymbol(0, (int)(pixel >> 8 & 255), writer, tables, frequencies, context, tableOffset, histograms, tile);
                    CachedSymbol(1, (int)(pixel >> 16 & 255), writer, tables, frequencies, context, tableOffset, histograms, tile);
                    CachedSymbol(2, (int)(pixel & 255), writer, tables, frequencies, context, tableOffset, histograms, tile);
                    CachedSymbol(3, (int)(pixel >> 24), writer, tables, frequencies, context, tableOffset, histograms, tile);
                }
                context.Work(1);
                if (useCache) cache[index] = pixel;
                position++;
            }
            previous = pixel; tokens++;
        }
        return new(alpha, literals, extraBits, tokens);
    }

    private static int CacheIndex(uint pixel) => (int)(unchecked(pixel * 0x1e35a7bdu) >> 22);

    private static uint GreenPixel(ImageBuffer image, int position, OperationContext context)
    {
        var pixel = Pixel(image, position, context); var green = (int)(pixel >> 8 & 255);
        return pixel & 0xff00ff00u | (uint)(((int)(pixel >> 16 & 255) - green) & 255) << 16
            | (uint)(((int)(pixel & 255) - green) & 255);
    }

    private static void CachedSymbol(int channel, int symbol, WebpBitWriter? writer,
        WebpPrefixCode[]? tables, int[][]? frequencies, OperationContext context,
        int tableOffset = 0, TileHistograms? histograms = null, int tile = 0)
    {
        if (writer is not null) tables![tableOffset + channel].Write(writer, symbol);
        else
        {
            context.Work(1);
            if (histograms is not null) histograms.Add(tile, channel, symbol);
            else if (frequencies is not null) frequencies[channel][symbol]++;
        }
    }
}
