using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegPrimitiveTests
{
    [Fact]
    public void EntropyBitsUnstuffBytesAndHandleSignAndRestarts()
    {
        var reader = new JpegEntropyReader(new byte[] { 0xFF, 0, 0b01111111, 0xFF, 0xD0, 0b10011111 }, 0, Context());
        Assert.Equal(255, reader.ReadBits(8));
        Assert.Equal(-2, reader.ReadSigned(2)); // Category 2 encodes 01 as -2.
        reader.Restart(0);
        Assert.Equal(4, reader.ReadSigned(3));
        reader.Align();
    }

    [Fact]
    public void RestartBoundariesResetBitState()
    {
        var reader = new JpegEntropyReader(new byte[] { 0b00111111, 0xFF, 0xD0, 0b10011111 }, 0, Context());
        Assert.Equal(-3, reader.ReadSigned(2));
        reader.Restart(0);
        Assert.Equal(4, reader.ReadSigned(3));
        reader.Align();
        Assert.Equal(4, reader.Position);
    }

    [Fact]
    public void CanonicalHuffmanHasIndependentBitPatterns()
    {
        var lengths = new byte[16]; lengths[0] = 1; lengths[1] = 1;
        var table = new JpegHuffman(lengths, new byte[] { 17, 29 }, Context());
        var reader = new JpegEntropyReader(new byte[] { 0b01001011 }, 0, Context());
        Assert.Equal(17, table.Read(ref reader)); Assert.Equal(29, table.Read(ref reader));
        Assert.Equal(17, table.Read(ref reader)); Assert.Equal(29, table.Read(ref reader)); reader.Align();
        lengths[0] = 2;
        Assert.Throws<ImageDataException>(() => new JpegHuffman(lengths, new byte[] { 1, 2, 3 }, Context()));
    }

    [Fact]
    public void ZigzagAndDcTransformsHaveAnalyticalExpectations()
    {
        Assert.Equal(new[] { 0, 1, 8, 16, 9, 2 }, JpegTransform.Zigzag[..6]);
        Assert.Equal(64, JpegTransform.Zigzag.Distinct().Count()); Assert.Equal(63, JpegTransform.Zigzag[^1]);
        var quantization = Enumerable.Repeat((ushort)1, 64).ToArray();
        var coefficients = new int[64]; coefficients[0] = 80;
        var output = new byte[64]; JpegTransform.Inverse(coefficients, quantization, output, Context());
        Assert.All(output, value => Assert.Equal(138, value));
        var transformed = new double[64]; JpegTransform.Forward(output, transformed, Context());
        Assert.Equal(80, transformed[0], 8);
        Assert.All(transformed.Skip(1), value => Assert.InRange(Math.Abs(value), 0, 1e-10));
    }

    private static OperationContext Context() => new(ImageLimits.Default, CancellationToken.None);
}
