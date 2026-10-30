using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegDequantizationTests
{
    private static readonly ushort[] Boundaries = [0, 1, 2, 3, 127, 128, 255, 256, 257, 1023, 4095, 16383, 32767, 32768, 65534, 65535];
    private static readonly int[] Extremes = [int.MinValue, int.MinValue + 1, -1073741824, -65537, -65536, -1, 0, 1, 65535, 65536, 65537, 1073741824, int.MaxValue - 1, int.MaxValue];

    [Fact]
    public void EveryAcLowWordAndQuantizerBoundaryKeepsWideScalarBitsAndLaneOrder()
    {
        var source = new int[66]; var scales = new ushort[66];
        var output = Enumerable.Repeat((short)0x5a5a, 70).ToArray();
        source[0] = 1234567; source[^1] = -7654321;
        foreach (var q in Boundaries)
        {
            Array.Fill(scales, q);
            for (var start = 0; start < 65536; start += 63)
            {
                source[1] = int.MaxValue;
                for (var i = 1; i < 64; i++)
                    source[i + 1] = unchecked((int)((0x13570000u ^ ((i & 1) == 0 ? 0u : 0x80000000u)) | (uint)((start + i - 1) & 65535)));
                JpegInverseTransform.Dequantize(source.AsSpan(1, 64), scales.AsSpan(1, 64), output.AsSpan(3, 64));
                for (var i = 0; i < 64; i++)
                    Assert.Equal(Expected(source[i + 1], q, i == 0), output[i + 3]);
            }
            Assert.All(scales, value => Assert.Equal(q, value));
            Assert.Equal(1234567, source[0]); Assert.Equal(-7654321, source[^1]);
        }
        Assert.All(output.Take(3), value => Assert.Equal((short)0x5a5a, value));
        Assert.All(output.Skip(67), value => Assert.Equal((short)0x5a5a, value));
    }

    [Fact]
    public void EveryQuantizerForWideSignedCoefficientBoundariesPreservesBorrowedInputs()
    {
        var source = new int[66]; var scales = new ushort[66];
        var output = Enumerable.Repeat((short)0x5a5a, 70).ToArray();
        foreach (var coefficient in Extremes)
        {
            Array.Fill(source, coefficient);
            for (var start = 0; start < 65536; start += 63)
            {
                for (var i = 0; i < 64; i++) scales[i + 1] = (ushort)((start + Math.Max(0, i - 1)) & 65535);
                JpegInverseTransform.Dequantize(source.AsSpan(1, 64), scales.AsSpan(1, 64), output.AsSpan(3, 64));
                for (var i = 0; i < 64; i++)
                {
                    Assert.Equal(Expected(coefficient, scales[i + 1], i == 0), output[i + 3]);
                    Assert.Equal(coefficient, source[i + 1]);
                    Assert.Equal((ushort)((start + Math.Max(0, i - 1)) & 65535), scales[i + 1]);
                }
            }
        }
        Assert.All(output.Take(3), value => Assert.Equal((short)0x5a5a, value));
        Assert.All(output.Skip(67), value => Assert.Equal((short)0x5a5a, value));
    }

    [Fact]
    public void DcLevelShiftUsesTheFullProductBeforeSaturation()
    {
        var source = new int[64]; var scales = new ushort[64]; var output = new short[64];
        foreach (var coefficient in Extremes)
        foreach (var q in Boundaries)
        {
            Array.Fill(source, coefficient); Array.Fill(scales, q);
            JpegInverseTransform.Dequantize(source, scales, output);
            Assert.Equal((short)Math.Clamp((long)coefficient * q + 1024, short.MinValue, short.MaxValue), output[0]);
        }
    }

    private static short Expected(int coefficient, ushort quantizer, bool dc)
        => dc ? (short)Math.Clamp((long)coefficient * quantizer + 1024, short.MinValue, short.MaxValue)
            : unchecked((short)((long)coefficient * quantizer));
}
