using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpOverwriteOwnershipTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CompleteLosslessResultsOverwriteDirtyStorageAndKeepIndependentOwners(bool governed)
    {
        var fixture = OriginalExif.Create(MetadataProfile.WebpLosslessAlpha, 19, 13);
        var unchanged = fixture.Encoded.ToArray();
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var options = new DecodeOptions { Limits = governed ? new() { MemoryBudget = root } : ImageLimits.Default };
        var frameBytes = fixture.Width * fixture.Height * 4;
        using (var dirty = ImageCodec.Decode(fixture.Encoded, options))
            for (var y = 0; y < dirty.Height; y++) dirty.GetRow8(y).Fill(0xA7);
        Assert.Equal(0, root.Bytes);
        using var first = ImageCodec.Decode(fixture.Encoded, options);
        using var second = ImageCodec.Decode(fixture.Encoded, options);
        Assert.Equal(fixture.Source, OriginalPixels.Bytes(first));
        Assert.Equal(fixture.Source, OriginalPixels.Bytes(second));
        Assert.Equal(governed ? 2 * frameBytes : 0, root.Bytes);
        first.GetRow8(0)[0] ^= 255;
        Assert.Equal(fixture.Source, OriginalPixels.Bytes(second));
        first.Dispose();
        Assert.Equal(governed ? frameBytes : 0, root.Bytes);
        using var third = ImageCodec.Decode(fixture.Encoded, options);
        Assert.Equal(fixture.Source, OriginalPixels.Bytes(third));
        Assert.Equal(fixture.Source, OriginalPixels.Bytes(second));
        second.Dispose();
        Assert.Equal(governed ? frameBytes : 0, root.Bytes);
        third.Dispose();
        Assert.Equal(0, root.Bytes);
        Assert.False(root.IsDisposed);
        Assert.Equal(unchanged, fixture.Encoded);
    }
}
