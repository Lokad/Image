using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngRowTests
{
    [Theory]
    [InlineData(false, 16376)] [InlineData(false, 16384)] [InlineData(false, 16392)]
    [InlineData(true, 16376)] [InlineData(true, 16384)] [InlineData(true, 16392)]
    public void RowCopyPreservesAllSamplesAndGuardsAcrossChunkBoundaries(bool reverse, int length)
    {
        // Original distinct high/low bytes, including alpha/hidden RGB slots.
        var row = Enumerable.Range(0, length).Select(i => (byte)((i * 137 + (i >> 4) * 29 + 17) & 255)).ToArray();
        var before = row.ToArray();
        var output = Enumerable.Repeat((byte)91, length + 16).ToArray();
        PngCodec.CopyRgbaRow(row, output.AsSpan(8, length), reverse, default);
        for (var i = 0; i < length; i++) Assert.Equal(before[reverse ? i ^ 1 : i], output[i + 8]);
        Assert.All(output[..8], value => Assert.Equal((byte)91, value));
        Assert.All(output[(length + 8)..], value => Assert.Equal((byte)91, value));
        Assert.Equal(before, row);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CanceledRowCopyLeavesItsDestinationUntouched(bool reverse)
    {
        byte[] row = [1, 2, 3, 4, 5, 6, 7, 8];
        var output = Enumerable.Repeat((byte)91, row.Length).ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => PngCodec.CopyRgbaRow(row, output, reverse, cancellation.Token));
        Assert.All(output, value => Assert.Equal((byte)91, value));
    }
}
