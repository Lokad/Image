using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class CrossByteBicubicOverlapTests
{
    [Theory]
    [InlineData(32, 15, 17)]
    [InlineData(34, 17, 35)]
    [InlineData(36, 17, 17)]
    [InlineData(73, 35, 35)]
    [InlineData(149, 75, 35)]
    public void OverlapAndUnprovenStripTailsKeepRationalPixels(int sourceWidth, int width, int height)
    {
        if (!Avx512F.IsSupported) return;
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
        var scale = (double)sourceWidth / width;
        Assert.True(CrossColumnBicubicResize.TryApply(source, result, scale, 1,
            (long)Math.Ceiling(4 * scale) + 1, 5, context));
        Assert.Equal(expected, OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Equal(4096, root.Peak); Assert.Equal(0, root.Bytes);
        Assert.Equal(4096, context.RemainingWorkingBytes); Assert.False(root.IsDisposed);
    }
}
