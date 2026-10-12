using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Lokad.Image.Internal;

namespace Lokad.Image.Tests;

public sealed class ResizeRound16Tests
{
    [Fact]
    public void AllLanesPreserveHalvesTheirAdjacentDoublesAndSaturation()
    {
        if (!Avx.IsSupported || !Sse41.IsSupported) return;
        foreach (var integral in new[] { 0, 1, 127, 255, 256, 32767, 65534, 65535 })
        {
            var half = integral + 0.5;
            Check([Math.BitDecrement(half), half, Math.BitIncrement(half), integral]);
        }
        Check([-65536, -0.5, -0.0, 65536]);
        Check([65534.49999999999, 65534.5, double.Epsilon, double.MaxValue]);
    }

    [Fact]
    public void AuthoredFractionsMatchIndependentScalarRoundingAcrossFullPrecision()
    {
        if (!Avx.IsSupported || !Sse41.IsSupported) return;
        uint state = 0x16a17e5;
        for (var i = 0; i < 1024; i++)
        {
            var samples = new double[4];
            for (var lane = 0; lane < 4; lane++)
            {
                state = unchecked(state * 1664525 + 1013904223);
                samples[lane] = (state & 65535) + (state >> 16) / 65536d;
            }
            Check(samples);
        }
    }

    private static void Check(double[] samples)
    {
        var actual = PairedResize.Round16(Vector256.Create(samples[0], samples[1], samples[2], samples[3]));
        for (var lane = 0; lane < 4; lane++)
        {
            var expected = (ushort)Math.Clamp(Math.Round(samples[lane], MidpointRounding.AwayFromZero), 0, 65535);
            Assert.Equal(expected, (ushort)(actual >> (lane * 16)));
        }
    }
}
