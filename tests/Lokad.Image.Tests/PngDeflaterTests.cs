using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PngDeflaterTests
{
    [Theory]
    [InlineData(32)] [InlineData(2048)] [InlineData(32768)] [InlineData(65532)]
    public void StoredWordRowsMatchIndependentNetworkOrderAcrossHardSplits(int samples)
    {
        var input = new ushort[samples + 2]; input[0] = 0x55aa; input[^1] = 0xaa55;
        using var root = new TestMemoryBudget(2 * PngDeflater.StoredWorkspaceBytes);
        using var expectedContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var actualContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var expected = new MemoryStream(); using var actual = new MemoryStream();
        using (var generic = new PngDeflater(expected, expectedContext, 0))
        using (var typed = new PngDeflater(actual, actualContext, 0))
        {
            for (var y = 0; y < 3; y++)
            {
                var network = new byte[samples * 2];
                for (var i = 0; i < samples; i++)
                {
                    input[i + 1] = y == 1 ? ushort.MaxValue : (ushort)(i * 71 + y * 123 + (i >> 7) * 2053);
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(network.AsSpan(i * 2), input[i + 1]);
                }
                var original = input.ToArray();
                generic.WriteRow(network); typed.WriteStoredRow16(input.AsSpan(1, samples));
                Assert.Equal(original, input);
            }
            generic.Complete(); typed.Complete();
        }
        Assert.Equal(expected.ToArray(), actual.ToArray());
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Theory]
    [InlineData(2, false)] [InlineData(9, false)]
    [InlineData(2, true)] [InlineData(9, true)]
    public void StoredChunkFailurePreservesPublishedPrefixPendingBytesAndAdmission(int stopAt, bool cancel)
    {
        var input = new byte[65531];
        for (var i = 0; i < input.Length; i++) input[i] = (byte)(i * 31 + 11);
        var original = input.ToArray();
        // RFC1950 header and one nonfinal RFC1951 stored block, LEN=65531.
        var expected = new byte[7 + input.Length];
        new byte[] { 0x78, 0x01, 0, 0xfb, 0xff, 4, 0 }.CopyTo(expected, 0);
        input.CopyTo(expected, 7);
        using var root = new TestMemoryBudget(PngDeflater.StoredWorkspaceBytes);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        using var output = new StoredChunkFailureOutput(stopAt, cancel ? cancellation : null,
            () => Assert.Equal(PngDeflater.StoredWorkspaceBytes, root.Bytes));
        var deflater = new PngDeflater(output, context, 0);
        try
        {
            var error = Record.Exception(() => deflater.Write(input));
            if (cancel) Assert.Equal(cancellation.Token, Assert.IsType<OperationCanceledException>(error).CancellationToken);
            else Assert.Same(output.Failure, error);
            Assert.Equal(PngDeflater.StoredWorkspaceBytes, root.Bytes);
            var published = (cancel ? stopAt : stopAt - 1) * 4096;
            Assert.Equal(expected[..published], output.ToArray());
            if (!cancel)
            {
                deflater.Flush();
                Assert.Equal(expected[..(stopAt * 4096)], output.ToArray());
            }
        }
        finally { var writes = output.Writes; deflater.Dispose(); Assert.Equal(writes, output.Writes); }
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed); Assert.Equal(original, input);
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var nextOutput = new MemoryStream();
        using (var next = new PngDeflater(nextOutput, nextContext, 0)) { next.Write(input); next.Complete(); }
        Assert.Equal(0, root.Bytes);
    }

    private sealed class StoredChunkFailureOutput(int stopAt, CancellationTokenSource? cancellation,
        Action observeAdmission) : MemoryStream
    {
        internal int Writes { get; private set; }
        internal IOException Failure { get; } = new("Authored stored-chunk failure.");
        public override void Write(ReadOnlySpan<byte> data)
        {
            observeAdmission(); Writes++;
            if (Writes == stopAt && cancellation is null) throw Failure;
            base.Write(data);
            if (Writes == stopAt) cancellation!.Cancel();
        }
    }

    [Fact]
    public void StoredTrailerMatchesLiteralChecksumForUnalignedBoundaryPayloads()
    {
        foreach (var length in new[] { 63, 64, 65, 4095, 4096, 4097, 65530, 65531, 65532 })
        foreach (var pattern in new[] { 0, 1, 2 })
        {
            var input = new byte[length + 2]; input[0] = 0xa7; input[^1] = 0x5a;
            uint a = 1, b = 0;
            for (var i = 0; i < length; i++)
            {
                var value = pattern == 0 ? (byte)0 : pattern == 1 ? (byte)255
                    : (byte)(i * 71 + (i >> 8) * 19 + 17);
                input[i + 1] = value;
                a = (a + value) % 65521; b = (b + a) % 65521;
            }
            byte[]? original = null;
            foreach (var chunk in new[] { 65, 65536 })
            {
                using var root = new TestMemoryBudget(PngDeflater.StoredWorkspaceBytes);
                using var context = new OperationContext(new() { MemoryBudget = root }, default);
                using var output = new MemoryStream();
                using (var deflater = new PngDeflater(output, context, 0))
                {
                    for (var at = 0; at < length; at += chunk)
                        deflater.Write(input.AsSpan(at + 1, Math.Min(chunk, length - at)));
                    deflater.Complete();
                }
                var packet = output.ToArray();
                Assert.Equal((b << 16) | a,
                    System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(packet.AsSpan(packet.Length - 4)));
                if (original is null) original = packet;
                else Assert.Equal(original, packet);
                Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
                Assert.Equal(0xa7, input[0]); Assert.Equal(0x5a, input[^1]);
            }
        }
    }

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
        var original = input.ToArray();
        foreach (var level in new[] { 0, 1, 6, 9 })
        {
            byte[]? expected = null;
            foreach (var chunk in new[] { 1, 262, 65536 })
            {
                using var root = new TestMemoryBudget(PngDeflater.WorkspaceBytes);
                using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default);
                using var output = new MemoryStream();
                using (var deflater = new PngDeflater(output, context, level))
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
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(6)] [InlineData(9)]
    public void SharedRefusalIsTerminalAndReleasedCapacityAdmitsSubsequentEncoding(int level)
    {
        using var root = new TestMemoryBudget(level == 0 ? PngDeflater.StoredWorkspaceBytes : PngDeflater.WorkspaceBytes);
        using var blocker = root.CreateScope(); Assert.True(blocker.TryReserve(1, out var held));
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, default);
        using var output = new MemoryStream();
        var error = Assert.Throws<ImageLimitException>(() => new PngDeflater(output, context, level));
        Assert.Equal(ImageLimitScope.SharedBudget, error.Scope); Assert.Equal(1, root.Refusals);
        Assert.Equal(1, root.Bytes); Assert.Equal(0, output.Length);
        held!.Dispose();
        using (var next = new PngDeflater(output, context, level)) { next.Write([1, 2, 3]); next.Complete(); }
        Assert.Equal(0, root.Bytes);
    }

    [Theory]
    [InlineData(0, false)] [InlineData(0, true)] [InlineData(1, false)] [InlineData(1, true)]
    [InlineData(6, false)] [InlineData(6, true)] [InlineData(9, false)] [InlineData(9, true)]
    public void CancellationAndWriterFailureDoNotFlushDuringCleanup(int level, bool cancel)
    {
        using var root = new TestMemoryBudget(PngDeflater.WorkspaceBytes);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root }, cancellation.Token);
        using var output = new FailingOutput(cancellation, cancel);
        var deflater = new PngDeflater(output, context, level);
        var error = Record.Exception(() => { deflater.Write(new byte[100000]); deflater.Complete(); });
        if (cancel) Assert.Equal(cancellation.Token, Assert.IsType<OperationCanceledException>(error).CancellationToken);
        else Assert.Same(output.Failure, error);
        var writes = output.Writes; deflater.Dispose(); deflater.Dispose();
        Assert.Equal(writes, output.Writes); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void StoredBlocksKeepTheCompleteScanlineBoundary()
    {
        using var context = new OperationContext(ImageLimits.Default, default);
        using var output = new MemoryStream();
        using (var deflater = new PngDeflater(output, context, 0))
        {
            // Seven filtered rows total32767 bytes. The next filter byte alone
            // reaches32768, but the stored block must include its full row.
            for (var row = 0; row < 8; row++) deflater.WriteRow(new byte[4680]);
            deflater.Complete();
        }
        var bytes = output.ToArray();
        Assert.Equal(0, bytes[2]);
        Assert.Equal(37448, bytes[3] | bytes[4] << 8);
        using var input = new MemoryStream(bytes);
        using var inflater = new ZLibStream(input, CompressionMode.Decompress);
        using var decoded = new MemoryStream(); inflater.CopyTo(decoded);
        Assert.Equal(new byte[37448], decoded.ToArray());
    }

    [Theory]
    [InlineData(1, false)] [InlineData(1, true)]
    [InlineData(6, false)] [InlineData(6, true)]
    public void FailureAfterDictionarySlidesReleasesStorageAndTheBorrowedRootCanEncodeAgain(int level, bool cancel)
    {
        var data = OriginalDeflateMatch.Create(false); var original = data.ToArray();
        using var root = new TestMemoryBudget(PngDeflater.WorkspaceBytes);
        using var cancellation = new CancellationTokenSource();
        using var context = new OperationContext(new() { MemoryBudget = root }, cancellation.Token);
        PngDeflater? compressor = null; var reachedSlide = false;
        using var sink = new LateFailureOutput(() =>
        {
            var origin = (long)typeof(PngDeflater).GetField("windowOrigin", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(compressor)!;
            reachedSlide = origin > 0; Assert.True(reachedSlide);
            if (cancel) cancellation.Cancel();
        }, cancel);
        using (compressor = new PngDeflater(sink, context, level))
        {
            var error = Record.Exception(() => { compressor.Write(data); compressor.Complete(); });
            if (cancel) Assert.Equal(cancellation.Token, Assert.IsType<OperationCanceledException>(error).CancellationToken);
            else Assert.Same(sink.Failure, error);
            var writes = sink.Writes; compressor.Dispose(); Assert.Equal(writes, sink.Writes);
        }
        Assert.True(reachedSlide); Assert.Equal(0, root.Bytes); Assert.Equal(original, data);
        using var nextContext = new OperationContext(new() { MemoryBudget = root }, default);
        using var output = new MemoryStream();
        using (var next = new PngDeflater(output, nextContext, level)) { next.Write(data); next.Complete(); }
        Assert.Equal(level == 1 ? "69e71b5130919c68443efb049041b9046d46271e15f4861b0538637e38a25527"
            : "4f018a3e97ec49f78ddb73365caace2d6f8012beb026ffcb6033f38f11148ab7",
            Convert.ToHexStringLower(SHA256.HashData(output.ToArray())));
        Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
    }

    private sealed class LateFailureOutput(Action callback, bool cancel) : Stream
    {
        private long bytes;
        internal int Writes { get; private set; }
        internal IOException Failure { get; } = new("Authored failure after real dictionary slides.");
        public override void Write(ReadOnlySpan<byte> data)
        {
            Writes++; bytes += data.Length;
            if (bytes < 80 * 1024) return;
            callback(); if (!cancel) throw Failure;
        }
        public override void Write(byte[] data, int offset, int count) => Write(data.AsSpan(offset, count));
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] data, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long length) => throw new NotSupportedException();
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
