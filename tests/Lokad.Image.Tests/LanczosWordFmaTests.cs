using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;
using Lokad.Image.Tooling;

namespace Lokad.Image.Tests;

public sealed class LanczosWordFmaTests
{
    [Fact]
    public void WordProducerPreservesUnsignedProductsGuardsAndExactAlphaWithoutAllocation()
    {
        if (!(Avx512F.IsSupported && Avx512DQ.IsSupported && Avx512DQ.VL.IsSupported
            && Avx2.IsSupported && Fma.IsSupported && Sse41.IsSupported)) return;
        foreach (var (width, target) in new[] { (1, 9), (3, 13), (4, 17), (5, 13), (513, 1027), (17, 9), (1031, 2) })
        {
            using var source = ImageBuffer.Create(width, 1, 16);
            for (var x = 0; x < width; x++)
            {
                source.SetSample(x, 0, 0, 65535);
                source.SetSample(x, 0, 1, x % 2 == 0 ? 65535 : 32767);
                source.SetSample(x, 0, 2, x % 3 == 0 ? 0 : 65534);
                source.SetSample(x, 0, 3, (x % 4) switch { 0 => 0, 1 => 1, 2 => 16, _ => 65535 });
            }
            var original = source.Pixels.ToArray();
            using var root = new TestMemoryBudget(1 << 26);
            using var context = new OperationContext(new() { MemoryBudget = root }, default);
            using var plan = LanczosPlan.Create(width, target, context); var charged = root.Bytes;
            const uint productGuard = 0xdec0ded;
            const ulong outputGuard = 0xfff8abcd12345678;
            var products = Enumerable.Repeat(productGuard, width * 4 + 8).ToArray();
            var output = Enumerable.Repeat(BitConverter.UInt64BitsToDouble(outputGuard), target * 4 + 8).ToArray();
            void Apply() => LanczosFmaResize.Horizontal(source, 0, plan,
                output.AsSpan(4, target * 4), products.AsSpan(4, width * 4), context);
            Apply();
            var before = GC.GetAllocatedBytesForCurrentThread(); Apply();
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
            Assert.Equal(charged, root.Bytes); Assert.Equal(original, source.Pixels.ToArray());
            if (width > 3) Assert.Contains(products.AsSpan(4, width * 4).ToArray(), value => value > int.MaxValue);
            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(productGuard, products[i]); Assert.Equal(productGuard, products[width * 4 + 4 + i]);
                Assert.Equal(outputGuard, BitConverter.DoubleToUInt64Bits(output[i]));
                Assert.Equal(outputGuard, BitConverter.DoubleToUInt64Bits(output[target * 4 + 4 + i]));
            }
            for (var x = 0; x < target; x++)
            for (var channel = 0; channel < 4; channel++)
            {
                var window = plan.Windows[x]; BigInteger exact = 0;
                for (var tap = 0; tap < window.Count; tap++)
                    exact += (BigInteger)source.Sample(window.First + tap, 0, 3)
                        * (channel == 3 ? 1 : source.Sample(window.First + tap, 0, channel))
                        * plan.Weights[window.Offset + tap];
                var error = BigInteger.Abs((BigInteger)output[4 + x * 4 + channel] - exact);
                Assert.True(error <= (channel == 3 ? BigInteger.Zero : (window.Count + 3L) * 512));
                // Independent scalar four-chain FMA oracle. The pair shares
                // source conversions, never coefficient streams or reductions.
                double c0 = 0, c1 = 0, c2 = 0, c3 = 0; var t = 0;
                double Value(int tap) => (double)source.Sample(window.First + tap, 0, 3)
                    * (channel == 3 ? 1 : source.Sample(window.First + tap, 0, channel));
                for (; t + 3 < window.Count; t += 4)
                {
                    c0 = Math.FusedMultiplyAdd(Value(t), plan.Weights[window.Offset + t], c0);
                    c1 = Math.FusedMultiplyAdd(Value(t + 1), plan.Weights[window.Offset + t + 1], c1);
                    c2 = Math.FusedMultiplyAdd(Value(t + 2), plan.Weights[window.Offset + t + 2], c2);
                    c3 = Math.FusedMultiplyAdd(Value(t + 3), plan.Weights[window.Offset + t + 3], c3);
                }
                var expected = (c0 + c2) + (c1 + c3);
                for (; t < window.Count; t++)
                    expected = Math.FusedMultiplyAdd(Value(t), plan.Weights[window.Offset + t], expected);
                Assert.Equal(BitConverter.DoubleToUInt64Bits(expected), BitConverter.DoubleToUInt64Bits(output[4 + x * 4 + channel]));
            }
            plan.Dispose(); Assert.Equal(0, root.Bytes); Assert.False(root.IsDisposed);
        }
    }

    [Fact]
    public void WordEnvelopeKeepsWorstCountPrefixesAndCertificateNumeratorBounded()
    {
        BigInteger maximum = 65535, variation = BigInteger.One << 31;
        foreach (var horizontal in new[] { 1, int.MaxValue })
        foreach (var vertical in new[] { 1, int.MaxValue })
        {
            var horizontalError = ((BigInteger)horizontal + 3) * 512;
            var e = LanczosFmaResize.Errors(vertical, horizontal);
            Assert.Equal(((BigInteger)horizontal + 3 + 2L * vertical) << 40, (BigInteger)e.Rgb);
            Assert.Equal((BigInteger)vertical << 25, (BigInteger)e.Alpha);
            Assert.True(maximum * maximum * variation + horizontalError < BigInteger.One << 63);
            Assert.True(maximum * maximum * variation * variation + (BigInteger)e.Rgb < BigInteger.One << 94);
            Assert.True(maximum * variation * variation + (BigInteger)e.Alpha < BigInteger.One << 78);
            Assert.True(maximum * variation < BigInteger.One << 47);
            var numerator = ((BigInteger)horizontal + 3 + 2L * vertical) * 32768 + maximum * vertical;
            Assert.True(numerator < BigInteger.One << 49);
            Assert.Equal(numerator << 25, (BigInteger)(e.Rgb + 65535d * e.Alpha));
        }
    }
}
