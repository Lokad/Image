using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class LanczosRowCacheBoundaryTests
{
    [Theory]
    [InlineData(8, 9, 17, 13, 35)]
    [InlineData(16, 9, 17, 13, 35)]
    [InlineData(8, 17, 33, 9, 11)] // Eligible edges and longer streaming interior.
    [InlineData(16, 17, 33, 9, 11)]
    [InlineData(8, 9, 65, 5, 3)] // Support exceeds the 64-slot cache.
    [InlineData(16, 9, 65, 5, 3)]
    public void CacheWindowBoundariesMatchExactIntegersAndKeepIndependentOwners(
        int depth, int sourceWidth, int sourceHeight, int width, int height)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        var original = OriginalPixels.Bytes(source);
        using var exactContext = new OperationContext(ImageLimits.Default, default);
        using var horizontal = LanczosPlan.Create(sourceWidth, width, exactContext);
        using var vertical = LanczosPlan.Create(sourceHeight, height, exactContext);
        using var workspace = new LanczosWorkspace(horizontal, vertical, depth, false, exactContext);
        using var expected = LanczosResize.Execute(source, width, height, horizontal, vertical, workspace, exactContext);
        using var root = new TestMemoryBudget(1_000_000);
        using var plan = Lanczos3ResizePlan.Create(sourceWidth, sourceHeight, width, height, depth, new() { MemoryBudget = root });
        using var actual = plan.Resize(source);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
        Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(plan.StorageBytes + (long)width * height * 4 * (depth / 8), root.Bytes);
        source.Dispose(); actual.Dispose(); Assert.Equal(plan.StorageBytes, root.Bytes);
        plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(width, expected.Width); Assert.NotEmpty(OriginalPixels.Bytes(expected));
    }
}
