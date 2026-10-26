using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class CloneReferenceTests
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
    public void CloneRetainsExactWordsAndAdmission(int depth, int width, int height)
    {
        using var source = OriginalPixels.Buffer(width, height, depth);
        if (depth == 8) source.GetRow8(0)[3] = 0;
        else source.GetRow16(0)[3] = 0;
        var original = OriginalPixels.Bytes(source);
        var expected = original.ToArray();
        var peak = original.Length * 2L;

        var limits = new ImageLimits { MaxWorkingBytes = peak};
        using var first = source.Clone( limits);
        using var second = source.Clone( limits);
        Assert.Equal((width, height, depth, source.Orientation),
            (first.Width, first.Height, first.BitDepth, first.Orientation));
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Throws<ImageLimitException>(() => source.Clone(
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<OperationCanceledException>(() => source.Clone(
            limits, new CancellationToken(true)));
        Assert.Equal(original, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)]
    [InlineData(MetadataProfile.Png16)]
    public void CloneRetainsExifWordsAndIndependentOwners(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 7, 5);
        var encoded = OriginalExif.Attach(fixture, ImageOrientation.Transverse, bigEndian: true);
        using var source = ImageCodec.Decode(encoded);
        var original = OriginalPixels.Bytes(source);
        using var first = source.Clone();
        using var second = source.Clone();
        Assert.Equal(ImageOrientation.Transverse, first.Orientation);
        Assert.Equal(original, OriginalPixels.Bytes(first));
        if (source.BitDepth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(original, OriginalPixels.Bytes(second));
        Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(ImageOrientation.Transverse, source.Orientation);
    }
    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void IdentityResizeUsesIndependentCompleteOverwrite(int depth)
    {
        using var source = OriginalPixels.Buffer(1025, 3, depth);
        var original = OriginalPixels.Bytes(source);
        using (var poison = source.Clone())
            for (var y = 0; y < poison.Height; y++)
                if (depth == 8) poison.GetRow8(y).Fill(0xA5);
                else poison.GetRow16(y).Fill(0xA55A);
        using var first = ImageTransforms.Resize(source, source.Width, source.Height);
        using var second = ImageTransforms.ResizeToFit(source, source.Width, source.Height, false);
        Assert.Equal(original, OriginalPixels.Bytes(first));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(original, OriginalPixels.Bytes(second));
        Assert.Equal(original, OriginalPixels.Bytes(source));
    }

}
