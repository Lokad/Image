using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class PngIdatPublicationTests
{
    [Theory]
    [InlineData(8, 0, false)] [InlineData(8, 1, false)] [InlineData(8, 2, false)]
    [InlineData(8, 0, true)] [InlineData(8, 1, true)] [InlineData(8, 2, true)]
    [InlineData(16, 0, false)] [InlineData(16, 1, false)] [InlineData(16, 2, false)]
    [InlineData(16, 0, true)] [InlineData(16, 1, true)] [InlineData(16, 2, true)]
    public void HeaderPayloadAndCrcFailuresPreservePrefixChargesAndIndependentRecovery(int depth, int phase, bool cancel)
    {
        const int width = 513, height = 259;
        var sourceBytes = width * height * (depth / 2);
        var working = sourceBytes + 4096 + PngDeflater.StoredWorkspaceBytes;
        using var root = new TestMemoryBudget(working);
        var limits = new ImageLimits { MaxWorkingBytes = working, MemoryBudget = root };
        using var seed = OriginalPixels.Buffer(width, height, depth);
        using var source = ImageBuffer.Create(width, height, depth, limits);
        if (depth == 8) seed.GetRow8(0)[3] = seed.GetRow8(0)[width * 4 - 1] = 0;
        else seed.GetRow16(0)[3] = seed.GetRow16(0)[width * 4 - 1] = 0;
        for (var y = 0; y < height; y++)
            if (depth == 8) seed.GetRow8(y).CopyTo(source.GetRow8(y));
            else seed.GetRow16(y).CopyTo(source.GetRow16(y));
        var original = OriginalPixels.Bytes(source);
        var options = new PngEncodeOptions { Compression = CompressionLevel.NoCompression, Limits = limits };
        using var complete = new MemoryStream(); ImageCodec.EncodePng(source, complete, options);
        var packet = complete.ToArray();
        Assert.Equal(depth == 8
            ? "c8f17d7b3b3c1aff850539b1be0ee318956a58ec6ce555da332838ed0cc1cd3c"
            : "9b31604447d61d5ab7a30aad3cfde4cd73fef6d85ff506e74baeba807e54574d",
            Convert.ToHexStringLower(SHA256.HashData(packet)));
        var at = 8;
        while (!packet.AsSpan(at + 4, 4).SequenceEqual("IDAT"u8))
            at += 12 + BinaryPrimitives.ReadInt32BigEndian(packet.AsSpan(at, 4));
        Assert.Equal(4096, BinaryPrimitives.ReadInt32BigEndian(packet.AsSpan(at, 4)));
        using var cancellation = new CancellationTokenSource();
        using var output = new InterruptingOutput(root, working, cancellation, phase, cancel);
        var error = Record.Exception(() => ImageCodec.EncodePng(source, output, options, cancellation.Token));
        if (cancel) Assert.Equal(cancellation.Token, Assert.IsType<OperationCanceledException>(error).CancellationToken);
        else Assert.Same(output.Failure, error);
        var sizes = new[] { 8, 4096, 4 };
        var expected = at;
        for (var i = 0; i < phase + (cancel ? 1 : 0); i++) expected += sizes[i];
        Assert.Equal(packet.AsSpan(0, expected).ToArray(), output.Bytes);
        Assert.Equal(phase + 1, output.IdatWrites); Assert.True(output.CanWrite);
        Assert.Equal(sourceBytes, root.Bytes); Assert.Equal(working, root.Peak); Assert.False(root.IsDisposed);
        Assert.Equal(original, OriginalPixels.Bytes(source));
        using var survivor = new MemoryStream(); ImageCodec.EncodePng(source, survivor, options);
        Assert.Equal(packet, survivor.ToArray()); Assert.Equal(sourceBytes, root.Bytes);
        source.Dispose(); complete.Dispose(); Assert.Equal(0, root.Bytes);
        Assert.Equal(packet, survivor.ToArray()); Assert.True(survivor.CanWrite);
    }

    private sealed class InterruptingOutput(TestMemoryBudget root, long working,
        CancellationTokenSource cancellation, int phase, bool cancel) : Stream
    {
        private readonly MemoryStream data = new();
        private bool inIdat, disposed;
        public IOException Failure { get; } = new("Authored IDAT publication failure.");
        public byte[] Bytes => data.ToArray();
        public int IdatWrites { get; private set; }
        public override void Write(ReadOnlySpan<byte> bytes)
        {
            // The producer's storage is still charged during every actual user.
            Assert.Equal(working, root.Bytes); Assert.False(root.IsDisposed);
            if (!inIdat && bytes.Length == 8 && bytes[4..].SequenceEqual("IDAT"u8)) inIdat = true;
            if (inIdat && IdatWrites++ == phase)
            {
                if (!cancel) throw Failure;
                data.Write(bytes); cancellation.Cancel(); return;
            }
            data.Write(bytes);
        }
        public override void Write(byte[] bytes, int offset, int count) => Write(bytes.AsSpan(offset, count));
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => !disposed;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override int Read(byte[] bytes, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) data.Dispose(); disposed = true; base.Dispose(disposing); }
    }
}
