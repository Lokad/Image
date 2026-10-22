using System.Numerics;

namespace Lokad.Image.Internal;

internal static partial class JpegEncoder
{
    internal const int SourcePriceCapacity = 750;
    private const int JointPriceLayoutWork = 1024;
    internal readonly record struct JointPriceShape(int Slots, long FinishWork);

    internal static JointPriceShape SourcePriceShape444(JointAcBands bands)
    {
        var slots = 0; var work = 0L;
        for (var family = 0; family < 2; family++)
        {
            var ends = bands.ForComponent(family) | (1UL << 63);
            for (var first = 1; first < 64;)
            {
                var last = BitOperations.TrailingZeroCount(ends & (ulong.MaxValue << first));
                var length = last - first + 1;
                slots += 2 + 10 * Math.Min(16, length);
                // At most r symbols have a positive source count. The other
                // smoothed leaves have equal unit weights and never cross ties.
                var r = 1 + 10 * Math.Min(16, length) + (length >= 17 ? 1 : 0);
                work += 40960 + r * (323 - r) / 2;
                first = last + 1;
            }
        }
        return new(slots, work);
    }

    internal static void SourcePriceOffsets444(JointAcBands bands, Span<ushort> offsets)
    {
        if (offsets.Length != 128 || SourcePriceShape444(bands).Slots > SourcePriceCapacity)
            throw new ArgumentException("Source-price layout exceeds its bounded workspace.");
        var at = 0;
        for (var family = 0; family < 2; family++)
        {
            offsets[family * 64] = 0;
            var ends = bands.ForComponent(family) | (1UL << 63);
            for (var first = 1; first < 64;)
            {
                var last = BitOperations.TrailingZeroCount(ends & (ulong.MaxValue << first));
                var runs = Math.Min(16, last - first + 1);
                offsets.Slice(family * 64 + first, last - first + 1).Fill((ushort)(at | runs << 10));
                at += 2 + 10 * runs; first = last + 1;
            }
        }
    }

    // Allocation-free planning of the existing Original quantizers. The bounded
    // all-quality layout test compares this scalar formula with actual DQT values.
    internal static JointAcBands PlannedSourcePriceBands444(int quality)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quality, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quality, 100);
        var scale = quality < 50 ? 5000 / quality : 200 - 2 * quality;
        var precision = (16 * scale + 50) / 100;
        ulong y = 0, cbcr = 0;
        for (var family = 0; family < 2; family++)
        {
            var ends = 1UL << 63; var previous = -1;
            for (var scan = 1; scan < 64; scan++)
            {
                var natural = JpegTransform.Zigzag[scan]; var u = natural % 8; var v = natural / 8;
                var basis = family == 0 ? 1 + 2 * (u + v) + u * v : 2 + 3 * (u + v) + u * v;
                var q = Math.Clamp((basis * scale + 50) / 100, 1, 255);
                var level = PointTransform444(q, precision);
                if (previous >= 0 && previous != level) ends |= 1UL << (scan - 1);
                previous = level;
            }
            if (family == 0) y = ends; else cbcr = ends;
        }
        return new(y, cbcr);
    }
}
