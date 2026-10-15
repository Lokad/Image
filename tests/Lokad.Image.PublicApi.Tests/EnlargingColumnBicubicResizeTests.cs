using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class EnlargingColumnBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 1027L * 519 * 5 * 5 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 1027L * 519 * 4 + scratch, MaxWork = work * 2 };
        if (scratch < 4096)
        {
            // The authored byte enlargement needs exact tiny-alpha scratch.
            Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits));
            return;
        }
        using var first = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("08cde71ddb8b20be884c7ba812cec349f9302b21a348b79d3614bfda19805b94",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void IdentityWidthEnlargementKeepsFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 513L * 519 * 5 * 5 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 513L * 519 * 4 + scratch, MaxWork = work + 10_000_000 };
        if (scratch < 4096)
        {
            Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 513, 519, ResizeFilter.Bicubic, limits));
            Assert.Equal(input, OriginalPixels.Bytes(source));
            return;
        }
        using var first = ImageTransforms.Resize(source, 513, 519, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 513, 519, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the vertical enlargement
        // benchmark; one independently certified rational blue half retains original binary64 rounding.
        Assert.Equal("705c36dd72e49076298952566b7e68b7922cb649c45d4dbf4cf68d9c6cd2887d",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 513, 519, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 513, 519, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 513, 519, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 513, 519, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void IdentityHeightEnlargementKeepsFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 1027L * 259 * 5 * 5 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 1027L * 259 * 4 + scratch, MaxWork = work + 10_000_000 };
        if (scratch < 4096)
        {
            Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 1027, 259, ResizeFilter.Bicubic, limits));
            Assert.Equal(input, OriginalPixels.Bytes(source));
            return;
        }
        using var first = ImageTransforms.Resize(source, 1027, 259, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 1027, 259, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the horizontal enlargement
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("ef9e67095fcbc4a0d9a4fb2591577e9141a188e6b6323a44073cfd2ea41b7724",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 1027, 259, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 1027, 259, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 1027, 259, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 1027, 259, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(1, 7, 1, 15, false)] [InlineData(9, 1, 19, 1, false)]
    [InlineData(9, 7, 9, 15, false)] [InlineData(11, 7, 23, 7, false)]
    [InlineData(9, 7, 9, 15, true)] [InlineData(11, 7, 23, 7, true)]
    public void IdentityAxisKeepsTailsEdgesAndLowAlpha(int sw, int sh, int width, int height, bool tiny)
    {
        using var source = OriginalPixels.Buffer(sw, sh, 8);
        if (tiny)
            for (var y = 0; y < sh; y++)
            for (var x = 0; x < sw; x++)
                source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 7 == 0 ? 255 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var expected = tiny
            ? ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic)
            : ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
                new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4095 });
        using var actual = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(9, 17, 9, 35)] [InlineData(9, 17, 19, 17)]
    public void IdentityAxisUnwindsBeforeExactTinyAlphaScratch(int sw, int sh, int width, int height)
    {
        using var source = ImageBuffer.Create(sw, sh, 8);
        for (var y = 0; y < sh; y++)
        for (var x = 0; x < sw; x++)
            new byte[] { 255, 255, 255, (byte)((width == sw ? y : x) < 4 ? 1 : 0) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4096 };
        using var actual = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, sw, sh, 8, width, height, ResizeFilter.Bicubic), OriginalPixels.Bytes(actual));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }));
    }

    [Theory]
    [InlineData(7, 5, 15, 11, false)] [InlineData(9, 7, 18, 14, false)]
    [InlineData(7, 5, 16, 12, false)] [InlineData(9, 7, 19, 15, false)]
    [InlineData(7, 5, 15, 11, true)] [InlineData(9, 7, 18, 14, true)]
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
        using var source = ImageBuffer.Create(5, 2, 8);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 5; x++)
            new byte[] { 255, 255, 255, (byte)(1 - y) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        const long peak = (5 * 2 + 11 * 5) * 4 + 4096;
        var limits = new ImageLimits { MaxWorkingBytes = peak };
        using var output = ImageTransforms.Resize(source, 11, 5, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 5, 2, 8, 11, 5, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        Assert.All(output.GetRow8(2).ToArray(), sample => Assert.True(sample is 255 or 1));
        for (var y = 0; y < 5; y++)
        for (var x = 0; x < 11; x++)
        {
            var pixel = output.GetRow8(y).Slice(x * 4, 4);
            if (pixel[3] == 0) Assert.True(pixel[..3].SequenceEqual(new byte[3]));
        }
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 11, 5, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 11, 5, ResizeFilter.Bicubic,
            limits with { MaxWork = 11L * 5 * 5 * 5 * 4 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 11, 5, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
    }
}
