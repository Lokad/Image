using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngGray8TransparentRowTests
{
    [Theory]
    [InlineData(1, 127)] [InlineData(7, 0)] [InlineData(8, 255)] [InlineData(9, 127)]
    [InlineData(16, 256)] [InlineData(17, 65535)] [InlineData(256, 127)] [InlineData(513, 127)]
    [InlineData(4095, 127)] [InlineData(4096, 127)] [InlineData(4097, 127)]
    public void KeepsFullKeyComparisonHiddenRgbTailsAndUnalignedGuards(int width, ushort key)
    {
        var input = new byte[width + 2];
        for (var x = 0; x < width; x++) input[x + 1] = (byte)x;
        var original = input.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
        PngCodec.CopyGray8TransparentRow(input.AsSpan(1, width), output.AsSpan(5, width * 4), key, default);
        for (var x = 0; x < width; x++)
        {
            var value = original[x + 1];
            for (var channel = 0; channel < 3; channel++) Assert.Equal(value, output[5 + x * 4 + channel]);
            Assert.Equal(value == key ? (byte)0 : (byte)255, output[8 + x * 4]);
        }
        Assert.Equal(original, input);
        Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 4 + 5), value => Assert.Equal((byte)0xa5, value));
    }

    [Fact]
    public void EveryRepresentableKeyMakesOnlyItsOwnSamplesTransparent()
    {
        var input = Enumerable.Range(0, 513).Select(x => (byte)x).ToArray(); var output = new byte[513 * 4];
        for (ushort key = 0; key < 256; key++)
        {
            PngCodec.CopyGray8TransparentRow(input, output, key, default);
            for (var x = 0; x < input.Length; x++)
            {
                Assert.Equal(input[x], output[x * 4]); Assert.Equal(input[x], output[x * 4 + 1]);
                Assert.Equal(input[x], output[x * 4 + 2]);
                Assert.Equal(input[x] == key ? (byte)0 : (byte)255, output[x * 4 + 3]);
            }
        }
    }

    [Theory]
    [InlineData(1)] [InlineData(8)]
    public void ChecksCancellationBeforeWriting(int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 4).ToArray();
        Assert.Throws<OperationCanceledException>(() => PngCodec.CopyGray8TransparentRow(
            new byte[width], output, 0, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
