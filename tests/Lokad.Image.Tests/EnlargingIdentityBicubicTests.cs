using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class EnlargingIdentityBicubicTests
{
    [Theory]
    [InlineData(8, 1)] [InlineData(8, 31)] [InlineData(8, 63)]
    [InlineData(16, 1)] [InlineData(16, 31)] [InlineData(16, 63)]
    public void IdentityHeightBoundaryFramesOverwritePoisonExactly(int depth, int height)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(31, height, depth);
        for (var y = 0; y < height; y++) for (var x = 0; x < source.Width; x++)
            if (depth == 8) source.GetRow8(y)[x * 4 + 3] = 255;
            else source.GetRow16(y)[x * 4 + 3] = 65535;
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, 31, height, depth, 62, height, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(62, height, depth); result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(EnlargingBicubicResize.TryApply(source, result, 1, 1, 5, 5, context));
        Assert.Equal(expected, OriginalPixels.Bytes(result)); Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(4096, root.Peak); Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void IdentityHeightLowAlphaKeepsIndependentOwnersAndCleanupBeforeRecovery(int depth)
    {
        using var source = OriginalPixels.Buffer(31, 31, depth);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
            if (depth == 8) source.GetRow8(y)[x * 4 + 3] = (byte)(x < 16 ? 1 : 0);
            else source.GetRow16(y)[x * 4 + 3] = (ushort)(x < 16 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, 31, 31, depth, 62, 31, ResizeFilter.Bicubic);
        var bytes = 62 * 31 * (depth / 2);
        using var root = new TestMemoryBudget(2L * bytes + 4096);
        var limits = new ImageLimits { MaxWorkingBytes = source.ByteLength + bytes + 4096, MemoryBudget = root };
        using (var first = ImageTransforms.Resize(source, 62, 31, ResizeFilter.Bicubic, limits))
        using (var survivor = ImageTransforms.Resize(source, 62, 31, ResizeFilter.Bicubic, limits))
        {
            Assert.Equal(expected, OriginalPixels.Bytes(first)); Assert.Equal(expected, OriginalPixels.Bytes(survivor));
            Assert.Equal(2L * bytes, root.Bytes); Assert.InRange(root.Peak, 2L * bytes, 2L * bytes + 4096);
            var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 62, 31, ResizeFilter.Bicubic, limits));
            Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
            Assert.Equal(2L * bytes, root.Bytes);
            first.Pixels[0] ^= 255; first.Dispose();
            Assert.Equal(bytes, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(survivor));
            using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
            var canceled = Assert.Throws<OperationCanceledException>(() =>
                ImageTransforms.Resize(source, 62, 31, ResizeFilter.Bicubic, limits, cancellation.Token));
            Assert.Equal(cancellation.Token, canceled.CancellationToken); Assert.Equal(bytes, root.Bytes);
            root.Admitted = null;
            using (var recovered = ImageTransforms.Resize(source, 62, 31, ResizeFilter.Bicubic, limits))
                Assert.Equal(expected, OriginalPixels.Bytes(recovered));
            Assert.Equal(bytes, root.Bytes); Assert.Equal(input, OriginalPixels.Bytes(source));
            Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
