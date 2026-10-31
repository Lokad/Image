using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class ResizeAlphaTests
{
    [Theory]
    [InlineData(3, 8, ResizeFilter.Bilinear)] [InlineData(3, 16, ResizeFilter.Bilinear)]
    [InlineData(3, 8, ResizeFilter.Bicubic)] [InlineData(3, 16, ResizeFilter.Bicubic)]
    [InlineData(8192, 8, ResizeFilter.Bilinear)] [InlineData(8192, 16, ResizeFilter.Bilinear)]
    [InlineData(8192, 8, ResizeFilter.Bicubic)] [InlineData(8192, 16, ResizeFilter.Bicubic)]
    [InlineData(8193, 8, ResizeFilter.Bilinear)] [InlineData(8193, 16, ResizeFilter.Bilinear)]
    [InlineData(8193, 8, ResizeFilter.Bicubic)] [InlineData(8193, 16, ResizeFilter.Bicubic)]
    public void SymmetricExactHalfSurvivesIntegerWidthBoundary(int width, int depth, ResizeFilter filter)
    {
        using var source = ImageBuffer.Create(width, 2, depth);
        FillAlpha(source, 0, 1);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096 }, default);
        // Constant rows remove horizontal dependence; symmetric kernels give
        // exactly 1/2 between the two rows, independently of denominator width.
        Assert.Equal(1, ResizeAlpha.Round(source, width / 2, 2, width, 5, filter, context));
        Assert.Equal(4096, context.RemainingWorkingBytes);
    }

    [Theory]
    [InlineData(8, ResizeFilter.Bilinear)] [InlineData(16, ResizeFilter.Bilinear)]
    [InlineData(8, ResizeFilter.Bicubic)] [InlineData(16, ResizeFilter.Bicubic)]
    public void AlphaStepKeepsExactHalfClampingAndNegativeLobes(int depth, ResizeFilter filter)
    {
        using var source = ImageBuffer.Create(1, 2, depth);
        FillAlpha(source, 0, 1);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096 }, default);
        int[] expected = [1, 1, 1, 0, 0];
        for (var y = 0; y < 5; y++) Assert.Equal(expected[y], ResizeAlpha.Round(source, 0, y, 1, 5, filter, context));
    }

    [Theory]
    [InlineData(63, 8)] [InlineData(63, 16)] [InlineData(64, 8)] [InlineData(64, 16)]
    public void ConstantAlphaSurvivesTermCountBoundary(int size, int depth)
    {
        using var source = ImageBuffer.Create(size, size, depth);
        var maximum = depth == 8 ? 255 : 65535;
        FillAlpha(source, -1, maximum);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096 }, default);
        // 63 and 64 fall on opposite sides of the integer-accumulator bound.
        // A constant field must remain constant regardless of clipped support.
        Assert.Equal(maximum, ResizeAlpha.Round(source, 0, 0, 1, 1, ResizeFilter.Bicubic, context));
        Assert.Equal(4096, context.RemainingWorkingBytes);
    }

    [Fact]
    public void RefusalCancellationCleanupAndBorrowedRootReuseRemainExact()
    {
        using var source = ImageBuffer.Create(1, 2, 8);
        FillAlpha(source, 0, 1);
        using var refusedRoot = new TestMemoryBudget(4095);
        using (var refused = new OperationContext(new() { MemoryBudget = refusedRoot }, default))
            Assert.Throws<ImageLimitException>(() => ResizeAlpha.Round(source, 0, 2, 1, 5, ResizeFilter.Bicubic, refused));
        Assert.Equal(1, refusedRoot.Refusals);
        Assert.Equal(1, refusedRoot.Attempts);
        Assert.Equal(0, refusedRoot.Bytes);
        Assert.False(refusedRoot.IsDisposed);

        using var root = new TestMemoryBudget(4096);
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => cancellation.Cancel();
        using (var canceled = new OperationContext(new() { MemoryBudget = root }, cancellation.Token))
            Assert.Throws<OperationCanceledException>(() => ResizeAlpha.Round(source, 0, 2, 1, 5, ResizeFilter.Bicubic, canceled));
        Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        using (var subsequent = new OperationContext(new() { MemoryBudget = root }, default))
            Assert.Equal(1, ResizeAlpha.Round(source, 0, 2, 1, 5, ResizeFilter.Bicubic, subsequent));
        Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, root.Peak);
        Assert.False(root.IsDisposed);
        using var local = new OperationContext(new() { MaxWorkingBytes = 4095 }, default);
        Assert.Throws<ImageLimitException>(() => ResizeAlpha.Round(source, 0, 2, 1, 5, ResizeFilter.Bicubic, local));
    }

    [Fact]
    public void BoundedExactHalfNeedsNoHeapArithmeticStorage()
    {
        using var source = ImageBuffer.Create(3, 2, 16);
        FillAlpha(source, 0, 1);
        using var context = new OperationContext(new() { MaxWorkingBytes = 4096 }, default);
        for (var i = 0; i < 4; i++) _ = ResizeAlpha.Round(source, 1, 2, 3, 5, ResizeFilter.Bicubic, context);
        var before = GC.GetAllocatedBytesForCurrentThread();
        var observed = 0;
        for (var i = 0; i < 16; i++) observed += ResizeAlpha.Round(source, 1, 2, 3, 5, ResizeFilter.Bicubic, context);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(16, observed);
        Assert.Equal(0, allocated);
    }

    private static void FillAlpha(ImageBuffer source, int onlyRow, int alpha)
    {
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            if (onlyRow < 0 || y == onlyRow)
            {
                if (source.BitDepth == 8) source.GetRow8(y)[x * 4 + 3] = (byte)alpha;
                else source.GetRow16(y)[x * 4 + 3] = (ushort)alpha;
            }
    }
}
