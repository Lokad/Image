using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegMixedCanonicalTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void MixedOneEightAndTenBitCodesKeepRequiredBytesFieldsAndRestart(int offset)
    {
        var counts = new byte[16]; counts[0] = counts[7] = counts[9] = 1;
        // Hand-derived canonical codes: 0, 10000000, 1000000100 (T.81 C.2).
        var context = Context(1296 + (offset == 0 ? 0 : 1) + 20 + 1);
        var tree = new JpegHuffman(counts, new byte[] { 17, 29, 43 }, context);
        var input = Stuff(new string('1', offset) + "0" + "10000000" + "1000000100" + new string('1', 16) + "0");
        var unchanged = input.ToArray(); var reader = new JpegEntropyReader(input, 0, context);
        if (offset != 0) Assert.Equal((1 << offset) - 1, reader.ReadBits(offset));
        Assert.Equal(17, tree.Read(ref reader)); Assert.Equal(29, tree.Read(ref reader)); Assert.Equal(43, tree.Read(ref reader));
        Assert.Equal(65535, reader.ReadBits(16)); Assert.Equal(17, tree.Read(ref reader));
        Assert.Equal(input.Length - 2, reader.Position); reader.Restart(0); Assert.Equal(input.Length, reader.Position);
        Assert.Throws<ImageLimitException>(() => context.Work(1)); Assert.Equal(unchanged, input);
    }

    [Fact]
    public void ShortCodesInLongTablesKeepEveryPartialBudgetAndCachedPhase()
    {
        for (var length = 1; length <= 8; length++)
        for (var offset = 0; offset < 8; offset++)
        for (var budget = 0; budget < length; budget++)
        {
            var context = Context(1296 + (offset == 0 ? 0 : 1) + budget);
            var counts = new byte[16]; counts[length - 1] = counts[9] = 1;
            var tree = new JpegHuffman(counts, new byte[] { 17, 29 }, context);
            var input = Stuff(new string('0', offset + length)); var unchanged = input.ToArray();
            var reader = new JpegEntropyReader(input, 0, context);
            if (offset != 0) Assert.Equal(0, reader.ReadBits(offset));
            try { tree.Read(ref reader); Assert.Fail("A partial short-code budget must reject."); }
            catch (ImageLimitException) { }
            Assert.Equal((offset + budget + 7) / 8, reader.Position);
            Assert.Throws<ImageLimitException>(() => context.Work(1)); Assert.Equal(unchanged, input);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void MixedShortNineAndTwelveBitCodesRetainFieldsStuffingPaddingAndRestart(int offset)
    {
        var counts = new byte[16]; counts[0] = counts[8] = counts[11] = 1;
        // Hand-derived canonical codes: 0, 100000000, 100000001000 (T.81 C.2).
        var context = Context(1296 + (offset == 0 ? 0 : 1) + 23 + 1);
        var tree = new JpegHuffman(counts, new byte[] { 17, 29, 43 }, context);
        var input = Stuff(new string('1', offset) + "0" + "100000000" + "100000001000" + new string('1', 16) + "0");
        var unchanged = input.ToArray(); var reader = new JpegEntropyReader(input, 0, context);
        if (offset != 0) Assert.Equal((1 << offset) - 1, reader.ReadBits(offset));
        Assert.Equal(17, tree.Read(ref reader)); Assert.Equal(29, tree.Read(ref reader)); Assert.Equal(43, tree.Read(ref reader));
        Assert.Equal(65535, reader.ReadBits(16)); Assert.Equal(17, tree.Read(ref reader));
        Assert.Equal(input.Length - 2, reader.Position); reader.Restart(0); Assert.Equal(input.Length, reader.Position);
        Assert.Throws<ImageLimitException>(() => context.Work(1)); Assert.Equal(unchanged, input);
    }

    [Theory]
    [InlineData(9)] [InlineData(10)] [InlineData(11)] [InlineData(12)]
    [InlineData(13)] [InlineData(14)] [InlineData(15)] [InlineData(16)]
    public void EveryPartialLongCanonicalBudgetRetainsOriginalBitsAndBytePosition(int length)
    {
        for (var offset = 0; offset < 8; offset++)
        for (var budget = 0; budget < length; budget++)
        {
            var context = Context(272 + (length <= 10 ? 1024 : 0) + (offset == 0 ? 0 : 1) + budget);
            var counts = new byte[16]; counts[length - 1] = 1;
            var tree = new JpegHuffman(counts, new byte[] { 17 }, context);
            var input = Stuff(new string('0', offset + length)); var unchanged = input.ToArray();
            var reader = new JpegEntropyReader(input, 0, context);
            if (offset != 0) Assert.Equal(0, reader.ReadBits(offset));
            try { tree.Read(ref reader); Assert.Fail("A partial code budget must reject."); }
            catch (ImageLimitException) { }
            Assert.Equal((offset + budget + 7) / 8, reader.Position);
            Assert.Throws<ImageLimitException>(() => context.Work(1)); Assert.Equal(unchanged, input);
        }
    }

    [Theory]
    [InlineData(false, 8, 1)] [InlineData(true, 8, 1)] [InlineData(true, 9, 3)]
    [InlineData(false, 9, 1)]
    public void IncompleteTwelveBitCodeRetainsMarkerOrTruncationVersusWorkPrecedence(bool marker, int budget, int position)
    {
        var context = Context(272 + budget); var counts = new byte[16]; counts[11] = 1;
        var tree = new JpegHuffman(counts, new byte[] { 17 }, context);
        var input = marker ? new byte[] { 0, 255, 0xD0 } : new byte[] { 0 };
        var reader = new JpegEntropyReader(input, 0, context);
        try { tree.Read(ref reader); Assert.Fail("Incomplete code must reject."); }
        catch (ImageLimitException) { Assert.Equal(8, budget); }
        catch (ImageDataException error)
        {
            Assert.Equal(9, budget);
            Assert.Equal(marker ? "Unexpected marker in JPEG entropy data." : "Truncated JPEG entropy data.", error.Message);
        }
        Assert.Equal(position, reader.Position); Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void UnassignedElevenBitPrefixRetainsSixteenBitCanonicalFailure()
    {
        var context = Context(272 + 16); var counts = new byte[16]; counts[10] = 1;
        var tree = new JpegHuffman(counts, new byte[] { 17 }, context);
        var reader = new JpegEntropyReader(new byte[] { 0, 0x20 }, 0, context);
        try { tree.Read(ref reader); Assert.Fail("Unassigned prefix must reject."); }
        catch (ImageDataException error) { Assert.Equal("Invalid JPEG Huffman code.", error.Message); }
        Assert.Equal(2, reader.Position); Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void OnlyNineAndTenBitCodesRequireTheLongLookupStorageAndConstruction()
    {
        var counts = new byte[16]; counts[9] = 1;
        var memory = new OperationContext(new ImageLimits { MaxWorkingBytes = 2764 }, default);
        Assert.Throws<ImageLimitException>(() => new JpegHuffman(counts, new byte[] { 17 }, memory));
        memory.Reserve(2047); Assert.Throws<ImageLimitException>(() => memory.Reserve(1));
        var work = Context(1295);
        Assert.Throws<ImageLimitException>(() => new JpegHuffman(counts, new byte[] { 17 }, work));
        work.Work(1023); Assert.Throws<ImageLimitException>(() => work.Work(1));
        foreach (var length in new[] { 1, 8, 10, 12, 16 })
        {
            var addedBytes = length == 10 ? 2048 : 0;
            var addedWork = length == 10 ? 1024 : 0;
            var exact = new OperationContext(new ImageLimits { MaxWorkingBytes = 717 + addedBytes, MaxWork = 272 + addedWork }, default);
            counts = new byte[16]; counts[length - 1] = 1;
            _ = new JpegHuffman(counts, new byte[] { 17 }, exact);
            Assert.Throws<ImageLimitException>(() => exact.Reserve(1)); Assert.Throws<ImageLimitException>(() => exact.Work(1));
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(7)]
    public void CanceledTwelveBitReadDoesNotCommitSpeculativeBytes(int offset)
    {
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var counts = new byte[16]; counts[11] = 1;
        var tree = new JpegHuffman(counts, new byte[] { 17 }, Context(long.MaxValue));
        var reader = new JpegEntropyReader(new byte[3], 0, context);
        if (offset != 0) reader.ReadBits(offset); var position = reader.Position; cancellation.Cancel();
        try { tree.Read(ref reader); Assert.Fail("Cancellation must precede code commit."); }
        catch (OperationCanceledException) { }
        Assert.Equal(position, reader.Position);
    }

    private static OperationContext Context(long work) => new(ImageLimits.Default with { MaxWork = work }, default);
    private static byte[] Stuff(string bits)
    {
        bits = bits.PadRight((bits.Length + 7) / 8 * 8, '1'); var bytes = new List<byte>();
        for (var index = 0; index < bits.Length; index += 8)
        {
            var value = Convert.ToByte(bits.Substring(index, 8), 2); bytes.Add(value); if (value == 255) bytes.Add(0);
        }
        bytes.Add(255); bytes.Add(0xD0); return bytes.ToArray();
    }
}
