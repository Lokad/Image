using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WebpBitReaderRefillTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(7)] [InlineData(8)]
    [InlineData(9)] [InlineData(31)] [InlineData(72)]
    public void EveryFieldWidthAndPhaseMatchesIndependentBitsThroughExactEnd(int length)
    {
        var input = Enumerable.Range(0, length).Select(i => (byte)(i * 73 + 19)).ToArray();
        var unchanged = input.ToArray();
        for (var phase = 0; phase < 8 && phase <= length * 8; phase++)
        for (var width = 1; width <= 24; width++)
        {
            using var context = new OperationContext(ImageLimits.Default, default);
            var reader = new WebpBitReader(input, context); var consumed = 0;
            Assert.Equal(Field(input, consumed, phase), reader.Read(phase)); consumed += phase;
            Assert.Equal(0, reader.Read(0));
            while (length * 8 - consumed >= width)
            {
                Assert.Equal(Field(input, consumed, width), reader.Read(width)); consumed += width;
            }
            var rest = length * 8 - consumed;
            Assert.Equal(Field(input, consumed, rest), reader.Read(rest));
            try { reader.Read(1); Assert.Fail("A refill must not expose bytes beyond the payload."); }
            catch (ImageDataException) { }
        }
        Assert.Equal(unchanged, input);
    }

    [Theory]
    [InlineData(9)] [InlineData(12)] [InlineData(15)]
    public void LongSymbolsKeepEveryTailAndRejectIncompleteCodes(int width)
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        var tree = new WebpHuffman(Enumerable.Repeat((byte)width, 1 << width).ToArray(), context);
        for (var length = 0; length <= 17; length++)
        for (var phase = 0; phase < 8 && phase <= length * 8; phase++)
        {
            var reader = new WebpBitReader(Enumerable.Repeat((byte)255, length).ToArray(), context);
            Assert.Equal((1 << phase) - 1, reader.Read(phase));
            for (var symbol = 0; symbol < (length * 8 - phase) / width; symbol++)
                Assert.Equal((1 << width) - 1, tree.Read(ref reader));
            try { tree.Read(ref reader); Assert.Fail("An incomplete canonical code must fail."); }
            catch (ImageDataException) { }
        }
    }

    [Fact]
    public void CanceledWordLookaheadKeepsSpeculativeStatePrivate()
    {
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var shortTree = new WebpHuffman(Enumerable.Repeat((byte)8, 256).ToArray(), context);
        var longTree = new WebpHuffman(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 15 }, context);
        var input = Enumerable.Repeat((byte)255, 16).ToArray();
        var shortReader = new WebpBitReader(input, context); var longReader = new WebpBitReader(input, context);
        cancellation.Cancel();
        try { shortTree.Read(ref shortReader); Assert.Fail("Short lookup must observe cancellation."); }
        catch (OperationCanceledException) { }
        try { longTree.Read(ref longReader); Assert.Fail("Long lookup must observe cancellation."); }
        catch (OperationCanceledException) { }
        Assert.Equal(0, shortReader.Position); Assert.Equal(0, longReader.Position);
        Assert.All(input, value => Assert.Equal((byte)255, value));
    }

    private static int Field(byte[] input, int first, int count)
    {
        var value = 0;
        for (var bit = 0; bit < count; bit++) value |= (input[(first + bit) / 8] >> ((first + bit) % 8) & 1) << bit;
        return value;
    }
}
