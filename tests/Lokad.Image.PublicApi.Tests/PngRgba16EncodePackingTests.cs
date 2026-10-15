using System.Buffers.Binary;
using System.IO.Compression;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngRgba16EncodePackingTests
{
    [Theory]
    [InlineData(1, CompressionLevel.NoCompression)]
    [InlineData(7, CompressionLevel.NoCompression)]
    [InlineData(4095, CompressionLevel.NoCompression)]
    [InlineData(4096, CompressionLevel.NoCompression)]
    [InlineData(4097, CompressionLevel.NoCompression)]
    [InlineData(8193, CompressionLevel.NoCompression)]
    [InlineData(4097, CompressionLevel.Fastest)]
    [InlineData(4097, CompressionLevel.Optimal)]
    [InlineData(4097, CompressionLevel.SmallestSize)]
    public void EncodedRowsContainExactBigEndianWordsIncludingHiddenRgb(int width, CompressionLevel compression)
    {
        using var source = OriginalPixels.Buffer(width, 2, 16);
        var expected = OriginalPixels.Bytes(source);
        using var output = new MemoryStream();
        ImageCodec.EncodePng(source, output, new PngEncodeOptions { Compression = compression });
        var png = output.ToArray();
        using var compressed = new MemoryStream();
        for (var position = 8; position < png.Length;)
        {
            var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(position)));
            if (png.AsSpan(position + 4, 4).SequenceEqual("IDAT"u8))
                compressed.Write(png.AsSpan(position + 8, length));
            position = checked(position + length + 12);
        }
        compressed.Position = 0;
        using var inflater = new ZLibStream(compressed, CompressionMode.Decompress);
        var row = new byte[width * 8];
        for (var y = 0; y < source.Height; y++)
        {
            Assert.Equal(0, inflater.ReadByte());
            inflater.ReadExactly(row);
            for (var sample = 0; sample < width * 4; sample++)
                Assert.Equal(source.GetRow16(y)[sample], BinaryPrimitives.ReadUInt16BigEndian(row.AsSpan(sample * 2)));
        }
        Assert.Equal(-1, inflater.ReadByte());
        Assert.Equal(expected, OriginalPixels.Bytes(source));
        using var decoded = ImageCodec.Decode(png);
        Assert.Equal(expected, OriginalPixels.Bytes(decoded));
    }

    [Fact]
    public void CancellationBeforePackingPreservesInputAndLeavesOutputOpen()
    {
        using var source = OriginalPixels.Buffer(4097, 2, 16);
        var expected = OriginalPixels.Bytes(source);
        using var cancellation = new CancellationTokenSource();
        using var output = new CancellingOutput(cancellation);
        Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodePng(source, output,
            new PngEncodeOptions { Compression = CompressionLevel.NoCompression }, cancellation.Token));
        Assert.True(output.CanWrite);
        Assert.Equal(expected, OriginalPixels.Bytes(source));
    }

    private sealed class CancellingOutput(CancellationTokenSource cancellation) : MemoryStream
    {
        public override void Write(ReadOnlySpan<byte> bytes) { base.Write(bytes); cancellation.Cancel(); }
    }
}
