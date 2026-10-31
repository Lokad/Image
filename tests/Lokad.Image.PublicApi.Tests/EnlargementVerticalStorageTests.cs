using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class EnlargementVerticalStorageTests
{
    [Theory]
    [InlineData(41)] [InlineData(42)] [InlineData(43)]
    public void CompleteFramesAcrossPlanBoundaryKeepExactPixelsAndIndependentLifetimes(int width)
    {
        using var source = OriginalPixels.Buffer(19, 17, 8);
        var input = OriginalPixels.Bytes(source); const int height = 35;
        var expected = ExactResizeReference.Resize(input, 19, 17, 8, width, height, ResizeFilter.Bicubic);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4096 };
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(first)); Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source)); first.Dispose(); source.Dispose();
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Fact]
    public void TinyAlphaReplayPublishesPixelsAfterEveryGeometryCellIsOverwritten()
    {
        using var source = OriginalPixels.Buffer(21, 17, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++) source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, 21, 17, 8, 43, 35, ResizeFilter.Bicubic);
        using var result = ImageTransforms.Resize(source, 43, 35, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = input.Length + 43L * 35 * 4 + 4096 });
        Assert.Equal(expected, OriginalPixels.Bytes(result)); Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void SharedAdmissionRefusalAndCancellationReleaseBeforeSuccessfulReuse()
    {
        using var source = OriginalPixels.Buffer(35, 31, 8);
        var input = OriginalPixels.Bytes(source); const int width = 71, height = 65;
        const long outputBytes = width * height * 4;
        var expected = ExactResizeReference.Resize(input, 35, 31, 8, width, height, ResizeFilter.Bicubic);
        using var root = new TestMemoryBudget(outputBytes + 4096);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = input.Length + outputBytes + 4096 };
        using var retained = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(retained)); Assert.Equal(outputBytes, root.Bytes);
        Assert.InRange(root.Peak, outputBytes, outputBytes + 4096);
        var refused = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (refused.Limit, refused.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(outputBytes, root.Bytes); retained.Dispose(); Assert.Equal(0, root.Bytes);
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => { if (root.Bytes >= outputBytes) cancellation.Cancel(); };
        var canceled = Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, width, height,
            ResizeFilter.Bicubic, limits, cancellation.Token));
        Assert.Equal(cancellation.Token, canceled.CancellationToken); Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var next = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits))
            Assert.Equal(expected, OriginalPixels.Bytes(next));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(input, OriginalPixels.Bytes(source));
    }
}
