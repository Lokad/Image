using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8TokenStatisticsDomainTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void EveryPositionPreservesTerminationAndRejectsOutOfRangeBeforeMutation(int first)
    {
        var levels = new short[18]; var actual = new uint[1058]; var expected = new uint[1058];
        short[] values = [short.MinValue, -2048, -2047, -67, -1, 0, 1, 67, 2047, 2048, short.MaxValue];
        static int Band(int p) => p switch { 0 => 0, 1 => 1, 2 => 2, 3 => 3, 5 => 4, 6 => 5, 15 => 7, _ => 6 };
        for (var position = first; position < 16; position++) foreach (var signed in values)
        {
            Array.Clear(levels); levels[position] = signed; levels[16] = short.MinValue; levels[17] = short.MaxValue;
            Array.Fill(actual, 0xfffefffdu); Array.Fill(expected, 0xfffefffdu);
            var type = position % 4; var initial = position % 3; var tokenBuffer = (position & 1) != 0;
            if (signed is < -2047 or > 2047)
            {
                Assert.Throws<ArgumentException>(() => Vp8TokenStatistics.Record(levels, type, first, initial, actual, tokenBuffer));
                Assert.Equal(expected, actual); continue;
            }
            void Event(int index, bool bit)
            {
                var packed = expected[index];
                if (packed >= 0xfffe0000u) packed = (unchecked(packed + 1u) >> 1) & 0x7fff7fff;
                expected[index] = packed + 65536u + (bit ? 1u : 0u);
            }
            var at = ((type * 8 + Band(first)) * 3 + initial) * 11;
            Event(at, signed != 0);
            if (signed != 0)
            {
                for (var p = first; p < position; p++)
                { Event(at + 1, false); at = (type * 8 + Band(p + 1)) * 33; }
                Event(at + 1, true);
                foreach (var (node, bit) in Branches(Math.Abs((int)signed)))
                    Event(at + (tokenBuffer && node == 10 ? 9 : node), bit);
                if (position < 15) Event(((type * 8 + Band(position + 1)) * 3 + Math.Min(Math.Abs((int)signed), 2)) * 11, false);
            }
            Assert.Equal(signed != 0, Vp8TokenStatistics.Record(levels, type, first, initial, actual, tokenBuffer));
            Assert.Equal(expected, actual); Assert.Equal(short.MinValue, levels[16]); Assert.Equal(short.MaxValue, levels[17]);
        }
    }

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
