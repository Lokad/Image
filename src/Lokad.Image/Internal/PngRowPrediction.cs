namespace Lokad.Image.Internal;

// PNG byte predictors (W3C PNG section9). A bounded sample admits additional
// row storage only for a strong residual reduction; unpredictable inputs keep
// their existing None-filter packet policy and workspace.
internal static class PngRowPrediction
{
    private static int Cost(int value) { value &= 255; return Math.Min(value, 256 - value); }

    internal static bool Useful(ImageBuffer source, int color, CancellationToken cancellation)
    {
        var channels = color == 0 ? 1 : color == 2 ? 3 : color == 4 ? 2 : 4;
        var samples = Math.Min(32, source.Width);
        long none = 0, sub = 0, up = 0, average = 0, paeth = 0;
        for (var probe = 0; probe < 4; probe++)
        {
            cancellation.ThrowIfCancellationRequested();
            var y = probe switch { 0 => 0, 1 => Math.Min(1, source.Height - 1), 2 => source.Height / 2, _ => source.Height - 1 };
            for (var sample = 0; sample < samples; sample++)
            {
                var x = samples == 1 ? 0 : (int)((long)sample * (source.Width - 1) / (samples - 1));
                for (var channel = 0; channel < channels; channel++)
                for (var shift = source.BitDepth - 8; shift >= 0; shift -= 8)
                {
                    var component = color == 4 && channel == 1 ? 3 : channel;
                    var value = Byte(source, x, y, component, shift);
                    var a = x == 0 ? 0 : Byte(source, x - 1, y, component, shift);
                    var b = y == 0 ? 0 : Byte(source, x, y - 1, component, shift);
                    var c = x == 0 || y == 0 ? 0 : Byte(source, x - 1, y - 1, component, shift);
                    none += Cost(value); sub += Cost(value - a); up += Cost(value - b);
                    average += Cost(value - ((a + b) >> 1)); paeth += Cost(value - PngCodec.Paeth(a, b, c));
                }
            }
        }
        return Math.Min(Math.Min(sub, up), Math.Min(average, paeth)) * 4 < none;
    }

    private static int Byte(ImageBuffer source, int x, int y, int channel, int shift)
        => ((source.BitDepth == 8 ? source.GetRow8(y)[x * 4 + channel] : source.GetRow16(y)[x * 4 + channel]) >> shift) & 255;

    internal static byte Apply(ReadOnlySpan<byte> row, ReadOnlySpan<byte> previous, Span<byte> filtered, int stride,
        CancellationToken cancellation)
    {
        Span<long> scores = stackalloc long[5]; scores.Clear();
        for (var i = 0; i < row.Length; i++)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var value = row[i]; var a = i < stride ? 0 : row[i - stride]; var b = previous[i]; var c = i < stride ? 0 : previous[i - stride];
            scores[0] += Cost(value); scores[1] += Cost(value - a); scores[2] += Cost(value - b);
            scores[3] += Cost(value - ((a + b) >> 1)); scores[4] += Cost(value - PngCodec.Paeth(a, b, c));
        }
        var filter = 0; for (var i = 1; i < 5; i++) if (scores[i] < scores[filter]) filter = i;
        if (filter == 0) return 0;
        for (var i = 0; i < row.Length; i++)
        {
            if ((i & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            var a = i < stride ? 0 : row[i - stride]; var b = previous[i]; var c = i < stride ? 0 : previous[i - stride];
            var prediction = filter switch { 1 => a, 2 => b, 3 => (a + b) >> 1, _ => PngCodec.Paeth(a, b, c) };
            filtered[i] = unchecked((byte)(row[i] - prediction));
        }
        return (byte)filter;
    }
}
