using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngStoredCapacityTests
{
    [Fact]
    public void CapacityPreparationDoesNotWriteOrChargeTheOutputCounter()
    {
        using var output = new MemoryStream(); output.Write(new byte[] { 5, 7, 11 }); output.Position = 8;
        using var limited = new LimitedWriteStream(output, 1000000, default);
        limited.PrepareStoredPngCapacity(100000);
        Assert.True(output.Capacity > 100000); Assert.Equal(3, output.Length); Assert.Equal(8, output.Position);
        Assert.Equal(new byte[] { 5, 7, 11 }, output.ToArray()); Assert.Equal(0, limited.Length);
        limited.WriteByte(13); Assert.Equal(1, limited.Length); Assert.Equal(9, output.Position);
        var capacity = output.Capacity; limited.PrepareStoredPngCapacity(65536); Assert.Equal(capacity, output.Capacity);
    }

    [Fact]
    public void IneligibleCapacityPreparationKeepsTheOriginalStorage()
    {
        foreach (var bytes in new[] { -1L, 0, 65535, 200000, long.MaxValue })
        {
            using var output = new MemoryStream(); using var limited = new LimitedWriteStream(output, 1000000, default);
            limited.PrepareStoredPngCapacity(bytes); Assert.Equal(0, output.Capacity); Assert.Equal(0, limited.Length);
        }
        using var cancellation = new CancellationTokenSource(); using var live = new MemoryStream();
        using var canceledPath = new LimitedWriteStream(live, 1000000, cancellation.Token);
        canceledPath.PrepareStoredPngCapacity(100000); Assert.Equal(0, live.Capacity);
        using var subclass = new CallbackOutput(); using var original = new LimitedWriteStream(subclass, 1000000, default);
        original.PrepareStoredPngCapacity(100000); Assert.Equal(0, subclass.CapacityCalls);
        using var hugePosition = new MemoryStream(); hugePosition.Position = int.MaxValue - 2L;
        using var boundedPosition = new LimitedWriteStream(hugePosition, 1000000, default);
        boundedPosition.PrepareStoredPngCapacity(100000); Assert.Equal(0, hugePosition.Capacity);
    }

    [Fact]
    public void FixedStorageRefusalKeepsOriginalWriteQuotaAndFailure()
    {
        using var output = new MemoryStream(new byte[4], writable: true);
        using var limited = new LimitedWriteStream(output, 1000000, default);
        limited.PrepareStoredPngCapacity(100000); Assert.Equal(4, output.Capacity); Assert.Equal(0, limited.Length);
        limited.Write(new byte[] { 1, 2, 3, 4 });
        Assert.Throws<NotSupportedException>(() => limited.WriteByte(5)); Assert.Equal(4, limited.Length);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, output.ToArray());
    }

    private sealed class CallbackOutput : MemoryStream
    {
        public int CapacityCalls { get; private set; }
        public override int Capacity { get => base.Capacity; set { CapacityCalls++; base.Capacity = value; } }
    }
}
