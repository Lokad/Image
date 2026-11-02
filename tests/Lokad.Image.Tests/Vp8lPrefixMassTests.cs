using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lPrefixMassTests
{
    private const int Length = 16384, Bits = 10;
    // 16,384 history bytes +2,046cache words +14,014histogram words +128metadata,
    // then10*1,024mass counters and32Barray overhead. One scope spans both scores.
    private const long Scratch = 121_744;

    private static (uint[] Pixels, Vp8lToken[] First, Vp8lToken[] Second) Input()
    {
        var pixels = new uint[Length]; var first = new Vp8lToken[Length]; var second = new Vp8lToken[Length / 4];
        for (var i = 0; i < pixels.Length; i++)
        {
            var block = i / 8; pixels[i] = block % 17 == 0 ? 0 : (uint)((block * 17 & 255) * 0x01010101u);
            first[i] = Vp8lToken.Literal(pixels[i]);
        }
        for (var block = 0; block < Length / 8; block++)
        { second[2 * block] = Vp8lToken.Literal(pixels[block * 8]); second[2 * block + 1] = Vp8lToken.Copy(7, 1); }
        return (pixels, first, second);
    }

    [Fact]
    public void LargeHistoryOwnsOneMassBufferAcrossBothScoresAndKeepsChoicesAndSourcesExact()
    {
        var (pixels, first, second) = Input(); var beforePixels = (uint[])pixels.Clone(); var beforeFirst = (Vp8lToken[])first.Clone(); var beforeSecond = (Vp8lToken[])second.Clone();
        using var reference = new OperationContext(ImageLimits.Default, default);
        var expected = (Vp8lReferenceCache.BestBits(pixels, first, Bits, reference), Vp8lReferenceCache.BestBits(pixels, second, Bits, reference));
        using var root = new TestMemoryBudget(Scratch); var admitted = false;
        root.Admitted = _ => { Assert.Equal(Scratch, root.Bytes); admitted = true; };
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.True(Vp8lReferenceCache.TryBestBitsWithHistory(pixels, first, second, Bits, context, out var a, out var b));
        Assert.Equal(expected, (a, b)); Assert.True(admitted); Assert.Equal(Scratch, root.Peak);
        Assert.Equal(beforePixels, pixels); Assert.Equal(beforeFirst, first); Assert.Equal(beforeSecond, second);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void InitiallyZeroCacheAndCachedSeedsKeepEveryPopulationAndGuardedCapacity()
    {
        const int size = 32;
        var pixels = new uint[size]; var tokens = Enumerable.Repeat(Vp8lToken.Cached(0), size).ToArray(); var minimum = new byte[size];
        for (var bits = 1; bits <= Bits; bits++)
        {
            Vp8lReferenceCache.BuildHistory(pixels, bits, new uint[(1 << (bits + 1)) - 2], minimum);
            var values = Vp8lReferenceCache.HistogramValues(bits);
            var countStore = Enumerable.Repeat(-17, values + 9).ToArray(); var massStore = Enumerable.Repeat(-29, bits * (1 << bits) + 7).ToArray();
            Vp8lReferenceCache.FillAllFromHistory(pixels, tokens, minimum, bits, countStore.AsSpan(3, values), massStore.AsSpan(2, massStore.Length - 7));
            for (var width = 0; width <= bits; width++)
            {
                var layout = new Vp8lHistogramLayout(width); var expected = new int[layout.Total];
                if (width == 0) expected[0] = expected[layout.Red] = expected[layout.Blue] = expected[layout.Alpha] = size;
                else expected[280] = size;
                Assert.Equal(expected, countStore.AsSpan(3 + Vp8lReferenceCache.HistogramOffset(width), layout.Total).ToArray());
            }
            Assert.All(countStore.Take(3).Concat(countStore.Skip(3 + values)), count => Assert.Equal(-17, count));
            Assert.All(massStore.Take(2).Concat(massStore.TakeLast(5)), count => Assert.Equal(-29, count));
            Assert.All(pixels, pixel => Assert.Equal(0u, pixel)); Assert.All(minimum, value => Assert.Equal((byte)1, value));
        }
        Assert.Throws<ArgumentException>(() => Vp8lReferenceCache.FillAllFromHistory(pixels, tokens, minimum, Bits, new int[Vp8lReferenceCache.HistogramValues(Bits)], new int[3]));
    }

    [Fact]
    public void SharedRefusalIsTerminalBeforeMassAllocationAndReleasedCapacityAllowsRecovery()
    {
        var (pixels, first, second) = Input(); using var root = new TestMemoryBudget(Scratch);
        Assert.True(root.TryReserve(1, out var held));
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceCache.TryBestBitsWithHistory(pixels, first, second, Bits, context, out _, out _));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(1, root.Bytes); Assert.Equal(1, root.Refusals); Assert.Equal(2, root.GrowthAttempts);
        }
        held.Dispose();
        using (var next = new OperationContext(new() { MemoryBudget = root }, default))
            Assert.True(Vp8lReferenceCache.TryBestBitsWithHistory(pixels, first, second, Bits, next, out _, out _));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SecondSeedFailureCleansSharedMassAndPreservesCallerOwnerBeforeNextOperation()
    {
        var (pixels, first, second) = Input(); var invalid = first[..^1];
        using var root = new TestMemoryBudget(Scratch + 1); Assert.True(root.TryReserve(1, out var held));
        using (var context = new OperationContext(new() { MemoryBudget = root }, default))
            Assert.Throws<ArgumentException>(() => Vp8lReferenceCache.TryBestBitsWithHistory(pixels, first, invalid, Bits, context, out _, out _));
        Assert.Equal(1, root.Bytes); Assert.Equal(Scratch + 1, root.Peak);
        using (var next = new OperationContext(new() { MemoryBudget = root }, default))
            Assert.True(Vp8lReferenceCache.TryBestBitsWithHistory(pixels, first, second, Bits, next, out _, out _));
        Assert.Equal(1, root.Bytes); held.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void LargeCancellableAndTightLocalPathsKeepTheirSelectionBeforeSharedAdmission()
    {
        var (pixels, first, second) = Input(); using var root = new TestMemoryBudget(Scratch);
        using var cancellation = new CancellationTokenSource();
        using (var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
        {
            Assert.False(Vp8lReferenceCache.TryBestBitsWithHistory(pixels, first, second, Bits, context, out _, out _));
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => Vp8lReferenceCache.BestBits(pixels, first, Bits, context));
        }
        using (var context = new OperationContext(new() { MemoryBudget = root, MaxWorkingBytes = Scratch - 1 }, default))
            Assert.False(Vp8lReferenceCache.TryBestBitsWithHistory(pixels, first, second, Bits, context, out _, out _));
        Assert.Equal(0, root.GrowthAttempts); Assert.Equal(0, root.Refusals); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
