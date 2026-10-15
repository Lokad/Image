using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class CrossColumnBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 256L * 519 * 10 * 5 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 256L * 519 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 256, 519, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 256, 519, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the reverse mixed-axis
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("46b86650bc27febb8c0ed688a02591b660fe77b099c5b9e2ffe42a94e38cae5e",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 256, 519, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 256, 519, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
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
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 256L * 259 * 10 * 5 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 256L * 259 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 256, 259, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 256, 259, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the reverse mixed-axis
        // benchmark; frozen binary64 pixels retain certified rational alpha halves.
        Assert.Equal("a5d2e75dbc30006f845c8a69061e096ded94ed06248e50350ec2c18456606a20",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 256, 259, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 256, 259, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
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
        using var source = OriginalPixels.Buffer(sourceWidth, height, 8);
        if (tiny)
            for (var y = 0; y < height; y++)
            for (var x = 0; x < sourceWidth; x++)
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

    [Fact]
    public void IdentityHeightUnwindsBeforeExactTinyAlphaScratch()
    {
        using var source = ImageBuffer.Create(9, 17, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new byte[] { 255, 255, 255, (byte)(x < 4 ? 1 : 0) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 5L * 17 * 4 + 4096 };
        using var actual = ImageTransforms.Resize(source, 5, 17, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 9, 17, 8, 5, 17, ResizeFilter.Bicubic), OriginalPixels.Bytes(actual));
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
        const long peak = (5 * 6 + 3 * 13) * 4 + 4096;
        var limits = new ImageLimits { MaxWorkingBytes = peak };
        using var output = ImageTransforms.Resize(source, 3, 13, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 5, 6, 8, 3, 13, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        for (var x = 0; x < 3; x++)
            Assert.Equal(new byte[] { 255, 255, 255, 1 }, output.GetRow8(6).Slice(x * 4, 4).ToArray());
        for (var y = 0; y < 13; y++)
        for (var x = 0; x < 3; x++)
        {
            var pixel = output.GetRow8(y).Slice(x * 4, 4);
            if (pixel[3] == 0) Assert.True(pixel[..3].SequenceEqual(new byte[3]));
        }
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 3, 13, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 3, 13, ResizeFilter.Bicubic,
            limits with { MaxWork = 3L * 13 * 8 * 5 * 4 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 3, 13, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
    }
}
