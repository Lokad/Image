using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngRgb16TransparentRowTests
{
    [Theory]
    [InlineData(1, 12347, 35806, 59265)] [InlineData(3, 12347, 35806, 59265)]
    [InlineData(4, 12347, 35806, 59265)] [InlineData(5, 12347, 35806, 59265)]
    [InlineData(7, 12347, 35806, 59265)] [InlineData(8, 12347, 35806, 59265)]
    [InlineData(9, 12347, 35806, 59265)] [InlineData(17, 12347, 35806, 59265)]
    [InlineData(513, 12347, 35806, 59265)] [InlineData(4095, 12347, 35806, 59265)]
    [InlineData(4096, 12347, 35806, 59265)] [InlineData(4097, 12347, 35806, 59265)]
    [InlineData(8, 0, 0, 0)] [InlineData(9, 65535, 65535, 65535)]
    public void ComparesFullWordsPreservingHiddenRgbTailsAndUnalignedGuards(int width,
        ushort redKey, ushort greenKey, ushort blueKey)
    {
        var keys = new[] { redKey, greenKey, blueKey }; var input = new byte[width * 6 + 2];
        for (var x = 0; x < width; x++)
        for (var c = 0; c < 3; c++)
            BinaryPrimitives.WriteUInt16BigEndian(input.AsSpan(1 + x * 6 + c * 2),
                (ushort)(keys[c] ^ (x % 4 == c + 1 ? 1 : 0)));
        var original = input.ToArray(); var output = Enumerable.Repeat((byte)0xa5, width * 8 + 14).ToArray();
        PngCodec.CopyRgb16TransparentRow(input.AsSpan(1, width * 6), output.AsSpan(7, width * 8),
            redKey, greenKey, blueKey, default);
        var samples = MemoryMarshal.Cast<byte, ushort>(output.AsSpan(7, width * 8));
        for (var x = 0; x < width; x++)
        {
            for (var c = 0; c < 3; c++)
                Assert.Equal(BinaryPrimitives.ReadUInt16BigEndian(original.AsSpan(1 + x * 6 + c * 2)), samples[x * 4 + c]);
            Assert.Equal(x % 4 == 0 ? (ushort)0 : ushort.MaxValue, samples[x * 4 + 3]);
        }
        Assert.Equal(original, input);
        Assert.All(output.Take(7), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 8 + 7), value => Assert.Equal((byte)0xa5, value));
    }

    [Fact]
    public void EveryFullWordInEachChannelOnlyMatchesItsExactKey()
    {
        var input = new byte[65536 * 6]; var output = new byte[65536 * 8];
        var key = new ushort[] { 12347, 35806, 59265 };
        for (var channel = 0; channel < 3; channel++)
        {
            for (var x = 0; x < 65536; x++)
            for (var c = 0; c < 3; c++)
                BinaryPrimitives.WriteUInt16BigEndian(input.AsSpan(x * 6 + c * 2), c == channel ? (ushort)x : key[c]);
            PngCodec.CopyRgb16TransparentRow(input, output, key[0], key[1], key[2], default);
            var samples = MemoryMarshal.Cast<byte, ushort>(output);
            for (var x = 0; x < 65536; x++)
            {
                for (var c = 0; c < 3; c++) Assert.Equal(c == channel ? (ushort)x : key[c], samples[x * 4 + c]);
                Assert.Equal(x == key[channel] ? (ushort)0 : ushort.MaxValue, samples[x * 4 + 3]);
            }
        }
    }

    [Theory]
    [InlineData(1)] [InlineData(4)]
    public void ChecksCancellationBeforeWriting(int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 8).ToArray();
        Assert.Throws<OperationCanceledException>(() => PngCodec.CopyRgb16TransparentRow(
            new byte[width * 6], output, 12347, 35806, 59265, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
