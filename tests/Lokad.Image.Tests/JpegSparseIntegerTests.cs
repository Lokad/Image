using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegSparseIntegerTests
{
    private static double[] Basis => (double[])typeof(JpegTransform).GetField("Cosines", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;

    [Fact]
    public void OriginalModuleDataHasNoHeapTableAndMatchesExactRationalQuantization()
    {
        var before = GC.GetAllocatedBytesForCurrentThread();
        var weights = JpegSparseBasis.Weights;
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, allocated); Assert.Equal(4096, weights.Length);
        Assert.Null(typeof(JpegSparseBasis).TypeInitializer);
        // This getter must reference compiler RVA data through CreateSpan, with
        // no newarr, constructor, managed cache or writable backing storage.
        var getter = typeof(JpegSparseBasis).GetProperty("Weights", BindingFlags.Static | BindingFlags.NonPublic)!.GetGetMethod(true)!;
        var il = getter.GetMethodBody()!.GetILAsByteArray()!;
        Assert.Equal(11, il.Length); Assert.Equal(0xd0, il[0]); Assert.Equal(0x28, il[5]); Assert.Equal(0x2a, il[10]);
        var field = getter.Module.ResolveField(BitConverter.ToInt32(il, 1))!;
        Assert.True((field.Attributes & FieldAttributes.HasFieldRVA) != 0);
        var call = getter.Module.ResolveMethod(BitConverter.ToInt32(il, 6))!;
        Assert.Equal(typeof(RuntimeHelpers), call.DeclaringType); Assert.Equal("CreateSpan", call.Name);
        Assert.Equal("88b0772fdef0ba197442ce4c99319fd98da871b6bcf7e342a6fba85946eb4226",
            Convert.ToHexStringLower(SHA256.HashData(MemoryMarshal.AsBytes(weights))));
        var basis = Basis;
        for (var v = 0; v < 8; v++)
        for (var u = 0; u < 8; u++)
        for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
        {
            var a = Rational(basis[x * 8 + u]); var b = Rational(basis[y * 8 + v]);
            var n = a.N * b.N * 16384; var d = a.D * b.D;
            var quotient = BigInteger.DivRem(BigInteger.Abs(n), d, out var remainder);
            if (2 * remainder > d || 2 * remainder == d && !quotient.IsEven) quotient++;
            Assert.Equal((short)(n.Sign * quotient), weights[(v * 8 + u) * 64 + y * 8 + x]);
        }
        Assert.True(65536L * 16384 + (128L << 16) + 32768 < int.MaxValue);
        var dc = Rational(basis[0]);
        Assert.True(BigInteger.Abs(8192 * dc.D * dc.D - 16384 * dc.N * dc.N) * 65536 * 2 < dc.D * dc.D);
    }

    [Fact]
    public void WholeBlockRejectionPreservesEveryByteAndAcceptedPixelsKeepOriginalOrder()
    {
        if (!Avx512F.IsSupported || !Avx512F.VL.IsSupported) return;
        int[][] patterns = [[0, 1, 8], [0, 7, 16, 33], [2, 9, 18, 27, 36, 45, 54, 63]];
        var accepted = 0; var refused = 0; var basis = Basis;
        foreach (var pattern in patterns)
        foreach (var q in new ushort[] { 1, 2, 255, 65535 })
        for (var seed = 0; seed < 16; seed++)
        {
            var coefficients = new int[64]; var quantization = Enumerable.Repeat((ushort)1, 64).ToArray(); ulong active = 0;
            for (var i = 0; i < pattern.Length; i++)
            {
                var at = pattern[i]; coefficients[at] = (seed * 73 + i * 37) % 129 - 64;
                quantization[at] = i % 2 == 0 ? q : (ushort)1; if (coefficients[at] != 0) active |= 1UL << at;
            }
            var original = coefficients.ToArray(); var originalQ = quantization.ToArray();
            var output = Enumerable.Repeat((byte)173, 66).ToArray(); var expected = new byte[64];
            for (var y = 0; y < 8; y++)
            for (var x = 0; x < 8; x++)
            {
                double sum = 0;
                for (var i = 0; i < 64; i++)
                    if (coefficients[i] != 0) sum += (double)coefficients[i] * quantization[i] * basis[x * 8 + i % 8] * basis[y * 8 + i / 8];
                expected[y * 8 + x] = (byte)Math.Clamp(Math.Round(sum * .25 + 128, MidpointRounding.AwayFromZero), 0, 255);
            }
            if (JpegTransform.TryInverseSparseInteger(coefficients, quantization, output.AsSpan(1, 64), active))
            { accepted++; Assert.Equal(expected, output.AsSpan(1, 64).ToArray()); }
            else { refused++; Assert.All(output, b => Assert.Equal(173, b)); }
            using var context = new OperationContext(ImageLimits.Default, default);
            Assert.True(JpegTransform.TryInverseSparse(coefficients, quantization, output.AsSpan(1, 64), context));
            Assert.Equal(expected, output.AsSpan(1, 64).ToArray()); Assert.Equal(4096, context.ConsumedWork);
            Assert.Equal(173, output[0]); Assert.Equal(173, output[^1]); Assert.Equal(original, coefficients); Assert.Equal(originalQ, quantization);
        }
        Assert.True(accepted > 0); Assert.True(refused > 0);
    }

    [Fact]
    public void HalfTiesExtremeMassAndDenseMasksCannotPublishPartialResults()
    {
        if (!Avx512F.IsSupported || !Avx512F.VL.IsSupported) return;
        var coefficients = new int[64]; var quantization = Enumerable.Repeat((ushort)1, 64).ToArray();
        var output = Enumerable.Repeat((byte)173, 64).ToArray(); coefficients[0] = 4;
        Assert.False(JpegTransform.TryInverseSparseInteger(coefficients, quantization, output, 1));
        Assert.All(output, b => Assert.Equal(173, b));
        // This eligible block certifies62 pixels, but two uncertain lanes must
        // keep the entire destination untouched, including otherwise good rows.
        coefficients[0] = 0; coefficients[1] = -200; quantization[1] = 17;
        coefficients[8] = 191; quantization[8] = 3; coefficients[63] = -31; quantization[63] = 11;
        Assert.False(JpegTransform.TryInverseSparseInteger(coefficients, quantization, output, (1UL << 1) | (1UL << 8) | (1UL << 63)));
        Assert.All(output, b => Assert.Equal(173, b));
        Array.Clear(coefficients); Array.Fill(quantization, (ushort)1);
        foreach (var value in new[] { 65537, -65537, int.MinValue, int.MaxValue })
        {
            coefficients[0] = 0; coefficients[8] = value; quantization[8] = 65535;
            Assert.False(JpegTransform.TryInverseSparseInteger(coefficients, quantization, output, 1UL << 8));
            Assert.All(output, b => Assert.Equal(173, b));
        }
        Array.Fill(coefficients, 1); Array.Fill(quantization, (ushort)1);
        Assert.False(JpegTransform.TryInverseSparseInteger(coefficients, quantization, output, 511));
        Assert.All(output, b => Assert.Equal(173, b));
        Array.Clear(coefficients); coefficients[0] = 65528; coefficients[1] = coefficients[8] = 4;
        Assert.True(JpegTransform.TryInverseSparseInteger(coefficients, quantization, output, 259));
        Assert.All(output, b => Assert.Equal(255, b));
        coefficients[0] = -65528;
        Assert.True(JpegTransform.TryInverseSparseInteger(coefficients, quantization, output, 259));
        Assert.All(output, b => Assert.Equal(0, b));
    }

    private static (BigInteger N, BigInteger D) Rational(double value)
    {
        var bits = BitConverter.DoubleToUInt64Bits(value); var exponent = (int)((bits >> 52) & 2047) - 1023 - 52;
        BigInteger n = (bits & ((1UL << 52) - 1)) | (1UL << 52); if ((bits >> 63) != 0) n = -n;
        return exponent >= 0 ? (n << exponent, BigInteger.One) : (n, BigInteger.One << -exponent);
    }
}
