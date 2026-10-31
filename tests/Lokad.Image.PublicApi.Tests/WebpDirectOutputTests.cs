using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpDirectOutputTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void DirtyFramesKeepLongGreenLookupHolesAndEveryPixelExact(bool governed)
    {
        var encoded = AuthoredWebp.LongGreenCacheRaster();
        var unchanged = encoded.ToArray();
        var dirtyPacket = AuthoredWebp.ConstantRaster(257, 33);
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = governed ? new() { MemoryBudget = root } : ImageLimits.Default };
        using (var dirty = ImageCodec.Decode(dirtyPacket, options))
            for (var y = 0; y < dirty.Height; y++) dirty.GetRow8(y).Fill(0xA7);
        using var first = ImageCodec.Decode(encoded, options);
        using var survivor = ImageCodec.Decode(encoded, options);
        for (var y = 0; y < first.Height; y++)
        {
            Assert.True(first.GetRow8(y).IndexOfAnyExcept((byte)0) < 0);
            Assert.True(survivor.GetRow8(y).IndexOfAnyExcept((byte)0) < 0);
        }
        first.GetRow8(0)[0] = 111; first.Dispose();
        using var third = ImageCodec.Decode(encoded, options);
        for (var y = 0; y < third.Height; y++)
        {
            Assert.True(third.GetRow8(y).IndexOfAnyExcept((byte)0) < 0);
            Assert.True(survivor.GetRow8(y).IndexOfAnyExcept((byte)0) < 0);
        }
        third.Dispose(); survivor.Dispose();
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(unchanged, encoded);
    }

    [Fact]
    public void FullResultFitsOneFramePlusParserStorageAndKeepsRetainedAdmission()
    {
        var fixture = OriginalExif.Create(MetadataProfile.WebpLosslessAlpha, 513, 259);
        const int frameBytes = 513 * 259 * 4;
        using var root = new TestMemoryBudget(frameBytes + 256 * 1024);
        var options = new DecodeOptions { Limits = new ImageLimits { MemoryBudget = root } };
        using (var first = ImageCodec.Decode(fixture.Encoded, options))
        {
            Assert.Equal(fixture.Source, OriginalPixels.Bytes(first));
            Assert.Equal(frameBytes, root.Bytes);
            var refusals = root.Refusals;
            var refused = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(fixture.Encoded, options));
            Assert.Equal(ImageLimitScope.SharedBudget, refused.Scope);
            Assert.Equal(refusals + 1, root.Refusals);
            Assert.Equal(frameBytes, root.Bytes);
            Assert.Equal(fixture.Source, OriginalPixels.Bytes(first));
        }
        Assert.Equal(0, root.Bytes);
        using var cancellation = new CancellationTokenSource();
        root.Admitted = bytes => { if (bytes >= frameBytes) cancellation.Cancel(); };
        Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.Decode(fixture.Encoded, options, cancellation.Token));
        Assert.Equal(0, root.Bytes); root.Admitted = null;
        using (var recovered = ImageCodec.Decode(fixture.Encoded, options))
            Assert.Equal(fixture.Source, OriginalPixels.Bytes(recovered));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }
}
