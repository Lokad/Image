using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class MixedWordColumnBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 1027L * 129 * 5 * 10 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 1027L * 129 * 8 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("6e289a9a8316325b78988adbad8ab7102c81e302216b79c45ef086410f6f691a",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
        using var changed = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 1027, 129, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void IdentityWidthKeepsFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 513L * 129 * 5 * 10 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 513L * 129 * 8 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 513, 129, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 513, 129, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("275a5c3d38d7ba7e6a1ca713fb69fd2ceaf665c18db8269f0bcc1d3e58dcd6fd",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 513, 129, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 513, 129, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
        using var changed = ImageTransforms.Resize(source, 513, 129, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 513, 129, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(1, 9, 5, false)] [InlineData(5, 9, 5, false)] [InlineData(9, 11, 6, false)]
    [InlineData(5, 9, 5, true)] [InlineData(9, 11, 6, true)]
    public void IdentityWidthKeepsTailsEdgesAndLowAlpha(int width, int sourceHeight, int height, bool tiny)
    {
        using var source = OriginalPixels.Buffer(width, sourceHeight, 16);
        if (tiny)
            for (var y = 0; y < sourceHeight; y++)
            for (var x = 0; x < width; x++)
                source.GetRow16(y)[x * 4 + 3] = (ushort)((x + y) % 7 == 0 ? 65535 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var direct = tiny
            ? ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic)
            : ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
                new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 8 + 4095 });
        using var bounded = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + (long)width * height * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(direct), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void IdentityWidthUnwindsBeforeExactTinyAlphaScratch()
    {
        using var source = ImageBuffer.Create(17, 9, 16);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new ushort[] { 65535, 65535, 65535, (ushort)(y < 4 ? 1 : 0) }.CopyTo(source.GetRow16(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 17L * 5 * 8 + 4096 };
        using var result = ImageTransforms.Resize(source, 17, 5, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 17, 9, 16, 17, 5, ResizeFilter.Bicubic), OriginalPixels.Bytes(result));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 17, 5, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }));
    }

    [Fact]
    public void IdentityWidthKeepsPixelsAcrossTheRowCacheAdmissionBoundary()
    {
        using var source = OriginalPixels.Buffer(17, 9, 16);
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, 17, 5, ResizeFilter.Bicubic);
        const long rowCacheAndScratch = 17L * (85 * 8 + 17 * 4) + 128 + 4096;
        foreach (var delta in new[] { -1, 0, 1 })
        {
            using var actual = ImageTransforms.Resize(source, 17, 5, ResizeFilter.Bicubic,
                new ImageLimits { MaxWorkingBytes = input.Length + 17L * 5 * 8 + rowCacheAndScratch + delta });
            Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
        }
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(5, 9, 11, 5, false)] [InlineData(9, 5, 5, 11, false)]
    [InlineData(5, 11, 12, 6, false)] [InlineData(11, 5, 6, 12, false)]
    [InlineData(5, 9, 11, 5, true)] [InlineData(9, 5, 5, 11, true)]
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
