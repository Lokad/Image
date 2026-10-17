using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class BilinearByteFactorizationTests
{
    [Fact]
    public void HorizontalProductBoundaryKeepsOriginalOrderPixelsAndOwners()
    {
        using var source = OriginalPixels.Buffer(9, 1, 8);
        for (var x = 0; x < source.Width; x++) source.GetRow8(0)[x * 4 + 3] = (byte)(192 + x * 7);
        var input = OriginalPixels.Bytes(source);
        foreach (var width in new[] { 16512, 16513 })
        {
            // The first width permits exact Int32 horizontal products; the
            // second keeps the original wide arithmetic. Neither is a bench.
            const int height = 3;
            var tight = ImageLimits.Default with { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4096 };
            using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear, tight);
            using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear);
            using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear);
            var pixels = OriginalPixels.Bytes(first);
            Assert.Equal(OriginalPixels.Bytes(expected), pixels); Assert.Equal(pixels, OriginalPixels.Bytes(second));
            first.GetRow8(0)[0] ^= 255;
            Assert.Equal(pixels, OriginalPixels.Bytes(second)); Assert.Equal(input, OriginalPixels.Bytes(source));
        }
    }

    [Fact]
    public void OddTailsClampedEdgesAndTinyAlphaKeepExactOriginalEvaluation()
    {
        using var source = OriginalPixels.Buffer(7, 5, 8);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
            source.GetRow8(y)[x * 4 + 3] = (byte)((x + y) % 7 == 0 ? 255 : (x + y) % 3 == 0 ? 1 : 0);
        var input = OriginalPixels.Bytes(source);
        foreach (var width in new[] { 15, 16, 17 })
        {
            const int height = 11;
            var tight = ImageLimits.Default with { MaxWorkingBytes = input.Length + (long)width * height * 4 + 4096 };
            using var expected = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear, tight);
            using var actual = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear);
            Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
            Assert.Equal(input, OriginalPixels.Bytes(source));
            Assert.Throws<OperationCanceledException>(() => ImageTransforms.Resize(source, width, height,
                ResizeFilter.Bilinear, cancellationToken: new CancellationToken(true)));
        }
    }
}
