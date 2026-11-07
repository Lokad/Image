using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8CoefficientProgramTests
{
    private delegate bool Encode(ref Vp8PartitionWriter.Cursor writer, ReadOnlySpan<short> levels,
        int type, int first, int initialContext, ReadOnlySpan<byte> probabilities);
    private static readonly Encode Actual = typeof(Vp8FrameEncoding).GetMethod("Coefficients", BindingFlags.NonPublic | BindingFlags.Static)!.CreateDelegate<Encode>();

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void InstructionsMatchDirectStatisticsAndActualWriterAcrossSignedCategoriesAndContexts(int type)
    {
        using var root = new TestMemoryBudget(16_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        short[] domain = [0, 1, -1, 2, -2, 3, 4, 5, 6, 7, 10, 11, 18, 19, 34, 35, 66, 67, 127, 255, 511, 1023, 2047, -2047];
        var probabilities = Enumerable.Range(0, 1056).Select(i => (byte)(1 + (i * 73 % 254))).ToArray();
        using var program = new Vp8CoefficientProgram(context);
        using var expectedWriter = new Vp8PartitionWriter(context, 4_000_000);
        using var actualWriter = new Vp8PartitionWriter(context, 4_000_000);
        var expectedCursor = expectedWriter.Borrow(); var actualCursor = actualWriter.Borrow();
        var blocks = 0;
        foreach (var first in new[] { 0, 1 }) foreach (var initial in new[] { 0, 1, 2 })
        foreach (var position in Enumerable.Range(first, 16 - first)) foreach (var magnitude in domain)
        {
            var levels = new short[18]; levels[position] = magnitude; levels[16] = -30000; levels[17] = 30000;
            var unchanged = levels.ToArray(); var expected = new uint[1058];
            for (var i = 0; i < 1056; i++) if (i % 7 == 0) expected[i] = 0xfffd7fff;
            expected[1056] = 123; expected[1057] = 456; var actual = expected.ToArray();
            Assert.Equal(Vp8TokenStatistics.Record(levels, type, first, initial, expected, tokenBuffer: false),
                program.Record(levels, type, first, initial, actual));
            Assert.Equal(expected, actual); Assert.Equal(unchanged, levels);
            Actual(ref expectedCursor, levels, type, first, initial, probabilities);
            program.EndBlock(); blocks++;
        }
        var cursor = program.Borrow();
        for (var i = 0; i < blocks; i++) cursor.WriteBlock(ref actualCursor, probabilities, skip: false);
        expectedCursor.Commit(); actualCursor.Commit(); expectedWriter.Finish(); actualWriter.Finish();
        Assert.Equal(expectedWriter.Memory.ToArray(), actualWriter.Memory.ToArray());
        program.Dispose(); expectedWriter.Dispose(); actualWriter.Dispose();
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void InvalidInputsDoNotChangeCountersOrOwnedStorage()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var program = new Vp8CoefficientProgram(context);
        var levels = new short[16]; var statistics = new uint[1056]; statistics[0] = 0x10001;
        var original = statistics.ToArray(); var baseline = root.Bytes;
        levels[15] = short.MinValue;
        Assert.Throws<ArgumentException>(() => program.Record(levels, 0, 0, 0, statistics));
        levels[15] = 0; levels[0] = 1;
        Assert.Throws<ArgumentException>(() => program.Record(levels, 0, 1, 0, statistics));
        levels[0] = 0;
        Assert.Throws<ArgumentException>(() => program.Record(levels, 4, 0, 0, statistics));
        Assert.Equal(original, statistics); Assert.Equal(baseline, root.Bytes);
        program.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)]
    public void EntireSignedLevelDomainPreservesTheActualPartition(int first)
    {
        using var root = new TestMemoryBudget(16_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var program = new Vp8CoefficientProgram(context);
        using var expected = new Vp8PartitionWriter(context, 4_000_000);
        using var actual = new Vp8PartitionWriter(context, 4_000_000);
        var expectedCursor = expected.Borrow(); var actualCursor = actual.Borrow();
        var statistics = new uint[1056]; var reference = new uint[1056];
        var levels = new short[16];
        var type = first == 0 ? 3 : 0;
        for (var value = -2047; value <= 2047; value++)
        {
            levels[15] = (short)value;
            Assert.Equal(Vp8TokenStatistics.Record(levels, type, first, 2, reference, tokenBuffer: false),
                program.Record(levels, type, first, 2, statistics));
            Actual(ref expectedCursor, levels, type, first, 2, Vp8Tables.CoefficientProbabilities);
            program.EndBlock();
        }
        Assert.Equal(reference, statistics);
        var cursor = program.Borrow();
        for (var value = -2047; value <= 2047; value++) cursor.WriteBlock(ref actualCursor, Vp8Tables.CoefficientProbabilities, skip: false);
        expectedCursor.Commit(); actualCursor.Commit(); expected.Finish(); actual.Finish();
        Assert.Equal(expected.Memory.ToArray(), actual.Memory.ToArray());
        program.Dispose(); expected.Dispose(); actual.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
