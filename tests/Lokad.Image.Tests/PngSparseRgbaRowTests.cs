using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngSparseRgbaRowTests
{
    [Theory]
    [InlineData(4, false)]
    [InlineData(8, false)]
    [InlineData(8, true)]
    public void SparsePassesKeepChannelsHiddenSamplesHolesAndGuardBytes(int bytesPerPixel, bool reverse)
    {
        foreach (var width in new[] { 1, 2, 31, 4095, 4096, 4097 })
        foreach (var step in new[] { 2, 4, 8 })
        {
            var row = Enumerable.Range(0, width * bytesPerPixel).Select(i => (byte)(i * 73 + i / 17 + 19)).ToArray();
            row[bytesPerPixel - 1] = 0;
            if (bytesPerPixel == 8) row[6] = 0;
            var unchanged = row.ToArray();
            var length = ((width - 1) * step + 1) * bytesPerPixel;
            var output = Enumerable.Repeat((byte)0xa5, length + 32).ToArray(); var expected = output.ToArray();
            for (var x = 0; x < width; x++)
            for (var channelByte = 0; channelByte < bytesPerPixel; channelByte++)
                expected[16 + x * step * bytesPerPixel + channelByte] = row[x * bytesPerPixel + (reverse ? channelByte ^ 1 : channelByte)];
            PngCodec.CopySparseRgbaRow(row, output.AsSpan(16, length), bytesPerPixel, step, reverse, default);
            Assert.Equal(expected, output); Assert.Equal(unchanged, row);
        }
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(8, false)]
    [InlineData(8, true)]
    public void CancellationPrecedesTheFirstSparsePixelWrite(int bytesPerPixel, bool reverse)
    {
        var row = new byte[bytesPerPixel * 4097];
        var output = Enumerable.Repeat((byte)0xa5, bytesPerPixel * 8 * 4097).ToArray();
        var unchanged = output.ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            PngCodec.CopySparseRgbaRow(row, output, bytesPerPixel, 8, reverse, cancellation.Token));
        Assert.Equal(unchanged, output);
    }
}
