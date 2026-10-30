using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class WebpHuffmanTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(5)] [InlineData(8)] [InlineData(9)] [InlineData(10)] [InlineData(12)] [InlineData(15)]
    public void CompleteUniformCodesPreserveSymbolsAndFollowingFields(int length)
    {
        var alphabet = 1 << length;
        var lengths = Enumerable.Repeat((byte)length, alphabet).ToArray();
        var symbols = new[] { 0, alphabet / 2, alphabet - 1, 1, alphabet - 2 };
        var codeBits = string.Concat(symbols.Select(symbol => Convert.ToString(symbol, 2).PadLeft(length, '0')));
        var tree = new WebpHuffman(lengths, Context());
        for (var phase = 0; phase < 8; phase++)
        {
            var input = Pack(new string('0', phase) + codeBits + "0011110010100101");
            var unchanged = input.ToArray(); var context = Context();
            var reader = new WebpBitReader(input, context);
            Assert.Equal(0, reader.Read(phase));
            for (var index = 0; index < symbols.Length; index++)
            {
                Assert.Equal(symbols[index], tree.Read(ref reader));
            }
            Assert.Equal(0xA53C, reader.Read(16));
            Assert.Equal(unchanged, input);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    public void MixedShortCodesAndCachedOneBitLeavesPreserveBitConsumption(int phase)
    {
        byte[] lengths = [1, 2, 3, 4, 5, 6, 7, 8, 8];
        string[] codes = ["0", "10", "110", "1110", "11110", "111110", "1111110", "11111110", "11111111"];
        var tree = new WebpHuffman(lengths, Context());
        var input = Pack(new string('0', phase) + string.Concat(codes) + "0011110010100101");
        var reader = new WebpBitReader(input, Context());
        Assert.Equal(0, reader.Read(phase));
        for (var symbol = 0; symbol < codes.Length; symbol++)
        {
            Assert.Equal(symbol, tree.Read(ref reader));
        }
        Assert.Equal(0xA53C, reader.Read(16));
        var cached = new WebpBitReader(new byte[] { 0 }, Context());
        Assert.Equal(0, cached.Read(7)); Assert.Equal(0, tree.Read(ref cached)); Assert.Equal(1, cached.Position);
    }

    [Fact]
    public void PrefixStorageAndConstructionAreAdmittedBeforeAllocation()
    {
        var lengths = Enumerable.Repeat((byte)8, 256).ToArray();
        var limits = new ImageLimits { MaxWorkingBytes = 1888};
        Assert.Equal(1888, new WebpHuffman(lengths, new OperationContext(limits, default)).MemoryBytes);
        var memory = new OperationContext(limits with { MaxWorkingBytes = 1887 }, default);
        Assert.Throws<ImageLimitException>(() => new WebpHuffman(lengths, memory));
        memory.Reserve(1887); Assert.Throws<ImageLimitException>(() => memory.Reserve(1));

        var longOnly = Enumerable.Repeat((byte)9, 512).ToArray();
        Assert.Equal(1344, new WebpHuffman(longOnly, new OperationContext(new ImageLimits { MaxWorkingBytes = 1344}, default)).MemoryBytes);
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

    private static (uint? Channels,Exception? Error) ReadThree(WebpHuffman tree,ref WebpBitReader reader)
    {
        try
        {
            var red=tree.Read(ref reader);var blue=tree.Read(ref reader);var alpha=tree.Read(ref reader);
            return ((uint)(alpha<<24|red<<16|blue),null);
        }
        catch(Exception error){return (null,error);}
    }

    [Fact]
    public void ByteTripletsKeepEveryChannelPhaseExactBytesAndFollowingFields()
    {
        var tree=new WebpHuffman(Enumerable.Repeat((byte)8,256).ToArray(),Context());
        foreach(var red in new[]{0,1,127,128,254,255})
        foreach(var blue in new[]{0,1,127,128,254,255})
        foreach(var alpha in new[]{0,1,127,128,254,255})for(var phase=0;phase<8;phase++)
        {
            var suffix=string.Concat(Enumerable.Range(0,9).Select(bit=>(0x12d>>bit&1)==0?'0':'1'));
            var input=Pack(new string('0',phase)+Convert.ToString(red,2).PadLeft(8,'0')
                +Convert.ToString(blue,2).PadLeft(8,'0')+Convert.ToString(alpha,2).PadLeft(8,'0')+suffix);
            var original=input.ToArray();var operation=Context();var referenceContext=Context();
            var reader=new WebpBitReader(input,operation);var reference=new WebpBitReader(input,referenceContext);
            reader.Read(phase);reference.Read(phase);
            Assert.True(tree.TryReadByteChannels(tree,tree,ref reader,out var channels));
            Assert.Equal((uint)(alpha<<24|red<<16|blue),channels);
            var expected=ReadThree(tree,ref reference);Assert.Null(expected.Error);Assert.Equal(expected.Channels,channels);
            Assert.Equal(reference.Position,reader.Position);            Assert.Equal(0x12d,reader.Read(9));Assert.Equal(0x12d,reference.Read(9));
Assert.Equal(original,input);
        }
    }

    [Fact]
    public void ByteTripletEligibilityKeepsOriginalMemoryAndRejectsOtherTreeShapes()
    {
        var lengths=Enumerable.Repeat((byte)8,256).ToArray();
        var memory=new OperationContext(new ImageLimits {MaxWorkingBytes=1888},default);
        var tree=new WebpHuffman(lengths,memory);Assert.Equal(1888,tree.MemoryBytes);
        Assert.Equal(0,memory.RemainingWorkingBytes);        var single=new WebpHuffman(new byte[]{0,1},Context());
        var mixed=new WebpHuffman(new byte[]{1,2,3,4,5,6,7,8,8},Context());
        var larger=new byte[512];lengths.CopyTo(larger,1);
        var nonByteAlphabet=new WebpHuffman(larger,Context());
        foreach(var other in new[]{single,mixed,nonByteAlphabet})
        {
            var operation=Context();var reader=new WebpBitReader(new byte[]{255,255,255},operation);
            Assert.False(other.TryReadByteChannels(tree,tree,ref reader,out _));
            Assert.False(tree.TryReadByteChannels(other,tree,ref reader,out _));
            Assert.False(tree.TryReadByteChannels(tree,other,ref reader,out _));
            Assert.Equal(0,reader.Position);        }
        var empty=new WebpBitReader([],Context());
        Assert.Equal(1,single.Read(ref empty));Assert.Equal(0,empty.Position);
    }

    private static OperationContext Context()
    {
        var context = new OperationContext(ImageLimits.Default, default);
        return context;
    }
    private static byte[] Pack(string bits)
    {
        var bytes = new byte[(bits.Length + 7) / 8];
        for (var index = 0; index < bits.Length; index++) if (bits[index] == '1') bytes[index / 8] |= (byte)(1 << (index % 8));
        return bytes;
    }
}
