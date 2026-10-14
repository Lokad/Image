using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PackedStreamingBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 4L * 2 * 514 * 519 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 4L * 2 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 4, 2, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 4, 2, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("fec514ef771042b0df252fa68e6dc3503607bb9f81683b1a0719632efd0cfc17",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 4, 2, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 4, 2, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 4, 2, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 4, 2, ResizeFilter.Bicubic);
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
        const long work = 4L * 9 * 514 * 117 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 4L * 9 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 4, 9, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 4, 9, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("df49ddc41185b3feaa2a0fc283fb3cd2432e463aa4aa7294a041cd76036effd1",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 4, 9, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 4, 9, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 4, 9, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 4, 9, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(9, false)] [InlineData(17, false)] [InlineData(33, false)]
    [InlineData(9, true)] [InlineData(17, true)]
    public void BlockBoundariesAndTailsKeepFullPrecision(int height, bool tiny)
    {
        using var source = OriginalPixels.Buffer(271, 133, 8);
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
        using var source = ImageBuffer.Create(271, 129, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new byte[] { 255, 255, 255, (byte)(x < 135 ? 1 : 0) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 2L * 9 * 4 + 4096 };
        using var output = ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 271, 129, 8, 2, 9, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }));
    }

    [Theory]
    [InlineData(271, 133, 2, 1, false)] [InlineData(272, 133, 2, 1, false)]
    [InlineData(269, 135, 2, 1, false)] [InlineData(273, 135, 3, 1, false)]
    [InlineData(271, 133, 2, 1, true)] [InlineData(272, 133, 2, 1, true)]
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
        using var source = ImageBuffer.Create(261, 129, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new byte[] { 255, 255, 255, (byte)(x < 130 ? 1 : 0) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        const long peak = (261 * 129 + 2) * 4 + 4096;
        var limits = new ImageLimits { MaxWorkingBytes = peak };
        using var output = ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 261, 129, 8, 2, 1, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
    }

    [Fact]
    public void PackedCoefficientUncertaintyKeepsOriginalRoundingAtCertifiedHalves()
    {
        using var source = ImageBuffer.Create(401, 131, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow8(y).Slice(x * 4, 4);
            pixel[0] = (byte)(x < 200 ? 1 : 2);
            pixel[1] = (byte)(x < 200 ? 3 : 4);
            pixel[2] = (byte)(x < 200 ? 5 : 6);
            pixel[3] = (byte)(x == 200 ? 0 : 255);
        }
        var input = OriginalPixels.Bytes(source);
        Assert.True(ExactResizeReference.SampleIsHalf(input, 401, 131, 8, 3, 1, 1, 0, 0, ResizeFilter.Bicubic));
        using var normal = ImageTransforms.Resize(source, 3, 1, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 3, 1, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + 3L * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }
}
