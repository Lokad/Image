using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lPopulationWorkspaceTests
{
    [Fact]
    public void ImmutableLeavesSurviveDenseTransfersAndRecycledSlots()
    {
        using var root = new TestMemoryBudget(1 << 24);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        var layout = new Vp8lHistogramLayout(0); var tokens = Tokens(17 * 17);
        using var owner = Vp8lPopulationWorkspace.Create(tokens, 17, 17, 2, layout, 12, context);
        var originals = Enumerable.Range(0, 25).Select(i => owner.Read(i, 0, true).ToArray()).ToArray();
        owner.Merge(0, 1); owner.Merge(2, 0); // Transfer B's dense storage to logical A.
        owner.Merge(3, 4); var capacity = root.Bytes;
        owner.Merge(2, 3); owner.Merge(5, 6); // Release then recycle the other dense slot.
        Assert.Equal(capacity, root.Bytes);
        Assert.Equal(Sum(0, 1, 2, 3, 4), owner.Read(2, 0).ToArray());
        Assert.Equal(Sum(5, 6), owner.Read(5, 0).ToArray());
        for (var i = 0; i < 25; i++) Assert.Equal(originals[i], owner.Read(i, 1, true).ToArray());
        var words = new ulong[4];
        foreach (var tile in new[] { 2, 5 })
        {
            var counts = owner.Read(tile, 0); var support = owner.Support(tile, 0);
            foreach (var (start, offset) in new[] { (layout.Red, 0), (layout.Blue, 4), (layout.Alpha, 8) })
            {
                Vp8lReferenceEntropy.PrepareColorSupport(counts.Slice(start, 256), words, context);
                Assert.Equal(words, support.Slice(offset, 4).ToArray());
            }
        }
        owner.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        int[] Sum(params int[] ids)
        {
            var result = new int[layout.Total];
            foreach (var id in ids) for (var i = 0; i < result.Length; i++) result[i] += originals[id][i];
            return result;
        }
    }

    [Fact]
    public void CompactLeafRetainsFullCountWidthAndReleasesPreparationStorage()
    {
        using var root = new TestMemoryBudget(1 << 24);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        var layout = new Vp8lHistogramLayout(10);
        var tokens = Enumerable.Repeat(Vp8lToken.Literal(0xff102030), 257 * 257).ToArray();
        using var owner = Vp8lPopulationWorkspace.Create(tokens, 257, 257, 9, layout, 12, context);
        var expected = new int[layout.Total]; layout.Fill(tokens, 257, true, expected, context);
        Assert.Equal(expected, owner.Read(0, 0, true).ToArray());
        Assert.Equal(257 * 257, owner.Read(0, 0)[layout.Alpha + 255]);
        Assert.True(root.Peak > root.Bytes); // Temporary row indexes end before the owner.
        Assert.False(owner.IsEmpty(0)); owner.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void DenseAdmissionRefusalCleansReservationsAndAllowsANewOperation()
    {
        using var root = new TestMemoryBudget(1 << 24);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        var tokens = Tokens(17 * 17); var layout = new Vp8lHistogramLayout(0);
        var owner = Vp8lPopulationWorkspace.Create(tokens, 17, 17, 2, layout, 12, context);
        var before = root.Bytes; root.RefuseAtGrowth = root.GrowthAttempts + 1;
        Assert.Throws<ImageLimitException>(() => owner.Merge(0, 1));
        Assert.Equal(before, root.Bytes); Assert.Equal(1, root.Refusals);
        owner.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var next = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        using var subsequent = Vp8lPopulationWorkspace.Create(tokens, 17, 17, 2, layout, 12, next);
        subsequent.Merge(0, 1); subsequent.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void SortedLeafRunCostsMatchDenseBanksWithoutOwningStorage()
    {
        using var root = new TestMemoryBudget(0);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, cancellation.Token);
        foreach (var (size, start) in new[] { (40, 2072), (256, 1304), (280, 0), (312, 0), (1304, 0) })
        for (var pattern = 0; pattern < 12; pattern++)
        {
            var counts = new int[size]; var entries = new uint[size]; var used = 0;
            for (var i = 0; i < size; i++)
            {
                var value = pattern == 0 ? 0 : (i / pattern) % 4 == 0 ? 0 : (i / pattern) % 5 + 1;
                if (pattern == 11 && i == size - 1) value = 1 << 18;
                counts[i] = value;
                if (value != 0) entries[used++] = (uint)value << 12 | (uint)(start + i);
            }
            var expected = Vp8lReferenceEntropy.Population(counts, context);
            Assert.Equal(expected, Vp8lReferenceEntropy.SparsePopulation(entries.AsSpan(0, used), start, size, context));
        }
        uint[] sample = [1u << 12, 3u << 12 | 64, 7u << 12 | 255];
        for (var i = 0; i < 128; i++) Vp8lReferenceEntropy.SparsePopulation(sample, 0, 280, context);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 128; i++) Vp8lReferenceEntropy.SparsePopulation(sample, 0, 280, context);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before); Assert.Equal(0, root.Bytes);
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceEntropy.SparsePopulation([], 0, 280, context));
    }

    [Fact]
    public void CancellationCleansOwnersWithoutReadingFurtherInputs()
    {
        using var root = new TestMemoryBudget(1 << 24);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, cancellation.Token);
        var layout = new Vp8lHistogramLayout(0);
        var owner = Vp8lPopulationWorkspace.Create(Tokens(17 * 17), 17, 17, 2, layout, 12, context);
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => owner.Merge(0, 1)); owner.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Throws<OperationCanceledException>(() => Vp8lPopulationWorkspace.Create([], 1, 1, 2, layout, 0, context));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static Vp8lToken[] Tokens(int count)
        => Enumerable.Range(0, count).Select(i => Vp8lToken.Literal(0xff000000u | (uint)(i * 31337) & 0xffffffu)).ToArray();
}
