using System.Runtime.InteropServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8TokenStatisticsTests
{
    [Theory]
    [InlineData(0xfffdfffcu, 0xfffefffcu, 0xfffefffdu)]
    [InlineData(0xfffe0000u, 0x80000000u, 0x80000001u)]
    [InlineData(0xfffefffdu, 0x80007fffu, 0x80008000u)]
    [InlineData(0xfffefffeu, 0x80007fffu, 0x80008000u)]
    public void PackedCountsRescaleAtTheBoundaryWithRoundedOnes(uint initial, uint zero, uint one)
    {
        var data = new[] { initial, 0x12345678u };
        Vp8TokenStatistics.Event(data, 0, false); Assert.Equal(zero, data[0]);
        data[0] = initial; Vp8TokenStatistics.Event(data, 0, true); Assert.Equal(one, data[0]); Assert.Equal(0x12345678u, data[1]);
    }

    [Fact]
    public void TokenAndDirectRecordingHaveTheExplicitCategoryStatisticsDifference()
    {
        var levels = new short[18]; levels[0] = 67; levels[16] = -30000; levels[17] = 30000;
        var token = new uint[1058]; var direct = new uint[1058]; token[1056] = direct[1056] = 123; token[1057] = direct[1057] = 456;
        Assert.True(Vp8TokenStatistics.Record(levels, 3, 0, 0, token));
        Assert.True(Vp8TokenStatistics.Record(levels, 3, 0, 0, direct, tokenBuffer: false));
        const int first = 3 * 8 * 3 * 11;
        for (var p = 0; p < 1056; p++)
        {
            if (p == first + 9) Assert.Equal((0x10001u, 0u), (token[p], direct[p]));
            else if (p == first + 10) Assert.Equal((0u, 0x10001u), (token[p], direct[p]));
            else Assert.Equal(token[p], direct[p]);
        }
        Assert.Equal((123u, 456u), (token[1056], token[1057])); Assert.Equal((123u, 456u), (direct[1056], direct[1057]));
        Assert.Equal((67, -30000, 30000), ((int)levels[0], (int)levels[16], (int)levels[17]));
    }

    [Fact]
    public void HeaderCostCanRejectEmptyUpdatesAndAcceptStrongEvidenceWhilePreservingInputsAndTails()
    {
        using var root = new TestMemoryBudget(1_000_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var rate = Vp8RatePlan.Create(Vp8Tables.CoefficientProbabilities, context); var retained = root.Bytes;
        var statistics = new uint[1058]; statistics[1056] = 123; statistics[1057] = 456; var probabilities = new byte[1058];
        probabilities[1056] = 91; probabilities[1057] = 92;
        var header = Vp8TokenStatistics.Update(statistics, rate.Entropy, probabilities, out var changed);
        Assert.False(changed); Assert.True(probabilities.AsSpan(0, 1056).SequenceEqual(Vp8Tables.CoefficientProbabilities));
        statistics[0] = 5000u << 16; var original = statistics.ToArray();
        var newHeader = Vp8TokenStatistics.Update(statistics, rate.Entropy, probabilities, out changed);
        Assert.True(changed); Assert.Equal((byte)255, probabilities[0]); Assert.True(newHeader > header);
        Assert.Equal(original, statistics); Assert.Equal((91, 92), ((int)probabilities[1056], (int)probabilities[1057])); Assert.Equal(retained, root.Bytes);
    }

    [Fact]
    public void InvalidOrOverlappingInputsFailBeforeChangingCountersOrDestination()
    {
        var levels = new short[16]; var stats = new uint[1056]; stats[0] = 0x10001; var original = stats.ToArray();
        levels[15] = short.MinValue; Assert.Throws<ArgumentException>(() => Vp8TokenStatistics.Record(levels, 3, 0, 0, stats));
        levels[15] = 0; levels[0] = 1; Assert.Throws<ArgumentException>(() => Vp8TokenStatistics.Record(levels, 0, 1, 0, stats));
        levels[0] = 0; Assert.Throws<ArgumentException>(() => Vp8TokenStatistics.Record(levels, 4, 0, 0, stats));
        Assert.Throws<ArgumentException>(() => Vp8TokenStatistics.Record(levels, 0, 0, 3, stats));
        Assert.Throws<ArgumentException>(() => Vp8TokenStatistics.Record(new short[15], 0, 0, 0, stats)); Assert.Equal(original, stats);
        Assert.Throws<ArgumentException>(() => Vp8TokenStatistics.Record(MemoryMarshal.Cast<uint, short>(stats.AsSpan()), 0, 0, 0, stats)); Assert.Equal(original, stats);
        var entropy = new ushort[256]; var destination = Enumerable.Repeat((byte)77, 1058).ToArray();
        Assert.Throws<ArgumentException>(() => Vp8TokenStatistics.Update(stats.AsSpan(0, 1055), entropy, destination, out _));
        stats[0] = 0x10002; Assert.Throws<ArgumentException>(() => Vp8TokenStatistics.Update(stats, entropy, destination, out _));
        Assert.All(destination, value => Assert.Equal((byte)77, value)); stats[0] = original[0];
        Assert.Throws<ArgumentException>(() => Vp8TokenStatistics.Update(stats, entropy, MemoryMarshal.AsBytes(stats.AsSpan()), out _)); Assert.Equal(original, stats);
    }
}
