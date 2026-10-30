using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegPackedInverseTests
{
    [Fact]
    public void EveryBasisAtSignedWordBoundariesKeepsIndependentMatrixPixelsAndGuards()
    {
        int[] values = [-32768, -32767, -4096, -1024, -1, 1, 1023, 4095, 32766, 32767];
        var coefficients = new int[66];
        coefficients[0] = 1234567; coefficients[^1] = -7654321;
        var quantizers = Enumerable.Repeat((ushort)1, 66).ToArray();
        var output = Enumerable.Repeat((byte)165, 70).ToArray();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root, MaxWorkingBytes = 1 }, default);
        for (var lane = 0; lane < 64; lane++)
        foreach (var value in values)
        {
            Array.Clear(coefficients, 1, 64);
            coefficients[1] = -1024;
            coefficients[lane + 1] = lane == 0 ? value - 1024 : value;
            JpegInverseTransform.Inverse(coefficients.AsSpan(1, 64), quantizers.AsSpan(1, 64), output.AsSpan(3, 64), context);
            hash.AppendData(output.AsSpan(3, 64));
            Assert.Equal(lane == 0 ? value - 1024 : value, coefficients[lane + 1]);
        }
        // Original scalar Q14 matrix evaluated independently over all640blocks.
        Assert.Equal("9ffe3ba0463ac03994667fd9df5bd220329e479919a71bfb8f1e5619e35dfc4d", Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.All(output.Take(3).Concat(output.Skip(67)), value => Assert.Equal(165, value));
        Assert.Equal(1234567, coefficients[0]); Assert.Equal(-7654321, coefficients[^1]);
        Assert.All(quantizers, value => Assert.Equal((ushort)1, value));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void FullWidthProductsAndDenseRowsKeepIndependentFrozenMatrixPixels()
    {
        var coefficients = new int[64]; var quantizers = new ushort[64]; var output = new byte[64];
        uint state = 0x3c1b52a9;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root, MaxWorkingBytes = 1 }, default);
        for (var block = 0; block < 2048; block++)
        {
            for (var k = 0; k < 64; k++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                coefficients[k] = unchecked((int)state); quantizers[k] = (ushort)(1 + state % 65535);
            }
            JpegInverseTransform.Inverse(coefficients, quantizers, output, context);
            hash.AppendData(output);
        }
        // Full scalar matrix with original word wrap, bias and saturation.
        Assert.Equal("5eb412caf31da56383787d927082bf0561c9bb6c7d71f46cc6dac0334f5720a2", Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }
}
