using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class EnlargingStripBoundaryTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void StripWrapAndOddTailsMatchDirectOrderAtBothDepths(int depth)
    {
        using var source = OriginalPixels.Buffer(9, 7, depth);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            // Positive high alpha isolates strip wrap from exact tiny-alpha
            // workspace, allowing a4095-byte original-order reference.
            if (depth == 8) source.GetRow8(y)[x * 4 + 3] = (byte)(192 + (x * 7 + y * 11) % 64);
            else source.GetRow16(y)[x * 4 + 3] = (ushort)(49152 + (x * 701 + y * 1103) % 16384);
        }
        var input = OriginalPixels.Bytes(source);
        foreach (var width in new[] { 13, 14, 15, 27, 28, 29 })
        foreach (var height in new[] { 7, 15 })
        {
            var bytes = input.Length + (long)width * height * (depth / 2);
            var limits = ImageLimits.Default with { MaxWorkingBytes = bytes + 4096 };
            using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
                limits with { MaxWorkingBytes = bytes + 4095 });
            using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
            using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
            var frozen = OriginalPixels.Bytes(first);
            Assert.Equal(OriginalPixels.Bytes(expected), frozen); Assert.Equal(frozen, OriginalPixels.Bytes(second));
            if (depth == 8) first.GetRow8(0)[0] ^= 255; else first.GetRow16(0)[0] ^= 65535;
            Assert.Equal(frozen, OriginalPixels.Bytes(second)); Assert.Equal(input, OriginalPixels.Bytes(source));
        }
    }
    [Theory]
    [InlineData(false, false)] [InlineData(true, false)]
    [InlineData(false, true)] [InlineData(true, true)]
    public void TallOutputKeepsExactPixelsAcrossWorkspaceRetirementAndIndependentOwners(bool tiny, bool bounded)
    {
        using var source = OriginalPixels.Buffer(17, 83, 8, opaque: true);
        if (tiny)
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
            {
                var pixel = source.GetRow8(y).Slice(x * 4, 4);
                pixel[..3].Fill(255);
                pixel[3] = (byte)((x + y) % 7 == 0 ? 255 : (x + y) % 3 == 0 ? 1 : 0);
            }
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, 17, 83, 8, 35, 167, ResizeFilter.Bicubic);
        const long capacity = 1_048_576;
        using var root = new TestMemoryBudget(capacity);
        var limits = new ImageLimits { MaxWorkingBytes = bounded ? input.Length + 35L * 167 * 4 + 4096 : ImageLimits.Default.MaxWorkingBytes, MemoryBudget = root };
        using var first = ImageTransforms.Resize(source, 35, 167, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 35, 167, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(first)); Assert.Equal(expected, OriginalPixels.Bytes(second));
        second.GetRow8(166)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(first)); Assert.Equal(input, OriginalPixels.Bytes(source));
        var retained = root.Bytes;
        Assert.True(retained >= 2L * expected.Length);
        Assert.True(root.TryReserve(capacity - retained, out var caller));
        using (caller)
        {
            var failure = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 35, 167, ResizeFilter.Bicubic, limits));
            Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope); Assert.Equal(1, root.Refusals);
            Assert.Equal(capacity, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(first));
        }
        Assert.Equal(retained, root.Bytes);
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 35, 167, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        Assert.Equal(retained, root.Bytes);
        first.Dispose(); second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        using var later = ImageTransforms.Resize(source, 35, 167, ResizeFilter.Bicubic, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(later)); Assert.Equal(input, OriginalPixels.Bytes(source));
        later.Dispose(); Assert.Equal(0, root.Bytes);
    }

}
