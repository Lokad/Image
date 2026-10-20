using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class Vp8AdaptiveEncodeTests
{
    [Theory] [InlineData(8)] [InlineData(16)]
    public void AdaptiveAndAdmittedFixedEncodersHaveIdenticalPixelsAndIndependentOwners(int depth)
    {
        using var source = OriginalPixels.Buffer(257, 129, depth); var unchanged = OriginalPixels.Bytes(source);
        using var adaptive = new MemoryStream(); using var fixedPacket = new MemoryStream();
        // Isolate entropy adaptation: higher efforts may also admit a lossy
        // reconstruction search whose pixels legitimately depend on local limits.
        var options = new WebpEncodeOptions { Lossless = false, Quality = 100, Effort = 3 };
        ImageCodec.EncodeWebp(source, adaptive, options);
        ImageCodec.EncodeWebp(source, fixedPacket, options with { Limits = new ImageLimits { MaxWorkingBytes = 1_000_000 } });
        var encoded = adaptive.ToArray(); Assert.True(encoded.Length < fixedPacket.Length);
        using var actual = ImageCodec.Decode(encoded); using var expected = ImageCodec.Decode(fixedPacket.ToArray());
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual)); Assert.Equal(unchanged, OriginalPixels.Bytes(source));
        source.Dispose(); encoded.AsSpan().Clear(); fixedPacket.GetBuffer().AsSpan().Clear();
        Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(actual));
    }
}
