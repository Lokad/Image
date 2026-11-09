using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lHuffmanWorkspaceTests
{
    [Fact]
    public void DirtyWorkspaceReuseKeepsFreshTreesAndIndependentSurvivingCodes()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var workspace = new Vp8lHuffmanWorkspace(1304, context);
        var scratch = root.Bytes; long retained = 0;
        var codes = new List<Vp8lHuffmanCode>(); var snapshots = new List<byte[]>();
        try
        {
            foreach (var count in new[] { 1304, 19, 256, 0, 40, 1304, 3 })
            foreach (var shape in new[] { 0, 1, 2, 3 })
            {
                for (var region = 0; region < 5; region++) workspace.Region(region, 1304).Fill(int.MinValue);
                workspace.ProtectedRun(1304).Fill(true);
                var populations = Enumerable.Range(0, count).Select(i => shape switch
                { 0 => 0, 1 => i == count / 2 ? 7 : 0, 2 => 1, _ => (i * 13 + 5) % 31 }).ToArray();
                var original = populations.ToArray();
                using var fresh = Vp8lHuffmanCode.Build(populations, context, out var expectedBits);
                var code = Vp8lHuffmanCode.Build(populations, context, out var actualBits, workspace: workspace);
                codes.Add(code); snapshots.Add(code.Lengths.ToArray()); retained += count * 3L;
                Assert.Equal(fresh.Lengths.ToArray(), code.Lengths.ToArray()); Assert.Equal(expectedBits, actualBits);
                Assert.Equal(original, populations); fresh.Dispose(); Assert.Equal(scratch + retained, root.Bytes);
            }
            workspace.Dispose(); workspace.Dispose(); Assert.Equal(retained, root.Bytes);
            for (var i = 0; i < codes.Count; i++) Assert.Equal(snapshots[i], codes[i].Lengths.ToArray());
        }
        finally { foreach (var code in codes) code.Dispose(); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void BorrowersMustShareTheOperationAndFitTheAdmittedCapacity()
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var other = new OperationContext(new() { MemoryBudget = root }, default);
        using var workspace = new Vp8lHuffmanWorkspace(3, context); var bytes = root.Bytes;
        Assert.Throws<ArgumentException>(() => Vp8lHuffmanCode.Build([1, 2, 3, 4], context, out _, workspace: workspace));
        Assert.Throws<ArgumentException>(() => Vp8lHuffmanCode.Build([1], other, out _, workspace: workspace));
        Assert.Equal(bytes, root.Bytes);
        workspace.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => Vp8lHuffmanCode.Build([1], context, out _, workspace: workspace));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(11)]
    public void GroupRefusalCleansWorkspaceAndPartialCodesWithoutRetry(int phase)
    {
        using var root = new TestMemoryBudget(1_000_000) { RefuseAtGrowth = phase };
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var layout = new Vp8lHistogramLayout(10); var populations = Enumerable.Repeat(1, layout.Total).ToArray();
        var failure = Assert.Throws<ImageLimitException>(() => Vp8lHuffmanGroup.Build(populations, layout, context, out _));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (failure.Limit, failure.Scope));
        Assert.Equal(phase, root.GrowthAttempts); Assert.Equal(1, root.Refusals); Assert.Equal(0, root.Bytes);
        root.RefuseAtGrowth = 0;
        using var success = Vp8lHuffmanGroup.Build(populations, layout, context, out _);
        Assert.Equal(layout.Total * 3L, root.Bytes);
        success.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void InvalidAlphabetDoesNotPoisonBorrowedScratchOrRetainedCodes()
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var workspace = new Vp8lHuffmanWorkspace(32, context);
        using var first = Vp8lHuffmanCode.Build([1, 1, 1], context, out _, workspace: workspace);
        var bytes = root.Bytes;
        Assert.Throws<ArgumentException>(() => Vp8lHuffmanCode.Build([1, 1, 1], context, out _, 1, workspace));
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8lHuffmanCode.Build([1, -1], context, out _, workspace: workspace));
        Assert.Equal(bytes, root.Bytes);
        using var next = Vp8lHuffmanCode.Build([0, 0, 7], context, out _, workspace: workspace);
        workspace.Dispose(); Assert.Equal(new byte[] { 1, 2, 2 }, first.Lengths.ToArray());
        Assert.Equal(new byte[] { 0, 0, 1 }, next.Lengths.ToArray()); Assert.Equal(18, root.Bytes);
    }

    [Fact]
    public void CancellationDuringGroupAdmissionReleasesEveryOwnedScopeAndAllowsRecovery()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => { if (root.GrowthAttempts == 6) cancellation.Cancel(); };
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        var layout = new Vp8lHistogramLayout(10); var populations = Enumerable.Repeat(1, layout.Total).ToArray();
        var failure = Assert.Throws<OperationCanceledException>(() => Vp8lHuffmanGroup.Build(populations, layout, context, out _));
        Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lHuffmanGroup.Build(populations, layout, nextContext, out _);
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
