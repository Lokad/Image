namespace Lokad.Image.Internal;

internal static partial class WebpLosslessEncoder
{
    private sealed record ColorPlan(byte[] Coefficients, WebpPrefixCode[] Tables);

    private static CachedPlan SelectColorPlan(ImageBuffer image, int effort, uint[] cache,
        CachedPlan baseline, OperationContext context)
    {
        var tiles = ((image.Width + 7) >> 3) * ((image.Height + 7) >> 3);
        var mapBytes = tiles * 3L + 64; // Byte array and retained ColorPlan object.
        var work = (long)image.Width * image.Height * 4096 + tiles * 32768L + 1_000_000;
        if (effort < 4 || mapBytes + 4096 + CachedWorkingBytes > context.RemainingWorkingBytes
            || work > context.RemainingWork) return baseline;
        context.Reserve(mapBytes); context.Work(0);
        var coefficients = new byte[tiles * 3];
        SelectColorCoefficients(image, baseline.Predict, baseline.Modes, coefficients, context);
        var color = new ColorPlan(coefficients, ColorTables(coefficients, context));
        var candidate = PrepareCachedTables(image, effort, cache, baseline.Predict, context,
            baseline.Modes, baseline.ModeTable, color) with { Alpha = baseline.Alpha };
        candidate = SelectEntropyPlan(image, effort, cache, candidate, context);
        if (PayloadUpper(image, candidate, context) + 2 < PayloadUpper(image, baseline, context))
        {
            // Both alternatives borrow the same predictor map. Its ownership
            // transfers to the winner, independently of coding/color storage.
            ReleaseColorPlan(baseline, context); return candidate;
        }
        ReleaseColorPlan(candidate, context); return baseline;
    }

    // RFC9649 section3.5.2: signed8-bit coefficients and source channels,
    // arithmetic right shifts, and the ORIGINAL red in the blue residual.
    internal static uint ColorResidual(uint pixel, int greenToRed, int greenToBlue, int redToBlue)
    {
        var green = (int)unchecked((sbyte)(pixel >> 8));
        var red = (int)unchecked((sbyte)(pixel >> 16));
        return pixel & 0xff00ff00u
            | (uint)((int)(pixel >> 16 & 255) - ((greenToRed * green) >> 5) & 255) << 16
            | (uint)((int)(pixel & 255) - ((greenToBlue * green) >> 5) - ((redToBlue * red) >> 5) & 255);
    }

    internal static void SelectColorCoefficients(ImageBuffer image, bool predict, byte[]? modes,
        Span<byte> coefficients, OperationContext context)
    {
        var columns = (image.Width + 7) >> 3; var rows = (image.Height + 7) >> 3;
        if (coefficients.Length != columns * rows * 3) throw new ArgumentException("Color map dimensions differ.", nameof(coefficients));
        context.Reserve(4096);
        try
        {
            context.Work(0);
            Span<uint> pixels = stackalloc uint[64]; Span<int> frequencies = stackalloc int[256];
            Span<byte> symbols = stackalloc byte[64]; Span<double> entropy = stackalloc double[65]; entropy[0] = 0;
            for (var n = 1; n <= 64; n++) entropy[n] = n * Math.Log2(n);
            for (var ty = 0; ty < rows; ty++)
            for (var tx = 0; tx < columns; tx++)
            {
                var count = 0;
                for (var y = ty * 8; y < Math.Min(ty * 8 + 8, image.Height); y++)
                for (var x = tx * 8; x < Math.Min(tx * 8 + 8, image.Width); x++)
                    pixels[count++] = CachedPixel(image, y * image.Width + x, predict, context, modes);
                var tile = pixels[..count]; long gg = 0, rr = 0, gr = 0, gb = 0, rb = 0;
                context.Work(count * 16L);
                foreach (var pixel in tile)
                {
                    var g = (long)unchecked((sbyte)(pixel >> 8)); var r = (long)unchecked((sbyte)(pixel >> 16));
                    var b = (long)unchecked((sbyte)pixel);
                    gg += g * g; rr += r * r; gr += g * r; gb += g * b; rb += r * b;
                }
                var determinant = gg * rr - gr * gr;
                var a = gg == 0 ? 0 : BoundColor(32d * gr / gg);
                var b0 = determinant != 0 ? BoundColor(32d * (gb * rr - rb * gr) / determinant)
                    : gg == 0 ? 0 : BoundColor(32d * gb / gg);
                var c = determinant == 0 ? 0 : BoundColor(32d * (rb * gg - gb * gr) / determinant);
                if (ColorScore(tile, a, 0, true, frequencies, symbols, entropy, context)
                    >= ColorScore(tile, 0, 0, true, frequencies, symbols, entropy, context)) a = 0;
                if (ColorScore(tile, b0, c, false, frequencies, symbols, entropy, context)
                    >= ColorScore(tile, 0, 0, false, frequencies, symbols, entropy, context)) { b0 = 0; c = 0; }
                // One fixed source-derived fit, not a settings or quality grid.
                for (var step = 16; step != 0; step >>= 1)
                {
                    a = ImproveColor(tile, a, 0, step, 0, frequencies, symbols, entropy, context);
                    b0 = ImproveColor(tile, b0, c, step, 1, frequencies, symbols, entropy, context);
                    c = ImproveColor(tile, c, b0, step, 2, frequencies, symbols, entropy, context);
                }
                var at = (ty * columns + tx) * 3;
                coefficients[at] = unchecked((byte)a); coefficients[at + 1] = unchecked((byte)b0); coefficients[at + 2] = unchecked((byte)c);
            }
        }
        finally { context.Release(4096); }
    }

    private static int BoundColor(double value) => (int)Math.Clamp(Math.Round(value), -128d, 127d);

    private static int ImproveColor(ReadOnlySpan<uint> pixels, int current, int other, int step, int channel,
        Span<int> frequencies, Span<byte> symbols, ReadOnlySpan<double> entropy, OperationContext context)
    {
        Span<int> candidates = [current, Math.Clamp(current - step, -128, 127), Math.Clamp(current + step, -128, 127)];
        var selected = current; var best = double.PositiveInfinity;
        foreach (var candidate in candidates)
        {
            var score = ColorScore(pixels, channel == 2 ? other : candidate, channel == 2 ? candidate : other,
                channel == 0, frequencies, symbols, entropy, context);
            if (score < best) { best = score; selected = candidate; }
        }
        return selected;
    }

    private static double ColorScore(ReadOnlySpan<uint> pixels, int a, int b, bool red,
        Span<int> frequencies, Span<byte> symbols, ReadOnlySpan<double> entropy, OperationContext context)
    {
        context.Work(256 + pixels.Length * 16L); frequencies.Clear(); var used = 0;
        foreach (var pixel in pixels)
        {
            var green = (int)unchecked((sbyte)(pixel >> 8));
            var value = red ? (int)(pixel >> 16 & 255) - ((a * green) >> 5)
                : (int)(pixel & 255) - ((a * green) >> 5) - ((b * (int)unchecked((sbyte)(pixel >> 16))) >> 5);
            value &= 255;
            if (frequencies[value]++ == 0) symbols[used++] = (byte)value;
        }
        context.Work(used); var score = 0d;
        // First-occurrence order makes the entropy tie rule deterministic.
        foreach (var symbol in symbols[..used]) score -= entropy[frequencies[symbol]];
        return score;
    }

    private static WebpPrefixCode[] ColorTables(byte[] coefficients, OperationContext context)
    {
        const int bytes = (280 + 256 * 3 + 40) * 4 + 256; context.Reserve(bytes);
        try
        {
            context.Work(0);
            int[][] frequencies = [new int[280], new int[256], new int[256], new int[256], new int[40]];
            context.Work(coefficients.Length);
            for (var at = 0; at < coefficients.Length; at += 3)
            {
                frequencies[0][coefficients[at + 1]]++; frequencies[1][coefficients[at + 2]]++;
                frequencies[2][coefficients[at]]++;
            }
            frequencies[3][255] = frequencies[4][0] = 1;
            context.Reserve(5 * 8 + 32); context.Work(0);
            var tables = new WebpPrefixCode[5];
            for (var channel = 0; channel < 5; channel++) tables[channel] = new(frequencies[channel], context);
            return tables;
        }
        finally { context.Release(bytes); }
    }

    private static void ColorHeader(ColorPlan color, WebpBitWriter writer, OperationContext context)
    {
        writer.Write(1, 1); writer.Write(1, 2); writer.Write(1, 3); // Cross-color,8x8 tiles.
        writer.Write(0, 1); // Auxiliary image: no cache or recursive spatial flag.
        foreach (var table in color.Tables) table.Header(writer, context);
        for (var at = 0; at < color.Coefficients.Length; at += 3)
        {
            color.Tables[0].Write(writer, color.Coefficients[at + 1]);
            color.Tables[1].Write(writer, color.Coefficients[at + 2]);
            color.Tables[2].Write(writer, color.Coefficients[at]);
        }
    }
}
