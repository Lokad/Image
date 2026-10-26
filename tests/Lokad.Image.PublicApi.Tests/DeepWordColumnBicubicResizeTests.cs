using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class DeepWordColumnBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 32L * 16 * 8 + scratch};
        using var first = ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("56948d8eacf081102f915276cdbd5daac3b2414f07ac93e3cf9f91fac5e5f053",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 32, 16, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
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
    public void RelativePackedOffsetsKeepAbsoluteCoordinatesAboveUInt16()
    {
        using var source = ImageBuffer.Create(65537, 4, 16, new ImageLimits { MaxDimension = 70000 });
        var raw = OriginalPixels.Generate(source.Width, source.Height, 16);
        for (var y = 0; y < source.Height; y++) System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ushort>(raw.AsSpan(y * source.Stride, source.Stride)).CopyTo(source.GetRow16(y));
        using var normal = ImageTransforms.Resize(source, 4097, 1, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 4097, 1, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = raw.Length + 4097L * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(raw, OriginalPixels.Bytes(source));
    }

}
