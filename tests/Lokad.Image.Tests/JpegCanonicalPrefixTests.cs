using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegCanonicalPrefixTests
{
    [Theory]
    [InlineData(11)] [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)] [InlineData(16)]
    public void LongCanonicalCodesKeepEveryCachedPhaseSymbolsAndPadding(int length)
    {
        for (var offset = 0; offset < 8; offset++)
        {
            var context = Context();
            var counts = new byte[16]; counts[length - 1] = 3;
            var table = new JpegHuffman(counts, new byte[] { 17, 29, 43 }, context);
            var bits = new string('1', offset) + string.Concat(new[] { 0, 2, 1 }.Select(value => Convert.ToString(value, 2).PadLeft(length, '0')));
            var data = Stuff(bits); var reader = new JpegEntropyReader(data, 0, context);
            if (offset != 0) Assert.Equal((1 << offset) - 1, reader.ReadBits(offset));
            foreach (var expected in new[] { 17, 43, 29 }) Assert.Equal(expected, table.Read(ref reader));
            Assert.Equal(data.Length - 2, reader.Position); reader.Restart(0); Assert.Equal(data.Length, reader.Position);
        }
    }

    [Theory]
    [InlineData(0, 0, "Truncated JPEG entropy data.")]
    [InlineData(1, 2, "Unexpected marker in JPEG entropy data.")]
    [InlineData(2, 1, "Unexpected marker in JPEG entropy data.")]
    [InlineData(3, 1, "Truncated JPEG entropy data.")]
    [InlineData(4, 3, "Unexpected marker in JPEG entropy data.")]
    [InlineData(5, 2, "Unexpected marker in JPEG entropy data.")]
    public void TruncatedOrMarkerPrefixesKeepOriginalFailurePosition(int scenario, int position, string message)
    {
        byte[] input = scenario switch { 0 => [], 1 => [255, 0xD0], 2 => [255], 3 => [0], 4 => [0, 255, 0xD0], _ => [0, 255] };
        var context = Context(); var counts = new byte[16]; counts[15] = 1;
        var table = new JpegHuffman(counts, new byte[] { 17 }, context); var reader = new JpegEntropyReader(input, 0, context);
        try { table.Read(ref reader); Assert.Fail("Incomplete entropy must reject."); }
        catch (ImageDataException error) { Assert.Equal(message, error.Message); }
        Assert.Equal(position, reader.Position);     }

    private static OperationContext Context() => new(ImageLimits.Default, default);
    private static byte[] Stuff(string bits)
    {
        bits = bits.PadRight((bits.Length + 7) / 8 * 8, '1'); var bytes = new List<byte>();
        for (var i = 0; i < bits.Length; i += 8) { var value = Convert.ToByte(bits.Substring(i, 8), 2); bytes.Add(value); if (value == 255) bytes.Add(0); }
        bytes.Add(255); bytes.Add(0xD0); return bytes.ToArray();
    }
}
