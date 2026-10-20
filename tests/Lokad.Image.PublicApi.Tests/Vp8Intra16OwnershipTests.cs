using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class Vp8Intra16OwnershipTests
{
    [Theory] [InlineData(8)] [InlineData(16)]
    public void LossySearchReleasesEveryRefusedOrCanceledReservationAndPreservesOutputs(int depth)
    {
        using var source = OriginalPixels.Buffer(129, 129, depth);
        var unchanged = OriginalPixels.Bytes(source);
        using var root = new TestMemoryBudget(16 * 1024 * 1024);
        var options = new WebpEncodeOptions { Lossless = false, Quality = 78, Effort = 4,
            Limits = new ImageLimits { MemoryBudget = root } };
        using var output = new MemoryStream(); ImageCodec.EncodeWebp(source, output, options);
        var packet = output.ToArray(); var growths = root.GrowthAttempts;
        Assert.True(growths >= 6); Assert.Equal(0, root.Bytes);
        // Inject refusal at each substantial storage acquisition, including search
        // cache and writer replacements. A refusal must be terminal, never retried.
        for (var at = 1L; at <= growths; at++)
        {
            root.RefuseAtGrowth = root.GrowthAttempts + at;
            var refusals = root.Refusals;
            using var failed = new MemoryStream();
            var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, failed, options));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(refusals + 1, root.Refusals); Assert.Equal(0, root.Bytes); Assert.True(failed.CanWrite);
        }
        root.RefuseAtGrowth = 0;
        using var cancellation = new CancellationTokenSource();
        var cancelAt = root.GrowthAttempts + 6;
        root.Admitted = _ => { if (root.GrowthAttempts == cancelAt) cancellation.Cancel(); };
        using var canceled = new MemoryStream();
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, canceled, options, cancellation.Token));
        Assert.Equal(0, root.Bytes); Assert.True(canceled.CanWrite); root.Admitted = null;
        using var subsequent = new MemoryStream(); ImageCodec.EncodeWebp(source, subsequent, options);
        Assert.Equal(packet, subsequent.ToArray()); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(unchanged, OriginalPixels.Bytes(source));
        using var first = ImageCodec.Decode(packet); using var second = ImageCodec.Decode(subsequent.ToArray());
        source.Dispose(); output.GetBuffer().AsSpan().Clear(); subsequent.GetBuffer().AsSpan().Clear();
        Assert.Equal(OriginalPixels.Bytes(first), OriginalPixels.Bytes(second));
    }
}
