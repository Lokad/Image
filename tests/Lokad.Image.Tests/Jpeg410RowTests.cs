using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Jpeg410RowTests
{
    [Theory]
    [InlineData(1)] [InlineData(3)]
    public void FractionalChromaKeepsBinary64PixelsClampsTailsAndSourceGuards(int topWeight)
    {
        var random = new Random(4102026);
        for (var trial = 0; trial < 4096; trial++)
        {
            var width = 1 + trial % 145; var chromaWidth = (width + 3) / 4;
            var planes = new byte[5][]; planes[0] = new byte[width + 2];
            for (var c = 1; c < 5; c++) planes[c] = new byte[chromaWidth + 2];
            foreach (var plane in planes) random.NextBytes(plane);
            if (trial < 256) planes[0].AsSpan(1, width).Fill((byte)trial);
            if (trial < 4)
                for (var c = 1; c < 5; c++) planes[c].AsSpan(1, chromaWidth).Fill((byte)(((trial + c) & 1) * 255));
            var source = planes.Select(p => p.ToArray()).ToArray();
            var result = Enumerable.Repeat((byte)173, width * 4 + 2).ToArray();
            JpegDecoder.RenderYcbcr410Row(planes[0].AsSpan(1, width), planes[1].AsSpan(1, chromaWidth), planes[2].AsSpan(1, chromaWidth),
                planes[3].AsSpan(1, chromaWidth), planes[4].AsSpan(1, chromaWidth), topWeight, result.AsSpan(1, width * 4));
            for (var x = 0; x < width; x++)
            {
                double Horizontal(byte[] values)
                {
                    var coordinate = (x + .5) / 4 - .5; var left = (int)Math.Floor(coordinate); var fraction = coordinate - left;
                    return values[1 + Math.Clamp(left, 0, chromaWidth - 1)] * (1 - fraction)
                        + values[1 + Math.Clamp(left + 1, 0, chromaWidth - 1)] * fraction;
                }
                var fractionTop = topWeight / 4d;
                var cb = Horizontal(planes[1]) * fractionTop + Horizontal(planes[2]) * (1 - fractionTop) - 128;
                var cr = Horizontal(planes[3]) * fractionTop + Horizontal(planes[4]) * (1 - fractionTop) - 128;
                var first = planes[0][x + 1];
                byte Q(double value) => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
                Assert.Equal(Q(first + 1.402 * cr), result[x * 4 + 1]);
                Assert.Equal(Q(first - .344136286 * cb - .714136286 * cr), result[x * 4 + 2]);
                Assert.Equal(Q(first + 1.772 * cb), result[x * 4 + 3]); Assert.Equal(255, result[x * 4 + 4]);
            }
            Assert.Equal(173, result[0]); Assert.Equal(173, result[^1]);
            for (var c = 0; c < 5; c++) Assert.Equal(source[c], planes[c]);
        }
    }
}
