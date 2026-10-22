using System.IO.Compression;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PngDeflaterTests
{
    [Theory]
    [InlineData(0, false)] [InlineData(262, false)] [InlineData(70000, false)]
    [InlineData(16384, true)] [InlineData(140000, true)]
    public void FixedStoredAndDynamicDataRoundTripAcrossInputBoundaries(int length, bool noise)
    {
        var input = new byte[length]; uint state = 0x947ac531;
        for (var i = 0; i < input.Length; i++)
        {
            state ^= state << 13; state ^= state >> 17; state ^= state << 5;
            input[i] = noise ? (byte)state : (byte)(i % 17);
        }
        var original = input.ToArray(); byte[]? expected = null;
        foreach (var chunk in new[] { 1, 262, 65536 })
        {
            using var root = new TestMemoryBudget(PngDeflater.WorkspaceBytes);
            using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default);
            using var output = new MemoryStream();
            using (var deflater = new PngDeflater(output, context))
            {
                for (var at = 0; at < input.Length; at += chunk)
                    deflater.Write(input.AsSpan(at, Math.Min(chunk, input.Length - at)));
                deflater.Complete();
            }
            Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
            var packet = output.ToArray();
            if (expected is null) expected = packet;
            else Assert.Equal(expected, packet);
            using var encoded = new MemoryStream(packet);
            using var inflater = new ZLibStream(encoded, CompressionMode.Decompress);
            using var decoded = new MemoryStream(); inflater.CopyTo(decoded);
            Assert.Equal(original, decoded.ToArray()); Assert.Equal(original, input);
        }
    }

    [Fact]
    public void SharedRefusalIsTerminalAndReleasedCapacityAdmitsSubsequentEncoding()
    {
        using var root = new TestMemoryBudget(PngDeflater.WorkspaceBytes);
        using var blocker = root.CreateScope(); Assert.True(blocker.TryReserve(1, out var held));
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default);
        using var output = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => new PngDeflater(output, context));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
        Assert.Equal(1, root.Bytes); Assert.Equal(0, output.Length);
        held!.Dispose();
        using (var next = new PngDeflater(output, context)) { next.Write([1, 2, 3]); next.Complete(); }
        Assert.Equal(0, root.Bytes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void CancellationAndWriterFailureDoNotFlushDuringCleanup(bool cancel)
    {
        using var root = new TestMemoryBudget(PngDeflater.WorkspaceBytes);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, cancellation.Token);
        using var output = new FailingOutput(cancellation, cancel);
        var deflater = new PngDeflater(output, context);
        var error = Record.Exception(() => { deflater.Write(new byte[100000]); deflater.Complete(); });
        if (cancel) Assert.Equal(cancellation.Token, Assert.IsType<OperationCanceledException>(error).CancellationToken);
        else Assert.Same(output.Failure, error);
        var writes = output.Writes; deflater.Dispose(); deflater.Dispose();
        Assert.Equal(writes, output.Writes); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private sealed class FailingOutput(CancellationTokenSource cancellation, bool cancel) : Stream
    {
        internal IOException Failure { get; } = new("Authored PNG writer failure.");
        internal int Writes { get; private set; }
        public override void Write(ReadOnlySpan<byte> bytes)
        { Writes++; if (cancel) cancellation.Cancel(); else throw Failure; }
        public override void Write(byte[] bytes, int offset, int count) => Write(bytes.AsSpan(offset, count));
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
