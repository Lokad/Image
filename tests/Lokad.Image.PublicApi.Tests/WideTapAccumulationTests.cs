using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WideTapAccumulationTests
{
    [Theory]
    [InlineData(8, 12, 8)]
    [InlineData(16, 12, 8)]
    [InlineData(8, 11, 7)]
    [InlineData(16, 11, 7)]
    public void WiderTapSumsKeepRationalPixelsAcrossUnequalSupportsAndOddTails(int depth, int width, int height)
    {
        using var source = OriginalPixels.Buffer(53, 37, depth);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var phase = (y * source.Width + x) % 13;
            if (phase > 2) continue;
            if (depth == 8) source.GetRow8(y)[x * 4 + 3] = phase == 0 ? (byte)0 : phase == 1 ? (byte)1 : (byte)255;
            else source.GetRow16(y)[x * 4 + 3] = phase == 0 ? (ushort)0 : phase == 1 ? (ushort)1 : (ushort)65535;
        }
        var input = OriginalPixels.Bytes(source);
        var expected = ExactResizeReference.Resize(input, source.Width, source.Height, depth, width, height, ResizeFilter.Bicubic);
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bicubic);
        Assert.Equal(expected, OriginalPixels.Bytes(first)); Assert.Equal(expected, OriginalPixels.Bytes(second));
        first.Dispose(); source.Dispose();
        Assert.Equal(expected, OriginalPixels.Bytes(second));
    }
}
