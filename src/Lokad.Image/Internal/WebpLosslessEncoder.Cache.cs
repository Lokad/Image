using System.Buffers.Binary;

namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    // Bounds simultaneous retained trees, cache, histograms, the largest tree's
    // construction scratch and code-length-header scratch. Actual major owned
    // capacities below are charged separately through the borrowed context.
    private const int CachedWorkingBytes = 65536;
    private const int CacheSize = 1024;

    private static void EncodeCached(ImageBuffer image, Stream output, WebpEncodeOptions options, OperationContext context)
    {
        context.Reserve(CacheSize * 4 + 32 + 128); context.Work(0);
        var cache = new uint[CacheSize];
        var tables = PrepareCachedTables(image, options.Effort, cache, context);
        using var counter = new CountStream(options.Limits.MaxOutputBytes);
        var alpha = CachedPayload(image, counter, options.Effort, false, cache, tables, context);
        var payloadLength = counter.Count;
        if (payloadLength + 20L + (payloadLength & 1) > options.Limits.MaxOutputBytes)
            throw new ImageLimitException(ImageLimitKind.OutputBytes, "WebP encoded output limit exceeded.");
        Span<byte> header = stackalloc byte[20];
        "RIFF"u8.CopyTo(header); BinaryPrimitives.WriteInt32LittleEndian(header[4..], payloadLength + 12 + (payloadLength & 1));
        "WEBPVP8L"u8.CopyTo(header[8..]); BinaryPrimitives.WriteInt32LittleEndian(header[16..], payloadLength);
        output.Write(header);
        CachedPayload(image, output, options.Effort, alpha, cache, tables, context);
        if ((payloadLength & 1) != 0) output.WriteByte(0);
    }

    private static WebpPrefixCode[] PrepareCachedTables(ImageBuffer image, int effort, uint[] cache, OperationContext context)
    {
        const int frequencyBytes = (280 + CacheSize + 256 * 3 + 40) * 4 + 256;
        context.Reserve(frequencyBytes);
        try
        {
            context.Work(0);
            int[][] frequencies = [new int[280 + CacheSize], new int[256], new int[256], new int[256], new int[40]];
            CachedPixels(image, effort, cache, null, null, frequencies, context);
            context.Reserve(5 * 8 + 32); context.Work(0);
            var result = new WebpPrefixCode[5];
            for (var channel = 0; channel < result.Length; channel++) result[channel] = new(frequencies[channel], context);
            return result;
        }
        finally { context.Release(frequencyBytes); }
    }

    private static bool CachedPayload(ImageBuffer image, Stream output, int effort, bool alphaHint,
        uint[] cache, WebpPrefixCode[] tables, OperationContext context)
    {
        Span<byte> header = stackalloc byte[5]; header[0] = 0x2f;
        BinaryPrimitives.WriteUInt32LittleEndian(header[1..], (uint)(image.Width - 1) | (uint)(image.Height - 1) << 14 | (alphaHint ? 1u << 28 : 0));
        output.Write(header);
        var writer = new WebpBitWriter(output, context);
        // RFC9649 sections3.5.3/3.6.1: green decorrelation, then a ten-bit cache.
        writer.Write(1, 1); writer.Write(2, 2); writer.Write(0, 1);
        writer.Write(1, 1); writer.Write(10, 4); writer.Write(0, 1);
        foreach (var table in tables) table.Header(writer, context);
        var alpha = CachedPixels(image, effort, cache, writer, tables, null, context);
        writer.Finish(); return alpha;
    }

    private static bool CachedPixels(ImageBuffer image, int effort, uint[] cache,
        WebpBitWriter? writer, WebpPrefixCode[]? tables, int[][]? frequencies, OperationContext context)
    {
        context.Work(CacheSize); cache.AsSpan().Clear();
        var count = image.Width * image.Height; var position = 0; uint previous = 0; var alpha = false;
        while (position < count)
        {
            var pixel = GreenPixel(image, position, context); alpha |= pixel >> 24 != 255;
            var maximum = Math.Min(64 << effort, count - position); var run = 1; var fromAbove = false;
            if (position != 0 && pixel == previous)
                while (run < maximum && GreenPixel(image, position + run, context) == pixel) run++;
            if (position >= image.Width && run < maximum)
            {
                var length = 0;
                while (length < maximum && GreenPixel(image, position + length, context)
                    == GreenPixel(image, position - image.Width + length, context)) length++;
                if (length > run) { run = length; fromAbove = true; }
            }
            if (run >= 2)
            {
                Prefix(run, out var code, out var extra, out var bits);
                CachedSymbol(0, 256 + code, writer, tables, frequencies, context);
                if (writer is not null) writer.Write(extra, bits); else context.Work(1);
                CachedSymbol(4, fromAbove ? 0 : 1, writer, tables, frequencies, context);
                if (fromAbove)
                {
                    for (var i = 0; i < run; i++)
                    {
                        pixel = GreenPixel(image, position + i, context);
                        context.Work(1); cache[CacheIndex(pixel)] = pixel;
                    }
                }
                else
                {
                    // Every copied pixel is identical. One assignment has the
                    // same final cache state; retain the per-pixel work charge.
                    context.Work(run); cache[CacheIndex(pixel)] = pixel;
                }
                position += run;
            }
            else
            {
                var index = CacheIndex(pixel);
                if (cache[index] == pixel) CachedSymbol(0, 280 + index, writer, tables, frequencies, context);
                else
                {
                    CachedSymbol(0, (int)(pixel >> 8 & 255), writer, tables, frequencies, context);
                    CachedSymbol(1, (int)(pixel >> 16 & 255), writer, tables, frequencies, context);
                    CachedSymbol(2, (int)(pixel & 255), writer, tables, frequencies, context);
                    CachedSymbol(3, (int)(pixel >> 24), writer, tables, frequencies, context);
                }
                context.Work(1); cache[index] = pixel; position++;
            }
            previous = pixel;
        }
        return alpha;
    }

    private static int CacheIndex(uint pixel) => (int)(unchecked(pixel * 0x1e35a7bdu) >> 22);

    private static uint GreenPixel(ImageBuffer image, int position, OperationContext context)
    {
        var pixel = Pixel(image, position, context); var green = (int)(pixel >> 8 & 255);
        return pixel & 0xff00ff00u | (uint)(((int)(pixel >> 16 & 255) - green) & 255) << 16
            | (uint)(((int)(pixel & 255) - green) & 255);
    }

    private static void CachedSymbol(int channel, int symbol, WebpBitWriter? writer,
        WebpPrefixCode[]? tables, int[][]? frequencies, OperationContext context)
    {
        if (writer is not null) tables![channel].Write(writer, symbol);
        else { context.Work(1); frequencies![channel][symbol]++; }
    }
}
