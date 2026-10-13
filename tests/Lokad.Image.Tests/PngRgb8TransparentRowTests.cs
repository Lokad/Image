using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngRgb8TransparentRowTests
{
    [Theory]
    [InlineData(1, 48, 139, 231)] [InlineData(7, 48, 139, 231)]
    [InlineData(8, 48, 139, 231)] [InlineData(9, 48, 139, 231)]
    [InlineData(16, 48, 139, 231)] [InlineData(17, 48, 139, 231)]
    [InlineData(256, 48, 139, 231)] [InlineData(513, 48, 139, 231)]
    [InlineData(4095, 48, 139, 231)] [InlineData(4096, 48, 139, 231)] [InlineData(4097, 48, 139, 231)]
    [InlineData(8, 0, 0, 0)] [InlineData(16, 255, 255, 255)]
    [InlineData(17, 304, 139, 231)] [InlineData(17, 48, 395, 231)]
    [InlineData(17, 48, 139, 487)] [InlineData(17, 65535, 65535, 65535)]
    public void ComparesFullTripletPreservingHiddenRgbTailsAndUnalignedGuards(int width,
        ushort redKey, ushort greenKey, ushort blueKey)
    {
        var input = new byte[width * 3 + 2];
        for (var x = 0; x < width; x++)
        {
            input[x * 3 + 1] = (byte)(redKey + (x % 4 == 1 ? 1 : 0));
            input[x * 3 + 2] = (byte)(greenKey + (x % 4 == 2 ? 1 : 0));
            input[x * 3 + 3] = (byte)(blueKey + (x % 4 == 3 ? 1 : 0));
        }
        var original = input.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
        PngCodec.CopyRgb8TransparentRow(input.AsSpan(1, width * 3), output.AsSpan(5, width * 4),
            redKey, greenKey, blueKey, default);
        for (var x = 0; x < width; x++)
        {
            for (var channel = 0; channel < 3; channel++)
                Assert.Equal(original[1 + x * 3 + channel], output[5 + x * 4 + channel]);
            var match = original[1 + x * 3] == redKey && original[2 + x * 3] == greenKey && original[3 + x * 3] == blueKey;
            Assert.Equal(match ? (byte)0 : (byte)255, output[8 + x * 4]);
        }
        Assert.Equal(original, input);
        Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 4 + 5), value => Assert.Equal((byte)0xa5, value));
    }

    [Fact]
    public void EveryRepresentableKeyInEachChannelMakesOnlyTheExactTripletTransparent()
    {
        var input = new byte[513 * 3]; var output = new byte[513 * 4];
        for (var channel = 0; channel < 3; channel++)
        {
            for (var x = 0; x < 513; x++)
            {
                input[x * 3] = 48; input[x * 3 + 1] = 139; input[x * 3 + 2] = 231;
                input[x * 3 + channel] = (byte)x;
            }
            for (ushort key = 0; key < 256; key++)
            {
                ushort red = channel == 0 ? key : (ushort)48;
                ushort green = channel == 1 ? key : (ushort)139;
                ushort blue = channel == 2 ? key : (ushort)231;
                PngCodec.CopyRgb8TransparentRow(input, output, red, green, blue, default);
                for (var x = 0; x < 513; x++)
                {
                    for (var c = 0; c < 3; c++) Assert.Equal(input[x * 3 + c], output[x * 4 + c]);
                    Assert.Equal(input[x * 3 + channel] == key ? (byte)0 : (byte)255, output[x * 4 + 3]);
                }
            }
        }
    }

    [Theory]
    [InlineData(1)] [InlineData(8)]
    public void ChecksCancellationBeforeWriting(int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 4).ToArray();
        Assert.Throws<OperationCanceledException>(() => PngCodec.CopyRgb8TransparentRow(
            new byte[width * 3], output, 48, 139, 231, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
