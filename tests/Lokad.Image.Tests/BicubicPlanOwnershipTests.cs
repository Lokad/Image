using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class BicubicPlanOwnershipTests
{
    [Theory]
    [InlineData(8, false, 37, 11)] [InlineData(16, false, 37, 11)]
    [InlineData(8, true, 37, 11)] [InlineData(16, true, 37, 11)]
    [InlineData(8, false, 263, 11)] [InlineData(16, false, 263, 11)]
    [InlineData(8, false, 263, 47)] [InlineData(16, false, 263, 47)]
    [InlineData(8, true, 263, 47)] [InlineData(16, true, 263, 47)]
    public void StackAndHeapRowMetadataKeepRationalPixelsAndIndependentRetainedOwners(int depth, bool tiny, int height, int outputWidth)
    {
        const int width = 53, outputHeight = 7;
        using var source = OriginalPixels.Buffer(width, height, depth);
        if (tiny)
            for (var y = 0; y < height; y++) for (var x = 0; x < width; x++)
                if (depth == 8) source.GetRow8(y)[x * 4 + 3] = (byte)(x < 29 ? 1 : 0);
                else source.GetRow16(y)[x * 4 + 3] = (ushort)(x < 29 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, width, height, depth, outputWidth, outputHeight, ResizeFilter.Bicubic);
        using var root = new TestMemoryBudget(1 << 20);
        using var plan = BicubicResizePlan.Create(width, height, outputWidth, outputHeight, depth, new() { MemoryBudget = root });
        Assert.Equal(plan.StorageBytes, root.Bytes);
        Assert.Throws<OperationCanceledException>(() => plan.Resize(source, new CancellationToken(true)));
        Assert.Equal(plan.StorageBytes, root.Bytes);
        using var first = plan.Resize(source);
        using var survivor = plan.Resize(source);
        Assert.Equal(expected, OriginalPixels.Bytes(first)); Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        Assert.Equal(plan.StorageBytes + expected.Length * 2L, root.Bytes);
        if (depth == 8) first.GetRow8(0)[0] ^= 127; else first.GetRow16(0)[0] ^= 32767;
        plan.Dispose(); first.Dispose();
        Assert.Equal(expected.Length, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        Assert.Equal(input, OriginalPixels.Bytes(source)); source.Dispose();
        Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        survivor.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
