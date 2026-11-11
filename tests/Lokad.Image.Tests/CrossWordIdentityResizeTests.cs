using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class CrossWordIdentityResizeTests
{
    [Theory]
    [InlineData(0, 0, false)] [InlineData(-1, -1, false)]
    [InlineData(1, 1, true)] [InlineData(259, 259, true)]
    [InlineData(67108863, 67108863, true)] [InlineData(67108864, 67108864, true)]
    [InlineData(67108865, 67108865, false)] [InlineData(259, 519, false)]
    public void IdentityGuardRetainsGeneralGeometryOutsideItsExactRange(int source, int result, bool expected) =>
        Assert.Equal(expected, CrossWordColumnBicubicResize.IsIdentityHeight(source, result));

    [Theory]
    [InlineData(1)] [InlineData(259)] [InlineData(32768)] [InlineData(67108864)]
    public void GuardedGeometryHasExactlyOneUnitTap(int height)
    {
        foreach (var y in new[] { 0, height / 2, height - 1 })
        {
            var center = (y + .5) * height / height - .5;
            Assert.Equal((double)y, center);
            var first = (int)Math.Ceiling(center - 2);
            Assert.Equal(y - 2, first);
            Assert.Equal(5, (int)Math.Floor(center + 2) - first + 1);
            for (var t = 0; t < 5; t++)
                Assert.Equal(BitConverter.DoubleToInt64Bits(t == 2 ? 1d : 0d),
                    BitConverter.DoubleToInt64Bits(ImageTransforms.Kernel(first + t - center, ResizeFilter.Bicubic)));
        }
    }

    [Theory]
    [InlineData(513, 259, 256, 259)] [InlineData(31, 23, 16, 129)]
    [InlineData(13, 7, 7, 7)] [InlineData(9, 7, 5, 15)]
    public void IdentityAndGeneralPrefixOrStripPathsOverwritePoisonExactly(int sw, int sh, int width, int height)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(sw, sh, 16);
        for (var y = 0; y < sh; y++) for (var x = 0; x < sw; x++)
            source.GetRow16(y)[x * 4 + 3] = 65535;
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        using var result = ImageBuffer.Create(width, height, 16);
        result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        var sx = (double)sw / width; var sy = Math.Max(1, (double)sh / height);
        Assert.True(CrossWordColumnBicubicResize.TryApply(source, result, sx, sy,
            (long)Math.Ceiling(4 * sx) + 1, (long)Math.Ceiling(4 * sy) + 1, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(4096, root.Peak); Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(259)] [InlineData(519)]
    public void RefusalAndCancellationRetirePrivateViewsBeforeRecovery(int height)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(513, 259, 16);
        for (var y = 0; y < source.Height; y++) for (var x = 0; x < source.Width; x++)
            source.GetRow16(y)[x * 4 + 3] = 65535;
        using var result = ImageBuffer.Create(256, height, 16);
        using var expected = ImageTransforms.Resize(source, 256, height, ResizeFilter.Bicubic);
        result.Pixels.Fill(0xa5);
        var sx = 513d / 256; var sy = Math.Max(1, 259d / height);
        using var denied = new TestMemoryBudget(4095);
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = denied }, default))
            Assert.Throws<ImageLimitException>(() => CrossWordColumnBicubicResize.TryApply(source, result, sx, sy, 10, 5, context));
        Assert.Equal(1, denied.Attempts); Assert.Equal(1, denied.Refusals); Assert.Equal(0, denied.Bytes);
        Assert.False(result.Pixels.ContainsAnyExcept((byte)0xa5));
        using var root = new TestMemoryBudget(4096);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => CrossWordColumnBicubicResize.TryApply(source, result, sx, sy, 10, 5, context));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default))
            Assert.True(CrossWordColumnBicubicResize.TryApply(source, result, sx, sy, 10, 5, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
