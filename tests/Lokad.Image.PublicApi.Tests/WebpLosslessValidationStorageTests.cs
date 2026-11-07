using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using Lokad.Image.Tooling;
using Xunit.Abstractions;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpLosslessValidationStorageTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void MainSyntaxAgreesWithRenderingAcrossAuxiliaryStateAndDamagedPayloads(bool live)
    {
        using var cancellation = new CancellationTokenSource();
        var token = live ? cancellation.Token : default;
        byte[][] cases = [AuthoredWebp.Palette(3, true), AuthoredWebp.Palette(17),
            AuthoredWebp.ColorTransform(), AuthoredWebp.SubtractGreen(), AuthoredWebp.CachedColor(11),
            AuthoredWebp.BackwardReference(), AuthoredWebp.EntropyGroupsWithCrossingCopy(),
            AuthoredWebp.LongGreenCacheRaster(), AuthoredWebp.DuplicateSimpleSymbols(),
            AuthoredWebp.Predictor(14), AuthoredWebp.InvalidCache(0),
            AuthoredWebp.BackwardReference(missingLiteral: true), AuthoredWebp.BackwardReference(width: 3)];
        foreach (var packet in cases)
        {
            var unchanged = packet.ToArray();
            Compare(packet);
            var length = BinaryPrimitives.ReadInt32LittleEndian(packet.AsSpan(16));
            foreach (var kept in new[] { 5, 6, length / 2, length - 1 }.Distinct())
                if (kept >= 5 && kept < length) Compare(AuthoredWebp.WrapPayload(packet.AsSpan(20, kept).ToArray()));
            foreach (var index in new[] { 25, packet.Length / 2, 19 + length }.Distinct())
            {
                var damaged = packet.ToArray(); damaged[index] ^= 0x40;
                Compare(damaged);
            }
            Assert.Equal(unchanged, packet);
        }

        void Compare(byte[] packet)
        {
            var decoded = Observe(() => { using var image = ImageCodec.Decode(packet, cancellationToken: token); });
            var validated = Observe(() => ImageCodec.ValidatePixels(packet, cancellationToken: token));
            Assert.Equal(decoded, validated);
            if (validated == "ok") Assert.Equal(ImageCodec.Identify(packet), ImageCodec.ValidatePixels(packet, cancellationToken: token));
        }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ValidationAdmitsOnlyRequiredStorageAndRecoversAfterRefusalAndCancellation(bool lossy)
    {
        var fixture = OriginalExif.Create(lossy ? MetadataProfile.WebpLossyAlpha : MetadataProfile.WebpLosslessAlpha, 513, 259);
        var unchanged = fixture.Encoded.ToArray(); var expected = ImageCodec.Identify(fixture.Encoded);
        const int capacity = 256 * 1024;
        using var root = new TestMemoryBudget(capacity);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 1024 * 1024 };
        Assert.Equal(expected, ImageCodec.ValidatePixels(fixture.Encoded, limits)); Assert.Equal(0, root.Bytes);
        using var live = new CancellationTokenSource();
        Assert.Equal(expected, ImageCodec.ValidatePixels(fixture.Encoded, limits, live.Token)); Assert.Equal(0, root.Bytes);
        var refused = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(fixture.Encoded, new() { Limits = limits }));
        Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope); Assert.Equal(0, root.Bytes);
        Assert.True(root.TryReserve(capacity - 4096, out var held));
        var refusals = root.Refusals;
        try
        {
            refused = Assert.Throws<ImageLimitException>(() => ImageCodec.ValidatePixels(fixture.Encoded, limits));
            Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope);
            Assert.Equal(refusals + 1, root.Refusals); Assert.Equal(capacity - 4096, root.Bytes);
        }
        finally { held.Dispose(); }
        using var canceled = new CancellationTokenSource();
        root.Admitted = _ => canceled.Cancel();
        var failure = Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.ValidatePixels(fixture.Encoded, limits, canceled.Token));
        Assert.Equal(canceled.Token, failure.CancellationToken); Assert.Equal(0, root.Bytes);
        root.Admitted = null;
        Assert.Equal(expected, ImageCodec.ValidatePixels(fixture.Encoded, limits));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(unchanged, fixture.Encoded);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CompleteValidationStorageIsReleasedAndItsPeakIsObservable(bool lossy)
    {
        var fixture = OriginalExif.Create(lossy ? MetadataProfile.WebpLossyAlpha : MetadataProfile.WebpLosslessAlpha, 513, 259);
        var unchanged = fixture.Encoded.ToArray(); var expected = ImageCodec.Identify(fixture.Encoded);
        using var root = new TestMemoryBudget(2 * 1024 * 1024);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 4 * 1024 * 1024 };
        Assert.Equal(expected, ImageCodec.ValidatePixels(fixture.Encoded, limits));
        var firstPeak = root.Peak; Assert.Equal(0, root.Bytes);
        using var live = new CancellationTokenSource();
        Assert.Equal(expected, ImageCodec.ValidatePixels(fixture.Encoded, limits, live.Token));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(unchanged, fixture.Encoded);
        output.WriteLine(JsonSerializer.Serialize(new { operation = lossy ? "VP8+ALPH validation" : "VP8L validation",
            encodedSha256 = Convert.ToHexStringLower(SHA256.HashData(fixture.Encoded)),
            firstPeakAdmittedBytes = firstPeak, livePeakAdmittedBytes = root.Peak, retainedBytes = root.Bytes,
            sourceAndDescriptorExact = true, borrowedRootAlive = !root.IsDisposed }));
    }

    private static string Observe(Action action)
    {
        try { action(); return "ok"; }
        catch (ImageDataException) { return nameof(ImageDataException); }
        catch (UnsupportedImageException error) { return "unsupported:" + error.Feature; }
        catch (ImageLimitException error) { return "limit:" + error.Limit + ":" + error.Scope; }
    }
}
