using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class GeneratedBicubicRowBlockTests
{
    [Theory]
    [InlineData(8, 257, 31, 1, 15)]
    [InlineData(8, 259, 32, 1, 16)]
    [InlineData(8, 257, 33, 1, 17)]
    [InlineData(8, 257, 49, 1, 33)]
    [InlineData(8, 65, 257, 31, 1)]
    [InlineData(8, 65, 259, 31, 1)]
    [InlineData(16, 257, 31, 1, 15)]
    [InlineData(16, 259, 32, 1, 16)]
    [InlineData(16, 257, 33, 1, 17)]
    [InlineData(16, 257, 49, 1, 33)]
    [InlineData(16, 65, 257, 31, 1)]
    [InlineData(16, 65, 259, 31, 1)]
    public void SourceAndOutputBlockTailsKeepExactPixelsAndClampedEdges(
        int depth, int sourceWidth, int sourceHeight, int width, int height)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        for (var y = 0; y < sourceHeight; y++)
            for (var x = 0; x < sourceWidth; x++)
                if (depth == 8)
                    source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 3 == 0 ? 0 : (x + y) % 3 == 1 ? 1 : 255);
                else
                    source.GetRow16(y)[x * 4 + 3] = (ushort)((x + y) % 3 == 0 ? 0 : (x + y) % 3 == 1 ? 1 : 65535);
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, sourceWidth, sourceHeight, depth, width, height, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(width, height, depth);
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

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void RefusalAndCancellationReleaseTheBoundedWorkspaceBeforeRecovery(int depth)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(257, 33, depth);
        for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
                if (depth == 8) source.GetRow8(y)[x * 4 + 3] = 255;
                else source.GetRow16(y)[x * 4 + 3] = 65535;
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, 257, 33, depth, 1, 17, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(1, 17, depth); result.Pixels.Fill(0xa5);
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
