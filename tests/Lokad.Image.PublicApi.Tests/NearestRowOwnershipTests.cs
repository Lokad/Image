using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class NearestRowOwnershipTests
{
    [Theory]
    [InlineData(8, 1, 3, 2053, 11)] [InlineData(16, 1, 3, 2053, 11)]
    [InlineData(8, 17, 5, 7, 13)] [InlineData(16, 17, 5, 7, 13)]
    public void EnlargedRowsPreserveExactPixelsAndIndependentChargedLifetimes(
        int depth, int sourceWidth, int sourceHeight, int width, int height)
    {
        using var original = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        var input = OriginalPixels.Bytes(original);
        var expected = ExactResizeReference.Resize(input, sourceWidth, sourceHeight, depth,
            width, height, ResizeFilter.Nearest);
        using var root = new TestMemoryBudget(input.Length + 2L * expected.Length);
        var limits = new ImageLimits
        {
            MemoryBudget = root,
            MaxWorkingBytes = input.Length + (long)expected.Length
        };
        using var source = original.Clone(limits);
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Nearest, limits);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Nearest, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input.Length + 2L * expected.Length, root.Bytes);
        var attempts = root.Attempts;
        var refusal = Assert.Throws<ImageLimitException>(() =>
            ImageTransforms.Resize(source, width, height, ResizeFilter.Nearest, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope);
        Assert.Equal(attempts + 1, root.Attempts);
        Assert.Equal(input.Length + 2L * expected.Length, root.Bytes);

        first.Dispose();
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => cancellation.Cancel();
        try
        {
            Assert.Throws<OperationCanceledException>(() =>
                ImageTransforms.Resize(source, width, height, ResizeFilter.Nearest, limits, cancellation.Token));
        }
        finally { root.Admitted = null; }
        Assert.Equal(input.Length + (long)expected.Length, root.Bytes);
        source.Dispose();
        Assert.Equal(expected.Length, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        using (var subsequent = ImageTransforms.Resize(original, width, height, ResizeFilter.Nearest, limits))
        {
            Assert.Equal(expected, OriginalPixels.Bytes(subsequent));
            Assert.Equal(2L * expected.Length, root.Bytes);
            if (depth == 8) subsequent.GetRow8(0)[0] ^= 255;
            else subsequent.GetRow16(0)[0] ^= 65535;
        }
        Assert.Equal(expected.Length, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(original));
        second.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
    }
}
