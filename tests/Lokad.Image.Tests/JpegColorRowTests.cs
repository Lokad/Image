using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegColorRowTests
{
    [Fact]
    public void EveryFullResolutionComponentTripleKeepsOriginalRgbAndOpaqueAlpha()
    {
        var first = new byte[8]; var second = new byte[8]; var third = new byte[8];
        var rgba = new byte[32]; var scratch = new byte[24];
        for (var y = 0; y < 256; y++)
        for (var cb = 0; cb < 256; cb++)
        for (var cr = 0; cr < 256; cr += 8)
        {
            Array.Fill(first, (byte)y); Array.Fill(second, (byte)cb);
            for (var lane = 0; lane < 8; lane++) third[lane] = (byte)(cr + lane);
            JpegDecoder.RenderYcbcrRow8(first, second, third, rgba, scratch);
            for (var lane = 0; lane < 8; lane++)
            {
                var r = OriginalRound(y + 1.402 * (cr + lane - 128));
                var g = OriginalRound(y - 0.344136286 * (cb - 128) - 0.714136286 * (cr + lane - 128));
                var b = OriginalRound(y + 1.772 * (cb - 128));
                if (rgba[lane * 4] != r) Assert.Equal(r, rgba[lane * 4]);
                if (rgba[lane * 4 + 1] != g) Assert.Equal(g, rgba[lane * 4 + 1]);
                if (rgba[lane * 4 + 2] != b) Assert.Equal(b, rgba[lane * 4 + 2]);
                if (rgba[lane * 4 + 3] != 255) Assert.Equal(255, rgba[lane * 4 + 3]);
            }
        }
    }

    [Fact]
    public void UnalignedMixedLanesKeepInputsAndDestinationScratchGuards()
    {
        for (var phase = 0; phase < 8; phase++)
        {
            var first = Enumerable.Repeat((byte)123, 10).ToArray();
            var second = first.ToArray(); var third = first.ToArray();
            for (var lane = 0; lane < 8; lane++)
            {
                first[lane + 1] = (byte)((lane * 73 + phase * 31) & 255);
                second[lane + 1] = (byte)((lane * 17 + phase * 101) & 255);
                third[lane + 1] = (byte)((lane * 101 + phase * 73) & 255);
            }
            var originalFirst = first.ToArray(); var originalSecond = second.ToArray(); var originalThird = third.ToArray();
            var rgba = Enumerable.Repeat((byte)123, 34).ToArray(); var scratch = Enumerable.Repeat((byte)123, 26).ToArray();
            JpegDecoder.RenderYcbcrRow8(first.AsSpan(1, 8), second.AsSpan(1, 8), third.AsSpan(1, 8), rgba.AsSpan(1, 32), scratch.AsSpan(1, 24));
            for (var lane = 0; lane < 8; lane++)
            {
                var y = first[lane + 1]; var cb = second[lane + 1] - 128; var cr = third[lane + 1] - 128;
                Assert.Equal(OriginalRound(y + 1.402 * cr), rgba[1 + lane * 4]);
                Assert.Equal(OriginalRound(y - 0.344136286 * cb - 0.714136286 * cr), rgba[2 + lane * 4]);
                Assert.Equal(OriginalRound(y + 1.772 * cb), rgba[3 + lane * 4]); Assert.Equal(255, rgba[4 + lane * 4]);
            }
            Assert.Equal(originalFirst, first); Assert.Equal(originalSecond, second); Assert.Equal(originalThird, third);
            Assert.Equal(123, rgba[0]); Assert.Equal(123, rgba[^1]); Assert.Equal(123, scratch[0]); Assert.Equal(123, scratch[^1]);
        }
    }

    [Theory]
    [InlineData(7,8,8,32,24)] [InlineData(9,8,8,32,24)] [InlineData(8,7,8,32,24)] [InlineData(8,9,8,32,24)]
    [InlineData(8,8,7,32,24)] [InlineData(8,8,9,32,24)] [InlineData(8,8,8,31,24)] [InlineData(8,8,8,33,24)]
    [InlineData(8,8,8,32,23)] [InlineData(8,8,8,32,25)]
    public void IncompleteRowsRejectBeforeChangingOutputOrScratch(int first, int second, int third, int output, int scratchLength)
    {
        var rgba = Enumerable.Repeat((byte)123, output).ToArray(); var scratch = Enumerable.Repeat((byte)123, scratchLength).ToArray();
        Assert.Throws<ArgumentException>(() => JpegDecoder.RenderYcbcrRow8(new byte[first], new byte[second], new byte[third], rgba, scratch));
        Assert.All(rgba, value => Assert.Equal(123, value)); Assert.All(scratch, value => Assert.Equal(123, value));
    }

    private static byte OriginalRound(double value) => (byte)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
}
