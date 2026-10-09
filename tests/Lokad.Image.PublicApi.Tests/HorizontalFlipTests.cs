using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class HorizontalFlipTests
{
    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void ReusedStorageIsCompletelyOverwrittenAndDisposeReturnsItOnlyOnce(int depth)
    {
        using var source = OriginalPixels.Buffer(513, 3, depth);
        var original = OriginalPixels.Bytes(source);
        var released = ImageTransforms.FlipHorizontal(source);
        for (var y = 0; y < released.Height; y++)
            if (depth == 8) released.GetRow8(y).Fill(0xA5);
            else released.GetRow16(y).Fill(0xA5A5);
        Parallel.Invoke(released.Dispose, released.Dispose);
        using var first = ImageTransforms.FlipHorizontal(source);
        released.Dispose(); // A repeated disposal must not release first's lease.
        using var second = ImageTransforms.FlipHorizontal(source);
        var expected = OriginalExif.Permute(original, source.Width, source.Height, depth, ImageOrientation.MirrorHorizontal);
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        if (depth == 8) second.GetRow8(0)[0] ^= 0xFF;
        else second.GetRow16(0)[0] ^= 0xFFFF;
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(original, OriginalPixels.Bytes(source));
        using var zeroed = ImageBuffer.Create(source.Width, source.Height, depth);
        Assert.All(OriginalPixels.Bytes(zeroed), sample => Assert.Equal(0, sample));
        if (depth == 8) Assert.Throws<ObjectDisposedException>(() => released.GetRow8(0));
        else Assert.Throws<ObjectDisposedException>(() => released.GetRow16(0));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void FailureAfterAPartialWriteCannotPublishStaleSamples(int depth)
    {
        using var source = OriginalPixels.Buffer(2051, 2, depth);
        var original = OriginalPixels.Bytes(source);
        Assert.Throws<ImageLimitException>(() => ImageTransforms.FlipHorizontal(source,
            limits: new ImageLimits { MaxWork = 1024 * 4 }));
        using var complete = ImageTransforms.FlipHorizontal(source);
        Assert.Equal(OriginalExif.Permute(original, source.Width, source.Height, depth, ImageOrientation.MirrorHorizontal),
            OriginalPixels.Bytes(complete));
        Assert.Equal(original, OriginalPixels.Bytes(source));
    }

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
