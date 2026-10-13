using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegComponentRow16Tests
{
    [Fact]
    public void EveryAlphaKeepsExactFullWordBlendingAndIndependentChannels()
    {
        var rgba = new ushort[34]; var output = new byte[10];
        foreach (var background in new uint[] { 0, 0xFFFFFF, 0x0000FF, 0x00FF00, 0xFF0000, 0x204060, 0x0101FE, 0xFF7F80 })
        for (var phase = 0; phase < 8192; phase++)
        {
            Array.Fill(rgba, (ushort)12345);
            for (var lane = 0; lane < 8; lane++)
            {
                rgba[1 + lane * 4] = unchecked((ushort)(phase * 7919 + lane * 37));
                rgba[2 + lane * 4] = unchecked((ushort)(65535 - phase * 127 + lane * 16383));
                rgba[3 + lane * 4] = unchecked((ushort)(phase * 257 + lane * 8191));
                rgba[4 + lane * 4] = (ushort)(phase * 8 + lane);
            }
            var original = rgba.ToArray();
            for (var component = 0; component < 3; component++)
            {
                Array.Fill(output, (byte)123);
                JpegEncoder.SampleComponentRow16(rgba.AsSpan(1, 32), component, background, output.AsSpan(1, 8));
                for (var lane = 0; lane < 8; lane++)
                {
                    var pixel = original.AsSpan(1 + lane * 4, 4);
                    Assert.Equal(Reference(pixel, background, component), output[lane + 1]);
                }
                Assert.Equal(123, output[0]); Assert.Equal(123, output[^1]);
            }
            Assert.Equal(original, rgba);
        }
    }

    [Fact]
    public void OpaqueExtremesPermitAnAbsentBackground()
    {
        ushort[] values = [0, 1, 127, 128, 255, 256, 257, 32767, 32768, 65407, 65534, 65535];
        var rgba = new ushort[32]; var output = new byte[8];
        foreach (var phase in Enumerable.Range(0, values.Length))
        {
            for (var lane = 0; lane < 8; lane++)
            {
                rgba[lane * 4] = values[(phase + lane) % values.Length];
                rgba[lane * 4 + 1] = values[(phase + lane * 3) % values.Length];
                rgba[lane * 4 + 2] = values[(phase + lane * 7) % values.Length];
                rgba[lane * 4 + 3] = 65535;
            }
            for (var component = 0; component < 3; component++)
            {
                JpegEncoder.SampleComponentRow16(rgba, component, null, output);
                for (var lane = 0; lane < 8; lane++)
                    Assert.Equal(Reference(rgba.AsSpan(lane * 4, 4), 0, component), output[lane]);
            }
        }
    }

    [Fact]
    public void TransparencyWithoutBackgroundKeepsTheOriginalErrorForEveryLane()
    {
        for (var lane = 0; lane < 8; lane++)
        {
            var rgba = Enumerable.Repeat((ushort)65535, 32).ToArray(); rgba[lane * 4 + 3] = 65534;
            var error = Assert.Throws<ArgumentException>(() => JpegEncoder.SampleComponentRow16(rgba, 0, null, new byte[8]));
            Assert.Equal("image", error.ParamName);
            Assert.StartsWith("JPEG requires opaque pixels or an explicit RGB background.", error.Message);
        }
    }

    [Theory]
    [InlineData(31, 8, 0)] [InlineData(32, 7, 0)] [InlineData(32, 8, 3)]
    public void InvalidGeometryLeavesDestinationUntouched(int words, int samples, int component)
    {
        var output = Enumerable.Repeat((byte)123, samples).ToArray();
        Assert.Throws<ArgumentException>(() => JpegEncoder.SampleComponentRow16(new ushort[words], component, 0, output));
        Assert.All(output, value => Assert.Equal((byte)123, value));
    }

    private static byte Reference(ReadOnlySpan<ushort> rgba, uint background, int component)
    {
        const long maximum = 65535, denominator = maximum * maximum;
        var alpha = rgba[3];
        int Blend(int channel, int value)
        {
            var bg = (int)(background >> (16 - channel * 8) & 255);
            var numerator = (long)value * alpha * 255 + bg * (maximum - alpha) * maximum;
            return (int)((numerator + denominator / 2) / denominator);
        }
        var r = Blend(0, rgba[0]); var g = Blend(1, rgba[1]); var b = Blend(2, rgba[2]);
        var sample = component switch
        {
            0 => 0.299 * r + 0.587 * g + 0.114 * b,
            1 => 128 - 0.168735892 * r - 0.331264108 * g + 0.5 * b,
            _ => 128 + 0.5 * r - 0.418687589 * g - 0.081312411 * b
        };
        return (byte)Math.Clamp(Math.Round(sample, MidpointRounding.AwayFromZero), 0, 255);
    }
}
