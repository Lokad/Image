using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class CrossByteBicubicGeometryTests
{
    [Theory]
    [InlineData(15, 17)]
    [InlineData(31, 17)]
    [InlineData(32, 35)]
    [InlineData(33, 17)]
    [InlineData(75, 35)]
    public void PrefixAndRetiredFooterOverwritePoisonWithExactPixels(int width, int height)
    {
        if (!Avx512F.IsSupported) return;
        var sourceWidth = width * 2 + 1;
        using var source = OriginalPixels.Buffer(sourceWidth, 17, 8);
        for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
                source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 3 == 0 ? 0 : (x + y) % 3 == 1 ? 1 : 255);
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, sourceWidth, 17, 8, width, height, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(width, height, 8);
        result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        var scaleX = (double)sourceWidth / width;
        Assert.True(CrossColumnBicubicResize.TryApply(source, result, scaleX, 1,
            (long)Math.Ceiling(4 * scaleX) + 1, 5, context));
        Assert.Equal(expected, OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(4096, root.Peak); Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(17)]
    [InlineData(35)]
    public void RefusalAndCancellationKeepPrivateStorageReusable(int height)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(67, 17, 8);
        for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++) source.GetRow8(y)[x * 4 + 3] = 255;
        var expected = ExactResizeReference.Resize(OriginalPixels.Bytes(source), 67, 17, 8, 33, height, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(33, height, 8); result.Pixels.Fill(0xa5);
        using var denied = new TestMemoryBudget(4095);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = denied }, default))
            Assert.Throws<ImageLimitException>(() => CrossColumnBicubicResize.TryApply(source, result, 67d / 33, 1, 10, 5, context));
        Assert.Equal(1, denied.Attempts); Assert.Equal(1, denied.Refusals); Assert.Equal(0, denied.Bytes);
        Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        using var root = new TestMemoryBudget(4096);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => CrossColumnBicubicResize.TryApply(source, result, 67d / 33, 1, 10, 5, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(CrossColumnBicubicResize.TryApply(source, result, 67d / 33, 1, 10, 5, context));
        Assert.Equal(expected, OriginalPixels.Bytes(result));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
