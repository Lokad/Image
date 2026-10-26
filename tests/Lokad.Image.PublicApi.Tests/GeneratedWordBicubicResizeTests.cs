using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class GeneratedWordBicubicResizeTests
{
    [Theory]
    [InlineData(4095)] [InlineData(4096)]
    public void WiderStripsKeepFrozenPixelsAdmissionAndIndependentOwners(int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16);
        source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 2L * 2 * 8 + scratch};
        using var first = ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("1c64bfdf6068fbbd9b564e1e0f9ef32110cd726eb66d814e67af828dd5df5a78",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
        using var changed = ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 2, 2, ResizeFilter.Bicubic);
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
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 2L * 9 * 8 + scratch};
        using var first = ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // The same authored raster and frozen direct output as the wide-support
        // benchmark; the independent rational reference is exact on this input.
        Assert.Equal("8cec67006c7c5b11584dfd0d0f69b4bdb5678fc65269445b011d386aa51f6f45",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        source.GetRow16(258)[512 * 4 + 3] ^= 65535;
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
        using var source = OriginalPixels.Buffer(551, 133, 16);
        if (tiny)
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < source.Width; x++)
                source.GetRow16(y)[x * 4 + 3] = (ushort)((x + y) % 7 == 0 ? 65535 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, 2, height, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 2, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + 2L * height * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void BlockedTinyAlphaUnwindsBeforeExactScratch()
    {
        using var source = ImageBuffer.Create(551, 129, 16);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new ushort[] { 65535, 65535, 65535, (ushort)(x < 275 ? 1 : 0) }.CopyTo(source.GetRow16(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 2L * 9 * 8 + 4096 };
        using var output = ImageTransforms.Resize(source, 2, 9, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 551, 129, 16, 2, 9, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
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
        using var source = ImageBuffer.Create(551, 129, 16);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            new ushort[] { 65535, 65535, 65535, (ushort)(x < 275 ? 1 : 0) }.CopyTo(source.GetRow16(y).Slice(x * 4, 4));
        var input = OriginalPixels.Bytes(source);
        const long peak = (551 * 129 + 2) * 8 + 4096;
        var limits = new ImageLimits { MaxWorkingBytes = peak };
        using var output = ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic, limits);
        Assert.Equal(ExactResizeReference.Resize(input, 551, 129, 16, 2, 1, ResizeFilter.Bicubic), OriginalPixels.Bytes(output));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 2, 1, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
    }

    [Fact]
    public void PackedCoefficientUncertaintyKeepsOriginalRoundingAtCertifiedHalves()
    {
        using var source = ImageBuffer.Create(1001, 131, 16);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var pixel = source.GetRow16(y).Slice(x * 4, 4);
            pixel[0] = (ushort)(x < 500 ? 1 : 2);
            pixel[1] = (ushort)(x < 500 ? 3 : 4);
            pixel[2] = (ushort)(x < 500 ? 5 : 6);
            pixel[3] = (ushort)(x == 500 ? 0 : 65535);
        }
        var input = OriginalPixels.Bytes(source);
        Assert.True(ExactResizeReference.SampleIsHalf(input, 1001, 131, 16, 3, 1, 1, 0, 0, ResizeFilter.Bicubic));
        using var normal = ImageTransforms.Resize(source, 3, 1, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 3, 1, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = input.Length + 3L * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }
    [Fact]
    public void GeneratedCoordinatesKeepAbsoluteSourceOffsetsAboveUInt16()
    {
        using var source = ImageBuffer.Create(65537, 4, 16, new ImageLimits { MaxDimension = 70000 });
        var raw = OriginalPixels.Generate(source.Width, source.Height, 16);
        for (var y = 0; y < source.Height; y++)
            System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ushort>(raw.AsSpan(y * source.Stride, source.Stride)).CopyTo(source.GetRow16(y));
        using var normal = ImageTransforms.Resize(source, 65, 1, ResizeFilter.Bicubic);
        using var bounded = ImageTransforms.Resize(source, 65, 1, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = raw.Length + 65L * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(bounded));
        Assert.Equal(raw, OriginalPixels.Bytes(source));
    }
}
