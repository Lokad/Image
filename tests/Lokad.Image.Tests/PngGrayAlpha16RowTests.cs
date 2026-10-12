using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngGrayAlpha16RowTests
{
    [Theory]
    [InlineData(1)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
    [InlineData(8)] [InlineData(9)] [InlineData(256)] [InlineData(513)]
    [InlineData(4095)] [InlineData(4096)] [InlineData(4097)] [InlineData(65536)]
    public void RetainsFullGrayAlphaSamplesAndHiddenRgbWithUnalignedRowGuards(int width)
    {
        var input = new byte[width * 4 + 2];
        for (var x = 0; x < width; x++)
        {
            BinaryPrimitives.WriteUInt16BigEndian(input.AsSpan(x * 4 + 1, 2), (ushort)(x + 1));
            BinaryPrimitives.WriteUInt16BigEndian(input.AsSpan(x * 4 + 3, 2), (ushort)(x * 73));
        }
        var original = input.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 8 + 14).ToArray();
        PngCodec.CopyGrayAlpha16Row(input.AsSpan(1, width * 4), output.AsSpan(7, width * 8), default);
        var samples = MemoryMarshal.Cast<byte, ushort>(output.AsSpan(7, width * 8));
        for (var x = 0; x < width; x++)
        {
            for (var channel = 0; channel < 3; channel++) Assert.Equal((ushort)(x + 1), samples[x * 4 + channel]);
            Assert.Equal((ushort)(x * 73), samples[x * 4 + 3]);
        }
        Assert.Equal(original, input);
        Assert.All(output.Take(7), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 8 + 7), value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(1)] [InlineData(4)]
    public void ChecksCancellationBeforeWriting(int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 8).ToArray();
        Assert.Throws<OperationCanceledException>(() =>
            PngCodec.CopyGrayAlpha16Row(new byte[width * 4], output, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
