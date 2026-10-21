using System.Security.Cryptography;
using System.Buffers.Binary;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class JpegFixedGeometricTests
{
    [Fact]
    public void FixedTableDeclaresEveryLegalAcSymbolAndReservesAllOnes()
    {
        using var source = ImageBuffer.Create(1, 1, 8); source.GetRow8(0).Fill(255);
        using var output = new MemoryStream(); ImageCodec.EncodeJpeg(source, output);
        var bytes = output.ToArray(); var found = false;
        for (var at = 2; bytes[at + 1] != 0xDA;)
        {
            var length = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(at + 2));
            if (bytes[at + 1] == 0xC4 && bytes[at + 4] == 0x10)
            {
                Assert.False(found); found = true;
                var code = 0; var symbols = 0;
                for (var width = 1; width <= 16; width++)
                {
                    var count = bytes[at + 4 + width];
                    Assert.True(code + count < 1 << width); code = (code + count) << 1; symbols += count;
                }
                Assert.Equal(162, symbols);
                Assert.Equal(Enumerable.Range(0, 256).Where(s => s is 0 or 240 || (s & 15) is >= 1 and <= 10),
                    bytes.AsSpan(at + 21, symbols).ToArray().Select(s => (int)s).Order());
            }
            at += 2 + length;
        }
        Assert.True(found);
    }

    [Fact]
    public void TightWorkspaceForwardOnlyPacketMatchesIndependentCoefficientPreservingRecoder()
    {
        using var source = OriginalPixels.Buffer(513, 259, 8);
        source.GetRow8(0)[3] = source.GetRow8(0)[513 * 4 - 1] = 0;
        var original = OriginalPixels.Bytes(source);
        var options = new JpegEncodeOptions { Quality = 69, Sampling = JpegSampling.YCbCr420, BackgroundRgb = 0x204060,
            Limits = new() { MaxWorkingBytes = original.Length + 8192L } };
        using var output = new MemoryStream(); using var stream = new ForwardOnlyOutput(output);
        ImageCodec.EncodeJpeg(source, stream, options);
        var packet = output.ToArray();
        Assert.Equal(60987, packet.Length);
        Assert.Equal("e85ffa2d67d64ffab746111d5dec26b3dca7825cbca90cee75b120b56dee6a5e", Hash(packet));
        using var decoded = ImageCodec.Decode(packet);
        Assert.Equal("d6ea22878331e7fcaec3ef5a08db203a2f81ff20abc9eddb1e9eb8a2286ad581", Hash(OriginalPixels.Bytes(decoded)));
        Assert.Equal(original, OriginalPixels.Bytes(source));
        using var exact = new MemoryStream(); using var exactStream = new ForwardOnlyOutput(exact);
        ImageCodec.EncodeJpeg(source, exactStream, options with { Limits = ImageLimits.Default with { MaxOutputBytes = packet.Length, MaxWorkingBytes = original.Length + 8192L } });
        Assert.Equal(packet, exact.ToArray());
        using var shortOutput = new MemoryStream(); using var shortStream = new ForwardOnlyOutput(shortOutput);
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(source, shortStream,
            options with { Limits = options.Limits with { MaxOutputBytes = packet.Length - 1 } }));
        Assert.InRange(shortOutput.Length, 1, packet.Length - 1);
        Assert.True(output.CanWrite); Assert.True(exact.CanWrite); Assert.True(shortOutput.CanWrite);
    }

    private static string Hash(byte[] value) => Convert.ToHexStringLower(SHA256.HashData(value));

    private sealed class ForwardOnlyOutput(MemoryStream output) : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => output.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Write(ReadOnlySpan<byte> buffer) => output.Write(buffer);
        public override void Write(byte[] buffer, int offset, int count) => output.Write(buffer, offset, count);
        public override void Flush() => output.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}
