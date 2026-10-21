using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegAdaptiveHuffmanTests
{
    [Theory]
    [InlineData(8, JpegSampling.Grayscale, 0)] [InlineData(16, JpegSampling.Grayscale, 1)]
    [InlineData(8, JpegSampling.YCbCr444, 0)] [InlineData(16, JpegSampling.YCbCr444, 3)]
    [InlineData(8, JpegSampling.YCbCr422, 1)] [InlineData(16, JpegSampling.YCbCr422, 7)]
    [InlineData(8, JpegSampling.YCbCr420, 0)] [InlineData(16, JpegSampling.YCbCr420, 1)]
    [InlineData(8, JpegSampling.YCbCr420, 3)] [InlineData(16, JpegSampling.YCbCr420, 7)]
    public void AdaptiveTablesRetainDecodedSamplesRestartEdgesAndSource(int depth, JpegSampling sampling, int restart)
    {
        using var image = Input(depth); var original = Pixels(image);
        var options = new JpegEncodeOptions { Quality = 69, Sampling = sampling, RestartInterval = restart, BackgroundRgb = 0x204060 };
        using var output = new MemoryStream(); ImageCodec.EncodeJpeg(image, output, options); var adaptive = output.ToArray();
        using var fixedOutput = new MemoryStream(); ImageCodec.EncodeJpeg(image, fixedOutput,
            options with { Limits = new() { MaxWorkingBytes = image.ByteLength + 8192L } });
        using var expected = ImageCodec.Decode(fixedOutput.ToArray()); using var actual = ImageCodec.Decode(adaptive);
        Assert.Equal(Pixels(expected), Pixels(actual)); Assert.Equal(original, Pixels(image));
        using var repeat = new MemoryStream(); ImageCodec.EncodeJpeg(image, repeat, options); Assert.Equal(adaptive, repeat.ToArray());
        Assert.True(output.CanWrite); Assert.True(fixedOutput.CanWrite);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)]
    public void TightAdmissionKeepsOriginalOutput(int constraint)
    {
        using var image = Input(8); var options = new JpegEncodeOptions { Quality = 69, BackgroundRgb = 0x204060 };
        using var expected = new MemoryStream();
        ImageCodec.EncodeJpeg(image, expected, options with { Limits = new() { MaxWorkingBytes = image.ByteLength + 8192L } });
        var limits = ImageLimits.Default with
        {
            MaxWorkingBytes = constraint == 0 ? image.ByteLength + 8192 : ImageLimits.Default.MaxWorkingBytes,
            // One below adaptive worst-case admission, including the 4:2:0
            // DAG bound. The incremental route still executes every decision.
            MaxWork = constraint == 1 ? 15744808 : ImageLimits.Default.MaxWork
        };
        using var actual = new MemoryStream();
        ImageCodec.EncodeJpeg(image, actual, options with { Limits = limits });
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    [Fact]
    public void PrivateScratchDoesNotAliasHeldOutputsOrSurviveAsCachedPackets()
    {
        using var first = Input(16); using var second = Input(16);
        second.GetRow16(0).Fill(65535); var options = new JpegEncodeOptions { Quality = 69, BackgroundRgb = 0x204060 };
        using var one = new MemoryStream(); ImageCodec.EncodeJpeg(first, one, options); var held = one.ToArray();
        using var two = new MemoryStream(); ImageCodec.EncodeJpeg(second, two, options);
        Assert.Equal(held, one.ToArray()); Assert.NotEqual(held, two.ToArray());
        using var three = new MemoryStream(); ImageCodec.EncodeJpeg(first, three, options); Assert.Equal(held, three.ToArray());
    }

    [Fact]
    public void AdaptiveEncodingUsesTheSamePacketAtItsExactOutputQuota()
    {
        using var image = Input(16); var options = new JpegEncodeOptions { Quality = 69, BackgroundRgb = 0x204060 };
        using var reference = new MemoryStream(); ImageCodec.EncodeJpeg(image, reference, options); var expected = reference.ToArray();
        using var exact = new MemoryStream(); ImageCodec.EncodeJpeg(image, exact, options with { Limits = ImageLimits.Default with { MaxOutputBytes = expected.Length } });
        Assert.Equal(expected, exact.ToArray());
        using var shortOutput = new MemoryStream();
        Assert.Throws<ImageLimitException>(() => ImageCodec.EncodeJpeg(image, shortOutput, options with { Limits = ImageLimits.Default with { MaxOutputBytes = expected.Length - 1 } }));
        Assert.InRange(shortOutput.Length, 1L, expected.Length - 1L);
    }

    [Theory]
    [InlineData(1)] [InlineData(256)] [InlineData(40)]
    public void CanonicalTablesKeepSymbolCoverageLengthLimitAndReservedOnes(int scenario)
    {
        var frequencies = new long[256];
        if (scenario == 1) frequencies[37] = 1;
        else if (scenario == 256) Array.Fill(frequencies, 1);
        else { long a = 1, b = 1; for (var i = 0; i < scenario; i++) { frequencies[i] = a; (a, b) = (b, a + b); } }
        var context = new OperationContext(ImageLimits.Default, default); var table = JpegHuffmanBuilder.Build(frequencies, context);
        Assert.Equal(frequencies.Count(x => x > 0), table.Symbols.Length); Assert.Equal(table.Symbols.Length, table.Counts.Sum(x => (int)x));
        for (var a = 0; a < 256; a++)
        {
            Assert.Equal(frequencies[a] > 0, table.Lengths[a] > 0);
            if (table.Lengths[a] == 0) continue;
            Assert.InRange(table.Lengths[a], (byte)1, (byte)16); Assert.NotEqual((1 << table.Lengths[a]) - 1, table.Codes[a]);
            for (var b = a + 1; b < 256; b++)
            {
                var x = table.Lengths[a]; var y = table.Lengths[b]; if (y == 0) continue;
                Assert.False(x <= y ? table.Codes[a] == table.Codes[b] >> (y - x) : table.Codes[b] == table.Codes[a] >> (x - y));
            }
        }
        if (scenario == 40) Assert.Contains((byte)16, table.Lengths);
    }

    private static ImageBuffer Input(int depth)
    {
        var image = ImageBuffer.Create(129, 129, depth);
        for (var y = 0; y < image.Height; y++)
        for (var x = 0; x < image.Width; x++)
        for (var c = 0; c < 4; c++)
        {
            var value = (x * 71 + y * 43 + c * 19 + x * y * 13) & 255;
            if (depth == 8) image.GetRow8(y)[x * 4 + c] = (byte)value;
            else image.GetRow16(y)[x * 4 + c] = (ushort)(value * 257);
        }
        return image;
    }

    private static byte[] Pixels(ImageBuffer image)
    {
        var result = new byte[image.ByteLength];
        for (var y = 0; y < image.Height; y++)
        {
            var row = result.AsSpan(y * image.Stride, image.Stride);
            if (image.BitDepth == 8) image.GetRow8(y).CopyTo(row);
            else System.Runtime.InteropServices.MemoryMarshal.AsBytes(image.GetRow16(y)).CopyTo(row);
        }
        return result;
    }

}
