namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    // RFC9649 sections3.5.2/3.7: price the signed color transform and its map
    // using the selected actual codes. Original red still drives blue.
    private static void SelectCodeCostColorCoefficients(ImageBuffer image, CachedPlan baseline,
        Span<byte> coefficients, OperationContext context)
    {
        var columns = (image.Width + 7) >> 3; var rows = (image.Height + 7) >> 3;
        if (coefficients.Length != columns * rows * 3) throw new ArgumentException("Color map dimensions differ.", nameof(coefficients));
        context.Reserve(4096);
        try
        {
            context.Work(0); Span<uint> pixels = stackalloc uint[64];
            var color = baseline.Color!;
            for (var ty = 0; ty < rows; ty++)
            for (var tx = 0; tx < columns; tx++)
            {
                var count = 0;
                for (var y = ty * 8; y < Math.Min(ty * 8 + 8, image.Height); y++)
                for (var x = tx * 8; x < Math.Min(tx * 8 + 8, image.Width); x++)
                    pixels[count++] = CachedPixel(image, y * image.Width + x, baseline.Predict, context, baseline.Modes);
                var tile = pixels[..count]; var at = (ty * columns + tx) * 3;
                var a = (int)unchecked((sbyte)color.Coefficients[at]);
                var b = (int)unchecked((sbyte)color.Coefficients[at + 1]);
                var c = (int)unchecked((sbyte)color.Coefficients[at + 2]);
                var tables = baseline.Tables.AsSpan(baseline.Groups!.Map[ty * columns + tx] * 5, 5);
                for (var step = 16; step != 0; step >>= 1)
                {
                    a = ImproveCodeCostColor(tile, a, 0, step, 0, tables, color.Tables, context);
                    b = ImproveCodeCostColor(tile, b, c, step, 1, tables, color.Tables, context);
                    c = ImproveCodeCostColor(tile, c, b, step, 2, tables, color.Tables, context);
                }
                coefficients[at] = unchecked((byte)a); coefficients[at + 1] = unchecked((byte)b); coefficients[at + 2] = unchecked((byte)c);
            }
        }
        finally { context.Release(4096); }
    }

    private static int ImproveCodeCostColor(ReadOnlySpan<uint> pixels, int current, int other,
        int step, int channel, ReadOnlySpan<WebpPrefixCode> tables,
        ReadOnlySpan<WebpPrefixCode> coefficients, OperationContext context)
    {
        var selected = current; var best = int.MaxValue;
        for (var option = 0; option < 3; option++)
        {
            var candidate = option == 0 ? current : Math.Clamp(current + (option == 1 ? -step : step), -128, 127);
            var score = CodeCostColorScore(pixels, channel == 2 ? other : candidate,
                channel == 2 ? candidate : other, channel == 0, tables, coefficients, context);
            if (score < best) { best = score; selected = candidate; }
        }
        return selected;
    }

    private static int CodeCostColorScore(ReadOnlySpan<uint> pixels, int a, int b, bool red,
        ReadOnlySpan<WebpPrefixCode> tables, ReadOnlySpan<WebpPrefixCode> coefficients, OperationContext context)
    {
        static int Cost(WebpPrefixCode table, int symbol) => table.Length(symbol) == 0 ? 15 : table.BitLength(symbol);
        context.Work(8 + pixels.Length * 16L);
        var score = Cost(coefficients[red ? 2 : 0], a & 255);
        if (!red) score += Cost(coefficients[1], b & 255);
        var table = tables[red ? 1 : 2];
        foreach (var pixel in pixels)
        {
            var green = (int)unchecked((sbyte)(pixel >> 8));
            var value = red ? (int)(pixel >> 16 & 255) - ((a * green) >> 5)
                : (int)(pixel & 255) - ((a * green) >> 5) - ((b * (int)unchecked((sbyte)(pixel >> 16))) >> 5);
            score += Cost(table, value & 255);
        }
        return score;
    }
}
