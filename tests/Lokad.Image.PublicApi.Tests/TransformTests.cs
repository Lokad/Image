namespace Lokad.Image.PublicApi.Tests;

public sealed class TransformTests
{
    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void ExactTransformsPreserveSamplesAndSourceOwnership(int depth)
    {
        using var image = ImageBuffer.Create(3, 2, depth);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 3; x++)
        {
            if (depth == 8) image.GetRow8(y)[x * 4] = (byte)(1 + x + y * 3);
            else image.GetRow16(y)[x * 4] = (ushort)(1234 + x * 7919 + y * 23456);
        }
        using var flipped = ImageTransforms.FlipHorizontal(image);
        using var rotated = ImageTransforms.Rotate(flipped, QuarterTurn.Clockwise90);
        using var cropped = ImageTransforms.Crop(rotated, 0, 1, 2, 2);
        Assert.Equal(2, cropped.Width); Assert.Equal(2, cropped.Height);
        var expected = depth == 8 ? new[] { 5, 2, 4, 1 } : new[] { 32609, 9153, 24690, 1234 };
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
            Assert.Equal(expected[y * 2 + x], depth == 8 ? cropped.GetRow8(y)[x * 4] : cropped.GetRow16(y)[x * 4]);
        Assert.Equal(depth == 8 ? 1 : 1234, depth == 8 ? image.GetRow8(0)[0] : image.GetRow16(0)[0]);
    }

    [Theory]
    [InlineData(ResizeFilter.Bilinear)]
    [InlineData(ResizeFilter.Bicubic)]
    public void ResizeAvoidsTransparentColorHalos(ResizeFilter filter)
    {
        using var image = ImageBuffer.Create(2, 1, 8);
        new byte[] { 255, 0, 0, 0, 0, 0, 255, 255 }.CopyTo(image.GetRow8(0));
        using var resized = ImageTransforms.Resize(image, 1, 1, filter);
        Assert.Equal(new byte[] { 0, 0, 255, 128 }, resized.GetRow8(0).ToArray());
    }

    [Theory]
    [InlineData(ResizeFilter.Nearest, new byte[] { 0, 0, 100, 100 })]
    [InlineData(ResizeFilter.Bilinear, new byte[] { 0, 25, 75, 100 })]
    // Catmull-Rom's negative lobe overshoots the last value by 7.03125;
    // clipping uses the channel range [0,255], not the source extrema [0,100].
    [InlineData(ResizeFilter.Bicubic, new byte[] { 0, 20, 80, 107 })]
    public void ResizeUsesHandComputedPixelCentersAndKernels(ResizeFilter filter, byte[] expected)
    {
        using var source = ImageBuffer.Create(2, 1, 8);
        new byte[] { 0, 0, 0, 255, 100, 100, 100, 255 }.CopyTo(source.GetRow8(0));
        using var result = ImageTransforms.Resize(source, 4, 1, filter);
        for (var x = 0; x < 4; x++)
            Assert.Equal(new byte[] { expected[x], expected[x], expected[x], 255 }, result.GetRow8(0).Slice(x * 4, 4).ToArray());
    }

    [Fact]
    public void SixteenBitResizePreservesPrecisionAndIdentityHiddenRgb()
    {
        using var source = ImageBuffer.Create(2, 1, 16);
        new ushort[] { 12345, 12345, 12345, 65535, 54321, 54321, 54321, 65535 }.CopyTo(source.GetRow16(0));
        using var result = ImageTransforms.Resize(source, 1, 1, ResizeFilter.Bilinear);
        Assert.Equal(new ushort[] { 33333, 33333, 33333, 65535 }, result.GetRow16(0).ToArray());
        source.GetRow16(0)[3] = 0;
        using var identity = ImageTransforms.Resize(source, 2, 1);
        Assert.Equal(source.GetRow16(0).ToArray(), identity.GetRow16(0).ToArray());
    }

    [Fact]
    public void LimitsCancellationAndDisposalAreObservable()
    {
        Assert.Throws<ImageLimitException>(() => ImageBuffer.Create(int.MaxValue, int.MaxValue, 16));
        Assert.Throws<OperationCanceledException>(() => ImageBuffer.Create(1, 1, 8, cancellationToken: new CancellationToken(true)));
        using var image = ImageBuffer.Create(3, 2, 8);
        Assert.Throws<ArgumentOutOfRangeException>(() => ImageTransforms.Crop(image, 2, 0, 2, 1));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Resize(image, 4, 4, limits: new ImageLimits { MaxWork = 1 }));
        image.Dispose();
        Assert.Throws<ObjectDisposedException>(() => image.GetRow8(0));
    }

    [Fact]
    public void FitPreservesAspectAndCanPreventUpscaling()
    {
        using var source = ImageBuffer.Create(7, 5, 8);
        using var small = ImageTransforms.ResizeToFit(source, 4, 4, true);
        Assert.Equal((4, 3), (small.Width, small.Height));
        using var unchanged = ImageTransforms.ResizeToFit(source, 100, 100, false);
        Assert.Equal((7, 5), (unchanged.Width, unchanged.Height));
    }
}
