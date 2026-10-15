using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngPackedGrayTransparentRowTests
{
    public static IEnumerable<object[]> Rows() => PngPackedGrayRowTests.Rows();

    [Theory]
    [MemberData(nameof(Rows))]
    public void KeepsFullKeyComparisonHiddenRgbPackedOrderAndUnalignedGuards(int depth, int width)
    {
        var bytes = (width * depth + 7) / 8; var input = new byte[bytes + 2];
        for (var i = 0; i < bytes; i++) input[i + 1] = unchecked((byte)(i * 73 + 19));
        var original = input.ToArray(); var maximum = (1 << depth) - 1;
        // Every representable key and two high keys also catch accidental masking
        // of the key, comparison after scaling and loss of nonzero hidden RGB.
        foreach (var key in Enumerable.Range(0, maximum + 1).Concat([256, 65535]))
        {
            var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
            PngCodec.CopyPackedGrayTransparentRow(input.AsSpan(1, bytes), output.AsSpan(5, width * 4),
                width, depth, (ushort)key, default);
            for (var x = 0; x < width; x++)
            {
                var sample = (original[1 + x * depth / 8] >> (8 - depth - x * depth % 8)) & maximum;
                var gray = (byte)(sample * 255 / maximum);
                for (var channel = 0; channel < 3; channel++) Assert.Equal(gray, output[5 + x * 4 + channel]);
                Assert.Equal(sample == key ? (byte)0 : (byte)255, output[8 + x * 4]);
            }
            Assert.Equal(original, input);
            Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
            Assert.All(output.Skip(width * 4 + 5), value => Assert.Equal((byte)0xa5, value));
        }
    }

    [Theory]
    [InlineData(1, 1)] [InlineData(1, 8)] [InlineData(2, 1)] [InlineData(2, 8)] [InlineData(4, 1)] [InlineData(4, 8)]
    public void ChecksCancellationBeforeWriting(int depth, int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 4).ToArray();
        Assert.Throws<OperationCanceledException>(() => PngCodec.CopyPackedGrayTransparentRow(
            new byte[(width * depth + 7) / 8], output, width, depth, 0, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
