using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Jpeg420ColorRowTests
{
    [Theory]
    [InlineData(0,2)] [InlineData(0,3)] [InlineData(1,2)] [InlineData(1,3)]
    [InlineData(2,2)] [InlineData(2,3)] [InlineData(3,2)] [InlineData(3,3)]
    [InlineData(4,2)] [InlineData(4,3)] [InlineData(5,2)] [InlineData(5,3)]
    [InlineData(6,2)] [InlineData(6,3)] [InlineData(7,2)] [InlineData(7,3)]
    public void CenteredQuarterInterpolationKeepsBinary64RgbAndGuards(int phase, int y)
    {
        for (var seed = 0; seed < 256; seed++)
        {
            var inputs = Enumerable.Range(0, 5).Select(component => Enumerable.Range(0, 10)
                .Select(i => (byte)(phase switch
                {
                    0 => 0, 1 => 255, 2 => 128,
                    3 => (i + component) % 2 == 0 ? 0 : 255,
                    _ => i * (53 + phase * 2) + component * 101 + seed * (phase * 2 + 1)
                })).ToArray()).ToArray();
            var originals = inputs.Select(value => value.ToArray()).ToArray();
            var rgba = Enumerable.Repeat((byte)123, 34).ToArray(); var scratch = Enumerable.Repeat((byte)123, 26).ToArray();
            JpegDecoder.RenderYcbcr420Row8(inputs[0].AsSpan(1,8), inputs[1].AsSpan(1,8), inputs[2].AsSpan(1,8),
                inputs[3].AsSpan(1,8), inputs[4].AsSpan(1,8), y % 2 == 0 ? 1 : 3, rgba.AsSpan(1,32), scratch.AsSpan(1,24));
            for (var lane = 0; lane < 8; lane++)
            {
                // Independent centered-coordinate binary64 recurrence, including
                // horizontal then vertical interpolation before color conversion.
                var cx = (lane + 2 + 0.5) / 2 - 0.5; var cy = (y + 0.5) / 2 - 0.5;
                var left = (int)Math.Floor(cx); var fx = cx - left; var fy = cy - Math.Floor(cy);
                double Interpolate(int upper, int lower)
                {
                    var top = inputs[upper][left+1] * (1-fx) + inputs[upper][left+2] * fx;
                    var bottom = inputs[lower][left+1] * (1-fx) + inputs[lower][left+2] * fx;
                    return top * (1-fy) + bottom * fy;
                }
                var luma = inputs[0][lane+1]; var cb = Interpolate(1,2)-128; var cr = Interpolate(3,4)-128;
                Assert.Equal(Round(luma + 1.402 * cr), rgba[1+lane*4]);
                Assert.Equal(Round(luma - 0.344136286 * cb - 0.714136286 * cr), rgba[2+lane*4]);
                Assert.Equal(Round(luma + 1.772 * cb), rgba[3+lane*4]); Assert.Equal(255, rgba[4+lane*4]);
            }
            for (var i = 0; i < inputs.Length; i++) Assert.Equal(originals[i], inputs[i]);
            Assert.Equal(123,rgba[0]); Assert.Equal(123,rgba[^1]); Assert.Equal(123,scratch[0]); Assert.Equal(123,scratch[^1]);
        }
    }

    private static byte Round(double value) => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero),0,255);
}
