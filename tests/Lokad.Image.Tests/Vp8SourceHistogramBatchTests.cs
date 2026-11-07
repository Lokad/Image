using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8SourceHistogramBatchTests
{
    [Fact]
    public void FourBlocksKeepEveryZeroMaskSignedBinsOverflowAndTails()
    {
        var workspace = new short[66]; workspace[0] = -139; workspace[65] = 149;
        var actual = new int[34]; var expected = new int[34];
        for (var mask = 0; mask < 65536; mask++)
        {
            for (var p = 0; p < actual.Length; p++) actual[p] = expected[p] = int.MaxValue - p % 5;
            for (var block = 0; block < 4; block++)
            for (var p = 0; p < 16; p++)
            {
                var bit = (p + block * 3) & 15;
                var bucket = (mask >> bit & 1) == 0 ? 0 : 1 + (p * 7 + block * 5 + (mask >> 8)) % 31;
                var value = bucket == 0 ? (short)((p & 1) == 0 ? -7 : 7)
                    : (short)((p & 1) == 0 ? -bucket * 8 : bucket * 8);
                workspace[1 + block * 16 + p] = value;
                expected[1 + bucket] = unchecked(expected[1 + bucket] + 1);
            }
            Vp8SourceHistogram.AddFourBlocks(workspace.AsSpan(1, 64), actual.AsSpan(1, 32));
            if (!actual.AsSpan().SequenceEqual(expected)) Assert.Equal(expected, actual);
        }
        Assert.Equal(-139, workspace[0]); Assert.Equal(149, workspace[65]);
    }

    [Fact]
    public void OverlappingPopulationsRetainFourCallSequentialFeedback()
    {
        for (var start = 1; start < 64; start++)
        {
            var actual = new int[160];
            for (var p = 0; p < actual.Length; p++) actual[p] = unchecked(p * 161279 + start * 7919);
            var expected = actual.ToArray();
            var expectedLevels = MemoryMarshal.Cast<int, short>(expected.AsSpan(32, 32));
            for (var block = 0; block < 4; block++)
                Vp8SourceHistogram.AddBlock(expectedLevels.Slice(block * 16, 16), expected.AsSpan(start, 32));
            Vp8SourceHistogram.AddFourBlocks(MemoryMarshal.Cast<int, short>(actual.AsSpan(32, 32)), actual.AsSpan(start, 32));
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void SignedExtremesAndReusedWorkspaceIntroduceNoManagedAllocation()
    {
        short[] domain = [short.MinValue, short.MaxValue, -248, 248, -247, 247, -16, 16, -15, 15, -8, 8, -7, 7, 0, 0];
        var source = Enumerable.Range(0, 64).Select(p => domain[p % domain.Length]).ToArray();
        var workspace = new short[64]; var counts = new int[32]; var expected = new int[32];
        foreach (var value in source) expected[Math.Min(31, Math.Abs((int)value) >> 3)]++;
        source.CopyTo(workspace, 0); Vp8SourceHistogram.AddFourBlocks(workspace, counts); Assert.Equal(expected, counts);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var trial = 0; trial < 1024; trial++)
        {
            source.AsSpan().CopyTo(workspace); counts.AsSpan().Clear();
            Vp8SourceHistogram.AddFourBlocks(workspace, counts);
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, allocated); Assert.Equal(expected, counts);
    }

    [Theory]
    [InlineData(63, 32)]
    [InlineData(64, 31)]
    public void InvalidBatchShapesRejectBeforeMutation(int levels, int buckets)
    {
        var workspace = Enumerable.Repeat((short)-173, 64).ToArray();
        var population = Enumerable.Repeat(179, 32).ToArray();
        Assert.Throws<ArgumentException>(() => Vp8SourceHistogram.AddFourBlocks(workspace.AsSpan(0, levels), population.AsSpan(0, buckets)));
        Assert.All(workspace, value => Assert.Equal(-173, value));
        Assert.All(population, value => Assert.Equal(179, value));
    }
}
