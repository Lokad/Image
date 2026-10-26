using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class DeepColumnBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 32L * 16 * 4 + scratch};
        using var first = ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("3ec9c663c4051ce2f14b6a84acf4f21cf4c495c4017200f50f469de1f4b6132d",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow8(258)[512 * 4 + 3] ^= 255;
        using var changed = ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(121, 61, 7, 4, false)] [InlineData(129, 65, 8, 4, false)]
    [InlineData(123, 63, 7, 4, false)] [InlineData(131, 67, 8, 4, false)]
    [InlineData(121, 61, 7, 4, true)] [InlineData(129, 65, 8, 4, true)]
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
    public void AlternatingReductionsKeepExactRationalPixelsWithNegativeLobesAndTinyAlpha()
    {
        foreach (var tiny in new[] { false, true })
        {
            using var source = ImageBuffer.Create(121, 61, 8);
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
            {
                var pixel = source.GetRow8(y).Slice(x * 4, 4);
                pixel[0] = (byte)(((x + y) & 1) == 0 ? 255 : 0);
                pixel[1] = (byte)((x * 73 + y * 19) & 255);
                pixel[2] = (byte)(((x * 3 + y) & 3) == 0 ? 254 : 1);
                pixel[3] = tiny ? (byte)((x + y) % 7 == 0 ? 255 : (x + y) % 3 == 0 ? 1 : 0)
                    : (byte)((x + y) % 4 == 0 ? 0 : (x + y) % 4 == 1 ? 1 : 255);
            }
            var input = OriginalPixels.Bytes(source);
            var expected = ExactResizeReference.Resize(input, 121, 61, 8, 7, 4, ResizeFilter.Bicubic);
            using var actual = ImageTransforms.Resize(source, 7, 4, ResizeFilter.Bicubic,
                new ImageLimits { MaxWorkingBytes = input.Length + 7 * 4 * 4 + 4096 });
            Assert.Equal(expected, OriginalPixels.Bytes(actual)); Assert.Equal(input, OriginalPixels.Bytes(source));
        }
    }

    [Fact]
    public void RelativePackedOffsetsKeepAbsoluteCoordinatesAboveUInt16()
    {
        using var source = ImageBuffer.Create(65537, 4, 8, new ImageLimits { MaxDimension = 70000 });
        var raw = OriginalPixels.Generate(source.Width, source.Height, 8);
        for (var y = 0; y < source.Height; y++) raw.AsSpan(y * source.Stride, source.Stride).CopyTo(source.GetRow8(y));
        using var normal = ImageTransforms.Resize(source, 4097, 1, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 4097, 1, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = raw.Length + 4097L * 4 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(raw, OriginalPixels.Bytes(source));
    }

}
