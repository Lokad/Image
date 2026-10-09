using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class HorizontalFlipTests
{
    [Theory]
    [InlineData(MetadataProfile.Png8)]
    [InlineData(MetadataProfile.Png16)]
    public void ExplicitFlipRetainsOutstandingOrientation(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 17, 3);
        using var source = ImageCodec.Decode(OriginalExif.Attach(fixture, ImageOrientation.Rotate90, bigEndian: true));
        var pixels = OriginalPixels.Bytes(source);
        using var flipped = ImageTransforms.FlipHorizontal(source);
        Assert.Equal(ImageOrientation.Rotate90, flipped.Orientation);
        Assert.Equal((17, 3), (flipped.Width, flipped.Height));
        Assert.Equal(OriginalExif.Permute(pixels, 17, 3, source.BitDepth, ImageOrientation.MirrorHorizontal),
            OriginalPixels.Bytes(flipped));
        Assert.Equal(pixels, OriginalPixels.Bytes(source));
        Assert.Equal(ImageOrientation.Rotate90, source.Orientation);
    }

    public static IEnumerable<object[]> Boundaries()
    {
        foreach (var depth in new[] { 8, 16 })
        foreach (var width in new[] { 1, 2, 3, 4, 5, 7, 8, 9, 15, 16, 17, 513, 1023, 1024, 1025, 4103 })
            yield return [depth, width];
    }

    [Theory]
    [MemberData(nameof(Boundaries))]
    public void ReversesWholePixelsAcrossVectorAndWorkBoundaries(int depth, int width)
    {
        using var source = OriginalPixels.Buffer(width, 3, depth);
        if (depth == 8) source.GetRow8(0)[3] = 0;
        else source.GetRow16(0)[3] = 0;
        var original = OriginalPixels.Bytes(source);
        using var flipped = ImageTransforms.FlipHorizontal(source);
        Assert.Equal((width, 3, depth, source.Orientation),
            (flipped.Width, flipped.Height, flipped.BitDepth, flipped.Orientation));
        // Check each source pixel's destination, preserving all four channels.
        for (var y = 0; y < 3; y++)
        for (var x = 0; x < width; x++)
            if (depth == 8)
                Assert.Equal(source.GetRow8(y).Slice(x * 4, 4).ToArray(), flipped.GetRow8(y).Slice((width - x - 1) * 4, 4).ToArray());
            else
                Assert.Equal(source.GetRow16(y).Slice(x * 4, 4).ToArray(), flipped.GetRow16(y).Slice((width - x - 1) * 4, 4).ToArray());
        using var restored = ImageTransforms.FlipHorizontal(flipped);
        Assert.Equal(original, OriginalPixels.Bytes(restored));
        // Output ownership is independent in both directions.
        flipped.Dispose();
        Assert.Equal(original, OriginalPixels.Bytes(source));
        source.Dispose();
        Assert.Equal(original, OriginalPixels.Bytes(restored));
    }
}
