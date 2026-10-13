using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class BilinearUpsampleTests
{
    [Theory]
    [InlineData(1, 5, 3, 17)]
    [InlineData(3, 9, 7, 19)]
    [InlineData(67, 13, 135, 27)]
    [InlineData(513, 9, 1027, 19)]
    public void TwoTapEdgesAndPreparedBudgetFallbacksPreserveEverySample(
        int sourceWidth, int sourceHeight, int width, int height)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 8);
        var input = OriginalPixels.Bytes(source);
        var outputBytes = (long)width * height * 4;
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
            actual.GetRow8(0)[0] ^= 255;
            Assert.Equal(expected, OriginalPixels.Bytes(direct));
            Assert.Equal(input, OriginalPixels.Bytes(source));
        }
        // Filtering belongs to this call, so a subsequent input edit must
        // be read even while a previous independent output remains alive.
        source.GetRow8(sourceHeight - 1)[0] ^= 255;
        using var changed = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear);
        using var changedDirect = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
            new ImageLimits { MaxWorkingBytes = input.Length + outputBytes + 4096 });
        Assert.Equal(OriginalPixels.Bytes(changedDirect), OriginalPixels.Bytes(changed));
        Assert.Equal(expected, OriginalPixels.Bytes(direct));
    }
}
