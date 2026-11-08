using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class MixedWordVerticalTests
{
    [Theory]
    [InlineData(1, 13, 7, 4096)]
    [InlineData(2, 13, 7, 4096)]
    [InlineData(17, 13, 7, 4096)]
    [InlineData(17, 35, 17, 8192)]
    [InlineData(33, 19, 9, 4096)]
    [InlineData(33, 20, 9, 4096)]
    public void IdentityAxisKeepsOriginalPixelsEdgesTailsAndAdmittedOwnership(int width, int sourceHeight, int height, int workspace)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(width, sourceHeight, 16);
        source.GetRow16(0)[3] = 0; source.GetRow16(sourceHeight - 1)[width * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = input.Length + width * (long)height * 8 + 4095 });
        using var result = ImageBuffer.Create(width, height, 16);
        using var root = new TestMemoryBudget(workspace);
        using var context = new OperationContext(new() { MaxWorkingBytes = workspace, MemoryBudget = root }, default);
        var scale = (double)sourceHeight / height;
        Assert.True(MixedWordColumnBicubicResize.TryApply(source, result, 1, scale, 5, (long)Math.Ceiling(4 * scale) + 1, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(workspace, root.Peak);
        Assert.Equal(workspace, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void IdentityAxisRefusalCancellationAndTinyAlphaReleaseBeforeRecovery()
    {
        if (!Avx512F.IsSupported) return;
        using var source = ImageBuffer.Create(17, 13, 16);
        using var result = ImageBuffer.Create(17, 7, 16);
        for (var y = 0; y < source.Height; y++)
        {
            source.GetRow16(y).Fill(65535);
            for (var x = 0; x < source.Width; x++) source.GetRow16(y)[x * 4 + 3] = (ushort)(y < 6 ? 1 : 0);
        }
        using var root = new TestMemoryBudget(4096);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.False(MixedWordColumnBicubicResize.TryApply(source, result, 1, 13d / 7, 5, 9, context));
        Assert.Equal(0, root.Bytes);
        using var refused = new TestMemoryBudget(4095);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = refused }, default))
            Assert.Throws<ImageLimitException>(() => MixedWordColumnBicubicResize.TryApply(source, result, 1, 13d / 7, 5, 9, context));
        Assert.Equal(1, refused.Refusals); Assert.Equal(1, refused.Attempts); Assert.Equal(0, refused.Bytes);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => MixedWordColumnBicubicResize.TryApply(source, result, 1, 13d / 7, 5, 9, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        for (var y = 0; y < source.Height; y++) source.GetRow16(y).Fill(65535);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(MixedWordColumnBicubicResize.TryApply(source, result, 1, 13d / 7, 5, 9, context));
        Assert.All(OriginalPixels.Bytes(result), b => Assert.Equal(255, b));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak); Assert.False(root.IsDisposed);
    }
}
