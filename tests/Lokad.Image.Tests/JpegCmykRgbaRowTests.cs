using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCmykRgbaRowTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryColorBlackPairKeepsIndependentChannelsAndOpaqueAlpha(bool inverted)
    {
        var cyan = new byte[18]; var magenta = new byte[18];
        var yellow = new byte[18]; var black = new byte[18];
        var output = Enumerable.Repeat((byte)173, 66).ToArray();
        for (var start = 0; start < 65536; start += 16)
        {
            for (var x = 0; x < 16; x++)
            {
                cyan[x + 1] = (byte)(start + x);
                magenta[x + 1] = (byte)(start + x + 17);
                yellow[x + 1] = (byte)(3 * (start + x) + 73);
                black[x + 1] = (byte)((start + x) >> 8);
            }
            JpegDecoder.RenderCmykRow(cyan.AsSpan(1, 16), magenta.AsSpan(1, 16),
                yellow.AsSpan(1, 16), black.AsSpan(1, 16), inverted, output.AsSpan(1, 64));
            for (var x = 0; x < 16; x++)
            {
                Assert.Equal(Reference(cyan[x + 1], black[x + 1], inverted), output[1 + x * 4]);
                Assert.Equal(Reference(magenta[x + 1], black[x + 1], inverted), output[2 + x * 4]);
                Assert.Equal(Reference(yellow[x + 1], black[x + 1], inverted), output[3 + x * 4]);
                Assert.Equal(255, output[4 + x * 4]);
            }
        }
        Assert.Equal(173, output[0]); Assert.Equal(173, output[^1]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnalignedRowsAndTailsPreserveInputsGuardsAndZeroAllocation(bool inverted)
    {
        foreach (var width in new[] { 0, 1, 15, 16, 17, 31, 32, 33, 65 })
        {
            var planes = Enumerable.Range(0, 4).Select(c => Enumerable.Range(0, width + 2)
                .Select(x => (byte)(x * (2 * c + 1) + c * 61)).ToArray()).ToArray();
            var before = planes.Select(p => p.ToArray()).ToArray();
            var output = Enumerable.Repeat((byte)165, width * 4 + 2).ToArray();
            for (var warm = 0; warm < 32; warm++)
                JpegDecoder.RenderCmykRow(planes[0].AsSpan(1, width), planes[1].AsSpan(1, width),
                    planes[2].AsSpan(1, width), planes[3].AsSpan(1, width), inverted, output.AsSpan(1, width * 4));
            var allocated = GC.GetAllocatedBytesForCurrentThread();
            for (var repeat = 0; repeat < 128; repeat++)
                JpegDecoder.RenderCmykRow(planes[0].AsSpan(1, width), planes[1].AsSpan(1, width),
                    planes[2].AsSpan(1, width), planes[3].AsSpan(1, width), inverted, output.AsSpan(1, width * 4));
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
            for (var x = 0; x < width; x++)
            {
                for (var c = 0; c < 3; c++)
                    Assert.Equal(Reference(planes[c][x + 1], planes[3][x + 1], inverted), output[1 + x * 4 + c]);
                Assert.Equal(255, output[4 + x * 4]);
            }
            Assert.Equal(165, output[0]); Assert.Equal(165, output[^1]);
            for (var c = 0; c < 4; c++) Assert.Equal(before[c], planes[c]);
        }
    }

    private static byte Reference(byte sample, byte black, bool inverted)
    {
        var color = inverted ? sample : 255 - sample;
        var value = inverted ? black : 255 - black;
        return (byte)((long)color * value * 257 / 65536);
    }
}
