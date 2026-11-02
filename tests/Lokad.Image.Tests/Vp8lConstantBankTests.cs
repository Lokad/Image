using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lConstantBankTests
{
    [Fact]
    public void PreparedLogicalCostsMatchEverySymbolAndCountBoundaryAndRestoreScratch()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var scratch = new int[256]; var counts = new int[256]; var other = new int[256];
        var empty = Vp8lReferenceEntropy.Population(counts, context);
        foreach (var symbol in Enumerable.Range(0, 256))
        {
            var cost = Vp8lReferenceClustering.ConstantPopulationCost(scratch, symbol, context);
            Assert.All(scratch, count => Assert.Equal(0, count));
            foreach (var count in new[] { 1, 2, 3, 4, 255, 256, 257, 65535, 65536, 1 << 28 })
            {
                counts[symbol] = count;
                Assert.Equal(cost, Vp8lReferenceEntropy.Population(counts, context));
                Assert.Equal(cost, Vp8lReferenceEntropy.PopulationSum(counts, other, context));
                other[symbol] = 1;
                Assert.Equal(cost, Vp8lReferenceEntropy.PopulationSum(counts, other, context));
                other[symbol] = 0; counts[symbol] = 0;
            }
            Assert.Equal(empty, Vp8lReferenceEntropy.Population(counts, context));
        }
        using var cancellation = new CancellationTokenSource();
        using var canceled = new OperationContext(ImageLimits.Default, cancellation.Token);
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceClustering.ConstantPopulationCost(scratch, 94, canceled));
        Assert.All(scratch, count => Assert.Equal(0, count));
    }

    [Fact]
    public void PhysicalLayoutsExpandAllBankCombinationsAtTrueSymbolsWithoutChangingSources()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var full = new Vp8lHistogramLayout(2);
        Assert.Equal(new Vp8lHistogramLayout(0).Total, default(Vp8lHistogramLayout).Total);
        Assert.Equal(0, default(Vp8lHistogramLayout).ConstantBanks);
        const uint literal = 0xae5e7b27u;
        Vp8lToken[] tokens = [Vp8lToken.Literal(literal), Vp8lToken.Copy(9, 1), Vp8lToken.Cached(3), Vp8lToken.Literal(literal)];
        var expected = new int[full.Total]; full.Fill(tokens, 65, true, expected, context);
        foreach (var mask in new[] { 0, 2, 4, 6, 8, 10, 12, 14 })
        {
            var layout = new Vp8lHistogramLayout(2, mask); var physical = new int[layout.Total];
            layout.Fill(tokens, 65, true, physical, context); var original = (int[])physical.Clone();
            var output = new int[full.Total]; layout.AddToFull(output, physical, literal, context);
            Assert.Equal(expected, output); Assert.Equal(original, physical);
            layout.AddToFull(output, physical, literal, context);
            Assert.Equal(expected.Select(count => 2 * count), output);
            Assert.Equal(expected.Sum(), physical.Sum());
            Assert.Equal(full.Total - 255 * System.Numerics.BitOperations.PopCount((uint)mask), layout.Total);
            if (mask != 0) Assert.Throws<InvalidOperationException>(() => layout.Population(physical, context));
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vp8lHistogramLayout(2, 1));
    }

    private static Vp8lToken[] Tokens()
    {
        var tokens = new List<Vp8lToken>(); var position = 0;
        while (position < 65 * 17)
        {
            var token = (tokens.Count % 3) switch
            {
                0 => Vp8lToken.Literal(0xae5e0027u | (uint)(tokens.Count * 17 & 255) << 8),
                1 => Vp8lToken.Cached(0),
                _ => Vp8lToken.Copy(Math.Min(9, 65 * 17 - position), 1)
            };
            tokens.Add(token); position += token.Length;
        }
        return tokens.ToArray();
    }

    [Fact]
    public void NonzeroConstantBanksPublishFullPopulationsAndIndependentRetainedOwners()
    {
        var tokens = Tokens(); var original = (Vp8lToken[])tokens.Clone(); var layout = new Vp8lHistogramLayout(2);
        using var root = new TestMemoryBudget(512_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var expected = new int[layout.Total]; layout.Fill(tokens, 65, true, expected, context);
        using var first = Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, context);
        Assert.Equal(0, first.Layout.ConstantBanks); Assert.Equal(layout.Total, first.Layout.Total);
        var actual = new int[layout.Total];
        for (var group = 0; group < first.GroupCount; group++)
            for (var i = 0; i < actual.Length; i++) actual[i] += first.Populations[group * layout.Total + i];
        Assert.Equal(expected, actual); Assert.Equal(original, tokens);
        Assert.All(first.Map.ToArray(), group => Assert.InRange(group, 0, first.GroupCount - 1));
        var before = first.Populations.ToArray(); var map = first.Map.ToArray();
        using (var second = Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, context))
        {
            Assert.Equal(before, second.Populations.ToArray()); Assert.Equal(map, second.Map.ToArray());
            Array.Clear(tokens); first.Dispose();
            Assert.Equal((second.Populations.Length + second.Map.Length) * 4L, root.Bytes);
            Assert.Equal(before, second.Populations.ToArray());
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CompactMapWorkspaceAndRetainedOwnerRefusalsAreTerminalAndReleaseCapacity(int phase)
    {
        var tokens = Tokens(); using var root = new TestMemoryBudget(512_000) { RefuseAtGrowth = phase };
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, context));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(phase, root.GrowthAttempts); Assert.Equal(1, root.Refusals); Assert.Equal(0, root.Bytes);
        }
        root.RefuseAtGrowth = 0;
        using (var later = new OperationContext(new() { MemoryBudget = root }, default))
        using (var plan = Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, later))
            Assert.Equal(0, plan.Layout.ConstantBanks);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CancellationAtEachCompactOwnershipBoundaryCleansUpBeforeSubsequentOperation(int phase)
    {
        var tokens = Tokens(); using var root = new TestMemoryBudget(512_000);
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => { if (root.GrowthAttempts == phase) cancellation.Cancel(); };
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var later = new OperationContext(new() { MemoryBudget = root }, default))
        using (var plan = Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, later))
            Assert.Equal(0, plan.Layout.ConstantBanks);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals); Assert.False(root.IsDisposed);
    }
}
