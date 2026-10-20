using System.Reflection;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class JpegHorizontalIntegerTests
{
    [Fact]
    public void RepeatedRowsKeepOriginalOrderedPixelsWorkInputsAndGuards()
    {
        if (!Avx512F.IsSupported || !Avx512F.VL.IsSupported) return;
        var basis = (double[])typeof(JpegTransform).GetField("Cosines", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        var accepted = 0; var rejected = 0;
        foreach (var q in new ushort[] { 1, 2, 255, 65535 })
        for (var seed = 0; seed < 64; seed++)
        {
            var coefficients = new int[64]; var quantization = Enumerable.Repeat((ushort)1, 64).ToArray(); ulong active = 0;
            for (var u = 0; u < 8; u++)
            {
                coefficients[u] = u == 0 ? seed % 2 == 0 ? 740 : -1020 : (seed * 73 + u * 37) % 129 - 64;
                quantization[u] = u % 2 == 0 ? q : (ushort)1; if (coefficients[u] != 0) active |= 1UL << u;
                for (var y = 0; y < 8; y++)
                for (var x = 0; x < 8; x++) Assert.Equal(JpegSparseBasis.Weights[u * 64 + x], JpegSparseBasis.Weights[u * 64 + y * 8 + x]);
            }
            var unchanged = coefficients.ToArray(); var unchangedQ = quantization.ToArray();
            var expected = new byte[64]; var output = Enumerable.Repeat((byte)173, 66).ToArray();
            for (var x = 0; x < 8; x++)
            {
                double sum = 0;
                for (var u = 0; u < 8; u++) sum += (double)coefficients[u] * quantization[u] * basis[x * 8 + u] * basis[0];
                var sample = (byte)Math.Clamp(Math.Round(sum * .25 + 128, MidpointRounding.AwayFromZero), 0, 255);
                for (var y = 0; y < 8; y++) expected[y * 8 + x] = sample;
            }
            if (JpegTransform.TryInverseHorizontalInteger(coefficients, quantization, output.AsSpan(1, 64), active))
            { accepted++; Assert.Equal(expected, output.AsSpan(1, 64).ToArray()); }
            else { rejected++; Assert.All(output, b => Assert.Equal(173, b)); }
            using var context = new OperationContext(ImageLimits.Default, default);
            Assert.True(JpegTransform.TryInverseSparse(coefficients, quantization, output.AsSpan(1, 64), context));
            Assert.Equal(expected, output.AsSpan(1, 64).ToArray()); Assert.Equal(4096, context.ConsumedWork);
            Assert.Equal(173, output[0]); Assert.Equal(173, output[^1]);
            Assert.Equal(unchanged, coefficients); Assert.Equal(unchangedQ, quantization);
        }
        Assert.True(accepted > 0); Assert.True(rejected > 0);
    }

    [Fact]
    public void UnknownRowHalfTiesVerticalMasksAndMassRejectWithoutPartialWrites()
    {
        if (!Avx512F.IsSupported || !Avx512F.VL.IsSupported) return;
        var coefficients = new int[64]; coefficients[0] = 4;
        var quantization = Enumerable.Repeat((ushort)1, 64).ToArray(); var output = Enumerable.Repeat((byte)173, 64).ToArray();
        Assert.False(JpegTransform.TryInverseHorizontalInteger(coefficients, quantization, output, 1));
        Assert.All(output, b => Assert.Equal(173, b));
        int[] row = [740, -54, -17, 20, 57, -35, 2, 39]; row.CopyTo(coefficients, 0);
        Assert.False(JpegTransform.TryInverseHorizontalInteger(coefficients, quantization, output, 255));
        Assert.All(output, b => Assert.Equal(173, b));
        Array.Clear(coefficients); coefficients[8] = 1;
        Assert.False(JpegTransform.TryInverseHorizontalInteger(coefficients, quantization, output, 1UL << 8));
        foreach (var value in new[] { 65537, -65537, int.MinValue, int.MaxValue })
        {
            coefficients[0] = value; quantization[0] = 65535;
            Assert.False(JpegTransform.TryInverseHorizontalInteger(coefficients, quantization, output, 1));
            Assert.All(output, b => Assert.Equal(173, b));
        }
        Array.Clear(coefficients); Array.Fill(quantization, (ushort)1);
        coefficients[0] = 65532; coefficients[1] = 4;
        Assert.True(JpegTransform.TryInverseHorizontalInteger(coefficients, quantization, output, 3)); Assert.All(output, b => Assert.Equal(255, b));
        coefficients[0] = -65532;
        Assert.True(JpegTransform.TryInverseHorizontalInteger(coefficients, quantization, output, 3)); Assert.All(output, b => Assert.Equal(0, b));
    }
}
