using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngUnfilterTests
{
    [Fact]
    public void NonePreservesBytesAndGuardsWithoutReadingNeighbourRows()
    {
        foreach (var length in new[] { 0, 1, 7, 8, 65535, 65536, 65537 })
        {
            var bytes = Enumerable.Range(0, length + 8).Select(i => (byte)(53 * i + 7)).ToArray();
            var original = bytes.ToArray();
            PngCodec.Unfilter(bytes.AsSpan(3, length), [], 8, 0);
            Assert.Equal(original, bytes);
        }
    }

    [Fact]
    public void NoneChecksCancellationWithoutChangingPixels()
    {
        var bytes = Enumerable.Range(0, 65537).Select(i => (byte)i).ToArray(); var original = bytes.ToArray();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => PngCodec.Unfilter(bytes, [], 4, 0, cancellation.Token));
        Assert.Equal(original, bytes);
    }
}
