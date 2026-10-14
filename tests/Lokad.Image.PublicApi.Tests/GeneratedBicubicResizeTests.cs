using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class GeneratedBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 2L * 2 * 1027 * 519 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 2L * 2 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("367dd1cd21ad0c23e2833780483256037d2368247295f14a099e11c126b6c858",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic);
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
        const long work = 2L * 9 * 1027 * 117 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 2L * 9 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("a9178bee6d91b18ee8bf5560450f3b2ae23a682c887ea832e8fcafb19a8e6442",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(9, false)] [InlineData(17, false)] [InlineData(33, false)]
    [InlineData(9, true)] [InlineData(17, true)]
    public void BlockBoundariesAndTailsKeepFullPrecision(int height, bool tiny)
    {
        using var source = OriginalPixels.Buffer(551, 133, 8);
        if (tiny)
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
                source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 7 == 0 ? 255 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, 2, height, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 2, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + 2L * height * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void BlockedTinyAlphaUnwindsBeforeExactScratch()
    {
        using var source = ImageBuffer.Create(551, 129, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new byte[] { 255, 255, 255, (byte)(x < 275 ? 1 : 0) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 2L * 9 * 4 + 4096 };
        using var output = ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 551, 129, 8, 2, 9, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }));
    }

    [Theory]
    [InlineData(551, 133, 2, 1, false)] [InlineData(553, 135, 2, 1, false)]
    [InlineData(547, 137, 2, 1, false)] [InlineData(557, 135, 3, 1, false)]
    [InlineData(551, 133, 2, 1, true)] [InlineData(553, 135, 2, 1, true)]
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
    public void ExtendedSupportUnwindsBeforeExactTinyAlphaScratch()
    {
        using var source = ImageBuffer.Create(551, 129, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new byte[] { 255, 255, 255, (byte)(x < 275 ? 1 : 0) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        const long peak = (551 * 129 + 2) * 4 + 4096;
        var limits = new ImageLimits { MaxWorkingBytes = peak };
        using var output = ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 551, 129, 8, 2, 1, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
    }

    [Fact]
    public void GeneratedCoefficientUncertaintyKeepsOriginalRoundingAtCertifiedHalves()
    {
        using var source = ImageBuffer.Create(1001, 131, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow8(y).Slice(x * 4, 4);
            pixel[0] = (byte)(x < 500 ? 1 : 2);
            pixel[1] = (byte)(x < 500 ? 3 : 4);
            pixel[2] = (byte)(x < 500 ? 5 : 6);
            pixel[3] = (byte)(x == 500 ? 0 : 255);
        }
        var input = OriginalPixels.Bytes(source);
        Assert.True(ExactResizeReference.SampleIsHalf(input, 1001, 131, 8, 3, 1, 1, 0, 0, ResizeFilter.Bicubic));
        using var normal = ImageTransforms.Resize(source, 3, 1, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 3, 1, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + 3L * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }
    [Fact]
    public void GeneratedCoordinatesKeepAbsoluteSourceOffsetsAboveUInt16()
    {
        using var source = ImageBuffer.Create(65537, 4, 8, new ImageLimits { MaxDimension = 70000 });
        var raw = OriginalPixels.Generate(source.Width, source.Height, 8);
        for (var y = 0; y < source.Height; y++)
            raw.AsSpan(y * source.Stride, source.Stride).CopyTo(source.GetRow8(y));
        using var normal = ImageTransforms.Resize(source, 65, 1, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 65, 1, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = raw.Length + 65L * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(raw, OriginalPixels.Bytes(source));
    }
}
