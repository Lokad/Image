using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class CachedBicubicAdmissionTests
{
    // Frozen pre-consolidation charges: small byte/word, odd mixed padding,
    // wide stack metadata and wide heap metadata. No output/source ownership
    // is transferred to the borrowed working context.
    [Theory]
    [InlineData(8, 33, 17, 16, 8, 12640)]
    [InlineData(16, 17, 9, 35, 19, 25732)]
    [InlineData(8, 17, 19, 35, 9, 26244)]
    [InlineData(8, 33, 17, 2, 2, 13368)]
    [InlineData(16, 257, 17, 3, 2, 84036)]
    public void CachedStorageKeepsFrozenAdmissionRefusalCancellationAndRecovery(
        int depth, int sourceWidth, int sourceHeight, int width, int height, int charge)
    {
        if (!Avx512F.IsSupported || !Avx512DQ.IsSupported || !Avx2.IsSupported || !Sse41.IsSupported) return;
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        for (var y = 0; y < sourceHeight; y++) for (var x = 0; x < sourceWidth; x++)
            if (depth == 8) source.GetRow8(y)[x * 4 + 3] = 255;
            else source.GetRow16(y)[x * 4 + 3] = 65535;
        var input = OriginalPixels.Bytes(source);
        using var direct = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 4 * (depth / 8) + 4095 });
        var expected = OriginalPixels.Bytes(direct);
        using var result = ImageBuffer.Create(width, height, depth);
        var scaleX = Math.Max(1, (double)sourceWidth / width); var scaleY = Math.Max(1, (double)sourceHeight / height);
        var tapsX = (long)Math.Ceiling(4 * scaleX) + 1; var tapsY = (long)Math.Ceiling(4 * scaleY) + 1;
        using var root = new TestMemoryBudget(charge);
        var limits = new ImageLimits { MaxWorkingBytes = charge + 4096L, MemoryBudget = root };
        using (var context = new OperationContext(limits, default))
        {
            Assert.True(CachedBicubicResize.TryApply(source, result, scaleX, scaleY, tapsX, tapsY, context));
            Assert.Equal(charge + 4096L, context.RemainingWorkingBytes);
        }
        Assert.Equal(expected, OriginalPixels.Bytes(result)); Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(charge, root.Peak); Assert.Equal(0, root.Bytes); Assert.Equal(1, root.GrowthAttempts);

        using var refused = new TestMemoryBudget(charge - 1);
        using (var context = new OperationContext(limits with { MemoryBudget = refused }, default))
        {
            var error = Assert.Throws<ImageLimitException>(() => CachedBicubicResize.TryApply(source, result, scaleX, scaleY, tapsX, tapsY, context));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        }
        Assert.Equal(1, refused.Refusals); Assert.Equal(1, refused.GrowthAttempts); Assert.Equal(0, refused.Bytes);
        using var cancellation = new CancellationTokenSource(); root.Admitted = _ => cancellation.Cancel();
        for (var y = 0; y < height; y++)
            if (depth == 8) result.GetRow8(y).Fill(7); else result.GetRow16(y).Fill(7);
        var untouched = OriginalPixels.Bytes(result);
        using (var context = new OperationContext(limits, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => CachedBicubicResize.TryApply(source, result, scaleX, scaleY, tapsX, tapsY, context));
        Assert.Equal(untouched, OriginalPixels.Bytes(result)); Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Refusals);
        root.Admitted = null;
        using (var context = new OperationContext(limits, default))
            Assert.True(CachedBicubicResize.TryApply(source, result, scaleX, scaleY, tapsX, tapsY, context));
        Assert.Equal(expected, OriginalPixels.Bytes(result)); Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.False(refused.IsDisposed);
    }
}
