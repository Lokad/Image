using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class DirectStreamInputTests
{
    [Theory]
    [InlineData(false, 256)] [InlineData(true, 256)]
    [InlineData(false, 8192)] [InlineData(true, 8192)]
    public async Task FullCapacityEofPreservesMetadataWithoutAnotherGrowth(bool asynchronous, int length)
    {
        var data = Packet(length); var saved = data.ToArray();
        var peak = length == 256 ? 256 : length + length / 2;
        using var root = new TestMemoryBudget(peak);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = peak };
        using var input = new Input(data, asynchronous, root);
        var actual = asynchronous ? await ImageCodec.IdentifyAsync(input, limits) : ImageCodec.Identify(input, limits);
        Assert.Equal(ImageCodec.Identify(data), actual); Assert.Equal(saved, data);
        Assert.Equal(1, input.EofReads); Assert.False(input.WasDisposed);
        Assert.Equal(peak, root.Peak); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.InRange(input.LargestRequest, 1, 8192);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task InputExcessAtFullCapacityFailsBeforeReplacementAdmission(bool asynchronous)
    {
        using var root = new TestMemoryBudget(256);
        var limits = new ImageLimits { MemoryBudget = root, MaxInputBytes = 256, MaxWorkingBytes = 256 };
        using var input = new Input(Packet(257), asynchronous, root);
        var error = await Assert.ThrowsAsync<ImageLimitException>(async () =>
        {
            if (asynchronous) await ImageCodec.IdentifyAsync(input, limits);
            else ImageCodec.Identify(input, limits);
        });
        Assert.Equal(ImageLimitKind.InputBytes, error.Limit); Assert.Equal(0, root.Refusals);
        Assert.Equal(256, root.Peak); Assert.Equal(0, root.Bytes); Assert.False(input.WasDisposed);
    }

    [Fact]
    public async Task CanceledFullCapacityProbeKeepsInputChargedUntilItsActualUserFinishes()
    {
        var data = Packet(256);
        using var root = new TestMemoryBudget(256);
        var limits = new ImageLimits { MemoryBudget = root, MaxWorkingBytes = 256 };
        using var cancellation = new CancellationTokenSource();
        using var input = new Input(data, true, root) { HoldEof = true };
        var pending = ImageCodec.IdentifyAsync(input, limits, cancellation.Token).AsTask();
        await input.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            Assert.Equal(256, root.Bytes); cancellation.Cancel();
            Assert.False(pending.IsCompleted); Assert.Equal(256, root.Bytes);
            Assert.Throws<ImageLimitException>(() => ImageBuffer.Create(1, 1, 8, limits));
        }
        finally { input.Resume.TrySetResult(); }
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.True(input.SawLiveCharge);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.False(input.WasDisposed);
        using var next = new Input(data, true, root);
        Assert.Equal(ImageCodec.Identify(data), await ImageCodec.IdentifyAsync(next, limits));
        Assert.Equal(0, root.Bytes);
    }

    private static byte[] Packet(int length)
    {
        var frame = new AuthoredPng.Frame("direct-input-boundary", 6, 8, 1, 1, false);
        var zlib = AuthoredPng.StoredZlib(AuthoredPng.FilteredRows(frame));
        var minimal = AuthoredPng.Wrap(frame, zlib);
        return AuthoredPng.Wrap(frame, zlib, ("tEXt", new byte[length - minimal.Length - 12]));
    }

    private sealed class Input(byte[] data, bool asynchronous, TestMemoryBudget root) : Stream
    {
        public bool WasDisposed, HoldEof, SawLiveCharge;
        public int EofReads, LargestRequest;
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int position;
        public override int Read(Span<byte> destination)
        {
            if (asynchronous) throw new InvalidOperationException("Synchronous reads are forbidden.");
            return Copy(destination);
        }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override async ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
        {
            if (!asynchronous) throw new InvalidOperationException("Asynchronous reads are forbidden.");
            if (position == data.Length && HoldEof)
            {
                Assert.Equal(1, destination.Length); Entered.SetResult();
                await Resume.Task.ConfigureAwait(false);
                SawLiveCharge = root.Bytes == 256;
            }
            cancellationToken.ThrowIfCancellationRequested(); return Copy(destination.Span);
        }
        private int Copy(Span<byte> destination)
        {
            ObjectDisposedException.ThrowIf(WasDisposed, this);
            Assert.True(root.Bytes >= destination.Length);
            LargestRequest = Math.Max(LargestRequest, destination.Length);
            if (position == data.Length) { EofReads++; return 0; }
            var count = Math.Min(destination.Length, data.Length - position);
            data.AsSpan(position, count).CopyTo(destination); position += count; return count;
        }
        protected override void Dispose(bool disposing) { WasDisposed = true; base.Dispose(disposing); }
        public override bool CanRead => !WasDisposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new InvalidOperationException("Length queries are forbidden.");
        public override long Position { get => throw new InvalidOperationException("Position queries are forbidden."); set => throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
