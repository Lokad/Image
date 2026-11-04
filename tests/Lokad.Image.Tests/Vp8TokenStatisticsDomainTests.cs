using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8TokenStatisticsDomainTests
{
    // An independent branch oracle retains the RFC6386 tree, rather than
    // deriving expectations from the compact production representation.
    private static List<(int Node, bool Bit)> Branches(int level)
    {
        List<(int, bool)> events = [(2, level > 1)];
        if (level <= 1) return events;
        events.Add((3, level > 4));
        if (level <= 4)
        {
            events.Add((4, level != 2));
            if (level != 2) events.Add((5, level == 4));
        }
        else
        {
            events.Add((6, level > 10));
            if (level <= 10) events.Add((7, level > 6));
            else
            {
                events.Add((8, level >= 35));
                events.Add((level < 35 ? 9 : 10, level >= (level < 35 ? 19 : 67)));
            }
        }
        return events;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EverySignedMagnitudePreservesContextualStatisticsAndRescaling(bool tokenBuffer)
    {
        var levels = new short[18]; var actual = new uint[1058]; var expected = new uint[1058];
        for (var signed = -2047; signed <= 2047; signed++)
        {
            if (signed == 0) continue;
            var level = Math.Abs(signed); var type = level % 4; var first = level % 2; var initial = level % 3;
            Array.Clear(levels); levels[first] = (short)signed; levels[16] = short.MinValue;
            levels[17] = short.MaxValue;
            Array.Fill(actual, 0xfffefffdu); Array.Fill(expected, 0xfffefffdu);
            void Event(int index, bool bit)
            {
                var packed = expected[index];
                if (packed >= 0xfffe0000u) packed = (unchecked(packed + 1u) >> 1) & 0x7fff7fff;
                expected[index] = packed + 65536u + (bit ? 1u : 0u);
            }
            var band = first; var at = ((type * 8 + band) * 3 + initial) * 11;
            Event(at, true); Event(at + 1, true);
            foreach (var (node, bit) in Branches(level))
                Event(at + (tokenBuffer && node == 10 ? 9 : node), bit);
            var nextBand = first == 0 ? 1 : 2;
            Event(((type * 8 + nextBand) * 3 + Math.Min(level, 2)) * 11, false);
            Assert.True(Vp8TokenStatistics.Record(levels, type, first, initial, actual, tokenBuffer));
            Assert.Equal(expected, actual); Assert.Equal((short)signed, levels[first]);
            Assert.Equal(short.MinValue, levels[16]); Assert.Equal(short.MaxValue, levels[17]);
        }
    }
}
