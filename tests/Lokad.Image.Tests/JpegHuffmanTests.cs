using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegHuffmanTests
{
    [Fact]
    public void MixedShortCanonicalCodesStopAtCachedPaddingBeforeAMarker()
    {
        var lengths = new byte[16]; lengths[0] = 1; lengths[2] = 2;
        var context = new OperationContext(ImageLimits.Default, default);
        var table = new JpegHuffman(lengths, new byte[] { 17, 29, 43 }, context);
        // Codes 0,100,101, then one padding bit and RST0.
        var reader = new JpegEntropyReader(new byte[] { 0x4B, 0xFF, 0xD0 }, 0, context);
        Assert.Equal(17, table.Read(ref reader)); Assert.Equal(1, reader.Position);
        Assert.Equal(29, table.Read(ref reader)); Assert.Equal(1, reader.Position);
        Assert.Equal(43, table.Read(ref reader)); Assert.Equal(1, reader.Position);
        reader.Restart(0); Assert.Equal(3, reader.Position);
    }

    [Fact]
    public void PrefixStorageIsAdmittedBeforeItsAllocation()
    {
        var lengths = new byte[16]; lengths[0] = 1;
        const int originalPayload = 17 * 4 * 3 + 1;
        var context = new OperationContext(ImageLimits.Default with { MaxWorkingBytes = originalPayload + 511 }, default);
        Assert.Throws<ImageLimitException>(() => new JpegHuffman(lengths, new byte[] { 17 }, context));
        context.Reserve(511); Assert.Throws<ImageLimitException>(() => context.Reserve(1));
    }

    [Fact]
    public void MarkerAndTightBudgetRetainOriginalFormatFailurePrecedence()
    {
        var lengths = new byte[16]; lengths[7] = 2;
        var context = new OperationContext(ImageLimits.Default, default);
        var table = new JpegHuffman(lengths, new byte[] { 17, 29 }, context);
        var reader = new JpegEntropyReader(new byte[] { 0xFF, 0xD0 }, 0, context);
        try { table.Read(ref reader); Assert.Fail("A marker cannot supply an eight-bit code."); }
        catch (ImageDataException error) { Assert.Equal("Unexpected marker in JPEG entropy data.", error.Message); }
        Assert.Equal(2, reader.Position);     }

    [Theory]
    [InlineData(1)] [InlineData(4)] [InlineData(8)] [InlineData(9)] [InlineData(10)] [InlineData(16)]
    public void OneLengthCanonicalCodesPreserveSymbolsPaddingPositions(int length)
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
        var context = new OperationContext(ImageLimits.Default, default);
        var table = new JpegHuffman(lengths, values, context);
        var reader = new JpegEntropyReader(bytes.ToArray(), 0, context);
        foreach (var code in codes) Assert.Equal(values[code], table.Read(ref reader));
        Assert.Equal(payloadBytes, reader.Position); reader.Restart(0); Assert.Equal(bytes.Count, reader.Position);
    }

    [Fact]
    public void UnassignedFixedPrefixRetainsSixteenBitFailure()
    {
        var lengths = new byte[16]; lengths[3] = 2;
        var context = new OperationContext(ImageLimits.Default, default);
        var table = new JpegHuffman(lengths, new byte[] { 17, 29 }, context);
        var reader = new JpegEntropyReader(new byte[] { 0x20, 0 }, 0, context);
        try { table.Read(ref reader); Assert.Fail("Canonical prefix 0010 is unassigned."); }
        catch (ImageDataException error) { Assert.Equal("Invalid JPEG Huffman code.", error.Message); }
        Assert.Equal(2, reader.Position);     }

    [Fact]
    public void LongPrefixStorageIsAdmittedBeforeAllocation()
    {
        var lengths = new byte[16]; lengths[9] = 1;
        const int previousPayload = 17 * 4 * 3 + 1 + 512;
        var context = new OperationContext(ImageLimits.Default with { MaxWorkingBytes = previousPayload + 2047 }, default);
        Assert.Throws<ImageLimitException>(() => new JpegHuffman(lengths, new byte[] { 17 }, context));
        context.Reserve(2047); Assert.Throws<ImageLimitException>(() => context.Reserve(1));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void LongPrefixFallbackReportsTruncationAndMarkers(bool marker)
    {
        var lengths = new byte[16]; lengths[9] = 1;
        using var context = new OperationContext(ImageLimits.Default, default);
        var table = new JpegHuffman(lengths, new byte[] { 17 }, context);
        var input = marker ? new byte[] { 0, 255, 0xD0 } : new byte[1];
        var reader = new JpegEntropyReader(input, 0, context);
        try { table.Read(ref reader); Assert.Fail("Incomplete entropy must reject."); }
        catch (ImageDataException error)
        {
            Assert.Equal(marker ? "Unexpected marker in JPEG entropy data." : "Truncated JPEG entropy data.", error.Message);
            Assert.Equal(marker ? 3 : 1, reader.Position);
        }
    }
}
