using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class BilinearByteFactorizationTests
{
    [Fact]
    public void ReusedByteRowsFitExistingGeometryAdmissionAndReleaseEveryOwner()
    {
        const int sourceWidth = 5, sourceHeight = 9, width = 11, height = 29;
        const long sourceBytes = sourceWidth * sourceHeight * 4;
        const long outputBytes = width * height * 4;
        const long planBytes = width * 100 + 192;
        using var root = new TestMemoryBudget(sourceBytes + outputBytes + planBytes);
        var limits = new ImageLimits { MemoryBudget = root,
            MaxWorkingBytes = sourceBytes + outputBytes + planBytes + 4096 };
        using var source = ImageBuffer.Create(sourceWidth, sourceHeight, 8, limits);
        for (var y = 0; y < sourceHeight; y++)
        for (var x = 0; x < sourceWidth; x++)
        {
            var row = source.GetRow8(y); var at = x * 4;
            row[at] = (byte)(x * 47 + y * 17); row[at + 1] = (byte)(x * 19 + y * 29);
            row[at + 2] = (byte)(x * 31 + y * 11); row[at + 3] = (byte)(64 + (x * 37 + y * 13) % 192);
        }
        var original = OriginalPixels.Bytes(source);
        using var direct = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear,
            new ImageLimits { MaxWorkingBytes = sourceBytes + outputBytes + 4096 });
        var expected = OriginalPixels.Bytes(direct);
        root.RefuseAtGrowth = root.GrowthAttempts + 2; // Result succeeds, geometry refuses.
        var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(source, width, height,
            ResizeFilter.Bilinear, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope);
        Assert.Equal(sourceBytes, root.Bytes); Assert.Equal(1, root.Refusals);
        root.RefuseAtGrowth = 0;
        using var first = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(sourceBytes + outputBytes + planBytes, root.Peak);
        Assert.Equal(sourceBytes + outputBytes, root.Bytes);
        first.GetRow8(0)[0] ^= 255; first.Dispose();
        Assert.Equal(sourceBytes, root.Bytes);
        using var second = ImageTransforms.Resize(source, width, height, ResizeFilter.Bilinear, limits);
        Assert.Equal(expected, OriginalPixels.Bytes(second)); Assert.Equal(original, OriginalPixels.Bytes(source));
        source.Dispose(); Assert.Equal(outputBytes, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

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
