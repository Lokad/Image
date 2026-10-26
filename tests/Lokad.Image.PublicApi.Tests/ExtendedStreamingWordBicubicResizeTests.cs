using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class ExtendedStreamingWordBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 8L * 4 * 8 + scratch};
        using var first = ImageTransforms.Resize(source, 8, 4, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 8, 4, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("a80b76861bacb9bf12c31dcb88214de854de34064cafe4018538f78cd4a9a7a6",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 8, 4, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
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
    [Fact]
    public void ExtendedSupportUnwindsBeforeExactTinyAlphaScratch()
    {
        using var source = ImageBuffer.Create(131, 65, 16);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new ushort[] { 65535, 65535, 65535, (ushort)(x < 65 ? 1 : 0) }.CopyTo(source.GetRow16(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        const long peak = (131 * 65 + 2) * 8 + 4096;
        var limits = new ImageLimits { MaxWorkingBytes = peak };
        using var output = ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 131, 65, 16, 2, 1, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
    }
}
