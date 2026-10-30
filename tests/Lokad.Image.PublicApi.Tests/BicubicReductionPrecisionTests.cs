using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class BicubicReductionPrecisionTests
{
    [Theory]
    [InlineData(17, 19, 8, 9)]
    [InlineData(65, 33, 32, 16)]
    [InlineData(40, 25, 16, 10)]
    [InlineData(23, 23, 10, 11)]
    [InlineData(7, 9, 3, 4)]
    [InlineData(9, 5, 4, 2)]
    [InlineData(17, 7, 7, 3)]
    public void ReductionKeepsOriginalPixelsAndIndependentOwners(int sw, int sh, int width, int height)
    {
        using var source = OriginalPixels.Buffer(sw, sh, 8);
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4096 });
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        var pixels = OriginalPixels.Bytes(expected);
        Assert.Equal(pixels, OriginalPixels.Bytes(first));
        Assert.Equal(pixels, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        Assert.Equal(pixels, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(63)]
    [InlineData(64)] [InlineData(65)] [InlineData(255)]
    public void TransparencyAndRoundingBoundariesKeepOriginalPixels(byte alpha)
    {
        using var source = OriginalPixels.Buffer(41, 29, 8);
        for (var y = 0; y < source.Height; y++)
        {
            var row = source.GetRow8(y);
            for (var x = 0; x < source.Width; x++) row[x * 4 + 3] = alpha;
        }
        using var expected = ImageTransforms.Resize(source, 17, 13, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = 41 * 29 * 4 + 17 * 13 * 4 + 4096 });
        using var actual = ImageTransforms.Resize(source, 17, 13, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
    }

    [Fact]
    public void AdmissionIsTerminalAndTemporaryChargesEndBeforeTheReturnedOwner()
    {
        using var source = OriginalPixels.Buffer(65, 33, 8);
        var input = OriginalPixels.Bytes(source);
        const int outputBytes = 32 * 16 * 4;
        using var expected = ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + outputBytes + 4096 });
        using var root = new TestMemoryBudget(outputBytes + 32 * 1024);
        var limits = new ImageLimits { MemoryBudget = root };
        var pixels = OriginalPixels.Bytes(expected);
        using (var actual = ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic, limits))
        {
            Assert.Equal(pixels, OriginalPixels.Bytes(actual));
            Assert.Equal(outputBytes, root.Bytes);
        }
        Assert.Equal(0, root.Bytes);
        Assert.True(root.TryReserve(32 * 1024 + 1, out var caller));
        try
        {
            var refusals = root.Refusals;
            var failure = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic, limits));
            Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
            Assert.Equal(refusals + 1, root.Refusals);
            Assert.Equal(32 * 1024 + 1, root.Bytes);
        }
        finally { caller.Dispose(); }
        using var canceled = new CancellationTokenSource();
        root.Admitted = _ => canceled.Cancel();
        Assert.ThrowsAny<OperationCanceledException>(() => ImageTransforms.Resize(source, 32, 16,
            ResizeFilter.Bicubic, limits, canceled.Token));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var recovered = ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic, limits))
            Assert.Equal(pixels, OriginalPixels.Bytes(recovered));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }
}
