using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegNormalizationTests
{
    [Fact]
    public void WordRowsPreserveSingleRoundingAcrossFullSamplesAndAlphaBoundaries()
    {
        var source = new ushort[256 * 4]; var actual = new int[256];
        const long maximum = 65535, denominator = maximum * maximum;
        foreach (var alpha in new[] { 0, 1, 255, 256, 257, 32767, 32768, 65000, 65534, 65535 })
        foreach (var background in new[] { 0, 1, 32, 64, 96, 128, 254, 255 })
        {
            var red = background; var green = (background * 73 + 29) & 255; var blue = 255 - background;
            var color = (uint)(red << 16 | green << 8 | blue);
            for (var start = 0; start <= 65535; start += 256)
            {
                for (var x = 0; x < 256; x++)
                {
                    var value = start + x;
                    source[x * 4] = (ushort)value; source[x * 4 + 1] = (ushort)((value * 40503L + 17) & 65535);
                    source[x * 4 + 2] = (ushort)(65535 - value); source[x * 4 + 3] = (ushort)alpha;
                }
                JpegReferenceColor.NormalizeRgba16(source, actual, color);
                for (var x = 0; x < 256; x++)
                for (var c = 0; c < 3; c++)
                {
                    var backdrop = c == 0 ? red : c == 1 ? green : blue;
                    var expected = (source[x * 4 + c] * (long)alpha * 255
                        + backdrop * (maximum - alpha) * maximum + denominator / 2) / denominator;
                    if ((actual[x] >> (c * 8) & 255) != expected || (actual[x] & ~0xFFFFFF) != 0)
                        Assert.Fail($"Channel {c}, source {source[x * 4 + c]}, alpha {alpha}, background {backdrop}.");
                }
            }
        }
    }

    [Fact]
    public void WordRowsPreserveUnalignedBoundsInputAndExplicitBackgroundRequirements()
    {
        foreach (var count in new[] { 1, 2, 3, 17, 18 })
        {
            var source = Enumerable.Repeat((ushort)42317, count * 4 + 6).ToArray();
            var output = Enumerable.Repeat(213, count + 6).ToArray();
            for (var x = 0; x < count; x++)
            {
                source[3 + x * 4] = (ushort)(x * 3721); source[4 + x * 4] = (ushort)(x * 5917 + 7);
                source[5 + x * 4] = (ushort)(65535 - x * 1103); source[6 + x * 4] = 65535;
            }
            var original = source.ToArray();
            JpegReferenceColor.NormalizeRgba16(source.AsSpan(3, count * 4), output.AsSpan(3, count), null);
            for (var x = 0; x < count; x++)
            for (var c = 0; c < 3; c++)
                Assert.Equal((source[3 + x * 4 + c] + 128) / 257, output[3 + x] >> (c * 8) & 255);
            Assert.Equal(original, source); Assert.All(output[..3], value => Assert.Equal(213, value));
            Assert.All(output[^3..], value => Assert.Equal(213, value));
            for (var x = 0; x < count; x++) source[6 + x * 4] = (ushort)((x * 9973 + 255) & 65535);
            var mixed = source.ToArray();
            JpegReferenceColor.NormalizeRgba16(source.AsSpan(3, count * 4), output.AsSpan(3, count), 0x204060);
            const long denominator = 65535L * 65535;
            for (var x = 0; x < count; x++)
            for (var c = 0; c < 3; c++)
            {
                var alpha = (long)source[6 + x * 4]; var backdrop = c == 0 ? 32 : c == 1 ? 64 : 96;
                var expected = (source[3 + x * 4 + c] * alpha * 255 + backdrop * (65535 - alpha) * 65535 + denominator / 2) / denominator;
                Assert.Equal(expected, output[3 + x] >> (c * 8) & 255);
            }
            Assert.Equal(mixed, source);
            for (var x = 0; x < count; x++) source[6 + x * 4] = 65535;
            source[6 + (count - 1) * 4] = 65534;
            Assert.Throws<ArgumentException>(() => JpegReferenceColor.NormalizeRgba16(source.AsSpan(3, count * 4), output.AsSpan(3, count), null));
        }
    }

    [Fact]
    public void ByteRowsPreserveEveryChannelAlphaAndBackgroundCombination()
    {
        var source = new byte[256 * 4]; var actual = new int[256];
        for (var background = 0; background < 256; background++)
        {
            var red = background; var green = (background * 73 + 29) & 255; var blue = 255 - background;
            var color = (uint)(red << 16 | green << 8 | blue);
            for (var alpha = 0; alpha < 256; alpha++)
            {
                for (var x = 0; x < 256; x++)
                {
                    source[x * 4] = (byte)x; source[x * 4 + 1] = (byte)(x * 37 + 17);
                    source[x * 4 + 2] = (byte)(255 - x); source[x * 4 + 3] = (byte)alpha;
                }
                JpegReferenceColor.NormalizeRgba8(source, actual, color);
                for (var x = 0; x < 256; x++)
                for (var c = 0; c < 3; c++)
                {
                    var backdrop = c == 0 ? red : c == 1 ? green : blue;
                    var expected = (source[x * 4 + c] * alpha + backdrop * (255 - alpha) + 127) / 255;
                    if ((actual[x] >> (c * 8) & 255) != expected || (actual[x] & ~0xFFFFFF) != 0)
                        Assert.Fail($"Channel {c}, source {source[x * 4 + c]}, alpha {alpha}, background {backdrop}.");
                }
            }
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(17)]
    [InlineData(20)]
    public void UnalignedRowsAndScalarTailsPreserveBoundsAndRequireExplicitFlattening(int count)
    {
        var source = Enumerable.Repeat((byte)167, count * 4 + 4).ToArray();
        var output = Enumerable.Repeat(213, count + 6).ToArray();
        for (var x = 0; x < count; x++)
        {
            source[2 + x * 4] = (byte)(x * 37); source[3 + x * 4] = (byte)(x * 59 + 7);
            source[4 + x * 4] = (byte)(255 - x * 11); source[5 + x * 4] = 255;
        }
        var original = source.ToArray();
        JpegReferenceColor.NormalizeRgba8(source.AsSpan(2, count * 4), output.AsSpan(3, count), null);
        for (var x = 0; x < count; x++)
            Assert.Equal(source[2 + x * 4] | source[3 + x * 4] << 8 | source[4 + x * 4] << 16, output[3 + x]);
        Assert.Equal(original, source); Assert.All(output[..3], value => Assert.Equal(213, value));
        Assert.All(output[^3..], value => Assert.Equal(213, value));
        source[5 + (count - 1) * 4] = 254;
        Assert.Throws<ArgumentException>(() => JpegReferenceColor.NormalizeRgba8(source.AsSpan(2, count * 4), output.AsSpan(3, count), null));
    }
}
