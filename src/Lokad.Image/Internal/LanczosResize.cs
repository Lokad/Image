namespace Lokad.Image.Internal;

// Separable, alpha-aware fixed-point resampling. No intermediate rounding:
// horizontal Int64 rows feed Int128 vertical sums, followed by one exact divide.
internal static class LanczosResize
{
    internal static ImageBuffer Apply(ImageBuffer source, int width, int height, OperationContext context)
    {
        using var horizontal = LanczosPlan.Create(source.Width, width, context);
        using var vertical = LanczosPlan.Create(source.Height, height, context);
        var slots = Math.Min(vertical.MaximumCount, 64);
        var channels = width * 4L; var rowElements = channels * slots;
        var bytes = 128 + rowElements * 8 + channels * 16 + slots * 4L;
        if (rowElements * 8 > Array.MaxLength || channels * 16 > Array.MaxLength)
            throw new ImageLimitException(ImageLimitKind.BufferLength, "Lanczos row storage exceeds the managed limit.");
        context.Reserve(bytes);
        try
        {
            var rows = new long[(int)rowElements]; var rowIds = new int[slots]; Array.Fill(rowIds, -1);
            var sums = new Int128[(int)channels];
            var result = ImageBuffer.AllocateForOverwrite(width, height, source.BitDepth, context);
            try
            {
                var maximum = source.BitDepth == 8 ? 255 : 65535;
                for (var y = 0; y < height; y++)
                {
                    context.Work(channels); sums.AsSpan().Clear();
                    var window = vertical.Windows[y];
                    for (var tap = 0; tap < window.Count; tap++)
                    {
                        context.Work(1); var weight = vertical.Weights[window.Offset + tap]; if (weight == 0) continue;
                        var sy = window.First + tap; var slot = sy % slots;
                        var row = rows.AsSpan(slot * (int)channels, (int)channels);
                        if (rowIds[slot] != sy) { Horizontal(source, sy, horizontal, row, context); rowIds[slot] = sy; }
                        context.Work(channels);
                        for (var i = 0; i < sums.Length; i++)
                        { if ((i & 4095) == 0) context.Cancellation.ThrowIfCancellationRequested(); sums[i] += (Int128)row[i] * weight; }
                    }
                    context.Work(channels);
                    Span<byte> destination8 = default; Span<ushort> destination16 = default;
                    if (source.BitDepth == 8) destination8 = result.GetRow8(y); else destination16 = result.GetRow16(y);
                    for (var x = 0; x < width; x++)
                    {
                        if ((x & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                        var at = x * 4; var alpha = sums[at + 3];
                        var a = Round(alpha, (Int128)LanczosPlan.One * LanczosPlan.One, maximum);
                        var r = a == 0 ? 0 : Round(sums[at], alpha, maximum);
                        var g = a == 0 ? 0 : Round(sums[at + 1], alpha, maximum);
                        var b = a == 0 ? 0 : Round(sums[at + 2], alpha, maximum);
                        if (source.BitDepth == 8)
                        {
                            destination8[at] = (byte)r; destination8[at + 1] = (byte)g;
                            destination8[at + 2] = (byte)b; destination8[at + 3] = (byte)a;
                        }
                        else
                        {
                            destination16[at] = (ushort)r; destination16[at + 1] = (ushort)g;
                            destination16[at + 2] = (ushort)b; destination16[at + 3] = (ushort)a;
                        }
                    }
                }
                result.Orientation = source.Orientation; return result;
            }
            catch { result.Dispose(); throw; }
        }
        finally { context.Release(bytes); }
    }

    private static void Horizontal(ImageBuffer source, int y, LanczosPlan plan, Span<long> output, OperationContext context)
    {
        ReadOnlySpan<byte> input8 = default; ReadOnlySpan<ushort> input16 = default;
        if (source.BitDepth == 8) input8 = source.GetRow8(y); else input16 = source.GetRow16(y);
        for (var x = 0; x < plan.Windows.Length; x++)
        {
            var window = plan.Windows[x]; context.Work(window.Count * 4L);
            long r = 0, g = 0, b = 0, alpha = 0;
            for (var i = 0; i < window.Count; i++)
            {
                if ((i & 1023) == 0) context.Cancellation.ThrowIfCancellationRequested();
                var weight = plan.Weights[window.Offset + i]; var at = (window.First + i) * 4;
                int a, red, green, blue;
                if (source.BitDepth == 8) { a = input8[at + 3]; red = input8[at]; green = input8[at + 1]; blue = input8[at + 2]; }
                else { a = input16[at + 3]; red = input16[at]; green = input16[at + 1]; blue = input16[at + 2]; }
                r += (long)a * red * weight; g += (long)a * green * weight; b += (long)a * blue * weight;
                alpha += (long)a * weight;
            }
            var destination = x * 4; output[destination] = r; output[destination + 1] = g;
            output[destination + 2] = b; output[destination + 3] = alpha;
        }
    }
    internal static int Round(Int128 numerator, Int128 denominator, int maximum)
    {
        if (numerator <= 0) return 0;
        if (numerator >= denominator * maximum) return maximum;
        var (quotient, remainder) = Int128.DivRem(numerator, denominator);
        return (int)(quotient + (remainder * 2 >= denominator ? 1 : 0));
    }
}
