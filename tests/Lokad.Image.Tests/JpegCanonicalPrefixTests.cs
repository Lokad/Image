using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCanonicalPrefixTests
{
    [Theory]
    [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)] [InlineData(16)]
    public void LongCanonicalCodesKeepEveryCachedPhaseSymbolsPaddingAndWork(int length)
    {
        for (var offset = 0; offset < 8; offset++)
        {
            var context = Context(272 + (offset == 0 ? 0 : 1) + 3 * length);
            var counts = new byte[16]; counts[length - 1] = 3;
            var table = new JpegHuffman(counts, new byte[] { 17, 29, 43 }, context);
            var bits = new string('1', offset) + string.Concat(new[] { 0, 2, 1 }.Select(value => Convert.ToString(value, 2).PadLeft(length, '0')));
            var data = Stuff(bits); var reader = new JpegEntropyReader(data, 0, context);
            if (offset != 0) Assert.Equal((1 << offset) - 1, reader.ReadBits(offset));
            foreach (var expected in new[] { 17, 43, 29 }) Assert.Equal(expected, table.Read(ref reader));
            Assert.Equal(data.Length - 2, reader.Position); reader.Restart(0); Assert.Equal(data.Length, reader.Position);
            Assert.Throws<ImageLimitException>(() => context.Work(1));
        }
    }

    [Fact]
    public void EveryPartialSixteenBitBudgetKeepsConsumedBitsAndBytePosition()
    {
        for (var offset = 0; offset < 8; offset++)
        for (var budget = 0; budget < 16; budget++)
        {
            var context = Context(272 + (offset == 0 ? 0 : 1) + budget);
            var counts = new byte[16]; counts[15] = 1; var table = new JpegHuffman(counts, new byte[] { 17 }, context);
            var reader = new JpegEntropyReader(Stuff(new string('0', offset + 16)), 0, context);
            if (offset != 0) Assert.Equal(0, reader.ReadBits(offset));
            try { table.Read(ref reader); Assert.Fail("Incomplete code work must reject."); }
            catch (ImageLimitException error) { Assert.Equal("Computational work limit exceeded.", error.Message); }
            Assert.Equal((offset + budget + 7) / 8, reader.Position);
            Assert.Throws<ImageLimitException>(() => context.Work(1));
        }
    }

    [Fact]
    public void AssignedShortCodesCannotBeSkippedWhenTheirWorkBudgetIsTight()
    {
        for (var length = 1; length <= 8; length++)
        for (var offset = 0; offset < 8; offset++)
        {
            var context = Context(272 + (offset == 0 ? 0 : 1) + length - 1);
            var counts = new byte[16]; counts[length - 1] = 1; var table = new JpegHuffman(counts, new byte[] { 17 }, context);
            var reader = new JpegEntropyReader(Stuff(new string('0', offset + length)), 0, context);
            if (offset != 0) Assert.Equal(0, reader.ReadBits(offset));
            try { table.Read(ref reader); Assert.Fail("Partial short code work must reject."); }
            catch (ImageLimitException) { }
            Assert.Equal((offset + length - 1 + 7) / 8, reader.Position);
            Assert.Throws<ImageLimitException>(() => context.Work(1));
        }
    }

    [Theory]
    [InlineData(0, 1, 0, "Truncated JPEG entropy data.")]
    [InlineData(1, 1, 2, "Unexpected marker in JPEG entropy data.")]
    [InlineData(2, 1, 1, "Unexpected marker in JPEG entropy data.")]
    [InlineData(3, 9, 1, "Truncated JPEG entropy data.")]
    [InlineData(4, 9, 3, "Unexpected marker in JPEG entropy data.")]
    [InlineData(5, 9, 2, "Unexpected marker in JPEG entropy data.")]
    public void TruncatedOrMarkerPrefixesKeepOriginalFailurePositionAndWork(int scenario, int budget, int position, string message)
    {
        byte[] input = scenario switch { 0 => [], 1 => [255, 0xD0], 2 => [255], 3 => [0], 4 => [0, 255, 0xD0], _ => [0, 255] };
        var context = Context(272 + budget); var counts = new byte[16]; counts[15] = 1;
        var table = new JpegHuffman(counts, new byte[] { 17 }, context); var reader = new JpegEntropyReader(input, 0, context);
        try { table.Read(ref reader); Assert.Fail("Incomplete entropy must reject."); }
        catch (ImageDataException error) { Assert.Equal(message, error.Message); }
        Assert.Equal(position, reader.Position); Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void CachedPrefixesKeepMarkerVersusWorkFailurePrecedence()
    {
        for (var offset = 0; offset < 8; offset++)
        foreach (var markerWork in new[] { false, true })
        {
            var context = Context(272 + (offset == 0 ? 0 : 1) + 8 - offset + (markerWork ? 1 : 0));
            var counts = new byte[16]; counts[15] = 1; var table = new JpegHuffman(counts, new byte[] { 17 }, context);
            var reader = new JpegEntropyReader(new byte[] { 0, 255, 0xD0 }, 0, context);
            if (offset != 0) Assert.Equal(0, reader.ReadBits(offset));
            try { table.Read(ref reader); Assert.Fail("The code cannot cross a restart marker."); }
            catch (ImageLimitException) { Assert.False(markerWork); }
            catch (ImageDataException error) { Assert.True(markerWork); Assert.Equal("Unexpected marker in JPEG entropy data.", error.Message); }
            Assert.Equal(markerWork ? 3 : 1, reader.Position); Assert.Throws<ImageLimitException>(() => context.Work(1));
        }
    }

    private static OperationContext Context(long work) => new(ImageLimits.Default with { MaxWork = work }, default);
    private static byte[] Stuff(string bits)
    {
        bits = bits.PadRight((bits.Length + 7) / 8 * 8, '1'); var bytes = new List<byte>();
        for (var i = 0; i < bits.Length; i += 8) { var value = Convert.ToByte(bits.Substring(i, 8), 2); bytes.Add(value); if (value == 255) bytes.Add(0); }
        bytes.Add(255); bytes.Add(0xD0); return bytes.ToArray();
    }
}
