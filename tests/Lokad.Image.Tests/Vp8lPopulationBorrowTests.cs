using System.Runtime.InteropServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lPopulationBorrowTests
{
    [Fact]
    public void BorrowedRowsMatchOwnedRowsWithoutTouchingTokensOrGuards()
    {
        foreach (var cacheBits in new[] { 0, 4 })
        {
            var owned = Run(cacheBits, 0);
            var borrowed = Run(cacheBits, 1);
            var insufficient = Run(cacheBits, 2);
            Assert.Equal(owned.Leaves.Length, borrowed.Leaves.Length);
            for (var i = 0; i < owned.Leaves.Length; i++)
            {
                Assert.Equal(owned.Leaves[i], borrowed.Leaves[i]);
                Assert.Equal(owned.Leaves[i], insufficient.Leaves[i]);
            }
            Assert.Equal(5 * 17 * 8 + 64, owned.Peak - borrowed.Peak);
            Assert.Equal(owned.Peak, insufficient.Peak);
            Assert.Equal(owned.Retained, borrowed.Retained);
        }

        static (int[][] Leaves, long Peak, long Retained) Run(int cacheBits, int mode)
        {
            using var root = new TestMemoryBudget(1 << 24);
            using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
            using var tokens = Tokens(context, cacheBits);
            using var matches = new Vp8lMatchIndex(context, 512);
            var prefix = tokens.Tokens.ToArray(); var before = root.Bytes;
            var tail = matches.Storage.AsSpan(); tail.Fill(unchecked((int)0xa5a5a5a5));
            const int words = 5 * 17 * 2;
            var scratch = mode == 0 ? default : tail.Slice(2, mode == 1 ? words : words - 1);
            using var population = Vp8lPopulationWorkspace.Create(tokens.Tokens, 17, 17, 2,
                new Vp8lHistogramLayout(cacheBits), 12, context, scratch);
            var leaves = Enumerable.Range(0, 25).Select(i => population.Read(i, 0, true).ToArray()).ToArray();
            Assert.Equal(prefix, tokens.Tokens.ToArray()); Assert.Equal(289, tokens.Count); Assert.Equal(cacheBits, tokens.CacheBits);
            Assert.Equal(unchecked((int)0xa5a5a5a5), tail[1]);
            Assert.Equal(unchecked((int)0xa5a5a5a5), tail[words + 2]);
            var peak = root.Peak; var retained = root.Bytes;
            population.Dispose(); Assert.Equal(before, root.Bytes);
            matches.Dispose(); tokens.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            return (leaves, peak, retained);
        }
    }

    [Fact]
    public void RefusalAndAliasingCleanPreparationWithoutRetryingOrCorruptingTokens()
    {
        using var root = new TestMemoryBudget(1 << 24);
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        using var tokens = Tokens(context, 4);
        using var matches = new Vp8lMatchIndex(context, 512);
        var before = root.Bytes; var prefix = tokens.Tokens.ToArray();
        Assert.Throws<ArgumentException>(() => Vp8lPopulationWorkspace.Create(tokens.Tokens, 17, 17, 2,
            new Vp8lHistogramLayout(4), 12, context, MemoryMarshal.Cast<Vp8lToken, int>(tokens.Writable)));
        Assert.Equal(before, root.Bytes); Assert.Equal(prefix, tokens.Tokens.ToArray());
        root.RefuseAtGrowth = root.GrowthAttempts + 2; // Sparse entries, after workspace construction.
        Assert.Throws<ImageLimitException>(() => Create(tokens, matches, context));
        Assert.Equal(1, root.Refusals); Assert.Equal(before, root.Bytes); Assert.Equal(prefix, tokens.Tokens.ToArray());
        root.RefuseAtGrowth = 0;
        using var subsequent = Create(tokens, matches, context);
        subsequent.Dispose(); Assert.Equal(before, root.Bytes);
        matches.Dispose(); tokens.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => { _ = matches.Storage; });
    }

    [Fact]
    public void CancellationDuringBorrowedPreparationKeepsSurvivingOwnerCharged()
    {
        using var root = new TestMemoryBudget(1 << 24);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new ImageLimits { MemoryBudget = root }, cancellation.Token);
        using var tokens = Tokens(context, 4);
        using var matches = new Vp8lMatchIndex(context, 512);
        var before = root.Bytes; var prefix = tokens.Tokens.ToArray(); var cancelAt = root.GrowthAttempts + 2;
        root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
        Assert.Throws<OperationCanceledException>(() => Create(tokens, matches, context));
        root.Admitted = null;
        Assert.Equal(before, root.Bytes); Assert.Equal(prefix, tokens.Tokens.ToArray());
        using var next = new OperationContext(new ImageLimits { MemoryBudget = root }, default);
        using var nextTokens = Tokens(next, 4);
        using var nextMatches = new Vp8lMatchIndex(next, 512);
        using var subsequent = Create(nextTokens, nextMatches, next);
        subsequent.Dispose(); nextMatches.Dispose(); nextTokens.Dispose(); Assert.Equal(before, root.Bytes);
        matches.Dispose(); tokens.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static Vp8lPopulationWorkspace Create(Vp8lTokenBuffer tokens, Vp8lMatchIndex matches, OperationContext context)
        => Vp8lPopulationWorkspace.Create(tokens.Tokens, 17, 17, 2, new Vp8lHistogramLayout(tokens.CacheBits),
            12, context, matches.Storage);

    private static Vp8lTokenBuffer Tokens(OperationContext context, int cacheBits)
    {
        var result = new Vp8lTokenBuffer(context, 512) { CacheBits = cacheBits };
        for (var i = 0; i < 289; i++) result.Add(Vp8lToken.Literal(0xff000000u | (uint)(i * 31337) & 0xffffffu));
        return result;
    }
}
