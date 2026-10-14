using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PackedColumnBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionWorkAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        const long work = 64L * 32 * 34 * 34 * 4;
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 64L * 32 * 4 + scratch, MaxWork = work };
        using var first = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("e2340f662ce94418bed4e7be5c65ac4c73fb7a705b047af06784e42b37c267c6",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 64, 32, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(129, 67, 15, 8, false)] [InlineData(137, 69, 16, 8, false)]
    [InlineData(131, 67, 15, 8, false)] [InlineData(139, 71, 16, 8, false)]
    [InlineData(129, 67, 15, 8, true)] [InlineData(137, 69, 16, 8, true)]
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
        using var normal = ImageTransforms.Resize(source, 8193, 1, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 8193, 1, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = raw.Length + 8193L * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(raw, OriginalPixels.Bytes(source));
    }

}
