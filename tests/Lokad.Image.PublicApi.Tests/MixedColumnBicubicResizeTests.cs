using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class MixedColumnBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 1027L * 129 * 5 * 10 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 1027L * 129 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the mixed-axis
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("1ca9938539ba1421fe4f1540055be5d0675ac8ddc1ea87df5932a93ca35a0356",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(5, 9, 11, 5, false)] [InlineData(9, 5, 5, 11, false)]
    [InlineData(5, 11, 12, 6, false)] [InlineData(11, 5, 6, 12, false)]
    [InlineData(5, 9, 11, 5, true)] [InlineData(9, 5, 5, 11, true)]
    public void WiderSupportKeepsStripTailsNegativeLobesAndTinyAlphaExact(
        int sourceWidth, int sourceHeight, int width, int height, bool tiny)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 8);
        if (tiny)
            for (var y = 0; y < sourceHeight; y++)
            for (var x = 0; x < sourceWidth; x++)
                source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 7 == 0 ? 255 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var normal = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void DeferredTinyAlphaKeepsExactHalvesScratchWorkAndClearedTransparentPixels()
    {
        using var source = ImageBuffer.Create(5, 6, 8);
        for (var y = 0; y < 6; y++)
        for (var x = 0; x < 5; x++)
            new byte[] { 255, 255, 255, (byte)(y < 3 ? 1 : 0) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        const long peak = (5 * 6 + 11 * 3) * 4 + 4096;
        var limits = new ImageLimits { MaxWorkingBytes = peak };
        using var output = ImageTransforms.Resize(source, 11, 3, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 5, 6, 8, 11, 3, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        for (var x = 0; x < 11; x++)
            Assert.Equal(new byte[] { 255, 255, 255, 1 }, output.GetRow8(1).Slice(x * 4, 4).ToArray());
        for (var y = 0; y < 3; y++)
        for (var x = 0; x < 11; x++)
        {
            var pixel = output.GetRow8(y).Slice(x * 4, 4);
            if (pixel[3] == 0) Assert.True(pixel[..3].SequenceEqual(new byte[3]));
        }
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 11, 3, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 11, 3, ResizeFilter.Bicubic,
            limits with { MaxWork = 11L * 3 * 5 * 9 * 4 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 11, 3, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
    }
}
