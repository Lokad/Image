using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class TransformAdmissionTests
{
    [Fact]
    public void LowerGeometryLimitsConstrainNewResultsAndStillCountTheLargerSource()
    {
        using var source = OriginalPixels.Buffer(3, 2, 8);
        var limits = new ImageLimits { MaxDimension = 1, MaxPixels = 1, MaxWorkingBytes = 28 };
        using var cropped = ImageTransforms.Crop(source, 0, 0, 1, 1, limits);
        Assert.Equal(source.GetRow8(0).Slice(0, 4).ToArray(), cropped.GetRow8(0).ToArray());
        using var resized = ImageTransforms.Resize(source, 1, 1, ResizeFilter.Nearest, limits);
        Assert.Equal(source.GetRow8(1).Slice(4, 4).ToArray(), resized.GetRow8(0).ToArray());
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Crop(source, 0, 0, 1, 1, limits with { MaxWorkingBytes = 27 }));
        Assert.Throws<ImageLimitException>(() => source.Clone(limits));
        using var output = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, output, new PngEncodeOptions { Limits = limits }));
        Assert.Equal(0, output.Length);
    }

    public static IEnumerable<object[]> Profiles()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var operation in new[] { "clone", "crop", "center", "clamp", "hflip", "vflip", "cw", "half", "ccw", "transpose", "transverse", "nearest", "identity" })
            yield return [depth, operation];
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void ExactTransformsAdmitBothBuffersAndChargeAllSamples(int depth, string operation)
    {
        // Cross both fixed copy/map checkpoint sizes and end with partial chunks.
        using var source = OriginalPixels.Buffer(4103, 2, depth);
        var sourcePixels = OriginalPixels.Bytes(source);
        using var expected = Apply(source, operation, ImageLimits.Default, default);
        var resultPixels = OriginalPixels.Bytes(expected);
        var peak = sourcePixels.Length + (long)resultPixels.Length;
        var work = (long)expected.Width * expected.Height * 4;
        var limits = new ImageLimits { MaxWorkingBytes = peak, MaxWork = work };
        using var exact = Apply(source, operation, limits, default);
        Assert.Equal(resultPixels, OriginalPixels.Bytes(exact));
        Assert.Equal((expected.Width, expected.Height, depth), (exact.Width, exact.Height, exact.BitDepth));
        Assert.Throws<ImageLimitException>(() => Apply(source, operation, limits with { MaxWorkingBytes = peak - 1 }, default));
        Assert.Throws<ImageLimitException>(() => Apply(source, operation, limits with { MaxWork = work - 1 }, default));
        Assert.Throws<OperationCanceledException>(() => Apply(source, operation, limits, new CancellationToken(true)));
        Assert.Equal(sourcePixels, OriginalPixels.Bytes(source));
    }

    private static ImageBuffer Apply(ImageBuffer source, string operation, ImageLimits limits, CancellationToken cancellation) => operation switch
    {
        "clone" => source.Clone(limits, cancellation),
        "crop" => ImageTransforms.Crop(source, 1, 0, 4101, 2, limits, cancellation),
        "center" => ImageTransforms.CropCentered(source, 4101, 2, limits, cancellation),
        "clamp" => ImageTransforms.CropClamped(source, int.MaxValue, int.MinValue, 4101, 2, limits, cancellation),
        "hflip" => ImageTransforms.FlipHorizontal(source, limits, cancellation),
        "vflip" => ImageTransforms.FlipVertical(source, limits, cancellation),
        "cw" => ImageTransforms.Rotate(source, QuarterTurn.Clockwise90, limits, cancellation),
        "half" => ImageTransforms.Rotate(source, QuarterTurn.Half, limits, cancellation),
        "ccw" => ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90, limits, cancellation),
        "transpose" => ImageTransforms.Transpose(source, false, limits, cancellation),
        "transverse" => ImageTransforms.Transpose(source, true, limits, cancellation),
        "nearest" => ImageTransforms.Resize(source, 4101, 1, ResizeFilter.Nearest, limits, cancellation),
        "identity" => ImageTransforms.Resize(source, 4103, 2, ResizeFilter.Bicubic, limits, cancellation),
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };
}
