using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

[Collection(FilteredResizeStorageCollection.Name)]
public sealed class SharedImageStoragePoolTests
{
    [Fact]
    public void SharedImagePoolRetainsFourIndependentExactLeases()
    {
        var pool = PixelStoragePool.Shared;
        _ = pool.Rent(111);
        var buffers = Enumerable.Range(0, 5).Select(_ => pool.Rent(110)).ToArray();
        foreach (var buffer in buffers) pool.Return(buffer);
        foreach (var buffer in buffers.Take(4)) Assert.Same(buffer, pool.Rent(110));
        Assert.NotSame(buffers[4], pool.Rent(110));
    }
}
