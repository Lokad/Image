using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class TransverseReferenceTests
{
    [Theory]
    [InlineData(8, 4103, 3)]
    [InlineData(16, 4103, 3)]
    [InlineData(8, 1025, 2)]
    [InlineData(16, 1025, 2)]
    [InlineData(8, 35, 67)]
    [InlineData(16, 35, 67)]
    [InlineData(8, 1, 5)]
    [InlineData(16, 1, 5)]
    [InlineData(8, 7, 1)]
    [InlineData(16, 7, 1)]
    public void TransverseMatchesIndependentPlacementAndExactAdmission(int depth, int width, int height)
    {
        using var source = OriginalPixels.Buffer(width, height, depth);
        if (depth == 8) source.GetRow8(0)[3] = 0;
        else source.GetRow16(0)[3] = 0;
        var original = OriginalPixels.Bytes(source);
        var expected = TransverseReference(original, width, height, depth);
        var peak = original.Length * 2L;

        var limits = new ImageLimits { MaxWorkingBytes = peak};
        using var first = ImageTransforms.Transpose(source, true, limits);
        using var second = ImageTransforms.Transpose(source, true, limits);
        Assert.Equal((height, width, depth, source.Orientation),
            (first.Width, first.Height, first.BitDepth, first.Orientation));
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Transpose(source, true,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Transpose(source, true,
            limits, new CancellationToken(true)));
        Assert.Equal(original, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)]
    [InlineData(MetadataProfile.Png16)]
    public void DirectMirroredTransposeRetainsExifWhileAutoOrientationResetsIt(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 7, 5);
        var encoded = OriginalExif.Attach(fixture, ImageOrientation.Transverse, bigEndian: true);
        using var source = ImageCodec.Decode(encoded);
        var original = OriginalPixels.Bytes(source);
        var expected = TransverseReference(original, source.Width, source.Height, source.BitDepth);
        using var transposed = ImageTransforms.Transpose(source, true);
        using var oriented = ImageTransforms.AutoOrient(source);
        Assert.Equal(ImageOrientation.Transverse, transposed.Orientation);
        Assert.Equal(ImageOrientation.Normal, oriented.Orientation);
        Assert.Equal(expected, OriginalPixels.Bytes(transposed));
        Assert.Equal(expected, OriginalPixels.Bytes(oriented));
        Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(ImageOrientation.Transverse, source.Orientation);
    }

    private static byte[] TransverseReference(byte[] input, int width, int height, int depth)
    {
        var result = new byte[input.Length];
        var pixelBytes = depth / 8 * 4;
        // Independent forward placement of every original pixel/channel byte.
        for (var sy = 0; sy < height; sy++)
        for (var sx = 0; sx < width; sx++)
        for (var b = 0; b < pixelBytes; b++)
            result[((width - 1 - sx) * height + height - 1 - sy) * pixelBytes + b] = input[(sy * width + sx) * pixelBytes + b];
        return result;
    }
}
