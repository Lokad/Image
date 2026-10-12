using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PixelStoragePoolTests
{
    [Fact]
    public void IndependentPoolsRespectTheirOwnCeilingsAndExactLeases()
    {
        var input = new PixelStoragePool(32); var output = new PixelStoragePool(64);
        var first = input.Rent(32); var held = input.Rent(32); var raster = output.Rent(32);
        Assert.NotSame(first, held); Assert.NotSame(first, raster); Assert.NotSame(held, raster);
        first.AsSpan().Fill(0xa5); input.Return(first);
        var larger = input.Rent(33); input.Return(larger);
        Assert.Same(first, input.Rent(32));
        output.Return(raster); Assert.Same(raster, output.Rent(32));
        Assert.Equal(32, held.Length); Assert.Equal(33, larger.Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelStoragePool(-1));
    }

    [Fact]
    public void OnlyReleasedExactSizeStorageCanBeReused()
    {
        var pool = new PixelStoragePool();
        var first = pool.Rent(33); var live = pool.Rent(33);
        Assert.NotSame(first, live);
        first.AsSpan().Fill(0xA5);
        pool.Return(first);
        Assert.Same(first, pool.Rent(33));
        pool.Return(live);
        var different = pool.Rent(34);
        Assert.Equal(34, different.Length);
        Assert.NotSame(live, different);
    }

    [Fact]
    public void AtMostOneIdleArrayIsRetained()
    {
        var pool = new PixelStoragePool();
        var first = pool.Rent(16); var second = pool.Rent(16);
        pool.Return(first); pool.Return(second);
        Assert.Same(first, pool.Rent(16));
        Assert.NotSame(second, pool.Rent(16));
    }

    [Fact]
    public void RetentionCeilingDoesNotRoundOrDisplaceAnIdleSmallerArray()
    {
        var pool = new PixelStoragePool();
        var maximum = pool.Rent(PixelStoragePool.MaxRetainedBytes);
        pool.Return(maximum);
        Assert.Same(maximum, pool.Rent(PixelStoragePool.MaxRetainedBytes));
        var small = pool.Rent(17); pool.Return(small);
        var tooLarge = pool.Rent(PixelStoragePool.MaxRetainedBytes + 1);
        Assert.Equal(PixelStoragePool.MaxRetainedBytes + 1, tooLarge.Length);
        pool.Return(tooLarge);
        Assert.Same(small, pool.Rent(17));
        Assert.NotSame(tooLarge, pool.Rent(tooLarge.Length));
    }

    [Fact]
    public async Task ConcurrentLeasesKeepAllLiveSamplesIndependent()
    {
        var pool = new PixelStoragePool();
        await Task.WhenAll(Enumerable.Range(1, 8).Select(tag => Task.Run(() =>
        {
            for (var iteration = 0; iteration < 100; iteration++)
            {
                var buffer = pool.Rent(257);
                buffer.AsSpan().Fill((byte)tag);
                Thread.Yield();
                Assert.All(buffer, value => Assert.Equal((byte)tag, value));
                pool.Return(buffer);
            }
        })));
    }
}
