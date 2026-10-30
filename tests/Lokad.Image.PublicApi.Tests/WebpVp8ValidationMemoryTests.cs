using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpVp8ValidationMemoryTests
{
    [Fact]
    public void CompleteValidationFitsWithoutDiscardedPlanesAndBorrowsTheSharedRoot()
    {
        var fixture = OriginalExif.Create(MetadataProfile.WebpLossyAlpha, 513, 259);
        var unchanged = fixture.Encoded.ToArray();
        const int capacity = 640 * 1024;
        using var root = new TestMemoryBudget(capacity);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 1024 * 1024 };
        var expected = ImageCodec.Identify(fixture.Encoded);
        Assert.Equal(expected, ImageCodec.ValidatePixels(fixture.Encoded, limits));
        Assert.InRange(root.Peak, 1, capacity); Assert.Equal(0, root.Bytes);
        using var live = new CancellationTokenSource();
        Assert.Equal(expected, ImageCodec.ValidatePixels(fixture.Encoded, limits, live.Token));
        Assert.Equal(0, root.Bytes);
        Assert.True(root.TryReserve(capacity - 4096, out var caller));
        var refusals = root.Refusals;
        try
        {
            var refused = Assert.Throws<ImageLimitException>(() => ImageCodec.ValidatePixels(fixture.Encoded, limits));
            Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope);
            Assert.Equal(refusals + 1, root.Refusals); Assert.Equal(capacity - 4096, root.Bytes);
        }
        finally { caller.Dispose(); }
        using var cancellation = new CancellationTokenSource();
        root.Admitted = _ => cancellation.Cancel();
        var failure = Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.ValidatePixels(fixture.Encoded, limits, cancellation.Token));
        Assert.Equal(cancellation.Token, failure.CancellationToken); Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        Assert.Equal(expected, ImageCodec.ValidatePixels(fixture.Encoded, limits));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(unchanged, fixture.Encoded);
    }
}
