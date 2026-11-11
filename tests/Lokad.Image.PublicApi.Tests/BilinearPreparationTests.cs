using Lokad.Image.Tooling;
using System.Runtime.Intrinsics.X86;

namespace Lokad.Image.PublicApi.Tests;

public sealed class BilinearPreparationTests
{
    private static bool Accelerated => Avx512F.IsSupported && Avx2.IsSupported && Avx.IsSupported && Sse41.IsSupported;

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void PreparationSharesTheRootWithoutTheUnusedHorizontalPlan(int depth)
    {
        using var pattern = OriginalPixels.Buffer(513, 259, depth);
        var sourceBytes = pattern.Stride * pattern.Height;
        var outputBytes = 256 * 129 * 4 * (depth / 8);
        using var expected = ImageTransforms.Resize(pattern, 256, 129, ResizeFilter.Bilinear,
            new ImageLimits { MaxWorkingBytes = sourceBytes + outputBytes + 4096 });
        // The prepared operation fits before the former unused horizontal plan;
        // portable original-order filtering retains its existing allowance.
        using var root = new TestMemoryBudget(sourceBytes + outputBytes + (Accelerated ? 24 : 40) * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        using var source = ImageBuffer.Create(513, 259, depth, limits);
        for (var y = 0; y < source.Height; y++)
            if (depth == 8) pattern.GetRow8(y).CopyTo(source.GetRow8(y));
            else pattern.GetRow16(y).CopyTo(source.GetRow16(y));
        var input = OriginalPixels.Bytes(source);
        var pixels = OriginalPixels.Bytes(expected);
        using (var first = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bilinear, limits))
        {
            Assert.Equal(pixels, OriginalPixels.Bytes(first));
            Assert.Equal(sourceBytes + outputBytes, root.Bytes);
            var refusals = root.Refusals;
            var refused = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 256, 129,
                ResizeFilter.Bilinear, limits));
            Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope);
            Assert.Equal(refusals + 1, root.Refusals);
            Assert.Equal(sourceBytes + outputBytes, root.Bytes);
            Assert.Equal(pixels, OriginalPixels.Bytes(first));
        }
        Assert.Equal(sourceBytes, root.Bytes);
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => { if (root.Bytes > sourceBytes + outputBytes) cancellation.Cancel(); };
        var canceled = Assert.ThrowsAny<OperationCanceledException>(() => ImageTransforms.Resize(source, 256, 129,
            ResizeFilter.Bilinear, limits, cancellation.Token));
        Assert.Equal(cancellation.Token, canceled.CancellationToken);
        Assert.Equal(sourceBytes, root.Bytes);
        root.Admitted = null;
        using (var recovered = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bilinear, limits))
            Assert.Equal(pixels, OriginalPixels.Bytes(recovered));
        Assert.Equal(sourceBytes, root.Bytes);
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void WorkspaceRefusalDoesNotRetryAndTheNextOperationSucceeds(int depth)
    {
        using var root = new TestMemoryBudget(2 * 1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        using var source = ImageBuffer.Create(513, 259, depth, limits);
        using var pattern = OriginalPixels.Buffer(513, 259, depth);
        for (var y = 0; y < source.Height; y++)
            if (depth == 8) pattern.GetRow8(y).CopyTo(source.GetRow8(y));
            else pattern.GetRow16(y).CopyTo(source.GetRow16(y));
        var sourceBytes = source.Stride * source.Height;
        // Admit the independent output, then refuse the first owned workspace.
        root.RefuseAtGrowth = root.GrowthAttempts + 2;
        var refusals = root.Refusals;
        var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 256, 129,
            ResizeFilter.Bilinear, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(refusals + 1, root.Refusals);
        Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts);
        Assert.Equal(sourceBytes, root.Bytes);
        root.RefuseAtGrowth = 0;
        using (var result = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bilinear, limits))
        {
            Assert.Equal(sourceBytes + result.Stride * result.Height, root.Bytes);
            using var expected = ImageTransforms.Resize(pattern, 256, 129, ResizeFilter.Bilinear,
                new ImageLimits { MaxWorkingBytes = sourceBytes + result.Stride * result.Height + 4096 });
            Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        }
        Assert.Equal(sourceBytes, root.Bytes);
        Assert.False(root.IsDisposed);
    }
}
