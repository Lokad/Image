using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;
using Lokad.Image.Internal;

public sealed class Vp8lCacheHistoryTests
{
    private uint state = 0x71a9d35b;
    private uint Next() { state ^= state << 13; state ^= state >> 17; state ^= state << 5; return state; }
    private Vp8lToken[] Seed(uint[] pixels)
    {
        var result = new List<Vp8lToken>();
        for (var at = 0; at < pixels.Length;)
        {
            if ((Next() & 3) == 0)
            { var length = Math.Min(pixels.Length - at, 1 + (int)(Next() % 73)); result.Add(Vp8lToken.Copy(length, 1)); at += length; }
            else { result.Add(Vp8lToken.Literal(pixels[at])); at++; }
        }
        return result.ToArray();
    }
    [Fact]
    public void FrozenHistogramsChoicesAndEveryPrefixCacheRemainExact()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        long compared = 0; for (var test = 0; test < 2048; test++)
        {
            var length = 1 + (int)(Next() % 257); var maximumBits = test % 11; var pixels = new uint[length];
            for (var i = 0; i < length; i++) pixels[i] = (test % 6) switch {
                0 => 0U, 1 => (Next() % 7) * 0x00000100U, 2 => Next(),
                3 => 0xff000000U | ((uint)i % 3) * 0x010101U,
                4 => (i & 1) == 0 ? 0U : 0x10001U, _ => (Next() & 255) * 0x01010101U };
            var a = Seed(pixels); var b = Seed(pixels);
            using var original = new OperationContext(ImageLimits.Default, default); using var candidate = new OperationContext(ImageLimits.Default, default);
            var before = (Vp8lReferenceCache.BestBits(pixels,a,maximumBits,original),Vp8lReferenceCache.BestBits(pixels,b,maximumBits,original));
            if (!Vp8lReferenceCache.TryBestBitsWithHistory(pixels, a, b, maximumBits, candidate, out var ca, out var cb))
            { ca = Vp8lReferenceCache.BestBits(pixels, a, maximumBits, candidate); cb = Vp8lReferenceCache.BestBits(pixels, b, maximumBits, candidate); }
            var after = (ca, cb);
            if (before != after)
                throw new Exception($"Choice/cleanup mismatch at {test}: {before}/{after}");
            var history = new byte[length];
            Vp8lReferenceCache.BuildHistory(pixels, maximumBits, new uint[(1 << (maximumBits + 1)) - 2], history);
            for (var bits = 1; bits <= maximumBits; bits++)
            {
                var cache = new uint[1 << bits];
                for (var position = 0; position < pixels.Length; position++)
                {
                    var key = Vp8lReferenceCache.Key(pixels[position], bits);
                    Assert.Equal(cache[key] == pixels[position], history[position] <= bits);
                    cache[key] = pixels[position];
                }
            }
            var firstCounts = new int[Vp8lReferenceCache.HistogramValues(maximumBits)];
            var secondCounts = new int[firstCounts.Length];
            Vp8lReferenceCache.FillAllFromHistory(pixels, a, history, maximumBits, firstCounts);
            Vp8lReferenceCache.FillAllFromHistory(pixels, b, history, maximumBits, secondCounts);
            for (var bits = 0; bits <= maximumBits; bits++)
            for (var side = 0; side < 2; side++)
            {
                var layout = new Vp8lHistogramLayout(bits);
                var counts = (side == 0 ? firstCounts : secondCounts).AsSpan(Vp8lReferenceCache.HistogramOffset(bits), layout.Total);
                hash.AppendData(MemoryMarshal.AsBytes(counts)); compared++;
            }
            hash.AppendData(MemoryMarshal.AsBytes(pixels.AsSpan()));
            int[] choice=[before.Item1,before.Item2];hash.AppendData(MemoryMarshal.AsBytes(choice.AsSpan()));
        }
        Assert.Equal(24558, compared);
        Assert.Equal("3173bdbb95d0152e56e89d294a53b1e6b4f34b7ba4ccc8cf1a0e66c6d0b3a1ab", Convert.ToHexStringLower(hash.GetHashAndReset()));
    }
}

public sealed class Vp8lCacheHistoryAdmissionTests
{
    private static readonly uint[] Pixels = [0, 7, 7, 0];
    private static readonly Vp8lToken[] First = [Vp8lToken.Literal(0), Vp8lToken.Literal(7), Vp8lToken.Copy(2, 1)];
    private static readonly Vp8lToken[] Second = [Vp8lToken.Literal(0), Vp8lToken.Copy(3, 1)];
    // Four history bytes,2,046uint cache entries,14,014int all-width histograms,128B metadata.
    private const long Scratch = 64_372;

    [Fact]
    public void ZeroWidthCancellableAndTightLocalCasesSelectBeforeAdmission()
    {
        using var root = new TestMemoryBudget(100_000); using var cancellation = new CancellationTokenSource();
        for (var kind = 0; kind < 3; kind++)
        {
            using var context = new OperationContext(new() { MemoryBudget = root,
                MaxWorkingBytes = kind == 2 ? Scratch - 1 : 100_000 }, kind == 0 ? cancellation.Token : default);
            var attempts = root.GrowthAttempts;
            Assert.False(Vp8lReferenceCache.TryBestBitsWithHistory(Pixels, First, Second, kind == 1 ? 0 : 10, context, out _, out _));
            Assert.Equal(attempts, root.GrowthAttempts); Assert.Equal(0, root.Bytes);
        }
        Assert.False(root.IsDisposed);
    }

    [Fact]
    public void SharedRefusalIsTerminalAndUnusedStorageRefundsBeforeIndependentRecovery()
    {
        using var root = new TestMemoryBudget(100_000);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        root.RefuseAtGrowth = 1;
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceCache.TryBestBitsWithHistory(Pixels, First, Second, 10, context, out _, out _));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(1, root.GrowthAttempts); Assert.Equal(0, root.Bytes);         root.RefuseAtGrowth = 0; var observed = false;
        root.Admitted = _ => { Assert.Equal(Scratch, root.Bytes); observed = true; };
        using var next = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.True(Vp8lReferenceCache.TryBestBitsWithHistory(Pixels, First, Second, 10, next, out _, out _));
        Assert.True(observed); Assert.Equal(Scratch, root.Peak); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void OneRootGovernsConcurrentOwnersAndReleaseAllowsSubsequent()
    {
        using var root = new TestMemoryBudget(Scratch + 1023);
        using var owner = new OperationContext(new() { MemoryBudget = root }, default); owner.Reserve(1024);
        using var context = new OperationContext(new() { MemoryBudget = root }, default);
        var error = Assert.Throws<ImageLimitException>(() => Vp8lReferenceCache.TryBestBitsWithHistory(Pixels, First, Second, 10, context, out _, out _));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals); Assert.Equal(1024, root.Bytes);
        owner.Dispose(); Assert.Equal(0, root.Bytes);
        using var next = new OperationContext(new() { MemoryBudget = root }, default);
        Assert.True(Vp8lReferenceCache.TryBestBitsWithHistory(Pixels, First, Second, 10, next, out _, out _));
        Assert.Equal(Scratch, root.Peak); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
