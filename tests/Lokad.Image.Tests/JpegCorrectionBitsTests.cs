using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCorrectionBitsTests
{
    [Theory]
    [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    [InlineData(5)] [InlineData(6)] [InlineData(7)] [InlineData(8)]
    [InlineData(9)] [InlineData(10)] [InlineData(11)] [InlineData(12)]
    [InlineData(13)] [InlineData(14)] [InlineData(15)] [InlineData(16)]
    public void GroupedBitsKeepValuesStuffingPositionsPaddingAndExactWork(int count)
    {
        for (var prefix=0;prefix<8;prefix++)
        for (var seed=0;seed<256;seed++)
        {
            var value=(seed*257+count*31)&((1<<count)-1); var first=(seed*13)&((1<<prefix)-1);
            var data=Payload(first,prefix,value,count); var original=data.ToArray();
            var budget=count+(prefix==0?0:1);
            var scalarContext=new OperationContext(ImageLimits.Default with { MaxWork=budget },default);
            var groupedContext=new OperationContext(ImageLimits.Default with { MaxWork=budget },default);
            var scalar=new JpegEntropyReader(data,0,scalarContext); var grouped=new JpegEntropyReader(data,0,groupedContext);
            if (prefix!=0) { Assert.Equal(first,scalar.ReadBits(prefix)); Assert.Equal(first,grouped.ReadBits(prefix)); }
            var expected=0;for(var k=0;k<count;k++)expected=(expected<<1)|scalar.ReadBits(1);
            Assert.True(grouped.TryReadCorrectionBits(count,out var actual)); Assert.Equal(expected,actual); Assert.Equal(value,actual);
            Assert.Equal(scalar.Position,grouped.Position);
            scalar.Restart(0);grouped.Restart(0);Assert.Equal(data.Length,grouped.Position);
            Assert.Throws<ImageLimitException>(()=>scalarContext.Work(1)); Assert.Throws<ImageLimitException>(()=>groupedContext.Work(1));
            Assert.Equal(original,data);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    public void FailedSpeculationKeepsScalarErrorAndPartialConsumption(int shape)
    {
        byte[] data=shape switch { 0=>[0xFF,0xD0],1=>[0x7F],2=>[0xFF],_=>[0x23,0xFF,0,0xFF,0xD0] };
        var count=shape==3?16:shape==1?16:8;var budget=shape==3?count-1:100;
        var scalarContext=new OperationContext(ImageLimits.Default with { MaxWork=budget },default);
        var groupedContext=new OperationContext(ImageLimits.Default with { MaxWork=budget },default);
        var scalar=new JpegEntropyReader(data,0,scalarContext);var grouped=new JpegEntropyReader(data,0,groupedContext);
        Assert.False(grouped.TryReadCorrectionBits(count,out _));Assert.Equal(0,grouped.Position);Assert.Equal(budget,groupedContext.RemainingWork);
        Exception? scalarError=null,groupedError=null;
        try { for(var k=0;k<count;k++)scalar.ReadBits(1); } catch(Exception error) { scalarError=error; }
        try { for(var k=0;k<count;k++)grouped.ReadBits(1); } catch(Exception error) { groupedError=error; }
        Assert.NotNull(scalarError);Assert.NotNull(groupedError);Assert.Equal(scalarError.GetType(),groupedError.GetType());Assert.Equal(scalarError.Message,groupedError.Message);
        Assert.Equal(scalar.Position,grouped.Position);Assert.Equal(scalarContext.RemainingWork,groupedContext.RemainingWork);
    }

    [Fact]
    public void CancellableReadersRetainOriginalChecksEvenAfterCancellation()
    {
        using var cancellation=new CancellationTokenSource();var context=new OperationContext(ImageLimits.Default,cancellation.Token);
        var reader=new JpegEntropyReader([0x7F],0,context);
        Assert.False(reader.TryReadCorrectionBits(3,out _));Assert.Equal(0,reader.Position);Assert.Equal(ImageLimits.Default.MaxWork,context.RemainingWork);
        cancellation.Cancel();Assert.False(reader.TryReadCorrectionBits(3,out _));Assert.Equal(0,reader.Position);
        Assert.Throws<OperationCanceledException>(()=>{var original=new JpegEntropyReader([0x7F],0,context);original.ReadBits(1);});
    }

    [Theory]
    [InlineData(0)] [InlineData(17)]
    public void InvalidWidthsRejectBeforeWork(int count)
    {
        var context=new OperationContext(ImageLimits.Default with { MaxWork=1 },default);
        Assert.Throws<ArgumentOutOfRangeException>(()=>{var reader=new JpegEntropyReader([],0,context);reader.TryReadCorrectionBits(count,out _);});
        Assert.Equal(1,context.RemainingWork);
    }

    private static byte[] Payload(int first,int prefix,int value,int count)
    {
        var length=prefix+count;var padding=(8-length%8)%8;
        var bits=(((ulong)(uint)first<<count)|(uint)value)<<padding|((1UL<<padding)-1);
        var result=new List<byte>();for(var i=(length+padding)/8-1;i>=0;i--) { var next=(byte)(bits>>(i*8));result.Add(next);if(next==255)result.Add(0); }
        result.Add(255);result.Add(0xD0);return result.ToArray();
    }
}
