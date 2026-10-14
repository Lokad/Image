using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class CrossWordColumnBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 256L * 519 * 10 * 5 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 256L * 519 * 8 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 256, 519, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 256, 519, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the reverse mixed-axis
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("ee02e57ff550047bf5296fb5af51c9396005653a8d37c3e0032abcdbb6db0db1",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 256, 519, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 256, 519, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
        using var changed = ImageTransforms.Resize(source, 256, 519, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 256, 519, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void IdentityHeightKeepsFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 256L * 259 * 10 * 5 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 256L * 259 * 8 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 256, 259, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 256, 259, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the reverse mixed-axis
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("ef78fb7dbfb05cb1a756a64702961cbe60abd5046fa5f2b7cc6fb91b3072cceb",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 256, 259, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 256, 259, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
        using var changed = ImageTransforms.Resize(source, 256, 259, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 256, 259, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(9, 1, 5, false)] [InlineData(11, 5, 6, false)] [InlineData(13, 7, 7, false)]
    [InlineData(9, 5, 5, true)] [InlineData(11, 7, 6, true)]
    public void IdentityHeightKeepsTailsEdgesAndLowAlpha(int sourceWidth, int height, int width, bool tiny)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, height, 16);
        if (tiny)
            for (var y = 0; y < height; y++)
            for (var x = 0; x < sourceWidth; x++)
                source.GetRow16(y)[x * 4 + 3] = (ushort)((x + y) % 7 == 0 ? 65535 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var expected = tiny
            ? ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic)
            : ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
                new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 8 + 4095 });
        using var actual = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void IdentityHeightUnwindsBeforeExactTinyAlphaScratch()
    {
        using var source = ImageBuffer.Create(9, 17, 16);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new ushort[] { 65535, 65535, 65535, (ushort)(x < 4 ? 1 : 0) }.CopyTo(source.GetRow16(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 5L * 17 * 8 + 4096 };
        using var actual = ImageTransforms.Resize(source, 5, 17, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 9, 17, 16, 5, 17, ResizeFilter.Bicubic), OriginalPixels.Bytes(actual));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 5, 17, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }));
    }

    [Theory]
    [InlineData(9, 5, 5, 11, false)] [InlineData(11, 7, 6, 14, false)]
    [InlineData(11, 5, 6, 12, false)] [InlineData(13, 7, 7, 15, false)]
    [InlineData(9, 5, 5, 11, true)] [InlineData(11, 7, 6, 14, true)]
    public void WiderSupportKeepsStripTailsNegativeLobesAndTinyAlphaExact(
        int sourceWidth, int sourceHeight, int width, int height, bool tiny)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 16);
        if (tiny)
            for (var y = 0; y < sourceHeight; y++)
            for (var x = 0; x < sourceWidth; x++)
                source.GetRow16(y)[x * 4 + 3] = (ushort)((x + y) % 7 == 0 ? 65535 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var normal = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }
}
