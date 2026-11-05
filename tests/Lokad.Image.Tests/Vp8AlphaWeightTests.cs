using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class Vp8AlphaWeightTests
{
    [Fact]
    public void PreparedWeightsMatchWideIntegerOracleForEverySumAndKeepDetachedZeroAllocationValues()
    {
        Assert.Equal(8, Marshal.SizeOf<Vp8PreparedInput.AlphaWeights>());
        var alpha = new byte[4];
        for (var sum = 0; sum <= 1020; sum++)
        {
            var remaining = sum;
            for (var i = 0; i < 4; i++) { alpha[i] = (byte)Math.Min(255, remaining); remaining -= alpha[i]; }
            for (var rotation = 0; rotation < 4; rotation++)
            {
                var a = alpha[rotation]; var b = alpha[(rotation + 1) % 4];
                var c = alpha[(rotation + 2) % 4]; var d = alpha[(rotation + 3) % 4];
                var weights = new Vp8PreparedInput.AlphaWeights(a, b, c, d);
                Check(0, 4095, 2117, 31); Check(4095, 4095, 4095, 4095); Check(1, 0, 1, 0);
                void Check(int x, int y, int z, int w)
                {
                    var wide = (BigInteger)x * a + (BigInteger)y * b + (BigInteger)z * c + (BigInteger)w * d;
                    var expected = sum is 0 or 1020 ? x + y + z + w : (int)(wide * ((1 << 19) / sum) / (1 << 17));
                    Assert.Equal(expected, weights.Combine(x, y, z, w));
                }
            }
        }
        var saved = new Vp8PreparedInput.AlphaWeights(1, 127, 254, 3);
        var expectedSaved = saved.Combine(0, 4095, 2117, 31);
        Array.Fill(alpha, (byte)255); Assert.Equal(expectedSaved, saved.Combine(0, 4095, 2117, 31));
        for (var i = 0; i < 100; i++) _ = ExerciseWeights();
        Assert.Equal(0L, 0L); // Initialize the assertion's long comparer outside measurement.
        var allocated = GC.GetAllocatedBytesForCurrentThread(); var checksum = ExerciseWeights();
        var used = GC.GetAllocatedBytesForCurrentThread() - allocated;
        Assert.Equal(0, used); GC.KeepAlive(checksum);
    }

    // Keep the measured operation outside the oracle's captured local state.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ExerciseWeights()
    {
        var checksum = 0;
        for (var i = 0; i < 10000; i++) checksum ^= new Vp8PreparedInput.AlphaWeights((byte)(i & 255), 127, 254, 3).Combine(0, 4095, 2117, 31);
        return checksum;
    }
}
