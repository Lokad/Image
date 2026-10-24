using System.Numerics;

namespace Lokad.Image.Internal;

internal static class Vp8lReferenceEntropy
{
    private const long Scale = Vp8lReferenceCost.Scale;

    internal static long Log(int count)
    {
        if (count < 2) return 0;
        if (count < 256) return Vp8lReferenceCost.SmallLog(count);
        if (count >= 65536) return (long)Math.Floor(Scale / Math.Log(2) * Math.Log(count) + 0.5);
        var shift = BitOperations.Log2((uint)count) - 7;
        var value = Vp8lReferenceCost.SmallLog(count >> shift) + shift * Scale;
        if (count >= 4096) value += ((long)(Scale / Math.Log(2)) * (count & ((1 << shift) - 1)) + count / 2) / count;
        return value;
    }

    internal static long Population(ReadOnlySpan<int> counts, OperationContext context)
    {
        context.Work(counts.Length * 2L);
        var sum = 0; var nonzero = 0; var maximum = 0; long entropy = 0;
        foreach (var count in counts)
        {
            sum += count; if (count != 0) nonzero++; maximum = Math.Max(maximum, count);
            entropy -= Vp8lReferenceCost.SLog(count);
        }
        entropy += Vp8lReferenceCost.SLog(sum);
        long refined;
        if (nonzero <= 1) refined = 0;
        else if (nonzero == 2) refined = (99L * sum * Scale + entropy + 50) / 100;
        else
        {
            var mix = nonzero == 3 ? 950 : nonzero == 4 ? 700 : 627;
            var lower = ((Int128)mix * (2L * sum - maximum) * Scale + (Int128)(1000 - mix) * entropy + 500) / 1000;
            refined = Math.Max(entropy, (long)lower);
        }
        var zeroRuns = 0; var nonzeroRuns = 0; var shortZero = 0; var shortNonzero = 0; var longZero = 0; var longNonzero = 0;
        for (var start = 0; start < counts.Length;)
        {
            var end = start + 1;
            while (end < counts.Length && counts[end] == counts[start]) end++;
            var length = end - start;
            if (length > 3)
            {
                if (counts[start] == 0) { zeroRuns++; longZero += length; }
                else { nonzeroRuns++; longNonzero += length; }
            }
            else if (counts[start] == 0) shortZero += length; else shortNonzero += length;
            start = end;
        }
        var overhead = 57 * Scale - (91 * Scale + 5) / 10;
        overhead += (zeroRuns * 1600L + longZero * 240L + nonzeroRuns * 2640L + longNonzero * 720L
            + shortZero * 1840L + shortNonzero * 3360L) * (Scale >> 10);
        return refined + overhead;
    }

    internal static long PopulationSum(ReadOnlySpan<int> a, ReadOnlySpan<int> b, OperationContext context)
    {
        if (a.Length != b.Length) throw new ArgumentException("Population lengths must match.", nameof(b));
        // Keep the separate addition and population charges without materializing
        // their sum. Equal summed runs share one exact integer logarithm.
        context.Work(a.Length); context.Work(a.Length * 2L);
        var sum = 0; var nonzero = 0; var maximum = 0; long entropy = 0;
        var zeroRuns = 0; var nonzeroRuns = 0; var shortZero = 0; var shortNonzero = 0; var longZero = 0; var longNonzero = 0;
        for (var start = 0; start < a.Length;)
        {
            var count = a[start] + b[start]; var end = start + 1;
            while (end < a.Length && a[end] + b[end] == count) end++;
            var length = end - start;
            sum += count * length; if (count != 0) nonzero += length; maximum = Math.Max(maximum, count);
            entropy -= Vp8lReferenceCost.SLog(count) * length;
            if (length > 3)
            {
                if (count == 0) { zeroRuns++; longZero += length; }
                else { nonzeroRuns++; longNonzero += length; }
            }
            else if (count == 0) shortZero += length; else shortNonzero += length;
            start = end;
        }
        entropy += Vp8lReferenceCost.SLog(sum);
        long refined;
        if (nonzero <= 1) refined = 0;
        else if (nonzero == 2) refined = (99L * sum * Scale + entropy + 50) / 100;
        else
        {
            var mix = nonzero == 3 ? 950 : nonzero == 4 ? 700 : 627;
            var lower = ((Int128)mix * (2L * sum - maximum) * Scale + (Int128)(1000 - mix) * entropy + 500) / 1000;
            refined = Math.Max(entropy, (long)lower);
        }
        var overhead = 57 * Scale - (91 * Scale + 5) / 10;
        overhead += (zeroRuns * 1600L + longZero * 240L + nonzeroRuns * 2640L + longNonzero * 720L
            + shortZero * 1840L + shortNonzero * 3360L) * (Scale >> 10);
        return refined + overhead;
    }
}

internal static class Vp8lSymbols
{
    // RFC 9649 section 3.6.2: derive the 120 near-distance codes from geometry.
    // This small format constant is generated, never imported as a lookup table.
    private readonly record struct Neighbor(int X, int Y);
    private static readonly byte[] Planes = CreatePlanes();
    private static byte[] CreatePlanes()
    {
        Span<Neighbor> neighbors = stackalloc Neighbor[120]; var count = 0;
        for (var y = 0; y < 8; y++)
        for (var x = -7; x <= 8; x++) if (y != 0 || x > 0) neighbors[count++] = new(x, y);
        neighbors.Sort(static (a, b) =>
        {
            var comparison = (a.X * a.X + a.Y * a.Y).CompareTo(b.X * b.X + b.Y * b.Y);
            if (comparison != 0) return comparison;
            comparison = b.Y.CompareTo(a.Y);
            return comparison != 0 ? comparison : (a.X < 0).CompareTo(b.X < 0);
        });
        var result = new byte[128];
        for (var i = 0; i < neighbors.Length; i++) result[neighbors[i].Y * 16 + neighbors[i].X + 7] = (byte)(i + 1);
        return result;
    }

    internal static int Plane(int distance, int width)
    {
        var y = distance / width; var x = distance % width;
        // Direct coordinates precede the wrapped representation on narrow rows.
        if (x <= 8 && y < 8) return Planes[y * 16 + x + 7];
        if (x > width - 8 && y < 7) return Planes[(y + 1) * 16 + x - width + 7];
        return distance + 120;
    }

    internal static (int Code, int Extra) Prefix(int value)
    {
        if (value <= 4) return (Math.Max(0, value - 1), 0);
        var high = BitOperations.Log2((uint)(value - 1)); var extra = high - 1;
        return (2 * high + ((value - 1) >> extra & 1), extra);
    }
    internal static int Extra(int code) => code < 4 ? 0 : code / 2 - 1;
}
