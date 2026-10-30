using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegZeroUpperRowsTests
{
    [Fact]
    public void DenseLowerRowsAndWrappedUpperProductsKeepIndependentMatrixPixelsAndBorrowing()
    {
        var coefficients = new int[66]; var quantizers = new ushort[66];
        var output = Enumerable.Repeat((byte)165, 70).ToArray();
        coefficients[0] = 1234567; coefficients[^1] = -7654321;
        quantizers[0] = 123; quantizers[^1] = 456;
        uint state = 0x2a6f713d;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root, MaxWorkingBytes = 1 }, default);
        for (var block = 0; block < 4096; block++)
        {
            for (var k = 0; k < 64; k++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                if (k >= 16)
                {
                    // Nonzero encoded-domain coefficients can have zero word
                    // products. Eligibility must use dequantized samples.
                    coefficients[k + 1] = unchecked((int)(state & 0xffff0000));
                    quantizers[k + 1] = (ushort)(1 + state % 65535);
                }
                else if (block % 2 == 0)
                {
                    coefficients[k + 1] = k == 0 ? (int)(state % 2049) - 1024 : (int)(state % 129) - 64;
                    quantizers[k + 1] = 1;
                }
                else
                {
                    coefficients[k + 1] = unchecked((int)state);
                    quantizers[k + 1] = (ushort)(1 + state % 65535);
                }
            }
            var source = coefficients.ToArray(); var scales = quantizers.ToArray();
            JpegInverseTransform.Inverse(coefficients.AsSpan(1, 64), quantizers.AsSpan(1, 64), output.AsSpan(3, 64), context);
            hash.AppendData(output.AsSpan(3, 64));
            Assert.Equal(source, coefficients); Assert.Equal(scales, quantizers);
        }
        // Independently evaluated full scalar matrix, including every original
        // row, word wrap, saturation, column bias and pixel clipping.
        Assert.Equal("6c9f6f9f5cc8ec4bd9349b121c2b457cd2195b643d5d83727d0243fa81f03902", Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.All(output.Take(3).Concat(output.Skip(67)), value => Assert.Equal(165, value));
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }
}
