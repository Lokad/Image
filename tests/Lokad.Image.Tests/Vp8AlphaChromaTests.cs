using System.Reflection;
using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8AlphaChromaTests
{
    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void HiddenBlueDoesNotDiluteVisibleRedChroma(int depth)
    {
        using var source = Source(2, 2, depth, [255, 0, 0, 255, 0, 0, 255, 0, 0, 0, 255, 0, 0, 0, 255, 0]);
        var input = Bytes(source); var (planes, alpha) = Convert(source);
        Assert.Equal(90, planes.U[0]); Assert.Equal(240, planes.V[0]);
        Assert.Equal(81, planes.Y[0]); Assert.Equal(41, planes.Y[1]);
        Assert.Equal(new byte[] { 255, 0, 0, 0 }, alpha);
        Assert.Equal(input, Bytes(source));
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void UnequalOpacityUsesExactEncodedSpaceWeights(int depth)
    {
        using var source = Source(2, 2, depth, [255, 0, 0, 1, 0, 0, 255, 3, 0, 255, 0, 0, 0, 255, 0, 0]);
        var (planes, alpha) = Convert(source);
        // Weighted RGB is (63.75, 0, 191.25), with literal BT.601 results.
        Assert.Equal(203, planes.U[0]); Assert.Equal(142, planes.V[0]);
        Assert.Equal(new byte[] { 1, 3, 0, 0 }, alpha);
    }

    [Theory]
    [InlineData(8, 0)] [InlineData(16, 0)]
    [InlineData(8, 128)] [InlineData(16, 128)]
    [InlineData(8, 255)] [InlineData(16, 255)]
    public void UniformOpacityRetainsOriginalGrayAverage(int depth, byte opacity)
    {
        using var source = Source(2, 2, depth, [255, 0, 0, opacity, 0, 255, 0, opacity, 0, 0, 255, opacity, 255, 255, 255, opacity]);
        var (planes, alpha) = Convert(source);
        Assert.Equal(128, planes.U[0]); Assert.Equal(128, planes.V[0]);
        Assert.Equal(81, planes.Y[0]); Assert.Equal(145, planes.Y[1]);
        Assert.Equal(41, planes.Y[planes.Stride]); Assert.Equal(235, planes.Y[planes.Stride + 1]);
        if (opacity == 255) Assert.Null(alpha);
        else Assert.Equal(Enumerable.Repeat(opacity, 4).ToArray(), alpha);
    }

    [Theory]
    [InlineData(8)] [InlineData(16)]
    public void OddAndPaddedEdgesRepeatTheLastSourcePixel(int depth)
    {
        using var source = Source(1, 3, depth, [255, 0, 0, 255, 0, 0, 255, 0, 0, 255, 0, 255]);
        var (planes, alpha) = Convert(source);
        Assert.Equal(90, planes.U[0]); Assert.Equal(240, planes.V[0]);
        Assert.Equal(54, planes.U[planes.Stride / 2]); Assert.Equal(34, planes.V[planes.Stride / 2]);
        Assert.Equal(54, planes.U[^1]); Assert.Equal(34, planes.V[^1]);
        Assert.Equal(new byte[] { 255, 0, 255 }, alpha);
    }

    private static (Vp8Planes Planes, byte[]? Alpha) Convert(ImageBuffer source)
    {
        var method = typeof(Vp8Encoder).GetMethod("Convert", BindingFlags.NonPublic | BindingFlags.Static)!;
        object?[] arguments = [source, new OperationContext(ImageLimits.Default, default), null];
        return ((Vp8Planes)method.Invoke(null, arguments)!, (byte[]?)arguments[2]);
    }

    private static ImageBuffer Source(int width, int height, int depth, byte[] rgba)
    {
        var source = ImageBuffer.Create(width, height, depth);
        for (var y = 0; y < height; y++)
        for (var i = 0; i < width * 4; i++)
            if (depth == 8) source.GetRow8(y)[i] = rgba[y * width * 4 + i];
            else source.GetRow16(y)[i] = (ushort)(rgba[y * width * 4 + i] * 257);
        return source;
    }

    private static byte[] Bytes(ImageBuffer source)
        => Enumerable.Range(0, source.Height).SelectMany(y => source.BitDepth == 8
            ? source.GetRow8(y).ToArray() : MemoryMarshal.AsBytes(source.GetRow16(y)).ToArray()).ToArray();
}
