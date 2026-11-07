using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpVp8ValidationMemoryTests
{
    [Fact]
    public async Task LiveTokenMaterializationPreservesPixelsAndIndependentAsyncOwnership()
    {
        var fixture = OriginalExif.Create(MetadataProfile.WebpLossyAlpha, 513, 259);
        var inputBytes = fixture.Encoded.ToArray();
        byte[] expected;
        using (var ordinary = ImageCodec.Decode(inputBytes)) expected = OriginalPixels.Bytes(ordinary);
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = new() { MemoryBudget = root } };
        using var live = new CancellationTokenSource();
        using var first = ImageCodec.Decode(inputBytes, options, live.Token);
        using var input = new MemoryStream(inputBytes, writable: false);
        using var second = await ImageCodec.DecodeAsync(input, options, live.Token);
        Assert.Equal(fixture.Encoded, inputBytes); Assert.True(input.CanRead);
        Assert.Equal(2L * 513 * 259 * 4, root.Bytes);
        inputBytes.AsSpan().Clear();
        Assert.Equal(expected, OriginalPixels.Bytes(first));
        first.GetRow8(0).Clear(); first.Dispose();
        Assert.Equal(513L * 259 * 4, root.Bytes);
        Assert.Equal(expected, OriginalPixels.Bytes(second));
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

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
        var firstPeak = root.Peak;
        using var live = new CancellationTokenSource();
        Assert.Equal(expected, ImageCodec.ValidatePixels(fixture.Encoded, limits, live.Token));
        Assert.Equal(0, root.Bytes);
        var livePeak = root.Peak;
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
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new {
            operation = "VP8+ALPH complete span validation", width = 513, height = 259,
            encodedSha256 = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(fixture.Encoded)),
            firstPeakAdmittedBytes = firstPeak, livePeakAdmittedBytes = livePeak,
            retainedBytes = root.Bytes, borrowedRootAlive = !root.IsDisposed,
            refusalCancellationAndRecoveryPassed = true }));
    }
}
