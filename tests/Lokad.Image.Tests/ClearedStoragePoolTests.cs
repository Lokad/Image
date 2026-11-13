using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class ClearedStoragePoolTests
{
    [Fact]
    public void ClearedLeasesPreserveOtherSizesAndIndependentLiveStorage()
    {
        var pool = new PixelStoragePool(96);
        var canvas = new byte[32]; canvas.AsSpan().Fill(0xa5);
        var other = new byte[24]; other.AsSpan().Fill(0x36);
        pool.ReturnLatest(canvas); pool.ReturnLatest(other);
        var first = pool.RentImage(32, default, clear: true);
        Assert.Same(canvas, first); Assert.All(first, value => Assert.Equal(0, value));
        Assert.Same(other, pool.RentPreservingIdle(24));
        Assert.All(other, value => Assert.Equal(0x36, value));
        var second = pool.RentImage(32, default, clear: true);
        Assert.NotSame(first, second); first.AsSpan().Fill(0x5a);
        Assert.All(second, value => Assert.Equal(0, value));
        pool.ReturnLatest(first); pool.ReturnLatest(second); pool.ReturnLatest(other);
        Assert.Same(other, pool.RentPreservingIdle(24));
        Assert.Same(second, pool.RentImage(32, default, clear: true));
        Assert.NotSame(first, pool.RentImage(32, default, clear: true)); // Only two idle arrays.
    }

    [Fact]
    public void CancellationPrecedesTakingIdleStorageAndRecoveryClearsAllSamples()
    {
        var pool = new PixelStoragePool(); var dirty = new byte[32771]; dirty.AsSpan().Fill(0xa5);
        pool.ReturnLatest(dirty); using var source = new CancellationTokenSource(); source.Cancel();
        var error = Assert.ThrowsAny<OperationCanceledException>(() => pool.RentImage(dirty.Length, source.Token, clear: true));
        Assert.Equal(source.Token, error.CancellationToken);
        Assert.Same(dirty, pool.RentPreservingIdle(dirty.Length));
        Assert.All(dirty, value => Assert.Equal(0xa5, value)); pool.ReturnLatest(dirty);
        using var active = new CancellationTokenSource();
        Assert.Same(dirty, pool.RentImage(dirty.Length, active.Token, clear: true));
        Assert.All(dirty, value => Assert.Equal(0, value));
    }

    [Fact]
    public void ColdMissesRemainZeroedWithoutDisplacingIdleStorageOrExceedingTheCap()
    {
        var pool = new PixelStoragePool(16); var idle = new byte[8]; idle.AsSpan().Fill(0x36); pool.Return(idle);
        var large = pool.RentImage(17, default, clear: true); Assert.Equal(17, large.Length); Assert.All(large, value => Assert.Equal(0, value));
        pool.ReturnLatest(large); Assert.Same(idle, pool.RentPreservingIdle(8));
        Assert.NotSame(large, pool.RentImage(17, default, clear: true));
        Assert.Empty(pool.RentImage(0, default, clear: true));
        Assert.Throws<ArgumentOutOfRangeException>(() => pool.RentImage(-1, default, clear: true));
    }

    [Theory]
    [InlineData(96, true)] [InlineData(48, false)]
    public void SizeMissMakesRoomForTheOrdinaryReturnAndKeepsARecentOtherSizeWhenItFits(int ceiling, bool keepsOther)
    {
        var pool = new PixelStoragePool(ceiling);
        var stale = new byte[16]; var other = new byte[24]; other.AsSpan().Fill(0x36);
        pool.ReturnLatest(stale); pool.ReturnLatest(other);
        var canvas = pool.RentImage(32, default, clear: true);
        Assert.All(canvas, value => Assert.Equal(0, value)); pool.Return(canvas);
        Assert.Same(canvas, pool.RentImage(32, default, clear: true));
        var remaining = pool.RentPreservingIdle(24);
        Assert.Equal(keepsOther, ReferenceEquals(other, remaining));
        if (keepsOther) Assert.All(remaining, value => Assert.Equal(0x36, value));
        Assert.NotSame(stale, pool.RentPreservingIdle(16));
    }

    [Fact]
    public async Task ConcurrentClearedLeasesStayZeroedAndNeverAliasLiveStorage()
    {
        var pool = new PixelStoragePool(2048);
        await Task.WhenAll(Enumerable.Range(1, 4).Select(tag => Task.Run(() =>
        {
            for (var i = 0; i < 32; i++)
            {
                var bytes = pool.RentImage(257 + i % 2, default, clear: true);
                Assert.All(bytes, value => Assert.Equal(0, value)); bytes.AsSpan().Fill((byte)tag);
                Thread.Yield(); Assert.All(bytes, value => Assert.Equal((byte)tag, value)); pool.ReturnLatest(bytes);
            }
        })));
    }
}
