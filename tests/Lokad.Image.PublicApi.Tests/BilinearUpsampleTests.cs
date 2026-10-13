using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class BilinearUpsampleTests
{
    [Theory]
    [InlineData(8, 1, 5, 3, 17)]
    [InlineData(16, 1, 5, 3, 17)]
    [InlineData(8, 3, 9, 7, 19)]
    [InlineData(16, 3, 9, 7, 19)]
    [InlineData(8, 67, 13, 135, 27)]
    [InlineData(16, 67, 13, 135, 27)]
    [InlineData(8, 513, 9, 1027, 19)]
    [InlineData(16, 513, 9, 1027, 19)]
    public void TwoTapEdgesAndPreparedBudgetFallbacksPreserveEverySample(
        int depth, int sourceWidth, int sourceHeight, int width, int height)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        var input = OriginalPixels.Bytes(source);
        var outputBytes = (long)width * height * 4 * (depth / 8);
        var planBytes = width * 8L * 12 + width * 4L + 192;
        var basePeak = input.Length + outputBytes + planBytes + 4096;
        using var direct = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
            new ImageLimits { MaxWorkingBytes = input.Length + outputBytes + 4096 });
        var expected = OriginalPixels.Bytes(direct);
        foreach (var peak in new[] { basePeak, basePeak - planBytes })
        {
            using var actual = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
                new ImageLimits { MaxWorkingBytes = peak });
            Assert.Equal(expected, OriginalPixels.Bytes(actual));
            if (depth == 8) actual.GetRow8(0)[0] ^= 255;
            else actual.GetRow16(0)[0] ^= 65535;
            Assert.Equal(expected, OriginalPixels.Bytes(direct));
            Assert.Equal(input, OriginalPixels.Bytes(source));
        }
        // Filtering belongs to this call, so a subsequent input edit must
        // be read even while a previous independent output remains alive.
        if (depth == 8) source.GetRow8(sourceHeight - 1)[0] ^= 255;
        else source.GetRow16(sourceHeight - 1)[0] ^= 65535;
        using var changed = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear);
        using var changedDirect = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
            new ImageLimits { MaxWorkingBytes = input.Length + outputBytes + 4096 });
        Assert.Equal(OriginalPixels.Bytes(changedDirect), OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(direct));
    }
    [Fact]
    public void FullWordPremultiplicationDoesNotSignExtendAndRetainsTinyAlpha()
    {
        using var source = ImageBuffer.Create(2, 2, 16);
        new ushort[] { 65535, 65534, 32769, 65535, 60001, 50003, 40007, 65535 }.CopyTo(source.GetRow16(0));
        new ushort[] { 65533, 35009, 65535, 1, 17, 29993, 59981, 0 }.CopyTo(source.GetRow16(1));
        var unchanged = OriginalPixels.Bytes(source);
        using var actual = ImageTransforms.Resize(source, 9, 7, ResizeFilter.Bilinear);
        using var direct = ImageTransforms.Resize(source, 9, 7, ResizeFilter.Bilinear,
            new ImageLimits { MaxWorkingBytes = unchanged.Length + 9 * 7 * 8 + 4096 });
        Assert.Equal(OriginalPixels.Bytes(direct), OriginalPixels.Bytes(actual));
        Assert.Equal(unchanged, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void FullWordHorizontalRowsRespectExactCacheAdmissionAndRelease()
    {
        using var source = OriginalPixels.Buffer(3, 11, 16);
        var unchanged = OriginalPixels.Bytes(source);
        const int width = 7, height = 23;
        const long outputBytes = width * height * 8;
        const long planBytes = width * 8 * 12 + width * 4 + 192;
        const long cacheBytes = width * 16 * 8 + 64;
        var basePeak = unchanged.Length + outputBytes + planBytes + 4096;
        using var direct = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
            new ImageLimits { MaxWorkingBytes = unchanged.Length + outputBytes + 4096 });
        var expected = OriginalPixels.Bytes(direct);
        foreach (var peak in new[] { basePeak, basePeak + cacheBytes - 1, basePeak + cacheBytes })
        {
            using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
                new ImageLimits { MaxWorkingBytes = peak });
            using var held = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
                new ImageLimits { MaxWorkingBytes = peak });
            Assert.Equal(expected, OriginalPixels.Bytes(first));
            first.GetRow16(0)[0] ^= 65535;
            Assert.Equal(expected, OriginalPixels.Bytes(held));
            Assert.Equal(unchanged, OriginalPixels.Bytes(source));
        }
    }
}
