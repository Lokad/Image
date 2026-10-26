using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lokad.Image.Internal;

// Mathematical fixed-point entropy for encoder decisions. No imported tables.
internal static partial class Vp8lReferenceCost
{
    internal const long Scale = 1L << 23;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long SLog(int count)
    {
        if (count < 2) return 0;
        if (count < 256) return SmallProducts[count];
        return LargeSLog(count);
    }

    // Keep the uncommon arithmetic out of per-bin zero/small-count callers.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long LargeSLog(int count)
    {
        if (count >= 65536) return (long)Math.Floor(Scale / Math.Log(2) * count * Math.Log(count) + 0.5);
        var shift = BitOperations.Log2((uint)count) - 7;
        var head = count >> shift;
        var log = SmallLog(head) + shift * Scale;
        return count * log + (long)(Scale / Math.Log(2)) * (count - (head << shift));
    }

    private static long SmallResidualBias(ReadOnlySpan<int> histogram)
    {
        var weighted = histogram[0] * Scale;
        var weight = 94 * Scale;
        for (var distance = 1; distance < 16; distance++)
        {
            weighted += (weight * (histogram[distance] + histogram[256 - distance]) + 50) / 100;
            weight = (weight * 6 + 5) / 10;
        }
        return -(weighted + 5) / 10;
    }

    internal static long Prediction(ReadOnlySpan<int> current, ReadOnlySpan<int> previous, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        long cost = 0;
        for (var plane = 0; plane < 4; plane++)
        {
            var tile = current.Slice(plane * 256, 256);
            var prior = previous.Slice(plane * 256, 256);
            var sumCurrent = 0; var sumCombined = 0;
            cost += SmallResidualBias(tile);
            for (var symbol = 0; symbol < 256; symbol++)
            {
                var x = tile[symbol]; var combined = x + prior[symbol];
                sumCurrent += x; sumCombined += combined;
                cost -= SLog(x) + SLog(combined);
            }
            cost += SLog(sumCurrent) + SLog(sumCombined);
        }
        return cost;
    }

    internal static long Shannon(ReadOnlySpan<int> counts, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        var sum = 0; long cost = 0;
        foreach (var count in counts) { sum += count; cost -= SLog(count); }
        return cost + SLog(sum);
    }
}
