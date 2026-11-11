using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class CachedWideQuartetTests
{
    [Theory]
    [InlineData(4, false)] [InlineData(5, false)] [InlineData(6, false)]
    [InlineData(7, false)] [InlineData(8, false)]
    [InlineData(4, true)] [InlineData(5, true)] [InlineData(6, true)]
    [InlineData(7, true)] [InlineData(8, true)]
    public void PrivateRowsAndCarriedPairsSurviveRepairTailsAndPlanReuse(int width, bool lowAlpha)
    {
        const int height = 3, sourceHeight = 13;
        var sourceWidth = 4 * width + 1;
        using var root = new TestMemoryBudget(1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        using var pattern = OriginalPixels.Buffer(sourceWidth, sourceHeight, 8);
        using var source = ImageBuffer.Create(sourceWidth, sourceHeight, 8, limits);
        for (var y = 0; y < sourceHeight; y++)
        {
            pattern.GetRow8(y).CopyTo(source.GetRow8(y));
            if (lowAlpha)
                for (var x = 0; x < sourceWidth; x++) source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 3);
        }
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, sourceWidth, sourceHeight, 8,
            width, height, ResizeFilter.Bicubic);
        using var plan = BicubicResizePlan.Create(sourceWidth, sourceHeight, width, height, 8, limits);
        using var first = plan.Resize(source);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        var sourceBytes = source.Stride * source.Height;
        var outputBytes = second.Stride * second.Height;
        Assert.Equal(sourceBytes + plan.StorageBytes + 2 * outputBytes, root.Bytes);
        first.GetRow8(0)[0] ^= 255;
        first.Dispose();
        using (var third = plan.Resize(source))
        {
            Assert.Equal(expected, OriginalPixels.Bytes(third));
            Assert.Equal(expected, OriginalPixels.Bytes(second));
        }
        plan.Dispose();
        Assert.Equal(sourceBytes + outputBytes, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.False(root.IsDisposed);
    }
}
