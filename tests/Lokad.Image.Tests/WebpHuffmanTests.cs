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
    public void ByteTripletsKeepEveryChannelPhaseExactBytesWorkAndFollowingFields()
    {
        var tree=new WebpHuffman(Enumerable.Repeat((byte)8,256).ToArray(),Context());
        foreach(var red in new[]{0,1,127,128,254,255})
        foreach(var blue in new[]{0,1,127,128,254,255})
        foreach(var alpha in new[]{0,1,127,128,254,255})for(var phase=0;phase<8;phase++)
        {
            var suffix=string.Concat(Enumerable.Range(0,9).Select(bit=>(0x12d>>bit&1)==0?'0':'1'));
            var input=Pack(new string('0',phase)+Convert.ToString(red,2).PadLeft(8,'0')
                +Convert.ToString(blue,2).PadLeft(8,'0')+Convert.ToString(alpha,2).PadLeft(8,'0')+suffix);
            var original=input.ToArray();var work=Context(26);var referenceWork=Context(26);
            var reader=new WebpBitReader(input,work);var reference=new WebpBitReader(input,referenceWork);
            reader.Read(phase);reference.Read(phase);
            Assert.True(tree.TryReadByteChannels(tree,tree,ref reader,out var channels));
            Assert.Equal((uint)(alpha<<24|red<<16|blue),channels);
            var expected=ReadThree(tree,ref reference);Assert.Null(expected.Error);Assert.Equal(expected.Channels,channels);
            Assert.Equal(reference.Position,reader.Position);Assert.Equal(referenceWork.RemainingWork,work.RemainingWork);
            Assert.Equal(0x12d,reader.Read(9));Assert.Equal(0x12d,reference.Read(9));
            Assert.Equal(0,work.RemainingWork);Assert.Equal(original,input);
        }
    }

    [Fact]
    public void ByteTripletRefusalRetainsShortInputPartialBudgetCancellationAndRecovery()
    {
        var tree=new WebpHuffman(Enumerable.Repeat((byte)8,256).ToArray(),Context());
        var counter=typeof(OperationContext).GetField("work",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
        for(var phase=0;phase<8;phase++)for(var length=0;length<=6;length++)
        for(var budget=0;budget<=25;budget++)foreach(var live in new[]{0,1,2})
        {
            if(phase!=0 && length==0)continue;
            var bytes=Pack(new string('0',phase)+"101001011101001100011110001100111111000000000000");
            var input=bytes[..Math.Min(length,bytes.Length)];var original=input.ToArray();
            using var cancellation=new CancellationTokenSource();
            var work=new OperationContext(new ImageLimits {MaxWork=10000},live==0?default:cancellation.Token);
            var referenceWork=new OperationContext(new ImageLimits {MaxWork=10000},live==0?default:cancellation.Token);
            work.Work(9999-budget);referenceWork.Work(9999-budget);
            var reader=new WebpBitReader(input,work);var reference=new WebpBitReader(input,referenceWork);
            reader.Read(phase);reference.Read(phase);if(live==2)cancellation.Cancel();
            var position=reader.Position;var remaining=work.RemainingWork;
            var accepted=tree.TryReadByteChannels(tree,tree,ref reader,out var channels);
            Assert.Equal(live==0 && budget>=24 && input.Length>=(phase+24+7)/8,accepted);
            if(!accepted){Assert.Equal(position,reader.Position);Assert.Equal(remaining,work.RemainingWork);}
            var expected=ReadThree(tree,ref reference);
            var actual=accepted?((uint?)channels,(Exception?)null):ReadThree(tree,ref reader);
            Assert.Equal(expected.Channels,actual.Item1);Assert.Equal(expected.Error?.GetType(),actual.Item2?.GetType());
            Assert.Equal(expected.Error?.Message,actual.Item2?.Message);
            Assert.Equal(reference.Position,reader.Position);Assert.Equal(referenceWork.RemainingWork,work.RemainingWork);
            Assert.Equal(original,input);
            if(live!=0)continue;
            // Observe the remaining reservoir after a partial failure without
            // adding a production diagnostic API or changing the reader fields.
            counter.SetValue(work,0L);counter.SetValue(referenceWork,0L);
            for(var bit=0;bit<64;bit++)
            {
                int? a=null,e=null;Exception? ae=null,ee=null;
                try{a=reader.Read(1);}catch(Exception error){ae=error;}
                try{e=reference.Read(1);}catch(Exception error){ee=error;}
                Assert.Equal(e,a);Assert.Equal(ee?.GetType(),ae?.GetType());
                Assert.Equal(reference.Position,reader.Position);Assert.Equal(referenceWork.RemainingWork,work.RemainingWork);
                if(ee is not null)break;
            }
        }
    }

    [Fact]
    public void ByteTripletEligibilityKeepsOriginalMemoryAndRejectsOtherTreeShapes()
    {
        var lengths=Enumerable.Repeat((byte)8,256).ToArray();
        var memory=new OperationContext(new ImageLimits {MaxWorkingBytes=1888,MaxWork=6400},default);
        var tree=new WebpHuffman(lengths,memory);Assert.Equal(1888,tree.MemoryBytes);
        Assert.Equal(0,memory.RemainingWorkingBytes);Assert.Equal(0,memory.RemainingWork);
        var single=new WebpHuffman(new byte[]{0,1},Context());
        var mixed=new WebpHuffman(new byte[]{1,2,3,4,5,6,7,8,8},Context());
        var larger=new byte[512];lengths.CopyTo(larger,1);
        var nonByteAlphabet=new WebpHuffman(larger,Context());
        foreach(var other in new[]{single,mixed,nonByteAlphabet})
        {
            var work=Context(24);var reader=new WebpBitReader(new byte[]{255,255,255},work);
            Assert.False(other.TryReadByteChannels(tree,tree,ref reader,out _));
            Assert.False(tree.TryReadByteChannels(other,tree,ref reader,out _));
            Assert.False(tree.TryReadByteChannels(tree,other,ref reader,out _));
            Assert.Equal(0,reader.Position);Assert.Equal(24,work.RemainingWork);
        }
        var empty=new WebpBitReader([],Context(0));
        Assert.Equal(1,single.Read(ref empty));Assert.Equal(0,empty.Position);
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
