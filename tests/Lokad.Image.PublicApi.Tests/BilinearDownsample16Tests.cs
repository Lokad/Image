using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class BilinearDownsample16Tests
{
    [Theory]
    [InlineData(67, 13, 40, 7)]
    [InlineData(17, 17, 8, 8)]
    [InlineData(40, 20, 16, 8)]
    [InlineData(65, 17, 33, 9)]
    public void CachedStripsKeepOriginalSamplesOwners(int sourceWidth, int sourceHeight, int width, int height)
    {
        using var source = OriginalPixels.Buffer(sourceWidth, sourceHeight, 16);
        source.GetRow16(0)[3] = 0;
        var input = OriginalPixels.Bytes(source);
        var outputBytes = (long)width * height * 8;
        var planBytes = (long)width * 8 * (sizeof(double) + sizeof(int)) + (long)width * sizeof(int) + 192;
        var scaleX = (double)sourceWidth / width;
        var scaleY = (double)sourceHeight / height;

        var limits = new ImageLimits { MaxWorkingBytes = input.Length + outputBytes + planBytes + 4096};
        using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
            limits with { MaxWorkingBytes = input.Length + outputBytes + 4096 });
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear, limits);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear, limits);
        var bytes = OriginalPixels.Bytes(expected);
        Assert.Equal(bytes, OriginalPixels.Bytes(first));
        Assert.Equal(bytes, OriginalPixels.Bytes(second));
        first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(bytes, OriginalPixels.Bytes(second));
        Assert.Equal(input, OriginalPixels.Bytes(source));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
            limits, new CancellationToken(true)));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void UncertainHalvesAndTinyAlphaUnwindBeforeExactScratch(bool tiny)
    {
        using var source = ImageBuffer.Create(8, 8, 16);
        for (var y = 0; y < 8; y++)
        {
            var row = source.GetRow16(y);
            for (var x = 0; x < 8; x++)
            {
                row[x * 4] = (ushort)(x % 2 == 0 ? 65535 : 0);
                row[x * 4 + 1] = (ushort)(x % 2 == 0 ? 42 : 43);
                row[x * 4 + 2] = 54321;
                row[x * 4 + 3] = tiny ? (ushort)(x % 2) : (ushort)65535;
            }
        }
        var input = OriginalPixels.Bytes(source);
        using var expected = ImageTransforms.Resize(source, 4, 4, ResizeFilter.Bilinear,
            new ImageLimits { MaxWorkingBytes = input.Length + 4 * 4 * 8 + 4096 });
        using var actual = ImageTransforms.Resize(source, 4, 4, ResizeFilter.Bilinear);
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
        Assert.Equal(input, OriginalPixels.Bytes(source));
    }
}
