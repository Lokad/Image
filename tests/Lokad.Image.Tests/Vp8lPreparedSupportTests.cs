using System.Numerics;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lPreparedSupportTests
{
    [Fact]
    public void SupportPreservesAllWordEdgesAndZeroGapAndEqualRunPartitions()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var a = Enumerable.Repeat(-13, 266).ToArray(); var b = Enumerable.Repeat(-29, 268).ToArray();
        var aWords = new ulong[4]; var bWords = new ulong[4];
        foreach (var start in Enumerable.Range(0, 256))
        foreach (var length in new[] { 1, 2, 3, 4, 5, 63, 64, 65 })
        {
            a.AsSpan(3, 256).Clear(); b.AsSpan(5, 256).Clear();
            for (var i = start; i < Math.Min(256, start + length); i++)
            {
                // Input runs differ; their combined count stays unchanged.
                a[i + 3] = (i & 1) == 0 ? 1 : 3; b[i + 5] = 7 - a[i + 3];
            }
            if (start > 4) a[3 + start - 4] = 13;
            if (start + length + 4 < 256) b[5 + start + length + 4] = 17;
            var originalA = (int[])a.Clone(); var originalB = (int[])b.Clone();
            CheckSupport(a.AsSpan(3, 256), aWords, context); CheckSupport(b.AsSpan(5, 256), bWords, context);
            Assert.Equal(Vp8lReferenceEntropy.PopulationSum(a.AsSpan(3, 256), b.AsSpan(5, 256), context),
                Vp8lReferenceEntropy.PopulationSumColor(a.AsSpan(3, 256), b.AsSpan(5, 256), aWords, bWords, context));
            Assert.Equal(originalA, a); Assert.Equal(originalB, b);
        }
    }

    [Fact]
    public void SparseDenseDispatchAndHighCountsKeepExactFullLogicalCosts()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var a = new int[256]; var b = new int[256]; var aWords = new ulong[4]; var bWords = new ulong[4];
        foreach (var support in new[] { 0, 1, 2, 3, 4, 7, 15, 16, 31, 63, 64, 65, 96, 127, 128, 129, 255, 256 })
        foreach (var shift in new[] { 0, 1, 63, 64, 127, 128, 191, 192, 255 })
        {
            Array.Clear(a); Array.Clear(b);
            for (var i = 0; i < support; i++)
            {
                var symbol = (i * 73 + shift) & 255; var count = (i % 4) switch { 0 => 1, 1 => 257, 2 => 65536, _ => 100003 };
                a[symbol] = count / 3; b[symbol] = count - a[symbol];
            }
            Check();
        }
        foreach (var total in new[] { (1 << 28) - 1, 1 << 28, (1 << 28) + 1, int.MaxValue })
        {
            Array.Clear(a); Array.Clear(b);
            a[0] = total / 3; b[64] = total / 3; a[255] = total - 2 * (total / 3); Check();
        }
        void Check()
        {
            CheckSupport(a, aWords, context); CheckSupport(b, bWords, context);
            var originalA = (ulong[])aWords.Clone(); var originalB = (ulong[])bWords.Clone();
            Assert.Equal(Vp8lReferenceEntropy.PopulationSum(a, b, context), Vp8lReferenceEntropy.PopulationSumColor(a, b, aWords, bWords, context));
            Assert.Equal(originalA, aWords); Assert.Equal(originalB, bWords);
        }
    }

    [Fact]
    public void PreparedSupportRejectsInvalidBoundariesHonorsCancellationAndAllocatesNothingPerPair()
    {
        using var cancellation = new CancellationTokenSource(); using var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var a = new int[256]; var b = new int[256]; var aWords = new ulong[4]; var bWords = new ulong[4];
        a[0] = 3; a[64] = 7; b[127] = 9; b[255] = 11;
        Vp8lReferenceEntropy.PrepareColorSupport(a, aWords, context); Vp8lReferenceEntropy.PrepareColorSupport(b, bWords, context);
        long cost = 0;
        for (var i = 0; i < 256; i++) cost ^= Vp8lReferenceEntropy.PopulationSumColor(a, b, aWords, bWords, context);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 256; i++) cost ^= Vp8lReferenceEntropy.PopulationSumColor(a, b, aWords, bWords, context);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before); Assert.Equal(0, cost);
        Assert.Throws<ArgumentException>(() => Vp8lReferenceEntropy.PrepareColorSupport([], aWords, context));
        Assert.Throws<ArgumentException>(() => Vp8lReferenceEntropy.PopulationSumColor(a, b, [], bWords, context));
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceEntropy.PrepareColorSupport(a, aWords, context));
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceEntropy.PopulationSumColor(a, b, aWords, bWords, context));
    }

    private static void CheckSupport(ReadOnlySpan<int> counts, Span<ulong> support, OperationContext context)
    {
        var result = Vp8lReferenceEntropy.PrepareColorSupport(counts, support, context); var nonzero = 0; var last = -1;
        for (var symbol = 0; symbol < 256; symbol++)
        {
            var present = counts[symbol] != 0; Assert.Equal(present, (support[symbol / 64] & 1UL << (symbol & 63)) != 0);
            if (present) { nonzero++; last = symbol; }
        }
        Assert.Equal((nonzero, last), result);
        Assert.Equal(nonzero, support.ToArray().Sum(word => BitOperations.PopCount(word)));
    }

    private static Vp8lToken[] Tokens()
    {
        var tokens = new List<Vp8lToken>(); var position = 0; uint state = 0x31415926;
        while (position < 65 * 17)
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            var token = (tokens.Count % 3) switch
            {
                0 => Vp8lToken.Literal(state), 1 => Vp8lToken.Cached(0),
                _ => Vp8lToken.Copy(Math.Min(9, 65 * 17 - position), 1)
            };
            tokens.Add(token); position += token.Length;
        }
        return tokens.ToArray();
    }

    [Fact]
    public void PreparedWorkingSupportDoesNotEscapeIntoIndependentRetainedPlans()
    {
        var tokens = Tokens(); var layout = new Vp8lHistogramLayout(2);
        using var root = new TestMemoryBudget(1_000_000); using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var expected = new int[layout.Total]; layout.Fill(tokens, 65, true, expected, context);
        using var first = Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, context);
        var counts = first.Populations.ToArray(); var map = first.Map.ToArray(); var actual = new int[layout.Total];
        for (var group = 0; group < first.GroupCount; group++)
            for (var i = 0; i < actual.Length; i++) actual[i] += counts[group * layout.Total + i];
        Assert.Equal(expected, actual);
        using (var second = Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, context))
        {
            Assert.Equal(counts, second.Populations.ToArray()); Assert.Equal(map, second.Map.ToArray());
            Array.Clear(tokens); first.Dispose();
            Assert.Equal((second.Populations.Length + second.Map.Length) * 4L, root.Bytes);
            Assert.Equal(counts, second.Populations.ToArray());
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void PreparedWorkspaceRefusalRemainsTerminalAndAllowsSubsequentOperation(int phase)
    {
        var tokens = Tokens(); using var root = new TestMemoryBudget(1_000_000) { RefuseAtGrowth = phase };
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, context));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(phase, root.GrowthAttempts);
            Assert.Equal(1, root.Refusals); Assert.Equal(0, root.Bytes);
        }
        root.RefuseAtGrowth = 0;
        using (var later = new OperationContext(new() { MemoryBudget = root }, default))
        using (var plan = Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, later)) Assert.True(plan.GroupCount > 0);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void PreparedWorkspaceCancellationReleasesAllActualUsersBeforeRecovery(int phase)
    {
        var tokens = Tokens(); using var root = new TestMemoryBudget(1_000_000); using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => { if (root.GrowthAttempts == phase) cancellation.Cancel(); };
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var later = new OperationContext(new() { MemoryBudget = root }, default))
        using (var plan = Vp8lReferenceClustering.Build(tokens, 65, 17, 2, 2, false, later)) Assert.True(plan.GroupCount > 0);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals); Assert.False(root.IsDisposed);
    }
}
