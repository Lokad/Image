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
        var context = Context();
        var tree = new JpegHuffman(counts, new byte[] { 17, 29, 43 }, context);
        var input = Stuff(new string('1', offset) + "0" + "10000000" + "1000000100" + new string('1', 16) + "0");
        var unchanged = input.ToArray(); var reader = new JpegEntropyReader(input, 0, context);
        if (offset != 0) Assert.Equal((1 << offset) - 1, reader.ReadBits(offset));
        Assert.Equal(17, tree.Read(ref reader)); Assert.Equal(29, tree.Read(ref reader)); Assert.Equal(43, tree.Read(ref reader));
        Assert.Equal(65535, reader.ReadBits(16)); Assert.Equal(17, tree.Read(ref reader));
        Assert.Equal(input.Length - 2, reader.Position); reader.Restart(0); Assert.Equal(input.Length, reader.Position);
Assert.Equal(unchanged, input);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void MixedShortNineAndTwelveBitCodesRetainFieldsStuffingPaddingAndRestart(int offset)
    {
        var counts = new byte[16]; counts[0] = counts[8] = counts[11] = 1;
        // Hand-derived canonical codes: 0, 100000000, 100000001000 (T.81 C.2).
        var context = Context();
        var tree = new JpegHuffman(counts, new byte[] { 17, 29, 43 }, context);
        var input = Stuff(new string('1', offset) + "0" + "100000000" + "100000001000" + new string('1', 16) + "0");
        var unchanged = input.ToArray(); var reader = new JpegEntropyReader(input, 0, context);
        if (offset != 0) Assert.Equal((1 << offset) - 1, reader.ReadBits(offset));
        Assert.Equal(17, tree.Read(ref reader)); Assert.Equal(29, tree.Read(ref reader)); Assert.Equal(43, tree.Read(ref reader));
        Assert.Equal(65535, reader.ReadBits(16)); Assert.Equal(17, tree.Read(ref reader));
        Assert.Equal(input.Length - 2, reader.Position); reader.Restart(0); Assert.Equal(input.Length, reader.Position);
Assert.Equal(unchanged, input);
    }

    [Fact]
    public void UnassignedElevenBitPrefixRetainsSixteenBitCanonicalFailure()
    {
        var context = Context(); var counts = new byte[16]; counts[10] = 1;
        var tree = new JpegHuffman(counts, new byte[] { 17 }, context);
        var reader = new JpegEntropyReader(new byte[] { 0, 0x20 }, 0, context);
        try { tree.Read(ref reader); Assert.Fail("Unassigned prefix must reject."); }
        catch (ImageDataException error) { Assert.Equal("Invalid JPEG Huffman code.", error.Message); }
        Assert.Equal(2, reader.Position);     }

    [Fact]
    public void OnlyNineAndTenBitCodesRequireTheLongLookupStorageAndConstruction()
    {
        var counts = new byte[16]; counts[9] = 1;
        var memory = new OperationContext(new ImageLimits { MaxWorkingBytes = 2764 }, default);
        Assert.Throws<ImageLimitException>(() => new JpegHuffman(counts, new byte[] { 17 }, memory));
        memory.Reserve(2047); Assert.Throws<ImageLimitException>(() => memory.Reserve(1));

        foreach (var length in new[] { 1, 8, 10, 12, 16 })
        {
            var addedBytes = length == 10 ? 2048 : 0;

            var exact = new OperationContext(new ImageLimits { MaxWorkingBytes = 717 + addedBytes}, default);
            counts = new byte[16]; counts[length - 1] = 1;
            _ = new JpegHuffman(counts, new byte[] { 17 }, exact);
            Assert.Throws<ImageLimitException>(() => exact.Reserve(1));         }
    }

    [Theory]
    [InlineData(0)] [InlineData(7)]
    public void CanceledTwelveBitReadDoesNotCommitSpeculativeBytes(int offset)
    {
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var counts = new byte[16]; counts[11] = 1;
        var tree = new JpegHuffman(counts, new byte[] { 17 }, Context());
        var reader = new JpegEntropyReader(new byte[3], 0, context);
        if (offset != 0) reader.ReadBits(offset); var position = reader.Position; cancellation.Cancel();
        try { tree.Read(ref reader); Assert.Fail("Cancellation must precede code commit."); }
        catch (OperationCanceledException) { }
        Assert.Equal(position, reader.Position);
    }

    private static OperationContext Context() => new(ImageLimits.Default, default);
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
