using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegChromaPairRowTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)]
    public void SharedFlatteningKeepsIndependentScalarChromaTapOrderAndGuards(int rows)
    {
        foreach (var background in new uint?[] { null, 0, 0xFFFFFF, 0x204060, 0x0101FE })
        for (var phase = 0; phase < 256; phase++)
        {
            var top = new byte[66]; var bottom = new byte[66];
            for (var y = 0; y < 2; y++)
            for (var x = 0; x < 16; x++)
            for (var c = 0; c < 4; c++)
                (y == 0 ? top : bottom)[1 + x * 4 + c] = background is null && c == 3 ? (byte)255
                    : (byte)(phase * (c * 17 + 1) + x * (c * 31 + 73) + y * 127);
            var originalTop = top.ToArray(); var originalBottom = bottom.ToArray();
            var cb = Enumerable.Repeat((byte)123, 10).ToArray(); var cr = cb.ToArray();
            JpegEncoder.SampleSubsampledChromaPairRow8(top.AsSpan(1, 64), rows == 2 ? bottom.AsSpan(1, 64) : [],
                rows, background, cb.AsSpan(1, 8), cr.AsSpan(1, 8));
            for (var component = 1; component <= 2; component++)
            for (var x = 0; x < 8; x++)
            {
                double sum = 0; var color = background ?? 0;
                for (var y = 0; y < rows; y++)
                for (var dx = 0; dx < 2; dx++)
                {
                    var src = y == 0 ? top : bottom; var p = 1 + (x * 2 + dx) * 4; var a = src[p + 3];
                    var r = (src[p] * a + (int)(color >> 16 & 255) * (255 - a) + 127) / 255;
                    var g = (src[p + 1] * a + (int)(color >> 8 & 255) * (255 - a) + 127) / 255;
                    var b = (src[p + 2] * a + (int)(color & 255) * (255 - a) + 127) / 255;
                    sum += component == 1 ? 128 - .168735892 * r - .331264108 * g + .5 * b
                        : 128 + .5 * r - .418687589 * g - .081312411 * b;
                }
                Assert.Equal((byte)Math.Clamp(Math.Round(sum / (rows * 2), MidpointRounding.AwayFromZero), 0, 255),
                    (component == 1 ? cb : cr)[x + 1]);
            }
            foreach (var output in new[] { cb, cr }) { Assert.Equal(123, output[0]); Assert.Equal(123, output[^1]); }
            Assert.Equal(originalTop, top); Assert.Equal(originalBottom, bottom);
        }
    }

    [Theory]
    [InlineData(1)] [InlineData(2)]
    public void BothComponentsRoundOnlyAfterAveraging(int rows)
    {
        foreach (var channel in new[] { 0, 2 })
        {
            var top = new byte[64];
            for (var x = 0; x < 16; x++) { top[x * 4 + 3] = 255; if ((x & 1) != 0) top[x * 4 + channel] = 1; }
            var cb = new byte[8]; var cr = new byte[8];
            JpegEncoder.SampleSubsampledChromaPairRow8(top, rows == 2 ? top : [], rows, null, cb, cr);
            Assert.All(cb.Concat(cr), value => Assert.Equal((byte)128, value));
        }
    }

    [Fact]
    public void InvalidRowsFailBeforeWritingEitherOutput()
    {
        foreach (var (top, bottom, rows, cbSize, crSize) in new[] {
            (63, 0, 1, 8, 8), (64, 64, 1, 8, 8), (64, 63, 2, 8, 8),
            (64, 0, 0, 8, 8), (64, 0, 1, 7, 8), (64, 0, 1, 8, 9) })
        {
            var cb = Enumerable.Repeat((byte)123, cbSize).ToArray(); var cr = Enumerable.Repeat((byte)123, crSize).ToArray();
            Assert.Throws<ArgumentException>(() => JpegEncoder.SampleSubsampledChromaPairRow8(new byte[top], new byte[bottom], rows, 0, cb, cr));
            Assert.All(cb.Concat(cr), value => Assert.Equal((byte)123, value));
        }
    }

    [Fact]
    public void TransparencyWithoutBackgroundFailsInEveryLane()
    {
        for (var x = 0; x < 32; x++)
        {
            var top = Enumerable.Repeat((byte)255, 64).ToArray(); var bottom = top.ToArray();
            (x < 16 ? top : bottom)[(x % 16) * 4 + 3] = 254;
            var error = Assert.Throws<ArgumentException>(() => JpegEncoder.SampleSubsampledChromaPairRow8(top, bottom, 2, null, new byte[8], new byte[8]));
            Assert.Equal("image", error.ParamName);
        }
    }
}
