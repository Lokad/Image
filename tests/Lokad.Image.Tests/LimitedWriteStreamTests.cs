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

    [Theory]
    [InlineData(0)] // Empty expandable stream, including capacity growth.
    [InlineData(1)] // Overwrite existing bytes.
    [InlineData(2)] // Gap within capacity.
    [InlineData(3)] // Gap requiring growth.
    [InlineData(4)] // Writable subrange with a nonzero origin.
    [InlineData(5)] // Fixed subrange at capacity.
    [InlineData(6)] // Read-only exposed subrange.
    [InlineData(7)] // Non-exposed buffer.
    [InlineData(8)] // Closed expandable stream.
    [InlineData(9)] // Closed read-only stream.
    [InlineData(10)] // Expandable stream at the integer overflow boundary.
    [InlineData(11)] // Nonzero origin at the same internal boundary.
    public void StockMemoryStreamMatchesOriginalSpanOutputAndFailureState(int scenario)
    {
        using var expected = CreateMemoryStream(scenario);
        using var actual = CreateMemoryStream(scenario);
        using var limited = new LimitedWriteStream(actual, 12, default);
        long admitted = 0;
        for (var i = 0; i < 12; i++)
        {
            var value = (byte)(i * 23);
            var originalError = Record.Exception(() => expected.Write(new byte[] { value }.AsSpan()));
            var error = Record.Exception(() => limited.WriteByte(value));
            Assert.Equal(originalError?.GetType(), error?.GetType());
            Assert.Equal(originalError?.Message, error?.Message);
            Assert.Equal((originalError as ArgumentException)?.ParamName, (error as ArgumentException)?.ParamName);
            if (error is null) admitted++;
            Assert.Equal(admitted, limited.Length);
            Assert.Equal(expected.ToArray(), actual.ToArray());
            Assert.Equal(expected.CanWrite, actual.CanWrite);
            if (expected.CanSeek)
            {
                Assert.Equal(expected.Length, actual.Length);
                Assert.Equal(expected.Position, actual.Position);
                Assert.Equal(expected.Capacity, actual.Capacity);
            }
        }
    }

    [Fact]
    public void MemoryStreamSubclassRetainsSpanCallbacksAndFailedWriteQuota()
    {
        using var output = new MemorySpanSink();
        using var limited = new LimitedWriteStream(output, 1, default);
        output.Fail = true;
        Assert.Throws<IOException>(() => limited.WriteByte(7));
        Assert.Equal(0, limited.Length);
        output.Fail = false;
        limited.WriteByte(19);
        Assert.Equal(new byte[] { 19 }, output.ToArray());
        Assert.Equal(2, output.SpanCalls);
        Assert.Equal(1, limited.Length);
        Assert.Throws<ImageLimitException>(() => limited.WriteByte(23));
        Assert.Equal(2, output.SpanCalls);
    }

    [Fact]
    public void CancellationThenQuotaPrecedeClosedMemoryStreamFailure()
    {
        using var output = new MemoryStream();
        output.Dispose();
        using var cancellation = new CancellationTokenSource();
        using var limited = new LimitedWriteStream(output, 0, cancellation.Token);
        Assert.Throws<ImageLimitException>(() => limited.WriteByte(1));
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => limited.WriteByte(1));
        Assert.Equal(0, limited.Length);
    }

    private static MemoryStream CreateMemoryStream(int scenario)
    {
        if (scenario is 4 or 5 or 6 or 11)
        {
            var stream = new MemoryStream(new byte[] { 251, 252, 11, 12, 13, 14, 15, 253 }, 2, 5,
                writable: scenario != 6, publiclyVisible: true);
            stream.Position = scenario switch { 4 => 1, 5 => 5, 11 => int.MaxValue - 2L, _ => 0 };
            return stream;
        }
        if (scenario is 7 or 9)
        {
            var stream = new MemoryStream(new byte[] { 11, 12, 13 }, writable: scenario == 7);
            if (scenario == 9) stream.Dispose();
            return stream;
        }
        var expandable = new MemoryStream(scenario == 0 ? 0 : 16);
        if (scenario != 0) expandable.Write(new byte[] { 11, 12, 13 });
        expandable.Position = scenario switch { 1 => 1, 2 => 8, 3 => 31, 10 => int.MaxValue, _ => expandable.Length };
        if (scenario == 8) expandable.Dispose();
        return expandable;
    }

    private sealed class MemorySpanSink : MemoryStream
    {
        public bool Fail { get; set; }
        public int SpanCalls { get; private set; }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            SpanCalls++;
            if (Fail) throw new IOException("Original failure before output.");
            base.Write(buffer);
        }
        public override void WriteByte(byte value) => throw new InvalidOperationException("The caller's span callback must be retained.");
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
