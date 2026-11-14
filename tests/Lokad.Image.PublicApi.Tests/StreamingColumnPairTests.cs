using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class StreamingColumnPairTests
{
    [Fact]
    public void OddColumnTailAndEdgesKeepExactPixelsAndSharedOwnership()
    {
        const int width = 17, height = 9, outputBytes = width * height * 8;
        using var source = OriginalPixels.Buffer(521, 263, 16);
        source.GetRow16(0)[3] = 0; source.GetRow16(262)[520 * 4 + 3] = 1;
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, source.Width, source.Height, 16, width, height, ResizeFilter.Bicubic);
        using var root = new TestMemoryBudget(2 * outputBytes + 4096);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + outputBytes + 4096, MemoryBudget = root };
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(first)); Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(2 * outputBytes, root.Bytes);
        Assert.InRange(root.Peak, 2L * outputBytes, 2L * outputBytes + 4096);
        first.GetRow16(0)[0] ^= 65535; first.Dispose();
        Assert.Equal(outputBytes, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        using var refused = new TestMemoryBudget(outputBytes - 1);
        var failure = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, width, height,
            ResizeFilter.Bicubic, limits with { MemoryBudget = refused }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
        Assert.Equal(1, refused.Refusals); Assert.Equal(0, refused.Bytes); Assert.False(refused.IsDisposed);
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, width, height,
            ResizeFilter.Bicubic, limits, new CancellationToken(true)));
        Assert.Equal(outputBytes, root.Bytes); Assert.False(root.IsDisposed);
        using var next = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(next));
    }
}
