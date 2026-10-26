using System.Runtime.InteropServices;

namespace Lokad.Image.Internal;

// Borrowed frame-level adaptive state. The enclosing encoder reserves the
// counter/probability capacities and admits cancellation in block batches.
// RFC6386 section13 supplies coefficient branches and update probabilities.
internal static class Vp8TokenStatistics
{
    private static int Band(int position) => position switch
    { 0 => 0, 1 => 1, 2 => 2, 3 => 3, 5 => 4, 6 => 5, 15 => 7, _ => 6 };

    internal static void Event(Span<uint> statistics, int index, bool bit)
    {
        var packed = statistics[index];
        if (packed >= 0xfffe0000u) packed = (unchecked(packed + 1u) >> 1) & 0x7fff7fff;
        statistics[index] = packed + 65536u + (bit ? 1u : 0u);
    }

    internal static bool Record(ReadOnlySpan<short> levels, int type, int first, int initialContext,
        Span<uint> statistics, bool tokenBuffer = true)
    {
        if (levels.Length < 16 || statistics.Length < 1056 || (uint)type > 3 || (uint)first > 1 || (uint)initialContext > 2
            || first == 1 && levels[0] != 0) throw new ArgumentException("Invalid VP8 coefficient statistics input.");
        if (MemoryMarshal.AsBytes(levels[..16]).Overlaps(MemoryMarshal.AsBytes(statistics[..1056])))
            throw new ArgumentException("Coefficient inputs must not overlap writable statistics.");
        var last = -1;
        for (var p = first; p < 16; p++)
        {
            if (levels[p] is < -2047 or > 2047) throw new ArgumentException("VP8 residual level exceeds 2,047.");
            if (levels[p] != 0) last = p;
        }
        var n = first; var at = ((type * 8 + Band(n)) * 3 + initialContext) * 11;
        Event(statistics, at, last >= 0); if (last < 0) return false;
        while (n < 16)
        {
            var level = Math.Abs((int)levels[n++]); Event(statistics, at + 1, level != 0);
            if (level == 0) { at = ((type * 8 + Band(n)) * 3) * 11; continue; }
            Event(statistics, at + 2, level > 1);
            if (level > 1)
            {
                Event(statistics, at + 3, level > 4);
                if (level <= 4)
                {
                    Event(statistics, at + 4, level != 2);
                    if (level != 2) Event(statistics, at + 5, level == 4);
                }
                else
                {
                    Event(statistics, at + 6, level > 10);
                    if (level <= 10) Event(statistics, at + 7, level > 6);
                    else
                    {
                        Event(statistics, at + 8, level >= 35);
                        // The pinned token-buffer policy records node10's
                        // category5/6 event into counter9; emitted syntax still
                        // addresses probability10. Direct recording differs.
                        Event(statistics, at + (level < 35 || tokenBuffer ? 9 : 10), level >= (level < 35 ? 19 : 67));
                    }
                }
            }
            if (n == 16) return true;
            at = ((type * 8 + Band(n)) * 3 + Math.Min(level, 2)) * 11;
            Event(statistics, at, n <= last); if (n > last) return true;
        }
        return true;
    }

    private static int Bit(ReadOnlySpan<ushort> entropy, bool bit, int probability)
        => entropy[bit ? 255 - probability : probability];

    internal static int Update(ReadOnlySpan<uint> statistics, ReadOnlySpan<ushort> entropy, Span<byte> probabilities, out bool changed)
    {
        if (statistics.Length < 1056 || entropy.Length < 256 || probabilities.Length < 1056)
            throw new ArgumentException("Incomplete VP8 probability update state.");
        if (MemoryMarshal.AsBytes(statistics[..1056]).Overlaps(probabilities[..1056])
            || MemoryMarshal.AsBytes(entropy[..256]).Overlaps(probabilities[..1056]))
            throw new ArgumentException("Probability output must not overlap its inputs.");
        foreach (var packed in statistics[..1056])
            if ((packed & 65535) > packed >> 16) throw new ArgumentException("Coefficient event count exceeds its total.");
        changed = false; var header = 0;
        var defaults = Vp8Tables.CoefficientProbabilities; var updates = Vp8Tables.CoefficientUpdateProbabilities;
        for (var p = 0; p < 1056; p++)
        {
            var ones = (int)(statistics[p] & 65535); var total = (int)(statistics[p] >> 16); var old = defaults[p];
            var candidate = ones == 0 ? 255 : 255 - ones * 255 / total;
            var oldCost = ones * Bit(entropy, true, old) + (total - ones) * Bit(entropy, false, old) + Bit(entropy, false, updates[p]);
            var newCost = ones * Bit(entropy, true, candidate) + (total - ones) * Bit(entropy, false, candidate) + Bit(entropy, true, updates[p]) + 8 * 256;
            var useNew = oldCost > newCost;
            probabilities[p] = (byte)(useNew ? candidate : old);
            changed |= probabilities[p] != old;
            header += Bit(entropy, useNew, updates[p]) + (useNew ? 8 * 256 : 0);
        }
        return header;
    }
}
