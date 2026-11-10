using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class PackedStreamingBicubicRowTests
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static double RoundedProduct(double sample, double weight) => sample * weight;

    [Fact]
    public void PairedRowsPreserveIndependentRoundedSumsAndBorrowedStorage()
    {
        if (!Avx512F.IsSupported || !Avx2.IsSupported) return;
        foreach (var (width, firstX, count) in new[]
        {
            (19, 0, 0), (19, 0, 1), (21, 3, 2), (17, -6, 33),
            (513, -192, 513), (19, 17, 8)
        })
        {
            const int guard = 32;
            var first = Enumerable.Repeat((byte)0xa5, guard * 2 + width * 4).ToArray();
            var second = first.ToArray();
            for (var x = 0; x < width; x++) for (var c = 0; c < 4; c++)
            {
                first[guard + x * 4 + c] = c == 3
                    ? (x % 3) switch { 0 => (byte)0, 1 => (byte)1, _ => (byte)255 }
                    : c == 0 ? (byte)255 : (byte)((x * 79 + c * 43) & 255);
                second[guard + x * 4 + c] = c == 3
                    ? (x % 3) switch { 0 => (byte)255, 1 => (byte)0, _ => (byte)1 }
                    : c == 1 ? (byte)255 : (byte)((x * 131 + c * 17) & 255);
            }
            var originalFirst = first.ToArray(); var originalSecond = second.ToArray();
            var low = Enumerable.Repeat(uint.MaxValue, count + 2).ToArray();
            var high = Enumerable.Repeat(ushort.MaxValue, count + 2).ToArray();
            for (var t = 0; t < count; t++)
            {
                var weight = (t % 4) switch { 0 => .1, 1 => -1d / 3, 2 => .75, _ => -1d / 7 };
                var bits = BitConverter.DoubleToUInt64Bits(weight);
                low[t + 1] = (uint)(bits >> 16); high[t + 1] = (ushort)(bits >> 48);
            }
            var sourceFirst = Math.Clamp(firstX, 0, width - 1);
            var sourceCount = count == 0 ? 0 : Math.Clamp(firstX + count - 1, 0, width - 1) - sourceFirst + 1;
            Vector512<double> Apply() => PackedStreamingBicubicResize.FilterRowPair(
                first.AsSpan(guard + sourceFirst * 4, sourceCount * 4),
                second.AsSpan(guard + sourceFirst * 4, sourceCount * 4),
                low.AsSpan(1, count), high.AsSpan(1, count), firstX, sourceFirst, width);
            Apply(); var allocated = GC.GetAllocatedBytesForCurrentThread(); var actual = Apply();
            Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - allocated);
            for (var c = 0; c < 8; c++)
            {
                var samples = c < 4 ? first : second; var expected = 0d;
                for (var t = 0; t < count; t++)
                {
                    var at = guard + Math.Clamp(firstX + t, 0, width - 1) * 4;
                    var sample = c % 4 == 3 ? samples[at + 3] : samples[at + c % 4] * samples[at + 3];
                    var weight = BitConverter.UInt64BitsToDouble(((ulong)high[t + 1] << 48) | ((ulong)low[t + 1] << 16));
                    expected += RoundedProduct(sample, weight);
                }
                Assert.Equal(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(actual.GetElement(c)));
            }
            Assert.Equal(originalFirst, first); Assert.Equal(originalSecond, second);
            Assert.Equal(uint.MaxValue, low[0]); Assert.Equal(uint.MaxValue, low[^1]);
            Assert.Equal(ushort.MaxValue, high[0]); Assert.Equal(ushort.MaxValue, high[^1]);
        }
    }
}
