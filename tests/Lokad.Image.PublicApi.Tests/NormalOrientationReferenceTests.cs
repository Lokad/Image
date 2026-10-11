using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class NormalOrientationReferenceTests
{
    [Theory]
    [InlineData(8, 4103, 3)]
    [InlineData(16, 4103, 3)]
    [InlineData(8, 1025, 2)]
    [InlineData(16, 1025, 2)]
    [InlineData(8, 1, 5)]
    [InlineData(16, 1, 5)]
    [InlineData(8, 7, 1)]
    [InlineData(16, 7, 1)]
    public void NormalOrientationRetainsExactWordsAndAdmission(int depth, int width, int height)
    {
        using var source = OriginalPixels.Buffer(width, height, depth);
        if (depth == 8) source.GetRow8(0)[3] = 0;
        else source.GetRow16(0)[3] = 0;
        var original = OriginalPixels.Bytes(source);
        var expected = original.ToArray();
        var peak = original.Length * 2L;
        var work = (long)width * height * 4;
        var limits = new ImageLimits { MaxWorkingBytes = peak, MaxWork = work };
        using var first = ImageTransforms.AutoOrient(source, limits);
        using var second = ImageTransforms.AutoOrient(source, limits);
        Assert.Equal((width, height, depth, source.Orientation),
            (first.Width, first.Height, first.BitDepth, first.Orientation));
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.AutoOrient(source,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.AutoOrient(source,
            limits with { MaxWork = work - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.AutoOrient(source,
            limits, new CancellationToken(true)));
        Assert.Equal(original, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void PartialFailureCannotPublishStaleReusableRows(int depth)
    {
        using var source = OriginalPixels.Buffer(2051, 3, depth);
        var original = OriginalPixels.Bytes(source);
        using (var poison = ImageTransforms.AutoOrient(source))
            for (var y = 0; y < poison.Height; y++)
                if (depth == 8) poison.GetRow8(y).Fill(0xA5);
                else poison.GetRow16(y).Fill(0xA55A);
        Assert.Throws<ImageLimitException>(() => ImageTransforms.AutoOrient(source,
            new ImageLimits { MaxWork = 4096 }));
        using var result = ImageTransforms.AutoOrient(source);
        Assert.Equal(original, OriginalPixels.Bytes(result));
        Assert.Equal(original, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)]
    [InlineData(MetadataProfile.Png16)]
    public void ExplicitNormalExifRetainsWordsAndIndependentOwners(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 7, 5);
        var encoded = OriginalExif.Attach(fixture, ImageOrientation.Normal, bigEndian: true);
        using var source = ImageCodec.Decode(encoded);
        var original = OriginalPixels.Bytes(source);
        using var first = ImageTransforms.AutoOrient(source);
        using var second = ImageTransforms.AutoOrient(source);
        Assert.Equal(ImageOrientation.Normal, first.Orientation);
        Assert.Equal(original, OriginalPixels.Bytes(first));
        if (source.BitDepth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(original, OriginalPixels.Bytes(second));
        Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(ImageOrientation.Normal, source.Orientation);
    }
}
