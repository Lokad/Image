using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class MixedWordScheduleTests
{
    [Theory]
    [InlineData(513, 259, 1027, 129, 4096)]
    [InlineData(513, 259, 1027, 129, 8192)]
    [InlineData(67, 211, 139, 107, 4096)]
    [InlineData(33, 259, 69, 129, 8192)]
    public void ResultBackedBandsRetireBeforeSuffixStores(int sourceWidth, int sourceHeight,
        int width, int height, int scratch)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 16);
        source.GetRow16(0)[3] = 0;
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = input.Length + (long)width * height * 8 + 4095 });
        using var result = ImageBuffer.Create(width, height, 16);
        result.Pixels.Fill(0xa5);
        Assert.True(MixedWordColumnBicubicResize.TryLocateResultWorkspace(result, out var start, out var safeRows));
        Assert.True((long)safeRows * result.Stride <= start);
        Assert.True(start + MixedWordColumnBicubicResize.ResultWorkspaceBytes <= result.ByteLength - (long)width * 84);
        using var root = new TestMemoryBudget(scratch);
        using var context = new OperationContext(new() { MaxWorkingBytes = scratch, MemoryBudget = root }, default);
        Assert.True(MixedWordColumnBicubicResize.TryApply(source, result, 1,
            (double)sourceHeight / height, 5, 10, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(scratch, root.Peak);
        Assert.Equal(scratch, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void ResultBackedWorkspaceKeepsTerminalRefusalAndCancellationCleanup()
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(67, 211, 16);
        using var result = ImageBuffer.Create(139, 107, 16);
        using var denied = new TestMemoryBudget(4095);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = denied }, default))
            Assert.Throws<ImageLimitException>(() => MixedWordColumnBicubicResize.TryApply(source, result, 1, 211d / 107, 5, 10, context));
        Assert.Equal(1, denied.Attempts); Assert.Equal(1, denied.Refusals); Assert.Equal(0, denied.Bytes);
        using var root = new TestMemoryBudget(4096);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => MixedWordColumnBicubicResize.TryApply(source, result, 1, 211d / 107, 5, 10, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(MixedWordColumnBicubicResize.TryApply(source, result, 1, 211d / 107, 5, 10, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(35, 17)] [InlineData(39, 19)]
    [InlineData(69, 33)] [InlineData(137, 65)]
    public void LargerBandsPreserveExactFramesTailsAndBorrowedOwnership(int sourceHeight, int height)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(17, sourceHeight, 16);
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, 35, height, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = input.Length + 35L * height * 8 + 4095 });
        using var result = ImageBuffer.Create(35, height, 16);
        using var root = new TestMemoryBudget(8192);
        using var context = new OperationContext(new() { MaxWorkingBytes = 8192, MemoryBudget = root }, default);
        Assert.True(MixedWordColumnBicubicResize.TryApply(source, result, 1, (double)sourceHeight / height, 5, 10, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(8192, root.Peak);
        Assert.Equal(8192, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void LargerBandRefusalIsTerminalAndCancellationReleasesBeforeRecovery()
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(17, 35, 16);
        using var result = ImageBuffer.Create(35, 17, 16);
        using var denied = new TestMemoryBudget(8191);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 8192, MemoryBudget = denied }, default))
            Assert.Throws<ImageLimitException>(() => MixedWordColumnBicubicResize.TryApply(source, result, 1, 35d / 17, 5, 10, context));
        Assert.Equal(1, denied.Refusals); Assert.Equal(1, denied.Attempts); Assert.Equal(0, denied.Bytes);
        using var root = new TestMemoryBudget(8192);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 8192, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => MixedWordColumnBicubicResize.TryApply(source, result, 1, 35d / 17, 5, 10, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 8192, MemoryBudget = root }, default))
            Assert.True(MixedWordColumnBicubicResize.TryApply(source, result, 1, 35d / 17, 5, 10, context));
        Assert.Equal(0, root.Bytes); Assert.Equal(8192, root.Peak); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(13, 7)] [InlineData(29, 15)]
    public void FourTwoAndOneRowTailsUseTheKernelAndPreserveExactFrames(int sourceHeight, int height)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(17, sourceHeight, 16);
        source.GetRow16(0)[3] = 0;
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, 35, height, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = input.Length + 35L * height * 8 + 4095 });
        using var result = ImageBuffer.Create(35, height, 16);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(MixedWordColumnBicubicResize.TryApply(source, result, 1, (double)sourceHeight / height, 5, 9, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void TinyAlphaRefusalAndCancellationReleaseTheSameWorkspaceBeforeRecovery()
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(17, 13, 16);
        using var result = ImageBuffer.Create(35, 7, 16);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow16(y).Slice(x * 4, 4); pixel.Fill(65535);
            pixel[3] = (ushort)(y < 6 ? 1 : 0);
        }
        using var root = new TestMemoryBudget(4096);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.False(MixedWordColumnBicubicResize.TryApply(source, result, 1, 13d / 7, 5, 9, context));
        Assert.Equal(0, root.Bytes);
        using var denied = new TestMemoryBudget(4095);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = denied }, default))
            Assert.Throws<ImageLimitException>(() => MixedWordColumnBicubicResize.TryApply(source, result, 1, 13d / 7, 5, 9, context));
        Assert.Equal(1, denied.Refusals); Assert.Equal(1, denied.Attempts); Assert.Equal(0, denied.Bytes);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => MixedWordColumnBicubicResize.TryApply(source, result, 1, 13d / 7, 5, 9, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        for (var y = 0; y < source.Height; y++) source.GetRow16(y).Fill(65535);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(MixedWordColumnBicubicResize.TryApply(source, result, 1, 13d / 7, 5, 9, context));
        Assert.All(OriginalPixels.Bytes(result), value => Assert.Equal(255, value));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak); Assert.False(root.IsDisposed);
    }
}
