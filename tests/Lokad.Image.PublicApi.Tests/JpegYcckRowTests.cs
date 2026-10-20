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
        double Sample(int component, int x, int y)
        {
            if (component is 0 or 3) return (basis[component] + x / 8 * 13 + y / 8 * 29) & 255;
            var sx = x / 2; var sy = y / 2; var left = (x & 1) == 0 ? sx - 1 : sx; var top = (y & 1) == 0 ? sy - 1 : sy;
            var wx = (x & 1) == 0 ? 1 : 3; var wy = (y & 1) == 0 ? 1 : 3;
            int At(int xx, int yy) => (basis[component] + Math.Clamp(xx, 0, (width - 1) / 2) / 8 * 13 + Math.Clamp(yy, 0, (height - 1) / 2) / 8 * 29) & 255;
            return (At(left, top) * wx * wy + At(left + 1, top) * (4 - wx) * wy
                + At(left, top + 1) * wx * (4 - wy) + At(left + 1, top + 1) * (4 - wx) * (4 - wy)) / 16d;
        }
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var luma = Sample(0, x, y); var cb = Sample(1, x, y) - 128; var cr = Sample(2, x, y) - 128;
            var c = 255 - (luma + 1.402 * cr); var m = 255 - (luma - .344136286 * cb - .714136286 * cr);
            var yellow = 255 - (luma + 1.772 * cb); var k = 255 - Sample(3, x, y);
            c = 255 - c; m = 255 - m; yellow = 255 - yellow;
            double[] colors = [c, m, yellow];
            for (var channel = 0; channel < 3; channel++)
            {
                var expectedSample = (byte)Math.Clamp(Math.Round((255 - colors[channel]) * (255 - k) / 255, MidpointRounding.AwayFromZero), 0, 255);
                Assert.Equal(expectedSample, first.GetRow8(y)[x * 4 + channel]);
                Assert.Equal(expectedSample, caller[y * stride + x * 3 + channel]); Assert.Equal(expectedSample, callerSurvivor[y * stride + x * 3 + channel]);
            }
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
