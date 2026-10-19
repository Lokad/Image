using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCmykRowTests
{
    [Fact]
    public void EveryByteProductHasExactIndependentDecimalRoundingAndOpaqueAlpha()
    {
        var planes = Enumerable.Range(0, 4).Select(_ => new byte[18]).ToArray(); var output = new byte[66];
        foreach (var inverted in new[] { false, true })
        for (var k = 0; k < 256; k++)
        for (var start = 0; start < 256; start += 16)
        {
            for (var lane = 0; lane < 16; lane++)
            {
                planes[0][lane + 1] = (byte)(start + lane); planes[1][lane + 1] = (byte)(255 - start - lane);
                planes[2][lane + 1] = (byte)((start + lane) * 37 + k); planes[3][lane + 1] = (byte)k;
            }
            var unchanged = planes.Select(p => p.ToArray()).ToArray(); Array.Fill(output, (byte)173);
            JpegDecoder.RenderCmykRow(planes[0].AsSpan(1, 16), planes[1].AsSpan(1, 16), planes[2].AsSpan(1, 16), planes[3].AsSpan(1, 16), inverted, output.AsSpan(1, 64));
            for (var lane = 0; lane < 16; lane++)
            for (var c = 0; c < 3; c++)
            {
                decimal channel = inverted ? planes[c][lane + 1] : 255 - planes[c][lane + 1];
                decimal black = inverted ? k : 255 - k;
                Assert.Equal((byte)decimal.Round(channel * black / 255, 0, MidpointRounding.AwayFromZero), output[lane * 4 + c + 1]);
            }
            for (var lane = 0; lane < 16; lane++) Assert.Equal(255, output[lane * 4 + 4]);
            Assert.Equal(173, output[0]); Assert.Equal(173, output[^1]);
            for (var c = 0; c < 4; c++) Assert.Equal(unchanged[c], planes[c]);
        }
    }

    [Fact]
    public void ScalarTailsKeepIndependentColorsAndDestinationGuards()
    {
        foreach (var width in new[] { 1, 7, 8, 15, 16, 17, 31, 513 })
        foreach (var inverted in new[] { false, true })
        {
            var planes = Enumerable.Range(0, 4).Select(c => Enumerable.Range(0, width).Select(i => (byte)(i * 73 + c * 37)).ToArray()).ToArray();
            var output = Enumerable.Repeat((byte)173, width * 4 + 2).ToArray();
            JpegDecoder.RenderCmykRow(planes[0], planes[1], planes[2], planes[3], inverted, output.AsSpan(1, width * 4));
            for (var x = 0; x < width; x++)
            for (var c = 0; c < 3; c++)
            {
                decimal channel = inverted ? planes[c][x] : 255 - planes[c][x]; decimal black = inverted ? planes[3][x] : 255 - planes[3][x];
                Assert.Equal((byte)decimal.Round(channel * black / 255, 0, MidpointRounding.AwayFromZero), output[x * 4 + c + 1]);
            }
            Assert.Equal(173, output[0]); Assert.Equal(173, output[^1]);
        }
    }
}
