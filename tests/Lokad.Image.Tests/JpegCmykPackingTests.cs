using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCmykPackingTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void EverySampleByteKeepsFourPlaneOrderBlackInversionAndGuards(bool invert)
    {
        foreach (var width in new[] { 1, 31, 32, 33, 65, 513 })
        for (var start = 0; start < 256; start++)
        {
            var planes = Enumerable.Range(0, 4).Select(c => Enumerable.Range(0, width + 2).Select(x => (byte)(start + x * (2 * c + 1) + c * 61)).ToArray()).ToArray();
            var before = planes.Select(p => p.ToArray()).ToArray(); var destination = Enumerable.Repeat((byte)173, width * 4 + 2).ToArray();
            JpegDecoder.PackCmykSamples(planes[0].AsSpan(1, width), planes[1].AsSpan(1, width), planes[2].AsSpan(1, width), planes[3].AsSpan(1, width), invert, destination.AsSpan(1, width * 4));
            for (var x = 0; x < width; x++)
            for (var c = 0; c < 4; c++) Assert.Equal(invert ? (byte)(255 - planes[c][x + 1]) : planes[c][x + 1], destination[1 + x * 4 + c]);
            Assert.Equal(173, destination[0]); Assert.Equal(173, destination[^1]);
            for (var c = 0; c < 4; c++) Assert.Equal(before[c], planes[c]);
        }
    }
}
