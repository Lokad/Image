using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegNormalizationTests
{
    [Fact]
    public void ByteRowsPreserveEveryChannelAlphaAndBackgroundCombination()
    {
        var source = new byte[256 * 4]; var actual = new byte[256 * 3];
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
                    if (actual[x * 3 + c] != expected)
                        Assert.Fail($"Channel {c}, source {source[x * 4 + c]}, alpha {alpha}, background {backdrop}.");
                }
            }
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(17)]
    [InlineData(20)]
    public void UnalignedRowsAndScalarTailsPreserveBoundsAndRequireExplicitFlattening(int count)
    {
        var source = Enumerable.Repeat((byte)167, count * 4 + 4).ToArray();
        var output = Enumerable.Repeat((byte)213, count * 3 + 6).ToArray();
        for (var x = 0; x < count; x++)
        {
            source[2 + x * 4] = (byte)(x * 37); source[3 + x * 4] = (byte)(x * 59 + 7);
            source[4 + x * 4] = (byte)(255 - x * 11); source[5 + x * 4] = 255;
        }
        var original = source.ToArray();
        JpegReferenceColor.NormalizeRgba8(source.AsSpan(2, count * 4), output.AsSpan(3, count * 3), null);
        for (var x = 0; x < count; x++) Assert.True(source.AsSpan(2 + x * 4, 3).SequenceEqual(output.AsSpan(3 + x * 3, 3)));
        Assert.Equal(original, source); Assert.All(output[..3], value => Assert.Equal(213, value));
        Assert.All(output[^3..], value => Assert.Equal(213, value));
        source[5 + (count - 1) * 4] = 254;
        Assert.Throws<ArgumentException>(() => JpegReferenceColor.NormalizeRgba8(source.AsSpan(2, count * 4), output.AsSpan(3, count * 3), null));
    }
}
