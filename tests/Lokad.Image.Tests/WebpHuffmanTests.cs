using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WebpHuffmanTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(5)] [InlineData(8)] [InlineData(9)] [InlineData(10)] [InlineData(12)] [InlineData(15)]
    public void CompleteUniformCodesPreserveSymbolsFieldsAndExactRequiredBytes(int length)
    {
        var alphabet = 1 << length;
        var lengths = Enumerable.Repeat((byte)length, alphabet).ToArray();
        var symbols = new[] { 0, alphabet / 2, alphabet - 1, 1, alphabet - 2 };
        var codeBits = string.Concat(symbols.Select(symbol => Convert.ToString(symbol, 2).PadLeft(length, '0')));
        var tree = new WebpHuffman(lengths, Context());
        for (var phase = 0; phase < 8; phase++)
        {
            var input = Pack(new string('0', phase) + codeBits + "0011110010100101");
            var unchanged = input.ToArray(); var context = Context(2 + symbols.Length * length);
            var reader = new WebpBitReader(input, context);
            Assert.Equal(0, reader.Read(phase));
            for (var index = 0; index < symbols.Length; index++)
            {
                Assert.Equal(symbols[index], tree.Read(ref reader));
                Assert.Equal((phase + (index + 1) * length + 7) / 8, reader.Position);
            }
            Assert.Equal(0xA53C, reader.Read(16));
            Assert.Throws<ImageLimitException>(() => context.Work(1));
            Assert.Equal(unchanged, input);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void MixedShortCodesAndCachedOneBitLeavesDoNotPrefetchUnneededBytes(int phase)
    {
        byte[] lengths = [1, 2, 3, 4, 5, 6, 7, 8, 8];
        string[] codes = ["0", "10", "110", "1110", "11110", "111110", "1111110", "11111110", "11111111"];
        var tree = new WebpHuffman(lengths, Context());
        var input = Pack(new string('0', phase) + string.Concat(codes));
        var reader = new WebpBitReader(input, Context()); var consumed = phase;
        Assert.Equal(0, reader.Read(phase));
        for (var symbol = 0; symbol < codes.Length; symbol++)
        {
            Assert.Equal(symbol, tree.Read(ref reader)); consumed += codes[symbol].Length;
            Assert.Equal((consumed + 7) / 8, reader.Position);
        }
        var cached = new WebpBitReader(new byte[] { 0 }, Context(2));
        Assert.Equal(0, cached.Read(7)); Assert.Equal(0, tree.Read(ref cached)); Assert.Equal(1, cached.Position);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void TightShortCodeBudgetsRetainEveryPartialBitAndBytePosition(int phase)
    {
        var tree = new WebpHuffman(Enumerable.Repeat((byte)8, 256).ToArray(), Context());
        for (var budget = 0; budget < 8; budget++)
        {
            var input = Pack(new string('0', phase) + "10100101" + "11111111");
            var unchanged = input.ToArray(); var context = Context(1 + budget);
            var reader = new WebpBitReader(input, context); reader.Read(phase);
            try { tree.Read(ref reader); Assert.Fail("Eight-bit code must exhaust a partial budget."); }
            catch (ImageLimitException) { }
            Assert.Equal((phase + budget + 7) / 8, reader.Position);
            Assert.Throws<ImageLimitException>(() => context.Work(1));
            Assert.Equal(unchanged, input);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void LongCanonicalFallbackRetainsAllPartialBudgetsAndExactSymbol(int phase)
    {
        byte[] lengths = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 15];
        var tree = new WebpHuffman(lengths, Context());
        for (var budget = 0; budget <= 15; budget++)
        {
            var input = Pack(new string('0', phase) + new string('1', 15) + "0");
            var context = Context(1 + budget); var reader = new WebpBitReader(input, context); reader.Read(phase);
            if (budget == 15) Assert.Equal(15, tree.Read(ref reader));
            else
            {
                try { tree.Read(ref reader); Assert.Fail("Fifteen-bit code must exhaust a partial budget."); }
                catch (ImageLimitException) { }
            }
            Assert.Equal((phase + budget + 7) / 8, reader.Position);
            Assert.Throws<ImageLimitException>(() => context.Work(1));
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(15)]
    public void TruncatedLongCodeRetainsLimitBeforeMissingBytePrecedence(int budget)
    {
        byte[] lengths = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 15];
        var tree = new WebpHuffman(lengths, Context()); var context = Context(budget);
        var reader = new WebpBitReader(new byte[] { 255 }, context);
        try { tree.Read(ref reader); Assert.Fail("One byte cannot provide this long code."); }
        catch (ImageLimitException) { Assert.True(budget <= 8); }
        catch (ImageDataException error) { Assert.True(budget >= 9); Assert.Equal("Truncated VP8L bitstream.", error.Message); }
        Assert.Equal(budget == 0 ? 0 : 1, reader.Position);
        context.Work(budget - Math.Min(budget, 9));
        Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void PrefixStorageAndConstructionWorkAreAdmittedBeforeAllocation()
    {
        var lengths = Enumerable.Repeat((byte)8, 256).ToArray();
        var limits = new ImageLimits { MaxWorkingBytes = 1888, MaxWork = 6400 };
        Assert.Equal(1888, new WebpHuffman(lengths, new OperationContext(limits, default)).MemoryBytes);
        var memory = new OperationContext(limits with { MaxWorkingBytes = 1887 }, default);
        Assert.Throws<ImageLimitException>(() => new WebpHuffman(lengths, memory));
        memory.Reserve(1887); Assert.Throws<ImageLimitException>(() => memory.Reserve(1));
        var work = new OperationContext(limits with { MaxWork = 6399 }, default);
        Assert.Throws<ImageLimitException>(() => new WebpHuffman(lengths, work));
        work.Work(2303); Assert.Throws<ImageLimitException>(() => work.Work(1));
        var longOnly = Enumerable.Repeat((byte)9, 512).ToArray();
        Assert.Equal(1344, new WebpHuffman(longOnly, new OperationContext(new ImageLimits { MaxWorkingBytes = 1344, MaxWork = 8192 }, default)).MemoryBytes);
        Assert.Equal(Enumerable.Repeat((byte)8, 256).ToArray(), lengths);
    }

    [Fact]
    public void CanceledPrefixReadDoesNotCommitSpeculativeBytesAndZeroBitLeafIsUnchanged()
    {
        using var cancellation = new CancellationTokenSource();
        var context = new OperationContext(ImageLimits.Default, cancellation.Token);
        var tree = new WebpHuffman(Enumerable.Repeat((byte)8, 256).ToArray(), Context());
        var longTree = new WebpHuffman(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 15 }, Context());
        var single = new WebpHuffman(new byte[] { 0, 0, 1 }, Context());
        var reader = new WebpBitReader(new byte[] { 0 }, context); cancellation.Cancel();
        try { tree.Read(ref reader); Assert.Fail("Cancellation must be observed before committing bits."); }
        catch (OperationCanceledException) { }
        Assert.Equal(0, reader.Position); Assert.Equal(2, single.Read(ref reader)); Assert.Equal(0, reader.Position);
        var longReader = new WebpBitReader(new byte[] { 255, 255 }, context);
        try { longTree.Read(ref longReader); Assert.Fail("Long canonical lookup must observe cancellation without consuming bytes."); }
        catch (OperationCanceledException) { }
        Assert.Equal(0, longReader.Position);
    }

    private static OperationContext Context(long work = long.MaxValue)
    {
        var context = new OperationContext(ImageLimits.Default with { MaxWork = Math.Max(1, work) }, default);
        if (work == 0) context.Work(1);
        return context;
    }
    private static byte[] Pack(string bits)
    {
        var bytes = new byte[(bits.Length + 7) / 8];
        for (var index = 0; index < bits.Length; index++) if (bits[index] == '1') bytes[index / 8] |= (byte)(1 << (index % 8));
        return bytes;
    }
}
