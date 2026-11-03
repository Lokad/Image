using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lPreparedPopulationTests
{
    [Fact]
    public void PreparedRepairsMatchFullRunCostsAtBoundariesAndLargeCounts()
    {
        using var context = new OperationContext(new ImageLimits(), default);
        foreach (var size in new[] { 0, 1, 2, 3, 4, 5, 40, 256, 280, 1304 })
        for (var pattern = 0; pattern < 8; pattern++)
        for (var edits = 0; edits < 8; edits++)
        {
            var a = new int[size]; var b = new int[size]; var packed = new uint[size]; var used = 0;
            for (var i = 0; i < size; i++)
            {
                a[i] = pattern switch
                {
                    0 => 0, 1 => 3, 2 => i / 4 % 3, 3 => i % 2 * 5, 4 => i % 19,
                    5 => (i * 31337 ^ i * 997) % 61, 6 => i == size / 2 ? 1 << 20 : 0, _ => i / 3 % 3 * 65536
                };
                b[i] = edits switch
                {
                    0 => 0, 1 => i == 0 ? 1 : 0, 2 => i == size - 1 ? 3 : 0,
                    3 => i >= size / 3 && i < size / 3 + 5 ? 2 : 0, 4 => 1,
                    5 => i % 7 == 0 ? i % 13 + 1 : 0, 6 => i == size / 2 ? 1 << 18 : 0, _ => i % 4 == 0 ? 6 : 0
                };
                if (b[i] != 0) packed[used++] = (uint)b[i] << 12 | (uint)(31 + i);
            }
            var runs = new uint[size]; var basis = Vp8lReferenceEntropy.PrepareRuns(a, runs, context);
            Assert.Equal(Vp8lReferenceEntropy.PopulationSum(a, b, context),
                Vp8lReferenceEntropy.PreparedPopulationSum(a, runs, basis, packed.AsSpan(0, used), 31, context));
        }
    }

    [Fact]
    public void PreparedStorageTracksActualViewsTransfersMergesAndRecycledCapacity()
    {
        using var root = new TestMemoryBudget(1 << 24);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        var layout = new Vp8lHistogramLayout(0);
        using var owner = Vp8lPopulationWorkspace.Create(Tokens(), 17, 17, 2, layout, 12, context);
        Check(0, 1); Check(2, 3); Check(0, 1); // Changing a scratch view invalidates its basis.
        owner.Merge(0, 1); Check(0, 2);
        owner.Merge(3, 0); Check(3, 2); // B's prepared physical block transfers to A.
        owner.Merge(4, 5); Check(4, 2); var capacity = root.Bytes;
        owner.Merge(3, 4); Check(3, 2);
        owner.Merge(6, 7); Check(6, 2); // Counts and prepared capacity are recycled together.
        Assert.Equal(capacity, root.Bytes);
        var leaf = Green(owner, 2, layout); var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 128; i++) owner.SumGreenLeaf(3, leaf);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        owner.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);

        void Check(int a, int b)
        {
            var counts = owner.Read(a, 0); var other = owner.Read(b, 1, true);
            Assert.Equal(Vp8lReferenceEntropy.PopulationSum(counts[..layout.GreenSize], other[..layout.GreenSize], context),
                owner.SumGreenLeaf(a, Green(owner, b, layout)));
        }
    }

    [Fact]
    public void PreparedAdmissionRefusalReleasesUnusedCapacityAndKeepsTheRootBorrowed()
    {
        using var root = new TestMemoryBudget(1 << 24);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        var layout = new Vp8lHistogramLayout(0);
        using var owner = Vp8lPopulationWorkspace.Create(Tokens(), 17, 17, 2, layout, 12, context);
        var before = root.Bytes; root.RefuseAtGrowth = root.GrowthAttempts + 1;
        var error = Assert.Throws<ImageLimitException>(() => owner.SumGreenLeaf(0, Green(owner, 1, layout)));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(before, root.Bytes); Assert.Equal(1, root.Refusals);
        owner.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var next = Vp8lPopulationWorkspace.Create(Tokens(), 17, 17, 2, layout, 12, context);
        next.SumGreenLeaf(0, Green(next, 1, layout)); next.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void RefusalAfterReferenceAdmissionKeepsActualCapacityUntilCleanup()
    {
        using var root = new TestMemoryBudget(1 << 24);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        var layout = new Vp8lHistogramLayout(10);
        using var owner = Vp8lPopulationWorkspace.Create(Tokens(), 17, 17, 2, layout, 12, context);
        owner.Merge(0, 1); var before = root.Bytes;
        root.RefuseAtGrowth = root.GrowthAttempts + 2; // Admit references, refuse their first run map.
        var error = Assert.Throws<ImageLimitException>(() => owner.SumGreenLeaf(0, Green(owner, 2, layout)));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.True(root.Bytes > before); Assert.Equal(1, root.Refusals);
        owner.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var next = Vp8lPopulationWorkspace.Create(Tokens(), 17, 17, 2, layout, 12, context);
        next.Merge(0, 1); next.SumGreenLeaf(0, Green(next, 2, layout));
        next.Dispose(); Assert.Equal(0, root.Bytes);
    }

    [Fact]
    public void CancellationKeepsPreparedStorageChargedUntilItsOwnerIsDisposed()
    {
        using var root = new TestMemoryBudget(1 << 24); using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, cancellation.Token);
        var layout = new Vp8lHistogramLayout(0);
        using var owner = Vp8lPopulationWorkspace.Create(Tokens(), 17, 17, 2, layout, 12, context);
        owner.SumGreenLeaf(0, Green(owner, 1, layout)); var capacity = root.Bytes;
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => owner.SumGreenLeaf(0, Green(owner, 1, layout)));
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceEntropy.PrepareRuns([], [], context));
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceEntropy.PreparedPopulationSum([], [], default, [1u << 12], 0, context));
        Assert.Equal(capacity, root.Bytes); owner.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static ReadOnlySpan<uint> Green(Vp8lPopulationWorkspace owner, int tile, Vp8lHistogramLayout layout)
    {
        var entries = owner.Leaf(tile); var used = 0;
        while (used < entries.Length && (entries[used] & 4095) < layout.GreenSize) used++;
        return entries[..used];
    }

    private static Vp8lToken[] Tokens()
        => Enumerable.Range(0, 17 * 17).Select(i => Vp8lToken.Literal(0xff000000u | (uint)(i * 31337) & 0xffffffu)).ToArray();
}
