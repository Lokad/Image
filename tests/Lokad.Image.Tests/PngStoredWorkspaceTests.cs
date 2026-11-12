using System.IO.Compression;
using System.Reflection;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PngStoredWorkspaceTests
{
    private static Array Buffer(PngDeflater encoder, string name)
        => (Array)typeof(PngDeflater).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(encoder)!;

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(6)] [InlineData(9)]
    public void ImmutableModeOwnsOnlyRequiredBuffersAndReleasesItsExactCharge(int level)
    {
        var capacity = level == 0 ? PngDeflater.StoredWorkspaceBytes : PngDeflater.WorkspaceBytes;
        using var root = new TestMemoryBudget(capacity);
        using var context = new OperationContext(new() { MaxWorkingBytes = capacity, MemoryBudget = root }, default);
        var output = Stream.Null;
        long start = 0, admittedAllocation = -1;
        root.Admitted = _ => admittedAllocation = GC.GetAllocatedBytesForCurrentThread() - start;
        start = GC.GetAllocatedBytesForCurrentThread();
        var encoder = new PngDeflater(output, context, level);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - start;
        root.Admitted = null;
        using (encoder)
        {
            Assert.InRange(admittedAllocation, 0, 8192);
            Assert.Equal(capacity, root.Bytes); Assert.Equal(0, context.RemainingWorkingBytes);
            Assert.Equal(65536, Buffer(encoder, "history").Length);
            Assert.Equal(4096, Buffer(encoder, "encoded").Length);
            Assert.Equal(level == 0 ? 0 : 32768, Buffer(encoder, "heads").Length);
            Assert.Equal(level == 0 ? 0 : 32768, Buffer(encoder, "older").Length);
            Assert.Equal(level == 0 ? 0 : 16383, Buffer(encoder, "tokens").Length);
            var tree = typeof(PngDeflater).GetField("treeWorkspace", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(encoder);
            if (level == 0)
            {
                Assert.Null(tree); Assert.InRange(allocated, 69632, PngDeflater.StoredWorkspaceBytes);
                encoder.Complete(); Assert.Equal(capacity, root.Bytes);
            }
            else Assert.NotNull(tree);
        }
        encoder.Dispose();
        Assert.Equal(0, root.Bytes); Assert.Equal(capacity, context.RemainingWorkingBytes);
        foreach (var name in new[] { "history", "encoded", "heads", "older", "tokens" }) Assert.Empty(Buffer(encoder, name));
        Assert.False(root.IsDisposed); Assert.True(output.CanWrite);
    }

    [Fact]
    public void AggregateStoredAdmissionRefusesBeforeAllocationAndReleasedCapacityCanBeReused()
    {
        const int capacity = PngDeflater.StoredWorkspaceBytes;
        using var root = new TestMemoryBudget(2L * capacity);
        using var a = new OperationContext(new() { MaxWorkingBytes = capacity, MemoryBudget = root }, default);
        using var b = new OperationContext(new() { MaxWorkingBytes = capacity, MemoryBudget = root }, default);
        using var c = new OperationContext(new() { MaxWorkingBytes = capacity, MemoryBudget = root }, default);
        using var first = new PngDeflater(Stream.Null, a, 0);
        using var second = new PngDeflater(Stream.Null, b, 0);
        ImageLimitException? failure = null;
        var start = GC.GetAllocatedBytesForCurrentThread();
        try { using var refused = new PngDeflater(Stream.Null, c, 0); }
        catch (ImageLimitException e) { failure = e; }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - start;
        Assert.NotNull(failure); Assert.Equal(ImageLimitScope.SharedBudget, failure.Scope);
        Assert.InRange(allocated, 0, capacity / 2);
        Assert.Equal(1, root.Refusals); Assert.Equal(2L * capacity, root.Bytes);
        Assert.Equal(capacity, c.RemainingWorkingBytes);
        first.Dispose(); Assert.Equal(capacity, root.Bytes);
        using (var next = new PngDeflater(Stream.Null, c, 0))
        {
            next.WriteRow([1, 2, 3]); next.Complete(); Assert.Equal(2L * capacity, root.Bytes);
        }
        second.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void LocalStoredBoundaryRejectsBeforeSharedAdmission()
    {
        using var root = new TestMemoryBudget(PngDeflater.WorkspaceBytes);
        using var context = new OperationContext(new() { MaxWorkingBytes = PngDeflater.StoredWorkspaceBytes - 1, MemoryBudget = root }, default);
        var error = Assert.Throws<ImageLimitException>(() => new PngDeflater(Stream.Null, context, 0));
        Assert.Equal(ImageLimitKind.WorkingBytes, error.Limit); Assert.Equal(ImageLimitScope.Operation, error.Scope);
        Assert.Equal(0, root.Attempts); Assert.Equal(0, root.Bytes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void StoredFailureAndCancellationReleaseActualUsersWithoutCleanupWrites(bool cancel)
    {
        using var root = new TestMemoryBudget(PngDeflater.StoredWorkspaceBytes);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MaxWorkingBytes = PngDeflater.StoredWorkspaceBytes, MemoryBudget = root }, cancellation.Token);
        using var output = new FailingOutput(cancellation, cancel);
        var encoder = new PngDeflater(output, context, 0);
        var error = Record.Exception(() => { encoder.Write(new byte[100000]); encoder.Complete(); });
        if (cancel) Assert.Equal(cancellation.Token, Assert.IsType<OperationCanceledException>(error).CancellationToken);
        else Assert.Same(output.Failure, error);
        var writes = output.Writes; encoder.Dispose(); encoder.Dispose();
        Assert.Equal(writes, output.Writes); Assert.Equal(0, root.Bytes); Assert.True(output.CanWrite);
        using var retry = new OperationContext(new() { MemoryBudget = root }, default);
        using (var next = new PngDeflater(Stream.Null, retry, 0)) { next.Write([9, 7, 5]); next.Complete(); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void PublicStoredEncodingComposesWithRetainedSourceAndIndependentPacket(int depth)
    {
        const int width = 19, height = 13;
        var sourceBytes = width * height * 4 * (depth / 8);
        var working = sourceBytes + width * 4 * (depth / 8) + 4096 + PngDeflater.StoredWorkspaceBytes;
        using var root = new TestMemoryBudget(working);
        var limits = new ImageLimits { MaxWorkingBytes = working, MemoryBudget = root };
        using var source = ImageBuffer.Create(width, height, depth, limits);
        for (var y = 0; y < height; y++) for (var x = 0; x < width * 4; x++)
        {
            if (depth == 8) source.GetRow8(y)[x] = (byte)(x * 71 + y * 13);
            else source.GetRow16(y)[x] = (ushort)(x * 977 + y * 1237 + 17);
        }
        var pixels = OriginalPixels.Bytes(source);
        using var output = new MemoryStream();
        ImageCodec.EncodePng(source, output, new() { Compression = CompressionLevel.NoCompression, Limits = limits });
        Assert.Equal(working, root.Peak); Assert.Equal(sourceBytes, root.Bytes);
        Assert.Equal(pixels, OriginalPixels.Bytes(source)); source.Dispose();
        Assert.Equal(0, root.Bytes); Assert.True(output.CanWrite); Assert.False(root.IsDisposed);
        using var decoded = ImageCodec.Decode(output.ToArray());
        Assert.Equal(depth, decoded.BitDepth); Assert.Equal(pixels, OriginalPixels.Bytes(decoded));
    }

    private sealed class FailingOutput(CancellationTokenSource cancellation, bool cancel) : Stream
    {
        public IOException Failure { get; } = new("Original stored-writer failure.");
        public int Writes { get; private set; }
        private bool disposed;
        public override void Write(ReadOnlySpan<byte> data)
        {
            Writes++;
            if (!cancel) throw Failure;
            cancellation.Cancel();
        }
        public override void Write(byte[] data, int offset, int count) => Write(data.AsSpan(offset, count));
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !disposed;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] data, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { disposed = true; base.Dispose(disposing); }
    }
}
