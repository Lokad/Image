using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegYcckRowTests
{
    [Fact]
    public void FractionalChromaAndBlackKeepOriginalBinary64PixelsAndGuards()
    {
        var random = new Random(74021); var planes = Enumerable.Range(0, 6).Select(_ => new byte[10]).ToArray();
        var output = new byte[34]; var scratch = new byte[26];
        foreach (var topWeight in new[] { 1, 3 })
        for (var trial = 0; trial < 1024; trial++)
        {
            foreach (var plane in planes) random.NextBytes(plane);
            // Include every input luma/black byte and opposing saturated chroma.
            if (trial < 256)
                for (var i = 1; i <= 8; i++) { planes[0][i] = (byte)trial; planes[1][i] = (byte)(255 - trial); }
            if (trial < 4)
                for (var c = 2; c < 6; c++) Array.Fill(planes[c], (byte)(((trial + c) & 1) * 255));
            var before = planes.Select(p => p.ToArray()).ToArray(); Array.Fill(output, (byte)173); Array.Fill(scratch, (byte)197);
            JpegDecoder.RenderYcck420Row8(planes[0].AsSpan(1, 8), planes[1].AsSpan(1, 8), planes[2].AsSpan(1, 8), planes[3].AsSpan(1, 8),
                planes[4].AsSpan(1, 8), planes[5].AsSpan(1, 8), topWeight, output.AsSpan(1, 32), scratch.AsSpan(1, 24));
            for (var x = 0; x < 8; x++)
            {
                double Chroma(int component)
                {
                    var left = x / 2 + (x & 1) + 1; var fx = (x & 1) == 0 ? .75 : .25;
                    return (planes[component][left] * (1 - fx) + planes[component][left + 1] * fx) * (topWeight / 4d)
                        + (planes[component + 1][left] * (1 - fx) + planes[component + 1][left + 1] * fx) * (1 - topWeight / 4d) - 128;
                }
                var luma = planes[0][x + 1]; var cb = Chroma(2); var cr = Chroma(4);
                var cyan = 255 - (luma + 1.402 * cr); var magenta = 255 - (luma - .344136286 * cb - .714136286 * cr);
                var yellow = 255 - (luma + 1.772 * cb); double black = planes[1][x + 1];
                cyan = 255 - cyan; magenta = 255 - magenta; yellow = 255 - yellow; black = 255 - black;
                double[] colors = [cyan, magenta, yellow];
                for (var c = 0; c < 3; c++)
                    Assert.Equal((byte)Math.Clamp(Math.Round((255 - colors[c]) * (255 - black) / 255, MidpointRounding.AwayFromZero), 0, 255), output[x * 4 + c + 1]);
                Assert.Equal(255, output[x * 4 + 4]);
            }
            for (var c = 0; c < 6; c++) Assert.Equal(before[c], planes[c]);
            Assert.Equal(173, output[0]); Assert.Equal(173, output[^1]); Assert.Equal(197, scratch[0]); Assert.Equal(197, scratch[^1]);
        }
    }
}
