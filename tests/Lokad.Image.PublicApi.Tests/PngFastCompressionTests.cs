using System.Buffers.Binary;
using System.IO.Compression;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngFastCompressionTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void FastSearchKeepsEverySampleAcrossLiveTokenAndStockOutput(int depth)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth); var original = OriginalPixels.Bytes(source);
        using var live = new CancellationTokenSource();
        var cancelable = Encode(source, token: live.Token); var candidate = Encode(source);
        Assert.Equal(cancelable, candidate);
        Assert.Equal(0, Idat(candidate)[1] >> 6);
        using var first = ImageCodec.Decode(candidate); using var second = ImageCodec.Decode(candidate);
        Assert.Equal(original, OriginalPixels.Bytes(first)); Assert.Equal(original, OriginalPixels.Bytes(second));
        if (depth == 8) first.GetRow8(0)[0] ^= 255;
        else first.GetRow16(0)[0] ^= 65535;
        Assert.Equal(original, OriginalPixels.Bytes(source)); Assert.Equal(original, OriginalPixels.Bytes(second));
    }

    [Fact]
    public void ExactBaseMemoryAdmitsTheSamePacketAndOneByteLessRefuses()
    {
        using var source = OriginalPixels.Buffer(513, 259, 16); using var live = new CancellationTokenSource();
        var cancelable = Encode(source, token: live.Token); var adapted = Encode(source);
        var rowBytes = 513 * 8;
        var peak = 513L * 259 * 8 + rowBytes + 65536 + 1024 * 1024;
        Assert.Equal(cancelable, adapted);
        Assert.Equal(adapted, Encode(source, ImageLimits.Default with { MaxWorkingBytes = peak }));
        using var rejected = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, rejected, Options(ImageLimits.Default with { MaxWorkingBytes = peak - 1 })));
        Assert.Equal(0, rejected.Length);
    }

    [Theory]
    [InlineData("memory")]  [InlineData("output")]
    public void ConstrainedFailuresKeepOriginalExceptionAndPartialPacket(string constraint)
    {
        using var source = OriginalPixels.Buffer(513, 259, 16); using var live = new CancellationTokenSource();
        var cancelable = Encode(source, token: live.Token);
        var limits = constraint switch
        {
            "memory" => ImageLimits.Default with { MaxWorkingBytes = 513L * 259 * 8 + 513 * 8 + 65536 + 1024 * 1024 - 1 },
            _ => ImageLimits.Default with { MaxOutputBytes = cancelable.Length - 1 }
        };
        using var actual = new MemoryStream(); using var reference = new MemoryStream();
        var expected = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, reference, Options(limits), live.Token));
        var error = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodePng(source, actual, Options(limits)));
        Assert.Equal(expected.Message, error.Message); Assert.Equal(reference.ToArray(), actual.ToArray());
    }

    [Fact]
    public void NonexpandableOutputKeepsOriginalPrefixAndCapacity()
    {
        using var source = OriginalPixels.Buffer(513, 259, 16); using var live = new CancellationTokenSource();
        using var reference = new MemoryStream(new byte[1024]); using var actual = new MemoryStream(new byte[1024]);
        reference.SetLength(0); actual.SetLength(0);
        Assert.Throws<NotSupportedException>(() => ImageCodec.EncodePng(source, reference, Options(), live.Token));
        Assert.Throws<NotSupportedException>(() => ImageCodec.EncodePng(source, actual, Options()));
        Assert.Equal(reference.ToArray(), actual.ToArray()); Assert.Equal(1024, actual.Capacity);
    }

    [Fact]
    public void CallerCallbackExceptionKeepsCallSequenceAndWrittenPrefix()
    {
        using var source = OriginalPixels.Buffer(513, 259, 16); using var live = new CancellationTokenSource();
        using var reference = new FailingStream(); using var actual = new FailingStream();
        Assert.Same(reference.Failure, Assert.Throws<IOException>(() => ImageCodec.EncodePng(source, reference, Options(), live.Token)));
        Assert.Same(actual.Failure, Assert.Throws<IOException>(() => ImageCodec.EncodePng(source, actual, Options())));
        Assert.Equal(reference.Writes, actual.Writes); Assert.Equal(reference.ToArray(), actual.ToArray());
    }

    [Fact]
    public void ForwardOnlyOutputUsesTheSamePacketAndLeavesCallerOpen()
    {
        using var source = OriginalPixels.Buffer(513, 259, 16); var expected = Encode(source);
        using var output = new MemoryStream(); using var forward = new ForwardOnlyOutput(output);
        using var live = new CancellationTokenSource();
        ImageCodec.EncodePng(source, forward, Options(), live.Token);
        Assert.Equal(expected, output.ToArray()); Assert.True(output.CanWrite);
    }

    [Fact]
    public void CancellationFromAnOutputCallbackPreservesTheTokenAndPublishedPrefix()
    {
        using var source = OriginalPixels.Buffer(513, 259, 16); var expected = Encode(source);
        using var cancellation = new CancellationTokenSource(); using var output = new CancelingOutput(cancellation);
        var error = Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodePng(source, output, Options(), cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken); Assert.Equal(10, output.Writes);
        Assert.Equal(expected.AsSpan(0, checked((int)output.Length)).ToArray(), output.ToArray()); Assert.True(output.CanWrite);
    }

    private static PngEncodeOptions Options(ImageLimits? limits = null) => new() { Compression = CompressionLevel.Fastest, Limits = limits ?? ImageLimits.Default };
    private static byte[] Encode(ImageBuffer source, ImageLimits? limits = null, CancellationToken token = default)
    {
        using var output = new MemoryStream(); ImageCodec.EncodePng(source, output, Options(limits), token); return output.ToArray();
    }
    private static byte[] Idat(byte[] png)
    {
        using var result = new MemoryStream();
        for (var p = 8; p < png.Length;)
        {
            var size = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(p));
            if (png.AsSpan(p + 4, 4).SequenceEqual("IDAT"u8)) result.Write(png.AsSpan(p + 8, size));
            p += size + 12;
        }
        return result.ToArray();
    }
    private sealed class FailingStream : MemoryStream
    {
        internal readonly IOException Failure = new("Original callback failure.");
        internal List<int> Writes { get; } = [];
        public override void Write(ReadOnlySpan<byte> data)
        {
            Writes.Add(data.Length); base.Write(data);
            if (Writes.Count == 10) throw Failure;
        }
    }
    private sealed class CancelingOutput(CancellationTokenSource cancellation) : MemoryStream
    {
        internal int Writes;
        public override void Write(ReadOnlySpan<byte> data)
        {
            base.Write(data); if (++Writes == 10) cancellation.Cancel();
        }
    }
    private sealed class ForwardOnlyOutput(MemoryStream target) : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Write(ReadOnlySpan<byte> data) => target.Write(data);
        public override void Write(byte[] data, int offset, int count) => target.Write(data, offset, count);
        public override void Flush() => target.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
