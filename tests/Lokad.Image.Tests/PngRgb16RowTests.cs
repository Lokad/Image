using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PngRgb16RowTests
{
    // Four hand-authored RGB16 pixels in PNG byte order, with distinct low bytes.
    private static readonly byte[] PixelBytes =
    [
        0x00, 0x01, 0x12, 0x34, 0xab, 0xcd,
        0xff, 0xff, 0x00, 0x00, 0x80, 0x01,
        0xfe, 0x01, 0x00, 0xff, 0x01, 0x00,
        0x43, 0x21, 0x89, 0xab, 0x00, 0x02
    ];
    private static readonly ushort[] Expected =
    [1, 0x1234, 0xabcd, 65535, 65535, 0, 0x8001, 65535,
     0xfe01, 0x00ff, 0x0100, 65535, 0x4321, 0x89ab, 2, 65535];

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(4095)]
    [InlineData(4096)]
    [InlineData(4097)]
    public void PreservesFullSamplesAndRowBoundaries(int width)
    {
        var input = new byte[width * 6 + 2];
        for (var i = 0; i < width * 6; i++) input[i + 1] = PixelBytes[i % PixelBytes.Length];
        var unchanged = input.ToArray();
        var output = Enumerable.Repeat((byte)0xa5, width * 8 + 14).ToArray();
        PngCodec.CopyRgb16Row(input.AsSpan(1, width * 6), output.AsSpan(7, width * 8), default);
        var samples = MemoryMarshal.Cast<byte, ushort>(output.AsSpan(7, width * 8));
        for (var i = 0; i < samples.Length; i++) Assert.Equal(Expected[i % Expected.Length], samples[i]);
        Assert.Equal(unchanged, input);
        Assert.All(output.Take(7), value => Assert.Equal((byte)0xa5, value));
        Assert.All(output.Skip(width * 8 + 7), value => Assert.Equal((byte)0xa5, value));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    public void ChecksCancellationBeforeWriting(int width)
    {
        var input = new byte[width * 6];
        var output = Enumerable.Repeat((byte)0xa5, width * 8).ToArray();
        Assert.Throws<OperationCanceledException>(() =>
            PngCodec.CopyRgb16Row(input, output, new CancellationToken(true)));
        Assert.All(output, value => Assert.Equal((byte)0xa5, value));
    }
}
