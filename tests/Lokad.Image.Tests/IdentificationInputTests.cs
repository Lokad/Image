using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class IdentificationInputTests
{
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(255)] [InlineData(256)]
    [InlineData(8191)] [InlineData(8192)] [InlineData(8193)] [InlineData(218775)]
    public void CompleteAcquisitionPreservesBytesAndNeverQueriesStreamMetadata(int length)
    {
        var data = Bytes(length, 31);
        foreach (var chunk in new[] { 3, 257, 8192 })
        {
            using var stream = new Reader(data, chunk);
            using var context = new OperationContext(ImageLimits.Default, default);
            using var input = IdentificationInput.Read(stream, context);
            Assert.Equal(data, input.Span.ToArray()); Assert.Equal(length, stream.Consumed);
            Assert.Equal(1, stream.EofReads); Assert.True(stream.CanRead);
            Assert.InRange(stream.LargestRequest, 1, 8192); Assert.Equal(Bytes(length, 31), data);
        }
    }

    [Fact]
    public void EofAtFullCapacityNeedsNoGrowthAndInputExcessIsStructured()
    {
        var data = Bytes(256, 17);
        using var stream = new Reader(data, 256);
        using var context = new OperationContext(new() { MaxWorkingBytes = 256 }, default);
        using var input = IdentificationInput.Read(stream, context);
        Assert.Equal(data, input.Span.ToArray()); Assert.Equal(0, context.RemainingWorkingBytes);
        Assert.Equal(1, stream.EofReads);
        using var excess = new Reader(Bytes(257, 17), 256);
        using var bounded = new OperationContext(new() { MaxInputBytes = 256, MaxWorkingBytes = 256 }, default);
        var error = Assert.Throws<ImageLimitException>(() => IdentificationInput.Read(excess, bounded));
        Assert.Equal(ImageLimitKind.InputBytes, error.Limit); Assert.True(excess.CanRead);
        Assert.Equal(256, bounded.RemainingWorkingBytes);
    }

    [Fact]
    public void GrowthAdmitsSimultaneousCapacityAndFailureRefundsTheOwnedInput()
    {
        var data = Bytes(257, 41);
        using var stream = new Reader(data, 256);
        using var context = new OperationContext(new() { MaxWorkingBytes = 768 }, default);
        using var input = IdentificationInput.Read(stream, context);
        Assert.Equal(data, input.Span.ToArray()); Assert.Equal(512, input.Capacity);
        Assert.Equal(256, context.RemainingWorkingBytes);
        using var rejected = new Reader(data, 256);
        using var shortContext = new OperationContext(new() { MaxWorkingBytes = 767 }, default);
        var error = Assert.Throws<ImageLimitException>(() => IdentificationInput.Read(rejected, shortContext));
        Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit);
        Assert.Equal(767, shortContext.RemainingWorkingBytes); Assert.True(rejected.CanRead);
    }

    [Fact]
    public void CallerFailureAndCallbackCancellationPreserveIdentityAndRefundInput()
    {
        foreach (var cancel in new[] { false, true })
        {
            using var cancellation = new CancellationTokenSource();
            var expected = new IOException("Original caller failure.");
            using var stream = new Reader(Bytes(33, 71), 3)
                { Failure = cancel ? null : expected, CancelDuringRead = cancel ? cancellation : null };
            using var context = new OperationContext(ImageLimits.Default, cancellation.Token);
            var error = Record.Exception(() => IdentificationInput.Read(stream, context));
            if (cancel) Assert.Equal(cancellation.Token, Assert.IsType<OperationCanceledException>(error).CancellationToken);
            else Assert.Same(expected, error);
            Assert.Equal(context.Limits.MaxWorkingBytes, context.RemainingWorkingBytes); Assert.True(stream.CanRead);
        }
    }

    [Fact]
    public void LiveInputsAndReusedDirtyPrefixesNeverAlias()
    {
        var a = Bytes(218775, 17); var b = Bytes(218775, 83); var c = Bytes(33, 151);
        using var sa = new Reader(a, 8192); using var sb = new Reader(b, 8192);
        using var ca = new OperationContext(ImageLimits.Default, default);
        using var cb = new OperationContext(ImageLimits.Default, default);
        var first = IdentificationInput.Read(sa, ca);
        using var second = IdentificationInput.Read(sb, cb);
        Assert.Equal(a, first.Span.ToArray()); Assert.Equal(b, second.Span.ToArray());
        first.Dispose(); first.Dispose();
        using var sc = new Reader(c, 8192); using var cc = new OperationContext(ImageLimits.Default, default);
        using var third = IdentificationInput.Read(sc, cc);
        Assert.Equal(c, third.Span.ToArray()); Assert.Equal(b, second.Span.ToArray());
    }

    private static byte[] Bytes(int length, int seed) => Enumerable.Range(0, length).Select(i => (byte)(i * 67 + seed)).ToArray();
    private sealed class Reader(byte[] bytes, int chunk) : Stream
    {
        public int Reads, Consumed, EofReads, LargestRequest;
        public IOException? Failure;
        public CancellationTokenSource? CancelDuringRead;
        private bool disposed;
        public override int Read(Span<byte> output)
        {
            ObjectDisposedException.ThrowIf(disposed, this); Reads++; LargestRequest = Math.Max(LargestRequest, output.Length);
            if (Reads == 2) { if (Failure is { } failure) throw failure; CancelDuringRead?.Cancel(); }
            if (Consumed == bytes.Length) { EofReads++; return 0; }
            var count = Math.Min(Math.Min(chunk, output.Length), bytes.Length - Consumed);
            bytes.AsSpan(Consumed, count).CopyTo(output); Consumed += count; return count;
        }
        public override int Read(byte[] output, int offset, int count) => Read(output.AsSpan(offset, count));
        public override bool CanRead => !disposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new InvalidOperationException("Length must not be queried.");
        public override long Position { get => throw new InvalidOperationException("Position must not be queried."); set => throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { disposed = true; base.Dispose(disposing); }
    }
}
