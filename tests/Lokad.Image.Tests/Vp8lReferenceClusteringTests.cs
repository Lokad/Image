using System.Buffers.Binary;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;
using static Lokad.Image.Tests.Vp8lReferencePredictorTests;

namespace Lokad.Image.Tests;

public sealed class Vp8lReferenceClusteringTests
{
    private static readonly Vp8lToken[] Sparse = [Vp8lToken.Literal(0xff010203u), Vp8lToken.Copy(63, 1)];

    private static string Hash(ReadOnlySpan<int> values)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); Span<byte> bytes = stackalloc byte[4];
        foreach (var value in values) { BinaryPrimitives.WriteInt32LittleEndian(bytes, value); hash.AppendData(bytes); }
        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static Vp8lTokenBuffer Tokens(int method, OperationContext context)
    {
        var lowEffort = method == 0;
        var pixels = Working(Asymmetric(), true, context);
        try
        {
            var bits = method == 4 ? 3 : 6;
            using var prediction = Vp8lReferencePredictor.SelectAndApply(pixels, 513, bits, bits, lowEffort, context);
            using var color = lowEffort ? null : Vp8lReferenceColorTransform.SelectAndApply(pixels, 513, bits, 75, context);
            using var matches = Vp8lReferenceMatcher.Build(pixels, 513, 75, lowEffort, context);
            return lowEffort ? Vp8lReferenceLz.Greedy(pixels, matches.Values, context)
                : Vp8lReferenceMainParsing.Encode(pixels, matches.Values, 513, false, 10, context);
        }
        finally { context.Release(pixels.Length * 4L); }
    }

    // Final coded-token populations and expanded native maps from the existing
    // asymmetric method4/method0/method1 packets; no reference data enters production.
    [Theory]
    [InlineData(4, 3, 2145, 9, 10, 110306, "a830a6669049c9cd598c370d1a99a77f609d39564b857294cbb6b38e45aa778c", "2f1af5ac8f2a35ca01ce4797780aba77f10f8853464cee96d7aa1d595aa1793b")]
    [InlineData(0, 7, 15, 2, 0, 129822, "a3e902d3485919e7f086f8f943bdd809b309ff86c3d71b220f76db088bf41955", "6eb392809e16ba3e66de8a9df4392c393824e9ee49e3cce9020a58db93bab913")]
    [InlineData(1, 6, 45, 16, 10, 102666, "5b9740d12603b1633b2aafad0e9ee9da5be75c16e6cd8b2ad0b0b4215e1e5eec", "a21ac006f47eab416493a42dd35f2dd9cd584c302249402a99e80b7603686014")]
    public void OrdinaryLowEffortAndSamplingTransitionMapsAndEveryFinalPopulationMatchNative(int method, int bits, int tiles, int groups, int cacheBits, int tokenCount, string map, string populations)
    {
        using var root = new TestMemoryBudget(128_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var tokens = Tokens(method, context); var original = tokens.Tokens.ToArray(); var baseline = root.Bytes;
        Assert.Equal((cacheBits, tokenCount), (tokens.CacheBits, tokens.Count));
        using var plan = Vp8lReferenceClustering.Build(tokens.Tokens, 513, 259, bits, cacheBits, method == 0, context);
        Assert.Equal((groups, tiles, bits), (plan.GroupCount, plan.Map.Length, plan.TileBits));
        Assert.Equal(map, Hash(plan.Map)); Assert.Equal(populations, Hash(plan.Populations));
        Assert.Equal(baseline + (plan.Populations.Length + plan.Map.Length) * 4L, root.Bytes);
        Assert.Equal(original, tokens.Tokens.ToArray());
        tokens.Dispose(); Assert.Equal((plan.Populations.Length + plan.Map.Length) * 4L, root.Bytes);
        Assert.Equal(populations, Hash(plan.Populations)); plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SparseTokenStartTilesInheritTheirPredecessorAndRetainedPlansHaveIndependentLifetimes()
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var first = Vp8lReferenceClustering.Build(Sparse, 8, 8, 2, 0, false, context);
        Assert.Equal(1, first.GroupCount); Assert.Equal(new int[4], first.Map.ToArray());
        var hash = Hash(first.Populations); var capacity = (first.Map.Length + first.Populations.Length) * 4L;
        using var second = Vp8lReferenceClustering.Build(Sparse, 8, 8, 2, 0, false, context);
        Assert.Equal(capacity * 2, root.Bytes); Assert.True(root.Peak > root.Bytes);
        first.Dispose(); first.Dispose(); Assert.Equal(capacity, root.Bytes);
        Assert.Throws<ObjectDisposedException>(() => first.Map.Length); Assert.Equal(hash, Hash(second.Populations));
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1, 75)]
    [InlineData(2, 75)]
    [InlineData(1, 8)]
    [InlineData(2, 8)]
    public void ScratchAndFinalOwnerAdmissionRefusalIsTerminalAndAllowsRecovery(int phase, int quality)
    {
        using var root = new TestMemoryBudget(100_000) { RefuseAtGrowth = phase };
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceClustering.Build(Sparse, 8, 8, 2, 0, false, context, quality));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(phase, root.GrowthAttempts); Assert.Equal(0, root.Bytes);
        root.RefuseAtGrowth = 0;
        using var next = Vp8lReferenceClustering.Build(Sparse, 8, 8, 2, 0, false, context, quality);
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(1, 75)]
    [InlineData(2, 75)]
    [InlineData(1, 8)]
    [InlineData(2, 8)]
    public void ScratchOrRetainedOwnerCancellationRefundsAllStorageAndAllowsRecovery(int phase, int quality)
    {
        using var root = new TestMemoryBudget(100_000); using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => { if (root.GrowthAttempts == phase) cancellation.Cancel(); };
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        Assert.Throws<OperationCanceledException>(() => Vp8lReferenceClustering.Build(Sparse, 8, 8, 2, 0, false, context, quality));
        Assert.True(cancellation.IsCancellationRequested); Assert.Equal(0, root.Bytes); root.Admitted = null;
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var next = Vp8lReferenceClustering.Build(Sparse, 8, 8, 2, 0, false, nextContext, quality);
        next.Dispose(); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
    }

    [Fact]
    public void InvalidTilingAndIncompleteTokensFailWithoutRetainingStorage()
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.Throws<ArgumentOutOfRangeException>(() => Vp8lReferenceClustering.Build(Sparse, 8, 8, 1, 0, false, context));
        Assert.Throws<ArgumentException>(() => Vp8lReferenceClustering.Build(Sparse, 513, 259, 2, 0, false, context));
        Assert.Throws<ArgumentException>(() => Vp8lReferenceClustering.Build([], 8, 8, 2, 0, false, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
    }
}
