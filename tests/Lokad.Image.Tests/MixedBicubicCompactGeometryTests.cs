using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class MixedBicubicCompactGeometryTests
{
    [Theory]
    [InlineData(2, 5, 65, 8)] [InlineData(3, 7, 65, 16)]
    [InlineData(35, 71, 65, 8)] [InlineData(35, 71, 65, 16)]
    [InlineData(513, 1027, 129, 8)] [InlineData(513, 1027, 129, 16)]
    [InlineData(67, 139, 107, 16)]
    public void EveryPreparedFieldMatchesOriginalArithmeticAndOnlyTheSuffixIsBorrowed(
        int sourceWidth, int width, int height, int depth)
    {
        using var source = ImageBuffer.Create(sourceWidth, height * 2 + 1, depth);
        using var result = ImageBuffer.Create(width, height, depth);
        result.Pixels.Fill(0xa5);
        var geometry = MixedBicubicGeometry.Prepare(source, result, 8, default);
        var bytes = MixedBicubicGeometry.StorageBytes(width);
        Assert.Equal(width * 36L, bytes);
        Assert.Equal((result.ByteLength - bytes) / result.Stride, geometry.SafeRows);
        Assert.All(result.Pixels[..(int)(result.ByteLength - bytes)].ToArray(), b => Assert.Equal(0xa5, b));
        Span<double> weights = stackalloc double[6]; Span<int> samples = stackalloc int[6];
        for (var x = 0; x < width; x++)
        {
            weights.Fill(123); samples.Fill(-123);
            geometry.Read(x, weights, samples, out var center, out var first, out var count, out var normalizer);
            var expectedCenter = (x + .5) * sourceWidth / width - .5;
            var expectedFirst = (int)Math.Ceiling(expectedCenter - 2);
            var expectedCount = (int)Math.Floor(expectedCenter + 2) - expectedFirst + 1;
            Assert.Equal(Bits(expectedCenter), Bits(center));
            Assert.Equal(expectedFirst, first); Assert.Equal(expectedCount, count);
            double expectedNormalizer = 0;
            for (var t = 0; t < expectedCount; t++)
            {
                var w = ImageTransforms.Kernel(expectedFirst + t - expectedCenter, ResizeFilter.Bicubic);
                expectedNormalizer += w;
                Assert.Equal(Bits(w), Bits(weights[t]));
                Assert.Equal(Math.Clamp(expectedFirst + t, 0, sourceWidth - 1), samples[t]);
            }
            Assert.Equal(Bits(expectedNormalizer), Bits(normalizer));
            Assert.Equal(123, weights[count]); Assert.Equal(-123, samples[count]);
        }
    }

    [Fact]
    public void CancellationKeepsUnpublishedStorageUntouchedAndUnavailableLayoutsDecline()
    {
        using var source = ImageBuffer.Create(2, 131, 16);
        using var result = ImageBuffer.Create(5, 65, 16); result.Pixels.Fill(0xa5);
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        var error = Assert.ThrowsAny<OperationCanceledException>(() => PrepareCanceled(source, result, canceled.Token));
        Assert.Equal(canceled.Token, error.CancellationToken);
        Assert.All(result.Pixels.ToArray(), b => Assert.Equal(0xa5, b));
        using var tiny = ImageBuffer.Create(5, 1, 16);
        Assert.Equal(0, MixedBicubicGeometry.Prepare(source, tiny, 8, default).SafeRows);
        Assert.Equal(0, MixedBicubicGeometry.Prepare(source, source, 8, default).SafeRows);
    }

    private static void PrepareCanceled(ImageBuffer source, ImageBuffer result, CancellationToken token)
        => _ = MixedBicubicGeometry.Prepare(source, result, 8, token).SafeRows;
    private static long Bits(double value) => BitConverter.DoubleToInt64Bits(value);
}
