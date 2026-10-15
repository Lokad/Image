using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegRgb420RowTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(7)]
    [InlineData(8)] [InlineData(9)] [InlineData(15)] [InlineData(16)]
    [InlineData(17)] [InlineData(31)] [InlineData(513)] [InlineData(4097)]
    public void CenteredTwoDimensionalPlanesMatchOriginalBinary64WithGuards(int width)
    {
        var count = (width + 1) / 2;
        var red = new byte[width + 2]; var gt = new byte[count + 2]; var gb = new byte[count + 2];
        var bt = new byte[count + 2]; var bb = new byte[count + 2];
        for (var x = 0; x < width; x++) red[x + 1] = unchecked((byte)(x * 73 + 19));
        for (var k = 0; k < count; k++)
        {
            gt[k + 1] = unchecked((byte)(k * 101 ^ 139)); gb[k + 1] = unchecked((byte)(k * 73 + 2));
            bt[k + 1] = k % 2 == 0 ? (byte)0 : (byte)255; bb[k + 1] = unchecked((byte)(255 - k * 17));
        }
        var sources = new[] { red, gt, gb, bt, bb }; var original = sources.Select(a => a.ToArray()).ToArray();
        foreach (var weight in new[] { 1, 3 })
        foreach (var clamped in new[] { false, true })
        {
            var lowerGreen = clamped ? gt : gb; var lowerBlue = clamped ? bt : bb;
            var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
            JpegDecoder.RenderRgb420Row(red.AsSpan(1, width), gt.AsSpan(1, count), lowerGreen.AsSpan(1, count),
                bt.AsSpan(1, count), lowerBlue.AsSpan(1, count), weight, output.AsSpan(5, width * 4));
            for (var x = 0; x < width; x++)
            {
                var coordinate = (x + 0.5) / 2 - 0.5; var left = (int)Math.Floor(coordinate);
                var fraction = coordinate - left; var fy = (4 - weight) / 4.0;
                var a = Math.Clamp(left, 0, count - 1) + 1; var b = Math.Clamp(left + 1, 0, count - 1) + 1;
                byte Reference(byte[] upper, byte[] lower)
                {
                    var top = upper[a] * (1 - fraction) + upper[b] * fraction;
                    var bottom = lower[a] * (1 - fraction) + lower[b] * fraction;
                    return (byte)Math.Round(top * (1 - fy) + bottom * fy, MidpointRounding.AwayFromZero);
                }
                Assert.Equal(red[x + 1], output[5 + x * 4]); Assert.Equal(Reference(gt, lowerGreen), output[6 + x * 4]);
                Assert.Equal(Reference(bt, lowerBlue), output[7 + x * 4]); Assert.Equal(255, output[8 + x * 4]);
            }
            Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
            Assert.All(output.Skip(5 + width * 4), value => Assert.Equal((byte)0xa5, value));
        }
        for (var i = 0; i < sources.Length; i++) Assert.Equal(original[i], sources[i]);
    }

    [Fact]
    public void FractionalRowsAreNotRoundedBeforeTheVerticalCombination()
    {
        byte[] red = [19, 73, 211], top = [0, 2], bottom = [0, 1], output = new byte[12];
        JpegDecoder.RenderRgb420Row(red, top, bottom, top, bottom, 3, output);
        // At x1: upper=0.5, lower=0.25; 3/4*0.5+1/4*0.25=0.4375.
        // Quantizing each intermediate row would incorrectly produce one.
        Assert.Equal(new byte[] { 19, 0, 0, 255, 73, 0, 0, 255, 211, 1, 1, 255 }, output);
    }
}
