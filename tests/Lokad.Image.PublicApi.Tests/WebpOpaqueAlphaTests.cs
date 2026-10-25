using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class WebpOpaqueAlphaTests
{
    private static byte[] Encode(ImageBuffer image, WebpEncodeOptions? options = null)
    { using var output = new MemoryStream(); ImageCodec.EncodeWebp(image, output, options); return output.ToArray(); }

    [Theory] [InlineData(28)] [InlineData(29)] [InlineData(30)] [InlineData(31)]
    public void OpaqueAndSubOpaqueAlphaKeepExactPacketsPixelsAndRiffLengths(int count)
    {
        using var opaque = ImageBuffer.Create(count, 1, 8); using var alpha = ImageBuffer.Create(count, 1, 8);
        for (var x = 0; x < count; x++)
        {
            new byte[] { (byte)(x * 17), (byte)(x * 29), (byte)(x * 47), 255 }.CopyTo(opaque.GetRow8(0)[(x * 4)..]);
            opaque.GetRow8(0).Slice(x * 4, 4).CopyTo(alpha.GetRow8(0)[(x * 4)..]); alpha.GetRow8(0)[x * 4 + 3] = 254;
        }
        var actual = Encode(opaque); var fixedAlpha = Encode(alpha);
        var payload = BinaryPrimitives.ReadInt32LittleEndian(actual.AsSpan(16));
        var fixedPayload = BinaryPrimitives.ReadInt32LittleEndian(fixedAlpha.AsSpan(16));
        Assert.Equal(fixedPayload + 20 + (fixedPayload & 1), fixedAlpha.Length);
        Assert.False(ImageCodec.Identify(actual).HasAlpha); Assert.True(ImageCodec.Identify(fixedAlpha).HasAlpha);
        using var decodedAlpha = ImageCodec.Decode(fixedAlpha); Assert.Equal(OriginalPixels.Bytes(alpha), OriginalPixels.Bytes(decodedAlpha));
        Assert.Equal(payload + 20 + (payload & 1), actual.Length);
        using var decoded = ImageCodec.Decode(actual); Assert.True(opaque.GetRow8(0).SequenceEqual(decoded.GetRow8(0)));
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void OriginalAlphaAndHiddenRgbControlRemainsExactAndDoesNotGrow(int depth)
    {
        using var source = OriginalPixels.Buffer(513, 259, depth);
        var packet = Encode(source);
        Assert.InRange(packet.Length, 1, 533100);
        using var decoded = ImageCodec.Decode(packet);
        Assert.Equal(OriginalPixels.Generate(513, 259, 8), OriginalPixels.Bytes(decoded));
    }

    [Fact]
    public void OpaqueOriginalControlLosesOneAlphaBytePerLiteralAndStaysExact()
    {
        using var source = OriginalPixels.Buffer(513, 259, 8, opaque: true); var unchanged = OriginalPixels.Bytes(source);
        var packet = Encode(source);
        Assert.InRange(packet.Length, 1, 400262); using var decoded = ImageCodec.Decode(packet);
        Assert.Equal(unchanged, OriginalPixels.Bytes(decoded)); Assert.Equal(unchanged, OriginalPixels.Bytes(source));
        var second = Encode(source); packet[0] ^= 255; Assert.Equal(second, Encode(source));
    }

    [Theory] [InlineData(8)] [InlineData(16)]
    public void OpaqueRunsKeepExactSourceScratchOutputAndNonseekableAdmission(int depth)
    {
        using var source = ImageBuffer.Create(47, 3, depth);
        for (var y = 0; y < source.Height; y++)
        for (var x = 0; x < source.Width; x++)
        {
            var color = (y * source.Width + x) / 3;
            if (depth == 8) new byte[] { (byte)(color * 17), (byte)(color * 29), (byte)(color * 47), 255 }.CopyTo(source.GetRow8(y)[(x * 4)..]);
            else new ushort[] { (ushort)((color * 12347) & 65535), (ushort)((color * 7919) & 65535), (ushort)((color * 101) & 65535), 65534 }.CopyTo(source.GetRow16(y)[(x * 4)..]);
        }
        // Fix the traversal when qualifying an exact local-memory boundary;
        // optional all-width histograms can otherwise be omitted at tighter limits.
        using var traversal = new CancellationTokenSource();
        var unchanged = OriginalPixels.Bytes(source); var reference = WebpAdmissionControl.Reference(source, cancellationToken: traversal.Token); var packet = reference.Packet;
        var limits = new ImageLimits { MaxWorkingBytes = reference.WorkingBytes, MaxOutputBytes = packet.Length };
        using var exact = new NonseekableOutput(); ImageCodec.EncodeWebp(source, exact, new() { Limits = limits }, traversal.Token);
        Assert.Equal(packet, exact.Bytes); Assert.True(exact.CanWrite);
        using var decoded = ImageCodec.Decode(packet);
        for (var y = 0; y < source.Height; y++)
        for (var c = 0; c < source.Width * 4; c++)
            Assert.Equal(depth == 8 ? source.GetRow8(y)[c] : (source.GetRow16(y)[c] + 128) / 257, decoded.GetRow8(y)[c]);
        foreach (var (denied, expected) in new[]
        {
            (limits with { MaxWorkingBytes = limits.MaxWorkingBytes - 1 }, ImageLimitKind.WorkingBytes),
            (limits with { MaxOutputBytes = limits.MaxOutputBytes - 1 }, ImageLimitKind.OutputBytes),
            (limits with { MaxWork = 1 }, ImageLimitKind.Work)
        })
        {
            using var rejected = new MemoryStream();
            var failure = Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeWebp(source, rejected, new() { Limits = denied }, traversal.Token));
            Assert.Equal(expected, failure.Limit); Assert.Equal(0, rejected.Length);
        }
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel(); using var cancelled = new MemoryStream();
        var cancel = Assert.ThrowsAny<OperationCanceledException>(() => ImageCodec.EncodeWebp(source, cancelled, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, cancel.CancellationToken); Assert.Equal(0, cancelled.Length); Assert.Equal(unchanged, OriginalPixels.Bytes(source));
    }

    [Fact]
    public void ASubOpaqueSampleSelectsOrdinaryAlphaAfterSixteenBitNormalization()
    {
        using var source = OriginalPixels.Buffer(64, 1, 16, opaque: true); source.GetRow16(0)[63 * 4 + 3] = 65000;
        using var decoded = ImageCodec.Decode(Encode(source));
        Assert.Equal((65000 + 128) / 257, decoded.GetRow8(0)[63 * 4 + 3]);
        Assert.Equal(255, decoded.GetRow8(0)[3]);
    }

    private sealed class NonseekableOutput : Stream
    {
        private readonly MemoryStream inner = new();
        internal byte[] Bytes => inner.ToArray();
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
        public override void Write(ReadOnlySpan<byte> buffer) => inner.Write(buffer);
        public override void WriteByte(byte value) => inner.WriteByte(value);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
    }
}
