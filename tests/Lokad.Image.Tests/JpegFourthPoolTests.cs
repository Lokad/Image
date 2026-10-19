using System.Collections.Concurrent;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegFourthPoolTests
{
    [Fact]
    public void GrowthPreservesCachedPlanesAndReusesFourDistinctZeroedLeases()
    {
        var pool = new JpegCoefficientPool(4096);
        var first = Enumerable.Range(0, 4).Select(_ => pool.Rent(128)).ToArray();
        for (var i = 0; i < first.Length; i++) Array.Fill(first[i], i + 1);
        for (var i = 0; i < 3; i++) pool.Return(first[i]);
        pool.EnableFourthSlot(); pool.EnableFourthSlot(); pool.Return(first[3]);
        var reused = Enumerable.Range(0, 4).Select(_ => pool.Rent(128)).ToArray();
        for (var i = 0; i < reused.Length; i++)
        {
            Assert.Contains(first, old => ReferenceEquals(old, reused[i])); Assert.All(reused[i], n => Assert.Equal(0, n));
            for (var j = 0; j < i; j++) Assert.NotSame(reused[i], reused[j]);
        }
        var fifth = pool.Rent(128); Assert.DoesNotContain(reused, old => ReferenceEquals(old, fifth));
    }

    [Fact]
    public void OriginalByteCeilingDuplicateReturnsAndShapeEvictionStillApply()
    {
        var pool = new JpegCoefficientPool(48); pool.EnableFourthSlot();
        var first = Enumerable.Range(0, 4).Select(_ => pool.Rent(4)).ToArray();
        foreach (var storage in first) pool.Return(storage);
        pool.Return(first[0]);
        var reused = Enumerable.Range(0, 4).Select(_ => pool.Rent(4)).ToArray();
        for (var i = 0; i < 3; i++) Assert.Same(first[i], reused[i]);
        Assert.DoesNotContain(first, old => ReferenceEquals(old, reused[3]));
        foreach (var storage in reused) pool.Return(storage);
        Assert.Equal(3, pool.Rent(3).Length);
        var changed = pool.Rent(4);
        Assert.DoesNotContain(reused, old => ReferenceEquals(old, changed));
        var disabled = new JpegCoefficientPool(0); disabled.EnableFourthSlot();
        var original = disabled.Rent(4); disabled.Return(original); Assert.NotSame(original, disabled.Rent(4));
    }

    [Fact]
    public void ConcurrentGrowthNeverAliasesActiveLeasesOrReturnsDirtyStorage()
    {
        var pool = new JpegCoefficientPool(4096); var active = new ConcurrentDictionary<int[], byte>(ReferenceEqualityComparer.Instance);
        Parallel.For(0, 16, worker =>
        {
            for (var iteration = 0; iteration < 40; iteration++)
            {
                if ((worker + iteration) % 4 == 0) pool.EnableFourthSlot();
                var storage = pool.Rent(97); Assert.True(active.TryAdd(storage, 0)); Assert.All(storage, n => Assert.Equal(0, n));
                Array.Fill(storage, worker + 1); Thread.Yield(); Assert.All(storage, n => Assert.Equal(worker + 1, n));
                Assert.True(active.TryRemove(storage, out _)); pool.Return(storage);
            }
        });
        Assert.Empty(active);
    }
}
