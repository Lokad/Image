using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngGray16TransparentRowTests
{
    [Theory]
    [InlineData(0, 0)] [InlineData(1, 65535)] [InlineData(3, 32639)] [InlineData(4, 256)]
    [InlineData(5, 257)] [InlineData(8, 255)] [InlineData(9, 1)] [InlineData(513, 32639)]
    [InlineData(4095, 256)] [InlineData(4096, 65535)] [InlineData(4097, 0)]
    public void PreservesFullKeyHiddenRgbUnalignedGuardsAndTails(int width, ushort key)
    {
        var input = new byte[width * 2 + 2];
        for (var x = 0; x < width; x++)
            BinaryPrimitives.WriteUInt16BigEndian(input.AsSpan(x * 2 + 1, 2), unchecked((ushort)(key + x % 7 - 3)));
        var original = input.ToArray(); var output = Enumerable.Repeat((byte)0xa5, width * 8 + 14).ToArray();
        PngCodec.CopyGray16TransparentRow(input.AsSpan(1, width * 2), output.AsSpan(7, width * 8), key, default);
        var samples = MemoryMarshal.Cast<byte, ushort>(output.AsSpan(7, width * 8));
        for (var x = 0; x < width; x++)
        {
            var value = BinaryPrimitives.ReadUInt16BigEndian(original.AsSpan(x * 2 + 1, 2));
            for (var channel = 0; channel < 3; channel++) Assert.Equal(value, samples[x * 4 + channel]);
            Assert.Equal(value == key ? (ushort)0 : ushort.MaxValue, samples[x * 4 + 3]);
        }
        Assert.Equal(original, input);
        Assert.All(output.Take(7), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 8 + 7), value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(255)] [InlineData(256)]
    [InlineData(257)] [InlineData(32639)] [InlineData(65280)] [InlineData(65535)]
    public void EverySampleRetainsBothBytesAndOnlyTheCompleteKeyIsTransparent(ushort key)
    {
        var input = new byte[65536 * 2]; var output = new byte[65536 * 8];
        for (var value = 0; value < 65536; value++) BinaryPrimitives.WriteUInt16BigEndian(input.AsSpan(value * 2, 2), (ushort)value);
        PngCodec.CopyGray16TransparentRow(input, output, key, default);
        var samples = MemoryMarshal.Cast<byte, ushort>(output);
        for (var value = 0; value < 65536; value++)
        {
            for (var channel = 0; channel < 3; channel++) Assert.Equal((ushort)value, samples[value * 4 + channel]);
            Assert.Equal(value == key ? (ushort)0 : ushort.MaxValue, samples[value * 4 + 3]);
        }
    }

    [Theory]
    [InlineData(1)] [InlineData(4)] [InlineData(4097)]
    public void ChecksCancellationBeforeWriting(int width)
    {
        var output = Enumerable.Repeat((byte)0xa5, width * 8).ToArray();
        Assert.Throws<OperationCanceledException>(() => PngCodec.CopyGray16TransparentRow(
            new byte[width * 2], output, 0, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
