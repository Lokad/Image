using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WebpLosslessPublicationTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(3)] [InlineData(4)]
    [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(255)]
    [InlineData(256)] [InlineData(257)] [InlineData(513)]
    public void PublicationPreservesAllChannelsAcrossVectorAndCheckpointTails(int length)
    {
        var random = new Random(6476 + length);
        var storage = new uint[length + 2]; var expected = new uint[length];
        storage[0] = 0x13579bdf; storage[^1] = 0x2468ace0;
        for (var i = 0; i < length; i++)
        {
            // Independent channel oracle; force wraparound and hidden RGB.
            var b = random.Next(256); var g = random.Next(256); var r = random.Next(256);
            var a = i % 3 == 0 ? 0 : random.Next(256);
            if (i % 7 == 0) { b = 255; g = 255; r = 0; }
            storage[i + 1] = (uint)(a << 24 | r << 16 | g << 8 | b);
            var rgba = new byte[] { (byte)((r + g) % 256), (byte)g, (byte)((b + g) % 256), (byte)a };
            expected[i] = (uint)rgba[0] | (uint)rgba[1] << 8 | (uint)rgba[2] << 16 | (uint)rgba[3] << 24;
        }
        using var context = new OperationContext(ImageLimits.Default, default);
        WebpLosslessDecoder.PublishSubtractGreenRgba(storage.AsSpan(1, length), context);
        Assert.True(expected.AsSpan().SequenceEqual(storage.AsSpan(1, length)));
        Assert.Equal(0x13579bdfu, storage[0]); Assert.Equal(0x2468ace0u, storage[^1]);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; i++)
            WebpLosslessDecoder.PublishSubtractGreenRgba(storage.AsSpan(1, length), context);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void CancellationPrecedesPublicationWritesIncludingEmptyRows()
    {
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        cancellation.Cancel();
        uint[] row = [0x0012ff80, 0xff34ff70]; var original = row.ToArray();
        Assert.Throws<OperationCanceledException>(() => WebpLosslessDecoder.PublishSubtractGreenRgba(row, context));
        Assert.Throws<OperationCanceledException>(() => WebpLosslessDecoder.PublishSubtractGreenRgba([], context));
        Assert.Equal(original, row);
    }
}
