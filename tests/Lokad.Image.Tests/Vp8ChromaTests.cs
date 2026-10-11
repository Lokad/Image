using System.Reflection;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8ChromaTests
{
    private delegate double Expansion(byte[] plane, int stride, int width, int height, int x, int y);
    private static readonly Expansion Expand = typeof(Vp8Color).GetMethod("Chroma", BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<Expansion>();

    [Fact]
    public void EveryByteImpulseAndAllCenteredPhasesMatchOriginalGeometry()
    {
        var plane = new byte[4];
        for (var corner = 0; corner < 4; corner++)
            for (var value = 0; value < 256; value++)
            {
                Array.Clear(plane); plane[corner] = (byte)value;
                for (var y = 0; y < 4; y++)
                    for (var x = 0; x < 4; x++) Assert.Equal(Scalar(plane, 2, 2, 2, x, y), Expand(plane, 2, 2, 2, x, y));
            }
    }

    [Fact]
    public void HalfSamplesRoundUpAndEdgesExtendExactly()
    {
        Assert.Equal(1, Expand([0, 0, 0, 8], 2, 2, 2, 1, 1));
        Assert.Equal(0, Expand([0, 0, 0, 7], 2, 2, 2, 1, 1));
        Assert.Equal(16, Expand([255, 0, 0, 0], 2, 2, 2, 2, 2));
        Assert.Equal(159, Expand([0, 255, 255, 0], 2, 2, 2, 1, 2));
        Assert.Equal(32, Expand([0, 255, 128, 64], 2, 2, 2, 0, 1));
        Assert.Equal(112, Expand([0, 255, 128, 64], 2, 2, 2, 3, 2));
        Assert.Equal(112, Expand([0, 255, 128, 64], 2, 2, 2, 1, 3));
        Assert.Equal(64, Expand([0, 255, 128, 64], 2, 2, 2, 3, 3));
    }

    [Theory]
    [InlineData(1, 1)] [InlineData(1, 7)] [InlineData(9, 1)]
    [InlineData(2, 3)] [InlineData(7, 4)] [InlineData(16, 29)]
    public void OddEvenThinAndPaddedPlanesMatchOriginalGeometry(int width, int height)
    {
        var stride = width + 3; var plane = Enumerable.Repeat((byte)0xd7, stride * height).ToArray();
        for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++) plane[y * stride + x] = (byte)((x * 37 + y * 53 + x * y * 17 + (x ^ y) * 11 + 19) & 255);
        for (var y = 0; y < 2 * height; y++)
            for (var x = 0; x < 2 * width; x++) Assert.Equal(Scalar(plane, stride, width, height, x, y), Expand(plane, stride, width, height, x, y));
        Assert.All(Enumerable.Range(0, height).SelectMany(y => plane.AsSpan(y * stride + width, 3).ToArray()), value => Assert.Equal(0xd7, value));
        for (var value = 0; value < 256; value++)
        {
            for (var y = 0; y < height; y++) plane.AsSpan(y * stride, width).Fill((byte)value);
            foreach (var y in new[] { 0, height - 1, 2 * height - 1 })
                foreach (var x in new[] { 0, width - 1, 2 * width - 1 }) Assert.Equal(value, Expand(plane, stride, width, height, x, y));
        }
    }

    // Retained original centered geometry and double interpolation recurrence.
    private static double Scalar(byte[] plane, int stride, int width, int height, int x, int y)
    {
        var sx = Math.Clamp((x + 0.5) / 2 - 0.5, 0, width - 1); var sy = Math.Clamp((y + 0.5) / 2 - 0.5, 0, height - 1);
        var left = (int)sx; var top = (int)sy; var right = Math.Min(left + 1, width - 1); var bottom = Math.Min(top + 1, height - 1);
        var dx = sx - left; var dy = sy - top;
        var sample = (plane[top * stride + left] * (1 - dx) + plane[top * stride + right] * dx) * (1 - dy)
            + (plane[bottom * stride + left] * (1 - dx) + plane[bottom * stride + right] * dx) * dy;
        return Math.Floor(sample + 0.5);
    }
}
