using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegRgbFrameStoreTests
{
    [Fact]
    public void ComponentStoresMatchIndependentBytePlacementAndPreserveOtherChannelsAndGuards()
    {
        var random = new Random(0x346b19);
        foreach (var channels in new[] { 3, 4 })
        for (var width = 1; width <= 8; width++)
        for (var height = 1; height <= 8; height++)
        for (var channel = 0; channel < channels; channel++)
        foreach (var padding in new[] { 0, 13 })
        {
            var stride = width * 4 + padding; var length = (height - 1) * stride + width * 4;
            var source = new byte[73]; random.NextBytes(source); var saved = source.ToArray();
            var actual = new byte[length + 11]; random.NextBytes(actual); var expected = actual.ToArray();
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                expected[3 + y * stride + x * 4 + channel] = source[5 + y * 8 + x];
                if (channels == 3 && channel == 2) expected[3 + y * stride + x * 4 + 3] = 255;
            }
            JpegDecoder.StoreComponentBlock(source.AsSpan(5, 64), actual.AsSpan(3, length), stride, width, height, channel, channels);
            Assert.Equal(expected, actual); Assert.Equal(saved, source);
        }
    }

    [Fact]
    public void BlockStoresAllocateNoAdditionalStorage()
    {
        var source = new byte[64]; var output = new byte[8 * 43];
        for (var i = 0; i < 4096; i++) JpegDecoder.StoreComponentBlock(source, output, 43, 8, 8, i % 3, 3);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 4096; i++) JpegDecoder.StoreComponentBlock(source, output, 43, 8, 8, i % 3, 3);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(output); Assert.Equal(0, allocated);
    }
}
