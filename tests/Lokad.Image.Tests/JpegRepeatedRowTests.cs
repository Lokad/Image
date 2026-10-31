using System.Security.Cryptography;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class JpegRepeatedRowTests
{
    [Theory]
    [InlineData(0, "e64166d6acb07e6d78e01950532db4434e97c1434787e696eb428db0245231d5")]
    [InlineData(1, "8b57bb2b3e3a7472acf92d3fefd790bfff4c4f1c762bff4471889db09b68c456")]
    public void CompletedRowsAndNonzeroSecondRowsKeepIndependentMatrixBytes(int phase, string expected)
    {
        var coefficients = new int[66]; coefficients[0] = 1234567; coefficients[^1] = -7654321;
        var quantizers = new ushort[66]; quantizers[0] = 173; quantizers[^1] = 251;
        var output = Enumerable.Repeat((byte)165, 70).ToArray();
        var c = coefficients.AsSpan(1, 64); var q = quantizers.AsSpan(1, 64);
        var beforeC = new int[64]; var beforeQ = new ushort[64];
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        using var root = new TestMemoryBudget(0);
        using var context = new OperationContext(ImageLimits.Default with { MemoryBudget = root, MaxWorkingBytes = 1 }, default);
        uint state = 0x5bd43a91;
        for (var block = 0; block < 512; block++)
        {
            for (var k = 0; k < 64; k++)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                c[k] = unchecked((int)state); q[k] = (ushort)(1 + state % 65535);
                // Nonzero source coefficients deliberately narrow to zero:
                // eligibility must follow the actual dequantized words.
                if (k >= 8 * (phase + 1)) { c[k] = (state & 1) != 0 ? 65536 : 32768; q[k] = (ushort)((state & 1) != 0 ? 3 : 2); }
                else if (block % 4 == 0) { c[k] = (int)(state % 129) - 64; q[k] = (ushort)(1 + state % 7); }
            }
            if (block % 4 == 0) { c[0] = ((block * 13) % 256 - 128) * 8; q[0] = 1; }
            c[1] = block % 2 == 0 ? 17 : -17; q[1] = 3;
            if (phase != 0) { c[8] = 1; q[8] = 1; }
            c.CopyTo(beforeC); q.CopyTo(beforeQ);
            JpegInverseTransform.Inverse(c, q, output.AsSpan(3, 64), context);
            hash.AppendData(output.AsSpan(3, 64));
            Assert.True(c.SequenceEqual(beforeC)); Assert.True(q.SequenceEqual(beforeQ));
            if (phase == 0)
                for (var y = 1; y < 8; y++) Assert.True(output.AsSpan(3, 8).SequenceEqual(output.AsSpan(3 + y * 8, 8)));
        }
        // Independent full scalar Q14 matrix, including modulo-32 sums,
        // signed-word bias/row saturation, wide quantizers and clipped pixels.
        Assert.Equal(expected, Convert.ToHexStringLower(hash.GetHashAndReset()));
        Assert.All(output.Take(3).Concat(output.Skip(67)), b => Assert.Equal(165, b));
        Assert.Equal(1234567, coefficients[0]); Assert.Equal(-7654321, coefficients[^1]);
        Assert.Equal(173, quantizers[0]); Assert.Equal(251, quantizers[^1]);
        for (var warm = 0; warm < 32; warm++) JpegInverseTransform.Inverse(c, q, output.AsSpan(3, 64), context);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var repeat = 0; repeat < 128; repeat++) JpegInverseTransform.Inverse(c, q, output.AsSpan(3, 64), context);
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.Equal(0, root.Bytes); Assert.Equal(0, root.Attempts); Assert.False(root.IsDisposed);
    }
}
