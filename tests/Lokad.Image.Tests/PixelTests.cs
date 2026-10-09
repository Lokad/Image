using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PixelTests
{
    [Fact]
    public void CrcHasIndependentCheckVector()
        => Assert.Equal(0xCBF43926u, Crc32.Compute("123456789"u8));

    [Theory]
    [InlineData(1, new byte[] { 10, 10, 10, 10 }, new byte[] { 10, 20, 30, 40 })]
    [InlineData(2, new byte[] { 9, 18, 27, 36 }, new byte[] { 10, 20, 30, 40 })]
    [InlineData(3, new byte[] { 10, 14, 19, 23 }, new byte[] { 10, 20, 30, 40 })]
    [InlineData(4, new byte[] { 9, 10, 10, 10 }, new byte[] { 10, 20, 30, 40 })]
    public void PngFiltersHaveHandComputedExpectedRows(int filter, byte[] row, byte[] expected)
    {
        PngCodec.Unfilter(row, new byte[] { 1, 2, 3, 4 }, 1, filter);
        Assert.Equal(expected, row);
    }

    [Fact]
    public void MemoryAdmissionIncludesSourceAndDestination()
    {
        using var image = ImageBuffer.Create(2, 2, 16);
        Assert.Throws<ImageLimitException>(() => ImageTransforms.FlipHorizontal(image,
            new ImageLimits { MaxWorkingBytes = 63 }));
    }
}
