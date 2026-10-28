using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngPredictableCompressionTests
{
    [Fact]
    public void LargeWordGradientAndItsGeometricTransformsStaySmallAndLossless()
    {
        using var source = OriginalPngGradient.Create();
        for (var geometry = 0; geometry < 6; geometry++)
        {
            using var transformed = geometry switch
            {
                0 => null, 1 => ImageTransforms.FlipHorizontal(source), 2 => ImageTransforms.FlipVertical(source),
                3 => ImageTransforms.Rotate(source, QuarterTurn.Clockwise90),
                4 => ImageTransforms.Rotate(source, QuarterTurn.Half),
                _ => ImageTransforms.Rotate(source, QuarterTurn.CounterClockwise90)
            };
            var image = transformed ?? source;
            using var output = new MemoryStream();
            ImageCodec.EncodePng(image, output, new() { Limits = ImageLimits.Default with { MaxOutputBytes = 2 * 1024 * 1024 } });
            Assert.InRange(output.Length, 1, 2 * 1024 * 1024 - 1); Assert.True(output.CanWrite);
            IndependentRgbaPng.Verify(output.ToArray(), image);
        }
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public async Task PredictableTransparencyAndHiddenRgbSurviveSyncAndAsyncEncoding(int depth)
    {
        using var source = OriginalPngGradient.Create(128, depth, transparent: true);
        var original = OriginalPixels.Bytes(source);
        using var sync = new MemoryStream(); using var asynchronous = new MemoryStream();
        ImageCodec.EncodePng(source, sync); await ImageCodec.EncodePngAsync(source, asynchronous);
        var bytes = sync.ToArray(); Assert.Equal(bytes, asynchronous.ToArray());
        IndependentRgbaPng.Verify(bytes, source); Assert.Equal(original, OriginalPixels.Bytes(source));
        Assert.True(sync.CanWrite); Assert.True(asynchronous.CanWrite);
    }

    [Fact]
    public void PredictionRowsShareTheCallerRootAndRefusalCleanupAdmitsTheNextEncode()
    {
        const int sourceBytes = 128 * 128 * 4, rowBytes = 128 * 4;
        const int owned = rowBytes * 3 + 65536 + 1024 * 1024;
        using var root = new TestMemoryBudget(sourceBytes + owned);
        using var source = OriginalPngGradient.Create(128, 8, budget: root);
        var options = new PngEncodeOptions { Limits = ImageLimits.Default with { MemoryBudget = root } };
        Assert.True(root.TryReserve(1, out var held));
        using var rejected = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, rejected, options));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
        Assert.Equal(0, rejected.Length); Assert.True(rejected.CanWrite); Assert.Equal(sourceBytes + 1, root.Bytes);
        held.Dispose(); using var next = new MemoryStream(); ImageCodec.EncodePng(source, next, options);
        IndependentRgbaPng.Verify(next.ToArray(), source); Assert.Equal(sourceBytes, root.Bytes);
        source.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void CancellationDuringPredictionRowAdmissionRefundsAllStorage()
    {
        using var root = new TestMemoryBudget(4 * 1024 * 1024); using var cancellation = new CancellationTokenSource();
        using var source = OriginalPngGradient.Create(128, 8, budget: root); var retained = root.Bytes;
        root.Admitted = bytes => { if (bytes == retained + 128 * 4 * 3 + 65536) cancellation.Cancel(); };
        using var output = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodePng(source, output,
            new() { Limits = ImageLimits.Default with { MemoryBudget = root } }, cancellation.Token));
        Assert.Equal(0, output.Length); Assert.True(output.CanWrite); Assert.Equal(retained, root.Bytes);
        root.Admitted = null; using var next = new MemoryStream();
        ImageCodec.EncodePng(source, next, new() { Limits = ImageLimits.Default with { MemoryBudget = root } });
        IndependentRgbaPng.Verify(next.ToArray(), source); Assert.Equal(retained, root.Bytes);
    }
}
