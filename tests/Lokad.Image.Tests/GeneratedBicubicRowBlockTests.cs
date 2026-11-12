using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class GeneratedBicubicRowBlockTests
{
    [Theory]
    [InlineData(257, 31, 1, 15)]
    [InlineData(259, 32, 1, 16)]
    [InlineData(257, 33, 1, 17)]
    [InlineData(257, 49, 1, 33)]
    [InlineData(65, 257, 31, 1)]
    [InlineData(65, 259, 31, 1)]
    public void SourceAndOutputBlockTailsKeepExactPixelsAndClampedEdges(
        int sourceWidth, int sourceHeight, int width, int height)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 8);
        for (var y = 0; y < sourceHeight; y++)
            for (var x = 0; x < sourceWidth; x++)
                source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 3 == 0 ? 0 : (x + y) % 3 == 1 ? 1 : 255);
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, sourceWidth, sourceHeight, 8, width, height, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(width, height, 8);
        result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        var scaleX = (double)sourceWidth / width;
        var scaleY = (double)sourceHeight / height;
        Assert.True(GeneratedBicubicResize.TryApply(source, result, scaleX, scaleY,
            (long)Math.Ceiling(4 * scaleX) + 1, (long)Math.Ceiling(4 * scaleY) + 1, context));
        Assert.Equal(expected, OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(4096, root.Peak); Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void RefusalAndCancellationReleaseTheBoundedWorkspaceBeforeRecovery()
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(257, 33, 8);
        for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++) source.GetRow8(y)[x * 4 + 3] = 255;
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, 257, 33, 8, 1, 17, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(1, 17, 8); result.Pixels.Fill(0xa5);
        using var denied = new TestMemoryBudget(4095);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = denied }, default))
            Assert.Throws<ImageLimitException>(() => GeneratedBicubicResize.TryApply(source, result, 257, 33d / 17, 1029, 9, context));
        Assert.Equal(1, denied.Attempts); Assert.Equal(1, denied.Refusals); Assert.Equal(0, denied.Bytes);
        Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        using var root = new TestMemoryBudget(4096);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => GeneratedBicubicResize.TryApply(source, result, 257, 33d / 17, 1029, 9, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(GeneratedBicubicResize.TryApply(source, result, 257, 33d / 17, 1029, 9, context));
        Assert.Equal(expected, OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
