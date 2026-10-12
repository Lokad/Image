using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngRgb8RowTests
{
    [Theory]
    [InlineData(1)] [InlineData(7)] [InlineData(8)] [InlineData(9)]
    [InlineData(16)] [InlineData(17)] [InlineData(256)] [InlineData(513)]
    [InlineData(4095)] [InlineData(4096)] [InlineData(4097)]
    public void PreservesEveryChannelAndOpaqueAlphaWithUnalignedRowGuards(int width)
    {
        var input = new byte[width * 3 + 2];
        for (var x = 0; x < width; x++)
        {
            input[x * 3 + 1] = (byte)x;
            input[x * 3 + 2] = (byte)(x * 3 + 77);
            input[x * 3 + 3] = (byte)(x * 71 + 119);
        }
        var original = input.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 4 + 10).ToArray();
        PngCodec.CopyRgb8Row(input.AsSpan(1, width * 3), output.AsSpan(5, width * 4), default);
        for (var x = 0; x < width; x++)
        {
            Assert.Equal((byte)x, output[5 + x * 4]);
            Assert.Equal((byte)(x * 3 + 77), output[6 + x * 4]);
            Assert.Equal((byte)(x * 71 + 119), output[7 + x * 4]);
            Assert.Equal((byte)255, output[8 + x * 4]);
        }
        Assert.Equal(original, input);
        Assert.All(output.Take(5), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 4 + 5), value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(1)] [InlineData(8)]
    public void ChecksCancellationBeforeWriting(int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 4).ToArray();
        Assert.Throws<OperationCanceledException>(() =>
            PngCodec.CopyRgb8Row(new byte[width * 3], output, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
