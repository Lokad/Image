using System.Security.Cryptography;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class Vp8ReferenceEncodeTests
{
    [Theory]
    [MemberData(nameof(OriginalVp8Controls.Cases), MemberType = typeof(OriginalVp8Controls))]
    public async Task PublicSyncAndAsyncMatchCompleteReferencePacketsWithoutChangingCallerOwnership(
        string name, int depth, int method, int quality, int bytes, string expected)
    {
        using var root = new TestMemoryBudget(128_000_000);
        using var source = OriginalVp8lControls.Buffer(name, depth, root);
        var original = OriginalPixels.Bytes(source); var baseline = root.Bytes;
        var options = new WebpEncodeOptions { Lossless = false, Effort = method, Quality = quality,
            Limits = new() { MemoryBudget = root} };
        using var sync = new MemoryStream(); ImageCodec.EncodeWebp(source, sync, options);
        Assert.Equal(baseline, root.Bytes); var packet = sync.ToArray();
        Assert.Equal(bytes, packet.Length); Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet)));
        using var asynchronous = new AsyncOutput(); await ImageCodec.EncodeWebpAsync(source, asynchronous, options);
        Assert.Equal(packet, asynchronous.Bytes); Assert.Equal(baseline, root.Bytes);
        Assert.Equal(original, OriginalPixels.Bytes(source)); Assert.True(sync.CanWrite); Assert.False(asynchronous.WasDisposed);
        source.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        Assert.Equal(expected, Convert.ToHexStringLower(SHA256.HashData(packet)));
        Assert.Equal(packet, asynchronous.Bytes);
    }

    [Fact]
    public void DefaultLossyQualityUsesTheFrozenQuality85Control()
    {
        using var source = OriginalVp8lControls.Buffer("diagram", 8); using var output = new MemoryStream();
        ImageCodec.EncodeWebp(source, output, new() { Lossless = false });
        Assert.Equal(8898, output.Length);
        Assert.Equal("51a4abeda30c4f8c8b1dea563ab6690a89a5feed93b69c467398f9b736aeb536", Convert.ToHexStringLower(SHA256.HashData(output.ToArray())));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EarlyAndFinalOwnedAdmissionRefusalsDoNotDeliverOutputAndRecoveryUsesTheSameRoot(bool final)
    {
        long growths;
        using (var sizingRoot = new TestMemoryBudget(1_000_000))
        using (var image = OriginalVp8lControls.Buffer("singleton", 8, sizingRoot))
        {
            var start = sizingRoot.GrowthAttempts; using var output = new MemoryStream();
            ImageCodec.EncodeWebp(image, output, new() { Lossless = false, Limits = new() { MemoryBudget = sizingRoot } });
            growths = sizingRoot.GrowthAttempts - start;
        }
        using var root = new TestMemoryBudget(1_000_000); using var source = OriginalVp8lControls.Buffer("singleton", 8, root);
        using var caller = root.CreateScope(); Assert.True(caller.TryReserve(1, out var held)); var baseline = root.Bytes;
        root.RefuseAtGrowth = root.GrowthAttempts + (final ? growths : 1);
        using var failed = new MemoryStream(); var options = new WebpEncodeOptions { Lossless = false, Limits = new() { MemoryBudget = root } };
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, failed, options));
        Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
        Assert.Equal(1, root.Refusals); Assert.Equal(root.RefuseAtGrowth, root.GrowthAttempts); Assert.Equal(baseline, root.Bytes); Assert.Equal(0, failed.Length);
        root.RefuseAtGrowth = 0; held.Dispose(); using var next = new MemoryStream(); ImageCodec.EncodeWebp(source, next, options);
        Assert.True(next.Length > 0); source.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PublicAsyncRetainsStorageUntilActualStreamUseEndsOnCancellationOrFailure(bool cancel)
    {
        using var root = new TestMemoryBudget(1_000_000); using var source = OriginalVp8lControls.Buffer("singleton", 8, root);
        using var cancellation = new CancellationTokenSource(); using var output = new BlockedOutput(cancel);
        var operation = ImageCodec.EncodeWebpAsync(source, output,
            new() { Lossless = false, Limits = new() { MemoryBudget = root } }, cancellation.Token).AsTask();
        await output.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10)); source.Dispose(); var live = root.Bytes; Assert.True(live > 0);
        using (var caller = root.CreateScope())
        {
            Assert.True(caller.TryReserve(1_000_000 - live - 127, out var held));
            using var concurrent = OriginalVp8lControls.Buffer("singleton", 8, root); using var rejected = new MemoryStream();
            var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(concurrent, rejected,
                new() { Lossless = false, Limits = new() { MemoryBudget = root } }));
            Assert.Equal((ImageLimitKind.WorkingBytes, ImageLimitScope.SharedBudget), (error.Limit, error.Scope));
            Assert.Equal(live + held.Bytes + concurrent.Stride * concurrent.Height, root.Bytes); Assert.Equal(0, rejected.Length);
        }
        Assert.Equal(live, root.Bytes); Assert.False(operation.IsCompleted);
        if (cancel) cancellation.Cancel(); await Task.Yield(); Assert.False(operation.IsCompleted); Assert.Equal(live, root.Bytes);
        output.Release.TrySetResult();
        if (cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
        else await Assert.ThrowsAsync<IOException>(() => operation);
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.False(output.WasDisposed);
        using var nextSource = OriginalVp8lControls.Buffer("singleton", 8, root); using var next = new AsyncOutput();
        await ImageCodec.EncodeWebpAsync(nextSource, next, new() { Lossless = false, Limits = new() { MemoryBudget = root } });
        nextSource.Dispose(); Assert.Equal(0, root.Bytes); Assert.True(next.Bytes.Length > 0);
    }

    private class AsyncOutput : Stream
    {
        private readonly MemoryStream output = new();
        internal byte[] Bytes => output.ToArray();
        internal bool WasDisposed { get; private set; }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !WasDisposed;
        public override long Length => output.Length;
        public override long Position { get => output.Position; set => throw new NotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Expected asynchronous writes.");
        public override void Write(ReadOnlySpan<byte> buffer) => throw new InvalidOperationException("Expected asynchronous writes.");
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        { cancellationToken.ThrowIfCancellationRequested(); output.Write(buffer.Span); return ValueTask.CompletedTask; }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { WasDisposed = true; if (disposing) output.Dispose(); base.Dispose(disposing); }
    }

    private sealed class BlockedOutput(bool cancellation) : AsyncOutput
    {
        internal readonly TaskCompletionSource Entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Entered.TrySetResult(); await Release.Task;
            if (cancellation) cancellationToken.ThrowIfCancellationRequested();
            throw new IOException("Expected stream failure.");
        }
    }
}
