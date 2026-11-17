using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class RectangleBulkRowTests
{
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
