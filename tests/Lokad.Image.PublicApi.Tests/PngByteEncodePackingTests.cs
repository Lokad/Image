using System.Buffers.Binary;
using System.IO.Compression;
using Lokad.Image.Tooling;

namespace Lokad.Image.PublicApi.Tests;

public sealed class PngByteEncodePackingTests
{
    [Theory]
    [InlineData(PngColorType.Grayscale)]
    [InlineData(PngColorType.Rgb)]
    [InlineData(PngColorType.GrayscaleAlpha)]
    public void PackedRowsPreserveExactChannelsAcrossPixelCheckpoints(PngColorType color)
    {
        foreach (var width in new[] { 1, 4095, 4096, 4097, 8193 })
        {
            using var source = OriginalPixels.Buffer(width, 2, 8,
                grayscale: color != PngColorType.Rgb, opaque: color != PngColorType.GrayscaleAlpha);
            var original = OriginalPixels.Bytes(source);
            using var output = new MemoryStream();
            ImageCodec.EncodePng(source, output, new PngEncodeOptions { ColorType = color });
            var png = output.ToArray(); using var compressed = new MemoryStream();
            for (var position = 8; position < png.Length;)
            {
                var length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(position)));
                if (png.AsSpan(position + 4, 4).SequenceEqual("IDAT"u8)) compressed.Write(png.AsSpan(position + 8, length));
                position = checked(position + length + 12);
            }
            compressed.Position = 0; using var inflater = new ZLibStream(compressed, CompressionMode.Decompress);
            for (var y = 0; y < source.Height; y++)
            {
                Assert.Equal(0, inflater.ReadByte());
                for (var x = 0; x < width; x++)
                {
                    var index = y * source.Stride + x * 4;
                    Assert.Equal(original[index], inflater.ReadByte());
                    if (color == PngColorType.Rgb)
                    {
                        Assert.Equal(original[index + 1], inflater.ReadByte());
                        Assert.Equal(original[index + 2], inflater.ReadByte());
                    }
                    if (color == PngColorType.GrayscaleAlpha) Assert.Equal(original[index + 3], inflater.ReadByte());
                }
            }
            Assert.Equal(-1, inflater.ReadByte()); Assert.Equal(original, OriginalPixels.Bytes(source));
            Assert.True(output.CanWrite);
        }
    }

    [Fact]
    public void FirstInvalidPixelAndHeaderCancellationRetainErrorOrdering()
    {
        foreach (var color in new[] { PngColorType.Grayscale, PngColorType.Rgb, PngColorType.GrayscaleAlpha })
        foreach (var position in new[] { 0, 1, 15, 16, 4095, 4096 })
        {
            using var source = OriginalPixels.Buffer(4097, 1, 8, grayscale: true, opaque: true);
            var pixel = source.GetRow8(0).Slice(position * 4, 4);
            pixel[1] ^= 1; pixel[3] = 0;
            using var output = new MemoryStream();
            var options = new PngEncodeOptions { ColorType = color };
            var error = Assert.Throws<ArgumentException>(() => ImageCodec.EncodePng(source, output, options));
            Assert.Equal("source", error.ParamName);
            Assert.StartsWith(color == PngColorType.Rgb ? "Encoding without alpha requires opaque pixels."
                : "Grayscale encoding requires equal RGB channels.", error.Message);
            Assert.True(output.CanWrite);
            using var cancellation = new CancellationTokenSource(); using var canceledOutput = new CancellingOutput(cancellation);
            var canceled = Assert.Throws<OperationCanceledException>(() => ImageCodec.EncodePng(source, canceledOutput, options, cancellation.Token));
            Assert.Equal(cancellation.Token, canceled.CancellationToken); Assert.True(canceledOutput.CanWrite);
        }
    }

    private sealed class CancellingOutput(CancellationTokenSource cancellation) : MemoryStream
    {
        public override void Write(ReadOnlySpan<byte> bytes) { base.Write(bytes); cancellation.Cancel(); }
    }
}
