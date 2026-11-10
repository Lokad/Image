using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class EnlargementQuartetExecutionTests
{
    [Fact]
    public void SharedQuartetsPreserveEveryOrderedScalarLaneAndBorrowedRow()
    {
        if (!Avx512F.IsSupported) return;
        const int width = 13;
        var cache = new Vector256<double>[5 * width];
        var weights = new double[6]; var rows = new int[6];
        var random = new Random(772);
        double[] values = [-0d, 0, Math.BitDecrement(.5), Math.BitIncrement(.5),
            -65025, 65025, -4294836225d, 4294836225d, 1d / (1L << 40)];
        for (var trial = 0; trial < 256; trial++)
        {
            for (var i = 0; i < cache.Length; i++)
                cache[i] = Vector256.Create(values[random.Next(values.Length)], values[random.Next(values.Length)],
                    values[random.Next(values.Length)], values[random.Next(values.Length)]);
            var before = cache.ToArray();
            for (var tap = 0; tap < 5; tap++)
            {
                rows[tap] = ((trial + tap * 3) % 5) * width;
                weights[tap] = ((trial + tap) % 5) switch
                {
                    0 => -.0625, 1 => 0, 2 => Math.BitDecrement(.125), 3 => 1, _ => -.5
                };
            }
            weights[5] = double.NaN; rows[5] = int.MaxValue;
            var x = trial % (width - 3);
            for (var taps = 1; taps <= 5; taps++)
            {
                EnlargingBicubicResize.FilterVerticalQuartet(cache, weights.AsSpan(0, taps), rows, x,
                    out var first, out var second);
                for (var pixel = 0; pixel < 4; pixel++)
                for (var channel = 0; channel < 4; channel++)
                {
                    // Independent scalar reference retains the required tap order.
                    double expected = 0;
                    for (var tap = 0; tap < taps; tap++)
                        expected += cache[rows[tap] + x + pixel].GetElement(channel) * weights[tap];
                    var observed = (pixel < 2 ? first : second).GetElement((pixel % 2) * 4 + channel);
                    Assert.Equal(BitConverter.DoubleToUInt64Bits(expected), BitConverter.DoubleToUInt64Bits(observed));
                }
            }
            for (var i = 0; i < cache.Length; i++)
                Assert.Equal(before[i].AsUInt64(), cache[i].AsUInt64());
            Assert.True(double.IsNaN(weights[5])); Assert.Equal(int.MaxValue, rows[5]);
        }
    }
}
