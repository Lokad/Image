using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class LimitedWriteStreamTests
{
    [Fact]
    public void ByteAndSpanWritesShareExactQuotaAndLeaveCallerOutputOpen()
    {
        using var output = new MemoryStream();
        using (var limited = new LimitedWriteStream(output, 3, default))
        {
            limited.WriteByte(255); limited.Write(new byte[] { 0, 128 });
            Assert.Equal(3, limited.Length);
            Assert.Throws<ImageLimitException>(() => limited.WriteByte(1));
            Assert.Equal(3, limited.Length);
            Assert.Equal(new byte[] { 255, 0, 128 }, output.ToArray());
        }
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void CancellationAfterConstructionPrecedesTheFullQuotaAndOutput()
    {
        using var cancellation = new CancellationTokenSource();
        using var output = new MemoryStream();
        using var limited = new LimitedWriteStream(output, 1, cancellation.Token);
        limited.WriteByte(127); cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => limited.WriteByte(255));
        Assert.Equal(1, limited.Length); Assert.Equal(new byte[] { 127 }, output.ToArray());
    }

    [Fact]
    public void FailedCallerWriteDoesNotConsumeTheByteQuota()
    {
        using var output = new SpanSink { Fail = true };
        using var limited = new LimitedWriteStream(output, 1, default);
        Assert.Throws<IOException>(() => limited.WriteByte(3)); Assert.Equal(0, limited.Length);
        output.Fail = false; limited.WriteByte(19);
        Assert.Equal(1, limited.Length); Assert.Equal(19, output.LastByte); Assert.Equal(1, output.SuccessfulWrites);
        Assert.Throws<ImageLimitException>(() => limited.WriteByte(5)); Assert.Equal(1, output.SuccessfulWrites);
    }

    [Fact]
    public void RepeatedByteWritesHaveNoManagedPerByteAllocationWithASpanSink()
    {
        // Allocation-only correctness check; no elapsed time or local speed claim.
        using var output = new SpanSink(); using var limited = new LimitedWriteStream(output, 8192, default);
        for (var i = 0; i < 32; i++) limited.WriteByte((byte)i);
        var start = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 4096; i++) limited.WriteByte((byte)i);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - start;
        Assert.Equal(0, allocated); Assert.Equal(4128, limited.Length); Assert.Equal(4128, output.SuccessfulWrites);
        Assert.Equal(255, output.LastByte);
    }

    private sealed class SpanSink : Stream
    {
        public bool Fail { get; set; }
        public int SuccessfulWrites { get; private set; }
        public byte LastByte { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (Fail) throw new IOException("Original fault injection before caller output.");
            if (buffer.Length != 1) throw new InvalidOperationException("Expected one admitted byte.");
            LastByte = buffer[0]; SuccessfulWrites++;
        }
        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException("The byte primitive must retain span dispatch.");
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
