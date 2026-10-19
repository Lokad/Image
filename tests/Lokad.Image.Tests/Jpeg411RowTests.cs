using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Jpeg411RowTests
{
    [Fact]
    public void QuarterWidthRowsKeepBinary64PixelsClampsTailsAndSourceGuards()
    {
        var random = new Random(4112026);
        for (var trial = 0; trial < 4096; trial++)
        {
            var width = 1 + trial % 145; var chromaWidth = (width + 3) / 4;
            var luma = new byte[width + 2]; var cb = new byte[chromaWidth + 2]; var cr = new byte[chromaWidth + 2];
            random.NextBytes(luma); random.NextBytes(cb); random.NextBytes(cr);
            if (trial < 256) luma.AsSpan(1, width).Fill((byte)trial);
            if (trial < 4) { cb.AsSpan(1, chromaWidth).Fill((byte)((trial & 1) * 255)); cr.AsSpan(1, chromaWidth).Fill((byte)(((trial >> 1) & 1) * 255)); }
            var source = new[] { luma.ToArray(), cb.ToArray(), cr.ToArray() };
            var result = Enumerable.Repeat((byte)173, width * 4 + 2).ToArray();
            JpegDecoder.RenderYcbcr411Row(luma.AsSpan(1, width), cb.AsSpan(1, chromaWidth), cr.AsSpan(1, chromaWidth), result.AsSpan(1, width * 4));
            for (var x = 0; x < width; x++)
            {
                double Chroma(byte[] values)
                {
                    var coordinate = (x + .5) / 4 - .5; var left = (int)Math.Floor(coordinate); var fraction = coordinate - left;
                    return values[1 + Math.Clamp(left, 0, chromaWidth - 1)] * (1 - fraction)
                        + values[1 + Math.Clamp(left + 1, 0, chromaWidth - 1)] * fraction - 128;
                }
                var second = Chroma(cb); var third = Chroma(cr); var first = luma[x + 1];
                byte Q(double value) => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
                Assert.Equal(Q(first + 1.402 * third), result[x * 4 + 1]);
                Assert.Equal(Q(first - .344136286 * second - .714136286 * third), result[x * 4 + 2]);
                Assert.Equal(Q(first + 1.772 * second), result[x * 4 + 3]); Assert.Equal(255, result[x * 4 + 4]);
            }
            Assert.Equal(173, result[0]); Assert.Equal(173, result[^1]);
            Assert.Equal(source[0], luma); Assert.Equal(source[1], cb); Assert.Equal(source[2], cr);
        }
    }
}
