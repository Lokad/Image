using System.Reflection;
using System.Runtime.InteropServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lClusteringStorageTests
{
    private const int Guard = unchecked((int)0xa5a5a5a5);
    private static readonly Func<int, int, int> PairCapacity = typeof(Vp8lReferenceClustering)
        .GetMethod("PairCapacity", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Func<int, int, int>>();
    private static readonly Func<int, int, int> StateWords = typeof(Vp8lReferenceClustering)
        .GetMethod("StateWords", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Func<int, int, int>>();

    [Fact]
    public void BorrowedStateMatchesOwnedStateAndKeepsIndependentSurvivors()
    {
        foreach (var shape in new[] { 0, 1, 2 })
        foreach (var cacheBits in new[] { 0, 4 })
        {
            var owned = Run(shape, cacheBits, 0);
            var borrowed = Run(shape, cacheBits, 1);
            var tooShort = Run(shape, cacheBits, 2);
            Assert.Equal(owned.Map, borrowed.Map); Assert.Equal(owned.Map, tooShort.Map);
            Assert.Equal(owned.Populations, borrowed.Populations); Assert.Equal(owned.Populations, tooShort.Populations);
            Assert.True(borrowed.Peak < owned.Peak);
            Assert.Equal(owned.Peak, tooShort.Peak);
            Assert.Equal(owned.Retained, borrowed.Retained); Assert.Equal(owned.Retained, tooShort.Retained);
        }

        static (int[] Map, int[] Populations, long Peak, long Retained) Run(int shape, int cacheBits, int mode)
        {
            var (tokens, width, height, bits) = Source(shape); var prefix = tokens.ToArray();
            var words = Words(width, height, bits); var columns = (width + (1 << bits) - 1) >> bits;
            using var root = new TestMemoryBudget(1 << 27);
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            using var matches = new Vp8lMatchIndex(context, words + 4);
            matches.Storage.AsSpan().Fill(Guard);
            var length = mode == 0 ? columns * height * 2 : mode == 1 ? words : words - 1;
            using var plan = Vp8lReferenceClustering.Build(tokens, width, height, bits, cacheBits, false, context,
                scratch: matches.Storage.AsSpan(2, length));
            var map = plan.Map.ToArray(); var populations = plan.Populations.ToArray(); var peak = root.Peak;
            Assert.Equal(prefix, tokens); Assert.Equal(Guard, matches.Storage[0]); Assert.Equal(Guard, matches.Storage[1]);
            Assert.Equal(Guard, matches.Storage[words + 2]); Assert.Equal(Guard, matches.Storage[words + 3]);
            matches.Storage.AsSpan().Fill(Guard); matches.Dispose();
            Assert.True(plan.Map.SequenceEqual(map)); Assert.True(plan.Populations.SequenceEqual(populations));
            var retained = root.Bytes; Assert.True(retained > 0);
            plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            return (map, populations, peak, retained);
        }
    }

    [Fact]
    public void AliasingBeyondPopulationRowPrefixIsRejectedBeforeMutation()
    {
        const int width = 64, height = 64, bits = 4;
        var words = Words(width, height, bits); var storage = Enumerable.Repeat(Guard, words + 4).ToArray();
        var tokenOffset = 2 + 4 * height * 2 + 8;
        var tokens = MemoryMarshal.Cast<int, Vp8lToken>(storage.AsSpan(tokenOffset, 4));
        tokens[0] = Vp8lToken.Literal(0xff010203); tokens[1] = Vp8lToken.Copy(4095, 1);
        var original = storage.ToArray();
        using var root = new TestMemoryBudget(1 << 24);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.Throws<ArgumentException>(() => Vp8lReferenceClustering.Build(
            MemoryMarshal.Cast<int, Vp8lToken>(storage.AsSpan(tokenOffset, 4)), width, height, bits, 0, false,
            context, scratch: storage.AsSpan(2, words)));
        Assert.Equal(original, storage); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RefusedBorrowedStateCleansUpWithoutRetryAndAllowsRecovery()
    {
        foreach (var offset in new[] { 1, 2, 3, 4 })
        {
            var (tokens, width, height, bits) = Source(2); var prefix = tokens.ToArray();
            using var root = new TestMemoryBudget(1 << 27);
            using var owner = new OperationContext(new() { MemoryBudget = root }, default);
            using var matches = new Vp8lMatchIndex(owner, Words(width, height, bits));
            var before = root.Bytes; root.RefuseAtGrowth = root.GrowthAttempts + offset;
            using (var operation = new OperationContext(new() { MemoryBudget = root }, default))
            {
                var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceClustering.Build(tokens,
                    width, height, bits, 4, false, operation, scratch: matches.Storage));
                Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
                Assert.Equal(1, root.Refusals); Assert.Equal(before, root.Bytes); Assert.Equal(prefix, tokens);
            }
            root.RefuseAtGrowth = 0;
            using (var later = new OperationContext(new() { MemoryBudget = root }, default))
            using (var plan = Vp8lReferenceClustering.Build(tokens, width, height, bits, 4, false, later, scratch: matches.Storage))
                Assert.Equal(1, plan.GroupCount);
            Assert.Equal(before, root.Bytes); matches.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }

    [Fact]
    public void CanceledBorrowedStateKeepsOwnerChargedAndAllowsRecovery()
    {
        foreach (var offset in new[] { 1, 2, 3, 4 })
        {
            var (tokens, width, height, bits) = Source(2); var prefix = tokens.ToArray();
            using var root = new TestMemoryBudget(1 << 27);
            using var owner = new OperationContext(new() { MemoryBudget = root }, default);
            using var matches = new Vp8lMatchIndex(owner, Words(width, height, bits));
            var before = root.Bytes; var cancelAt = root.GrowthAttempts + offset;
            using var cancellation = new CancellationTokenSource();
            root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
            using (var operation = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
                Assert.Throws<OperationCanceledException>(() => Vp8lReferenceClustering.Build(tokens,
                    width, height, bits, 4, false, operation, scratch: matches.Storage));
            root.Admitted = null; Assert.Equal(before, root.Bytes); Assert.Equal(prefix, tokens);
            using (var later = new OperationContext(new() { MemoryBudget = root }, default))
            using (var plan = Vp8lReferenceClustering.Build(tokens, width, height, bits, 4, false, later, scratch: matches.Storage))
                Assert.Equal(1, plan.GroupCount);
            Assert.Equal(before, root.Bytes); matches.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }

    private static int Words(int width, int height, int bits)
    {
        var tiles = ((width + (1 << bits) - 1) >> bits) * ((height + (1 << bits) - 1) >> bits);
        return StateWords(tiles, PairCapacity(tiles, 43)); // Existing default quality75 threshold.
    }

    private static (Vp8lToken[] Tokens, int Width, int Height, int Bits) Source(int shape)
    {
        if (shape == 2) return ([Vp8lToken.Literal(0xff010203), Vp8lToken.Copy(4095, 1)], 64, 64, 2);
        var width = shape == 0 ? 17 : 129; var height = shape == 0 ? 17 : 65;
        var tokens = new Vp8lToken[width * height]; var random = new Random(673 + shape);
        for (var i = 0; i < tokens.Length; i++) tokens[i] = Vp8lToken.Literal(
            shape == 0 ? 0xff010203u + (uint)(i % 5) : 0xff000000u | (uint)random.Next(1 << 24));
        return (tokens, width, height, shape == 0 ? 2 : 3);
    }
}
