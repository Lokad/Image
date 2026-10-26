using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed partial class Vp8CoefficientBlockTests
{
    [Fact]
    public void LiveBlocksMatchOriginalCoefficientsCursorAndFollowingReads()
    {
        var random = new Random(63862602);
        using var cancellation = new CancellationTokenSource();
        for (var trial = 0; trial < 1024; trial++)
        {
            var data = new byte[1024]; random.NextBytes(data);
            var probabilities = new byte[1056]; random.NextBytes(probabilities);
            var range = 128 + trial % 128; var reader = Reader(data, trial % 12, trial % 8, range, random.Next(range));
            var original = reader; var expected = Enumerable.Repeat((short)1234,18).ToArray(); var actual = expected.ToArray();
            using var context = new OperationContext(new() {},cancellation.Token);
            using var reference = new OperationContext(new() {},cancellation.Token);
            Assert.True(reader.CanReadCancellableCoefficientBlock(data,context));
            var dc = trial%2==0 ? 132 : 32767; var ac = trial%2==0 ? 284 : 1;
            var nonzero = Original(ref original,data,probabilities,expected.AsSpan(1,16),trial%4,trial%3,dc,ac,reference);
            var result = reader.ReadCancellableCoefficientBlock(data,probabilities,actual.AsSpan(1,16),trial%4,trial%3,dc,ac,context,out var seen);
            Assert.Equal(nonzero,seen);Assert.Equal(nonzero!=0,result);Assert.Equal(expected,actual);
            Assert.Equal(State(original),State(reader));            for (var i=0;i<19;i++)
            {
                var p=random.Next(256);Assert.Equal(original.Read(data,p,reference),reader.Read(data,p,context));
                Assert.Equal(State(original),State(reader));            }
        }
        Assert.True(cancellation.Token.CanBeCanceled); // Library borrowed it.
    }

    [Fact]
    public void EveryCancellationOrdinalPublishesExactAndScalarPrefixWithoutCleanupPoll()
    {
        var data=Enumerable.Range(0,1024).Select(i=>(byte)(73*i+19)).ToArray();
        ReadOnlySpan<int> probabilityCycle=[0,1,127,128,254,255];
        for (var stop=0;stop<=304;stop++)
        {
            using var cancellation=new CancellationTokenSource();using var root=new TestMemoryBudget(0);
            using var context=new OperationContext(new() {MemoryBudget=root},cancellation.Token);
            using var reference=new OperationContext(new() {},cancellation.Token);
            var range=128+stop%128;var actual=Reader(data,stop%12,stop%8,range,stop%range);var expected=actual;
            var buffered=new Vp8BooleanReader.CancellableBlockReader(data,actual,cancellation.Token);
            for (var i=0;i<stop;i++)
            {
                var p=probabilityCycle[i%6];
                Assert.Equal(expected.Read(data,p,reference),buffered.Read(p));
            }
            cancellation.Cancel();
            try {buffered.Read(128);throw new Exception("Canceled decision was accepted.");}
            catch(OperationCanceledException error) {Assert.Equal(cancellation.Token,error.CancellationToken);}
            buffered.Publish(ref actual,context);
            Assert.Equal(State(expected),State(actual));Assert.Equal(0,root.GrowthAttempts);
            Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
            using var following=new OperationContext(new() {},default);
            using var followingReference=new OperationContext(new() {},default);
            for (var i=0;i<19;i++)Assert.Equal(expected.Read(data,128,followingReference),actual.Read(data,128,following));
            Assert.Equal(State(expected),State(actual));
        }
    }

    [Fact]
    public void LiveSelectionMaximumBlockAndInitialCancellationKeepBoundsAndBorrowedRoot()
    {
        using var root=new TestMemoryBudget(0);
        for (var phase=0;phase<8;phase++)
        {
            using var cancellation=new CancellationTokenSource();
            using var context=new OperationContext(new() {MemoryBudget=root},cancellation.Token);
            using var reference=new OperationContext(new() {},cancellation.Token);
            using var plain=new OperationContext(new() {MemoryBudget=root},default);
            var data=Enumerable.Repeat((byte)255,273).ToArray();data[0]=254;
            var reader=Reader(data,0,phase,255,254);var original=reader;
            Assert.True(reader.CanReadCancellableCoefficientBlock(data,context));
            Assert.False(reader.CanReadCancellableCoefficientBlock(data,plain));
            Assert.False(reader.CanReadCancellableCoefficientBlock(data.AsSpan(0,272),context));
            Assert.False(new Vp8BooleanReader(data,0,272).CanReadCancellableCoefficientBlock(data,context));
            Assert.False(Reader(data,0,phase,255,255).CanReadCancellableCoefficientBlock(data,context));
            var probabilities=new byte[1056];var actual=new short[16];var expected=new short[16];
            Assert.Equal(1,Original(ref original,data,probabilities,expected,3,0,132,284,reference));
            Assert.True(reader.ReadCancellableCoefficientBlock(data,probabilities,actual,3,0,132,284,context,out var nonzero));
            Assert.Equal(1,nonzero);Assert.Equal(expected,actual);Assert.Equal(State(original),State(reader));
            using var canceledContext=new OperationContext(new() {MemoryBudget=root},cancellation.Token);
            var before=State(reader);var guarded=Enumerable.Repeat((short)1234,18).ToArray();cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(()=>reader.ReadCancellableCoefficientBlock(data,probabilities,guarded.AsSpan(1,16),3,0,132,284,canceledContext,out _));
            Assert.Equal(before,State(reader));            Assert.All(guarded,x=>Assert.Equal(1234,x));Assert.Equal(0,root.Bytes);Assert.Equal(0,root.GrowthAttempts);
        }
        Assert.False(root.IsDisposed);
    }
}
