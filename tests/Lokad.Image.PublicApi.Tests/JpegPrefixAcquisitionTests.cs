using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegPrefixAcquisitionTests
{
    // Authored ISO/IEC 10918-1 marker/segment prefix, deliberately without a
    // complete pixel stream. Probe must establish dimensions before that tail.
    private static (byte[] Data,int Prefix,int BodyEnd) Prefix(int opaque)
    {
        var end=6+opaque;var prefix=end+19;var data=new byte[prefix+32];
        new byte[]{255,216,255,226}.CopyTo(data,0);
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(4),(ushort)(opaque+2));
        for(var i=0;i<opaque;i++)data[6+i]=(byte)(i*37+11);
        new byte[]{255,192,0,17,8,0,17,0,19,3,1,17,0,2,17,0,3,17,0}.CopyTo(data,end);
        data.AsSpan(prefix).Fill(0xa7);return(data,prefix,end);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task PrefixBoundsPartialReadsAndIndependentResults(bool asynchronous)
    {
        foreach(var body in new[]{0,1,253,254,255,256,511})
        foreach(var chunk in new[]{int.MaxValue,1,17})
        {
            var data=Prefix(body);using var root=new TestMemoryBudget(512);
            var options=new ImageProbeOptions{Limits=new ImageLimits{MemoryBudget=root,MaxWorkingBytes=512,MaxInputBytes=data.Prefix},MaxAcquiredBytes=data.Prefix};
            using var first=new PrefixInput(data.Data,data.Prefix,chunk,asynchronous);
            var result=asynchronous?await ImageCodec.ProbeAsync(first,options):ImageCodec.Probe(first,options);
            Assert.Equal(ImageFormat.Jpeg,result.Format);Assert.Equal((19,17),(result.Width,result.Height));Assert.False(result.HasAlpha);
            Assert.Equal(data.Prefix,result.BytesRead);Assert.Equal(data.Prefix,result.BytesTraversed);Assert.Equal(data.Prefix,first.Offset);
            Assert.False(first.WasDisposed);Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);
            using var second=new PrefixInput(data.Data,data.Prefix,chunk,asynchronous);
            var other=asynchronous?await ImageCodec.ProbeAsync(second,options):ImageCodec.Probe(second,options);
            Assert.NotSame(result,other);data.Data.AsSpan().Clear();first.Dispose();second.Dispose();
            Assert.Equal((19,17),(result.Width,result.Height));Assert.Equal((19,17),(other.Width,other.Height));Assert.Equal(0,root.Bytes);
        }
    }

    [Fact]
    public async Task PendingOpaqueAcquisitionKeepsAdmissionThroughActualUserAndRecovery()
    {
        var data=Prefix(65);using var root=new TestMemoryBudget(512);using var cancel=new CancellationTokenSource();
        var options=new ImageProbeOptions{Limits=new ImageLimits{MemoryBudget=root,MaxWorkingBytes=512},MaxAcquiredBytes=data.Prefix};
        using var source=new PrefixInput(data.Data,data.Prefix,int.MaxValue,true){HoldBody=true};
        var task=ImageCodec.ProbeAsync(source,options,cancel.Token).AsTask();await source.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(512,root.Bytes);Assert.False(task.IsCompleted);cancel.Cancel();await Task.Yield();
        Assert.False(task.IsCompleted);Assert.Equal(512,root.Bytes);
        using var refusedSource=new PrefixInput(data.Data,data.Prefix,int.MaxValue,true);
        var refused=await Assert.ThrowsAsync<ImageLimitException>(()=>ImageCodec.ProbeAsync(refusedSource,options).AsTask());
        Assert.Equal((ImageLimitKind.WorkingBytes,ImageLimitScope.SharedBudget),(refused.Limit,refused.Scope));
        Assert.Equal(0,refusedSource.Offset);Assert.Equal(2,root.Attempts);Assert.Equal(512,root.Bytes);
        source.ReleaseRead();var canceled=await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>task);
        Assert.True(task.IsCanceled);Assert.Equal(cancel.Token,canceled.CancellationToken);Assert.Equal(0,root.Bytes);
        Assert.False(source.WasDisposed);Assert.False(root.IsDisposed);
        using var recoveredSource=new PrefixInput(data.Data,data.Prefix,int.MaxValue,true);
        var recovered=await ImageCodec.ProbeAsync(recoveredSource,options);Assert.Equal((19,17),(recovered.Width,recovered.Height));
        Assert.Equal(0,root.Bytes);Assert.False(recoveredSource.WasDisposed);
    }

    [Fact]
    public async Task CancellationAfterOpaqueReadPrecedesInputSentinelOrInvalidMarker()
    {
        foreach(var invalid in new[]{false,true})
        foreach(var allowance in new[]{0,1,2})
        {
            var data=Prefix(65);if(invalid)data.Data[data.BodyEnd]=0;
            using var root=new TestMemoryBudget(512);using var cancel=new CancellationTokenSource();
            var options=new ImageProbeOptions{Limits=new ImageLimits{MemoryBudget=root,MaxWorkingBytes=512,MaxInputBytes=data.BodyEnd+allowance}};
            using var input=new PrefixInput(data.Data,data.Prefix,int.MaxValue,true){AfterRead=offset=>{if(offset>=data.BodyEnd)cancel.Cancel();}};
            var task=ImageCodec.ProbeAsync(input,options,cancel.Token).AsTask();
            var failure=await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>task);
            Assert.True(task.IsCanceled);Assert.Equal(cancel.Token,failure.CancellationToken);Assert.Equal(0,root.Bytes);
            Assert.False(root.IsDisposed);Assert.False(input.WasDisposed);
        }
    }

    [Fact]
    public async Task BoundaryCapsKeepStructuredFailurePrecedenceAndCleanup()
    {
        foreach(var invalid in new[]{false,true})
        foreach(var acquired in new[]{false,true})
        foreach(var allowance in new[]{0,1,2})
        {
            var data=Prefix(65);if(invalid)data.Data[data.BodyEnd]=0;
            using var root=new TestMemoryBudget(512);var cap=data.BodyEnd+allowance;
            var options=new ImageProbeOptions{Limits=new ImageLimits{MemoryBudget=root,MaxWorkingBytes=512,MaxInputBytes=acquired?data.Prefix:cap},MaxAcquiredBytes=acquired?cap:data.Prefix};
            using var input=new PrefixInput(data.Data,data.Prefix,int.MaxValue,true);
            if(invalid&&allowance>0)
                await Assert.ThrowsAsync<ImageDataException>(()=>ImageCodec.ProbeAsync(input,options).AsTask());
            else
            {
                var failure=await Assert.ThrowsAsync<ImageLimitException>(()=>ImageCodec.ProbeAsync(input,options).AsTask());
                Assert.Equal(acquired?ImageLimitKind.AcquiredBytes:ImageLimitKind.InputBytes,failure.Limit);
            }
            Assert.Equal(0,root.Bytes);Assert.False(root.IsDisposed);Assert.False(input.WasDisposed);
        }
    }

    private sealed class PrefixInput(byte[] data,int prefix,int chunk,bool asynchronous):Stream
    {
        private Memory<byte> held;private TaskCompletionSource<int>? completion;private bool heldOnce;
        internal bool HoldBody{get;init;}internal Action<int>? AfterRead{get;init;}
        internal TaskCompletionSource ReadStarted{get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal int Offset{get;private set;}internal bool WasDisposed{get;private set;}
        private int Copy(Span<byte> target)
        {
            ObjectDisposedException.ThrowIf(WasDisposed,this);
            if((long)Offset+target.Length>prefix)throw new InvalidOperationException("Dimension probe requested the pixel tail.");
            var n=Math.Min(target.Length,Math.Min(chunk,prefix-Offset));data.AsSpan(Offset,n).CopyTo(target);Offset+=n;AfterRead?.Invoke(Offset);return n;
        }
        internal void ReleaseRead(){var owner=completion??throw new InvalidOperationException();var n=Copy(held.Span);held=default;owner.SetResult(n);}
        public override ValueTask<int> ReadAsync(Memory<byte> buffer,CancellationToken cancellationToken=default)
        {
            if(!asynchronous)throw new InvalidOperationException("Unexpected async input.");cancellationToken.ThrowIfCancellationRequested();
            if(HoldBody&&!heldOnce&&Offset==6){heldOnce=true;held=buffer;completion=new(TaskCreationOptions.RunContinuationsAsynchronously);ReadStarted.SetResult();return new(completion.Task);}
            return ValueTask.FromResult(Copy(buffer.Span));
        }
        public override int Read(Span<byte> buffer){if(asynchronous)throw new InvalidOperationException("Unexpected sync input.");return Copy(buffer);}
        public override int Read(byte[] buffer,int offset,int count)=>Read(buffer.AsSpan(offset,count));
        public override bool CanRead=>!WasDisposed;public override bool CanSeek=>false;public override bool CanWrite=>false;
        public override long Length=>throw new NotSupportedException();public override long Position{get=>throw new NotSupportedException();set=>throw new NotSupportedException();}
        public override long Seek(long offset,SeekOrigin origin)=>throw new NotSupportedException();public override void SetLength(long value)=>throw new NotSupportedException();
        public override void Write(byte[] buffer,int offset,int count)=>throw new NotSupportedException();public override void Flush()=>throw new NotSupportedException();
        protected override void Dispose(bool disposing){WasDisposed=true;base.Dispose(disposing);}
    }
}
