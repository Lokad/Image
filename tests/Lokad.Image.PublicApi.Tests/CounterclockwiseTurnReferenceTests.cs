using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class CounterclockwiseTurnReferenceTests
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
    [InlineData(16, 3, 7)]
    [InlineData(16, 4, 4)]
    [InlineData(16, 5, 7)]
    [InlineData(16, 31, 33)]
    [InlineData(16, 32, 32)]
    [InlineData(16, 33, 35)]
    public void CounterclockwiseMatchesIndependentPlacementAndExactAdmission(int depth, int width, int height)
    {
        using var source = OriginalPixels.Buffer(width, height, depth);
        if (depth == 8) source.GetRow8(0)[3] = 0;
        else source.GetRow16(0)[3] = 0;
        var original = OriginalPixels.Bytes(source);
        var expected = CounterclockwiseReference(original, width, height, depth);
        var peak = original.Length * 2L;

        var limits = new ImageLimits { MaxWorkingBytes = peak};
        using var first = ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90, limits);
        using var second = ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90, limits);
        Assert.Equal((height, width, depth, source.Orientation),
            (first.Width, first.Height, first.BitDepth, first.Orientation));
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        Assert.Throws<ImageLimitException>(() => ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90,
            limits with { MaxWorkingBytes = peak - 1 }));
        Assert.Throws<OperationCanceledException>(() => ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90,
            limits, new CancellationToken(true)));
        Assert.Equal(original, OriginalPixels.Bytes(source));
    }

    [Theory]
    [InlineData(MetadataProfile.Png8)]
    [InlineData(MetadataProfile.Png16)]
    public void DirectRotationRetainsExifWhileAutoOrientationResetsIt(MetadataProfile profile)
    {
        var fixture = OriginalExif.Create(profile, 7, 5);
        var encoded = OriginalExif.Attach(fixture, ImageOrientation.Rotate270, bigEndian: true);
        using var source = ImageCodec.Decode(encoded);
        var original = OriginalPixels.Bytes(source);
        var expected = CounterclockwiseReference(original, source.Width, source.Height, source.BitDepth);
        using var flipped = ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90);
        using var oriented = ImageTransforms.AutoOrient(source);
        Assert.Equal(ImageOrientation.Rotate270, flipped.Orientation);
        Assert.Equal(ImageOrientation.Normal, oriented.Orientation);
        Assert.Equal(expected, OriginalPixels.Bytes(flipped));
        Assert.Equal(expected, OriginalPixels.Bytes(oriented));
        Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.Equal(ImageOrientation.Rotate270, source.Orientation);
    }

    [Fact]
    public void WordMatricesKeepEveryPixelBitAndReleaseRefusedOrCanceledOwners()
    {
        const int width = 35, height = 37, bytes = width * height * 8;
        using var root = new TestMemoryBudget(3L * bytes);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2L * bytes };
        using var source = ImageBuffer.Create(width, height, 16, limits);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var index = y * width + x;
            var payload = unchecked((ulong)(index + 1) * 0x9e3779b97f4a7c15UL)
                ^ System.Numerics.BitOperations.RotateLeft(1UL, index & 63);
            if (index % 11 == 0) payload &= 0x0000ffffffffffffUL;
            var row = source.GetRow16(y);
            for (var c = 0; c < 4; c++) row[x * 4 + c] = (ushort)(payload >> (c * 16));
        }
        var original = OriginalPixels.Bytes(source);
        var expected = CounterclockwiseReference(original, width, height, 16);
        using (var cancellation = new CancellationTokenSource())
        {
            root.Admitted = used => { if (used == 2L * bytes) cancellation.Cancel(); };
            Assert.Throws<OperationCanceledException>(() => ImageTransforms.Rotate(source,
                QuarterTurn.CounterClockwise90, limits, cancellation.Token));
            root.Admitted = null;
        }
        Assert.Equal(bytes, root.Bytes);
        using var first = ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90, limits);
        using var second = ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90, limits);
        Assert.Equal(3L * bytes, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(first)); Assert.Equal(expected, OriginalPixels.Bytes(second));
        var attempts = root.Attempts;
        var error = Assert.Throws<ImageLimitException>(() => ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90, limits));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(attempts + 1, root.Attempts);
        Assert.Equal(3L * bytes, root.Bytes); Assert.Equal(original, OriginalPixels.Bytes(source));
        source.Dispose(); first.Dispose();
        Assert.Equal(bytes, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(second));
        using (var restored = ImageTransforms.Rotate(second, QuarterTurn.Clockwise90, limits))
        {
            Assert.Equal(original, OriginalPixels.Bytes(restored)); Assert.Equal(2L * bytes, root.Bytes);
            second.Dispose(); Assert.Equal(bytes, root.Bytes); Assert.Equal(original, OriginalPixels.Bytes(restored));
        }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private static byte[] CounterclockwiseReference(byte[] input, int width, int height, int depth)
    {
        var result = new byte[input.Length];
        var pixelBytes = depth / 8 * 4;
        // Independent forward placement of every original pixel/channel byte.
        for (var sy = 0; sy < height; sy++)
        for (var sx = 0; sx < width; sx++)
        for (var b = 0; b < pixelBytes; b++)
            result[((width - 1 - sx) * height + sy) * pixelBytes + b] = input[(sy * width + sx) * pixelBytes + b];
        return result;
    }
}
