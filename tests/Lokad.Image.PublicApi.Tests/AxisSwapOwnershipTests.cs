using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class AxisSwapOwnershipTests
{
    [Theory]
    [InlineData(8, ImageOrientation.Transpose)]
    [InlineData(16, ImageOrientation.Transpose)]
    [InlineData(8, ImageOrientation.Rotate90)]
    [InlineData(16, ImageOrientation.Rotate90)]
    [InlineData(8, ImageOrientation.Rotate270)]
    [InlineData(16, ImageOrientation.Rotate270)]
    [InlineData(8, ImageOrientation.Transverse)]
    [InlineData(16, ImageOrientation.Transverse)]
    public void CompletePixelPayloadsAndOwnersSurviveMatrixAndTileBoundaries(int depth, ImageOrientation orientation)
    {
        foreach (var (width, height) in new[] { (3, 7), (4, 4), (5, 7), (31, 33), (32, 32), (33, 35) })
        {
            var bytes = width * height * (depth / 8) * 4;
            using var root = new TestMemoryBudget(3L * bytes);
            var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2L * bytes };
            using var source = ImageBuffer.Create(width, height, depth, limits);
            FillEveryPixelBit(source);
            var original = OriginalPixels.Bytes(source);
            var expected = Reference(original, width, height, depth, orientation);
            ImageBuffer Copy(CancellationToken cancellation = default) => orientation switch
            {
                ImageOrientation.Transpose => ImageTransforms.Transpose(source, false, limits, cancellation),
                ImageOrientation.Transverse => ImageTransforms.Transpose(source, true, limits, cancellation),
                ImageOrientation.Rotate90 => ImageTransforms.Rotate(source, QuarterTurn.Clockwise90, limits, cancellation),
                ImageOrientation.Rotate270 => ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90, limits, cancellation),
                _ => throw new ArgumentOutOfRangeException(nameof(orientation))
            };

            using (var cancellation = new CancellationTokenSource())
            {
                root.Admitted = used => { if (used == 2L * bytes) cancellation.Cancel(); };
                var error = Assert.Throws<OperationCanceledException>(() => Copy(cancellation.Token));
                Assert.Equal(cancellation.Token, error.CancellationToken);
                root.Admitted = null;
            }
            Assert.Equal(bytes, root.Bytes);
            using var first = Copy(); using var second = Copy();
            Assert.Equal((height, width, depth, source.Orientation),
                (first.Width, first.Height, first.BitDepth, first.Orientation));
            Assert.Equal(expected, OriginalPixels.Bytes(first)); Assert.Equal(expected, OriginalPixels.Bytes(second));
            Assert.Equal(3L * bytes, root.Bytes); Assert.Equal(3L * bytes, root.Peak);
            var attempts = root.Attempts;
            var refusal = Assert.Throws<ImageLimitException>(() => Copy());
            Assert.Equal(ImageLimitScope.SharedBudget, refusal.Scope);
            Assert.Equal(attempts + 1, root.Attempts); Assert.Equal(3L * bytes, root.Bytes);
            if (depth == 8) first.GetRow8(0)[0] ^= 255;
            else first.GetRow16(0)[0] ^= 65535;
            Assert.Equal(original, OriginalPixels.Bytes(source));
            source.Dispose(); first.Dispose();
            Assert.Equal(bytes, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(second));
            second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }

    private static void FillEveryPixelBit(ImageBuffer image)
    {
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        {
            var index = y * image.Width + x;
            var payload = unchecked((ulong)(index + 1) * 0x9e3779b97f4a7c15UL)
                ^ System.Numerics.BitOperations.RotateLeft(1UL, index & 63);
            if (image.BitDepth == 8)
            {
                if (index % 11 == 0) payload &= 0x00ffffffUL;
                for (var channel = 0; channel < 4; channel++) image.GetRow8(y)[x * 4 + channel] = (byte)(payload >> (channel * 8));
            }
            else
            {
                if (index % 11 == 0) payload &= 0x0000ffffffffffffUL;
                for (var channel = 0; channel < 4; channel++) image.GetRow16(y)[x * 4 + channel] = (ushort)(payload >> (channel * 16));
            }
        }
    }

    private static byte[] Reference(byte[] input, int width, int height, int depth, ImageOrientation orientation)
    {
        var result = new byte[input.Length]; var pixelBytes = depth / 8 * 4;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var target = orientation switch
            {
                ImageOrientation.Transpose => (X: y, Y: x),
                ImageOrientation.Rotate90 => (X: height - 1 - y, Y: x),
                ImageOrientation.Rotate270 => (X: y, Y: width - 1 - x),
                ImageOrientation.Transverse => (X: height - 1 - y, Y: width - 1 - x),
                _ => throw new ArgumentOutOfRangeException(nameof(orientation))
            };
            input.AsSpan((y * width + x) * pixelBytes, pixelBytes)
                .CopyTo(result.AsSpan((target.Y * height + target.X) * pixelBytes, pixelBytes));
        }
        return result;
    }
}
