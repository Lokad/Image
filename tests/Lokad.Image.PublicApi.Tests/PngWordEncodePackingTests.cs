using System.Buffers.Binary;
using System.IO.Compression;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngWordEncodePackingTests
{
    [Theory]
    [InlineData(PngColorType.Grayscale)]
    [InlineData(PngColorType.Rgb)]
    [InlineData(PngColorType.GrayscaleAlpha)]
    public void FullPrecisionChannelsAreBigEndianAcrossGroupsRowsAndCheckpoints(PngColorType color)
    {
        foreach (var width in new[] { 1, 7, 8, 9, 4095, 4096, 4097, 8193 })
        {
            using var source = OriginalPixels.Buffer(width, 2, 16,
                grayscale: color != PngColorType.Rgb, opaque: color != PngColorType.GrayscaleAlpha);
            ushort[] samples = [0, 1, 255, 256, 257, 32769, 65534, 65535];
            for (var y = 0; y < source.Height; y++)
            for (var x = 0; x < width; x++)
            {
                var pixel = source.GetRow16(y).Slice(x * 4, 4);
                pixel[0] = samples[(x + y) & 7];
                pixel[1] = color == PngColorType.Rgb ? samples[(x + y + 3) & 7] : pixel[0];
                pixel[2] = color == PngColorType.Rgb ? samples[(x + y + 5) & 7] : pixel[0];
                pixel[3] = color == PngColorType.GrayscaleAlpha ? samples[(x + y + 2) & 7] : ushort.MaxValue;
            }
            var original = OriginalPixels.Bytes(source);
            using var output = new MemoryStream();
            ImageCodec.EncodePng(source, output, new PngEncodeOptions
            { ColorType = color, Compression = CompressionLevel.NoCompression });
            var png = output.ToArray();
            using var compressed = new MemoryStream();
            for (var position = 8; position < png.Length;)
            {
                var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(position)));
                if (png.AsSpan(position + 4, 4).SequenceEqual("IDAT"u8)) compressed.Write(png.AsSpan(position + 8, length));
                position = checked(position + length + 12);
            }
            compressed.Position = 0;
            using var inflater = new ZLibStream(compressed, CompressionMode.Decompress);
            int[] channels = color == PngColorType.Rgb ? [0, 1, 2]
                : color == PngColorType.GrayscaleAlpha ? [0, 3] : [0];
            var row = new byte[width * channels.Length * 2];
            for (var y = 0; y < source.Height; y++)
            {
                Assert.Equal(0, inflater.ReadByte()); inflater.ReadExactly(row);
                var position = 0;
                for (var x = 0; x < width; x++)
                foreach (var channel in channels)
                {
                    Assert.Equal(source.GetRow16(y)[x * 4 + channel], BinaryPrimitives.ReadUInt16BigEndian(row.AsSpan(position)));
                    position += 2;
                }
            }
            Assert.Equal(-1, inflater.ReadByte()); Assert.True(output.CanWrite);
            Assert.Equal(original, OriginalPixels.Bytes(source));
            using var decoded = ImageCodec.Decode(png);
            Assert.Equal(original, OriginalPixels.Bytes(decoded));
        }
    }

    [Theory]
    [InlineData(PngColorType.Grayscale)]
    [InlineData(PngColorType.Rgb)]
    [InlineData(PngColorType.GrayscaleAlpha)]
    public void FirstInvalidWordPixelKeepsRgbBeforeOpacityOrdering(PngColorType color)
    {
        foreach (var position in new[] { 0, 1, 7, 8, 4095, 4096 })
        {
            using var source = OriginalPixels.Buffer(4097, 1, 16, grayscale: true, opaque: true);
            var pixel = source.GetRow16(0).Slice(position * 4, 4); pixel[1] ^= 1; pixel[3] = 0;
            var original = OriginalPixels.Bytes(source);
            using var output = new MemoryStream();
            var error = Assert.Throws<ArgumentException>(() => ImageCodec.EncodePng(source, output,
                new PngEncodeOptions { ColorType = color }));
            Assert.Equal("source", error.ParamName);
            Assert.StartsWith(color == PngColorType.Rgb ? "Encoding without alpha requires opaque pixels."
                : "Grayscale encoding requires equal RGB channels.", error.Message);
            Assert.True(output.CanWrite); Assert.Equal(original, OriginalPixels.Bytes(source));
        }
    }

    [Fact]
    public void EarlierOpacityFailurePrecedesLaterRgbFailureInOneGroup()
    {
        using var source = OriginalPixels.Buffer(9, 1, 16, grayscale: true, opaque: true);
        source.GetRow16(0)[3] = 0; source.GetRow16(0)[5] ^= 1;
        using var output = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() => ImageCodec.EncodePng(source, output,
            new PngEncodeOptions { ColorType = PngColorType.Grayscale }));
        Assert.StartsWith("Encoding without alpha requires opaque pixels.", error.Message);
        Assert.Equal("source", error.ParamName); Assert.True(output.CanWrite);
    }

    [Theory]
    [InlineData(PngColorType.Grayscale)]
    [InlineData(PngColorType.Rgb)]
    [InlineData(PngColorType.GrayscaleAlpha)]
    public void HeaderCancellationPrecedesWordValidationAndPreservesCallerOwners(PngColorType color)
    {
        using var source = OriginalPixels.Buffer(4097, 2, 16, grayscale: true, opaque: true);
        source.GetRow16(0)[1] ^= 1; source.GetRow16(0)[3] = 0;
        var original = OriginalPixels.Bytes(source);
        using var cancellation = new CancellationTokenSource();
        using var output = new CancellingOutput(cancellation);
        var error = Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodePng(source, output,
            new PngEncodeOptions { ColorType = color }, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.True(output.CanWrite); Assert.Equal(original, OriginalPixels.Bytes(source));
    }

    private sealed class CancellingOutput(CancellationTokenSource cancellation) : MemoryStream
    {
        public override void Write(ReadOnlySpan<byte> bytes) { base.Write(bytes); cancellation.Cancel(); }
    }
}
