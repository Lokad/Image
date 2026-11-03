using System.Runtime.InteropServices;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class Vp8lHistoryTailTests
{
    [Theory]
    [InlineData(257)] [InlineData(258)] [InlineData(259)] [InlineData(260)] [InlineData(8192)]
    public void BorrowedHistoryMatchesIndependentChoicesAndPreservesLivePrefixesAndTailGuards(int length)
    {
        foreach (var pixel in new[] { 0u, 0xff718293u })
        for (var bits = 1; bits <= 10; bits++)
        {
            var pixels = Enumerable.Repeat(pixel, length).ToArray();
            var repeated = new List<Vp8lToken> { Vp8lToken.Literal(pixel) };
            for (var at = 1; at < length;)
            { var count = Math.Min(4096, length - at); repeated.Add(Vp8lToken.Copy(count, 1)); at += count; }
            var second = repeated.ToArray();
            var massWords = bits >= 4 && length >= 8 * (1 << bits) ? bits * (1 << bits) : 0;
            var cacheWords = (1 << (bits + 1)) - 2;
            var payload = ((length + 3) & ~3) + (cacheWords + Vp8lReferenceCache.HistogramValues(bits) + massWords) * 4;
            using var root = new TestMemoryBudget(2_000_000);
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            using var candidate = new Vp8lTokenBuffer(context, length + (payload + 7) / 8 + 2);
            for (var p = 0; p < length; p++) candidate.Add(Vp8lToken.Literal(pixel));
            var prefix = candidate.Tokens.ToArray();
            var tailBytes = checked((int)candidate.CapacityBytes - candidate.Count * 8);
            candidate.BorrowTailBytes(context, tailBytes).Fill(0xa7);
            var before = root.Bytes; var attempts = root.Attempts;
            root.RefuseAtGrowth = root.GrowthAttempts + 1;
            using var oracle = new OperationContext(ImageLimits.Default, default);
            var expected = (Vp8lReferenceCache.BestBits(pixels, prefix, bits, oracle),
                Vp8lReferenceCache.BestBits(pixels, second, bits, oracle));
            Assert.True(Vp8lReferenceCache.TryBestBitsWithHistory(pixels, candidate.Tokens, second, bits,
                context, out var firstBits, out var secondBits, candidate));
            Assert.Equal(expected, (firstBits, secondBits)); Assert.Equal(prefix, candidate.Tokens.ToArray());
            Assert.All(candidate.BorrowTailBytes(context, tailBytes)[payload..].ToArray(), b => Assert.Equal((byte)0xa7, b));
            Assert.Equal(before, root.Bytes); Assert.Equal(attempts, root.Attempts); Assert.Equal(0, root.Refusals);
            // A later token write owns every byte it publishes, after history's users finish.
            candidate.Add(Vp8lToken.Literal(0x12345678));
            Assert.Equal(prefix, candidate.Tokens[..length].ToArray());
            Assert.Equal(Vp8lToken.Literal(0x12345678), candidate.Tokens[length]);
            candidate.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }

    [Fact]
    public void DirtyBorrowedHistoryCanBeRepeatedWithoutHeapAllocationOrNewAdmission()
    {
        uint[] pixels = new uint[513]; Vp8lToken[] seed = [Vp8lToken.Literal(0), Vp8lToken.Copy(512, 1)];
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var candidate = new Vp8lTokenBuffer(context, 20_000);
        foreach (var token in seed) candidate.Add(token);
        candidate.BorrowTailBytes(context, 150_000).Fill(0xff);
        Assert.True(Vp8lReferenceCache.TryBestBitsWithHistory(pixels, candidate.Tokens, seed, 10, context, out _, out _, candidate));
        var attempts = root.Attempts; var retained = root.Bytes;
        var before = GC.GetAllocatedBytesForCurrentThread(); var success = true;
        for (var i = 0; i < 8; i++)
            success &= Vp8lReferenceCache.TryBestBitsWithHistory(pixels, candidate.Tokens, seed, 10, context, out _, out _, candidate);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.True(success); Assert.Equal(0, allocated); Assert.Equal(attempts, root.Attempts); Assert.Equal(retained, root.Bytes);
    }

    [Fact]
    public void ContextCacheDisposalAndAliasingGuardsPrecedeScratchWrites()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        using var other = new OperationContext(ImageLimits.Default, default);
        using var candidate = new Vp8lTokenBuffer(context, 8192); candidate.Add(Vp8lToken.Literal(0));
        candidate.BorrowTailBytes(context, 65_000).Fill(0xa7);
        Assert.Throws<ArgumentOutOfRangeException>(() => candidate.BorrowTailBytes(context, -1));
        Assert.Throws<ArgumentException>(() => candidate.BorrowTailBytes(other, 4));
        candidate.CacheBits = 1;
        Assert.Throws<ArgumentException>(() => candidate.BorrowTailBytes(context, 4)); candidate.CacheBits = 0;
        Assert.Throws<ArgumentException>(() =>
        {
            var aliasedPixels = MemoryMarshal.Cast<byte, uint>(candidate.BorrowTailBytes(context, 4));
            Vp8lReferenceCache.TryBestBitsWithHistory(aliasedPixels, candidate.Tokens, candidate.Tokens, 1, context, out _, out _, candidate);
        });
        Assert.Throws<ArgumentException>(() =>
        {
            var aliasedSeed = MemoryMarshal.Cast<byte, Vp8lToken>(candidate.BorrowTailBytes(context, 8));
            Vp8lReferenceCache.TryBestBitsWithHistory(new uint[1], aliasedSeed, candidate.Tokens, 1, context, out _, out _, candidate);
        });
        Assert.Throws<ArgumentException>(() =>
        {
            var aliasedSeed = MemoryMarshal.Cast<byte, Vp8lToken>(candidate.BorrowTailBytes(context, 8));
            Vp8lReferenceCache.TryBestBitsWithHistory(new uint[1], candidate.Tokens, aliasedSeed, 1, context, out _, out _, candidate);
        });
        Assert.All(candidate.BorrowTailBytes(context, 65_000).ToArray(), b => Assert.Equal((byte)0xa7, b));
        Assert.Equal(Vp8lToken.Literal(0), candidate.Tokens[0]); candidate.Dispose();
        Assert.Throws<ObjectDisposedException>(() => candidate.BorrowTailBytes(context, 4));
    }

    [Fact]
    public void InsufficientTailRefusalIsTerminalAndCleanupAllowsSubsequentOperation()
    {
        using var root = new TestMemoryBudget(1_000_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        using var candidate = new Vp8lTokenBuffer(context, 1); candidate.Add(Vp8lToken.Literal(0));
        root.RefuseAtGrowth = root.GrowthAttempts + 1;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceCache.TryBestBitsWithHistory(new uint[1],
            candidate.Tokens, candidate.Tokens, 10, context, out _, out _, candidate));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
        Assert.Equal(8, root.Bytes); Assert.Equal(1, candidate.Count); candidate.Dispose(); Assert.Equal(0, root.Bytes);
        root.RefuseAtGrowth = 0;
        using var next = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.True(Vp8lReferenceCache.TryBestBitsWithHistory(new uint[1], [Vp8lToken.Literal(0)], [Vp8lToken.Literal(0)], 10, next, out _, out _));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
