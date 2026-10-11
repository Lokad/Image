using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegComponentRowTests
{
    [Fact]
    public void MixedLaneAlphaKeepsIndependentChannelAndBackgroundProducts()
    {
        var rgba = new byte[34]; var pixels = new byte[10];
        foreach (var background in new uint[] { 0, 0xFFFFFF, 0x0000FF, 0x00FF00, 0xFF0000, 0x204060, 0x0101FE, 0xFF7F80 })
        for (var phase = 0; phase < 256; phase++)
        {
            Array.Fill(rgba, (byte)123);
            for (var lane = 0; lane < 8; lane++)
            {
                rgba[1 + lane * 4] = (byte)(phase + lane * 73);
                rgba[2 + lane * 4] = (byte)(255 - phase + lane * 127);
                rgba[3 + lane * 4] = (byte)(phase * 17 + lane * 31);
                rgba[4 + lane * 4] = (byte)(phase + lane * 37);
            }
            var original = rgba.ToArray();
            for (var component = 0; component < 3; component++)
            {
                Array.Fill(pixels, (byte)123);
                JpegEncoder.SampleComponentRow8(rgba.AsSpan(1, 32), component, background, pixels.AsSpan(1, 8));
                for (var lane = 0; lane < 8; lane++)
                {
                    var alpha = rgba[4 + lane * 4];
                    var r = (rgba[1 + lane * 4] * alpha + (int)(background >> 16 & 255) * (255 - alpha) + 127) / 255;
                    var g = (rgba[2 + lane * 4] * alpha + (int)(background >> 8 & 255) * (255 - alpha) + 127) / 255;
                    var b = (rgba[3 + lane * 4] * alpha + (int)(background & 255) * (255 - alpha) + 127) / 255;
                    Assert.Equal(OriginalSample(r, g, b, component), pixels[lane + 1]);
                }
                Assert.Equal(123, pixels[0]); Assert.Equal(123, pixels[^1]);
            }
            Assert.Equal(original, rgba);
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void EveryOpaqueRgbColorKeepsTheOriginalRoundedComponent(int component)
    {
        var rgba = new byte[32]; var pixels = new byte[8];
        for (var r = 0; r < 256; r++)
        for (var g = 0; g < 256; g++)
        for (var b = 0; b < 256; b += 8)
        {
            for (var lane = 0; lane < 8; lane++)
            {
                rgba[lane * 4] = (byte)r; rgba[lane * 4 + 1] = (byte)g;
                rgba[lane * 4 + 2] = (byte)(b + lane); rgba[lane * 4 + 3] = 255;
            }
            JpegEncoder.SampleComponentRow8(rgba, component, null, pixels);
            for (var lane = 0; lane < 8; lane++)
            {
                var expected = OriginalSample(r, g, b + lane, component);
                if (pixels[lane] != expected) Assert.Equal(expected, pixels[lane]);
            }
        }
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public void EveryValueAndAlphaRetainsExactFlatteningAndLaneOrder(int component)
    {
        var rgba = new byte[34]; var original = new byte[34]; var pixels = new byte[10];
        foreach (var channel in new[] { 0, 1, 32, 64, 96, 128, 254, 255 })
        for (var alpha = 0; alpha < 256; alpha++)
        for (var value = 0; value < 256; value += 8)
        {
            var background = (uint)(channel << 16 | (255 - channel) << 8 | (channel * 73 & 255));
            Array.Fill(rgba, (byte)123); Array.Fill(pixels, (byte)123);
            for (var lane = 0; lane < 8; lane++)
            {
                rgba[1 + lane * 4] = (byte)(value + lane);
                rgba[2 + lane * 4] = (byte)(255 - value - lane);
                rgba[3 + lane * 4] = (byte)((value + lane) * 73 & 255);
                rgba[4 + lane * 4] = (byte)alpha;
            }
            rgba.CopyTo(original, 0);
            JpegEncoder.SampleComponentRow8(rgba.AsSpan(1, 32), component, background, pixels.AsSpan(1, 8));
            for (var lane = 0; lane < 8; lane++)
            {
                var r = (rgba[1 + lane * 4] * alpha + channel * (255 - alpha) + 127) / 255;
                var g = (rgba[2 + lane * 4] * alpha + (255 - channel) * (255 - alpha) + 127) / 255;
                var b = (rgba[3 + lane * 4] * alpha + (channel * 73 & 255) * (255 - alpha) + 127) / 255;
                Assert.Equal(OriginalSample(r, g, b, component), pixels[lane + 1]);
            }
            Assert.Equal(123, pixels[0]); Assert.Equal(123, pixels[^1]); Assert.Equal(original, rgba);
        }
    }

    [Theory]
    [InlineData(31, 8, 0)] [InlineData(33, 8, 0)] [InlineData(32, 7, 0)]
    [InlineData(32, 9, 0)] [InlineData(32, 8, -1)] [InlineData(32, 8, 3)]
    public void IncompleteRowsAndInvalidComponentsRejectBeforeWriting(int input, int output, int component)
    {
        var pixels = Enumerable.Repeat((byte)123, output).ToArray();
        Assert.Throws<ArgumentException>(() => JpegEncoder.SampleComponentRow8(new byte[input], component, 0, pixels));
        Assert.All(pixels, value => Assert.Equal(123, value));
    }

    [Fact]
    public void TransparencyWithoutBackgroundKeepsTheOriginalErrorForEveryLane()
    {
        for (var lane = 0; lane < 8; lane++)
        {
            var rgba = Enumerable.Repeat((byte)255, 32).ToArray(); rgba[lane * 4 + 3] = 254;
            var error = Assert.Throws<ArgumentException>(() => JpegEncoder.SampleComponentRow8(rgba, 0, null, new byte[8]));
            Assert.Equal("image", error.ParamName);
            Assert.StartsWith("JPEG requires opaque pixels or an explicit RGB background.", error.Message);
        }
    }

    private static byte OriginalSample(int r, int g, int b, int component)
    {
        var sample = component switch
        {
            0 => 0.299 * r + 0.587 * g + 0.114 * b,
            1 => 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b,
            _ => 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b
        };
        return (byte)Math.Clamp(Math.Round(sample, MidpointRounding.AwayFromZero), 0, 255);
    }
}
