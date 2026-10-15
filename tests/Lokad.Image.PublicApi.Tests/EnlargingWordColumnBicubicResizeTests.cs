using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class EnlargingWordColumnBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 1027L * 519 * 5 * 5 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 1027L * 519 * 8 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("c17909e129d3a6775ebd3c863c5fce3e8c3fedfd0f9cb5aefd6eda4a68e993c0",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 1027, 519, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
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
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 513L * 519 * 5 * 5 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 513L * 519 * 8 + scratch, MaxWork = work + 10_000_000 };
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
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("c9f1c8cc6a1a0bbfc4f8b5de0bfabc79484c81d5aa062a3b2630a81fba0f6676",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 513, 519, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 513, 519, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
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
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 1027L * 259 * 5 * 5 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 1027L * 259 * 8 + scratch, MaxWork = work + 10_000_000 };
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
        Assert.Equal("12073f9f1f3c8aa21fd50826350ec72021c8252923f9f52055302cb811405a35",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 1027, 259, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 1027, 259, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
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
        using var source = OriginalPixels.Buffer(sw, sh, 16);
        if (tiny)
            for (var y = 0; y < sh; y++)
            for (var x = 0; x < sw; x++)
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

    [Theory]
    [InlineData(9, 17, 9, 35)] [InlineData(9, 17, 19, 17)]
    public void IdentityAxisUnwindsBeforeExactTinyAlphaScratch(int sw, int sh, int width, int height)
    {
        using var source = ImageBuffer.Create(sw, sh, 16);
        for (var y = 0; y < sh; y++)
        for (var x = 0; x < sw; x++)
            new ushort[] { 65535, 65535, 65535, (ushort)((width == sw ? y : x) < 4 ? 1 : 0) }.CopyTo(source.GetRow16(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 8 + 4096 };
        using var actual = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, sw, sh, 16, width, height, ResizeFilter.Bicubic), OriginalPixels.Bytes(actual));
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
