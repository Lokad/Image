using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class StreamingBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 16L * 8 * 130 * 131 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 16L * 8 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("eec2e89602720fa8ca330733763524017f49de642fb341ba839aeca0aca887fd",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void BlockedRowsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 8L * 17 * 258 * 62 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 8L * 17 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 8, 17, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 8, 17, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("481df76194da6537d6ecfbc24f37c842ae750b7752791f35d40d43454532eee1",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 8, 17, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 8, 17, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 8, 17, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 8, 17, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(65, false)] [InlineData(17, false)] [InlineData(33, false)]
    [InlineData(65, true)] [InlineData(17, true)]
    public void BlockBoundariesAndTailsKeepFullPrecision(int height, bool tiny)
    {
        using var source = OriginalPixels.Buffer(271, 133, 8);
        if (tiny)
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
                source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 7 == 0 ? 255 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, 4, height, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 4, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + 4L * height * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void BlockedTinyAlphaUnwindsBeforeExactScratch()
    {
        using var source = ImageBuffer.Create(271, 129, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new byte[] { 255, 255, 255, (byte)(x < 135 ? 1 : 0) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 4L * 17 * 4 + 4096 };
        using var output = ImageTransforms.Resize(source, 4, 17, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 271, 129, 8, 4, 17, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 4, 17, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }));
    }

    [Theory]
    [InlineData(121, 61, 3, 2, false)] [InlineData(129, 65, 4, 2, false)]
    [InlineData(123, 63, 3, 2, false)] [InlineData(131, 67, 4, 2, false)]
    [InlineData(121, 61, 3, 2, true)] [InlineData(129, 65, 4, 2, true)]
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
    public void RelativePackedOffsetsKeepAbsoluteCoordinatesAboveUInt16()
    {
        using var source = ImageBuffer.Create(65537, 4, 8, new ImageLimits { MaxDimension = 70000 });
        var raw = OriginalPixels.Generate(source.Width, source.Height, 8);
        for (var y = 0; y < source.Height; y++) raw.AsSpan(y * source.Stride, source.Stride).CopyTo(source.GetRow8(y));
        using var normal = ImageTransforms.Resize(source, 2049, 1, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 2049, 1, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = raw.Length + 2049L * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(raw, OriginalPixels.Bytes(source));
    }

}
