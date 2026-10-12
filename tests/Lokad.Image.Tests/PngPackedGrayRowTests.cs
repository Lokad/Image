using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngPackedGrayRowTests
{
    public static IEnumerable<object[]> Rows()
    {
        foreach (var depth in new[] { 1, 2, 4 })
        foreach (var width in new[] { 1, 3, 4, 5, 7, 8, 9, 15, 16, 17, 256, 513, 2048, 4095, 4096, 4097 })
            yield return new object[] { depth, width };
    }

    [Theory]
    [MemberData(nameof(Rows))]
    public void PreservesPackedBitOrderScalingPaddingAndUnalignedRowGuards(int depth, int width)
    {
        var bytes = (width * depth + 7) / 8; var input = new byte[bytes + 2];
        for (var i = 0; i < bytes; i++) input[i + 1] = unchecked((byte)(i * 73 + 19));
        var original = input.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
        PngCodec.CopyPackedGrayRow(input.AsSpan(1, bytes), output.AsSpan(5, width * 4), width, depth, default);
        var maximum = (1 << depth) - 1;
        for (var x = 0; x < width; x++)
        {
            var sample = (original[1 + x * depth / 8] >> (8 - depth - x * depth % 8)) & maximum;
            var expected = (byte)(sample * 255 / maximum);
            for (var channel = 0; channel < 3; channel++) Assert.Equal(expected, output[5 + x * 4 + channel]);
            Assert.Equal((byte)255, output[8 + x * 4]);
        }
        Assert.Equal(original, input);
        Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 4 + 5), value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(1, 1)] [InlineData(1, 8)] [InlineData(2, 1)] [InlineData(2, 8)] [InlineData(4, 1)] [InlineData(4, 8)]
    public void ChecksCancellationBeforeWriting(int depth, int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 4).ToArray();
        Assert.Throws<OperationCanceledException>(() =>
            PngCodec.CopyPackedGrayRow(new byte[(width * depth + 7) / 8], output, width, depth, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
