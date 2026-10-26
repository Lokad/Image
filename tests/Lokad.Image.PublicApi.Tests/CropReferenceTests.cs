using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class CropReferenceTests
{
    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void RejectedPartialCopyCannotLeaveStalePublishedSamples(int depth)
    {
        using var source = OriginalPixels.Buffer(2051, 3, depth);
        var original = OriginalPixels.Bytes(source);
        using (var poison = ImageTransforms.Crop(source, 1, 1, 2049, 2))
            for (var row = 0; row < poison.Height; row++)
                if (depth == 8) poison.GetRow8(row).Fill(0xA5);
                else poison.GetRow16(row).Fill(0xA55A);
        using var result = ImageTransforms.Crop(source, 1, 1, 2049, 2);
        var pixelBytes = depth / 8 * 4;
        var actual = OriginalPixels.Bytes(result);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2049; x++)
        for (var b = 0; b < pixelBytes; b++)
            Assert.Equal(original[((y + 1) * 2051 + x + 1) * pixelBytes + b],
                actual[(y * 2049 + x) * pixelBytes + b]);
        Assert.Equal(original, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(8, 4103, 3, 1, 1, 4101, 2)]
    [InlineData(16, 4103, 3, 1, 1, 4101, 2)]
    [InlineData(8, 1025, 2, 0, 0, 1025, 2)]
    [InlineData(16, 1025, 2, 0, 0, 1025, 2)]
    [InlineData(8, 7, 5, 6, 4, 1, 1)]
    [InlineData(16, 7, 5, 6, 4, 1, 1)]
    public void CropMatchesIndependentPlacementAndExactAdmission(int depth, int sourceWidth,
        int sourceHeight, int x, int y, int width, int height)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, depth);
        // Keep visible hidden RGB in a selected zero-alpha pixel at both depths.
        if (depth == 8) source.GetRow8(y)[x * 4 + 3] = 0;
        else source.GetRow16(y)[x * 4 + 3] = 0;
        var original = OriginalPixels.Bytes(source);
        var pixelBytes = depth / 8 * 4;
        var expected = new byte[width * height * pixelBytes];
        for (var dy = 0; dy < height; dy++)
        for (var dx = 0; dx < width; dx++)
        for (var sampleByte = 0; sampleByte < pixelBytes; sampleByte++)
            expected[(dy * width + dx) * pixelBytes + sampleByte] =
                original[((dy + y) * sourceWidth + dx + x) * pixelBytes + sampleByte];
        var peak = original.Length + (long)expected.Length;

        var limits = new ImageLimits { MaxWorkingBytes = peak};
        using var first = ImageTransforms.Crop(source, x, y, width, height, limits);
        using var second = ImageTransforms.Crop(source, x, y, width, height, limits);
        Assert.Equal((width, height, depth, source.Orientation),
            (first.Width, first.Height, first.BitDepth, first.Orientation));
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Crop(source, x, y, width, height,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Crop(source, x, y, width, height,
            limits, new CancellationToken(true)));
        Assert.Equal(original, OriginalPixels.Bytes(source));
    }
}
