using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class ColumnBicubicResizeTests
{
    [Theory]
    [InlineData(65, 35, 29, 16)]
    [InlineData(72, 35, 32, 16)]
    [InlineData(65, 36, 29, 16)]
    [InlineData(17, 19, 8, 9)]
    public void WiderStripsKeepCacheWrapOffsetsAndScaleBoundaryPixels(int sourceWidth, int sourceHeight, int width, int height)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 8);
        source.GetRow8(0)[3] = 0;
        var input = OriginalPixels.Bytes(source);
        var bytes = input.Length + (long)width * height * 4;
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = bytes + 4095 });
        using var actual = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic,
            new ImageLimits { MaxWorkingBytes = bytes + 4096 });
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(8, 2047)] [InlineData(8, 2048)] [InlineData(8, 4095)] [InlineData(8, 4096)]
    [InlineData(16, 2047)] [InlineData(16, 2048)] [InlineData(16, 4095)] [InlineData(16, 4096)]
    public void BoundedWorkspaceKeepsFrozenPixelsAndIndependentOwners(int depth, int scratch)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth);
        if (depth == 8) source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        else source.GetRow16(0)[3] = source.GetRow16(0)[513 * 4 - 1] = 0;
        var input = OriginalPixels.Bytes(source);
        var limits = new ImageLimits { MaxWorkingBytes = input.Length + 256L * 129 * 4 * (depth / 8) + scratch};
        using var first = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits);
        using var second = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits);
        var expected = OriginalPixels.Bytes(first);
        // Original independently authored performance raster; frozen original
        // direct output also matches the independent rational reference exactly.
        Assert.Equal(depth == 8 ? "656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8"
            : "2676207be4d1a7c9109061fc3c8eb5a0d6b8d10a97bbbc611fbebda3330ffd9f",
            Convert.ToHexString(SHA256.HashData(expected)).ToLowerInvariant());
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic,
            limits, new CancellationToken(true)));
        if (depth == 8) source.GetRow8(258)[512 * 4 + 3] ^= 255;
        else source.GetRow16(258)[512 * 4 + 3] ^= 65535;
        using var changed = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic, limits);
        using var normal = ImageTransforms.Resize(source, 256, 129, ResizeFilter.Bicubic);
        Assert.Equal(OriginalPixels.Bytes(normal), OriginalPixels.Bytes(changed));
        Assert.NotEqual(expected, OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }

    [Theory]
    [InlineData(33, 17, 16, 8, false)] [InlineData(37, 19, 15, 7, false)]
    [InlineData(19, 17, 9, 35, false)] [InlineData(17, 19, 35, 9, false)]
    [InlineData(17, 9, 35, 19, false)] [InlineData(61, 39, 18, 11, false)]
    [InlineData(35, 21, 17, 9, false)] [InlineData(51, 35, 23, 16, false)]
    [InlineData(33, 17, 16, 8, true)] [InlineData(17, 11, 35, 23, true)]
    [InlineData(17, 11, 9, 5, true)] [InlineData(19, 17, 9, 35, true)]
    public void ColumnsAndWholeRowFallbackKeepNegativeTinyAndHiddenAlphaExact(
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
    [Theory]
    [InlineData(33, 17, 16, 8, false)] [InlineData(35, 21, 17, 9, false)]
    [InlineData(51, 35, 23, 16, false)] [InlineData(33, 17, 16, 8, true)]
    [InlineData(17, 11, 9, 5, true)]
    public void WordStripsKeepUnsignedProductsOddTailsAndTinyAlphaExact(
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

}
