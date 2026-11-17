using Lokad.Image.Tooling;
namespace Lokad.Image.Tests;
public sealed class VerticalPreparedRowsTests
{
    [Theory]
    [InlineData(8, false)] [InlineData(8, true)]
    [InlineData(16, false)] [InlineData(16, true)]
    public void ExactRowsChunksAdmissionAndSurvivors(int depth, bool liveToken)
    {
        foreach (var width in new[] { 7, 31, 32, 33, 63, 64, 65, 79, 95, 127, 128, 129, 256, 512, 513, 1023, 1024, 1025 })
        {
            using var source = OriginalPixels.Buffer(width, 5, depth);
            var saved = OriginalPixels.Bytes(source); var expected = new byte[saved.Length];
            for (var row = 0; row < source.Height; row++)
                Array.Copy(saved, (source.Height - 1 - row) * source.Stride, expected, row * source.Stride, source.Stride);
            using var root = new TestMemoryBudget(saved.Length);
            var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 2L * saved.Length };
            using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
            Assert.ThrowsAny<OperationCanceledException>(() => ImageTransforms.FlipVertical(source, limits, cancelled.Token));
            Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts);
            using var active = new CancellationTokenSource();
            using var result = ImageTransforms.FlipVertical(source, limits, liveToken ? active.Token : default);
            Assert.Equal(expected, OriginalPixels.Bytes(result)); Assert.Equal(saved, OriginalPixels.Bytes(source));
            Assert.Equal(saved.Length, root.Bytes);
            var failure = Assert.Throws<ImageLimitException>(() => ImageTransforms.FlipVertical(source, limits));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (failure.Limit, failure.Scope));
            Assert.Equal(saved.Length, root.Bytes); Assert.Equal(expected, OriginalPixels.Bytes(result));
            result.Dispose(); Assert.Equal(0, root.Bytes);
            using var recovered = ImageTransforms.FlipVertical(source, limits);
            source.Dispose(); Assert.Equal(expected, OriginalPixels.Bytes(recovered));
            recovered.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }
}
