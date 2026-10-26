using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

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
            cost += SmallResidualBias(tile) + PredictionEntropy(tile, prior);
        }
        return cost;
    }

    // Each caller supplies complete 256-bin planes. A pair of zero counts
    // contributes zero to both sums and both entropy terms. Masks enumerate
    // every other bin in ascending order, without narrowing any count.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long PredictionEntropy(ReadOnlySpan<int> current, ReadOnlySpan<int> previous)
    {
        var sumCurrent = 0; var sumCombined = 0; long cost = 0;
        if (Vector256.IsHardwareAccelerated)
        {
            var width = Vector512.IsHardwareAccelerated ? 16 : 8;
            for (var start = 0; start < 256; start += width)
            {
                uint active;
                if (Vector512.IsHardwareAccelerated)
                {
                    var values = Vector512.Create(current.Slice(start, 16)) | Vector512.Create(previous.Slice(start, 16));
                    active = (uint)Vector512.Equals(values, Vector512<int>.Zero).ExtractMostSignificantBits() ^ 0xffffu;
                }
                else
                {
                    var values = Vector256.Create(current.Slice(start, 8)) | Vector256.Create(previous.Slice(start, 8));
                    active = Vector256.Equals(values, Vector256<int>.Zero).ExtractMostSignificantBits() ^ 0xffu;
                }
                while (active != 0)
                {
                    var symbol = start + BitOperations.TrailingZeroCount(active);
                    var x = current[symbol]; var combined = x + previous[symbol];
                    sumCurrent += x; sumCombined += combined;
                    cost -= SLog(x) + SLog(combined);
                    active &= active - 1;
                }
            }
        }
        else
        {
            for (var symbol = 0; symbol < 256; symbol++)
            {
                var x = current[symbol]; var combined = x + previous[symbol];
                sumCurrent += x; sumCombined += combined;
                cost -= SLog(x) + SLog(combined);
            }
        }
        return cost + SLog(sumCurrent) + SLog(sumCombined);
    }

    internal static long Shannon(ReadOnlySpan<int> counts, OperationContext context)
    {
        context.Cancellation.ThrowIfCancellationRequested();
        var sum = 0; long cost = 0;
        foreach (var count in counts) { sum += count; cost -= SLog(count); }
        return cost + SLog(sum);
    }
}
