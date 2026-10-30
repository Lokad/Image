using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegConstantBlockTests
{
    [Fact]
    public void EverySignedDcWordKeepsFrozenMatrixPixelsGuardsAndBorrowing()
    {
        var coefficients = new int[66];
        var quantizers = Enumerable.Repeat((ushort)1, 66).ToArray();
        var guarded = Enumerable.Repeat((byte)165, 80).ToArray();
        coefficients[0] = 1234567; coefficients[^1] = -7654321;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root, MaxWorkingBytes = 1 }, default);
        for (var dc = (int)short.MinValue; dc <= short.MaxValue; dc++)
        {
            // Full-width product plus1024 reaches every post-dequant DCword.
            coefficients[1] = dc - 1024;
            JpegInverseTransform.Inverse(coefficients.AsSpan(1, 64), quantizers.AsSpan(1, 64), guarded.AsSpan(8, 64), context);
            hash.AppendData(guarded.AsSpan(8, 64));
            Assert.Equal(dc - 1024, coefficients[1]);
        }
        // Independently evaluated original two-pass matrix, signed row wrap,
        // signed column bias and output clipping over all65536 DCword states.
        Assert.Equal("f2a4a47839628cfdce2e43e0bc0f0ef7077d6838a3fc1998bd127e89e259728c", Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.All(guarded.Take(8).Concat(guarded.Skip(72)), value => Assert.Equal(165, value));
        Assert.Equal(1234567, coefficients[0]); Assert.Equal(-7654321, coefficients[^1]);
        Assert.All(coefficients.Skip(2).Take(63), value => Assert.Equal(0, value));
        Assert.All(quantizers, value => Assert.Equal((ushort)1, value));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }

    [Fact]
    public void NonzeroAcCoefficientsWithZeroLowProductsKeepConstantPixels()
    {
        var coefficients = new int[64];
        var quantizers = new ushort[64];
        var output = new byte[64];
        coefficients[0] = 17; quantizers[0] = 1;
        for (var k = 1; k < 64; k++)
        {
            coefficients[k] = (k % 3) switch { 0 => int.MinValue, 1 => 65536, _ => -32768 };
            quantizers[k] = (k % 3) switch { 0 => ushort.MaxValue, 1 => 1, _ => 2 };
        }
        var source = coefficients.ToArray(); var scales = quantizers.ToArray();
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root, MaxWorkingBytes = 1 }, default);
        JpegInverseTransform.Inverse(coefficients, quantizers, output, context);
        Assert.All(output, value => Assert.Equal(130, value));
        Assert.Equal(source, coefficients); Assert.Equal(scales, quantizers);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }
}
