using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegYcckRowTests
{
    [Theory]
    [InlineData(1, 3, false)] [InlineData(17, 9, true)] [InlineData(65, 31, false)] [InlineData(66, 33, true)]
    public void VaryingSubsampledYcckKeepsBordersBlackAndIndependentOutputs(int width, int height, bool progressive)
    {
        byte[] basis = [17, 191, 250, 63];
        var packet = AuthoredJpegCompatibility.Create(width, height, basis, 2, 2, true, 2, progressive, restart: 1, vary: true);
        var unchanged = packet.ToArray(); using var first = ImageCodec.Decode(packet); using var survivor = ImageCodec.Decode(packet);
        var stride = width * 3 + 7;
        var caller = Enumerable.Repeat((byte)173, stride * height + 17).ToArray(); var callerSurvivor = caller.ToArray();
        ImageCodec.DecodeJpeg(packet, caller, JpegSampleFormat.Rgb, stride); ImageCodec.DecodeJpeg(packet, callerSurvivor, JpegSampleFormat.Rgb, stride);
        JpegFourComponentExactTests.AssertNative(packet, OriginalPixels.Bytes(first));
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var native = first.GetRow8(y).Slice(x * 4, 3).ToArray();
            Assert.Equal(native, caller.AsSpan(y * stride + x * 3, 3).ToArray());
            Assert.Equal(native, callerSurvivor.AsSpan(y * stride + x * 3, 3).ToArray());
            Assert.Equal(255, first.GetRow8(y)[x * 4 + 3]);
        }
        var expected = OriginalPixels.Bytes(survivor); Assert.Equal(expected, OriginalPixels.Bytes(first));
        for (var y = 0; y < height; y++) first.GetRow8(y).Fill(173);
        first.Dispose(); Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        using var subsequent = ImageCodec.Decode(packet);
        Assert.Equal(expected, OriginalPixels.Bytes(subsequent));
        subsequent.GetRow8(0).Fill(81); Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        for (var y = 0; y < height; y++) Assert.All(caller.AsSpan(y * stride + width * 3, 7).ToArray(), b => Assert.Equal(173, b));
        Assert.All(caller.AsSpan(stride * height).ToArray(), b => Assert.Equal(173, b));
        var expectedCaller = callerSurvivor.ToArray(); Array.Clear(caller); Assert.Equal(expectedCaller, callerSurvivor);
        Assert.Equal(unchanged, packet); Array.Clear(packet); Assert.Equal(expected, OriginalPixels.Bytes(survivor));
        Assert.Equal(expectedCaller, callerSurvivor);
    }
}
