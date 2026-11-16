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
    public void AtMostTwoIdleArraysAreRetained()
    {
        var pool = new PixelStoragePool();
        var first = pool.Rent(16); var second = pool.Rent(16); var third = pool.Rent(16);
        pool.Return(first); pool.Return(second); pool.Return(third);
        Assert.Same(first, pool.Rent(16));
        Assert.Same(second, pool.Rent(16));
        Assert.NotSame(third, pool.Rent(16));
    }

    [Theory]
    [InlineData(16, 16, 32, true)]
    [InlineData(17, 15, 32, true)]
    [InlineData(24, 24, 32, false)]
    [InlineData(32, 1, 32, false)]
    [InlineData(1, 1, 0, false)]
    public void CombinedIdleBytesRespectTheOriginalCeiling(int firstLength, int secondLength, int ceiling, bool keepsBoth)
    {
        var pool = new PixelStoragePool(ceiling);
        var first = pool.Rent(firstLength); var second = pool.Rent(secondLength);
        pool.Return(first); pool.Return(second);
        var firstLease = pool.Rent(firstLength); var secondLease = pool.Rent(secondLength);
        Assert.Equal(firstLength, firstLease.Length); Assert.Equal(secondLength, secondLease.Length);
        Assert.NotSame(firstLease, secondLease);
        Assert.Equal(firstLength <= ceiling, ReferenceEquals(first, firstLease));
        Assert.Equal(keepsBoth, ReferenceEquals(second, secondLease));
    }

    [Fact]
    public void DuplicateIdleReturnCannotCreateTwoLiveLeasesOfOneArray()
    {
        var pool = new PixelStoragePool(); var buffer = pool.Rent(16);
        pool.Return(buffer); pool.Return(buffer);
        var first = pool.Rent(16); var second = pool.Rent(16);
        Assert.Same(buffer, first); Assert.NotSame(first, second);
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

    [Theory]
    [InlineData(2)] [InlineData(4)]
    public async Task ConcurrentLeasesKeepAllLiveSamplesIndependent(int retainedArrays)
    {
        var pool = new PixelStoragePool(retainedArrays: retainedArrays);
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
    [Fact]
    public void FourFittingExactSizesSurviveUntilIndependentlyRented()
    {
        var pool = new PixelStoragePool(100, retainedArrays: 4);
        byte[][] buffers = [new byte[24], new byte[23], new byte[22], new byte[21]];
        foreach (var buffer in buffers) pool.Return(buffer);
        var discarded = new byte[20]; pool.Return(discarded);
        foreach (var buffer in buffers)
            Assert.Same(buffer, pool.RentPreservingIdle(buffer.Length));
        Assert.NotSame(discarded, pool.RentPreservingIdle(discarded.Length));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelStoragePool(100, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelStoragePool(100, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PixelStoragePool(100, 5));
    }

    [Fact]
    public void FourSlotLargestReplacementKeepsTheCombinedByteCeiling()
    {
        var pool = new PixelStoragePool(100, retainedArrays: 4);
        byte[][] buffers = [new byte[24], new byte[23], new byte[22], new byte[21]];
        foreach (var buffer in buffers) pool.Return(buffer);
        var replacement = new byte[25]; pool.ReturnLargest(replacement);
        var oversized = new byte[75]; pool.ReturnLargest(oversized);
        Assert.NotSame(oversized, pool.RentPreservingIdle(75));
        Assert.NotSame(buffers[3], pool.RentPreservingIdle(21));
        Assert.Same(replacement, pool.RentPreservingIdle(25));
        foreach (var buffer in buffers.Take(3))
            Assert.Same(buffer, pool.RentPreservingIdle(buffer.Length));
    }

    [Fact]
    public void FourSlotLatestReturnDropsOnlyStorageThatDoesNotFit()
    {
        var pool = new PixelStoragePool(100, retainedArrays: 4);
        byte[][] buffers = [new byte[24], new byte[23], new byte[22], new byte[21]];
        foreach (var buffer in buffers) pool.Return(buffer);
        var latest = new byte[70]; pool.ReturnLatest(latest);
        Assert.Same(latest, pool.RentPreservingIdle(70));
        Assert.Same(buffers[0], pool.RentPreservingIdle(24));
        foreach (var buffer in buffers.Skip(1))
            Assert.NotSame(buffer, pool.RentPreservingIdle(buffer.Length));
        var zero = new PixelStoragePool(0, retainedArrays: 4);
        var positive = new byte[1]; zero.ReturnLatest(positive);
        Assert.NotSame(positive, zero.RentPreservingIdle(1));
        Assert.Empty(zero.RentImage(0, default, clear: true));
    }

}
