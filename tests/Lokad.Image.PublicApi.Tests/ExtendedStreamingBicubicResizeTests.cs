using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class ExtendedStreamingBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 8L * 4 * 4 + scratch};
        using var first = ImageTransforms.Resize(source, 8, 4, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 8, 4, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("5ebc1d60ad1c4bcaeeb105d2f5f83285b0dbac88ebbc83cb2f04732ee134b4bc",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 8, 4, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 8, 4, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 8, 4, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(151, 73, 2, 1, false)] [InlineData(152, 73, 2, 1, false)]
    [InlineData(149, 75, 2, 1, false)] [InlineData(153, 75, 3, 1, false)]
    [InlineData(151, 73, 2, 1, true)] [InlineData(152, 73, 2, 1, true)]
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
        using var source = ImageBuffer.Create(131, 65, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new byte[] { 255, 255, 255, (byte)(x < 65 ? 1 : 0) }.CopyTo(source.GetRow8(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        const long peak = (131 * 65 + 2) * 4 + 4096;
        var limits = new ImageLimits { MaxWorkingBytes = peak };
        using var output = ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 131, 65, 8, 2, 1, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
    }
}
