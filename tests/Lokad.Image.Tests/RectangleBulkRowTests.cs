using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class RectangleBulkRowTests
{
    [Fact]
    public void SeparateByteCopyKeepsBothClippedEdgesAndUntouchedPixelsExact()
    {
        foreach (var width in new[] { 127, 128, 129, 254, 256, 257, 4096, 4097 })
        {
            using var source = OriginalPixels.Buffer(width + 5, 6, 8);
            using var canvas = OriginalPixels.Buffer(width + 9, 6, 8);
            var input = OriginalPixels.Bytes(source);
            var before = OriginalPixels.Bytes(canvas);
            var expected = before.ToArray();
            for (var row = 0; row < 3; row++)
                input.AsSpan((1 + row) * source.Stride + 3 * 4, width * 4)
                    .CopyTo(expected.AsSpan((2 + row) * canvas.Stride, width * 4));

            using var budget = new TestMemoryBudget(0);
            var limits = new ImageLimits { MemoryBudget = budget, MaxWorkingBytes = input.LongLength + before.LongLength };
            ImageDrawing.CopyRegion(source, 1, 1, width + 2, 3, canvas, -2, 2, limits);

            Assert.Equal(expected, OriginalPixels.Bytes(canvas));
            Assert.Equal(input, OriginalPixels.Bytes(source));
            Assert.Equal(0, budget.Bytes);
            Assert.False(budget.IsDisposed);
        }
    }

    [Fact]
    public void SeparateWordCopyKeepsCanariesAcrossPairedRows()
    {
        foreach (var width in new[] { 255, 256, 257, 512 })
        foreach (var height in new[] { 1, 2, 3, 4, 5 })
        {
            using var source = OriginalPixels.Buffer(width + 5, height + 2, 16);
            using var canvas = OriginalPixels.Buffer(width + 9, height + 3, 16);
            var input = OriginalPixels.Bytes(source);
            var before = OriginalPixels.Bytes(canvas);
            var expected = before.ToArray();
            for (var row = 0; row < height; row++)
                input.AsSpan((1 + row) * source.Stride + 8, width * 8)
                    .CopyTo(expected.AsSpan((2 + row) * canvas.Stride + 24, width * 8));

            using var budget = new TestMemoryBudget(0);
            var limits = new ImageLimits { MemoryBudget = budget, MaxWorkingBytes = input.LongLength + before.LongLength };
            using var canceled = new CancellationTokenSource();
            canceled.Cancel();
            Assert.ThrowsAny<OperationCanceledException>(() => ImageDrawing.CopyRegion(source, 1, 1, width, height,
                canvas, 3, 2, limits, canceled.Token));
            Assert.Equal(before, OriginalPixels.Bytes(canvas));

            ImageDrawing.CopyRegion(source, 1, 1, width, height, canvas, 3, 2, limits);
            Assert.Equal(expected, OriginalPixels.Bytes(canvas));
            Assert.Equal(input, OriginalPixels.Bytes(source));
            Assert.Equal(0, budget.Bytes);
            Assert.False(budget.IsDisposed);
        }
    }

    [Theory]
    [InlineData(8, -1)] [InlineData(8, 1)]
    [InlineData(16, -1)] [InlineData(16, 1)]
    public void ExactSnapshotAtBulkAndCancellationChunkBoundaries(int depth, int direction)
    {
        var pixelBytes = depth / 8 * 4;
        var threshold = 256 / pixelBytes;
        foreach (var width in new[] { threshold - 1, threshold, threshold + 1, threshold + 64 / pixelBytes, threshold + 128 / pixelBytes, threshold + 192 / pixelBytes, 4096, 4097 })
        {
            using var canvas = OriginalPixels.Buffer(width + 11, 6, depth);
            var before = OriginalPixels.Bytes(canvas); var expected = before.ToArray();
            var sx = direction > 0 ? 3 : 4; var sy = direction > 0 ? 1 : 2;
            var dx = sx + direction; var dy = sy + direction;
            for (var row = 0; row < 3; row++)
                Array.Copy(before, ((sy + row) * canvas.Width + sx) * pixelBytes,
                    expected, ((dy + row) * canvas.Width + dx) * pixelBytes, width * pixelBytes);
            using var root = new TestMemoryBudget(0);
            var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = before.LongLength };
            using var canceled = new CancellationTokenSource(); canceled.Cancel();
            Assert.ThrowsAny<OperationCanceledException>(() => ImageDrawing.CopyRegion(canvas, sx, sy, width, 3,
                canvas, dx, dy, limits, canceled.Token));
            Assert.Equal(before, OriginalPixels.Bytes(canvas));
            ImageDrawing.CopyRegion(canvas, sx, sy, width, 3, canvas, dx, dy, limits);
            Assert.Equal(expected, OriginalPixels.Bytes(canvas));
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }
}
