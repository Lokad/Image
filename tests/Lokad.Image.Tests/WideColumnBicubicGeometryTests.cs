using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class WideColumnBicubicGeometryTests
{
    [Theory]
    [InlineData(8, 513, 259, 128, 64)] // Four-column preparation and tail retirement.
    [InlineData(16, 513, 259, 128, 64)] // Three-column word preparation.
    [InlineData(8, 307, 197, 75, 47)] // Larger support, three columns and partial strip.
    [InlineData(16, 307, 197, 75, 47)]
    [InlineData(8, 49, 41, 11, 9)] // Result too narrow to retain geometry.
    [InlineData(16, 49, 41, 11, 9)]
    public void WideStripsMatchIndependentDirectPixels(int depth, int width, int height, int outputWidth, int outputHeight)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(width, height, depth);
        // Opaque controls keep certification successful so the whole optimized
        // driver, including prepared and final strips, is compared directly.
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                if (depth == 8) source.GetRow8(y)[x * 4 + 3] = byte.MaxValue;
                else source.GetRow16(y)[x * 4 + 3] = ushort.MaxValue;
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, outputWidth, outputHeight, ResizeFilter.Bicubic,
            new() { MaxWorkingBytes = source.ByteLength + outputWidth * outputHeight * (depth / 8) * 4 });
        using var result = ImageBuffer.Create(outputWidth, outputHeight, depth);
        using var root = new TestMemoryBudget(4096);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        Assert.True(Apply(source, result, context));
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void RefusalAndCancellationReleaseWorkspaceBeforeRecovery(int depth)
    {
        if (!Avx512F.IsSupported) return;
        using var source = OriginalPixels.Buffer(513, 259, depth);
        using var result = ImageBuffer.Create(128, 64, depth);
        var input = OriginalPixels.Bytes(source);
        result.Pixels.Fill(0xa5);
        using var root = new TestMemoryBudget(4096);
        using (var held = root.CreateScope())
        {
            Assert.True(held.TryReserve(1, out var reservation));
            using var refused = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
            Assert.Throws<ImageLimitException>(() => Apply(source, result, refused));
            Assert.Equal(1, root.Bytes); Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        }
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => cancellation.Cancel();
        using (var canceled = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => Apply(source, result, canceled));
        root.Admitted = null;
        Assert.Equal(0, root.Bytes); Assert.Equal(-1, result.Pixels.IndexOfAnyExcept((byte)0xa5));
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096, MemoryBudget = root }, default);
        _ = Apply(source, result, context); // Tiny-alpha certification may request exact replay.
        Assert.Equal(0, root.Bytes); Assert.Equal(4096, root.Peak);
        Assert.Equal(input, OriginalPixels.Bytes(source)); Assert.False(root.IsDisposed);
    }

    private static bool Apply(ImageBuffer source, ImageBuffer result, OperationContext context)
    {
        var sx = (double)source.Width / result.Width; var sy = (double)source.Height / result.Height;
        var tapsX = (long)Math.Ceiling(4 * sx) + 1; var tapsY = (long)Math.Ceiling(4 * sy) + 1;
        return source.BitDepth == 8
            ? WideColumnBicubicResize.TryApply(source, result, sx, sy, tapsX, tapsY, context)
            : WideWordColumnBicubicResize.TryApply(source, result, sx, sy, tapsX, tapsY, context);
    }
}
