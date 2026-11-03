using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

// Directly inspecting the shared pool requires isolation from other tests.
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class FilteredResizeStorageCollection
{
    public const string Name = "Filtered resize storage";
}

[Collection(FilteredResizeStorageCollection.Name)]
public sealed class FilteredResizeStorageTests
{
    private const int Width = 11, Height = 7;

    private static byte[] Poison(int length)
    {
        // Two distinct lengths empty either of the existing exact-size slots.
        _ = PixelStoragePool.Shared.Rent(length + 1);
        var storage = PixelStoragePool.Shared.Rent(length);
        storage.AsSpan().Fill(0xa5);
        PixelStoragePool.Shared.Return(storage);
        return storage;
    }

    [Theory]
    [InlineData(8, ResizeFilter.Bilinear)] [InlineData(16, ResizeFilter.Bilinear)]
    [InlineData(8, ResizeFilter.Bicubic)] [InlineData(16, ResizeFilter.Bicubic)]
    public void DirtyStorageIsCompletelyWrittenAndLiveOutputsNeverShare(int depth, ResizeFilter filter)
    {
        using var source = OriginalPixels.Buffer(19, 11, depth);
        var input = OriginalPixels.Bytes(source);
        using var root = new TestMemoryBudget(8_000_000);
        using var expected = ImageTransforms.Resize(source, Width, Height, filter, new() { MemoryBudget = root });
        var pixels = OriginalPixels.Bytes(expected);
        var dirty = Poison(pixels.Length);
        using var first = ImageTransforms.Resize(source, Width, Height, filter);
        Assert.True(first.Pixels.Overlaps(dirty));
        using var second = ImageTransforms.Resize(source, Width, Height, filter);
        Assert.False(first.Pixels.Overlaps(second.Pixels));
        Assert.Equal(pixels, OriginalPixels.Bytes(first));
        Assert.Equal(pixels, OriginalPixels.Bytes(second));
        first.Pixels[0] ^= 255;
        first.Dispose();
        Assert.Same(dirty, PixelStoragePool.Shared.Rent(pixels.Length));
        dirty.AsSpan().Fill(0x3c);
        PixelStoragePool.Shared.Return(dirty);
        using var subsequent = ImageTransforms.Resize(source, Width, Height, filter);
        Assert.True(subsequent.Pixels.Overlaps(dirty));
        Assert.Equal(pixels, OriginalPixels.Bytes(subsequent));
        Assert.Equal(pixels, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(8, ResizeFilter.Bilinear)] [InlineData(16, ResizeFilter.Bilinear)]
    [InlineData(8, ResizeFilter.Bicubic)] [InlineData(16, ResizeFilter.Bicubic)]
    public async Task SharedOutputsStayChargedThroughRefusalCancellationAndAsyncUse(int depth, ResizeFilter filter)
    {
        using var original = OriginalPixels.Buffer(19, 11, depth);
        using var root = new TestMemoryBudget(8_000_000);
        var limits = new ImageLimits { MemoryBudget = root };
        using var source = original.Clone(limits);
        var inputBytes = source.ByteLength;
        var outputBytes = Width * Height * 4 * (depth / 8);
        var dirty = Poison(outputBytes);

        var attempts = root.Attempts;
        root.RefuseAtGrowth = root.GrowthAttempts + 1;
        var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, Width, Height, filter, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(attempts + 1, root.Attempts);
        Assert.Equal(inputBytes, root.Bytes);
        root.RefuseAtGrowth = 0;

        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, Width, Height, filter, limits, cancellation.Token));
        Assert.Equal(inputBytes, root.Bytes);
        root.Admitted = null;

        using var result = ImageTransforms.Resize(source, Width, Height, filter, limits);
        Assert.False(result.Pixels.Overlaps(dirty));
        Assert.Equal(inputBytes + outputBytes, root.Bytes);
        source.Dispose();
        Assert.Equal(outputBytes, root.Bytes);
        using var output = new MemoryStream();
        await ImageCodec.EncodePngAsync(result, output, new() { Limits = limits });
        Assert.True(output.CanWrite);
        Assert.Equal(outputBytes, root.Bytes);
        Assert.Same(dirty, PixelStoragePool.Shared.Rent(outputBytes));
        PixelStoragePool.Shared.Return(dirty);
        result.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
    }
}
