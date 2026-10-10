using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegHuffmanTests
{
    [Theory]
    [InlineData(1)] [InlineData(4)] [InlineData(8)] [InlineData(16)]
    public void OneLengthCanonicalCodesPreserveSymbolsPaddingPositionsAndWork(int length)
    {
        var values = length == 1 ? new byte[] { 17 } : new byte[] { 17, 29, 43 };
        var lengths = new byte[16]; lengths[length - 1] = (byte)values.Length;
        // Explicit canonical codes 0,1,2 (only 0 fits a one-bit table reserving
        // all ones). Bit text/FF stuffing form an original T.81 input fixture.
        var codes = length == 1 ? new[] { 0, 0, 0 } : new[] { 0, 1, 2 };
        var bits = string.Concat(codes.Select(value => Convert.ToString(value, 2).PadLeft(length, '0')));
        bits = bits.PadRight((bits.Length + 7) / 8 * 8, '1');
        var bytes = new List<byte>();
        for (var i = 0; i < bits.Length; i += 8)
        {
            var value = Convert.ToByte(bits.Substring(i, 8), 2); bytes.Add(value);
            if (value == 255) bytes.Add(0);
        }
        var payloadBytes = bytes.Count; bytes.Add(255); bytes.Add(0xD0);
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 16 + 3 * length }, default);
        var table = new JpegHuffman(lengths, values, context);
        var reader = new JpegEntropyReader(bytes.ToArray(), 0, context);
        foreach (var code in codes) Assert.Equal(values[code], table.Read(ref reader));
        Assert.Equal(payloadBytes, reader.Position); reader.Restart(0); Assert.Equal(bytes.Count, reader.Position);
        Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void UnassignedFixedPrefixRetainsSixteenBitFailureAndWork()
    {
        var lengths = new byte[16]; lengths[3] = 2;
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 32 }, default);
        var table = new JpegHuffman(lengths, new byte[] { 17, 29 }, context);
        var reader = new JpegEntropyReader(new byte[] { 0x20, 0 }, 0, context);
        try { table.Read(ref reader); Assert.Fail("Canonical prefix 0010 is unassigned."); }
        catch (ImageDataException error) { Assert.Equal("Invalid JPEG Huffman code.", error.Message); }
        Assert.Equal(2, reader.Position); Assert.Throws<ImageLimitException>(() => context.Work(1));
    }

    [Fact]
    public void PartialCodeLimitPreservesOneBitAdmissionAndBytePosition()
    {
        var lengths = new byte[16]; lengths[3] = 2;
        var context = new OperationContext(ImageLimits.Default with { MaxWork = 19 }, default);
        var table = new JpegHuffman(lengths, new byte[] { 17, 29 }, context);
        var reader = new JpegEntropyReader(new byte[] { 0x10 }, 0, context);
        try { table.Read(ref reader); Assert.Fail("A four-bit code needs four work units."); }
        catch (ImageLimitException) { Assert.Equal(1, reader.Position); }
        Assert.Throws<ImageLimitException>(() => context.Work(1));
    }
}
