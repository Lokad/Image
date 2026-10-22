using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegPrimitiveTests
{
    [Fact]
    public void MagnitudeCategoriesCoverEveryJpegCoefficientAndDifference()
    {
        // Independent T.81 category intervals: 2^(SIZE-1) through 2^SIZE-1.
        // Cover both signs throughout the complete 16-bit magnitude domain.
        Assert.Equal(0, JpegEncoder.Category(0));
        for (var category = 1; category <= 16; category++)
        for (var value = 1 << (category - 1); value < 1 << category; value++)
        {
            Assert.Equal(category, JpegEncoder.Category(value));
            Assert.Equal(category, JpegEncoder.Category(-value));
        }
    }

    [Fact]
    public void MagnitudeCategoriesKeepFullIntegerBoundariesAndOverflow()
    {
        for (var category = 1; category <= 31; category++)
        {
            var first = 1 << (category - 1); var last = (int)((1L << category) - 1);
            Assert.Equal(category - 1, JpegEncoder.Category(first - 1));
            Assert.Equal(category - 1, JpegEncoder.Category(1 - first));
            Assert.Equal(category, JpegEncoder.Category(first));
            Assert.Equal(category, JpegEncoder.Category(-first));
            Assert.Equal(category, JpegEncoder.Category(last));
            Assert.Equal(category, JpegEncoder.Category(-last));
        }
        Assert.Throws<OverflowException>(() => JpegEncoder.Category(int.MinValue));
    }

    [Fact]
    public void EightBitFlattenIsTheUniqueNearestEncodedSpaceSample()
    {
        // Original rational-error control independent of the division formula:
        // denominator 255 is odd, so exactly one integer has residual <=127.
        // All channel/alpha byte values, endpoint/near-endpoint backgrounds and
        // the asymmetric warm JPEG background channels are included.
        foreach (var background in new[] { 0, 1, 32, 64, 96, 128, 254, 255 })
        for (var alpha = 0; alpha <= 255; alpha++)
        for (var value = 0; value <= 255; value++)
        {
            var actual = JpegEncoder.Flatten8(value, alpha, background);
            var residual = actual * 255L - (value * alpha + background * (255 - alpha));
            Assert.InRange(residual, -127L, 127L);
        }
    }

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

    }

    private static OperationContext Context() => new(ImageLimits.Default, CancellationToken.None);
}
