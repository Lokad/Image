using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class StreamingWordBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 16L * 8 * 8 + scratch};
        using var first = ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("67fc5ce26cfe8812561835d03f375e5d036e2d56bf513638d5c21fb4a5dac1a8",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
        using var changed = ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 16, 8, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void BlockedRowsKeepFrozenPixelsAdmissionAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 8L * 17 * 8 + scratch};
        using var first = ImageTransforms.Resize(source, 8, 17, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 8, 17, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("9f1aaa6ef72a650982022d0a36563789f9ae50d79a094f97141af8ed50051b6c",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 8, 17, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
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
        using var source = OriginalPixels.Buffer(271, 133, 16);
        if (tiny)
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
                source.GetRow16(y)[x * 4 + 3] = (ushort)((x + y) % 7 == 0 ? 65535 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, 4, height, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 4, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + 4L * height * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void BlockedTinyAlphaUnwindsBeforeExactScratch()
    {
        using var source = ImageBuffer.Create(271, 129, 16);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new ushort[] { 65535, 65535, 65535, (ushort)(x < 135 ? 1 : 0) }.CopyTo(source.GetRow16(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 4L * 17 * 8 + 4096 };
        using var output = ImageTransforms.Resize(source, 4, 17, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 271, 129, 16, 4, 17, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
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
        using var normal = ImageTransforms.Resize(source, 2049, 1, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 2049, 1, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = raw.Length + 2049L * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(raw, OriginalPixels.Bytes(source));
    }

}
