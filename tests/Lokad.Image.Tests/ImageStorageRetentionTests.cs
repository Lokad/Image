using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class ImageStorageRetentionTests
{
    [Fact]
    public void OverlappingImageOwnersReuseTheReleasedLargePairWithoutAliasing()
    {
        var pool = new PixelStoragePool(96);
        var first = pool.RentImage(32, default, clear: false);
        var second = pool.RentImage(32, default, clear: false);
        var crop = pool.RentImage(8, default, clear: false);
        Assert.NotSame(first, second); Assert.NotSame(first, crop); Assert.NotSame(second, crop);
        first.AsSpan().Fill(0x35); second.AsSpan().Fill(0x79); crop.AsSpan().Fill(0xa5);
        pool.ReturnLargest(crop); pool.ReturnLargest(second); pool.ReturnLargest(first);
        var retainedFirst = pool.RentImage(32, default, clear: false);
        var retainedSecond = pool.RentImage(32, default, clear: false);
        Assert.NotSame(retainedFirst, retainedSecond);
        Assert.True(ReferenceEquals(first, retainedFirst) || ReferenceEquals(second, retainedFirst));
        Assert.True(ReferenceEquals(first, retainedSecond) || ReferenceEquals(second, retainedSecond));
        Assert.All(first, value => Assert.Equal(0x35, value)); Assert.All(second, value => Assert.Equal(0x79, value));
        Assert.NotSame(crop, pool.RentImage(8, default, clear: false));
    }

    [Theory]
    [InlineData(96)] [InlineData(40)] [InlineData(24)]
    public void ANewSizeRemainsReusableDespiteLargerIdleImages(int ceiling)
    {
        var pool = new PixelStoragePool(ceiling);
        pool.ReturnLargest(new byte[32]); pool.ReturnLargest(new byte[32]);
        var small = pool.RentImage(16, default, clear: false);
        small.AsSpan().Fill(0x63); pool.ReturnLargest(small);
        Assert.Same(small, pool.RentImage(16, default, clear: false));
        Assert.All(small, value => Assert.Equal(0x63, value));
    }

    [Fact]
    public void ReplacementNeverExceedsTheSharedByteCeiling()
    {
        var pool = new PixelStoragePool(40);
        var large = new byte[32]; var small = new byte[8]; var replacement = new byte[16];
        pool.ReturnLargest(large); pool.ReturnLargest(small); pool.ReturnLargest(replacement);
        Assert.Same(large, pool.RentImage(32, default, clear: false));
        Assert.Same(small, pool.RentImage(8, default, clear: false));
        Assert.NotSame(replacement, pool.RentImage(16, default, clear: false));
    }

    [Fact]
    public void DuplicateImageReturnsDoNotAliasLiveOwnersAndCanvasRentClearsDirtySamples()
    {
        var pool = new PixelStoragePool(96); var dirty = new byte[32]; dirty.AsSpan().Fill(0xa5);
        pool.ReturnLargest(dirty); pool.ReturnLargest(dirty);
        var first = pool.RentImage(32, default, clear: true);
        var second = pool.RentImage(32, default, clear: false);
        Assert.Same(dirty, first); Assert.NotSame(first, second); Assert.All(first, value => Assert.Equal(0, value));
    }

    [Fact]
    public void CanceledOverwriteRentDoesNotTakeOrDiscardIdleStorage()
    {
        var pool = new PixelStoragePool(96); var dirty = new byte[32]; dirty.AsSpan().Fill(0x71); pool.ReturnLargest(dirty);
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        var error = Assert.ThrowsAny<OperationCanceledException>(() => pool.RentImage(16, canceled.Token, clear: false));
        Assert.Equal(canceled.Token, error.CancellationToken);
        Assert.Same(dirty, pool.RentImage(32, default, clear: false)); Assert.All(dirty, value => Assert.Equal(0x71, value));
    }

    [Fact]
    public async Task ConcurrentImageRentalsKeepEveryLiveOwnerIndependent()
    {
        var pool = new PixelStoragePool(2048);
        await Task.WhenAll(Enumerable.Range(1, 4).Select(tag => Task.Run(() =>
        {
            for (var i = 0; i < 64; i++)
            {
                var bytes = pool.RentImage(257 + i % 3, default, clear: false);
                bytes.AsSpan().Fill((byte)tag); Thread.Yield();
                Assert.All(bytes, value => Assert.Equal((byte)tag, value)); pool.ReturnLargest(bytes);
            }
        })));
    }
}
