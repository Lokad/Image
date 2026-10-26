using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class StreamDecodeInputTests
{
    private static readonly ImageLimits Limits = new() { MaxWorkingBytes = 4 * 1024 * 1024};

    [Fact]
    public void LargeOptionalMetadataNeedsNoFinalFullInputCopy()
    {
        var frame = new AuthoredPng.Frame("large-stream-input", 6, 8, 1, 1, false);
        var data = AuthoredPng.Wrap(frame, AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)), ("tEXt", new byte[1024 * 1024]));
        var previousCapacity = 8192L; while (previousCapacity * 2 < data.Length) previousCapacity *= 2;
        var limits = Limits with { MaxInputBytes = data.Length, MaxWorkingBytes = 8192 + previousCapacity + data.Length};
        using var input = new SyncInput(data, 8192); using var result = ImageCodec.Decode(input, new() { Limits = limits });
        using var expected = ImageCodec.Decode(data); Assert.Equal(OriginalPixels.Bytes(expected), OriginalPixels.Bytes(result));
        using var rejected = new SyncInput(data, 8192);
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(rejected,
            new() { Limits = limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 } }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit); Assert.False(rejected.WasDisposed);
    }

    [Fact]
    public void RetainedCapacityRemainsAdmittedDuringPixelDecoding()
    {
        var frame = new AuthoredPng.Frame("retained-input-capacity", 6, 8, 1, 1, false);
        var compressed = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame)); var data = AuthoredPng.Wrap(frame, compressed);
        Assert.True(data.Length < 256);
        // PNG result + two rows + inflater + compressed bytes, with the full
        // retained 256-byte input lease rather than just its acquired prefix.
        var peak = 256L + compressed.Length + 4 + 8 + 256 * 1024;
        using var exact = new SyncInput(data); using var result = ImageCodec.Decode(exact, new() { Limits = Limits with { MaxWorkingBytes = peak } });
        using var shortInput = new SyncInput(data);
        var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.Decode(shortInput,
            new() { Limits = Limits with { MaxWorkingBytes = peak - 1 } }));
        Assert.Equal(ImageLimitKind.WorkingBytes, failure.Limit);
        using var span = ImageCodec.Decode(data, new() { Limits = Limits with { MaxWorkingBytes = peak - 1 } });
        Assert.Equal(OriginalPixels.Bytes(span), OriginalPixels.Bytes(result));
    }

    [Theory]
    [InlineData(-1)] [InlineData(8193)]
    public void InvalidCallerReadCountsFailBeforePublication(int count)
    {
        using var input = new SyncInput([]) { InvalidCount = count };
        Assert.Throws<ImageDataException>(() => ImageCodec.Decode(input)); Assert.False(input.WasDisposed);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CallerIoFailureAndEofCancellationReleaseCapacityAndAllowRecovery(bool cancel)
    {
        var data = OriginalExif.Create(MetadataProfile.Png8, 17, 11).Encoded;
        using var root = new TestMemoryBudget(4 * 1024 * 1024);
        var limits = Limits with { MemoryBudget = root };
        using var token = new CancellationTokenSource();
        var expected = new IOException("Original caller failure.");
        using var input = new SyncInput(data) { Failure = cancel ? null : expected,
            FailAfter = 17, CancelAtEof = cancel ? token : null };
        if (cancel)
        {
            var error = Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.Decode(input, new() { Limits = limits }, token.Token));
            Assert.Equal(token.Token, error.CancellationToken);
        }
        else Assert.Same(expected, Assert.Throws<IOException>(() => ImageCodec.Decode(input, new() { Limits = limits })));
        Assert.Equal(0, root.Bytes); Assert.False(input.WasDisposed); Assert.False(root.IsDisposed);
        using (var recoveryInput = new SyncInput(data))
        using (var recovery = ImageCodec.Decode(recoveryInput, new() { Limits = limits })) Assert.Equal(17 * 11 * 4, root.Bytes);
        Assert.Equal(0, root.Bytes);
    }

    private sealed class SyncInput(byte[] data, int chunk = 17) : Stream
    {
        public int ReadCalls, EofReads, Consumed, LargestRequest;
        public bool WasDisposed;
        public int FailAfter = int.MaxValue;
        public int? InvalidCount;
        public IOException? Failure;
        public CancellationTokenSource? CancelAtEof;
        public override int Read(Span<byte> buffer)
        {
            ObjectDisposedException.ThrowIf(WasDisposed, this); ReadCalls++; LargestRequest = Math.Max(LargestRequest, buffer.Length);
            if (InvalidCount is { } invalid) return invalid;
            if (Consumed >= FailAfter && Failure is { } failure) throw failure;
            if (Consumed == data.Length) { EofReads++; CancelAtEof?.Cancel(); return 0; }
            var count = Math.Min(Math.Min(chunk, buffer.Length), data.Length - Consumed);
            data.AsSpan(Consumed, count).CopyTo(buffer); Consumed += count; return count;
        }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => throw new InvalidOperationException("Async read forbidden.");
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        public override bool CanRead => !WasDisposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
